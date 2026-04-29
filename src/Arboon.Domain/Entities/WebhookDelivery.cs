namespace Arboon.Domain.Entities;

public class WebhookDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WebhookEndpointId { get; set; }
    public string? EscrowId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public int Attempts { get; set; } = 0;
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsFailed { get; set; } = false;

    // Navigation property
    public virtual WebhookEndpoint WebhookEndpoint { get; set; } = null!;
}
