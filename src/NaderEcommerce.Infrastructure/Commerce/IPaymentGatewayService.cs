namespace NaderEcommerce.Infrastructure.Commerce;

public interface IPaymentGatewayService
{
    PaymentGatewaySession CreateSession(Guid paymentId);
}
