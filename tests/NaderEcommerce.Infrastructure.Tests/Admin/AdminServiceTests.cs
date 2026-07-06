using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Admin;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Domain.Orders;
using NaderEcommerce.Infrastructure.Admin;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Tests.Admin;

public sealed class AdminServiceTests
{
    [Fact]
    public async Task ProductManagement_CreatesUpdatesAndSoftDeletesProduct()
    {
        await using var dbContext = CreateDbContext();
        var category = new Category
        {
            Name = "Skincare",
            Slug = "skincare",
            IsActive = true
        };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var service = new AdminService(dbContext);

        var created = await service.CreateProductAsync(new UpsertProductRequest(
            "Vitamin C Serum",
            "vitamin-c-serum",
            "NDR-VC-01",
            "Daily serum",
            "30ml",
            890000m,
            760000m,
            12,
            true,
            true,
            false,
            null,
            [category.Id],
            [
                new UpsertProductImageRequest(
                    "https://example.test/vitamin-c.jpg",
                    "Vitamin C Serum",
                    2,
                    false),
                new UpsertProductImageRequest(
                    "https://example.test/vitamin-c-primary.jpg",
                    "Vitamin C Serum primary",
                    1,
                    true)
            ]));

        Assert.Equal("vitamin-c-serum", created.Slug);
        Assert.Equal(category.Id, Assert.Single(created.CategoryIds));
        Assert.Equal(2, created.Images.Count);
        Assert.True(created.Images.First().IsPrimary);

        var updated = await service.UpdateProductAsync(created.Id, new UpsertProductRequest(
            "Vitamin C Serum Pro",
            "vitamin-c-serum-pro",
            "NDR-VC-02",
            "Updated serum",
            "30ml, pro",
            990000m,
            880000m,
            9,
            true,
            false,
            true,
            null,
            [category.Id],
            [
                new UpsertProductImageRequest(
                    "https://example.test/vitamin-c-pro.jpg",
                    "Vitamin C Serum Pro",
                    1,
                    false)
            ]));

        Assert.Equal("Vitamin C Serum Pro", updated.Name);
        Assert.Equal("NDR-VC-02", updated.Sku);
        Assert.Single(updated.Images);
        Assert.True(updated.Images.Single().IsPrimary);

        await service.DeleteProductAsync(created.Id);

        var deletedProduct = await dbContext.Products.SingleAsync(product => product.Id == created.Id);
        Assert.False(deletedProduct.IsActive);
    }

    [Fact]
    public async Task OrderManagement_UpdatesOrderStatusAndDashboardTotals()
    {
        await using var dbContext = CreateDbContext();
        var user = new User
        {
            Email = "customer@example.com",
            NormalizedEmail = "CUSTOMER@EXAMPLE.COM",
            FullName = "Customer User",
            PasswordHash = "hash"
        };
        var customerRole = new Role
        {
            Name = Role.Customer,
            NormalizedName = Role.Customer.ToUpperInvariant()
        };
        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = customerRole
        });
        var order = new Order
        {
            User = user,
            OrderNumber = "ORD-TEST-000001",
            CustomerFullName = "Customer User",
            CustomerEmail = "customer@example.com",
            ShippingAddress = "Test address",
            Status = OrderStatus.Paid,
            Subtotal = 500000m,
            ShippingAmount = 120000m,
            TaxAmount = 45000m,
            TotalAmount = 665000m
        };
        order.Items.Add(new OrderItem
        {
            ProductName = "Vitamin C Serum",
            Sku = "NDR-VC-01",
            Quantity = 1,
            UnitPrice = 500000m,
            TotalPrice = 500000m
        });
        order.Payments.Add(new Payment
        {
            GatewayName = "Test gateway",
            Amount = 665000m,
            Status = PaymentStatus.Succeeded,
            VerifiedAt = DateTimeOffset.UtcNow
        });
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var service = new AdminService(dbContext);

        var changed = await service.UpdateOrderStatusAsync(
            order.Id,
            new UpdateOrderStatusRequest(OrderStatus.Shipping));
        var dashboard = await service.GetDashboardAsync();

        Assert.Equal(OrderStatus.Shipping, changed.Status);
        Assert.Equal(665000m, dashboard.RevenueTotal);
        Assert.Single(dashboard.RecentOrders);
        Assert.Equal(1, dashboard.CustomerCount);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
