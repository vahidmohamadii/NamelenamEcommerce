using NaderEcommerce.Application.Auth;

namespace NaderEcommerce.Infrastructure.Tests.Auth;

public sealed class PasswordValidationTests
{
    [Fact]
    public void RegisterRequestValidator_RejectsWeakPassword()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(
            new RegisterRequest("weak@example.com", "password", "Weak Password", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void RegisterRequestValidator_AcceptsStrongPassword()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(
            new RegisterRequest("strong@example.com", "P@ssword123", "Strong Password", null));

        Assert.True(result.IsValid);
    }
}
