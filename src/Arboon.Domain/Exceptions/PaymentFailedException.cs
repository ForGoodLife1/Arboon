namespace Arboon.Domain.Exceptions;

public class PaymentFailedException : DomainException
{
    public PaymentFailedException(string reason)
        : base("PAYMENT_FAILED", $"فشلت عملية الدفع: {reason}")
    {
    }
}
