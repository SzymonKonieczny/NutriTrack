using Microsoft.EntityFrameworkCore;
using NutriTrack.Domain.Entities;

namespace NutriTrack.Domain.Data;

public class NutriTrackDbContext : DbContext
{
    public NutriTrackDbContext(DbContextOptions<NutriTrackDbContext> options)
        : base(options)
    {
    }

    public DbSet<Micronutrient> Micronutrients => Set<Micronutrient>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<IngredientMicronutrient> IngredientMicronutrients => Set<IngredientMicronutrient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("domain");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NutriTrackDbContext).Assembly);
    }
}