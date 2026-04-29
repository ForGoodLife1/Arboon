using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Dispute;

public class OpenDisputeBuyerDto : OpenDisputeDto
{
    [JsonPropertyName("buyer_token")]
    public Guid? BuyerToken { get; set; }
}
