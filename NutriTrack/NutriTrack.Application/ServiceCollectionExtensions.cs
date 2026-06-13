using Microsoft.Extensions.DependencyInjection;
using NutriTrack.Application.Services;

namespace NutriTrack.Application;

/// <summary>
/// Extension methods for registering the NutriTrack Application layer services with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the NutriTrack Application-layer services, including auth orchestration.
    /// Requires that the Identity layer is already registered via <c>AddNutriTrackIdentity</c>.
    /// </summary>
    public static IServiceCollection AddNutriTrackApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRecipieProposalService, MockRecipieProposalService>();
        services.AddScoped<IRecipeNutritionService, RecipeNutritionService>();
        services.AddScoped<IDeficiencyAnalysisService, DeficiencyAnalysisService>();
        return services;
    }
}