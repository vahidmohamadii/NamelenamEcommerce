using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Auth;

public sealed class AuthService(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthSecurityOptions> securityOptions)
    : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly AuthSecurityOptions _securityOptions = securityOptions.Value;

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Normalize(request.Email);
        var emailExists = await dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("کاربری با این ایمیل قبلا ثبت شده است.");
        }

        var customerRole = await EnsureRoleAsync(Role.Customer, cancellationToken);

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            FullName = request.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim()
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        var response = CreateAuthResponse(user, [customerRole.Name]);

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = customerRole
        });
        user.RefreshTokens.Add(CreateRefreshToken(response.RefreshToken, ipAddress));

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await dbContext.Users
            .Include(entity => entity.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(entity => entity.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (IsLockedOut(user))
        {
            throw new UnauthorizedAccessException("User account is temporarily locked.");
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= _securityOptions.MaxFailedLoginAttempts)
            {
                user.LockoutEndAt = DateTimeOffset.UtcNow.AddMinutes(_securityOptions.LockoutMinutes);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.FailedLoginAttempts = 0;
        user.LockoutEndAt = null;

        var roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
        var response = CreateAuthResponse(user, roles);
        var refreshToken = CreateRefreshToken(response.RefreshToken, ipAddress);
        refreshToken.UserId = user.Id;
        dbContext.RefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var refreshToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive || !refreshToken.User.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var roles = refreshToken.User.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
        var response = CreateAuthResponse(refreshToken.User, roles);
        var newRefreshToken = CreateRefreshToken(response.RefreshToken, ipAddress);
        newRefreshToken.UserId = refreshToken.UserId;

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.ReplacedByTokenHash = HashToken(response.RefreshToken);
        dbContext.RefreshTokens.Add(newRefreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task RevokeRefreshTokenAsync(
        RefreshTokenRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (userId.HasValue && refreshToken.UserId != userId.Value)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(entity => entity.RefreshTokens)
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("کاربر پیدا نشد.");
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.CurrentPassword);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Current password is invalid.");
        }

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);

        foreach (var refreshToken in user.RefreshTokens.Where(token => token.IsActive))
        {
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse CreateAuthResponse(User user, IReadOnlyCollection<string> roles)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var accessToken = CreateAccessToken(user, roles, expiresAt);
        var refreshToken = GenerateSecureToken();

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserSummaryDto(user.Id, user.Email, user.FullName, roles));
    }

    private string CreateAccessToken(User user, IEnumerable<string> roles, DateTimeOffset expiresAt)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken CreateRefreshToken(string token, string? ipAddress)
    {
        return new RefreshToken
        {
            TokenHash = HashToken(token),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = ipAddress
        };
    }

    private async Task<Role> EnsureRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var normalizedName = Normalize(roleName);
        var role = await dbContext.Roles
            .SingleOrDefaultAsync(entity => entity.NormalizedName == normalizedName, cancellationToken);

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

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static bool IsLockedOut(User user)
    {
        return user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTimeOffset.UtcNow;
    }
}
