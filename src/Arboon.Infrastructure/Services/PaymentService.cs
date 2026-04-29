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
        _logger.LogInformation(
            "💳 [MOCK] Processing payment: {Amount} {Currency} with card token {CardToken}",
            amount, currency, cardToken);

        // Mock: always succeeds
        return Task.FromResult(true);
    }
}
