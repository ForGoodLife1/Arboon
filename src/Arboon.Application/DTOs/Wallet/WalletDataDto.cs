namespace Arboon.Application.DTOs.Wallet;

public class WalletStatsDto
{
    public decimal Available { get; set; }
    public decimal Pending { get; set; }
    public decimal Withdrawn { get; set; }
}

public class TransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // WITHDRAWAL | DEPOSIT
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // PENDING | COMPLETED | FAILED
}

public class WalletDataDto
{
    public WalletStatsDto Stats { get; set; } = new();
    public List<TransactionDto> Transactions { get; set; } = new();
}

public class WithdrawalRequestDto
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;       // INSTAPAY | BANK_TRANSFER | WALLET
    public string AccountDetails { get; set; } = string.Empty;
}
