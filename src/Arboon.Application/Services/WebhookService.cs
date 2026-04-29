using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Arboon.Application.Interfaces;
using Arboon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arboon.Application.Services;

public class WebhookService : IWebhookService
{
    private readonly IAppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IAppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task DispatchAsync(string eventName, string escrowId, object payload)
    {
        var escrow = await _context.Escrows.FindAsync(escrowId);
        if (escrow == null) return;

        // Find active endpoints for the seller subscribed to this event
        var endpoints = await _context.WebhookEndpoints
            .Where(e => e.SellerId == escrow.SellerId && e.IsActive && e.Events.Contains(eventName))
            .ToListAsync();

        if (!endpoints.Any()) return;

        var jsonPayload = JsonSerializer.Serialize(new
        {
            @event = eventName,
            timestamp = DateTime.UtcNow.ToString("O"),
            data = payload
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10); // Standard timeout

        foreach (var endpoint in endpoints)
        {
            var deliveryId = Guid.NewGuid();
            var delivery = new WebhookDelivery
            {
                Id = deliveryId,
                WebhookEndpointId = endpoint.Id,
                EscrowId = escrowId,
                EventName = eventName,
                Payload = jsonPayload,
                Attempts = 1,
                LastAttemptAt = DateTime.UtcNow
            };

            _context.WebhookDeliveries.Add(delivery);

            try
            {
                var signature = GenerateSignature(endpoint.Secret, eventName, jsonPayload);

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                request.Headers.Add("Arboon-Event", eventName);
                request.Headers.Add("Arboon-Signature", $"sha256={signature}");
                request.Headers.Add("Arboon-Delivery-Id", deliveryId.ToString());

                var response = await httpClient.SendAsync(request);
                
                delivery.StatusCode = (int)response.StatusCode;
                delivery.ResponseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    delivery.DeliveredAt = DateTime.UtcNow;
                }
                else
                {
                    delivery.IsFailed = true;
                    _logger.LogWarning("Webhook delivery {DeliveryId} failed with status {StatusCode}", deliveryId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                delivery.IsFailed = true;
                delivery.ResponseBody = ex.Message;
                _logger.LogError(ex, "Exception while sending webhook delivery {DeliveryId} to {Url}", deliveryId, endpoint.Url);
            }
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateSignature(string secret, string eventName, string jsonPayload)
    {
        var data = $"arboon.{eventName}.{jsonPayload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
