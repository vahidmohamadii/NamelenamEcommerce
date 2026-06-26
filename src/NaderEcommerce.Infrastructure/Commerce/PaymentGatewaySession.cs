namespace NaderEcommerce.Infrastructure.Commerce;

public sealed record PaymentGatewaySession(
    string TransactionId,
    string VerificationToken,
    string PaymentUrl,
    string VerificationUrl);
