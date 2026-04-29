using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Escrow;

public class BuyerStatusResponseDto
{
    [JsonPropertyName("escrow_status")]
    public string EscrowStatus { get; set; } = string.Empty;

    [JsonPropertyName("is_authorized_buyer")]
    public bool IsAuthorizedBuyer { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}
