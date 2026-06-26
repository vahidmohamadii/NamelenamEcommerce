namespace NaderEcommerce.Application.Commerce;

public interface IShoppingCartService
{
    Task<ShoppingCartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ShoppingCartDto> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken = default);

    Task<ShoppingCartDto> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);

    Task<ShoppingCartDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    Task<ShoppingCartDto> ApplyCouponAsync(Guid userId, ApplyCouponRequest request, CancellationToken cancellationToken = default);

    Task<ShoppingCartDto> RemoveCouponAsync(Guid userId, CancellationToken cancellationToken = default);
}
