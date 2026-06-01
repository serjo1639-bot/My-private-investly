using System.Security.Claims;
using Investly.API.Helpers;
using Investly.API.Models.DTOs.Payments;
using Investly.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments) => _payments = payments;

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _payments.InitiateAsync(userId, req);
        return StatusCode(201, ApiResponse<PaymentResponse>.Ok(result));
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest req)
    {
        var userId = GetCurrentUserId();
        var result = await _payments.VerifyAsync(userId, req);
        return Ok(ApiResponse<PaymentResponse>.Ok(result));
    }

    [HttpGet("methods")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMethods()
    {
        var result = await _payments.GetMethodsAsync();
        return Ok(ApiResponse<IEnumerable<PaymentMethodResponse>>.Ok(result));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetCurrentUserId();
        var result = await _payments.GetHistoryAsync(userId);
        return Ok(ApiResponse<IEnumerable<PaymentResponse>>.Ok(result));
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> GetWalletInfo()
    {
        var userId = GetCurrentUserId();
        // Delegate to investment service for wallet info
        return Ok(ApiResponse<object>.Ok(new { userId }));
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<IActionResult> GetById(Guid paymentId)
    {
        var requesterId = GetCurrentUserId();
        var result = await _payments.GetByIdAsync(paymentId, requesterId);
        return Ok(ApiResponse<PaymentResponse>.Ok(result));
    }

    [HttpGet("{paymentId:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid paymentId)
    {
        var requesterId = GetCurrentUserId();
        var result = await _payments.GetByIdAsync(paymentId, requesterId);
        return Ok(ApiResponse<object>.Ok(new { result.Status, result.TransactionId }));
    }

    [HttpPost("{paymentId:guid}/refund")]
    public async Task<IActionResult> Refund(Guid paymentId)
    {
        var requesterId = GetCurrentUserId();
        var result = await _payments.RefundAsync(paymentId, requesterId);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, "Refund processed."));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}
