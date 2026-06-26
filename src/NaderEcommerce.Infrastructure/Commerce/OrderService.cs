using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Domain.Orders;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Commerce;

public sealed class OrderService(
    ApplicationDbContext dbContext,
    ShoppingCartService shoppingCartService,
    IPaymentGatewayService paymentGatewayService) : IOrderService
{
    public async Task<ShoppingCartDto> GetCheckoutSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await shoppingCartService.GetOrCreateCartAsync(userId, cancellationToken);
        return CommerceMapping.ToCartDto(cart);
    }

    public async Task<CheckoutSessionDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await shoppingCartService.GetOrCreateCartAsync(userId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("سبد خرید خالی است.");
        }

        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
            {
                throw new InvalidOperationException($"محصول «{item.Product.Name}» دیگر در دسترس نیست.");
            }

            if (item.Quantity > item.Product.Stock)
            {
                throw new InvalidOperationException($"موجودی محصول «{item.Product.Name}» کافی نیست.");
            }
        }

        var pricing = CommercePricing.Calculate(cart);
        var payment = new Payment
        {
            GatewayName = "درگاه آزمایشی",
            Amount = pricing.TotalAmount
        };
        var gatewaySession = paymentGatewayService.CreateSession(payment.Id);
        payment.GatewayTransactionId = gatewaySession.TransactionId;
        payment.VerificationToken = gatewaySession.VerificationToken;

        var order = new Order
        {
            UserId = userId,
            OrderNumber = GenerateOrderNumber(),
            CustomerFullName = request.CustomerFullName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhoneNumber = string.IsNullOrWhiteSpace(request.CustomerPhoneNumber) ? null : request.CustomerPhoneNumber.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = OrderStatus.Pending,
            CouponId = cart.CouponId,
            Subtotal = pricing.Subtotal,
            DiscountAmount = pricing.DiscountAmount,
            ShippingAmount = pricing.ShippingAmount,
            TaxAmount = pricing.TaxAmount,
            TotalAmount = pricing.TotalAmount
        };

        foreach (var item in cart.Items)
        {
            var unitPrice = CommercePricing.GetUnitPrice(item.Product);
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Product = item.Product,
                ProductName = item.Product.Name,
                Sku = item.Product.Sku,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                TotalPrice = unitPrice * item.Quantity
            });

            item.Product.Stock -= item.Quantity;
        }

        order.Payments.Add(payment);
        dbContext.Orders.Add(order);

        dbContext.ShoppingCartItems.RemoveRange(cart.Items.ToArray());
        cart.Items.Clear();
        cart.CouponId = null;
        cart.Coupon = null;

        await dbContext.SaveChangesAsync(cancellationToken);

        var loadedOrder = await GetOrderEntityAsync(order.Id, cancellationToken)
            ?? throw new InvalidOperationException("سفارش ایجاد نشد.");

        return new CheckoutSessionDto(
            CommerceMapping.ToOrderDetailsDto(loadedOrder),
            new PaymentSessionDto(
                payment.Id,
                payment.GatewayName,
                gatewaySession.TransactionId,
                gatewaySession.VerificationToken,
                gatewaySession.PaymentUrl,
                gatewaySession.VerificationUrl));
    }

    public async Task<OrderDetailsDto> VerifyPaymentAsync(Guid userId, Guid paymentId, VerifyPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await dbContext.Payments
            .Include(entity => entity.Order)
                .ThenInclude(order => order.Items)
            .Include(entity => entity.Order)
                .ThenInclude(order => order.Payments)
            .Include(entity => entity.Order)
                .ThenInclude(order => order.Coupon)
            .SingleOrDefaultAsync(entity => entity.Id == paymentId && entity.Order.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("پرداخت پیدا نشد.");

        if (!string.Equals(payment.VerificationToken, request.VerificationToken, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("توکن تایید پرداخت نامعتبر است.");
        }

        if (payment.Status == PaymentStatus.Pending)
        {
            if (request.Succeed)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.VerifiedAt = DateTimeOffset.UtcNow;
                payment.FailureReason = null;
                payment.Order.Status = OrderStatus.Paid;

                if (payment.Order.Coupon is not null)
                {
                    payment.Order.Coupon.UsedCount += 1;
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "تایید پرداخت توسط درگاه آزمایشی رد شد.";
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var order = await GetOrderEntityAsync(payment.OrderId, cancellationToken)
            ?? throw new InvalidOperationException("سفارش پیدا نشد.");

        return CommerceMapping.ToOrderDetailsDto(order);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders
            .AsSplitQuery()
            .AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.Payments)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(CommerceMapping.ToOrderSummaryDto).ToArray();
    }

    public async Task<OrderDetailsDto?> GetOrderAsync(Guid userId, string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .AsSplitQuery()
            .AsNoTracking()
            .Include(entity => entity.Coupon)
            .Include(entity => entity.Items)
            .Include(entity => entity.Payments)
            .SingleOrDefaultAsync(entity => entity.UserId == userId && entity.OrderNumber == orderNumber, cancellationToken);

        return order is null ? null : CommerceMapping.ToOrderDetailsDto(order);
    }

    private async Task<Order?> GetOrderEntityAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsSplitQuery()
            .Include(entity => entity.Coupon)
            .Include(entity => entity.Items)
            .Include(entity => entity.Payments)
            .SingleOrDefaultAsync(entity => entity.Id == orderId, cancellationToken);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}
