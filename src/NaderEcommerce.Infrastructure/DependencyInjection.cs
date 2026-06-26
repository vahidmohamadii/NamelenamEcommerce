using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Application.Admin;
using NaderEcommerce.Application.Catalog;
using NaderEcommerce.Application.Cms;
using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Application.Common.Interfaces;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Infrastructure.Admin;
using NaderEcommerce.Infrastructure.Auth;
using NaderEcommerce.Infrastructure.Catalog;
using NaderEcommerce.Infrastructure.Cms;
using NaderEcommerce.Infrastructure.Commerce;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("رشته اتصال 'DefaultConnection' پیدا نشد.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthSecurityOptions>(configuration.GetSection(AuthSecurityOptions.SectionName));
        services.Configure<AdminBootstrapOptions>(configuration.GetSection(AdminBootstrapOptions.SectionName));
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICmsService, CmsService>();
        services.AddScoped<ShoppingCartService>();
        services.AddScoped<IShoppingCartService>(provider => provider.GetRequiredService<ShoppingCartService>());
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IPaymentGatewayService, MockPaymentGatewayService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
