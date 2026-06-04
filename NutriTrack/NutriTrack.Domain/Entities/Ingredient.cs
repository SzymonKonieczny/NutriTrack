namespace NutriTrack.Domain.Entities;

public class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }

    // Navigation
    public ICollection<IngredientMicronutrient> IngredientMicronutrients { get; set; } = new List<IngredientMicronutrient>();
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}