namespace NutriTrack.Application.Abstractions;

/// <summary>
/// Provides access to the current authenticated user's identity context.
/// Implemented in the API host using IHttpContextAccessor to extract
/// the user ID and roles from the ClaimsPrincipal.
/// </summary>
public interface IUserContext
{
    /// <summary>The authenticated user's ID (from ClaimTypes.NameIdentifier).</summary>
    Guid UserId { get; }

    /// <summary>True if the current user has the Admin role.</summary>
    bool IsAdmin { get; }

    /// <summary>True if the request is authenticated.</summary>
    bool IsAuthenticated { get; }
}