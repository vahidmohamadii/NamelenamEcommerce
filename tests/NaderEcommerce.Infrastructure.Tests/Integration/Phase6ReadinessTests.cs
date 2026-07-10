using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Application.Catalog;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Infrastructure.Catalog;
using NaderEcommerce.Infrastructure.Cms;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Tests.Integration;

public sealed class Phase6ReadinessTests
{
    [Fact]
    public async Task DatabaseInitializer_SeedsRolesAdminCatalogCmsAndCouponsWithoutDuplicates()
    {
        await using var services = CreateInitializerProvider();

        await services.InitializeDatabaseAsync();

        await using (var scope = services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.True(await dbContext.Roles.AnyAsync(role => role.Name == Role.Customer));
            Assert.True(await dbContext.Roles.AnyAsync(role => role.Name == Role.Administrator));
            Assert.True(await dbContext.Users.AnyAsync(user => user.Email == "admin@naderecommerce.local"));
            Assert.True(await dbContext.Categories.CountAsync() >= 4);
            Assert.True(await dbContext.Products.CountAsync() >= 6);
            Assert.True(await dbContext.Coupons.AnyAsync(coupon => coupon.Code == "WELCOME150"));
            Assert.True(await dbContext.WebsiteSettings.AnyAsync());
            Assert.True(await dbContext.Sliders.AnyAsync(slider => slider.IsActive));
            Assert.True(await dbContext.FaqItems.AnyAsync(faq => faq.IsActive));
            Assert.True(await dbContext.Pages.AnyAsync(page => page.Key == "about-us"));
            Assert.True(await dbContext.Pages.AnyAsync(page => page.Key == "contact-us"));
        }

        await services.InitializeDatabaseAsync();

        await using (var scope = services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.Equal(2, await dbContext.Coupons.CountAsync());
            Assert.Equal(4, await dbContext.Categories.CountAsync());
            Assert.Equal(6, await dbContext.Products.CountAsync());
            Assert.Single(await dbContext.Users
                .Where(user => user.Email == "admin@naderecommerce.local")
                .ToListAsync());
        }
    }

    [Fact]
    public async Task CatalogService_UsesSeededDataForHomeSearchDetailsAndPagination()
    {
        await using var services = CreateInitializerProvider();
        await services.InitializeDatabaseAsync();

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = new CatalogService(dbContext);
        var cmsService = new CmsService(dbContext);

        var home = await service.GetHomeAsync();
        var faqs = await cmsService.GetActiveFaqsAsync();
        var products = await service.GetProductsAsync(new ProductCatalogQuery
        {
            PageNumber = -5,
            PageSize = 500,
            Search = "NDR-",
            InStockOnly = true
        });
        var details = await service.GetProductBySlugAsync("nader-vitamin-c-serum");

        Assert.NotEmpty(home.Categories);
        Assert.NotEmpty(home.FeaturedProducts);
        Assert.NotEmpty(home.LatestProducts);
        Assert.NotEmpty(faqs);
        Assert.All(faqs, faq => Assert.False(string.IsNullOrWhiteSpace(faq.Question)));
        Assert.Equal(1, products.PageNumber);
        Assert.Equal(48, products.PageSize);
        Assert.True(products.TotalCount >= 6);
        Assert.NotNull(details);
        Assert.Equal("NDR-SER-VC", details.Sku);
        Assert.NotEmpty(details.Images);
        Assert.NotEmpty(details.Categories);
        Assert.NotNull(details.QrLinkUrl);
    }

    private static ServiceProvider CreateInitializerProvider()
    {
        var databaseName = Guid.NewGuid().ToString();
        var configuration = new TestConfiguration(new Dictionary<string, string?>
        {
            ["Database:ApplyMigrationsOnStartup"] = "false",
            ["SeedData:Admin:Email"] = "admin@naderecommerce.local",
            ["SeedData:Admin:Password"] = "Admin@123456",
            ["SeedData:Admin:FullName"] = "مدیر سیستم"
        });

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName))
            .AddScoped<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddSingleton<IOptions<AdminBootstrapOptions>>(Options.Create(new AdminBootstrapOptions
            {
                Email = "admin@naderecommerce.local",
                Password = "Admin@123456",
                FullName = "مدیر سیستم"
            }))
            .BuildServiceProvider();
    }

    private sealed class TestConfiguration(IReadOnlyDictionary<string, string?> values) : IConfiguration
    {
        public string? this[string key]
        {
            get => values.TryGetValue(key, out var value) ? value : null;
            set => throw new NotSupportedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return [];
        }

        public IChangeToken GetReloadToken()
        {
            return NoopChangeToken.Instance;
        }

        public IConfigurationSection GetSection(string key)
        {
            return new TestConfigurationSection(key, this[key]);
        }
    }

    private sealed class TestConfigurationSection(string key, string? value) : IConfigurationSection
    {
        public string? this[string key]
        {
            get => null;
            set => throw new NotSupportedException();
        }

        public string Key { get; } = key;
        public string Path { get; } = key;
        public string? Value { get; set; } = value;

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return [];
        }

        public IChangeToken GetReloadToken()
        {
            return NoopChangeToken.Instance;
        }

        public IConfigurationSection GetSection(string key)
        {
            return new TestConfigurationSection(key, null);
        }
    }

    private sealed class NoopChangeToken : IChangeToken
    {
        public static readonly NoopChangeToken Instance = new();

        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            return NoopDisposable.Instance;
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
