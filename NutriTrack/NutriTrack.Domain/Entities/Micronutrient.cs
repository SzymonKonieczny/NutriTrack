using NutriTrack.Domain.Enums;

namespace NutriTrack.Domain.Entities;

public class Micronutrient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal DailyReferenceAmount { get; set; }
    public MicronutrientUnit Unit { get; set; }
    public string? Note { get; set; }

    // Navigation
    public ICollection<IngredientMicronutrient> IngredientMicronutrients { get; set; } = new List<IngredientMicronutrient>();
}