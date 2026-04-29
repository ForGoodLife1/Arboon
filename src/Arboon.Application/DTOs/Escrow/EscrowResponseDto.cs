using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Escrow;

public class EscrowResponseDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("seller_name")]
    public string? SellerName { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("fee")]
    public decimal Fee { get; set; }

    [JsonPropertyName("total_to_pay")]
    public decimal TotalToPay { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("conditions")]
    public string? Conditions { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("buyer_email")]
    public string? BuyerEmail { get; set; }

    [JsonPropertyName("payment_url")]
    public string? PaymentUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("released_at")]
    public DateTime? ReleasedAt { get; set; }
}
