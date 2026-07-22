using System.Security.Claims;
using NutriTrack.Application.Abstractions;
using NutriTrack.Identity.Configuration;

namespace NutriTrackerAPI.Auth;

/// <summary>
/// Extracts the current user's identity from the HttpContext ClaimsPrincipal.
/// Requires <see cref="IHttpContextAccessor"/> to be registered in DI.
/// </summary>
internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No HttpContext available.");

    public Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token."));

    public bool IsAdmin =>
        User.IsInRole(IdentityConstants.Roles.Admin);

    public bool IsAuthenticated =>
        User.Identity?.IsAuthenticated ?? false;
}