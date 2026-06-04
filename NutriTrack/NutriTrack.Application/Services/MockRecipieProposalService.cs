using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Data;
using NutriTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack.Application.Services
{
    public class MockRecipieProposalService : IRecipieProposalService
    {
        readonly private NutriTrackDbContext _db;
        public MockRecipieProposalService(NutriTrackDbContext db)
        {
            _db = db;
        }
        public async Task<List<Recipe>> GetRecipiesForUserAsync(string userId)
        {
            var weekAgo = DateTimeOffset.Now.AddDays(-7);
            var mealsEaten = new List<MealLog>();
            if(_db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                mealsEaten = await _db.MealLogs.Where(m => m.EatenByUserId == userId).ToListAsync();
                mealsEaten = mealsEaten.Where(m => m.EatenAt < weekAgo).ToList();
            }
            else
            {
                mealsEaten = await _db.MealLogs.Where(m => m.EatenByUserId == userId && m.EatenAt < weekAgo).ToListAsync();
            }


            return new List<Recipe>();
        }
    }
}
