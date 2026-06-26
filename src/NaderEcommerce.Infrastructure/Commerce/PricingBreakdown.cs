namespace NaderEcommerce.Infrastructure.Commerce;

internal sealed record PricingBreakdown(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount);
