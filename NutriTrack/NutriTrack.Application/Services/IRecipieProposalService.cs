using NutriTrack.Domain.Entities;

namespace NutriTrack.Application.Services
{
    public interface IRecipieProposalService
    {
        Task<List<Recipe>> GetRecipiesForUserAsync(string userId);
    }
}
