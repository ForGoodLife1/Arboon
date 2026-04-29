using Arboon.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Arboon.Infrastructure.Services;

/// <summary>
/// Mock payment service — always succeeds in development.
/// Replace with Stripe integration for production.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ProcessPaymentAsync(string cardToken, decimal amount, string currency)
    {
        _logger.LogInformation("Simulating payment of {Amount} {Currency} using token {CardToken}",
            amount, currency, cardToken);

        // Simulate network delay
        Thread.Sleep(1000);

        // Always succeed in MVP simulation unless token is specific
        bool success = cardToken != "tok_fail";

        if (success)
            _logger.LogInformation("Payment successful.");
        else
            _logger.LogWarning("Payment failed (simulated).");

        return Task.FromResult(success);
    }

    public Task<bool> ProcessRefundAsync(string escrowId, decimal amount, string currency)
    {
        _logger.LogInformation("Simulating REFUND of {Amount} {Currency} for escrow {EscrowId}",
            amount, currency, escrowId);

        // Simulate network delay
        Thread.Sleep(500);

        return Task.FromResult(true);
    }
}
