using Microsoft.EntityFrameworkCore;
using NutriTrack.Application.Abstractions;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using NutriTrack.Domain.Enums;

namespace NutriTrack.Application.Services
{
    public class MockRecipieProposalService : IRecipieProposalService
    {
        private readonly NutriTrackDbContext _db;
        private readonly IUserContext _user;

        public MockRecipieProposalService(NutriTrackDbContext db, IUserContext user)
        {
            _db = db;
            _user = user;
        }

        public async Task<List<Recipe>> GetRecipiesForUserAsync(string userId)
        {
            var weekAgo = DateTimeOffset.Now.AddDays(-7);
            var mealsEaten = new List<MealLog>();
            if (_db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                mealsEaten = await _db.MealLogs.Where(m => m.EatenByUserId == userId).ToListAsync();
                mealsEaten = mealsEaten.Where(m => m.EatenAt < weekAgo).ToList();
            }
            else
            {
                mealsEaten = await _db.MealLogs.Where(m => m.EatenByUserId == userId && m.EatenAt < weekAgo).ToListAsync();
            }

            // TODO: implement actual recipe proposal logic based on meal history
            return new List<Recipe>();
        }
    }
}
