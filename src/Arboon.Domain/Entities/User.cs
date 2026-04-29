namespace Arboon.Domain.Entities;

/// <summary>
/// Represents a seller (registered user) on the platform.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Role { get; set; } = "Seller"; // Seller, Admin

    // Navigation properties
    public virtual ICollection<Escrow> Escrows { get; set; } = new List<Escrow>();
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<WebhookEndpoint> WebhookEndpoints { get; set; } = new List<WebhookEndpoint>();
}
