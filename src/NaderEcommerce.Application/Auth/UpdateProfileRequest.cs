namespace NaderEcommerce.Application.Auth;

public sealed record UpdateProfileRequest(
    string FullName,
    string? PhoneNumber);
