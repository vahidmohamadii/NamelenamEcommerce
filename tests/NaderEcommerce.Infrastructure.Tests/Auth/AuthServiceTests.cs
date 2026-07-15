using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Infrastructure.Auth;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Tests.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_AssignsCustomerRole()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var response = await service.RegisterAsync(
            new RegisterRequest("customer@example.com", "P@ssword123", "Customer User", "09120000000"),
            "127.0.0.1");

        Assert.Contains(Role.Customer, response.User.Roles);
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_LocksUserAfterConfiguredFailedAttempts()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext, maxFailedLoginAttempts: 2);

        await service.RegisterAsync(
            new RegisterRequest("lockout@example.com", "P@ssword123", "Locked User", "09120000000"),
            "127.0.0.1");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("lockout@example.com", "Wrong123!"), "127.0.0.1"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("lockout@example.com", "Wrong123!"), "127.0.0.1"));

        var user = await dbContext.Users.SingleAsync(user => user.Email == "lockout@example.com");

        Assert.Equal(2, user.FailedLoginAttempts);
        Assert.True(user.LockoutEndAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ChangePasswordAsync_RevokesExistingRefreshTokens()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var registered = await service.RegisterAsync(
            new RegisterRequest("password@example.com", "P@ssword123", "Password User", "09120000000"),
            "127.0.0.1");

        await service.ChangePasswordAsync(
            registered.User.Id,
            new ChangePasswordRequest("P@ssword123", "N3wP@ssword456"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshTokenAsync(
                new RefreshTokenRequest(registered.RefreshToken),
                "127.0.0.1"));

        var login = await service.LoginAsync(
            new LoginRequest("password@example.com", "N3wP@ssword456"),
            "127.0.0.1");

        Assert.Equal(registered.User.Id, login.User.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AuthService CreateService(
        ApplicationDbContext dbContext,
        int maxFailedLoginAttempts = 5)
    {
        return new AuthService(
            dbContext,
            new PasswordHasher<User>(),
            Options.Create(new JwtOptions
            {
                Issuer = "NaderEcommerce.Tests",
                Audience = "NaderEcommerce.Tests",
                Secret = "UnitTestSecretKeyForNaderEcommerceJwtTokenSigning12345",
                AccessTokenMinutes = 60,
                RefreshTokenDays = 14
            }),
            Options.Create(new AuthSecurityOptions
            {
                MaxFailedLoginAttempts = maxFailedLoginAttempts,
                LockoutMinutes = 15
            }));
    }
}
