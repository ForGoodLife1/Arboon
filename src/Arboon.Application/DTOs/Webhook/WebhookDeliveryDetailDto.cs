using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Webhook;

public class WebhookDeliveryDetailDto : WebhookDeliveryResponseDto
{
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    [JsonPropertyName("response_body")]
    public string? ResponseBody { get; set; }
}
