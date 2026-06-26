using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Common.Interfaces;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Cms;
using NaderEcommerce.Domain.Common;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    private static readonly Guid CustomerRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AdminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> Wishlist => Set<WishlistItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<QRLink> QRLinks => Set<QRLink>();
    public DbSet<WebsiteSettings> WebsiteSettings => Set<WebsiteSettings>();
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<Page> Pages => Set<Page>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ConfigureIdentity(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureCms(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.Property(user => user.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(160).IsRequired();
            entity.Property(user => user.PhoneNumber).HasMaxLength(32);
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.FailedLoginAttempts).HasDefaultValue(0);
            entity.HasIndex(user => user.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(role => role.Id);
            entity.Property(role => role.Name).HasMaxLength(64).IsRequired();
            entity.Property(role => role.NormalizedName).HasMaxLength(64).IsRequired();
            entity.HasIndex(role => role.NormalizedName).IsUnique();

            entity.HasData(
                new Role
                {
                    Id = CustomerRoleId,
                    Name = Role.Customer,
                    NormalizedName = Normalize(Role.Customer),
                    CreatedAt = DateTimeOffset.UnixEpoch
                },
                new Role
                {
                    Id = AdminRoleId,
                    Name = Role.Administrator,
                    NormalizedName = Normalize(Role.Administrator),
                    CreatedAt = DateTimeOffset.UnixEpoch
                });
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId);
            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.CreatedByIp).HasMaxLength(64);
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);
            entity.HasIndex(token => token.TokenHash).IsUnique();
        });
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).HasMaxLength(220).IsRequired();
            entity.Property(product => product.Slug).HasMaxLength(260).IsRequired();
            entity.Property(product => product.Sku).HasMaxLength(80).IsRequired();
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.DiscountPrice).HasPrecision(18, 2);
            entity.HasIndex(product => product.Slug).IsUnique();
            entity.HasIndex(product => product.Sku).IsUnique();
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(image => image.Id);
            entity.Property(image => image.Url).HasMaxLength(2048).IsRequired();
            entity.Property(image => image.AltText).HasMaxLength(220);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(160).IsRequired();
            entity.Property(category => category.Slug).HasMaxLength(220).IsRequired();
            entity.HasIndex(category => category.Slug).IsUnique();
            entity.HasOne(category => category.ParentCategory)
                .WithMany(category => category.Children)
                .HasForeignKey(category => category.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(productCategory => new { productCategory.ProductId, productCategory.CategoryId });
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(review => review.Id);
            entity.Property(review => review.Comment).HasMaxLength(2000);
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("Wishlist");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
        });

        modelBuilder.Entity<QRLink>(entity =>
        {
            entity.ToTable("QRLinks");
            entity.HasKey(link => link.Id);
            entity.Property(link => link.TargetUrl).HasMaxLength(2048).IsRequired();
            entity.Property(link => link.Label).HasMaxLength(160);
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.CustomerFullName).HasMaxLength(160).IsRequired();
            entity.Property(order => order.CustomerEmail).HasMaxLength(256).IsRequired();
            entity.Property(order => order.CustomerPhoneNumber).HasMaxLength(32);
            entity.Property(order => order.ShippingAddress).HasMaxLength(1000).IsRequired();
            entity.Property(order => order.PostalCode).HasMaxLength(32);
            entity.Property(order => order.Notes).HasMaxLength(2000);
            entity.Property(order => order.Subtotal).HasPrecision(18, 2);
            entity.Property(order => order.DiscountAmount).HasPrecision(18, 2);
            entity.Property(order => order.ShippingAmount).HasPrecision(18, 2);
            entity.Property(order => order.TaxAmount).HasPrecision(18, 2);
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(order => order.OrderNumber).IsUnique();
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProductName).HasMaxLength(220).IsRequired();
            entity.Property(item => item.Sku).HasMaxLength(80).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.TotalPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.GatewayName).HasMaxLength(80).IsRequired();
            entity.Property(payment => payment.GatewayTransactionId).HasMaxLength(160);
            entity.Property(payment => payment.VerificationToken).HasMaxLength(160);
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.Property(payment => payment.FailureReason).HasMaxLength(1000);
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.ToTable("ShoppingCarts");
            entity.HasKey(cart => cart.Id);
            entity.HasIndex(cart => cart.UserId).IsUnique();
        });

        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.ToTable("ShoppingCartItems");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.ShoppingCartId, item.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons");
            entity.HasKey(coupon => coupon.Id);
            entity.Property(coupon => coupon.Code).HasMaxLength(64).IsRequired();
            entity.Property(coupon => coupon.DiscountAmount).HasPrecision(18, 2);
            entity.Property(coupon => coupon.MinimumOrderAmount).HasPrecision(18, 2);
            entity.HasIndex(coupon => coupon.Code).IsUnique();
        });
    }

    private static void ConfigureCms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebsiteSettings>(entity =>
        {
            entity.ToTable("WebsiteSettings");
            entity.HasKey(settings => settings.Id);
            entity.Property(settings => settings.SiteName).HasMaxLength(160).IsRequired();
            entity.Property(settings => settings.LogoUrl).HasMaxLength(2048);
            entity.Property(settings => settings.SupportEmail).HasMaxLength(256);
            entity.Property(settings => settings.SupportPhone).HasMaxLength(32);
            entity.Property(settings => settings.SeoTitle).HasMaxLength(220);
            entity.Property(settings => settings.SeoDescription).HasMaxLength(500);
        });

        modelBuilder.Entity<Slider>(entity =>
        {
            entity.ToTable("Sliders");
            entity.HasKey(slider => slider.Id);
            entity.Property(slider => slider.Title).HasMaxLength(220).IsRequired();
            entity.Property(slider => slider.Subtitle).HasMaxLength(500);
            entity.Property(slider => slider.ImageUrl).HasMaxLength(2048).IsRequired();
            entity.Property(slider => slider.LinkUrl).HasMaxLength(2048);
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("Pages");
            entity.HasKey(page => page.Id);
            entity.Property(page => page.Key).HasMaxLength(80).IsRequired();
            entity.Property(page => page.Title).HasMaxLength(220).IsRequired();
            entity.Property(page => page.Slug).HasMaxLength(220).IsRequired();
            entity.Property(page => page.MetaTitle).HasMaxLength(220);
            entity.Property(page => page.MetaDescription).HasMaxLength(500);
            entity.HasIndex(page => page.Key).IsUnique();
            entity.HasIndex(page => page.Slug).IsUnique();
        });
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
