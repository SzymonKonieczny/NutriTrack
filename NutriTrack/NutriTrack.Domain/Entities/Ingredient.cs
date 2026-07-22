using NutriTrack.Domain.Enums;

namespace NutriTrack.Domain.Entities;

public class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The user who created this ingredient. Null for system/admin-created ingredients.
    /// </summary>
    public Guid? AuthorId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }

    /// <summary>
    /// Visibility level for this ingredient.
    /// Defaults to <see cref="EntryVisibility.Private"/> — only the author (and admins) can see it.
    /// </summary>
    public EntryVisibility Visibility { get; set; } = EntryVisibility.Private;

    // Navigation
    public ICollection<IngredientMicronutrient> IngredientMicronutrients { get; set; } = new List<IngredientMicronutrient>();
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}