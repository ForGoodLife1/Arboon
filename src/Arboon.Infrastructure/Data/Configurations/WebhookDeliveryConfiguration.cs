using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arboon.Infrastructure.Data.Configurations;

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("WebhookDeliveries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EscrowId)
            .HasMaxLength(20);

        builder.Property(e => e.Payload)
            .HasColumnType("text"); // JSON data

        builder.Property(e => e.ResponseBody)
            .HasColumnType("text");

        // Relationships
        builder.HasOne(e => e.WebhookEndpoint)
            .WithMany(w => w.Deliveries)
            .HasForeignKey(e => e.WebhookEndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for the retry background service
        builder.HasIndex(e => new { e.IsFailed, e.Attempts });
    }
}
