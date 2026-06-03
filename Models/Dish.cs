using SQLite;

namespace CampusEats.Models;

[Table("Dishes")]
public class Dish
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public int RestaurantId { get; set; }

    [MaxLength(100), NotNull]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [NotNull]
    public double Price { get; set; } = 0.0;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ImageName { get; set; } = "food_placeholder.jpg";

    [NotNull]
    public bool IsAvailable { get; set; } = true;

    [NotNull]
    public bool IsSpicy { get; set; } = false;

    [NotNull]
    public bool IsVegetarian { get; set; } = false;

    [NotNull]
    public int SpiceLevel { get; set; } = 0;

    [NotNull]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public byte[]? ImageData { get; set; }

    [Ignore]
    public string SpicyIcon => SpiceLevel switch
    {
        1 => "🌶️",
        2 => "🌶️🌶️",
        3 => "🌶️🌶️🌶️",
        4 => "🌶️🌶️🌶️🌶️",
        5 => "🌶️🌶️🌶️🌶️🌶️",
        _ => ""
    };

    [Ignore]
    public string DietaryIcons => (IsVegetarian ? "🥬 " : "") + (IsSpicy ? SpicyIcon : "");

    [Ignore]
    public string PriceDisplay => $"${Price:F2}";
}
