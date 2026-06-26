namespace NaderEcommerce.Application.Catalog;

public sealed record ProductCardDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    bool IsFeatured,
    bool IsBestSeller,
    string? PrimaryImageUrl,
    IReadOnlyList<string> Categories);
