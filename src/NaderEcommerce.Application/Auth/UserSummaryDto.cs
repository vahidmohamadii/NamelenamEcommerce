namespace NaderEcommerce.Application.Auth;

public sealed record UserSummaryDto(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles);
