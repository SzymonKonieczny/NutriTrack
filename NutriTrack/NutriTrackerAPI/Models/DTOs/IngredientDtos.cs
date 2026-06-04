namespace NutriTrackerAPI.Models.DTOs;

/// <summary>A micronutrient entry to associate when creating an ingredient.</summary>
public record CreateIngredientMicronutrientEntry(
    Guid MicronutrientId,
    decimal AmountPer100g);

/// <summary>Request to create or update an ingredient.</summary>
public record CreateIngredientRequest(
    string Name,
    string? Note = null,
    List<CreateIngredientMicronutrientEntry>? Micronutrients = null);

/// <summary>Request to update an existing ingredient.</summary>
public record UpdateIngredientRequest(
    string Name,
    string? Note = null);

/// <summary>Response containing ingredient data.</summary>
public record IngredientResponse(
    Guid Id,
    string Name,
    string? Note);