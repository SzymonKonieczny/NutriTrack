using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriTrack.Application.DTOs;
using NutriTrack.Application.Services;
using NutriTrack.Identity.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// Authentication endpoints — register, login, refresh tokens, and logout.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new user account.</summary>
    /// <response code="200">Account created successfully with tokens.</response>
    /// <response code="400">Validation failure or user already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);

        if (result.IsFailure)
            return BadRequest(new ErrorResponse(result.ErrorMessage!, result.Errors));

        return Ok(result.Response);
    }

    /// <summary>Authenticate a user and return JWT + refresh token.</summary>
    /// <response code="200">Login successful with tokens.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);

        if (result.IsFailure)
            return Unauthorized(new ErrorResponse(result.ErrorMessage!));

        return Ok(result.Response);
    }

    /// <summary>Exchange an expired access token + valid refresh token for a new pair.</summary>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="401">Invalid or expired tokens.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request, ct);

        if (result.IsFailure)
            return Unauthorized(new ErrorResponse(result.ErrorMessage!));

        return Ok(result.Response);
    }

    /// <summary>Revoke all refresh tokens for the current user (logout everywhere).</summary>
    /// <response code="204">Logged out successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new ErrorResponse("User identity not found in token."));

        await _authService.LogoutAsync(userId, ct);
        return NoContent();
    }
}