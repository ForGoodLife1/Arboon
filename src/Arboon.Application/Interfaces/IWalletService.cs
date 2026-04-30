using Arboon.Application.DTOs.Wallet;

namespace Arboon.Application.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceDto> GetBalanceAsync(Guid userId);
    Task EnsureWalletExistsAsync(Guid userId);
    Task<WalletDataDto> GetWalletDataAsync(Guid userId);
    Task RequestWithdrawalAsync(Guid userId, WithdrawalRequestDto dto);
}
