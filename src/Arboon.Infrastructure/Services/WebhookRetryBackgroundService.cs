using System.Security.Cryptography;
using System.Text;
using Arboon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arboon.Infrastructure.Services;

public class WebhookRetryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookRetryBackgroundService> _logger;

    public WebhookRetryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<WebhookRetryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRetriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred processing webhook retries.");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task ProcessRetriesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var now = DateTime.UtcNow;

        var failedDeliveries = await context.WebhookDeliveries
            .Include(d => d.WebhookEndpoint)
            .Where(d => d.IsFailed && d.Attempts < 3 && d.WebhookEndpoint.IsActive)
            .ToListAsync(stoppingToken);

        foreach (var delivery in failedDeliveries)
        {
            if (stoppingToken.IsCancellationRequested) break;

            if (!ShouldRetry(delivery, now)) continue;

            _logger.LogInformation("Retrying webhook delivery {DeliveryId} (Attempt {Attempt})", delivery.Id, delivery.Attempts + 1);

            delivery.Attempts++;
            delivery.LastAttemptAt = now;

            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                var signature = GenerateSignature(delivery.WebhookEndpoint.Secret, delivery.EventName, delivery.Payload);

                var request = new HttpRequestMessage(HttpMethod.Post, delivery.WebhookEndpoint.Url);
                request.Content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json");
                request.Headers.Add("Arboon-Event", delivery.EventName);
                request.Headers.Add("Arboon-Signature", $"sha256={signature}");
                request.Headers.Add("Arboon-Delivery-Id", delivery.Id.ToString());

                var response = await httpClient.SendAsync(request, stoppingToken);

                delivery.StatusCode = (int)response.StatusCode;
                delivery.ResponseBody = await response.Content.ReadAsStringAsync(stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    delivery.IsFailed = false;
                    delivery.DeliveredAt = now;
                    _logger.LogInformation("Webhook delivery {DeliveryId} succeeded on retry.", delivery.Id);
                }
            }
            catch (Exception ex)
            {
                delivery.ResponseBody = ex.Message;
                _logger.LogWarning("Webhook delivery {DeliveryId} failed again: {Message}", delivery.Id, ex.Message);
            }
        }

        if (failedDeliveries.Any())
        {
            await context.SaveChangesAsync(stoppingToken);
        }
    }

    private bool ShouldRetry(Arboon.Domain.Entities.WebhookDelivery delivery, DateTime now)
    {
        if (delivery.LastAttemptAt == null) return true;

        var minutesSinceLastAttempt = (now - delivery.LastAttemptAt.Value).TotalMinutes;

        return delivery.Attempts switch
        {
            1 => minutesSinceLastAttempt >= 1,   // Retry 1 after 1 min
            2 => minutesSinceLastAttempt >= 5,   // Retry 2 after 5 min
            _ => false                           // Beyond max attempts
        };
    }

    private string GenerateSignature(string secret, string eventName, string jsonPayload)
    {
        var data = $"arboon.{eventName}.{jsonPayload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
