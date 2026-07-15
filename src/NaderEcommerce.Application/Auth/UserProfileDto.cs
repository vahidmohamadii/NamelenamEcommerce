namespace NaderEcommerce.Application.Auth;

public sealed record UserProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? Address,
    IReadOnlyCollection<string> Roles);
