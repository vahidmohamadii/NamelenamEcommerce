namespace NaderEcommerce.Application.Commerce;

public interface IOrderService
{
    Task<ShoppingCartDto> GetCheckoutSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CheckoutSessionDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default);

    Task<OrderDetailsDto> VerifyPaymentAsync(Guid userId, Guid paymentId, VerifyPaymentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<OrderDetailsDto?> GetOrderAsync(Guid userId, string orderNumber, CancellationToken cancellationToken = default);
}
