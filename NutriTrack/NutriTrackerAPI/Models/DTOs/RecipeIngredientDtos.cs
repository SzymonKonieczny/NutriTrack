namespace NutriTrackerAPI.Models.DTOs;

/// <summary>Request to add or update a recipe-ingredient relationship.</summary>
public record CreateRecipeIngredientRequest(
    Guid IngredientId,
    decimal AmountInGrams);

/// <summary>Request to update the amount of an existing recipe ingredient.</summary>
public record UpdateRecipeIngredientRequest(
    decimal AmountInGrams);

/// <summary>Response containing recipe-ingredient data.</summary>
public record RecipeIngredientResponse(
    Guid RecipeId,
    Guid IngredientId,
    string IngredientName,
    decimal AmountInGrams);