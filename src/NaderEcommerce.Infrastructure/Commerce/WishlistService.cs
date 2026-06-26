using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Commerce;

public sealed class WishlistService(ApplicationDbContext dbContext) : IWishlistService
{
    public async Task<IReadOnlyList<WishlistProductDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await LoadWishlistAsync(userId, cancellationToken);
        return items.Select(CommerceMapping.ToWishlistDto).ToArray();
    }

    public async Task<IReadOnlyList<WishlistProductDto>> AddAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(entity => entity.Id == productId && entity.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("محصول پیدا نشد.");

        var exists = await dbContext.Wishlist
            .AnyAsync(item => item.UserId == userId && item.ProductId == product.Id, cancellationToken);

        if (!exists)
        {
            await dbContext.Wishlist.AddAsync(new WishlistItem
            {
                UserId = userId,
                ProductId = product.Id
            }, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var items = await LoadWishlistAsync(userId, cancellationToken);
        return items.Select(CommerceMapping.ToWishlistDto).ToArray();
    }

    public async Task<IReadOnlyList<WishlistProductDto>> RemoveAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Wishlist
            .SingleOrDefaultAsync(entity => entity.UserId == userId && entity.ProductId == productId, cancellationToken);

        if (item is not null)
        {
            dbContext.Wishlist.Remove(item);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var items = await LoadWishlistAsync(userId, cancellationToken);
        return items.Select(CommerceMapping.ToWishlistDto).ToArray();
    }

    private async Task<IReadOnlyList<WishlistItem>> LoadWishlistAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Wishlist
            .AsSplitQuery()
            .AsNoTracking()
            .Include(item => item.Product)
                .ThenInclude(product => product.Images)
            .Where(item => item.UserId == userId && item.Product.IsActive)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
