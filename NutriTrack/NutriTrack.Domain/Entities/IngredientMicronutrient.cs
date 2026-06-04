namespace NutriTrack.Domain.Entities;

public class IngredientMicronutrient
{
    public Guid IngredientId { get; set; }
    public Guid MicronutrientId { get; set; }
    public decimal AmountPer100g { get; set; }

    // Navigation
    public Ingredient Ingredient { get; set; } = null!;
    public Micronutrient Micronutrient { get; set; } = null!;
}