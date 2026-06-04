namespace NutriTrackerAPI.Models.DTOs;

/// <summary>Request to create or update a micronutrient.</summary>
public record CreateMicronutrientRequest(
    string Name,
    decimal DailyReferenceAmount,
    string Unit,
    string? Note = null);

/// <summary>Request to update an existing micronutrient.</summary>
public record UpdateMicronutrientRequest(
    string Name,
    decimal DailyReferenceAmount,
    string Unit,
    string? Note = null);

/// <summary>Response containing micronutrient data.</summary>
public record MicronutrientResponse(
    Guid Id,
    string Name,
    decimal DailyReferenceAmount,
    string Unit,
    string? Note);