using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Webhook;

public class WebhookDeliveryResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("escrow_id")]
    public string? EscrowId { get; set; }

    [JsonPropertyName("event_name")]
    public string EventName { get; set; } = string.Empty;

    [JsonPropertyName("status_code")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }

    [JsonPropertyName("is_failed")]
    public bool IsFailed { get; set; }

    [JsonPropertyName("last_attempt_at")]
    public DateTime? LastAttemptAt { get; set; }

    [JsonPropertyName("delivered_at")]
    public DateTime? DeliveredAt { get; set; }
}
