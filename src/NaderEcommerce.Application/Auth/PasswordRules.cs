namespace NaderEcommerce.Application.Auth;

public static class PasswordRules
{
    public const string Message =
        "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";

    public static bool IsStrong(string password)
    {
        return password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(character => !char.IsLetterOrDigit(character));
    }
}
