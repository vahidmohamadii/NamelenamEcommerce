using NaderEcommerce.Domain.Common;
using NaderEcommerce.Domain.Identity;

namespace NaderEcommerce.Domain.Orders;

public sealed class ShoppingCart : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public ICollection<ShoppingCartItem> Items { get; } = new List<ShoppingCartItem>();
}
