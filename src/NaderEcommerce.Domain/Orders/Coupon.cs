using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Orders;

public sealed class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; } = new List<Order>();
}
