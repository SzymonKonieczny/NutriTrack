using Microsoft.AspNetCore.Authorization;
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
        private readonly IDeficiencyAnalysisService _deficiencyAnalysisService;
        public RecipieProposalsController(
            NutriTrackDbContext db,
            IRecipieProposalService recipieProposalService,
            IDeficiencyAnalysisService deficiencyAnalysisService)
        {
            _db = db;
            _recipieProposalService = recipieProposalService;
            _deficiencyAnalysisService = deficiencyAnalysisService;
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

        /// <summary>
        /// Analyzes the authenticated user's meal history and suggests recipes
        /// that best address their top micronutrient deficiencies.
        /// </summary>
        [HttpGet("deficiency")]
        [Authorize]
        public async Task<ActionResult<DeficiencyAnalysisResponse>> AnalyzeDeficiency(CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("You need to be logged in to request a deficiency analysis");
            }

            var result = await _deficiencyAnalysisService.AnalyzeAndSuggestAsync(userId, ct);

            var response = new DeficiencyAnalysisResponse(
                result.LookbackDays,
                result.TotalDaysWithData,
                result.TopDeficits.Select(d => new MicronutrientDeficitResponse(
                    d.MicronutrientId,
                    d.MicronutrientName,
                    d.RecommendedDailyAmount,
                    d.AverageDailyConsumed,
                    d.DeficitPercentage,
                    d.Unit
                )).ToList(),
                result.SuggestedRecipes.Select(r => new DeficitRecipeSuggestionResponse(
                    r.RecipeId,
                    r.RecipeName,
                    r.PrepNote,
                    r.YouTubeUrl,
                    r.CoveredMicronutrients.Select(m => new CoveredMicronutrientResponse(
                        m.MicronutrientId,
                        m.MicronutrientName,
                        m.TotalAmount,
                        m.Unit
                    )).ToList()
                )).ToList()
            );

            return Ok(response);
        }

    }
}
