namespace NaderEcommerce.Application.Commerce;

public sealed record PaymentSessionDto(
    Guid PaymentId,
    string GatewayName,
    string TransactionId,
    string VerificationToken,
    string PaymentUrl,
    string VerificationUrl);
