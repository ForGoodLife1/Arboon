using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arboon.Infrastructure.Data.Configurations;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.ToTable("Disputes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EscrowId)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.OpenedBy)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Resolution)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.AdminNote)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(e => e.Escrow)
            .WithMany(es => es.Disputes)
            .HasForeignKey(e => e.EscrowId)
            .OnDelete(DeleteBehavior.Restrict);

        // A business rule is one active dispute per escrow
        builder.HasIndex(e => e.EscrowId)
            .HasFilter("\"Status\" != 'RESOLVED'")
            .IsUnique();
    }
}
