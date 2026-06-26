namespace NaderEcommerce.Application.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserSummaryDto User);
