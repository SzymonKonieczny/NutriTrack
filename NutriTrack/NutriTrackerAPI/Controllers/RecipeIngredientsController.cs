using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for recipe-ingredient associations. Restricted to administrators.
/// Nested under /api/recipes/{recipeId}/ingredients.
/// </summary>
[ApiController]
[Route("api/recipes/{recipeId:guid}/ingredients")]
[Authorize(Roles = IdentityConstants.Roles.Admin)]
public class RecipeIngredientsController : ControllerBase
{
    private readonly NutriTrackDbContext _db;

    public RecipeIngredientsController(NutriTrackDbContext db)
    {
        _db = db;
    }

    /// <summary>List all ingredients for a specific recipe.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RecipeIngredientResponse>>> GetAll(
        Guid recipeId, CancellationToken ct)
    {
        var recipeExists = await _db.Recipes.AnyAsync(r => r.Id == recipeId, ct);
        if (!recipeExists)
            return NotFound(new { error = "Recipe not found." });

        var ingredients = await _db.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId)
            .Include(ri => ri.Ingredient)
            .OrderBy(ri => ri.Ingredient.Name)
            .Select(ri => new RecipeIngredientResponse(
                ri.RecipeId, ri.IngredientId, ri.Ingredient.Name, ri.AmountInGrams))
            .ToListAsync(ct);

        return Ok(ingredients);
    }

    /// <summary>Add an ingredient to a recipe.</summary>
    [HttpPost]
    public async Task<ActionResult<RecipeIngredientResponse>> Create(
        Guid recipeId, [FromBody] CreateRecipeIngredientRequest request, CancellationToken ct)
    {
        var recipeExists = await _db.Recipes.AnyAsync(r => r.Id == recipeId, ct);
        if (!recipeExists)
            return NotFound(new { error = "Recipe not found." });

        var ingredientExists = await _db.Ingredients.AnyAsync(i => i.Id == request.IngredientId, ct);
        if (!ingredientExists)
            return BadRequest(new { error = "Ingredient not found." });

        var alreadyExists = await _db.RecipeIngredients
            .AnyAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == request.IngredientId, ct);
        if (alreadyExists)
            return Conflict(new { error = "This ingredient is already added to the recipe." });

        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = recipeId,
            IngredientId = request.IngredientId,
            AmountInGrams = request.AmountInGrams,
        };

        _db.RecipeIngredients.Add(recipeIngredient);
        await _db.SaveChangesAsync(ct);

        var ingredient = await _db.Ingredients.FindAsync([request.IngredientId], ct);

        return CreatedAtAction(nameof(GetAll), new { recipeId },
            new RecipeIngredientResponse(recipeId, request.IngredientId, ingredient!.Name, request.AmountInGrams));
    }

    /// <summary>Update the amount of an ingredient in a recipe.</summary>
    [HttpPut("{ingredientId:guid}")]
    public async Task<ActionResult<RecipeIngredientResponse>> Update(
        Guid recipeId, Guid ingredientId,
        [FromBody] UpdateRecipeIngredientRequest request, CancellationToken ct)
    {
        var recipeIngredient = await _db.RecipeIngredients
            .Include(ri => ri.Ingredient)
            .FirstOrDefaultAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == ingredientId, ct);

        if (recipeIngredient is null)
            return NotFound();

        recipeIngredient.AmountInGrams = request.AmountInGrams;
        await _db.SaveChangesAsync(ct);

        return Ok(new RecipeIngredientResponse(
            recipeId, ingredientId, recipeIngredient.Ingredient.Name, request.AmountInGrams));
    }

    /// <summary>Remove an ingredient from a recipe.</summary>
    [HttpDelete("{ingredientId:guid}")]
    public async Task<IActionResult> Delete(Guid recipeId, Guid ingredientId, CancellationToken ct)
    {
        var recipeIngredient = await _db.RecipeIngredients
            .FirstOrDefaultAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == ingredientId, ct);

        if (recipeIngredient is null)
            return NotFound();

        _db.RecipeIngredients.Remove(recipeIngredient);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}