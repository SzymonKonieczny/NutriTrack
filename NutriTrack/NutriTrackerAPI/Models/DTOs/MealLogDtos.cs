namespace NutriTrackerAPI.Models.DTOs;

/// <summary>Request to create a meal log entry.</summary>
public record CreateMealLogRequest(
    DateTime EatenAt,
    Guid? RecipeId = null,
    Guid? IngredientId = null,
    decimal? AmountInGrams = null,
    decimal? Servings = null,
    string? Note = null);

/// <summary>Request to update a meal log entry.</summary>
public record UpdateMealLogRequest(
    DateTime EatenAt,
    Guid? RecipeId = null,
    Guid? IngredientId = null,
    decimal? AmountInGrams = null,
    decimal? Servings = null,
    string? Note = null);

/// <summary>Response containing meal log data.</summary>
public record MealLogResponse(
    Guid Id,
    DateTimeOffset EatenAt,
    string EatenByUserId,
    Guid? RecipeId,
    Guid? IngredientId,
    decimal? AmountInGrams,
    decimal? Servings,
    string? Note);