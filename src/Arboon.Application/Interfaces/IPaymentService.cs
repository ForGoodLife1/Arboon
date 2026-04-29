namespace Arboon.Application.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(string cardToken, decimal amount, string currency);
    Task<bool> ProcessRefundAsync(string escrowId, decimal amount, string currency);
}
