using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Escrow;

public class PayEscrowDto
{
    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; } = "mock_card";

    [JsonPropertyName("card_token")]
    public string? CardToken { get; set; } = "tok_mock";

    [JsonPropertyName("buyer_email")]
    public string BuyerEmail { get; set; } = string.Empty;
}
