using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Catalog;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Catalog;

public sealed class CatalogService(ApplicationDbContext dbContext) : ICatalogService
{
    private const int MaxPageSize = 48;

    public async Task<CatalogHomeDto> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var categories = await GetCategoriesAsync(cancellationToken);

        var featured = await LoadProductCards(dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.IsFeatured)
            .OrderByDescending(product => product.CreatedAt)
            .Take(8), cancellationToken);

        var bestSellers = await LoadProductCards(dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.IsBestSeller)
            .OrderByDescending(product => product.CreatedAt)
            .Take(8), cancellationToken);

        var latest = await LoadProductCards(dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderByDescending(product => product.CreatedAt)
            .Take(8), cancellationToken);

        return new CatalogHomeDto(categories, featured, bestSellers, latest);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .AsSplitQuery()
            .Include(category => category.ProductCategories)
                .ThenInclude(productCategory => productCategory.Product)
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return categories
            .Where(category => category.ParentCategoryId is null)
            .Select(category => MapCategory(category, categories))
            .ToArray();
    }

    public async Task<PagedResult<ProductCardDto>> GetProductsAsync(
        ProductCatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var products = dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(product =>
                product.Name.Contains(search) ||
                product.Sku.Contains(search) ||
                (product.Description != null && product.Description.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
        {
            var categorySlug = query.CategorySlug.Trim();
            products = products.Where(product => product.ProductCategories
                .Any(productCategory =>
                    productCategory.Category.IsActive &&
                    productCategory.Category.Slug == categorySlug));
        }

        if (query.MinPrice is not null)
        {
            products = products.Where(product => (product.DiscountPrice ?? product.Price) >= query.MinPrice);
        }

        if (query.MaxPrice is not null)
        {
            products = products.Where(product => (product.DiscountPrice ?? product.Price) <= query.MaxPrice);
        }

        if (query.InStockOnly)
        {
            products = products.Where(product => product.Stock > 0);
        }

        products = ApplySort(products, query.Sort);

        var totalCount = await products.CountAsync(cancellationToken);
        var page = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var items = await LoadProductCards(page, cancellationToken);

        return new PagedResult<ProductCardDto>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<ProductDetailsDto?> GetProductBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.Images)
            .Include(entity => entity.QrLink)
            .Include(entity => entity.ProductCategories)
                .ThenInclude(productCategory => productCategory.Category)
            .SingleOrDefaultAsync(
                entity => entity.IsActive && entity.Slug == slug,
                cancellationToken);

        if (product is null)
        {
            return null;
        }

        var categoryIds = product.ProductCategories
            .Select(productCategory => productCategory.CategoryId)
            .ToArray();

        var related = await LoadProductCards(dbContext.Products
            .AsNoTracking()
            .Where(entity =>
                entity.IsActive &&
                entity.Id != product.Id &&
                entity.ProductCategories.Any(productCategory => categoryIds.Contains(productCategory.CategoryId)))
            .OrderByDescending(entity => entity.IsFeatured)
            .ThenByDescending(entity => entity.CreatedAt)
            .Take(4), cancellationToken);

        return new ProductDetailsDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Sku,
            product.Description,
            product.Specifications,
            product.Price,
            product.DiscountPrice,
            product.Stock,
            product.QrLink?.IsActive == true ? product.QrLink.TargetUrl : null,
            product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .Take(10)
                .Select(MapImage)
                .ToArray(),
            product.ProductCategories
                .Where(productCategory => productCategory.Category.IsActive)
                .OrderBy(productCategory => productCategory.Category.SortOrder)
                .Select(productCategory => MapCategory(productCategory.Category, Array.Empty<Category>()))
                .ToArray(),
            related);
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> products, string? sort)
    {
        return sort?.Trim().ToLowerInvariant() switch
        {
            "price_asc" => products
                .OrderBy(product => product.DiscountPrice ?? product.Price)
                .ThenBy(product => product.Name),
            "price_desc" => products
                .OrderByDescending(product => product.DiscountPrice ?? product.Price)
                .ThenBy(product => product.Name),
            "name" => products.OrderBy(product => product.Name),
            "newest" => products.OrderByDescending(product => product.CreatedAt),
            _ => products
                .OrderByDescending(product => product.IsFeatured)
                .ThenByDescending(product => product.IsBestSeller)
                .ThenByDescending(product => product.CreatedAt)
        };
    }

    private async Task<IReadOnlyList<ProductCardDto>> LoadProductCards(
        IQueryable<Product> query,
        CancellationToken cancellationToken)
    {
        var products = await query
            .AsSplitQuery()
            .Include(product => product.Images)
            .Include(product => product.ProductCategories)
                .ThenInclude(productCategory => productCategory.Category)
            .ToListAsync(cancellationToken);

        return products.Select(MapProductCard).ToArray();
    }

    private static ProductCardDto MapProductCard(Product product)
    {
        return new ProductCardDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Sku,
            product.Price,
            product.DiscountPrice,
            product.Stock,
            product.IsFeatured,
            product.IsBestSeller,
            product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .Select(image => image.Url)
                .FirstOrDefault(),
            product.ProductCategories
                .Where(productCategory => productCategory.Category.IsActive)
                .OrderBy(productCategory => productCategory.Category.SortOrder)
                .ThenBy(productCategory => productCategory.Category.Name)
                .Select(productCategory => productCategory.Category.Name)
                .ToArray());
    }

    private static CategoryDto MapCategory(Category category, IReadOnlyList<Category> allCategories)
    {
        var children = allCategories
            .Where(child => child.ParentCategoryId == category.Id)
            .OrderBy(child => child.SortOrder)
            .ThenBy(child => child.Name)
            .Select(child => MapCategory(child, allCategories))
            .ToArray();

        var activeProductCount = category.ProductCategories
            .Count(productCategory => productCategory.Product.IsActive);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            activeProductCount,
            children);
    }

    private static ProductImageDto MapImage(ProductImage image)
    {
        return new ProductImageDto(
            image.Url,
            image.AltText,
            image.IsPrimary,
            image.DisplayOrder);
    }
}
