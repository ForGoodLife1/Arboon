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
}
