using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriTrack.Domain.Entities;

namespace NutriTrack.Domain.Data.Configurations;

public class IngredientMicronutrientConfiguration : IEntityTypeConfiguration<IngredientMicronutrient>
{
    public void Configure(EntityTypeBuilder<IngredientMicronutrient> builder)
    {
        builder.ToTable("IngredientMicronutrients");

        builder.HasKey(e => new { e.IngredientId, e.MicronutrientId });

        builder.Property(e => e.AmountPer100g)
               .HasColumnType("decimal(18,4)");

        builder.HasOne(e => e.Ingredient)
               .WithMany(i => i.IngredientMicronutrients)
               .HasForeignKey(e => e.IngredientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Micronutrient)
               .WithMany(m => m.IngredientMicronutrients)
               .HasForeignKey(e => e.MicronutrientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}