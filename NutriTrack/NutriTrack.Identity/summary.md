# NutriTrack Identity

Boilerplate ASP.NET Core Identity library using .NET's built-in Identity framework with JWT authentication and refresh token rotation.

## Structure

```
NutriTrack.Identity/
├── NutriTrack.Identity.csproj      # targets net8.0, EF Core + Identity + JWT packages
├── ServiceCollectionExtensions.cs  # DI extension: AddNutriTrackIdentity(…)
├── Configuration/
│   ├── JwtSettings.cs              # JWT options POCO (Issuer, Audience, SecretKey, expirations)
│   └── IdentityConstants.cs        # Role & policy string constants
├── Data/
│   └── IdentityDbContext.cs        # IdentityDbContext<ApplicationUser> + RefreshTokens DbSet, schema "identity"
├── DTOs/
│   ├── Requests.cs                 # RegisterRequest, LoginRequest, RefreshTokenRequest
│   └── Responses.cs                # AuthResponse, ErrorResponse
├── Models/
│   ├── ApplicationUser.cs          # extends IdentityUser (Name, soft-delete, refresh tokens)
│   └── RefreshToken.cs             # token value, expiry, revocation, navigation to user
└── Services/
    ├── IAuthService.cs             # Register / Login / RefreshToken / Logout contract
    ├── AuthService.cs              # full implementation with Identity + refresh rotation
    ├── ITokenService.cs            # JWT generation, refresh token management contract
    └── TokenService.cs             # JWT + crypto-random refresh tokens, validation
```

## Key design decisions

- **`AddIdentity<ApplicationUser, IdentityRole>`** — gives you the full cookie + token pipeline and default token providers (email confirmation, password reset) out of the box
- **Refresh token rotation** — every refresh invalidates the old token and issues a new one
- **Soft-delete** on `ApplicationUser.IsDeleted` — accounts can be deactivated without data loss
- **Self-registering DbContext** — `AddNutriTrackIdentity(…)` registers its own `IdentityDbContext` with `HasDefaultSchema("identity")`. The host project does not register it directly.
- **Extensible** — add custom properties to `ApplicationUser`, swap in `ApplicationRole` (just change the generic), or add more services without touching existing files

## Dependencies

- Each module registers its own DbContext — the host project calls all module extensions in `Program.cs`
- `AddNutriTrackDomain(connectionString)` must be called **before** `AddNutriTrackIdentity(config, connectionString)` if there are cross-module FK references (e.g. MealLog → AspNetUsers)

## Quick start

```csharp
// In Program.cs:
builder.Services.AddNutriTrackDomain(connectionString);
builder.Services.AddNutriTrackIdentity(builder.Configuration, connectionString);
builder.Services.AddNutriTrackApplication();
```

Add the following to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NutriTrack.db"
  },
  "JwtSettings": {
    "Issuer": "NutriTrack",
    "Audience": "NutriTrack",
    "SecretKey": "your-256-bit-secret-key-here-at-least-32-characters",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 1
  }
}
```

## Creating migrations

Since EF Core scans the **startup project** for DbContext registrations, always run migrations with `-s NutriTrackerAPI`:

```bash
dotnet ef migrations add MigrationName -p NutriTrack.Identity -s NutriTrackerAPI --context IdentityDbContext
dotnet ef database update -p NutriTrack.Identity -s NutriTrackerAPI --context IdentityDbContext
```

**Schema:** All Identity tables are configured with `HasDefaultSchema("identity")`. SQLite ignores this (expected); on SQL Server they are placed under the `identity` schema automatically.
