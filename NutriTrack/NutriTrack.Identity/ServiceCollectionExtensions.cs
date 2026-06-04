using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NutriTrack.Identity.Configuration;
using NutriTrack.Identity.Data;
using NutriTrack.Identity.Models;
using NutriTrack.Identity.Services;

namespace NutriTrack.Identity;

/// <summary>
/// Extension methods for registering NutriTrack Identity services with DI.
/// Each identity component registers its own <see cref="IdentityDbContext"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the NutriTrack Identity library to the service collection.
    /// Registers its <see cref="IdentityDbContext"/> with SQLite, ASP.NET Core Identity
    /// (with identity rules), JWT config binding, JWT bearer authentication, authorization policies,
    /// and scoped application services.
    /// </summary>
    public static IServiceCollection AddNutriTrackIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        // Register Identity's own DbContext
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlite(connectionString));

        // Bind JWT settings from appsettings.json
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);

        // ASP.NET Core Identity — use AddIdentity (includes AddDefaultTokenProviders)
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Sensible defaults — customise as needed
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        // JWT bearer authentication
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration section not found.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero,
            };
        });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Configuration.IdentityConstants.Policies.RequireAdmin,
                policy => policy.RequireRole(Configuration.IdentityConstants.Roles.Admin));
            options.AddPolicy(Configuration.IdentityConstants.Policies.RequireAuthenticatedUser,
                policy => policy.RequireAuthenticatedUser());
        });

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}