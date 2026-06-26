namespace NaderEcommerce.Application.Catalog;

public sealed record CatalogHomeDto(
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyList<ProductCardDto> FeaturedProducts,
    IReadOnlyList<ProductCardDto> BestSellers,
    IReadOnlyList<ProductCardDto> LatestProducts);
