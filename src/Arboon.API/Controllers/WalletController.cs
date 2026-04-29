using System.Security.Claims;
using Arboon.Application.Common;
using Arboon.Application.DTOs.Wallet;
using Arboon.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arboon.API.Controllers;

[ApiController]
[Route("api/v1/wallet")]
//[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue("user_id");
        return claim != null 
            ? Guid.Parse(claim) 
            : Guid.Parse("11111111-1111-1111-1111-111111111111"); // MVP fallback
    }

    /// <summary>
    /// Get the authenticated seller's wallet balance.
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(ApiResponse<WalletBalanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance()
    {
        var result = await _walletService.GetBalanceAsync(GetUserId());
        return Ok(ApiResponse<WalletBalanceDto>.SuccessResponse(result));
    }
}
