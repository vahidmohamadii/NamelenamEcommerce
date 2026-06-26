namespace NaderEcommerce.Application.Commerce;

public sealed record VerifyPaymentRequest(
    string VerificationToken,
    bool Succeed = true);
