using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Dispute;

public class DisputeResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("escrow_id")]
    public string EscrowId { get; set; } = string.Empty;

    [JsonPropertyName("opened_by")]
    public string OpenedBy { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [JsonPropertyName("escrow_title")]
    public string? EscrowTitle { get; set; }

    [JsonPropertyName("escrow_amount")]
    public decimal? EscrowAmount { get; set; }

    [JsonPropertyName("escrow_currency")]
    public string? EscrowCurrency { get; set; }
}
