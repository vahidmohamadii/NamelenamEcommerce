using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Auth;

public sealed class UserRoleService(ApplicationDbContext dbContext) : IUserRoleService
{
    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.Email)
            .Select(user => new AdminUserDto(
                user.Id,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.IsActive,
                user.FailedLoginAttempts,
                user.LockoutEndAt,
                user.UserRoles.Select(userRole => userRole.Role.Name).ToArray()))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSummaryDto> AssignRoleAsync(
        AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleName = NormalizeRole(request.RoleName);
        var normalizedRoleName = Normalize(roleName);

        var user = await dbContext.Users
            .Include(entity => entity.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(entity => entity.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("کاربر پیدا نشد.");
        }

        var role = await dbContext.Roles
            .SingleOrDefaultAsync(entity => entity.NormalizedName == normalizedRoleName, cancellationToken);

        if (role is null)
        {
            role = new Role
            {
                Name = roleName,
                NormalizedName = normalizedRoleName
            };
            dbContext.Roles.Add(role);
        }

        var alreadyAssigned = user.UserRoles
            .Any(userRole => userRole.Role.NormalizedName == normalizedRoleName);

        if (!alreadyAssigned)
        {
            user.UserRoles.Add(new UserRole
            {
                User = user,
                Role = role
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UserSummaryDto(user.Id, user.Email, user.FullName, roles);
    }

    public async Task<AdminUserDto> SetUserActiveAsync(
        Guid userId,
        SetUserActiveRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(entity => entity.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Include(entity => entity.RefreshTokens)
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("کاربر پیدا نشد.");
        }

        user.IsActive = request.IsActive;
        if (request.IsActive)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
        }
        else
        {
            foreach (var refreshToken in user.RefreshTokens.Where(token => token.IsActive))
            {
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AdminUserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.IsActive,
            user.FailedLoginAttempts,
            user.LockoutEndAt,
            user.UserRoles.Select(userRole => userRole.Role.Name).ToArray());
    }

    private static string NormalizeRole(string roleName)
    {
        return roleName.Trim().Equals(Role.Administrator, StringComparison.OrdinalIgnoreCase)
            ? Role.Administrator
            : Role.Customer;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
