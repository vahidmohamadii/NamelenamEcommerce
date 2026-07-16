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
            IsActive = true,
            SortOrder = 10
        };
        var secondCategory = new Category
        {
            Name = "Serums",
            Slug = "serums",
            IsActive = true,
            SortOrder = 20
        };
        dbContext.Categories.AddRange(category, secondCategory);
        await dbContext.SaveChangesAsync();
        var categoryId = category.Id;
        var secondCategoryId = secondCategory.Id;
        dbContext.ChangeTracker.Clear();

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
            [categoryId, categoryId],
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
        Assert.Equal(categoryId, Assert.Single(created.CategoryIds));
        Assert.Equal(2, created.Images.Count);
        Assert.True(created.Images.First().IsPrimary);
        Assert.Equal(
            ["https://example.test/vitamin-c-primary.jpg", "https://example.test/vitamin-c.jpg"],
            created.Images.Select(image => image.Url));
        Assert.Equal(2, await dbContext.ProductImages.CountAsync(image => image.ProductId == created.Id));
        Assert.Equal(1, await dbContext.ProductCategories.CountAsync(productCategory => productCategory.ProductId == created.Id));

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
            [categoryId, secondCategoryId],
            [
                new UpsertProductImageRequest(
                    "https://example.test/vitamin-c-pro-front.jpg",
                    "Vitamin C Serum Pro front",
                    1,
                    false),
                new UpsertProductImageRequest(
                    "https://example.test/vitamin-c-pro-box.jpg",
                    "Vitamin C Serum Pro box",
                    2,
                    false)
            ]));

        Assert.Equal("Vitamin C Serum Pro", updated.Name);
        Assert.Equal("NDR-VC-02", updated.Sku);
        Assert.Equal(2, updated.CategoryIds.Count);
        Assert.Contains(categoryId, updated.CategoryIds);
        Assert.Contains(secondCategoryId, updated.CategoryIds);
        Assert.Equal(2, updated.Images.Count);
        Assert.Single(updated.Images.Where(image => image.IsPrimary));
        Assert.True(updated.Images.Single(image => image.DisplayOrder == 1).IsPrimary);
        Assert.Equal(2, await dbContext.ProductImages.CountAsync(image => image.ProductId == created.Id));
        Assert.Equal(2, await dbContext.ProductCategories.CountAsync(productCategory => productCategory.ProductId == created.Id));

        var trimmed = await service.UpdateProductAsync(created.Id, new UpsertProductRequest(
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
            [secondCategoryId],
            [
                new UpsertProductImageRequest(
                    "https://example.test/vitamin-c-pro-box.jpg",
                    "Vitamin C Serum Pro box",
                    2,
                    false)
            ]));

        Assert.Equal(secondCategoryId, Assert.Single(trimmed.CategoryIds));
        Assert.Single(trimmed.Images);
        Assert.True(trimmed.Images.Single().IsPrimary);
        Assert.Equal(1, await dbContext.ProductImages.CountAsync(image => image.ProductId == created.Id));
        Assert.Equal(1, await dbContext.ProductCategories.CountAsync(productCategory => productCategory.ProductId == created.Id));

        await service.DeleteProductAsync(created.Id);

        var deletedProduct = await dbContext.Products.SingleAsync(product => product.Id == created.Id);
        Assert.False(deletedProduct.IsActive);
        Assert.DoesNotContain(await service.GetProductsAsync(), product => product.Id == created.Id);

        var reactivated = await service.SetProductActiveAsync(
            created.Id,
            new SetProductActiveRequest(true));

        Assert.True(reactivated.IsActive);
        Assert.Contains(await service.GetProductsAsync(), product => product.Id == created.Id);
    }

    [Fact]
    public async Task ProductManagement_CreatesInactiveProductAndTogglesVisibility()
    {
        await using var dbContext = CreateDbContext();
        var category = new Category
        {
            Name = "Makeup",
            Slug = "makeup",
            IsActive = true
        };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var service = new AdminService(dbContext);

        var created = await service.CreateProductAsync(new UpsertProductRequest(
            "Velvet Lipstick",
            "velvet-lipstick",
            "NDR-LIP-01",
            null,
            null,
            390000m,
            null,
            20,
            false,
            false,
            false,
            null,
            [category.Id],
            []));

        Assert.False(created.IsActive);
        Assert.Empty(created.Images);
        Assert.DoesNotContain(await service.GetProductsAsync(), product => product.Id == created.Id);

        var activated = await service.SetProductActiveAsync(created.Id, new SetProductActiveRequest(true));

        Assert.True(activated.IsActive);
        Assert.Contains(await service.GetProductsAsync(), product => product.Id == created.Id);

        var deactivated = await service.SetProductActiveAsync(created.Id, new SetProductActiveRequest(false));

        Assert.False(deactivated.IsActive);
        Assert.DoesNotContain(await service.GetProductsAsync(), product => product.Id == created.Id);
    }

    [Fact]
    public async Task ProductManagement_RejectsDuplicateProductSlugOrSku()
    {
        await using var dbContext = CreateDbContext();
        var category = new Category
        {
            Name = "Tools",
            Slug = "tools",
            IsActive = true
        };
        dbContext.Categories.Add(category);
        dbContext.Products.Add(new Product
        {
            Name = "Brush Set",
            Slug = "brush-set",
            Sku = "NDR-BRS-01",
            Price = 980000m,
            Stock = 5,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new AdminService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductAsync(new UpsertProductRequest(
            "Brush Set Duplicate Slug",
            "brush-set",
            "NDR-BRS-02",
            null,
            null,
            980000m,
            null,
            5,
            true,
            false,
            false,
            null,
            [category.Id],
            [])));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductAsync(new UpsertProductRequest(
            "Brush Set Duplicate SKU",
            "brush-set-new",
            "NDR-BRS-01",
            null,
            null,
            980000m,
            null,
            5,
            true,
            false,
            false,
            null,
            [category.Id],
            [])));
    }

    [Fact]
    public async Task ProductManagement_RejectsMissingCategory()
    {
        await using var dbContext = CreateDbContext();
        var service = new AdminService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductAsync(new UpsertProductRequest(
            "Missing Category Product",
            "missing-category-product",
            "NDR-MISSING-CAT",
            null,
            null,
            100000m,
            null,
            3,
            true,
            false,
            false,
            null,
            [Guid.NewGuid()],
            [])));
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
