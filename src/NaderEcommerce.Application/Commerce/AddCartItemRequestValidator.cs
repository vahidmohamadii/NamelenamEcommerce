using FluentValidation;

namespace NaderEcommerce.Application.Commerce;

public sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(request => request.ProductId).NotEmpty();
        RuleFor(request => request.Quantity).GreaterThan(0).LessThanOrEqualTo(50);
    }
}
