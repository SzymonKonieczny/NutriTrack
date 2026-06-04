using NutriTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack.Application.Services
{
    public interface IRecipieProposalService
    {
        Task<List<Recipe>> GetRecipiesForUserAsync(string userId);
    }
}
