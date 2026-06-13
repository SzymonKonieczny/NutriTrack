using Microsoft.EntityFrameworkCore;
using NutriTrack.Application.DTOs;
using NutriTrack.Domain.Data;

namespace NutriTrack.Application.Services;

/// <summary>
/// Computes recipe nutrition by loading the recipe graph and aggregating
/// ingredient micronutrient values scaled by the amount used.
/// </summary>
internal sealed class RecipeNutritionService : IRecipeNutritionService
{
    private readonly NutriTrackDbContext _db;

    public RecipeNutritionService(NutriTrackDbContext db)
    {
        _db = db;
    }

    public async Task<RecipeNutritionDto?> ComputeNutritionAsync(Guid recipeId, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                    .ThenInclude(i => i.IngredientMicronutrients)
                        .ThenInclude(im => im.Micronutrient)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct);

        if (recipe is null)
            return null;

        // For each recipe ingredient, scale the per-100g micronutrient values by the actual gram amount.
        var micronutrients = recipe.RecipeIngredients
            .SelectMany(ri => ri.Ingredient.IngredientMicronutrients
                .Select(im => new
                {
                    im.MicronutrientId,
                    im.Micronutrient.Name,
                    im.Micronutrient.Unit,
                    Amount = im.AmountPer100g * ri.AmountInGrams / 100m
                }))
            .GroupBy(x => new { x.MicronutrientId, x.Name, x.Unit })
            .Select(g => new MicronutrientTotalDto(
                g.Key.MicronutrientId,
                g.Key.Name,
                Math.Round(g.Sum(x => x.Amount), 2),
                g.Key.Unit.ToString()))
            .OrderBy(m => m.MicronutrientName)
            .ToList();

        return new RecipeNutritionDto(recipe.Id, recipe.Name, micronutrients);
    }
}