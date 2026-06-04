using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for ingredient-micronutrient associations. Restricted to administrators.
/// Nested under /api/ingredients/{ingredientId}/micronutrients.
/// </summary>
[ApiController]
[Route("api/ingredients/{ingredientId:guid}/micronutrients")]
[Authorize(Roles = IdentityConstants.Roles.Admin)]
public class IngredientMicronutrientsController : ControllerBase
{
    private readonly NutriTrackDbContext _db;

    public IngredientMicronutrientsController(NutriTrackDbContext db)
    {
        _db = db;
    }

    /// <summary>List all micronutrients for a specific ingredient.</summary>
    [HttpGet]
    public async Task<ActionResult<List<IngredientMicronutrientResponse>>> GetAll(
        Guid ingredientId, CancellationToken ct)
    {
        var ingredientExists = await _db.Ingredients.AnyAsync(i => i.Id == ingredientId, ct);
        if (!ingredientExists)
            return NotFound(new { error = "Ingredient not found." });

        var micronutrients = await _db.IngredientMicronutrients
            .Where(im => im.IngredientId == ingredientId)
            .Include(im => im.Micronutrient)
            .OrderBy(im => im.Micronutrient.Name)
            .Select(im => new IngredientMicronutrientResponse(
                im.IngredientId, im.MicronutrientId, im.Micronutrient.Name, im.AmountPer100g))
            .ToListAsync(ct);

        return Ok(micronutrients);
    }

    /// <summary>Add a micronutrient to an ingredient.</summary>
    [HttpPost]
    public async Task<ActionResult<IngredientMicronutrientResponse>> Create(
        Guid ingredientId, [FromBody] CreateIngredientMicronutrientRequest request, CancellationToken ct)
    {
        var ingredientExists = await _db.Ingredients.AnyAsync(i => i.Id == ingredientId, ct);
        if (!ingredientExists)
            return NotFound(new { error = "Ingredient not found." });

        var micronutrientExists = await _db.Micronutrients.AnyAsync(m => m.Id == request.MicronutrientId, ct);
        if (!micronutrientExists)
            return BadRequest(new { error = "Micronutrient not found." });

        var alreadyExists = await _db.IngredientMicronutrients
            .AnyAsync(im => im.IngredientId == ingredientId && im.MicronutrientId == request.MicronutrientId, ct);
        if (alreadyExists)
            return Conflict(new { error = "This micronutrient is already associated with the ingredient." });

        var ingredientMicronutrient = new IngredientMicronutrient
        {
            IngredientId = ingredientId,
            MicronutrientId = request.MicronutrientId,
            AmountPer100g = request.AmountPer100g,
        };

        _db.IngredientMicronutrients.Add(ingredientMicronutrient);
        await _db.SaveChangesAsync(ct);

        var micronutrient = await _db.Micronutrients.FindAsync([request.MicronutrientId], ct);

        return CreatedAtAction(nameof(GetAll), new { ingredientId },
            new IngredientMicronutrientResponse(
                ingredientId, request.MicronutrientId, micronutrient!.Name, request.AmountPer100g));
    }

    /// <summary>Update the amount of a micronutrient for an ingredient.</summary>
    [HttpPut("{micronutrientId:guid}")]
    public async Task<ActionResult<IngredientMicronutrientResponse>> Update(
        Guid ingredientId, Guid micronutrientId,
        [FromBody] UpdateIngredientMicronutrientRequest request, CancellationToken ct)
    {
        var ingredientMicronutrient = await _db.IngredientMicronutrients
            .Include(im => im.Micronutrient)
            .FirstOrDefaultAsync(
                im => im.IngredientId == ingredientId && im.MicronutrientId == micronutrientId, ct);

        if (ingredientMicronutrient is null)
            return NotFound();

        ingredientMicronutrient.AmountPer100g = request.AmountPer100g;
        await _db.SaveChangesAsync(ct);

        return Ok(new IngredientMicronutrientResponse(
            ingredientId, micronutrientId, ingredientMicronutrient.Micronutrient.Name, request.AmountPer100g));
    }

    /// <summary>Remove a micronutrient from an ingredient.</summary>
    [HttpDelete("{micronutrientId:guid}")]
    public async Task<IActionResult> Delete(
        Guid ingredientId, Guid micronutrientId, CancellationToken ct)
    {
        var ingredientMicronutrient = await _db.IngredientMicronutrients
            .FirstOrDefaultAsync(
                im => im.IngredientId == ingredientId && im.MicronutrientId == micronutrientId, ct);

        if (ingredientMicronutrient is null)
            return NotFound();

        _db.IngredientMicronutrients.Remove(ingredientMicronutrient);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}