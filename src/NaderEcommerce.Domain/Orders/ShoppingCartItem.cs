using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Orders;

public sealed class ShoppingCartItem : BaseEntity
{
    public Guid ShoppingCartId { get; set; }
    public ShoppingCart ShoppingCart { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
