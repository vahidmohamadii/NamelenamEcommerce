namespace NaderEcommerce.Application.Commerce;

public interface IWishlistService
{
    Task<IReadOnlyList<WishlistProductDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WishlistProductDto>> AddAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WishlistProductDto>> RemoveAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
