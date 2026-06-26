using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Domain.Orders;
using NaderEcommerce.Infrastructure.Commerce;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Tests.Commerce;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task ShoppingCartService_AddItemAndApplyCoupon_CalculatesTotals()
    {
        await using var dbContext = CreateDbContext();
        var user = SeedUser(dbContext);
        var product = SeedProduct(dbContext, price: 500000m, discountPrice: 450000m, stock: 10);
        SeedCoupon(dbContext, "WELCOME150", 150000m, 800000m);
        await dbContext.SaveChangesAsync();

        var service = new ShoppingCartService(dbContext);

        await service.AddItemAsync(user.Id, new AddCartItemRequest(product.Id, 2));
        var cart = await service.ApplyCouponAsync(user.Id, new ApplyCouponRequest("WELCOME150"));

        Assert.Equal(900000m, cart.Subtotal);
        Assert.Equal(150000m, cart.DiscountAmount);
        Assert.Equal(120000m, cart.ShippingAmount);
        Assert.Equal(67500m, cart.TaxAmount);
        Assert.Equal(937500m, cart.TotalAmount);
        Assert.True(cart.CanCheckout);
    }

    [Fact]
    public async Task CheckoutAndVerifyPayment_CreatesPaidOrderAndClearsCart()
    {
        await using var dbContext = CreateDbContext();
        var user = SeedUser(dbContext);
        var product = SeedProduct(dbContext, price: 890000m, discountPrice: 760000m, stock: 8);
        var coupon = SeedCoupon(dbContext, "SAVE100", 100000m, 500000m);
        await dbContext.SaveChangesAsync();

        var cartService = new ShoppingCartService(dbContext);
        await cartService.AddItemAsync(user.Id, new AddCartItemRequest(product.Id, 2));
        await cartService.ApplyCouponAsync(user.Id, new ApplyCouponRequest(coupon.Code));

        var orderService = new OrderService(dbContext, cartService, new FakePaymentGatewayService());
        var checkout = await orderService.CheckoutAsync(
            user.Id,
            new CheckoutRequest(
                "Customer User",
                "customer@example.com",
                "09120000000",
                "Tehran, Sample Street, No 10",
                "1234567890",
                null));

        Assert.Equal(OrderStatus.Pending, checkout.Order.Status);
        Assert.NotEmpty(checkout.Payment.VerificationToken);
        Assert.Equal(6, product.Stock);

        var verifiedOrder = await orderService.VerifyPaymentAsync(
            user.Id,
            checkout.Payment.PaymentId,
            new VerifyPaymentRequest(checkout.Payment.VerificationToken));

        Assert.Equal(OrderStatus.Paid, verifiedOrder.Status);
        Assert.Equal(PaymentStatus.Succeeded, verifiedOrder.Payments.Single().Status);
        Assert.Equal(1, coupon.UsedCount);

        var cart = await cartService.GetCartAsync(user.Id);
        Assert.Empty(cart.Items);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User SeedUser(ApplicationDbContext dbContext)
    {
        var user = new User
        {
            Email = "customer@example.com",
            NormalizedEmail = "CUSTOMER@EXAMPLE.COM",
            FullName = "Customer User",
            PasswordHash = "hash"
        };

        dbContext.Users.Add(user);
        return user;
    }

    private static Product SeedProduct(ApplicationDbContext dbContext, decimal price, decimal? discountPrice, int stock)
    {
        var product = new Product
        {
            Name = "Sample Product",
            Slug = $"sample-product-{Guid.NewGuid():N}",
            Sku = $"SKU-{Guid.NewGuid():N}"[..12],
            Price = price,
            DiscountPrice = discountPrice,
            Stock = stock
        };

        dbContext.Products.Add(product);
        return product;
    }

    private static Coupon SeedCoupon(ApplicationDbContext dbContext, string code, decimal discount, decimal minimumAmount)
    {
        var coupon = new Coupon
        {
            Code = code,
            DiscountAmount = discount,
            MinimumOrderAmount = minimumAmount,
            StartsAt = DateTimeOffset.UtcNow.AddDays(-1),
            EndsAt = DateTimeOffset.UtcNow.AddDays(10),
            IsActive = true
        };

        dbContext.Coupons.Add(coupon);
        return coupon;
    }

    private sealed class FakePaymentGatewayService : IPaymentGatewayService
    {
        public PaymentGatewaySession CreateSession(Guid paymentId)
        {
            return new PaymentGatewaySession(
                $"TXN-{paymentId:N}"[..16],
                $"TOKEN-{paymentId:N}",
                $"/mock-payments/{paymentId}",
                $"/api/orders/payments/{paymentId}/verify");
        }
    }
}
