using FluentValidation;

namespace NaderEcommerce.Application.Auth;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Must(PasswordRules.IsStrong)
            .WithMessage(PasswordRules.Message)
            .NotEqual(request => request.CurrentPassword)
            .WithMessage("NewPassword must be different from CurrentPassword.");
    }
}
