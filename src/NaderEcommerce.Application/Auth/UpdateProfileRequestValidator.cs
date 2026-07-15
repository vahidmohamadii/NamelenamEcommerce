using FluentValidation;

namespace NaderEcommerce.Application.Auth;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(request => request.PhoneNumber)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(request => request.Address)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
