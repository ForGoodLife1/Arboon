namespace Arboon.Domain.Exceptions;

public class TokenAlreadyUsedException : DomainException
{
    public TokenAlreadyUsedException()
        : base("TOKEN_ALREADY_USED", "رمز التأكيد مستخدم مسبقاً")
    {
    }
}
