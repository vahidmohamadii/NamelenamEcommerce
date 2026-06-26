namespace NaderEcommerce.Application.Catalog;

public sealed record ProductDetailsDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    string? Description,
    string? Specifications,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    string? QrLinkUrl,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyList<ProductCardDto> RelatedProducts);
