using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Dispute;

public class ResolveDisputeDto
{
    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = string.Empty; // ReleasedToSeller or RefundedToBuyer

    [JsonPropertyName("admin_note")]
    public string? AdminNote { get; set; }
}
