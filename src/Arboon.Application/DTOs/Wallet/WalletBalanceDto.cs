using System.Text.Json.Serialization;

namespace Arboon.Application.DTOs.Wallet;

public class WalletBalanceDto
{
    [JsonPropertyName("available_balance")]
    public decimal AvailableBalance { get; set; }

    [JsonPropertyName("frozen_balance")]
    public decimal FrozenBalance { get; set; }
}
