using System.Net;
using System.Text.Json;
using Arboon.Application.Common;
using Arboon.Domain.Exceptions;
using FluentValidation;

namespace Arboon.API.Middleware;

/// <summary>
/// Global exception middleware that catches all exceptions and returns
/// consistent JSON error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            EscrowNotFoundException ex =>
                (HttpStatusCode.NotFound, ex.ErrorCode, ex.Message),

            InvalidStatusTransitionException ex =>
                (HttpStatusCode.Conflict, ex.ErrorCode, ex.Message),

            InvalidBuyerTokenException ex =>
                (HttpStatusCode.Unauthorized, ex.ErrorCode, ex.Message),

            TokenAlreadyUsedException ex =>
                (HttpStatusCode.Conflict, ex.ErrorCode, ex.Message),

            PaymentFailedException ex =>
                (HttpStatusCode.BadGateway, ex.ErrorCode, ex.Message),

            UnauthorizedException ex =>
                (HttpStatusCode.Unauthorized, ex.ErrorCode, ex.Message),

            DuplicateEmailException ex =>
                (HttpStatusCode.Conflict, ex.ErrorCode, ex.Message),

            DisputeNotFoundException ex =>
                (HttpStatusCode.NotFound, ex.ErrorCode, ex.Message),

            ActiveDisputeExistsException ex =>
                (HttpStatusCode.Conflict, ex.ErrorCode, ex.Message),

            WebhookEndpointNotFoundException ex =>
                (HttpStatusCode.NotFound, ex.ErrorCode, ex.Message),

            DomainException ex =>
                (HttpStatusCode.BadRequest, ex.ErrorCode, ex.Message),

            ValidationException ex =>
                (HttpStatusCode.BadRequest, ErrorCodes.ValidationError,
                    string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),

            _ =>
                (HttpStatusCode.InternalServerError, ErrorCodes.InternalError, "حدث خطأ غير متوقع")
        };

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Domain/Validation exception: {ErrorCode}", errorCode);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.ErrorResponse(errorCode, message);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}
