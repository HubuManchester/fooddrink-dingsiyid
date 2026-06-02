using SQLite;

namespace CampusEats.Models;

[Table("Restaurants")]
public class Restaurant
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100), NotNull]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Cuisine { get; set; } = string.Empty;

    [NotNull]
    public double Rating { get; set; } = 0.0;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [MaxLength(200)]
    public string ImageName { get; set; } = "food_placeholder.jpg";

    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public byte[]? ImageData { get; set; }

    [Ignore]
    public double Distance { get; set; }

    [Ignore]
    public string RatingStars => GenerateStars(Rating);

    private static string GenerateStars(double rating)
    {
        int full = (int)Math.Floor(rating);
        bool half = (rating - full) >= 0.5;
        int empty = 5 - full - (half ? 1 : 0);
        return new string('★', full) + (half ? "½" : "") + new string('☆', empty);
    }
}
