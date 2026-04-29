using Arboon.Domain.Enums;
using Arboon.Domain.Exceptions;

namespace Arboon.Domain.Entities;

/// <summary>
/// Represents an escrow transaction between a seller and buyer.
/// Contains domain logic for state transitions and fee calculation.
/// </summary>
public class Escrow
{
    public string Id { get; set; } = GenerateId();
    public Guid SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal TotalToPay { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? Conditions { get; set; }
    public EscrowStatus Status { get; set; } = EscrowStatus.PENDING;
    public string? BuyerEmail { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }

    // Navigation properties
    public virtual User Seller { get; set; } = null!;
    public virtual BuyerToken? BuyerToken { get; set; }
    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    /// <summary>
    /// Generates a unique escrow ID in the format "esc_XXXXXXXXX" (9 random digits).
    /// </summary>
    public static string GenerateId()
    {
        var random = new Random();
        var digits = random.Next(100_000_000, 999_999_999);
        return $"esc_{digits}";
    }

    /// <summary>
    /// Calculates the escrow fee: 1% of amount, minimum 5.
    /// </summary>
    public static decimal CalculateFee(decimal amount)
    {
        return Math.Max(amount * 0.01m, 5m);
    }

    /// <summary>
    /// Initializes fee and total based on the escrow amount.
    /// </summary>
    public void CalculateAndSetFee()
    {
        Fee = CalculateFee(Amount);
        TotalToPay = Amount + Fee;
    }

    /// <summary>
    /// Transitions escrow from PENDING to FROZEN when buyer pays.
    /// </summary>
    public void Pay(string buyerEmail)
    {
        if (Status != EscrowStatus.PENDING)
            throw new InvalidStatusTransitionException(Status, EscrowStatus.FROZEN);

        Status = EscrowStatus.FROZEN;
        BuyerEmail = buyerEmail;
    }

    /// <summary>
    /// Transitions escrow from FROZEN/DISPUTED to RELEASED when buyer confirms delivery or admin resolves.
    /// </summary>
    public void Release()
    {
        if (Status != EscrowStatus.FROZEN && Status != EscrowStatus.DISPUTED)
            throw new InvalidStatusTransitionException(Status, EscrowStatus.RELEASED);

        Status = EscrowStatus.RELEASED;
        ReleasedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transitions escrow from PENDING to CANCELLED.
    /// </summary>
    public void Cancel()
    {
        if (Status != EscrowStatus.PENDING)
            throw new InvalidStatusTransitionException(Status, EscrowStatus.CANCELLED);

        Status = EscrowStatus.CANCELLED;
    }

    /// <summary>
    /// Transitions escrow from FROZEN to DISPUTED when a party opens a dispute.
    /// </summary>
    public void Dispute()
    {
        if (Status != EscrowStatus.FROZEN)
            throw new InvalidStatusTransitionException(Status, EscrowStatus.DISPUTED);

        Status = EscrowStatus.DISPUTED;
    }

    /// <summary>
    /// Transitions escrow from DISPUTED to REFUNDED when admin resolves the dispute in favor of buyer.
    /// </summary>
    public void Refund()
    {
        if (Status != EscrowStatus.DISPUTED)
            throw new InvalidStatusTransitionException(Status, EscrowStatus.REFUNDED);

        Status = EscrowStatus.REFUNDED;
    }
}
