using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Identity.Configuration;
using NutriTrackerAPI.Models.DTOs;

namespace NutriTrackerAPI.Controllers;

/// <summary>
/// CRUD endpoints for micronutrients. Restricted to administrators.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = IdentityConstants.Roles.Admin)]
public class MicronutrientsController : ControllerBase
{
    private readonly NutriTrackDbContext _db;

    public MicronutrientsController(NutriTrackDbContext db)
    {
        _db = db;
    }

    /// <summary>List all micronutrients.</summary>
    [Authorize(Roles = IdentityConstants.Roles.User)]
    [HttpGet]
    public async Task<ActionResult<List<MicronutrientResponse>>> GetAll(CancellationToken ct)
    {
        var micronutrients = await _db.Micronutrients
            .OrderBy(m => m.Name)
            .Select(m => new MicronutrientResponse(
                m.Id, m.Name, m.DailyReferenceAmount, m.Unit.ToString(), m.Note, m.IsNonFoodSource))
            .ToListAsync(ct);

        return Ok(micronutrients);
    }

    /// <summary>Get a specific micronutrient by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = IdentityConstants.Roles.User)]
    public async Task<ActionResult<MicronutrientResponse>> GetById(Guid id, CancellationToken ct)
    {
        var micronutrient = await _db.Micronutrients.FindAsync([id], ct);
        if (micronutrient is null)
            return NotFound();

        return Ok(new MicronutrientResponse(
            micronutrient.Id, micronutrient.Name,
            micronutrient.DailyReferenceAmount, micronutrient.Unit.ToString(),
            micronutrient.Note, micronutrient.IsNonFoodSource));
    }

    /// <summary>Create a new micronutrient.</summary>
    [HttpPost]
    public async Task<ActionResult<MicronutrientResponse>> Create(
        [FromBody] CreateMicronutrientRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<NutriTrack.Domain.Enums.MicronutrientUnit>(request.Unit, ignoreCase: true, out var unit))
            return BadRequest(new { error = $"Invalid unit '{request.Unit}'. Valid values: Mg, Mcg, IU, G." });

        if(_db.Micronutrients.Any(m => m.Name == request.Name.Trim().ToLower()))
        {
            return BadRequest(new { error = "The ingredient (matched by name) already exists." });
        }

        var micronutrient = new Micronutrient
        {
            Name = request.Name.Trim().ToLower(),
            DailyReferenceAmount = request.DailyReferenceAmount,
            Unit = unit,
            Note = request.Note,
            IsNonFoodSource = request.IsNonFoodSource,
        };

        _db.Micronutrients.Add(micronutrient);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = micronutrient.Id },
            new MicronutrientResponse(
                micronutrient.Id, micronutrient.Name,
                micronutrient.DailyReferenceAmount, micronutrient.Unit.ToString(),
                micronutrient.Note, micronutrient.IsNonFoodSource));
    }

    /// <summary>Update an existing micronutrient.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MicronutrientResponse>> Update(
        Guid id, [FromBody] UpdateMicronutrientRequest request, CancellationToken ct)
    {
        var micronutrient = await _db.Micronutrients.FindAsync([id], ct);
        if (micronutrient is null)
            return NotFound();

        if (!Enum.TryParse<NutriTrack.Domain.Enums.MicronutrientUnit>(request.Unit, ignoreCase: true, out var unit))
            return BadRequest(new { error = $"Invalid unit '{request.Unit}'. Valid values: Mg, Mcg, IU, G." });

        micronutrient.Name = request.Name;
        micronutrient.DailyReferenceAmount = request.DailyReferenceAmount;
        micronutrient.Unit = unit;
        micronutrient.Note = request.Note;
        micronutrient.IsNonFoodSource = request.IsNonFoodSource;

        await _db.SaveChangesAsync(ct);

        return Ok(new MicronutrientResponse(
            micronutrient.Id, micronutrient.Name,
            micronutrient.DailyReferenceAmount, micronutrient.Unit.ToString(),
            micronutrient.Note, micronutrient.IsNonFoodSource));
    }

    /// <summary>Delete a micronutrient.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var micronutrient = await _db.Micronutrients.FindAsync([id], ct);
        if (micronutrient is null)
            return NotFound();

        _db.Micronutrients.Remove(micronutrient);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}