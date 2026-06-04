using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NutriTrack.Domain.Data;

/// <summary>
/// Extension methods for registering the NutriTrack domain DbContext with DI.
/// Each data module registers its own <see cref="NutriTrackDbContext"/> independently.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="NutriTrackDbContext"/> to the service collection
    /// using SQLite with the given connection string.
    /// </summary>
    public static IServiceCollection AddNutriTrackDomain(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<NutriTrackDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}