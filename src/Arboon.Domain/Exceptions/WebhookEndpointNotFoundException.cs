namespace Arboon.Domain.Exceptions;

public class WebhookEndpointNotFoundException : DomainException
{
    public WebhookEndpointNotFoundException(Guid id)
        : base("WEBHOOK_ENDPOINT_NOT_FOUND", $"لم يتم العثور على الرابط: {id}")
    {
    }
}
