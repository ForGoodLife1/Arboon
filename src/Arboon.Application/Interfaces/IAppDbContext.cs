using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arboon.Application.Interfaces;

/// <summary>
/// Abstraction over the database context so Application layer
/// doesn't depend on Infrastructure/EF Core directly.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Escrow> Escrows { get; }
    DbSet<BuyerToken> BuyerTokens { get; }
    DbSet<Wallet> Wallets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
