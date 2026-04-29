namespace Arboon.Domain.Exceptions;

public class InvalidBuyerTokenException : DomainException
{
    public InvalidBuyerTokenException()
        : base("INVALID_BUYER_TOKEN", "رمز المشتري غير صالح أو غير موجود")
    {
    }
}
