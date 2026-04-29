namespace Arboon.Domain.Entities;

/// <summary>
/// Represents a seller's wallet with available and frozen balances.
/// </summary>
public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public decimal AvailableBalance { get; set; } = 0m;
    public decimal FrozenBalance { get; set; } = 0m;

    // Navigation property
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Freeze funds when escrow is paid. Adds to frozen balance.
    /// </summary>
    public void FreezeFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        FrozenBalance += amount;
    }

    /// <summary>
    /// Release frozen funds to available balance when escrow is released.
    /// </summary>
    public void ReleaseFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (FrozenBalance < amount)
            throw new InvalidOperationException("Insufficient frozen balance");

        FrozenBalance -= amount;
        AvailableBalance += amount;
    }
}
