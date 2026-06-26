namespace NaderEcommerce.Domain.Orders;

public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Preparing = 3,
    Shipping = 4,
    Completed = 5,
    Cancelled = 6,
    Refunded = 7
}
