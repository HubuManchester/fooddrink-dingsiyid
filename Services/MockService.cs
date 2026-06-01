using CampusEats.Models;
using Microsoft.Maui.Devices.Sensors;

namespace CampusEats.Services;

/// <summary>
/// Mock service - provides simulated camera, location, and food recognition functionality
/// Used for local development and testing
/// </summary>
public class MockService
{
    private readonly Random _random = new Random();
    
    // Mock food recognition results
    private static readonly List<(string Name, string Category, int Calories, string Description)> MockFoods = new()
    {
        ("Tomato Egg Stir Fry", "Staples", 280, "Classic home cooking, sweet and sour, nutritious. Main ingredients: tomato, egg, scallion."),
        ("Braised Pork", "Staples", 450, "Traditional famous dish, bright red color, fatty but not greasy. Main ingredients: pork belly, ginger, star anise."),
        ("Vegetable Salad", "Vegetarian", 150, "Fresh and healthy green salad. Main ingredients: lettuce, tomato, cucumber, purple cabbage."),
        ("Tiramisu", "Desserts", 350, "Italian classic dessert, smooth texture. Main ingredients: mascarpone cheese, ladyfingers, cocoa powder."),
        ("Bubble Tea", "Beverages", 280, "Taiwanese milk tea, chewy pearls. Main ingredients: black tea, milk, brown sugar pearls."),
        ("Kung Pao Chicken", "Staples", 320, "Sichuan cuisine classic, spicy and fragrant. Main ingredients: chicken, peanuts, dried chilies.")
    };

    // Mock restaurant locations (around Beijing) - remove hardcoded distance, use real calculation
    private static readonly List<(string Name, double Latitude, double Longitude)> MockLocations = new()
    {
        ("Beijing Roast Duck Restaurant", 39.9087, 116.3912),
        ("Sichuan Hotpot", 39.9150, 116.3970),
        ("Japanese Cuisine", 39.9000, 116.3850),
        ("Cantonese Restaurant", 39.9200, 116.4000),
        ("Muslim Restaurant", 39.8950, 116.3900)
    };

    // Current user mock location
    public (double Latitude, double Longitude) CurrentLocation { get; private set; } = (39.9042, 116.4074); // Beijing Tiananmen

    /// <summary>
    /// Whether to use mock data (enabled by default)
    /// </summary>
    public bool UseMockData { get; set; } = true;

    /// <summary>
    /// Simulate taking photo and recognizing food
    /// </summary>
    /// <returns>Recognized food information</returns>
    public async Task<(string FoodName, string Category, int Calories, string Description, byte[] MockImage)> MockCaptureAndRecognizeAsync()
    {
        await Task.Delay(_random.Next(800, 1500)); // Simulate photo and recognition delay
        
        var food = MockFoods[_random.Next(MockFoods.Count)];
        
        // Generate simple mock image data
        var mockImage = GenerateMockImage(food.Name);
        
        return (food.Name, food.Category, food.Calories, food.Description, mockImage);
    }

    /// <summary>
    /// Simulate getting current location
    /// </summary>
    /// <returns>Latitude and longitude coordinates</returns>
    public async Task<(double Latitude, double Longitude)> MockGetLocationAsync()
    {
        await Task.Delay(_random.Next(300, 600)); // Simulate location delay
        
        // Random drift to make it more realistic
        var lat = CurrentLocation.Latitude + (_random.NextDouble() - 0.5) * 0.002;
        var lon = CurrentLocation.Longitude + (_random.NextDouble() - 0.5) * 0.002;
        
        CurrentLocation = (lat, lon);
        return CurrentLocation;
    }

    /// <summary>
    /// Simulate getting nearby restaurants
    /// </summary>
    /// <param name="currentLocation">Current location</param>
    /// <returns>List of nearby restaurants</returns>
    public async Task<List<Restaurant>> MockGetNearbyRestaurantsAsync((double Latitude, double Longitude) currentLocation)
    {
        await Task.Delay(_random.Next(400, 800)); // Simulate network request delay
        
        var userLocation = new Location(currentLocation.Latitude, currentLocation.Longitude);
        var restaurants = new List<Restaurant>();
        int id = 1;
        
        foreach (var location in MockLocations)
        {
            // Create restaurant location and calculate real distance
            var restaurantLoc = new Location(location.Latitude, location.Longitude);
            var distance = userLocation.CalculateDistance(restaurantLoc, DistanceUnits.Kilometers);
            
            var cuisineName = location.Name.Contains("Beijing") ? "Beijing Cuisine" : 
                          location.Name.Contains("Sichuan") ? "Sichuan Cuisine" : 
                          location.Name.Contains("Japanese") ? "Japanese Cuisine" :
                          location.Name.Contains("Cantonese") ? "Cantonese Cuisine" : "Muslim Cuisine";
            
            restaurants.Add(new Restaurant
            {
                Id = id++,
                Name = location.Name,
                Description = $"Serves authentic {cuisineName}",
                ImageName = "food_placeholder.jpg",
                Cuisine = cuisineName,
                Rating = 4.0 + _random.NextDouble() * 1.0,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Distance = distance, // Real calculated distance
                IsFavorite = _random.NextDouble() > 0.7,
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
            });
        }
        
        // Sort by distance
        return restaurants.OrderBy(r => r.Distance).ToList();
    }

    /// <summary>
    /// Simulate shake to get random recipe
    /// </summary>
    public async Task<Recipe> MockShakeForRecipeAsync()
    {
        await Task.Delay(_random.Next(500, 1000)); // Simulate shake delay
        
        var food = MockFoods[_random.Next(MockFoods.Count)];
        
        return new Recipe
        {
            Id = _random.Next(1000),
            Name = food.Name,
            Category = food.Category,
            Rating = 4.2 + _random.NextDouble() * 0.8,
            Description = food.Description,
            Calories = food.Calories,
            PrepTime = 15 + _random.Next(45),
            Ingredients = "See details for ingredient list",
            Instructions = "See details for instructions",
            ImageName = "food_placeholder.jpg",
            IsFavorite = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generate mock image data
    /// </summary>
    private byte[] GenerateMockImage(string foodName)
    {
        // Create simple byte array as mock image
        var seed = foodName.GetHashCode();
        var bytes = new byte[2048];
        new Random(seed).NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// Manually set user location (for testing convenience)
    /// </summary>
    public void SetLocation(double latitude, double longitude)
    {
        CurrentLocation = (latitude, longitude);
    }

    /// <summary>
    /// Get all mock food names (for demo purposes)
    /// </summary>
    public List<string> GetAllMockFoodNames()
    {
        return MockFoods.Select(f => f.Name).ToList();
    }
}
