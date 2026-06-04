using NutriTrack.Application.DTOs;

namespace NutriTrack.Application.Services;

/// <summary>
/// Application-layer auth service that delegates to the Identity-layer <see cref="NutriTrack.Identity.Services.IAuthService"/>
/// and translates its exception-based error handling into a result pattern.
/// </summary>
internal sealed class AuthService : IAuthService
{
    private readonly NutriTrack.Identity.Services.IAuthService _identityAuth;

    public AuthService(NutriTrack.Identity.Services.IAuthService identityAuth)
    {
        _identityAuth = identityAuth;
    }

    public async Task<AuthResult> RegisterAsync(
        NutriTrack.Identity.DTOs.RegisterRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _identityAuth.RegisterAsync(request);
            return AuthResult.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }

    public async Task<AuthResult> LoginAsync(
        NutriTrack.Identity.DTOs.LoginRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _identityAuth.LoginAsync(request);
            return AuthResult.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(
        NutriTrack.Identity.DTOs.RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _identityAuth.RefreshTokenAsync(request);
            return AuthResult.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }

    public async Task LogoutAsync(string userId, CancellationToken ct = default)
    {
        await _identityAuth.LogoutAsync(userId);
    }
}