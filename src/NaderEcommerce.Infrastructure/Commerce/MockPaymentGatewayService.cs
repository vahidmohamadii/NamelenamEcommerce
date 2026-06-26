namespace NaderEcommerce.Infrastructure.Commerce;

internal sealed class MockPaymentGatewayService : IPaymentGatewayService
{
    public PaymentGatewaySession CreateSession(Guid paymentId)
    {
        var transactionId = $"TXN-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var verificationToken = Convert.ToHexString(Guid.NewGuid().ToByteArray());

        return new PaymentGatewaySession(
            transactionId,
            verificationToken,
            $"/mock-payments/{paymentId}?token={verificationToken}",
            $"/api/orders/payments/{paymentId}/verify");
    }
}
