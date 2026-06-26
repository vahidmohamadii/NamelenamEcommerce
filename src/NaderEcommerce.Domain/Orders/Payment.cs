using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Orders;

public sealed class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string GatewayName { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string? VerificationToken { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? FailureReason { get; set; }
}
