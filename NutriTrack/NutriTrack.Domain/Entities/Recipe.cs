using NutriTrack.Domain.Enums;

namespace NutriTrack.Domain.Entities;

public class Recipe
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? AuthorId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? PrepNote { get; set; }
    public string? YouTubeUrl { get; set; }

    /// <summary>
    /// Visibility level for this recipe.
    /// Defaults to <see cref="EntryVisibility.Private"/> — only the author (and admins) can see it.
    /// </summary>
    public EntryVisibility Visibility { get; set; } = EntryVisibility.Private;

    // Navigation
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}