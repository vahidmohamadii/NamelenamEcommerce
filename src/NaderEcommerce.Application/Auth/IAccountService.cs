namespace NaderEcommerce.Application.Auth;

public interface IAccountService
{
    Task<UserProfileDto> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);
}
