namespace NutriTrack.Application.DTOs;

/// <summary>
/// Represents a single micronutrient deficit found in the user's intake history.
/// </summary>
public record MicronutrientDeficitDto(
    Guid MicronutrientId,
    string MicronutrientName,
    decimal RecommendedDailyAmount,
    decimal AverageDailyConsumed,
    decimal DeficitPercentage,
    string Unit);

/// <summary>
/// A recipe suggestion generated to address the user's micronutrient deficits,
/// with a breakdown of how much of each target deficit it covers.
/// </summary>
public record DeficitRecipeSuggestionDto(
    Guid RecipeId,
    string RecipeName,
    string? PrepNote,
    string? YouTubeUrl,
    List<MicronutrientTotalDto> CoveredMicronutrients);

/// <summary>
/// The complete result of a deficiency analysis for a user,
/// containing their top deficits and suggested recipes.
/// </summary>
public record DeficiencyAnalysisResultDto(
    int LookbackDays,
    int TotalDaysWithData,
    List<MicronutrientDeficitDto> TopDeficits,
    List<DeficitRecipeSuggestionDto> SuggestedRecipes);