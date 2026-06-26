using FluentValidation;

namespace NaderEcommerce.Application.Commerce;

public sealed class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(request => request.Quantity).GreaterThan(0).LessThanOrEqualTo(50);
    }
}
