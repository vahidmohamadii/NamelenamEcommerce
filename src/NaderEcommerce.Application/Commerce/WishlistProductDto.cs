namespace NaderEcommerce.Application.Commerce;

public sealed record WishlistProductDto(
    Guid ProductId,
    string Name,
    string Slug,
    string Sku,
    string? ImageUrl,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    DateTimeOffset AddedAt);
