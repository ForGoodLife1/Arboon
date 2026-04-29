using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arboon.Infrastructure.Data.Configurations;

public class WebhookEndpointConfiguration : IEntityTypeConfiguration<WebhookEndpoint>
{
    public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
    {
        builder.ToTable("WebhookEndpoints");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Secret)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Events)
            .IsRequired()
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.Seller)
            .WithMany(u => u.WebhookEndpoints)
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index for faster lookups when dispatching webhooks
        builder.HasIndex(e => new { e.SellerId, e.IsActive });
    }
}
