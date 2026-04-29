using System.Security.Claims;
using Arboon.Application.Common;
using Arboon.Application.DTOs.Webhook;
using Arboon.Domain.Entities;
using Arboon.Domain.Exceptions;
using Arboon.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arboon.API.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
// [Authorize] // Commented out for MVP, as requested
public class WebhooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IValidator<RegisterWebhookDto> _registerValidator;

    public WebhooksController(
        AppDbContext context,
        IValidator<RegisterWebhookDto> registerValidator)
    {
        _context = context;
        _registerValidator = registerValidator;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue("user_id");
        return claim != null 
            ? Guid.Parse(claim) 
            : Guid.Parse("11111111-1111-1111-1111-111111111111"); // MVP fallback
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WebhookEndpointResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterWebhookDto dto)
    {
        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var endpoint = new WebhookEndpoint
        {
            SellerId = GetUserId(),
            Url = dto.Url,
            Secret = dto.Secret,
            Events = string.Join(",", dto.Events)
        };

        _context.WebhookEndpoints.Add(endpoint);
        await _context.SaveChangesAsync();

        var response = new WebhookEndpointResponseDto
        {
            Id = endpoint.Id,
            Url = endpoint.Url,
            IsActive = endpoint.IsActive,
            Events = endpoint.Events,
            CreatedAt = endpoint.CreatedAt
        };

        return Ok(ApiResponse<WebhookEndpointResponseDto>.SuccessResponse(response));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<WebhookEndpointResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEndpoints()
    {
        var endpoints = await _context.WebhookEndpoints
            .Where(e => e.SellerId == GetUserId())
            .Select(e => new WebhookEndpointResponseDto
            {
                Id = e.Id,
                Url = e.Url,
                IsActive = e.IsActive,
                Events = e.Events,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<WebhookEndpointResponseDto>>.SuccessResponse(endpoints));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var endpoint = await _context.WebhookEndpoints.FindAsync(id)
            ?? throw new WebhookEndpointNotFoundException(id);

        if (endpoint.SellerId != GetUserId())
            throw new UnauthorizedException();

        _context.WebhookEndpoints.Remove(endpoint);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.SuccessResponse("تم حذف الرابط بنجاح"));
    }

    [HttpGet("deliveries")]
    [ProducesResponseType(typeof(ApiResponse<List<WebhookDeliveryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveries()
    {
        var deliveries = await _context.WebhookDeliveries
            .Include(d => d.WebhookEndpoint)
            .Where(d => d.WebhookEndpoint.SellerId == GetUserId())
            .OrderByDescending(d => d.LastAttemptAt ?? d.DeliveredAt)
            .Take(50)
            .Select(d => new WebhookDeliveryResponseDto
            {
                Id = d.Id,
                EscrowId = d.EscrowId,
                EventName = d.EventName,
                StatusCode = d.StatusCode,
                Attempts = d.Attempts,
                IsFailed = d.IsFailed,
                LastAttemptAt = d.LastAttemptAt,
                DeliveredAt = d.DeliveredAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<WebhookDeliveryResponseDto>>.SuccessResponse(deliveries));
    }

    [HttpGet("deliveries/{id}")]
    [ProducesResponseType(typeof(ApiResponse<WebhookDeliveryDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveryDetail(Guid id)
    {
        var delivery = await _context.WebhookDeliveries
            .Include(d => d.WebhookEndpoint)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (delivery == null || delivery.WebhookEndpoint.SellerId != GetUserId())
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.InternalError, "غير موجود"));

        var response = new WebhookDeliveryDetailDto
        {
            Id = delivery.Id,
            EscrowId = delivery.EscrowId,
            EventName = delivery.EventName,
            StatusCode = delivery.StatusCode,
            Attempts = delivery.Attempts,
            IsFailed = delivery.IsFailed,
            LastAttemptAt = delivery.LastAttemptAt,
            DeliveredAt = delivery.DeliveredAt,
            Payload = delivery.Payload,
            ResponseBody = delivery.ResponseBody
        };

        return Ok(ApiResponse<WebhookDeliveryDetailDto>.SuccessResponse(response));
    }

    [HttpPost("deliveries/{id}/retry")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RetryDelivery(Guid id)
    {
        var delivery = await _context.WebhookDeliveries
            .Include(d => d.WebhookEndpoint)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (delivery == null || delivery.WebhookEndpoint.SellerId != GetUserId())
            return NotFound();

        delivery.IsFailed = true;
        delivery.Attempts = 0; // Reset so background service picks it up immediately
        delivery.LastAttemptAt = DateTime.UtcNow.AddMinutes(-10); // Force immediate retry
        
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.SuccessResponse("تم جدولة إعادة الإرسال"));
    }
}
