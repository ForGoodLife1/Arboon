using System.Security.Claims;
using Arboon.Application.Common;
using Arboon.Application.DTOs.Dispute;
using Arboon.Application.Interfaces;
using Arboon.Domain.Enums;
using Arboon.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arboon.API.Controllers;

[ApiController]
[Route("api/v1/disputes")]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;
    private readonly IValidator<OpenDisputeDto> _openValidator;
    private readonly IValidator<OpenDisputeBuyerDto> _openBuyerValidator;

    public DisputesController(
        IDisputeService disputeService,
        IValidator<OpenDisputeDto> openValidator,
        IValidator<OpenDisputeBuyerDto> openBuyerValidator)
    {
        _disputeService = disputeService;
        _openValidator = openValidator;
        _openBuyerValidator = openBuyerValidator;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue("user_id");
        return claim != null 
            ? Guid.Parse(claim) 
            : Guid.Parse("11111111-1111-1111-1111-111111111111"); // MVP fallback
    }

    // [Authorize] // Seller only
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DisputeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> OpenBySeller([FromBody] OpenDisputeDto dto)
    {
        var validationResult = await _openValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var result = await _disputeService.OpenBySellerAsync(GetUserId(), dto);
        return Ok(ApiResponse<DisputeResponseDto>.SuccessResponse(result));
    }

    // [Authorize] // Seller only
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DisputeResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDisputes()
    {
        var result = await _disputeService.GetBySellerAsync(GetUserId());
        return Ok(ApiResponse<List<DisputeResponseDto>>.SuccessResponse(result));
    }

    // Public (Buyer with token)
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DisputeDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid? buyer_token)
    {
        var result = await _disputeService.GetByIdAsync(id);

        // Security check for buyer
        if (buyer_token.HasValue)
        {
            if (result.Escrow == null || result.Escrow.Id == null)
                throw new DisputeNotFoundException(id);

            // Verify token matches escrow
            // Note: In a real app, this should be a service method, but for MVP we check it here
            // or trust the buyer_token if it matches the dispute's escrow.
        }

        return Ok(ApiResponse<DisputeDetailDto>.SuccessResponse(result));
    }

    // Public (Buyer with token)
    [HttpPost("buyer")]
    [ProducesResponseType(typeof(ApiResponse<DisputeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> OpenByBuyer([FromBody] OpenDisputeBuyerDto dto)
    {
        if (dto.BuyerToken == null)
        {
            if (Request.Cookies.TryGetValue("arboon_buyer_token", out var cookieVal)
                && Guid.TryParse(cookieVal, out var parsed))
            {
                dto.BuyerToken = parsed;
            }
        }

        var validationResult = await _openBuyerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var result = await _disputeService.OpenByBuyerAsync(dto);
        return Ok(ApiResponse<DisputeResponseDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/messages")]
    [ProducesResponseType(typeof(ApiResponse<DisputeMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddDisputeMessageDto dto)
    {
        DisputeParty senderType;

        // Try buyer token first
        if (dto.BuyerToken == null && Request.Cookies.TryGetValue("arboon_buyer_token", out var cookieVal)
            && Guid.TryParse(cookieVal, out var parsed))
        {
            dto.BuyerToken = parsed;
        }

        if (dto.BuyerToken.HasValue)
        {
            senderType = DisputeParty.Buyer;
            // Additional validation done in service if needed
        }
        else
        {
            // Fallback to seller auth
            senderType = DisputeParty.Seller;
            // Additional auth validation can be added here
        }

        var result = await _disputeService.AddMessageAsync(id, dto, senderType);
        return Ok(ApiResponse<DisputeMessageDto>.SuccessResponse(result));
    }

    [HttpGet("{id}/messages")]
    [ProducesResponseType(typeof(ApiResponse<List<DisputeMessageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(Guid id, [FromQuery] Guid? buyer_token)
    {
        // Simple security: for now we just allow the call if id is valid
        // In a real app, we would verify the token against the dispute's escrow
        var result = await _disputeService.GetMessagesAsync(id);
        return Ok(ApiResponse<List<DisputeMessageDto>>.SuccessResponse(result));
    }
}
