namespace NaderEcommerce.Application.Commerce;

public sealed record CheckoutRequest(
    string CustomerFullName,
    string CustomerEmail,
    string? CustomerPhoneNumber,
    string ShippingAddress,
    string? PostalCode,
    string? Notes);
