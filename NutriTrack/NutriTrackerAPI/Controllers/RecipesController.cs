using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for recipes. Restricted to administrators.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = IdentityConstants.Roles.User)]
public class RecipesController : ControllerBase
{
    private readonly NutriTrackDbContext _db;

    public RecipesController(NutriTrackDbContext db)
    {
        _db = db;
    }

    /// <summary>List all recipes.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RecipeResponse>>> GetAll(CancellationToken ct)
    {
        var recipes = await _db.Recipes
            .OrderBy(r => r.Name)
            .Select(r => new RecipeResponse(r.Id, r.Name, r.PrepNote, r.YouTubeUrl))
            .ToListAsync(ct);

        return Ok(recipes);
    }

    /// <summary>Get a specific recipe by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeResponse>> GetById(Guid id, CancellationToken ct)
    {
        var recipe = await _db.Recipes.FindAsync([id], ct);
        if (recipe is null)
            return NotFound();

        return Ok(new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl));
    }

    /// <summary>Create a new recipe.</summary>
    [HttpPost]
    [Authorize(Roles = IdentityConstants.Roles.Admin)]
    public async Task<ActionResult<RecipeResponse>> Create(
        [FromBody] CreateRecipeRequest request, CancellationToken ct)
    {
        var recipe = new Recipe
        {
            Name = request.Name,
            PrepNote = request.PrepNote,
            YouTubeUrl = request.YouTubeUrl,
        };

        if (request.Ingredients is { Count: > 0 })
        {
            var ingredientIds = request.Ingredients.Select(i => i.IngredientId).ToHashSet();
            var existing = (await _db.Ingredients
                .Where(i => ingredientIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync(ct)).ToHashSet();

            foreach (var entry in request.Ingredients)
            {
                if (!existing.Contains(entry.IngredientId))
                    return BadRequest(new { error = $"Ingredient {entry.IngredientId} not found." });

                recipe.RecipeIngredients.Add(new RecipeIngredient
                {
                    IngredientId = entry.IngredientId,
                    AmountInGrams = entry.AmountInGrams,
                });
            }
        }

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = recipe.Id },
            new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl));
    }

    /// <summary>Update an existing recipe.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = IdentityConstants.Roles.Admin)]
    public async Task<ActionResult<RecipeResponse>> Update(
        Guid id, [FromBody] UpdateRecipeRequest request, CancellationToken ct)
    {
        var recipe = await _db.Recipes.FindAsync([id], ct);
        if (recipe is null)
            return NotFound();

        recipe.Name = request.Name;
        recipe.PrepNote = request.PrepNote;
        recipe.YouTubeUrl = request.YouTubeUrl;

        await _db.SaveChangesAsync(ct);

        return Ok(new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl));
    }

    /// <summary>Delete a recipe.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = IdentityConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var recipe = await _db.Recipes.FindAsync([id], ct);
        if (recipe is null)
            return NotFound();

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}