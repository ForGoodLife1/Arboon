namespace Arboon.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level errors.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
