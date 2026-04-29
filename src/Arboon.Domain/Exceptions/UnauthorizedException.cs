namespace Arboon.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "غير مصرح")
        : base("UNAUTHORIZED", message)
    {
    }
}
