using NutriTrack.Application.DTOs;

namespace NutriTrack.Application.Services;

/// <summary>
/// Application-layer service for user registration, authentication, token refresh, and logout.
/// Wraps the Identity-layer auth service with a result pattern suitable for API controllers.
/// </summary>
public interface IAuthService
{
    /// <summary>Register a new user account.</summary>
    Task<AuthResult> RegisterAsync(NutriTrack.Identity.DTOs.RegisterRequest request, CancellationToken ct = default);

    /// <summary>Authenticate a user and return tokens.</summary>
    Task<AuthResult> LoginAsync(NutriTrack.Identity.DTOs.LoginRequest request, CancellationToken ct = default);

    /// <summary>Exchange an expired access token + valid refresh token for a new token pair.</summary>
    Task<AuthResult> RefreshTokenAsync(NutriTrack.Identity.DTOs.RefreshTokenRequest request, CancellationToken ct = default);

    /// <summary>Revoke all refresh tokens for a user (logout everywhere).</summary>
    Task LogoutAsync(string userId, CancellationToken ct = default);
}