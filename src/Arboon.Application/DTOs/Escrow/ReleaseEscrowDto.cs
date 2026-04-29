using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Escrow;

public class ReleaseEscrowDto
{
    [JsonPropertyName("buyer_token")]
    public Guid? BuyerToken { get; set; }
}
