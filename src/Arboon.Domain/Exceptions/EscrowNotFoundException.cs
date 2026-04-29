namespace Arboon.Domain.Exceptions;

public class EscrowNotFoundException : DomainException
{
    public EscrowNotFoundException(string escrowId)
        : base("ESCROW_NOT_FOUND", $"العُهدة غير موجودة: {escrowId}")
    {
    }
}
