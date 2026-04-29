using Arboon.Domain.Enums;

namespace Arboon.Domain.Entities;

public class Dispute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EscrowId { get; set; } = string.Empty;
    public DisputeParty OpenedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; } = DisputeStatus.OPEN;
    public DisputeResolution? Resolution { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual Escrow Escrow { get; set; } = null!;
    public virtual ICollection<DisputeMessage> Messages { get; set; } = new List<DisputeMessage>();
}
