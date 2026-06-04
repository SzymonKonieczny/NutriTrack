using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriTrack.Domain.Entities;

namespace NutriTrack.Domain.Data.Configurations;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.ToTable("MealLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EatenByUserId)
               .IsRequired()
               .HasMaxLength(450);

        builder.Property(e => e.AmountInGrams)
               .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Servings)
               .HasColumnType("decimal(18,2)");

        // One of RecipeId or IngredientId must be set, but this is a business rule
        // enforced at the application layer, not a FK constraint.
        builder.HasOne(e => e.Recipe)
               .WithMany()
               .HasForeignKey(e => e.RecipeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Ingredient)
               .WithMany()
               .HasForeignKey(e => e.IngredientId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.EatenByUserId);
        builder.HasIndex(e => e.EatenAt);
    }
}