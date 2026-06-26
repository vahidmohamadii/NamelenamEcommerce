using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using NaderEcommerce.Application.Catalog;

namespace NaderEcommerce.BlazorWeb.Services;

public sealed class CatalogApiClient(HttpClient httpClient)
{
    public async Task<CatalogHomeDto?> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CatalogHomeDto>("api/catalog/home", cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<CategoryDto>>(
                "api/catalog/categories",
                cancellationToken)
            ?? Array.Empty<CategoryDto>();
    }

    public async Task<PagedResult<ProductCardDto>?> GetProductsAsync(
        ProductCatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        var values = new Dictionary<string, string?>
        {
            ["search"] = query.Search,
            ["categorySlug"] = query.CategorySlug,
            ["minPrice"] = query.MinPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["maxPrice"] = query.MaxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["inStockOnly"] = query.InStockOnly ? "true" : null,
            ["sort"] = query.Sort,
            ["pageNumber"] = query.PageNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["pageSize"] = query.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        var path = QueryHelpers.AddQueryString(
            "api/catalog/products",
            values.Where(pair => !string.IsNullOrWhiteSpace(pair.Value)));

        return await httpClient.GetFromJsonAsync<PagedResult<ProductCardDto>>(path, cancellationToken);
    }

    public async Task<ProductDetailsDto?> GetProductAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/catalog/products/{Uri.EscapeDataString(slug)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDetailsDto>(cancellationToken);
    }
}
