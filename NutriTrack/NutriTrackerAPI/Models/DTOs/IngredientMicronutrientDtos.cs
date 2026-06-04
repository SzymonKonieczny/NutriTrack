namespace NutriTrackerAPI.Models.DTOs;

/// <summary>Request to add or update an ingredient-micronutrient relationship.</summary>
public record CreateIngredientMicronutrientRequest(
    Guid MicronutrientId,
    decimal AmountPer100g);

/// <summary>Request to update the amount of an existing ingredient-micronutrient relationship.</summary>
public record UpdateIngredientMicronutrientRequest(
    decimal AmountPer100g);

/// <summary>Response containing ingredient-micronutrient data.</summary>
public record IngredientMicronutrientResponse(
    Guid IngredientId,
    Guid MicronutrientId,
    string MicronutrientName,
    decimal AmountPer100g);