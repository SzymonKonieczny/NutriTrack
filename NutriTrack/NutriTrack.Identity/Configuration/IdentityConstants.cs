namespace NutriTrack.Identity.Configuration;

/// <summary>
/// Centralised constants for roles, policies, and claim types.
/// </summary>
public static class IdentityConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
    }
}