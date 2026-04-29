namespace Arboon.Domain.Exceptions;

public class ActiveDisputeExistsException : DomainException
{
    public ActiveDisputeExistsException(string escrowId)
        : base("ACTIVE_DISPUTE_EXISTS", $"يوجد نزاع نشط بالفعل على هذه العُهدة: {escrowId}")
    {
    }
}
