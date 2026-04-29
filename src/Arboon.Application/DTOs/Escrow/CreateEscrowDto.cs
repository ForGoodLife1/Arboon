using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Escrow;

public class CreateEscrowDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";

    [JsonPropertyName("conditions")]
    public string? Conditions { get; set; }
}
