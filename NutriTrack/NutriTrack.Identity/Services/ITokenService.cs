using System.Security.Claims;
using NutriTrack.Identity.Models;

namespace NutriTrack.Identity.Services;

public interface ITokenService
{
    /// <summary>Generate a signed JWT access token for the given user.</summary>
    (string token, DateTime expiresAtUtc) GenerateAccessToken(ApplicationUser user, IList<string> roles);

    /// <summary>Generate a cryptographically random refresh token.</summary>
    string GenerateRefreshToken();

    /// <summary>Extract the principal from an expired (or valid) token — used for refresh flows.</summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>Validate a refresh token exists and is active for the given user.</summary>
    Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);

    /// <summary>Revoke a specific refresh token.</summary>
    Task RevokeRefreshTokenAsync(string userId, string refreshToken);
}