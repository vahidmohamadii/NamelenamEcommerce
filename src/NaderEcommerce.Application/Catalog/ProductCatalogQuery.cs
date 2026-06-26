namespace NaderEcommerce.Application.Catalog;

public sealed record ProductCatalogQuery
{
    public string? Search { get; init; }
    public string? CategorySlug { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool InStockOnly { get; init; }
    public string? Sort { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
