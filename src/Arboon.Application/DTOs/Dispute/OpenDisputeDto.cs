using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Dispute;

public class OpenDisputeDto
{
    [JsonPropertyName("escrow_id")]
    public string EscrowId { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}
