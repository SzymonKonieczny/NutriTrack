using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Identity.Data;
using NutriTrack.Identity.DTOs;
using NutriTrack.Identity.Models;

namespace NutriTrack.Identity.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IdentityDbContext _db;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        IdentityDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _db = db;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name,
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new InvalidOperationException(
                $"User registration failed: {string.Join("; ", errors)}");
        }

        // Ensure default role exists and assign it
        const string defaultRole = "User";
        if (!await _roleManager.RoleExistsAsync(defaultRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(defaultRole));
        }
        await _userManager.AddToRoleAsync(user, defaultRole);

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        await StoreRefreshTokenAsync(user.Id, refreshTokenValue);

        return new AuthResponse(
            UserId: user.Id,
            Email: user.Email!,
            Name: user.Name,
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            AccessTokenExpiresAtUtc: expiresAt
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.IsDeleted)
        {
            throw new UnauthorizedAccessException("This account has been deactivated.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        await StoreRefreshTokenAsync(user.Id, refreshTokenValue);

        return new AuthResponse(
            UserId: user.Id,
            Email: user.Email!,
            Name: user.Name,
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            AccessTokenExpiresAtUtc: expiresAt
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Invalid token payload.");
        }

        var isValid = await _tokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
        {
            throw new UnauthorizedAccessException("User not found or deactivated.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        await StoreRefreshTokenAsync(user.Id, newRefreshToken);

        return new AuthResponse(
            UserId: user.Id,
            Email: user.Email!,
            Name: user.Name,
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            AccessTokenExpiresAtUtc: expiresAt
        );
    }

    public async Task LogoutAsync(string userId)
    {
        var tokens = _db.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null);
        await tokens.ForEachAsync(rt => rt.RevokedAtUtc = DateTime.UtcNow);
        await _db.SaveChangesAsync();
    }

    private async Task StoreRefreshTokenAsync(string userId, string refreshTokenValue)
    {
        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
        });

        await _db.SaveChangesAsync();
    }
}