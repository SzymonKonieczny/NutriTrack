using Microsoft.AspNetCore.Identity;

namespace NutriTrack.Identity.Models;

/// <summary>
/// Custom application user extending the ASP.NET Core IdentityUser.
/// Add additional user-specific properties here to keep the identity layer extensible.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>User's name (optional).</summary>
    public string? Name { get; set; }

    /// <summary>Timestamp when the account was created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp of last profile update.</summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>Soft-delete flag; when true the account is deactivated, not removed.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Timestamp of soft-delete, if applicable.</summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>Refresh tokens owned by this user.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}