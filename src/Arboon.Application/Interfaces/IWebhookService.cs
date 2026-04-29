namespace Arboon.Application.Interfaces;

public interface IWebhookService
{
    Task DispatchAsync(string eventName, string escrowId, object payload);
}
