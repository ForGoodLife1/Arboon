using Arboon.Application.DTOs.Wallet;

namespace Arboon.Application.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceDto> GetBalanceAsync(Guid userId);
    Task EnsureWalletExistsAsync(Guid userId);
}
