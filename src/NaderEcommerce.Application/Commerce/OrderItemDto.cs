namespace NaderEcommerce.Application.Commerce;

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
