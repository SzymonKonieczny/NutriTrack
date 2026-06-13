namespace NutriTrackerAPI.Models.DTOs;

/// <summary>
/// A single micronutrient deficit found in the user's intake analysis.
/// </summary>
public record MicronutrientDeficitResponse(
    Guid MicronutrientId,
    string MicronutrientName,
    decimal RecommendedDailyAmount,
    decimal AverageDailyConsumed,
    decimal DeficitPercentage,
    string Unit);

/// <summary>
/// A micronutrient amount as part of a recipe suggestion response.
/// </summary>
public record CoveredMicronutrientResponse(
    Guid MicronutrientId,
    string MicronutrientName,
    decimal TotalAmount,
    string Unit);

/// <summary>
/// A recipe suggested to address the user's deficiencies.
/// </summary>
public record DeficitRecipeSuggestionResponse(
    Guid RecipeId,
    string RecipeName,
    string? PrepNote,
    string? YouTubeUrl,
    List<CoveredMicronutrientResponse> CoveredMicronutrients);

/// <summary>
/// The full deficiency analysis result for the API response.
/// </summary>
public record DeficiencyAnalysisResponse(
    int LookbackDays,
    int TotalDaysWithData,
    List<MicronutrientDeficitResponse> TopDeficits,
    List<DeficitRecipeSuggestionResponse> SuggestedRecipes);