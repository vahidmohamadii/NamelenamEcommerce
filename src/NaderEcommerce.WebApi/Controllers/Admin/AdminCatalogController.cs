using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NaderEcommerce.Application.Admin;

namespace NaderEcommerce.WebApi.Controllers.Admin;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
public sealed class AdminCatalogController(
    IAdminService adminService,
    IValidator<UpsertCategoryRequest> categoryValidator,
    IValidator<UpsertProductRequest> productValidator,
    IOutputCacheStore outputCacheStore) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetDashboardAsync(cancellationToken));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetCategoriesAsync(cancellationToken));
    }

    [HttpPost("categories")]
    public async Task<ActionResult<AdminCategoryDto>> CreateCategory(
        UpsertCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await categoryValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            var category = await adminService.CreateCategoryAsync(request, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return Ok(category);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("categories/{categoryId:guid}")]
    public async Task<ActionResult<AdminCategoryDto>> UpdateCategory(
        Guid categoryId,
        UpsertCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await categoryValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            var category = await adminService.UpdateCategoryAsync(categoryId, request, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return Ok(category);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("categories/{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        try
        {
            await adminService.DeleteCategoryAsync(categoryId, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<AdminProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetProductsAsync(cancellationToken));
    }

    [HttpGet("products/{productId:guid}")]
    public async Task<ActionResult<AdminProductDto>> GetProduct(Guid productId, CancellationToken cancellationToken)
    {
        var product = await adminService.GetProductAsync(productId, cancellationToken);
        return product is null
            ? NotFound(new { message = "محصول پیدا نشد." })
            : Ok(product);
    }

    [HttpPost("products")]
    public async Task<ActionResult<AdminProductDto>> CreateProduct(
        UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await productValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            var product = await adminService.CreateProductAsync(request, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("products/{productId:guid}")]
    public async Task<ActionResult<AdminProductDto>> UpdateProduct(
        Guid productId,
        UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await productValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            var product = await adminService.UpdateProductAsync(productId, request, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPatch("products/{productId:guid}/active")]
    public async Task<ActionResult<AdminProductDto>> SetProductActive(
        Guid productId,
        SetProductActiveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await adminService.SetProductActiveAsync(productId, request, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpDelete("products/{productId:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            await adminService.DeleteProductAsync(productId, cancellationToken);
            await EvictCatalogCacheAsync(cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken)
    {
        return outputCacheStore.EvictByTagAsync("catalog", cancellationToken);
    }
}