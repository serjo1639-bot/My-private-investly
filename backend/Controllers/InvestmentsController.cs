using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Investments;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/investments")]
[Authorize]
public class InvestmentsController : ControllerBase
{
    private readonly IInvestmentService _investments;

    public InvestmentsController(IInvestmentService investments) => _investments = investments;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
    {
        var investorId = GetCurrentUserId();
        var result = await _investments.CheckoutAsync(investorId, req);
        return Ok(ApiResponse<IEnumerable<InvestmentResponse>>.Ok(result, "Investment submitted successfully."));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyInvestments()
    {
        var investorId = GetCurrentUserId();
        var result = await _investments.GetMyInvestmentsAsync(investorId);
        return Ok(ApiResponse<IEnumerable<InvestmentResponse>>.Ok(result));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var investorId = GetCurrentUserId();
        var result = await _investments.GetMyInvestmentsAsync(investorId);
        return Ok(ApiResponse<IEnumerable<InvestmentResponse>>.Ok(result));
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallet()
    {
        var userId = GetCurrentUserId();
        var result = await _investments.GetWalletAsync(userId);
        return Ok(ApiResponse<WalletResponse>.Ok(result));
    }

    [HttpPost("wallet/topup")]
    public async Task<IActionResult> Topup([FromBody] TopupRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _investments.TopupAsync(userId, req);
        return Ok(ApiResponse<WalletResponse>.Ok(result, "Wallet topped up successfully."));
    }

    [HttpPost("wallet/withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _investments.WithdrawAsync(userId, req);
        return Ok(ApiResponse<WalletResponse>.Ok(result, "Withdrawal successful."));
    }

    [HttpGet("funding-options")]
    public async Task<IActionResult> GetFundingOptions()
    {
        var result = await _investments.GetFundingOptionsAsync();
        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    [HttpPost("topup/redeem")]
    public async Task<IActionResult> RedeemCode([FromBody] RedeemCodeRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _investments.RedeemTopupCodeAsync(userId, req);
        return Ok(ApiResponse<WalletResponse>.Ok(result, "Code redeemed successfully."));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
