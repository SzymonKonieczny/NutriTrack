# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

NutriTrack is a calorie/micronutrient tracking web app with a .NET 8 backend and a React (Vite) frontend.

## Solution structure

```
NutriTrack.sln
├── NutriTrackerAPI/           # Web API host (startup project)
│   ├── Controllers/           # AuthController, Ingredients, Micronutrients, Recipes,
│   │                          #   RecipeIngredients, IngredientMicronutrients, MealLogs
│   ├── Models/DTOs/           # Request/response records per entity group
│   └── Program.cs             # Wires domain → identity → application (order matters)
│
├── NutriTrack.Application/   # Service orchestration layer
│   ├── Services/              # IAuthService → AuthService (wraps identity layer, result pattern)
│   ├── DTOs/                  # AuthResult (success/failure with response or error)
│   └── ServiceCollectionExtensions.cs
│
├── NutriTrack.Domain/        # Domain entities & EF Core data access
│   ├── Entities/              # Micronutrient, Ingredient, Recipe, MealLog, etc.
│   ├── Data/
│   │   ├── NutriTrackDbContext.cs        # schema "domain", 6 DbSets
│   │   ├── Configurations/              # EF Fluent API per entity
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── Migrations/
│   └── Enums/                 # MicronutrientUnit (Mg, Mcg, IU)
│
├── NutriTrack.Identity/      # ASP.NET Core Identity + JWT auth
│   ├── Data/                  # IdentityDbContext (schema "identity")
│   ├── Models/                # ApplicationUser (extends IdentityUser), RefreshToken
│   ├── Services/              # AuthService, TokenService (JWT issuance + refresh rotation)
│   ├── DTOs/                  # RegisterRequest, LoginRequest, RefreshTokenRequest, AuthResponse
│   ├── Configuration/         # JwtSettings, IdentityConstants
│   └── ServiceCollectionExtensions.cs
│
└── UI/                        # React (Vite + TypeScript) frontend
    └── src/                   # App.tsx, index.css, main.tsx (still boilerplate scaffold)
```

## Architecture notes

- **Clean architecture with 4 projects**: API host → Application (orchestration) → Identity (auth) + Domain (data). API controllers inject domain DbContext directly (no service layer for CRUD). The Application layer exists only for auth orchestration — it wraps the Identity-layer auth service with a result pattern so controllers get `AuthResult.Ok/Fail` instead of exceptions.

- **Two DbContexts, one SQLite database**: `NutriTrackDbContext` (schema `"domain"`) and `IdentityDbContext` (schema `"identity"`). Each has its own independent migrations. SQLite ignores schemas; they're for future SQL Server migration.

- **DI registration order is fixed** (in Program.cs): Domain → Identity → Application.

- **Auth architecture**: ASP.NET Core Identity with JWT bearer auth. Two roles: `Admin` and `User`. Refresh token rotation (each refresh issues a new token pair and revokes the old one). Soft-delete on `ApplicationUser`.

- **Authorization model**: Admin-only CRUD for micronutrients. Authenticated users can create their own recipes and ingredients (default `Private` visibility). Users see their own items + public items + system/admin-created items; admins see all. Recipe/ingredient authors can update/delete their own items. The `EntryVisibility` enum (`Private`, `Unlisted`, `Public`, `Rejected`) controls publication status. Meal logs follow the same user-scoped access pattern.

## Build & run

```bash
cd NutriTrack/NutriTrackerAPI
dotnet run
```

In development mode, the app auto-runs pending migrations on startup. Swagger is available at `/swagger` in development.

## Creating migrations

Since each module owns its own DbContext, always target the specific project with `-s NutriTrackerAPI`:

```bash
# Domain
dotnet ef migrations add MigrationName -p NutriTrack.Domain -s NutriTrackerAPI --context NutriTrackDbContext
dotnet ef database update -p NutriTrack.Domain -s NutriTrackerAPI --context NutriTrackDbContext

# Identity
dotnet ef migrations add MigrationName -p NutriTrack.Identity -s NutriTrackerAPI --context IdentityDbContext
dotnet ef database update -p NutriTrack.Identity -s NutriTrackerAPI --context IdentityDbContext
```

The `-p` flag points to the project with the DbContext (and `Migrations/` folder); `-s` points to the startup project (API).

## Domain model

| Entity | Key relationships |
|---|---|
| `Micronutrient` | Has many `IngredientMicronutrient` (via join, with `AmountPer100g`) |
| `Ingredient` | Has many `IngredientMicronutrient` and `RecipeIngredient` |
| `Recipe` | Has many `RecipeIngredient` (via join, with `AmountInGrams`) |
| `MealLog` | Optional FK to `Recipe` or `Ingredient`; `EatenByUserId` references identity user |

## UI

```bash
cd NutriTrack/UI
npm install
npm run dev     # Vite dev server
npm run build   # TypeScript check + Vite build
npm run lint    # ESLint
```

The UI is a fresh Vite + React 19 + TypeScript 6 scaffold — no app-specific code yet. Uses the React Compiler Babel plugin.

## API routes

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | Anonymous | Register new user |
| POST | `/api/auth/login` | Anonymous | Login |
| POST | `/api/auth/refresh` | Anonymous | Refresh token pair |
| POST | `/api/auth/logout` | Authenticated | Revoke all refresh tokens |
| GET/POST/PUT/DELETE | `/api/ingredients` | User/Admin | CRUD ingredients (users see own + public + system; admins see all) |
| GET/POST/PUT/DELETE | `/api/micronutrients` | Admin | CRUD micronutrients |
| GET/POST/PUT/DELETE | `/api/recipes` | User/Admin | CRUD recipes (users see own + public + system; admins see all) |
| GET/POST/PUT/DELETE | `/api/recipes/{recipeId}/ingredients` | User/Admin | Manage recipe-ingredient associations (recipe author or admin) |
| GET/POST/PUT/DELETE | `/api/ingredients/{ingredientId}/micronutrients` | User/Admin | Manage ingredient-micronutrient associations (ingredient author or admin) |
| GET/POST/PUT/DELETE | `/api/meal-logs` | User/Admin | CRUD own meal logs (admin sees all) |

## Config

Connection string and JWT settings live in `appsettings.json` (development) or user secrets. The connection string points to a local SQLite file `NutriTrack.db` at the API project root.