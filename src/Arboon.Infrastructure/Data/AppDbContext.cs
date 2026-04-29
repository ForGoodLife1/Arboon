using Arboon.Application.Interfaces;
using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arboon.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Escrow> Escrows => Set<Escrow>();
    public DbSet<BuyerToken> BuyerTokens => Set<BuyerToken>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
