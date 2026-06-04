# NutriTrack — Architecture & Project Summary

## Current standing (2026-06-06)

The solution is fully building and running with a clean layered architecture. Authentication endpoints (register, login, refresh, logout) are wired and working against a SQLite database containing two independently-migrated contexts.

---

## Solution structure

```
NutriTrack.sln
├── NutriTrackerAPI/           # Web API host (startup project)
│   ├── Controllers/           # AuthController (register, login, refresh, logout)
│   └── Program.cs             # wires up all modules
│
├── NutriTrack.Application/   # Service orchestration layer
│   ├── Services/              # IAuthService → AuthService (wraps Identity layer)
│   ├── DTOs/                  # AuthResult (Result pattern)
│   └── ServiceCollectionExtensions.cs   # AddNutriTrackApplication()
│
├── NutriTrack.Domain/        # Domain entities & data access
│   ├── Entities/              # Micronutrient, Ingredient, Recipe, MealLog, etc.
│   ├── Data/
│   │   ├── NutriTrackDbContext.cs     # schema "domain"
│   │   ├── Configurations/           # EF Core Fluent API configs
│   │   ├── ServiceCollectionExtensions.cs  # AddNutriTrackDomain(connStr)
│   │   └── Migrations/               # independent migrations
│   └── Enums/
│
├── NutriTrack.Identity/      # Authentication & user management
│   ├── Data/
│   │   ├── IdentityDbContext.cs      # schema "identity"
│   │   └── Migrations/               # independent migrations
│   ├── Models/                # ApplicationUser, RefreshToken
│   ├── Services/              # AuthService, TokenService
│   ├── DTOs/                  # Requests & Responses
│   ├── Configuration/         # JwtSettings, IdentityConstants
│   └── ServiceCollectionExtensions.cs  # AddNutriTrackIdentity(config, connStr)
```

## Database architecture

| Module | DbContext | Schema | Migrations dir |
|---|---|---|---|
| Domain | `NutriTrackDbContext` | `"domain"` | `NutriTrack.Domain/Data/Migrations/` |
| Identity | `IdentityDbContext` | `"identity"` | `NutriTrack.Identity/Data/Migrations/` |

Both contexts share **one physical SQLite database** (`NutriTrack.db`). Schema annotations (`HasDefaultSchema`) are **planned for SQL Server** — SQLite ignores them silently. When you switch to SQL Server, tables are placed in their respective schemas automatically.

### Dependency injection order

In `Program.cs`, module extensions must be registered in this order:

```csharp
builder.Services.AddNutriTrackDomain(connectionString);       // 1. Domain DbContext
builder.Services.AddNutriTrackIdentity(builder.Configuration, connectionString);  // 2. Identity DbContext
builder.Services.AddNutriTrackApplication();                   // 3. Application services
```

## Creating migrations

Since each module owns its own DbContext and migrations, always target the specific project and context with `-s NutriTrackerAPI` as the startup project:

### Domain module

```bash
dotnet ef migrations add MigrationName -p NutriTrack.Domain -s NutriTrackerAPI --context NutriTrackDbContext
dotnet ef database update -p NutriTrack.Domain -s NutriTrackerAPI --context NutriTrackDbContext
```

### Identity module

```bash
dotnet ef migrations add MigrationName -p NutriTrack.Identity -s NutriTrackerAPI --context IdentityDbContext
dotnet ef database update -p NutriTrack.Identity -s NutriTrackerAPI --context IdentityDbContext
```

> `-p` = the project with the DbContext (and the `Migrations/` folder), `-s` = the startup project (API) that has the connection string and `AddDbContext` call chain.

## Running the app

```bash
cd NutriTrackerAPI
dotnet run
```

In development mode, the app automatically runs pending migrations on startup via `Database.Migrate()`. No manual step needed.
