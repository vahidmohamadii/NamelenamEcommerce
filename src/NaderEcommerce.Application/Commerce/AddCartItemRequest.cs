namespace NaderEcommerce.Application.Commerce;

public sealed record AddCartItemRequest(
    Guid ProductId,
    int Quantity);
