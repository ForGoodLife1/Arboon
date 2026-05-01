using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Dispute;

public class OpenDisputeBuyerDto : OpenDisputeDto
{
    [JsonPropertyName("buyer_token")]
    public string? BuyerTokenString { get; set; }

    [JsonIgnore]
    public Guid? BuyerToken
    {
        get => Guid.TryParse(BuyerTokenString, out var g) ? g : null;
        set => BuyerTokenString = value?.ToString();
    }
}
