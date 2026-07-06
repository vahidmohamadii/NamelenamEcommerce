using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Cms;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminBootstrapOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
        {
            await dbContext.Database.MigrateAsync();
        }

        var adminRole = await EnsureRoleAsync(dbContext, Role.Administrator);
        await EnsureRoleAsync(dbContext, Role.Customer);
        await EnsureCatalogSeedAsync(dbContext);
        await EnsureCmsSeedAsync(dbContext);
        await EnsureSeedMediaUrlsAsync(dbContext);

        if (string.IsNullOrWhiteSpace(adminOptions.Email) ||
            string.IsNullOrWhiteSpace(adminOptions.Password))
        {
            logger.LogWarning("Admin bootstrap user was not configured.");
            await dbContext.SaveChangesAsync();
            return;
        }

        var normalizedEmail = Normalize(adminOptions.Email);
        var adminUser = await dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Email = adminOptions.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                FullName = string.IsNullOrWhiteSpace(adminOptions.FullName)
                    ? "مدیر سیستم"
                    : adminOptions.FullName.Trim(),
                IsActive = true
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminOptions.Password);
            adminUser.UserRoles.Add(new UserRole
            {
                User = adminUser,
                Role = adminRole
            });

            dbContext.Users.Add(adminUser);
            logger.LogInformation("Admin bootstrap user created for {Email}.", adminUser.Email);
        }
        else if (adminUser.UserRoles.All(userRole => userRole.Role.NormalizedName != Normalize(Role.Administrator)))
        {
            adminUser.UserRoles.Add(new UserRole
            {
                User = adminUser,
                Role = adminRole
            });
            logger.LogInformation("Admin role assigned to existing user {Email}.", adminUser.Email);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Role> EnsureRoleAsync(ApplicationDbContext dbContext, string roleName)
    {
        var normalizedName = Normalize(roleName);
        var role = await dbContext.Roles
            .SingleOrDefaultAsync(entity => entity.NormalizedName == normalizedName);

        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            Name = roleName,
            NormalizedName = normalizedName
        };
        dbContext.Roles.Add(role);
        return role;
    }

    private static async Task EnsureCatalogSeedAsync(ApplicationDbContext dbContext)
    {
        await EnsureCouponSeedAsync(dbContext);

        if (await dbContext.Products.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var skincare = CreateCategory(
            "0e37c321-a2d4-4b86-b0ad-17db50c63d3e",
            "مراقبت پوست",
            "skincare",
            "کرم، سرم و شوینده‌های روزانه",
            10,
            now);
        var makeup = CreateCategory(
            "73f70982-7ea3-4446-94ef-217656a6a4b0",
            "آرایش صورت",
            "makeup",
            "محصولات منتخب آرایشی برای استفاده روزانه",
            20,
            now);
        var fragrance = CreateCategory(
            "2bb5b4f3-29cc-464c-b5df-c5980fe8eb0e",
            "عطر و بادی اسپلش",
            "fragrance",
            "رایحه‌های سبک، ماندگار و هدیه‌پسند",
            30,
            now);
        var tools = CreateCategory(
            "87521c1d-8a11-4dc5-a8d8-8b6f532105e7",
            "ابزار زیبایی",
            "beauty-tools",
            "براش، پد و ابزار کاربردی مراقبت",
            40,
            now);

        dbContext.Categories.AddRange(skincare, makeup, fragrance, tools);

        AddProduct(
            dbContext,
            "6ddf85e8-3b01-4eb7-9b32-41409672879d",
            "سرم ویتامین C نادر",
            "nader-vitamin-c-serum",
            "NDR-SER-VC",
            "سرم سبک روزانه با بافت زودجذب برای کمک به شفافیت پوست.",
            "حجم: 30ml\nمناسب: انواع پوست\nروش مصرف: روزی یک بار قبل از مرطوب‌کننده",
            890000,
            760000,
            34,
            true,
            true,
            now.AddMinutes(-9),
            "/images/products/vitamin-c-serum.svg",
            skincare);

        AddProduct(
            dbContext,
            "a6a56362-3fe5-495d-8829-071768c248e7",
            "کرم آبرسان روزانه",
            "daily-hydra-cream",
            "NDR-CRM-HYD",
            "کرم مرطوب‌کننده با پایان سبک برای استفاده زیر آرایش.",
            "حجم: 50ml\nبافت: کرمی سبک\nویژگی: بدون حس چربی",
            640000,
            null,
            51,
            true,
            false,
            now.AddMinutes(-8),
            "/images/products/hydra-cream.svg",
            skincare);

        AddProduct(
            dbContext,
            "0d0c69f4-aea6-44dd-a948-827f636940c3",
            "رژ لب مخملی شماره 08",
            "velvet-lipstick-08",
            "NDR-LIP-08",
            "رنگ گرم و پوشش یکدست با حس نرم و مخملی.",
            "رنگ: رز آجری\nپوشش: نیمه مات\nماندگاری: مناسب استفاده روزانه",
            390000,
            330000,
            82,
            true,
            true,
            now.AddMinutes(-7),
            "/images/products/velvet-lipstick.svg",
            makeup);

        AddProduct(
            dbContext,
            "c078f52c-f25d-4f40-a206-1f1666a88887",
            "پالت سایه چهار رنگ",
            "four-tone-eye-palette",
            "NDR-EYE-04",
            "چهار رنگ هماهنگ برای آرایش سریع روزانه و مهمانی.",
            "رنگ‌ها: نود، رز، قهوه‌ای، شامپاینی\nفینیش: مات و براق",
            720000,
            650000,
            19,
            false,
            true,
            now.AddMinutes(-6),
            "/images/products/eye-palette.svg",
            makeup,
            tools);

        AddProduct(
            dbContext,
            "d33ab1a8-8a5f-496f-887c-388833489a6c",
            "بادی اسپلش گل سفید",
            "white-flower-body-splash",
            "NDR-BSP-WF",
            "رایحه تمیز، سبک و روزانه با نت‌های گلی.",
            "حجم: 200ml\nگروه بویایی: گلی ملایم\nمناسب: استفاده روزانه",
            460000,
            null,
            46,
            false,
            true,
            now.AddMinutes(-5),
            "/images/products/body-splash.svg",
            fragrance);

        AddProduct(
            dbContext,
            "1bdb46db-daa8-4f05-ae53-7718eed990df",
            "ست براش آرایشی 7 عددی",
            "seven-piece-brush-set",
            "NDR-BRS-07",
            "ست کاربردی برای زیرسازی، رژگونه، سایه و هایلایتر.",
            "تعداد: 7 عدد\nجنس مو: الیاف نرم مصنوعی\nکیف نگهدارنده: دارد",
            980000,
            840000,
            24,
            true,
            false,
            now.AddMinutes(-4),
            "/images/products/brush-set.svg",
            tools,
            makeup);

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureCouponSeedAsync(ApplicationDbContext dbContext)
    {
        var existingCodes = await dbContext.Coupons
            .Select(coupon => coupon.Code)
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;
        var coupons = new[]
        {
            new Coupon
            {
                Id = Guid.Parse("9e18e5c5-e515-4969-a2af-68015f13c22a"),
                Code = "WELCOME150",
                DiscountAmount = 150000m,
                MinimumOrderAmount = 800000m,
                UsageLimit = 200,
                IsActive = true,
                StartsAt = now.AddDays(-7),
                EndsAt = now.AddMonths(3),
                CreatedAt = now
            },
            new Coupon
            {
                Id = Guid.Parse("634d6139-ee1c-4fdb-8a1f-38599f9321c8"),
                Code = "FREESHIP",
                DiscountAmount = 120000m,
                MinimumOrderAmount = 1200000m,
                UsageLimit = 100,
                IsActive = true,
                StartsAt = now.AddDays(-7),
                EndsAt = now.AddMonths(2),
                CreatedAt = now
            }
        };

        foreach (var coupon in coupons.Where(coupon => !existingCodes.Contains(coupon.Code)))
        {
            dbContext.Coupons.Add(coupon);
        }
    }

    private static async Task EnsureCmsSeedAsync(ApplicationDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;

        if (!await dbContext.WebsiteSettings.AnyAsync())
        {
            dbContext.WebsiteSettings.Add(new WebsiteSettings
            {
                SiteName = "فروشگاه نفس",
                SupportEmail = "support@nafashshop786.com",
                SupportPhone = "+98 21 0000 0000",
                Address = "تهران، ایران",
                SeoTitle = "فروشگاه نفس | لوازم آرایشی و بهداشتی نفس",
                SeoDescription = "فروشگاه نفس مرجع خرید لوازم آرایشی نفس و لوازم ارایشی و بهداشتی نفس با محصولات مراقبت پوست، آرایش، عطر و ابزار زیبایی. nafasshop",
                CreatedAt = now
            });
        }

        if (!await dbContext.Sliders.AnyAsync())
        {
            dbContext.Sliders.Add(new Slider
            {
                Title = "فروشگاه نفس",
                Subtitle = "لوازم آرایشی نفس و محصولات بهداشتی منتخب برای روتین زیبایی و مراقبت روزانه.",
                ImageUrl = "/images/hero/beauty-essentials.svg",
                LinkUrl = "/products",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = now
            });
        }

        var existingKeys = await dbContext.Pages
            .Select(page => page.Key)
            .ToListAsync();

        if (!existingKeys.Contains("about-us"))
        {
            dbContext.Pages.Add(new Page
            {
                Key = "about-us",
                Title = "درباره فروشگاه نفس",
                Slug = "about-us",
                Content = "فروشگاه نفس یک فروشگاه آنلاین متمرکز برای لوازم آرایشی و بهداشتی، مراقبت پوست، عطر و ابزار زیبایی است.\n\nدر فروشگاه نفس تلاش می‌کنیم خرید لوازم آرایشی نفس و محصولات بهداشتی کاربردی، ساده، شفاف و قابل اعتماد باشد.",
                MetaTitle = "درباره فروشگاه نفس",
                MetaDescription = "با فروشگاه نفس، فروشگاه لوازم آرایشی و بهداشتی نفس، بیشتر آشنا شوید.",
                IsPublished = true,
                CreatedAt = now
            });
        }

        if (!existingKeys.Contains("contact-us"))
        {
            dbContext.Pages.Add(new Page
            {
                Key = "contact-us",
                Title = "تماس با ما",
                Slug = "contact-us",
                Content = "برای پیگیری سفارش یا انتخاب محصول نیاز به راهنمایی داری؟ با تیم پشتیبانی فروشگاه نفس تماس بگیر.\n\nایمیل: support@nafashshop786.com\nتلفن: +98 21 0000 0000",
                MetaTitle = "تماس با فروشگاه نفس",
                MetaDescription = "راه‌های ارتباط با پشتیبانی فروشگاه نفس.",
                IsPublished = true,
                CreatedAt = now
            });
        }
    }

    private static Category CreateCategory(
        string id,
        string name,
        string slug,
        string description,
        int sortOrder,
        DateTimeOffset createdAt)
    {
        return new Category
        {
            Id = Guid.Parse(id),
            Name = name,
            Slug = slug,
            Description = description,
            SortOrder = sortOrder,
            CreatedAt = createdAt
        };
    }

    private static async Task EnsureSeedMediaUrlsAsync(ApplicationDbContext dbContext)
    {
        var seedImageUrls = new Dictionary<string, string>
        {
            ["nader-vitamin-c-serum"] = "/images/products/vitamin-c-serum.svg",
            ["daily-hydra-cream"] = "/images/products/hydra-cream.svg",
            ["velvet-lipstick-08"] = "/images/products/velvet-lipstick.svg",
            ["four-tone-eye-palette"] = "/images/products/eye-palette.svg",
            ["white-flower-body-splash"] = "/images/products/body-splash.svg",
            ["seven-piece-brush-set"] = "/images/products/brush-set.svg"
        };

        var products = await dbContext.Products
            .Include(product => product.Images)
            .Where(product => seedImageUrls.Keys.Contains(product.Slug))
            .ToListAsync();

        foreach (var product in products)
        {
            var imageUrl = seedImageUrls[product.Slug];
            var images = product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .ToArray();

            if (images.Length == 0)
            {
                product.Images.Add(new ProductImage
                {
                    ProductId = product.Id,
                    Url = imageUrl,
                    AltText = product.Name,
                    DisplayOrder = 1,
                    IsPrimary = true
                });
                continue;
            }

            for (var index = 0; index < images.Length; index++)
            {
                images[index].Url = imageUrl;
                images[index].AltText ??= index == 0
                    ? product.Name
                    : $"گالری {product.Name}";
                images[index].DisplayOrder = index + 1;
                images[index].IsPrimary = index == 0;
            }
        }

        var sliders = await dbContext.Sliders.ToListAsync();
        foreach (var slider in sliders.Where(slider => slider.ImageUrl.Contains("images.unsplash.com")))
        {
            slider.ImageUrl = "/images/hero/beauty-essentials.svg";
        }

        await dbContext.SaveChangesAsync();
    }

    private static void AddProduct(
        ApplicationDbContext dbContext,
        string id,
        string name,
        string slug,
        string sku,
        string description,
        string specifications,
        decimal price,
        decimal? discountPrice,
        int stock,
        bool isFeatured,
        bool isBestSeller,
        DateTimeOffset createdAt,
        string imageUrl,
        params Category[] categories)
    {
        var productId = Guid.Parse(id);
        var qrLink = new QRLink
        {
            Id = Guid.NewGuid(),
            Label = name,
            TargetUrl = $"/products/{slug}",
            CreatedAt = createdAt
        };

        var product = new Product
        {
            Id = productId,
            Name = name,
            Slug = slug,
            Sku = sku,
            Description = description,
            Specifications = specifications,
            Price = price,
            DiscountPrice = discountPrice,
            Stock = stock,
            IsFeatured = isFeatured,
            IsBestSeller = isBestSeller,
            QrLink = qrLink,
            CreatedAt = createdAt
        };

        product.Images.Add(new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = imageUrl,
            AltText = name,
            DisplayOrder = 1,
            IsPrimary = true,
            CreatedAt = createdAt
        });

        product.Images.Add(new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = imageUrl.Replace("w=900", "w=1200"),
            AltText = $"گالری {name}",
            DisplayOrder = 2,
            CreatedAt = createdAt
        });

        foreach (var category in categories)
        {
            product.ProductCategories.Add(new ProductCategory
            {
                ProductId = productId,
                CategoryId = category.Id
            });
        }

        dbContext.Products.Add(product);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
