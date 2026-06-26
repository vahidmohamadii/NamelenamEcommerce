using FluentValidation;

namespace NaderEcommerce.Application.Commerce;

public sealed class VerifyPaymentRequestValidator : AbstractValidator<VerifyPaymentRequest>
{
    public VerifyPaymentRequestValidator()
    {
        RuleFor(request => request.VerificationToken).NotEmpty().MaximumLength(160);
    }
}
