namespace NaderEcommerce.Application.Commerce;

public sealed record ShoppingCartDto(
    Guid CartId,
    IReadOnlyList<ShoppingCartItemDto> Items,
    string? CouponCode,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    bool CanCheckout);
