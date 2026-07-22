using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Application.DTOs;
using NutriTrack.Application.Services;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Domain.Enums;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;
using System.Security.Claims;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for recipes.
/// Authenticated users can create their own recipes. Users see their own recipes
/// plus public recipes; admins see everything.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = IdentityConstants.Roles.User)]
public class RecipesController : ControllerBase
{
    private readonly NutriTrackDbContext _db;
    private readonly IRecipeNutritionService _nutritionService;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.IsInRole(IdentityConstants.Roles.Admin);

    public RecipesController(NutriTrackDbContext db, IRecipeNutritionService nutritionService)
    {
        _db = db;
        _nutritionService = nutritionService;
    }

    /// <summary>List all recipes the current user can see.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RecipeResponse>>> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();

        var query = _db.Recipes.AsQueryable();

        if (!IsAdmin())
        {
            // Users see: their own recipes + public recipes + system recipes (no author)
            query = query.Where(r =>
                r.AuthorId == userId ||
                r.Visibility == EntryVisibility.Public ||
                r.AuthorId == null);
        }

        var recipes = await query
            .OrderBy(r => r.Name)
            .Select(r => new RecipeResponse(
                r.Id, r.Name, r.PrepNote, r.YouTubeUrl, r.AuthorId, r.Visibility.ToString()))
            .ToListAsync(ct);

        return Ok(recipes);
    }

    /// <summary>Get a specific recipe by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeResponse>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var recipe = await _db.Recipes.FindAsync([id], ct);

        if (recipe is null)
            return NotFound();

        // Non-admin users can only access their own recipes or public ones
        if (!IsAdmin()
            && recipe.AuthorId != userId
            && recipe.Visibility != EntryVisibility.Public
            && recipe.AuthorId != null)
            return Forbid();

        return Ok(new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl, recipe.AuthorId, recipe.Visibility.ToString()));
    }

    /// <summary>Get the computed nutrition profile for a recipe.</summary>
    [HttpGet("{id:guid}/nutrition")]
    public async Task<ActionResult<RecipeNutritionDto>> GetNutrition(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var recipe = await _db.Recipes.FindAsync([id], ct);

        if (recipe is null)
            return NotFound();

        // Same visibility check
        if (!IsAdmin()
            && recipe.AuthorId != userId
            && recipe.Visibility != EntryVisibility.Public
            && recipe.AuthorId != null)
            return Forbid();

        var result = await _nutritionService.ComputeNutritionAsync(id, ct);
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>Create a new recipe. Any authenticated user can create.</summary>
    [HttpPost]
    public async Task<ActionResult<RecipeResponse>> Create(
        [FromBody] CreateRecipeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();

        var visibility = request.Visibility switch
        {
            nameof(EntryVisibility.Private) => EntryVisibility.Private,
            nameof(EntryVisibility.Unlisted) => EntryVisibility.Unlisted,
            _ => EntryVisibility.Private,
        };

        var recipe = new Recipe
        {
            Name = request.Name,
            PrepNote = request.PrepNote,
            YouTubeUrl = request.YouTubeUrl,
            AuthorId = userId,
            Visibility = visibility,
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
            new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl, recipe.AuthorId, recipe.Visibility.ToString()));
    }

    /// <summary>Update an existing recipe. Author or admin.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RecipeResponse>> Update(
        Guid id, [FromBody] UpdateRecipeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var recipe = await _db.Recipes.FindAsync([id], ct);

        if (recipe is null)
            return NotFound();

        if (!IsAdmin() && recipe.AuthorId != userId)
            return Forbid();

        recipe.Name = request.Name;
        recipe.PrepNote = request.PrepNote;
        recipe.YouTubeUrl = request.YouTubeUrl;

        if (request.Visibility is not null)
        {
            recipe.Visibility = request.Visibility switch
            {
                nameof(EntryVisibility.Private) => EntryVisibility.Private,
                nameof(EntryVisibility.Unlisted) => EntryVisibility.Unlisted,
                nameof(EntryVisibility.Public) => EntryVisibility.Public,
                nameof(EntryVisibility.Rejected) => EntryVisibility.Rejected,
                _ => recipe.Visibility,
            };
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl, recipe.AuthorId, recipe.Visibility.ToString()));
    }

    /// <summary>Approve a recipe (set visibility to Public). Admin only.</summary>
    [HttpPut("{id:guid}/approve")]
    public async Task<ActionResult<RecipeResponse>> Approve(Guid id, CancellationToken ct)
    {
        if (!IsAdmin())
            return Forbid();

        var recipe = await _db.Recipes.FindAsync([id], ct);

        if (recipe is null)
            return NotFound();

        recipe.Visibility = EntryVisibility.Public;
        await _db.SaveChangesAsync(ct);

        return Ok(new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl, recipe.AuthorId, recipe.Visibility.ToString()));
    }

    /// <summary>Reject a recipe (set visibility to Rejected). Admin only.</summary>
    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<RecipeResponse>> Reject(Guid id, CancellationToken ct)
    {
        if (!IsAdmin())
            return Forbid();

        var recipe = await _db.Recipes.FindAsync([id], ct);

        if (recipe is null)
            return NotFound();

        recipe.Visibility = EntryVisibility.Rejected;
        await _db.SaveChangesAsync(ct);

        return Ok(new RecipeResponse(recipe.Id, recipe.Name, recipe.PrepNote, recipe.YouTubeUrl, recipe.AuthorId, recipe.Visibility.ToString()));
    }

    /// <summary>Delete a recipe. Author or admin.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var recipe = await _db.Recipes.FindAsync([id], ct);

        if (recipe is null)
            return NotFound();

        if (!IsAdmin() && recipe.AuthorId != userId)
            return Forbid();

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}