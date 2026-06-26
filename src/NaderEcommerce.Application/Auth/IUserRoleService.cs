namespace NaderEcommerce.Application.Auth;

public interface IUserRoleService
{
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<UserSummaryDto> AssignRoleAsync(
        AssignRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto> SetUserActiveAsync(
        Guid userId,
        SetUserActiveRequest request,
        CancellationToken cancellationToken = default);
}
