using NutriTrack.Application.DTOs;

namespace NutriTrack.Application.Services;

/// <summary>
/// Analyzes a user's meal log history for micronutrient deficiencies
/// and suggests recipes that best address the top deficits.
/// </summary>
public interface IDeficiencyAnalysisService
{
    /// <summary>
    /// Analyzes the user's recent meal history, identifies their most
    /// deficient micronutrients, and suggests recipes to address those deficits.
    /// </summary>
    Task<DeficiencyAnalysisResultDto> AnalyzeAndSuggestAsync(string userId, CancellationToken ct = default);
}