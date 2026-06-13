using NutriTrack.Application.DTOs;

namespace NutriTrack.Application.Services;

/// <summary>
/// Computes the aggregate micronutrient content of a recipe from its ingredients.
/// </summary>
public interface IRecipeNutritionService
{
    /// <summary>
    /// Computes the total micronutrient amounts for a given recipe.
    /// Returns null if the recipe does not exist.
    /// </summary>
    Task<RecipeNutritionDto?> ComputeNutritionAsync(Guid recipeId, CancellationToken ct = default);
}