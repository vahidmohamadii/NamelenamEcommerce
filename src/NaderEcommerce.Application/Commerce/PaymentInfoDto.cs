using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Application.Commerce;

public sealed record PaymentInfoDto(
    Guid PaymentId,
    string GatewayName,
    string? GatewayTransactionId,
    PaymentStatus Status,
    decimal Amount,
    DateTimeOffset? VerifiedAt,
    string? FailureReason);
