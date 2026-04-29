using Arboon.Domain.Entities;
using Arboon.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arboon.Infrastructure.Data.Configurations;

public class EscrowConfiguration : IEntityTypeConfiguration<Escrow>
{
    public void Configure(EntityTypeBuilder<Escrow> builder)
    {
        builder.ToTable("Escrows");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Fee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.TotalToPay)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(e => e.Conditions)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.BuyerEmail)
            .HasMaxLength(255);

        builder.Property(e => e.PaymentUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Seller)
            .WithMany(u => u.Escrows)
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BuyerToken)
            .WithOne(bt => bt.Escrow)
            .HasForeignKey<BuyerToken>(bt => bt.EscrowId);
    }
}
