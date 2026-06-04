using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for ingredients. Restricted to administrators.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = IdentityConstants.Roles.User)]
public class IngredientsController : ControllerBase
{
    private readonly NutriTrackDbContext _db;

    public IngredientsController(NutriTrackDbContext db)
    {
        _db = db;
    }

    /// <summary>List all ingredients.</summary>
    [HttpGet]
    [Authorize(Roles = IdentityConstants.Roles.User)]
    public async Task<ActionResult<List<IngredientResponse>>> GetAll(CancellationToken ct)
    {
        var ingredients = await _db.Ingredients
            .OrderBy(i => i.Name)
            .Select(i => new IngredientResponse(i.Id, i.Name, i.Note))
            .ToListAsync(ct);

        return Ok(ingredients);
    }

    /// <summary>Get a specific ingredient by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = IdentityConstants.Roles.User)]
    public async Task<ActionResult<IngredientResponse>> GetById(Guid id, CancellationToken ct)
    {
        var ingredient = await _db.Ingredients.FindAsync([id], ct);
        if (ingredient is null)
            return NotFound();

        return Ok(new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note));
    }

    /// <summary>Create a new ingredient.</summary>
    [HttpPost]
    [Authorize(Roles = IdentityConstants.Roles.Admin)]
    public async Task<ActionResult<IngredientResponse>> Create(
        [FromBody] CreateIngredientRequest request, CancellationToken ct)
    {
        var ingredient = new Ingredient
        {
            Name = request.Name,
            Note = request.Note,
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
            new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note));
    }

    /// <summary>Update an existing ingredient.</summary>
    [Authorize(Roles = IdentityConstants.Roles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<IngredientResponse>> Update(
        Guid id, [FromBody] UpdateIngredientRequest request, CancellationToken ct)
    {
        var ingredient = await _db.Ingredients.FindAsync([id], ct);
        if (ingredient is null)
            return NotFound();

        ingredient.Name = request.Name;
        ingredient.Note = request.Note;

        await _db.SaveChangesAsync(ct);

        return Ok(new IngredientResponse(ingredient.Id, ingredient.Name, ingredient.Note));
    }

    /// <summary>Delete an ingredient.</summary>
    [Authorize(Roles = IdentityConstants.Roles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ingredient = await _db.Ingredients.FindAsync([id], ct);
        if (ingredient is null)
            return NotFound();

        _db.Ingredients.Remove(ingredient);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}