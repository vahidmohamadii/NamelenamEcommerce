namespace NaderEcommerce.Application.Commerce;

public sealed record ShoppingCartItemDto(
    Guid ProductId,
    string Name,
    string Slug,
    string Sku,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock,
    decimal LineTotal);
