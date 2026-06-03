using SQLite;
using CampusEats.Services;

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
    public string ImagePath
    {
        get
        {
            if (!string.IsNullOrEmpty(ImageName) && ImageName != "food_placeholder.jpg")
                return ImageName;
            return GetCuisineDefaultImage();
        }
    }

    [Ignore]
    public string FavoriteIcon => IsFavorite ? "★" : "☆";

    [Ignore]
    public string RatingStars => StringHelper.GenerateStars(Rating);

    private string GetCuisineDefaultImage()
    {
        return Cuisine.ToLower() switch
        {
            "chinese" => "restaurant_chinese.jpg",
            "italian" => "restaurant_italian.jpg",
            "japanese" => "restaurant_japanese.jpg",
            "mexican" => "restaurant_mexican.jpg",
            "indian" => "restaurant_indian.jpg",
            "thai" => "restaurant_thai.jpg",
            "american" => "restaurant_american.jpg",
            "korean" => "restaurant_korean.jpg",
            "french" => "restaurant_french.jpg",
            "vietnamese" => "restaurant_vietnamese.jpg",
            "fast food" => "restaurant_fastfood.jpg",
            "seafood" => "restaurant_seafood.jpg",
            "vegetarian" => "restaurant_vegetarian.jpg",
            "cafe" => "restaurant_cafe.jpg",
            "bakery" => "restaurant_bakery.jpg",
            "barbecue" => "restaurant_barbecue.jpg",
            "sushi" => "restaurant_sushi.jpg",
            "pizza" => "restaurant_pizza.jpg",
            "burger" => "restaurant_burger.jpg",
            "noodles" => "restaurant_noodles.jpg",
            _ => "food_placeholder.jpg"
        };
    }
}
