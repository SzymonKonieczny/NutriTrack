namespace NutriTrack.Application.DTOs;

/// <summary>
/// Represents the total amount of a single micronutrient in a recipe.
/// </summary>
public record MicronutrientTotalDto(
    Guid MicronutrientId,
    string MicronutrientName,
    decimal TotalAmount,
    string Unit);

/// <summary>
/// The computed micronutrient breakdown for a recipe.
/// </summary>
public record RecipeNutritionDto(
    Guid RecipeId,
    string RecipeName,
    List<MicronutrientTotalDto> Micronutrients);