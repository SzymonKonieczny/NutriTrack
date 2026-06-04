using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NutriTrack.Application.Services;
using NutriTrack.Domain.Data;
using NutriTrackerAPI.Models.DTOs;
using System.Security.Claims;

namespace NutriTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipieProposalsController : ControllerBase {


        private readonly NutriTrackDbContext _db;
        private readonly IRecipieProposalService _recipieProposalService;
        public RecipieProposalsController(NutriTrackDbContext db, IRecipieProposalService recipieProposalService)
        {
            _db = db;
            _recipieProposalService = recipieProposalService;
        }


        [HttpGet("")]
        public async Task<ActionResult<List<RecipeResponse>>> GetForUser( CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("You need to be logged in to request recipies");
            }

            var recipieList = await _recipieProposalService.GetRecipiesForUserAsync(userId);

            List<RecipeResponse> result = new List<RecipeResponse>();
            foreach (var item in recipieList)
            {
                result.Add(new RecipeResponse(
                     item.Id,
                     item.Name,
                     item.PrepNote,
                     item.YouTubeUrl ?? "No Link Provided"
                ));
            }

            return Ok(result);
        }

    }
}
