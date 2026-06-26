namespace NaderEcommerce.Application.Auth;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
