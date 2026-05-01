using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Dispute;

public class AddDisputeMessageDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("buyer_token")]
    public string? BuyerTokenString { get; set; }

    [JsonIgnore]
    public Guid? BuyerToken
    {
        get => Guid.TryParse(BuyerTokenString, out var g) ? g : null;
        set => BuyerTokenString = value?.ToString();
    }
}
