using SQLite;

namespace CampusEats.Models;

[Table("Reviews")]
public class Review
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int RestaurantId { get; set; }

    [MaxLength(500)]
    public string UserComment { get; set; } = string.Empty;

    [NotNull]
    public int Rating { get; set; }

    [NotNull]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}