namespace NaderEcommerce.Application.Commerce;

public sealed record CheckoutSessionDto(
    OrderDetailsDto Order,
    PaymentSessionDto Payment);
