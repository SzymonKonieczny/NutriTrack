using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Domain.Enums;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;
using System.Security.Claims;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for ingredients.
/// Authenticated users can create their own ingredients. Users see their own
/// ingredients plus public ones; admins see everything.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = IdentityConstants.Roles.User)]
public class IngredientsController : ControllerBase
{
    private readonly NutriTrackDbContext _db;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.IsInRole(IdentityConstants.Roles.Admin);

    public IngredientsController(NutriTrackDbContext db)
    {
        _db = db;
    }

    /// <summary>List all ingredients the current user can see.</summary>
    [HttpGet]
    public async Task<ActionResult<List<IngredientResponse>>> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();

        var query = _db.Ingredients.AsQueryable();

        if (!IsAdmin())
        {
            // Users see: their own ingredients + public ingredients + system ingredients (no author)
            query = query.Where(i =>
                i.AuthorId == userId ||
                i.Visibility == EntryVisibility.Public ||
                i.AuthorId == null);
        }

        var ingredients = await query
            .OrderBy(i => i.Name)
            .Select(i => new IngredientResponse(i.Id, i.Name, i.Note, i.AuthorId, i.Visibility.ToString()))
            .ToListAsync(ct);

        return Ok(ingredients);
    }

    /// <summary>Get a specific ingredient by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IngredientResponse>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var ingredient = await _db.Ingredients.FindAsync([id], ct);

        if (ingredient is null)
            return NotFound();

        // Non-admin users can only access their own ingredients or public ones
        if (!IsAdmin()
            && ingredient.AuthorId != userId
            && ingredient.Visibility != EntryVisibility.Public
            && ingredient.AuthorId != null)
            return Forbid();

        return Ok(new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note, ingredient.AuthorId, ingredient.Visibility.ToString()));
    }

    /// <summary>Create a new ingredient. Any authenticated user can create.</summary>
    [HttpPost]
    public async Task<ActionResult<IngredientResponse>> Create(
        [FromBody] CreateIngredientRequest request, CancellationToken ct)
    {
        var userId = GetUserId();

        var visibility = request.Visibility switch
        {
            nameof(EntryVisibility.Private) => EntryVisibility.Private,
            nameof(EntryVisibility.Unlisted) => EntryVisibility.Unlisted,
            _ => EntryVisibility.Private,
        };

        var ingredient = new Ingredient
        {
            Name = request.Name,
            Note = request.Note,
            AuthorId = userId,
            Visibility = visibility,
        };

        if (request.Micronutrients is { Count: > 0 })
        {
            var micronutrientIds = request.Micronutrients.Select(m => m.MicronutrientId).ToHashSet();
            var existing = (await _db.Micronutrients
                .Where(m => micronutrientIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync(ct)).ToHashSet();

            foreach (var entry in request.Micronutrients)
            {
                if (!existing.Contains(entry.MicronutrientId))
                    return BadRequest(new { error = $"Micronutrient {entry.MicronutrientId} not found." });

                ingredient.IngredientMicronutrients.Add(new IngredientMicronutrient
                {
                    MicronutrientId = entry.MicronutrientId,
                    AmountPer100g = entry.AmountPer100g,
                });
            }
        }

        _db.Ingredients.Add(ingredient);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = ingredient.Id },
            new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note, ingredient.AuthorId, ingredient.Visibility.ToString()));
    }

    /// <summary>Update an existing ingredient. Author or admin.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<IngredientResponse>> Update(
        Guid id, [FromBody] UpdateIngredientRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var ingredient = await _db.Ingredients.FindAsync([id], ct);

        if (ingredient is null)
            return NotFound();

        if (!IsAdmin() && ingredient.AuthorId != userId)
            return Forbid();

        ingredient.Name = request.Name;
        ingredient.Note = request.Note;

        if (request.Visibility is not null)
        {
            ingredient.Visibility = request.Visibility switch
            {
                nameof(EntryVisibility.Private) => EntryVisibility.Private,
                nameof(EntryVisibility.Unlisted) => EntryVisibility.Unlisted,
                nameof(EntryVisibility.Public) => EntryVisibility.Public,
                nameof(EntryVisibility.Rejected) => EntryVisibility.Rejected,
                _ => ingredient.Visibility,
            };
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note, ingredient.AuthorId, ingredient.Visibility.ToString()));
    }

    /// <summary>Approve an ingredient (set visibility to Public). Admin only.</summary>
    [HttpPut("{id:guid}/approve")]
    public async Task<ActionResult<IngredientResponse>> Approve(Guid id, CancellationToken ct)
    {
        if (!IsAdmin())
            return Forbid();

        var ingredient = await _db.Ingredients.FindAsync([id], ct);

        if (ingredient is null)
            return NotFound();

        ingredient.Visibility = EntryVisibility.Public;
        await _db.SaveChangesAsync(ct);

        return Ok(new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note, ingredient.AuthorId, ingredient.Visibility.ToString()));
    }

    /// <summary>Reject an ingredient (set visibility to Rejected). Admin only.</summary>
    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<IngredientResponse>> Reject(Guid id, CancellationToken ct)
    {
        if (!IsAdmin())
            return Forbid();

        var ingredient = await _db.Ingredients.FindAsync([id], ct);

        if (ingredient is null)
            return NotFound();

        ingredient.Visibility = EntryVisibility.Rejected;
        await _db.SaveChangesAsync(ct);

        return Ok(new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note, ingredient.AuthorId, ingredient.Visibility.ToString()));
    }

    /// <summary>Delete an ingredient. Author or admin.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var ingredient = await _db.Ingredients.FindAsync([id], ct);

        if (ingredient is null)
            return NotFound();

        if (!IsAdmin() && ingredient.AuthorId != userId)
            return Forbid();

        _db.Ingredients.Remove(ingredient);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}