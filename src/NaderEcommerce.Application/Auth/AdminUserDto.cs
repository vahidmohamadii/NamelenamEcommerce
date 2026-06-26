namespace NaderEcommerce.Application.Auth;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive,
    int FailedLoginAttempts,
    DateTimeOffset? LockoutEndAt,
    IReadOnlyCollection<string> Roles);
