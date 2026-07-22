using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Application.DTOs;
using NutriTrack.Application.Services;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for meal logs. Authenticated users can manage their own logs.
/// Users with the Admin role can view and manage all logs.
/// </summary>
[ApiController]
[Route("api/meal-logs")]
[Authorize(Roles = $"{IdentityConstants.Roles.User},{IdentityConstants.Roles.Admin}")]
public class MealLogsController : ControllerBase
{
    private readonly NutriTrackDbContext _db;
    private readonly IRecipeNutritionService _nutritionService;

    public MealLogsController(NutriTrackDbContext db, IRecipeNutritionService nutritionService)
    {
        _db = db;
        _nutritionService = nutritionService;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identity not found in token.");

    private bool IsAdmin() =>
        User.IsInRole(IdentityConstants.Roles.Admin);

    /// <summary>List meal logs for the current user (admin sees all).</summary>
    [HttpGet]
    public async Task<ActionResult<List<MealLogResponse>>> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();

        var query = _db.MealLogs
            .Include(m => m.Recipe)
            .Include(m => m.Ingredient)
            .AsQueryable();

        if (!IsAdmin())
            query = query.Where(m => m.EatenByUserId == userId);

        var logs = await query
//            .OrderByDescending(m => m.EatenAt) not allowed in SQLite
            .Select(m => new MealLogResponse(
                m.Id, m.EatenAt, m.EatenByUserId,
                m.RecipeId, m.IngredientId,
                m.AmountInGrams, m.Servings, m.Note))
            .ToListAsync(ct);
        
        return Ok(logs.OrderByDescending(l => l.EatenAt));
    }

    /// <summary>Get a specific meal log entry.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MealLogResponse>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();

        var mealLog = await _db.MealLogs.FindAsync([id], ct);
        if (mealLog is null)
            return NotFound();

        // Non-admin users can only access their own logs
        if (!IsAdmin() && mealLog.EatenByUserId != userId)
            return Forbid();

        return Ok(new MealLogResponse(
            mealLog.Id, mealLog.EatenAt, mealLog.EatenByUserId,
            mealLog.RecipeId, mealLog.IngredientId,
            mealLog.AmountInGrams, mealLog.Servings, mealLog.Note));
    }

    [HttpGet("{id:guid}/nutrition")]
    public async Task<ActionResult<RecipeNutritionDto>> GetNutritionById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();

        var mealLog = await _db.MealLogs.FindAsync([id], ct);
        if (mealLog is null)
            return NotFound();

        // Non-admin users can only access their own logs
        if (!IsAdmin() && mealLog.EatenByUserId != userId)
            return Forbid();

        var result  = await _nutritionService.ComputeNutritionAsync(mealLog.RecipeId!.Value,ct);

        return Ok(result);
    }

    /// <summary>Create a new meal log entry for the current user.</summary>
    [HttpPost]
    public async Task<ActionResult<MealLogResponse>> Create(
        [FromBody] CreateMealLogRequest request, CancellationToken ct)
    {
        var userId = GetUserId();

        // Validate that at least one of RecipeId or IngredientId is provided
        if (request.RecipeId is null && request.IngredientId is null)
            return BadRequest(new { error = "Either RecipeId or IngredientId must be provided." });

        // Validate referenced entities exist
        if (request.RecipeId.HasValue)
        {
            var recipeExists = await _db.Recipes.AnyAsync(r => r.Id == request.RecipeId.Value, ct);
            if (!recipeExists)
                return BadRequest(new { error = "Recipe not found." });
        }

        if (request.IngredientId.HasValue)
        {
            var ingredientExists = await _db.Ingredients.AnyAsync(i => i.Id == request.IngredientId.Value, ct);
            if (!ingredientExists)
                return BadRequest(new { error = "Ingredient not found." });
        }

        var mealLog = new MealLog
        {
            EatenAt = request.EatenAt,
            EatenByUserId = userId,
            RecipeId = request.RecipeId,
            IngredientId = request.IngredientId,
            AmountInGrams = request.AmountInGrams,
            Servings = request.Servings,
            Note = request.Note,
        };

        _db.MealLogs.Add(mealLog);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = mealLog.Id },
            new MealLogResponse(
                mealLog.Id, mealLog.EatenAt, mealLog.EatenByUserId,
                mealLog.RecipeId, mealLog.IngredientId,
                mealLog.AmountInGrams, mealLog.Servings, mealLog.Note));
    }

    /// <summary>Update an existing meal log entry (own logs only, unless admin).</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MealLogResponse>> Update(
        Guid id, [FromBody] UpdateMealLogRequest request, CancellationToken ct)
    {
        var userId = GetUserId();

        var mealLog = await _db.MealLogs.FindAsync([id], ct);
        if (mealLog is null)
            return NotFound();

        // Non-admin users can only update their own logs
        if (!IsAdmin() && mealLog.EatenByUserId != userId)
            return Forbid();

        // Validate that at least one of RecipeId or IngredientId is provided
        if (request.RecipeId is null && request.IngredientId is null)
            return BadRequest(new { error = "Either RecipeId or IngredientId must be provided." });

        // Validate referenced entities exist
        if (request.RecipeId.HasValue)
        {
            var recipeExists = await _db.Recipes.AnyAsync(r => r.Id == request.RecipeId.Value, ct);
            if (!recipeExists)
                return BadRequest(new { error = "Recipe not found." });
        }

        if (request.IngredientId.HasValue)
        {
            var ingredientExists = await _db.Ingredients.AnyAsync(i => i.Id == request.IngredientId.Value, ct);
            if (!ingredientExists)
                return BadRequest(new { error = "Ingredient not found." });
        }

        mealLog.EatenAt = request.EatenAt;
        mealLog.RecipeId = request.RecipeId;
        mealLog.IngredientId = request.IngredientId;
        mealLog.AmountInGrams = request.AmountInGrams;
        mealLog.Servings = request.Servings;
        mealLog.Note = request.Note;

        await _db.SaveChangesAsync(ct);

        return Ok(new MealLogResponse(
            mealLog.Id, mealLog.EatenAt, mealLog.EatenByUserId,
            mealLog.RecipeId, mealLog.IngredientId,
            mealLog.AmountInGrams, mealLog.Servings, mealLog.Note));
    }

    /// <summary>Delete a meal log entry (own logs only, unless admin).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();

        var mealLog = await _db.MealLogs.FindAsync([id], ct);
        if (mealLog is null)
            return NotFound();

        // Non-admin users can only delete their own logs
        if (!IsAdmin() && mealLog.EatenByUserId != userId)
            return Forbid();

        _db.MealLogs.Remove(mealLog);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}