namespace Arboon.Domain.Exceptions;

public class DisputeNotFoundException : DomainException
{
    public DisputeNotFoundException(Guid id)
        : base("DISPUTE_NOT_FOUND", $"لم يتم العثور على النزاع: {id}")
    {
    }
}
