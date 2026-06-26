using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Orders;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Commerce;

public sealed class ShoppingCartService(ApplicationDbContext dbContext) : IShoppingCartService
{
    public async Task<ShoppingCartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return CommerceMapping.ToCartDto(cart);
    }

    public async Task<ShoppingCartDto> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var product = await GetActiveProductAsync(request.ProductId, cancellationToken);

        var existingItem = cart.Items.SingleOrDefault(item => item.ProductId == request.ProductId);
        var newQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
        EnsureStock(product, newQuantity);

        if (existingItem is null)
        {
            var item = new ShoppingCartItem
            {
                ShoppingCartId = cart.Id,
                ShoppingCart = cart,
                ProductId = product.Id,
                Quantity = request.Quantity
            };

            dbContext.ShoppingCartItems.Add(item);
        }
        else
        {
            existingItem.Quantity = newQuantity;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return CommerceMapping.ToCartDto(cart);
    }

    public async Task<ShoppingCartDto> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.SingleOrDefault(entity => entity.ProductId == productId)
            ?? throw new InvalidOperationException("این محصول در سبد خرید نیست.");

        EnsureStock(item.Product, request.Quantity);
        item.Quantity = request.Quantity;

        await dbContext.SaveChangesAsync(cancellationToken);
        return CommerceMapping.ToCartDto(cart);
    }

    public async Task<ShoppingCartDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.SingleOrDefault(entity => entity.ProductId == productId);
        if (item is not null)
        {
            dbContext.ShoppingCartItems.Remove(item);
            cart.Items.Remove(item);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return CommerceMapping.ToCartDto(cart);
    }

    public async Task<ShoppingCartDto> ApplyCouponAsync(Guid userId, ApplyCouponRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var couponCode = request.Code.Trim().ToUpperInvariant();
        var coupon = await dbContext.Coupons
            .SingleOrDefaultAsync(entity => entity.Code.ToUpper() == couponCode, cancellationToken)
            ?? throw new InvalidOperationException("کوپن پیدا نشد.");

        var subtotal = cart.Items.Sum(item => CommercePricing.GetUnitPrice(item.Product) * item.Quantity);
        if (!CommercePricing.IsCouponUsable(coupon, subtotal))
        {
            throw new InvalidOperationException("این کوپن برای سبد خرید فعلی قابل استفاده نیست.");
        }

        cart.CouponId = coupon.Id;
        cart.Coupon = coupon;

        await dbContext.SaveChangesAsync(cancellationToken);
        return CommerceMapping.ToCartDto(cart);
    }

    public async Task<ShoppingCartDto> RemoveCouponAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        cart.CouponId = null;
        cart.Coupon = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommerceMapping.ToCartDto(cart);
    }

    internal async Task<ShoppingCart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await dbContext.ShoppingCarts
            .AsSplitQuery()
            .Include(entity => entity.Coupon)
            .Include(entity => entity.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product.Images)
            .SingleOrDefaultAsync(entity => entity.UserId == userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new ShoppingCart
        {
            UserId = userId
        };

        dbContext.ShoppingCarts.Add(cart);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetOrCreateCartAsync(userId, cancellationToken);
    }

    private async Task<Product> GetActiveProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .Include(product => product.Images)
            .SingleOrDefaultAsync(product => product.Id == productId && product.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("محصول پیدا نشد.");
    }

    private static void EnsureStock(Product product, int quantity)
    {
        if (quantity > product.Stock)
        {
            throw new InvalidOperationException("تعداد درخواستی بیشتر از موجودی است.");
        }
    }
}
