using Arboon.Application.Common;
using Arboon.Application.DTOs.Dispute;
using Arboon.Application.Interfaces;
using Arboon.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arboon.API.Controllers;

[ApiController]
[Route("api/v1/admin/disputes")]
// [Authorize(Roles = "Admin")] // Uncomment for real auth
public class AdminDisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;
    private readonly IValidator<ResolveDisputeDto> _resolveValidator;

    public AdminDisputesController(
        IDisputeService disputeService,
        IValidator<ResolveDisputeDto> resolveValidator)
    {
        _disputeService = disputeService;
        _resolveValidator = resolveValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DisputeResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOpen()
    {
        var result = await _disputeService.GetAllOpenAsync();
        return Ok(ApiResponse<List<DisputeResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DisputeDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _disputeService.GetByIdAsync(id);
        return Ok(ApiResponse<DisputeDetailDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<DisputeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveDisputeDto dto)
    {
        var validationResult = await _resolveValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var result = await _disputeService.ResolveAsync(id, dto);
        return Ok(ApiResponse<DisputeResponseDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/messages")]
    [ProducesResponseType(typeof(ApiResponse<DisputeMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddDisputeMessageDto dto)
    {
        var result = await _disputeService.AddMessageAsync(id, dto, DisputeParty.Admin);
        return Ok(ApiResponse<DisputeMessageDto>.SuccessResponse(result));
    }
}
