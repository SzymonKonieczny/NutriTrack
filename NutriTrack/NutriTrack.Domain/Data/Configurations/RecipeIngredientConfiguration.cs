using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriTrack.Domain.Entities;

namespace NutriTrack.Domain.Data.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("RecipeIngredients");

        builder.HasKey(e => new { e.RecipeId, e.IngredientId });

        builder.Property(e => e.AmountInGrams)
               .HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.Recipe)
               .WithMany(r => r.RecipeIngredients)
               .HasForeignKey(e => e.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Ingredient)
               .WithMany(i => i.RecipeIngredients)
               .HasForeignKey(e => e.IngredientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}