using AutoMapper;
using Arboon.Application.DTOs.Wallet;
using Arboon.Application.Interfaces;
using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arboon.Application.Services;

public class WalletService : IWalletService
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public WalletService(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            return new WalletBalanceDto
            {
                AvailableBalance = 0,
                FrozenBalance = 0
            };
        }

        return _mapper.Map<WalletBalanceDto>(wallet);
    }

    public async Task EnsureWalletExistsAsync(Guid userId)
    {
        var exists = await _context.Wallets.AnyAsync(w => w.UserId == userId);

        if (!exists)
        {
            _context.Wallets.Add(new Wallet
            {
                UserId = userId,
                AvailableBalance = 0,
                FrozenBalance = 0
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<WalletDataDto> GetWalletDataAsync(Guid userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        // Calculate "withdrawn" as total released escrow amounts for this seller
        var withdrawnTotal = await _context.Escrows
            .Where(e => e.SellerId == userId &&
                        e.Status == Arboon.Domain.Enums.EscrowStatus.RELEASED)
            .SumAsync(e => (decimal?)e.Amount) ?? 0m;

        var stats = new WalletStatsDto
        {
            Available = wallet?.AvailableBalance ?? 0m,
            Pending   = wallet?.FrozenBalance    ?? 0m,
            Withdrawn = withdrawnTotal
        };

        // Build a lightweight transaction list from released/cancelled escrows
        var recentEscrows = await _context.Escrows
            .Where(e => e.SellerId == userId &&
                        (e.Status == Arboon.Domain.Enums.EscrowStatus.RELEASED ||
                         e.Status == Arboon.Domain.Enums.EscrowStatus.FROZEN))
            .OrderByDescending(e => e.CreatedAt)
            .Take(20)
            .ToListAsync();

        var transactions = recentEscrows.Select(e => new TransactionDto
        {
            Id     = "TX-" + e.Id.ToUpperInvariant()[..6],
            Type   = "DEPOSIT",
            Amount = e.Amount,
            Method = "عُربون",
            Date   = (e.ReleasedAt ?? e.CreatedAt).ToString("yyyy-MM-dd"),
            Status = e.Status == Arboon.Domain.Enums.EscrowStatus.RELEASED ? "COMPLETED" : "PENDING"
        }).ToList();

        return new WalletDataDto { Stats = stats, Transactions = transactions };
    }

    public async Task RequestWithdrawalAsync(Guid userId, WithdrawalRequestDto dto)
    {
        // MVP: just validate funds are available and log the request.
        // A real implementation would create a WithdrawalRequest entity.
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("المحفظة غير موجودة");

        if (dto.Amount <= 0)
            throw new ArgumentException("المبلغ يجب أن يكون أكبر من الصفر");

        if (dto.Amount > wallet.AvailableBalance)
            throw new InvalidOperationException("الرصيد غير كافٍ");

        // تسجيل الطلب — سيتم ربطه بجدول WithdrawalRequests لاحقاً
        await Task.CompletedTask;
    }
}
