using Microsoft.EntityFrameworkCore;
using NutriTrack.Application.DTOs;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;

namespace NutriTrack.Application.Services;

/// <summary>
/// Analyzes a user's meal log history for micronutrient deficiencies
/// and suggests recipes that best address the top deficits.
/// </summary>
internal sealed class DeficiencyAnalysisService : IDeficiencyAnalysisService
{
    private const int DefaultLookbackDays = 7;
    private const int TopDeficitsToTarget = 5;
    private const int TopRecipesToSuggest = 5;

    private readonly NutriTrackDbContext _db;
    private readonly IRecipeNutritionService _recipeNutritionService;

    public DeficiencyAnalysisService(NutriTrackDbContext db, IRecipeNutritionService recipeNutritionService)
    {
        _db = db;
        _recipeNutritionService = recipeNutritionService;
    }

    public async Task<DeficiencyAnalysisResultDto> AnalyzeAndSuggestAsync(string userId, CancellationToken ct = default)
    {
        // -----------------------------------------------------------------------
        // 1. Load the user's meal logs within the lookback window
        // -----------------------------------------------------------------------
        var cutoffDate = DateTime.UtcNow.AddDays(-DefaultLookbackDays);

        // SQLite-friendly: pull records for the user, then filter by date in-memory
        var userMealLogs = _db.MealLogs
            .Include(ml => ml.Recipe)
                .ThenInclude(r => r!.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                        .ThenInclude(i => i.IngredientMicronutrients)
                            .ThenInclude(im => im.Micronutrient)
            .Include(ml => ml.Ingredient)
                .ThenInclude(i => i!.IngredientMicronutrients)
                    .ThenInclude(im => im.Micronutrient)
            .Where(ml => ml.EatenByUserId == userId);

        List<MealLog> mealLogs;
        if (_db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            mealLogs = await userMealLogs.ToListAsync(ct);
            mealLogs = mealLogs.Where(ml => ml.EatenAt >= cutoffDate).ToList();
        }
        else
        {
            mealLogs = await userMealLogs.Where(ml => ml.EatenAt >= cutoffDate).ToListAsync(ct);
        }

        if (mealLogs.Count == 0)
        {
            return new DeficiencyAnalysisResultDto(
                DefaultLookbackDays,
                0,
                new List<MicronutrientDeficitDto>(),
                new List<DeficitRecipeSuggestionDto>());
        }

        // -----------------------------------------------------------------------
        // 2. Sum micronutrient consumption across all meal logs
        //    Key:   (micronutrientId, name, unit)
        //    Value: total amount consumed
        // -----------------------------------------------------------------------
        var consumedTotals = new Dictionary<(Guid Id, string Name, string Unit), decimal>();

        foreach (var mealLog in mealLogs)
        {
            if (mealLog.RecipeId is not null && mealLog.Recipe is not null)
            {
                var servings = mealLog.Servings ?? 1m;
                foreach (var ri in mealLog.Recipe.RecipeIngredients)
                {
                    foreach (var im in ri.Ingredient.IngredientMicronutrients)
                    {
                        var amount = im.AmountPer100g * ri.AmountInGrams / 100m * servings;
                        var key = (im.MicronutrientId, im.Micronutrient.Name, im.Micronutrient.Unit.ToString());
                        consumedTotals[key] = consumedTotals.GetValueOrDefault(key) + amount;
                    }
                }
            }
            else if (mealLog.IngredientId is not null && mealLog.Ingredient is not null)
            {
                var grams = mealLog.AmountInGrams ?? 100m;
                foreach (var im in mealLog.Ingredient.IngredientMicronutrients)
                {
                    var amount = im.AmountPer100g * grams / 100m;
                    var key = (im.MicronutrientId, im.Micronutrient.Name, im.Micronutrient.Unit.ToString());
                    consumedTotals[key] = consumedTotals.GetValueOrDefault(key) + amount;
                }
            }
        }

        // -----------------------------------------------------------------------
        // 3. Load all micronutrients with their daily reference amounts
        // -----------------------------------------------------------------------
        var allMicronutrients = await _db.Micronutrients.ToListAsync(ct);
        var referenceLookup = allMicronutrients.ToDictionary(m => m.Id);

        // -----------------------------------------------------------------------
        // 4. Compute deficit percentage per micronutrient and rank
        //    Deficit % = max(0, (RDA - avgDaily) / RDA * 100)
        // -----------------------------------------------------------------------
        var daysInRange = Math.Max(1, (int)(DateTime.UtcNow - cutoffDate).TotalDays);
        var deficits = new List<MicronutrientDeficitDto>();

        foreach (var ((id, name, unit), total) in consumedTotals)
        {
            if (!referenceLookup.TryGetValue(id, out var micronutrient))
                continue;

            var rda = micronutrient.DailyReferenceAmount;
            if (rda <= 0)
                continue;

            var avgDaily = Math.Round(total / daysInRange, 4);
            var deficitPct = Math.Max(0, (double)((rda - avgDaily) / rda * 100));

            deficits.Add(new MicronutrientDeficitDto(
                id,
                name,
                rda,
                avgDaily,
                Math.Round((decimal)deficitPct, 1),
                unit));
        }

        var topDeficits = deficits
            .OrderByDescending(d => d.DeficitPercentage)
            .ThenBy(d => d.MicronutrientName)
            .Take(TopDeficitsToTarget)
            .ToList();

        if (topDeficits.Count == 0)
        {
            return new DeficiencyAnalysisResultDto(
                DefaultLookbackDays,
                mealLogs.Select(ml => ml.EatenAt.Date).Distinct().Count(),
                new List<MicronutrientDeficitDto>(),
                new List<DeficitRecipeSuggestionDto>());
        }

        // -----------------------------------------------------------------------
        // 5. Find recipes richest in the deficient micronutrients
        //    Load all recipes with their full ingredient → micronutrient graph.
        // -----------------------------------------------------------------------
        var targetMicronutrientIds = topDeficits.Select(d => d.MicronutrientId).ToHashSet();

        var recipes = await _db.Recipes
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                    .ThenInclude(i => i.IngredientMicronutrients)
                        .ThenInclude(im => im.Micronutrient)
            .Where(r => r.RecipeIngredients
                .Any(ri => ri.Ingredient.IngredientMicronutrients
                    .Any(im => targetMicronutrientIds.Contains(im.MicronutrientId))))
            .ToListAsync(ct);

        // Score each recipe: for each deficient micronutrient, what % of RDA
        // does one serving of this recipe cover? Sum those percentages.
        var scoredRecipes = new List<(RecipeNutritionDto Nutrition, decimal Score)>();

        foreach (var recipe in recipes)
        {
            var nutrition = await _recipeNutritionService.ComputeNutritionAsync(recipe.Id, ct);
            if (nutrition is null)
                continue;

            decimal score = 0;
            var covered = new List<MicronutrientTotalDto>();

            foreach (var deficit in topDeficits)
            {
                var match = nutrition.Micronutrients
                    .FirstOrDefault(m => m.MicronutrientId == deficit.MicronutrientId);

                if (match is null)
                    continue;

                covered.Add(match);

                if (referenceLookup.TryGetValue(deficit.MicronutrientId, out var micronutrient) &&
                    micronutrient.DailyReferenceAmount > 0)
                {
                    // Normalized contribution: what % of RDA does one serving provide?
                    score += match.TotalAmount / micronutrient.DailyReferenceAmount;
                }
            }

            if (score > 0)
            {
                scoredRecipes.Add((
                    nutrition with { Micronutrients = covered },
                    score));
            }
        }

        // -----------------------------------------------------------------------
        // 6. Take the top recipes, build the response
        // -----------------------------------------------------------------------
        var bestRecipes = scoredRecipes
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Nutrition.RecipeName)
            .Take(TopRecipesToSuggest)
            .ToList();

        var recipeSuggestions = bestRecipes.Select(r =>
        {
            var recipe = recipes.First(rec => rec.Id == r.Nutrition.RecipeId);
            return new DeficitRecipeSuggestionDto(
                r.Nutrition.RecipeId,
                r.Nutrition.RecipeName,
                recipe.PrepNote,
                recipe.YouTubeUrl,
                r.Nutrition.Micronutrients);
        }).ToList();

        return new DeficiencyAnalysisResultDto(
            DefaultLookbackDays,
            mealLogs.Select(ml => ml.EatenAt.Date).Distinct().Count(),
            topDeficits,
            recipeSuggestions);
    }
}