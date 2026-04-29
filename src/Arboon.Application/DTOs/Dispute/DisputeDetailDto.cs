using System.Text.Json.Serialization;
using Arboon.Application.DTOs.Escrow;

namespace Arboon.Application.DTOs.Dispute;

public class DisputeDetailDto : DisputeResponseDto
{
    [JsonPropertyName("admin_note")]
    public string? AdminNote { get; set; }

    [JsonPropertyName("escrow")]
    public EscrowResponseDto? Escrow { get; set; }

    [JsonPropertyName("messages")]
    public List<DisputeMessageDto> Messages { get; set; } = new();
}
