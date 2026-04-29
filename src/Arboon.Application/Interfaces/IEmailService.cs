namespace Arboon.Application.Interfaces;

public interface IEmailService
{
    Task SendMagicLinkAsync(string buyerEmail, string escrowId, Guid buyerToken, string escrowTitle);
}
