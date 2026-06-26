using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Application.Commerce;

public sealed record OrderDetailsDto(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    string CustomerFullName,
    string CustomerEmail,
    string? CustomerPhoneNumber,
    string ShippingAddress,
    string? PostalCode,
    string? Notes,
    string? CouponCode,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<PaymentInfoDto> Payments);
