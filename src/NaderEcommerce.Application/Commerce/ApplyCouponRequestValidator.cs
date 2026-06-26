using FluentValidation;

namespace NaderEcommerce.Application.Commerce;

public sealed class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequest>
{
    public ApplyCouponRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty().MaximumLength(64);
    }
}
