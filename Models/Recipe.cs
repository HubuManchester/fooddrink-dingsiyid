using SQLite;

namespace CampusEats.Models;

[Table("Recipes")]
public class Recipe
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100), NotNull]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [NotNull]
    public double Rating { get; set; } = 0.0;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Ingredients { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Instructions { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ImageName { get; set; } = "food_placeholder.jpg";

    [Ignore]
    public byte[]? ImageData { get; set; }

    [NotNull]
    public bool IsFavorite { get; set; } = false;

    [NotNull]
    public int Calories { get; set; } = 0;

    [NotNull]
    public int PrepTime { get; set; } = 0;

    [NotNull]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string RatingStars => GenerateStars(Rating);

    public string CategoryIcon => Category switch
    {
        "Staples" => "🍚",
        "Desserts" => "🍰",
        "Vegetarian" => "🥗",
        "Soups" => "🍲",
        "Beverages" => "🥤",
        _ => "🍽️"
    };

    private static string GenerateStars(double rating)
    {
        int full = (int)Math.Floor(rating);
        bool half = (rating - full) >= 0.5;
        int empty = 5 - full - (half ? 1 : 0);
        return new string('★', full) + (half ? "½" : "") + new string('☆', empty);
    }
}
