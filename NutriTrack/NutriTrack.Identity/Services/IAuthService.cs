using NutriTrack.Identity.DTOs;

namespace NutriTrack.Identity.Services;

public interface IAuthService
{
    /// <summary>Register a new user account.</summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>Authenticate a user and return tokens.</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>Exchange an expired access token + valid refresh token for a new token pair.</summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>Revoke all refresh tokens for a user (logout everywhere).</summary>
    Task LogoutAsync(string userId);
}