using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arboon.Infrastructure.Data.Configurations;

public class BuyerTokenConfiguration : IEntityTypeConfiguration<BuyerToken>
{
    public void Configure(EntityTypeBuilder<BuyerToken> builder)
    {
        builder.ToTable("BuyerTokens");

        builder.HasKey(bt => bt.Id);

        builder.Property(bt => bt.EscrowId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(bt => bt.Token)
            .IsRequired();

        builder.HasIndex(bt => bt.Token)
            .IsUnique();

        builder.Property(bt => bt.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(bt => bt.CreatedAt)
            .IsRequired();
    }
}
