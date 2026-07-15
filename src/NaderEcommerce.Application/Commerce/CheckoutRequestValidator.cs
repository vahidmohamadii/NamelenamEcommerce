using FluentValidation;

namespace NaderEcommerce.Application.Commerce;

public sealed class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(request => request.CustomerFullName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.CustomerPhoneNumber).NotEmpty().MaximumLength(32);
        RuleFor(request => request.ShippingAddress).NotEmpty().MaximumLength(1000);
        RuleFor(request => request.PostalCode).MaximumLength(32);
        RuleFor(request => request.Notes).MaximumLength(2000);
    }
}
