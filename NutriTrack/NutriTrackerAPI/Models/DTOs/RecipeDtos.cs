namespace NutriTrackerAPI.Models.DTOs;

/// <summary>An ingredient entry to associate when creating a recipe.</summary>
public record CreateRecipeIngredientEntry(
    Guid IngredientId,
    decimal AmountInGrams);

/// <summary>Request to create or update a recipe.</summary>
public record CreateRecipeRequest(
    string Name,
    string? PrepNote = null,
    string? YouTubeUrl = null,
    string? Visibility = null,
    List<CreateRecipeIngredientEntry>? Ingredients = null);

/// <summary>Request to update an existing recipe.</summary>
public record UpdateRecipeRequest(
    string Name,
    string? PrepNote = null,
    string? YouTubeUrl = null,
    string? Visibility = null);

/// <summary>Response containing recipe data.</summary>
public record RecipeResponse(
    Guid Id,
    string Name,
    string? PrepNote,
    string? YouTubeUrl,
    Guid? AuthorId = null,
    string? Visibility = null);