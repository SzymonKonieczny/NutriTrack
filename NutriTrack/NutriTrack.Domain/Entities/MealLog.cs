namespace NutriTrack.Domain.Entities;

public class MealLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime EatenAt { get; set; }
    public string EatenByUserId { get; set; } = string.Empty;
    public Guid? RecipeId { get; set; }
    public Guid? IngredientId { get; set; }
    public decimal? AmountInGrams { get; set; }
    public decimal? Servings { get; set; }
    public string? Note { get; set; }

    // Navigation
    public Recipe? Recipe { get; set; }
    public Ingredient? Ingredient { get; set; }
}