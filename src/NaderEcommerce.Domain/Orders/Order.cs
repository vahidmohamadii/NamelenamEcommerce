using NaderEcommerce.Domain.Common;
using NaderEcommerce.Domain.Identity;

namespace NaderEcommerce.Domain.Orders;

public sealed class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhoneNumber { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; } = new List<Payment>();
}
