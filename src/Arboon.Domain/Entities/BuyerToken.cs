namespace Arboon.Domain.Entities;

/// <summary>
/// Represents a magic link token for buyer identity verification.
/// </summary>
public class BuyerToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EscrowId { get; set; } = string.Empty;
    public Guid Token { get; set; } = Guid.NewGuid();
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Escrow Escrow { get; set; } = null!;
}
