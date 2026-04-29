namespace Arboon.Domain.Exceptions;

public class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email)
        : base("USER_ALREADY_EXISTS", $"البريد الإلكتروني مسجل مسبقاً: {email}")
    {
    }
}
