using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Infrastructure.Commerce;

internal static class CommerceMapping
{
    public static ShoppingCartDto ToCartDto(ShoppingCart cart)
    {
        var pricing = CommercePricing.Calculate(cart);

        return new ShoppingCartDto(
            cart.Id,
            cart.Items
                .OrderBy(item => item.CreatedAt)
                .Select(ToCartItemDto)
                .ToArray(),
            cart.Coupon?.Code,
            pricing.Subtotal,
            pricing.DiscountAmount,
            pricing.ShippingAmount,
            pricing.TaxAmount,
            pricing.TotalAmount,
            cart.Items.Count > 0 && cart.Items.All(item => item.Quantity <= item.Product.Stock));
    }

    public static WishlistProductDto ToWishlistDto(WishlistItem item)
    {
        return new WishlistProductDto(
            item.ProductId,
            item.Product.Name,
            item.Product.Slug,
            item.Product.Sku,
            item.Product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .Select(image => image.Url)
                .FirstOrDefault(),
            item.Product.Price,
            item.Product.DiscountPrice,
            item.Product.Stock,
            item.CreatedAt);
    }

    public static OrderSummaryDto ToOrderSummaryDto(Order order)
    {
        return new OrderSummaryDto(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.TotalAmount,
            order.Items.Sum(item => item.Quantity),
            order.CreatedAt,
            order.Payments
                .OrderByDescending(payment => payment.CreatedAt)
                .Select(payment => payment.Status)
                .FirstOrDefault());
    }

    public static OrderDetailsDto ToOrderDetailsDto(Order order)
    {
        return new OrderDetailsDto(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.CustomerFullName,
            order.CustomerEmail,
            order.CustomerPhoneNumber,
            order.ShippingAddress,
            order.PostalCode,
            order.Notes,
            order.Coupon?.Code,
            order.Subtotal,
            order.DiscountAmount,
            order.ShippingAmount,
            order.TaxAmount,
            order.TotalAmount,
            order.CreatedAt,
            order.Items
                .OrderBy(item => item.CreatedAt)
                .Select(item => new OrderItemDto(
                    item.ProductId,
                    item.ProductName,
                    item.Sku,
                    item.UnitPrice,
                    item.Quantity,
                    item.TotalPrice))
                .ToArray(),
            order.Payments
                .OrderBy(payment => payment.CreatedAt)
                .Select(payment => new PaymentInfoDto(
                    payment.Id,
                    payment.GatewayName,
                    payment.GatewayTransactionId,
                    payment.Status,
                    payment.Amount,
                    payment.VerifiedAt,
                    payment.FailureReason))
                .ToArray());
    }

    private static ShoppingCartItemDto ToCartItemDto(ShoppingCartItem item)
    {
        return new ShoppingCartItemDto(
            item.ProductId,
            item.Product.Name,
            item.Product.Slug,
            item.Product.Sku,
            item.Product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .Select(image => image.Url)
                .FirstOrDefault(),
            CommercePricing.GetUnitPrice(item.Product),
            item.Quantity,
            item.Product.Stock,
            CommercePricing.GetUnitPrice(item.Product) * item.Quantity);
    }
}
