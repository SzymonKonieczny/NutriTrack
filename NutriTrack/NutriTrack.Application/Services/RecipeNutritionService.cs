using Microsoft.EntityFrameworkCore;
using NutriTrack.Application.Abstractions;
using NutriTrack.Application.DTOs;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Enums;

namespace NutriTrack.Application.Services;

/// <summary>
/// Computes recipe nutrition by loading the recipe graph and aggregating
/// ingredient micronutrient values scaled by the amount used.
/// Enforces visibility: only returns nutrition for recipes the current user
/// is allowed to see.
/// </summary>
internal sealed class RecipeNutritionService : IRecipeNutritionService
{
    private readonly NutriTrackDbContext _db;
    private readonly IUserContext _user;

    public RecipeNutritionService(NutriTrackDbContext db, IUserContext user)
    {
        _db = db;
        _user = user;
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

        // Visibility guard: non-admin users can only see their own recipes,
        // public recipes, or system recipes (no author).
        if (!_user.IsAdmin
            && recipe.AuthorId != _user.UserId
            && recipe.Visibility != EntryVisibility.Public
            && recipe.AuthorId != null)
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