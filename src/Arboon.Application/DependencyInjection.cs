using Arboon.Application.Interfaces;
using Arboon.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Arboon.Application;

/// <summary>
/// Extension method to register Application layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // FluentValidation - register all validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Application services
        services.AddScoped<IEscrowService, EscrowService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDisputeService, DisputeService>();
        services.AddScoped<IWebhookService, WebhookService>();

        return services;
    }
}
