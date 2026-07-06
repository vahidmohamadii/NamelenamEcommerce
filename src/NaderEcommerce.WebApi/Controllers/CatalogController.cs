using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NaderEcommerce.Application.Catalog;

namespace NaderEcommerce.WebApi.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet("home")]
    [OutputCache(PolicyName = "PublicCatalog")]
    public async Task<ActionResult<CatalogHomeDto>> GetHome(CancellationToken cancellationToken)
    {
        return Ok(await catalogService.GetHomeAsync(cancellationToken));
    }

    [HttpGet("categories")]
    [OutputCache(PolicyName = "PublicCatalog")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        return Ok(await catalogService.GetCategoriesAsync(cancellationToken));
    }

    [HttpGet("products")]
    [OutputCache(PolicyName = "PublicCatalog")]
    public async Task<ActionResult<PagedResult<ProductCardDto>>> GetProducts(
        [FromQuery] ProductCatalogQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await catalogService.GetProductsAsync(query, cancellationToken));
    }

    [HttpGet("products/{slug}")]
    [OutputCache(PolicyName = "PublicCatalog")]
    public async Task<ActionResult<ProductDetailsDto>> GetProduct(
        string slug,
        CancellationToken cancellationToken)
    {
        var product = await catalogService.GetProductBySlugAsync(slug, cancellationToken);

        return product is null
            ? NotFound(new { message = "محصول پیدا نشد." })
            : Ok(product);
    }
}
