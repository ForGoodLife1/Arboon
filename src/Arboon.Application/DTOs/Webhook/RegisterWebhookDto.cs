using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Webhook;

public class RegisterWebhookDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("secret")]
    public string Secret { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<string> Events { get; set; } = new();
}
