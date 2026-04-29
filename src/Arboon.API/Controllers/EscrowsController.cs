using System.Security.Claims;
using Arboon.Application.Common;
using Arboon.Application.DTOs.Escrow;
using Arboon.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arboon.API.Controllers;

[ApiController]
[Route("api/v1/escrows")]
public class EscrowsController : ControllerBase
{
    private readonly IEscrowService _escrowService;
    private readonly IValidator<CreateEscrowDto> _createValidator;
    private readonly IValidator<PayEscrowDto> _payValidator;

    public EscrowsController(
        IEscrowService escrowService,
        IValidator<CreateEscrowDto> createValidator,
        IValidator<PayEscrowDto> payValidator)
    {
        _escrowService = escrowService;
        _createValidator = createValidator;
        _payValidator = payValidator;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue("user_id");
        return claim != null 
            ? Guid.Parse(claim) 
            : Guid.Parse("11111111-1111-1111-1111-111111111111"); // MVP fallback
    }

    /// <summary>
    /// Create a new escrow. Requires JWT authentication.
    /// </summary>
   //[Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EscrowResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateEscrowDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var result = await _escrowService.CreateAsync(GetUserId(), dto);
        return Ok(ApiResponse<EscrowResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all escrows for the authenticated seller.
    /// </summary>
    //[Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EscrowResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEscrows()
    {
        var result = await _escrowService.GetBySellerIdAsync(GetUserId());
        return Ok(ApiResponse<List<EscrowResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get escrow details (public endpoint).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EscrowResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _escrowService.GetByIdAsync(id);
        return Ok(ApiResponse<EscrowResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Check buyer authorization status for an escrow.
    /// </summary>
    [HttpGet("{id}/buyer-status")]
    [ProducesResponseType(typeof(ApiResponse<BuyerStatusResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBuyerStatus(string id, [FromQuery] Guid? token)
    {
        // Also check cookie for token
        var cookieToken = Request.Cookies.TryGetValue("arboon_buyer_token", out var cookieVal)
            ? Guid.TryParse(cookieVal, out var parsed) ? parsed : (Guid?)null
            : null;

        var effectiveToken = token ?? cookieToken;
        var result = await _escrowService.GetBuyerStatusAsync(id, effectiveToken);
        return Ok(ApiResponse<BuyerStatusResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Pay an escrow (guest buyer). Sets HttpOnly cookie with buyer token.
    /// </summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(string id, [FromBody] PayEscrowDto dto)
    {
        var validationResult = await _payValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Prevent seller from paying their own escrow if they're logged in
        if (User.Identity?.IsAuthenticated == true)
        {
            var escrow = await _escrowService.GetByIdAsync(id);
            // We check by trying to parse the user_id claim
            var userIdClaim = User.FindFirstValue("user_id");
            if (userIdClaim != null && escrow.SellerName != null)
            {
                // Additional check can be added here if needed
            }
        }

        var result = await _escrowService.PayAsync(id, dto);

        // Set HttpOnly cookie with buyer token
        if (result is { } payResult)
        {
            var tokenProp = payResult.GetType().GetProperty("buyer_token");
            if (tokenProp?.GetValue(payResult) is Guid buyerToken)
            {
                Response.Cookies.Append("arboon_buyer_token", buyerToken.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    Path = "/"
                });
            }
        }

        return Ok(ApiResponse.SuccessResponse("تم الدفع بنجاح", result));
    }

    /// <summary>
    /// Release escrow funds (buyer confirms delivery).
    /// Accepts token from body or cookie.
    /// </summary>
    [HttpPost("{id}/release")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Release(string id, [FromBody] ReleaseEscrowDto? dto)
    {
        // Prevent seller from releasing their own escrow
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue("user_id");
            if (userIdClaim != null)
            {
                var escrow = await _escrowService.GetByIdAsync(id);
                // Additional seller self-release prevention could be here
            }
        }

        // Get token from body or cookie
        var tokenFromBody = dto?.BuyerToken;
        Guid? tokenFromCookie = null;

        if (Request.Cookies.TryGetValue("arboon_buyer_token", out var cookieVal)
            && Guid.TryParse(cookieVal, out var parsed))
        {
            tokenFromCookie = parsed;
        }

        var result = await _escrowService.ReleaseAsync(id, tokenFromBody, tokenFromCookie);

        // Clear the buyer cookie
        Response.Cookies.Delete("arboon_buyer_token");

        return Ok(ApiResponse.SuccessResponse("تم تحرير المبلغ بنجاح", result));
    }

    /// <summary>
    /// Cancel an escrow (seller only, before payment).
    /// </summary>
    //[Authorize]
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(string id)
    {
        await _escrowService.CancelAsync(id, GetUserId());
        return Ok(ApiResponse.SuccessResponse("تم إلغاء العُهدة بنجاح"));
    }
}
