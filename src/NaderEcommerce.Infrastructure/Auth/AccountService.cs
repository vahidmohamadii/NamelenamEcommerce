using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Auth;

public sealed class AccountService(ApplicationDbContext dbContext) : IAccountService
{
    public async Task<UserProfileDto> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(entity => entity.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("کاربر پیدا نشد.");
        }

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.Address,
            user.UserRoles.Select(userRole => userRole.Role.Name).ToArray());
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(entity => entity.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException("کاربر پیدا نشد.");
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.Address = request.Address.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.Address,
            user.UserRoles.Select(userRole => userRole.Role.Name).ToArray());
    }
}
