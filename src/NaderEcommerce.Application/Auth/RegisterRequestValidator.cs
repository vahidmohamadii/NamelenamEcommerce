using FluentValidation;

namespace NaderEcommerce.Application.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Must(PasswordRules.IsStrong)
            .WithMessage(PasswordRules.Message);

        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(request => request.PhoneNumber)
            .MaximumLength(32);
    }
}
