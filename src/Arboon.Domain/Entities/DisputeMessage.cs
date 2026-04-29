using Arboon.Domain.Enums;

namespace Arboon.Domain.Entities;

public class DisputeMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DisputeId { get; set; }
    public DisputeParty SenderType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Dispute Dispute { get; set; } = null!;
}
