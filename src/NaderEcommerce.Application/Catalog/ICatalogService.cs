namespace NaderEcommerce.Application.Catalog;

public interface ICatalogService
{
    Task<CatalogHomeDto> GetHomeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<ProductCardDto>> GetProductsAsync(
        ProductCatalogQuery query,
        CancellationToken cancellationToken = default);

    Task<ProductDetailsDto?> GetProductBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
