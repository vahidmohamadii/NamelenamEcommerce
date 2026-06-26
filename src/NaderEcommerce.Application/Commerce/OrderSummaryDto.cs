using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Application.Commerce;

public sealed record OrderSummaryDto(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemCount,
    DateTimeOffset CreatedAt,
    PaymentStatus PaymentStatus);
