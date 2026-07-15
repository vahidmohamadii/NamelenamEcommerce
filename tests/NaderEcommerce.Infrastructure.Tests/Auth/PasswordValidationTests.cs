using NaderEcommerce.Application.Auth;

namespace NaderEcommerce.Infrastructure.Tests.Auth;

public sealed class PasswordValidationTests
{
    [Fact]
    public void RegisterRequestValidator_RejectsWeakPassword()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(
            new RegisterRequest("weak@example.com", "password", "Weak Password", "09120000000"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void RegisterRequestValidator_AcceptsStrongPassword()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(
            new RegisterRequest("strong@example.com", "P@ssword123", "Strong Password", "09120000000"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_RejectsEmptyPhoneNumber()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(
            new RegisterRequest("phone@example.com", "P@ssword123", "Phone User", string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.PhoneNumber));
    }
}
