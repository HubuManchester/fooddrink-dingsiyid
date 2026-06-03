using CampusEats.Models;
using Microsoft.Maui.Devices.Sensors;

namespace CampusEats.Services;

public class MockService
{
    private readonly Random _random = new Random();
    private readonly LocalStorageService _localStorage;

    public (double Latitude, double Longitude) CurrentLocation { get; private set; } = (30.5447, 114.3549);

    public bool UseMockData { get; set; } = true;

    public MockService(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    private static readonly List<(string Name, string Category, int Calories, string Description)> MockFoods = new()
    {
        ("Tomato Egg Stir Fry", "Staples", 280, "Classic home cooking, sweet and sour, nutritious. Main ingredients: tomato, egg, scallion."),
        ("Braised Pork", "Staples", 450, "Traditional famous dish, bright red color, fatty but not greasy. Main ingredients: pork belly, ginger, star anise."),
        ("Vegetable Salad", "Vegetarian", 150, "Fresh and healthy green salad. Main ingredients: lettuce, tomato, cucumber, purple cabbage."),
        ("Tiramisu", "Desserts", 350, "Italian classic dessert, smooth texture. Main ingredients: mascarpone cheese, ladyfingers, cocoa powder."),
        ("Bubble Tea", "Beverages", 280, "Taiwanese milk tea, chewy pearls. Main ingredients: black tea, milk, brown sugar pearls."),
        ("Kung Pao Chicken", "Staples", 320, "Sichuan cuisine classic, spicy and fragrant. Main ingredients: chicken, peanuts, dried chilies."),
        ("Mapo Tofu", "Staples", 220, "Sichuan signature dish, spicy and numbing. Main ingredients: tofu, minced pork, doubanjiang."),
        ("Sweet and Sour Pork", "Staples", 380, "Popular Chinese dish, sweet and sour taste. Main ingredients: pork, pineapple, bell pepper."),
        ("Steamed Fish", "Staples", 200, "Healthy cooking method, fresh and tender. Main ingredients: fresh fish, ginger, scallion."),
        ("Egg Fried Rice", "Staples", 350, "Classic Chinese fried rice. Main ingredients: rice, egg, scallion, soy sauce.")
    };

    private static readonly List<MockRestaurantData> MockRestaurants = new()
    {
        new MockRestaurantData { Id = 1, Name = "Hubei University Canteen", Cuisine = "Chinese Home", Rating = 4.2, Description = "Campus canteen offering affordable and diverse Chinese dishes.", Latitude = 30.5445, Longitude = 114.3545, ImageName = "rice_bowl.jpg" },
        new MockRestaurantData { Id = 2, Name = "Wuchang Fish Restaurant", Cuisine = "Hubei Cuisine", Rating = 4.5, Description = "Specializes in Wuchang fish, a famous local delicacy.", Latitude = 30.5460, Longitude = 114.3560, ImageName = "sushi.jpg" },
        new MockRestaurantData { Id = 3, Name = "Haidilao Hotpot", Cuisine = "Sichuan Hotpot", Rating = 4.3, Description = "Authentic Sichuan hotpot with spicy broth.", Latitude = 30.5430, Longitude = 114.3530, ImageName = "noodle_soup.jpg" },
        new MockRestaurantData { Id = 4, Name = "Wuhan Hot Dry Noodles", Cuisine = "Hubei Noodles", Rating = 4.0, Description = "Authentic Wuhan hot dry noodles with sesame paste.", Latitude = 30.5455, Longitude = 114.3555, ImageName = "reganmian.jpg" },
        new MockRestaurantData { Id = 5, Name = "McDonald's", Cuisine = "Western Fast Food", Rating = 4.1, Description = "Classic burgers and fries, quick and convenient.", Latitude = 30.5425, Longitude = 114.3525, ImageName = "mcdonalds.jpg" },
        new MockRestaurantData { Id = 6, Name = "Luckin Coffee", Cuisine = "Coffee & Drinks", Rating = 4.4, Description = "Coconut latte, thick milk latte and various coffees.", Latitude = 30.5465, Longitude = 114.3570, ImageName = "luckin.jpg" },
        new MockRestaurantData { Id = 7, Name = "Mixue Ice Cream", Cuisine = "Desserts & Drinks", Rating = 4.2, Description = "Ice cream, milk tea at affordable prices.", Latitude = 30.5440, Longitude = 114.3540, ImageName = "mixue.jpg" },
        new MockRestaurantData { Id = 8, Name = "Pizza House", Cuisine = "Italian", Rating = 4.3, Description = "Authentic Italian pizza, freshly baked.", Latitude = 30.5435, Longitude = 114.3565, ImageName = "pizza.jpg" }
    };

    private static readonly List<MockRecipeData> MockRecipes = new()
    {
        new MockRecipeData { Id = 1, Name = "Tomato Egg Stir Fry", Category = "Staples", Rating = 4.5, Description = "Classic Chinese home cooking, sweet and sour taste.", Calories = 280, PrepTime = 15, Ingredients = "Tomato (2), Eggs (3), Scallion, Salt, Sugar", Instructions = "1. Cut tomatoes. 2. Beat eggs. 3. Fry eggs, add tomatoes. 4. Season.", ImageName = "tomato_egg.jpg" },
        new MockRecipeData { Id = 2, Name = "Kung Pao Chicken", Category = "Staples", Rating = 4.7, Description = "Famous Sichuan dish, spicy and fragrant.", Calories = 320, PrepTime = 25, Ingredients = "Chicken, Peanuts, Dried chilies, Sichuan peppercorn", Instructions = "1. Dice chicken. 2. Fry peanuts. 3. Stir-fry chicken. 4. Add peanuts.", ImageName = "kung_pao.jpg" },
        new MockRecipeData { Id = 3, Name = "Vegetable Salad", Category = "Vegetarian", Rating = 4.3, Description = "Fresh and healthy green salad.", Calories = 150, PrepTime = 10, Ingredients = "Lettuce, Tomato, Cucumber, Salad dressing", Instructions = "1. Wash vegetables. 2. Cut into pieces. 3. Mix with dressing.", ImageName = "salad.jpg" },
        new MockRecipeData { Id = 4, Name = "Tiramisu", Category = "Desserts", Rating = 4.8, Description = "Italian classic dessert with smooth texture.", Calories = 350, PrepTime = 45, Ingredients = "Mascarpone, Ladyfingers, Espresso, Cocoa powder", Instructions = "1. Mix mascarpone. 2. Dip ladyfingers. 3. Layer cream. 4. Chill.", ImageName = "tiramisu.jpg" },
        new MockRecipeData { Id = 5, Name = "Mapo Tofu", Category = "Staples", Rating = 4.6, Description = "Sichuan signature dish, spicy and numbing.", Calories = 220, PrepTime = 20, Ingredients = "Tofu, Minced pork, Doubanjiang, Sichuan peppercorn", Instructions = "1. Blanch tofu. 2. Fry pork. 3. Add sauce. 4. Thicken.", ImageName = "mapo_tofu.jpg" },
        new MockRecipeData { Id = 6, Name = "Bubble Tea", Category = "Beverages", Rating = 4.4, Description = "Taiwanese milk tea with chewy tapioca pearls.", Calories = 280, PrepTime = 15, Ingredients = "Black tea, Milk, Brown sugar, Tapioca pearls", Instructions = "1. Brew tea. 2. Cook pearls. 3. Mix tea with milk. 4. Add pearls.", ImageName = "bubble_tea.jpg" },
        new MockRecipeData { Id = 7, Name = "Sweet and Sour Pork", Category = "Staples", Rating = 4.5, Description = "Crispy pork in sweet and sour sauce.", Calories = 380, PrepTime = 30, Ingredients = "Pork, Pineapple, Bell pepper, Sweet and sour sauce", Instructions = "1. Cut pork. 2. Deep fry. 3. Stir-fry vegetables. 4. Mix with sauce.", ImageName = "sweet_sour_pork.jpg" },
        new MockRecipeData { Id = 8, Name = "Steamed Fish", Category = "Staples", Rating = 4.7, Description = "Healthy cooking preserving fresh taste.", Calories = 200, PrepTime = 20, Ingredients = "Fresh fish, Ginger, Scallion, Soy sauce", Instructions = "1. Clean fish. 2. Stuff with ginger. 3. Steam 10 min. 4. Pour hot oil.", ImageName = "steamed_fish.jpg" }
    };

    private static readonly List<MockReviewData> MockReviews = new()
    {
        new MockReviewData { Id = 1, RestaurantId = 1, UserComment = "Great food at affordable prices!", Rating = 5 },
        new MockReviewData { Id = 2, RestaurantId = 1, UserComment = "Good variety but crowded during lunch.", Rating = 4 },
        new MockReviewData { Id = 3, RestaurantId = 2, UserComment = "The Wuchang fish is absolutely delicious!", Rating = 5 },
        new MockReviewData { Id = 4, RestaurantId = 3, UserComment = "Spicy and authentic! Love the hotpot.", Rating = 4 },
        new MockReviewData { Id = 5, RestaurantId = 4, UserComment = "Best hand-pulled noodles in the area!", Rating = 5 },
        new MockReviewData { Id = 6, RestaurantId = 5, UserComment = "Perfect for late night cravings.", Rating = 4 },
        new MockReviewData { Id = 7, RestaurantId = 6, UserComment = "Love the dim sum selection.", Rating = 5 },
        new MockReviewData { Id = 8, RestaurantId = 7, UserComment = "Authentic Muslim cuisine.", Rating = 4 }
    };

    public async Task<(string FoodName, string Category, int Calories, string Description, byte[] MockImage)> MockCaptureAndRecognizeAsync()
    {
        await Task.Delay(_random.Next(800, 1500));
        var food = MockFoods[_random.Next(MockFoods.Count)];
        var mockImage = GenerateMockImage(food.Name);
        return (food.Name, food.Category, food.Calories, food.Description, mockImage);
    }

    public async Task<(double Latitude, double Longitude)> MockGetLocationAsync()
    {
        await Task.Delay(_random.Next(300, 600));
        var lat = CurrentLocation.Latitude + (_random.NextDouble() - 0.5) * 0.001;
        var lon = CurrentLocation.Longitude + (_random.NextDouble() - 0.5) * 0.001;
        CurrentLocation = (lat, lon);
        return CurrentLocation;
    }

    public async Task<List<Restaurant>> MockGetNearbyRestaurantsAsync((double Latitude, double Longitude) currentLocation)
    {
        await Task.Delay(_random.Next(400, 800));
        var userLocation = new Location(currentLocation.Latitude, currentLocation.Longitude);
        var favoriteIds = _localStorage.GetFavoriteRestaurants();

        // Set some default favorites for testing if none exist
        if (favoriteIds.Count == 0)
        {
            favoriteIds = new List<int> { 2, 5, 8 };
            _localStorage.SaveFavoriteRestaurants(favoriteIds);
        }

        return MockRestaurants.Select(mock =>
        {
            var restaurantLoc = new Location(mock.Latitude, mock.Longitude);
            var distance = userLocation.CalculateDistance(restaurantLoc, DistanceUnits.Kilometers);
            return new Restaurant
            {
                Id = mock.Id,
                Name = mock.Name,
                Description = mock.Description,
                ImageName = mock.ImageName,
                Cuisine = mock.Cuisine,
                Rating = mock.Rating,
                Latitude = mock.Latitude,
                Longitude = mock.Longitude,
                Distance = Math.Round(distance, 2),
                IsFavorite = favoriteIds.Contains(mock.Id),
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
            };
        }).OrderBy(r => r.Distance).ToList();
    }

    public List<Restaurant> GetAllMockRestaurants()
    {
        var favoriteIds = _localStorage.GetFavoriteRestaurants();
        return MockRestaurants.Select(mock => new Restaurant
        {
            Id = mock.Id,
            Name = mock.Name,
            Description = mock.Description,
            ImageName = mock.ImageName,
            Cuisine = mock.Cuisine,
            Rating = mock.Rating,
            Latitude = mock.Latitude,
            Longitude = mock.Longitude,
            Distance = 0,
            IsFavorite = favoriteIds.Contains(mock.Id),
            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
        }).ToList();
    }

    public async Task<List<Recipe>> MockGetRecipesAsync()
    {
        await Task.Delay(_random.Next(300, 600));
        var favoriteIds = _localStorage.GetFavoriteRecipes();
        return MockRecipes.Select(r => new Recipe
        {
            Id = r.Id,
            Name = r.Name,
            Category = r.Category,
            Rating = r.Rating,
            Description = r.Description,
            Calories = r.Calories,
            PrepTime = r.PrepTime,
            Ingredients = r.Ingredients,
            Instructions = r.Instructions,
            ImageName = r.ImageName,
            IsFavorite = favoriteIds.Contains(r.Id),
            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
        }).ToList();
    }

    public List<Recipe> GetAllMockRecipes()
    {
        var favoriteIds = _localStorage.GetFavoriteRecipes();
        return MockRecipes.Select(r => new Recipe
        {
            Id = r.Id,
            Name = r.Name,
            Category = r.Category,
            Rating = r.Rating,
            Description = r.Description,
            Calories = r.Calories,
            PrepTime = r.PrepTime,
            Ingredients = r.Ingredients,
            Instructions = r.Instructions,
            ImageName = r.ImageName,
            IsFavorite = favoriteIds.Contains(r.Id),
            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
        }).ToList();
    }

    public async Task<List<Review>> MockGetReviewsAsync(int restaurantId)
    {
        await Task.Delay(_random.Next(200, 400));
        return MockReviews.Where(r => r.RestaurantId == restaurantId).Select(r => new Review
        {
            Id = r.Id,
            RestaurantId = r.RestaurantId,
            UserComment = r.UserComment,
            Rating = r.Rating,
            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
        }).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<Recipe> MockShakeForRecipeAsync()
    {
        await Task.Delay(_random.Next(500, 1000));
        var recipe = MockRecipes[_random.Next(MockRecipes.Count)];
        var favoriteIds = _localStorage.GetFavoriteRecipes();
        return new Recipe
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Category = recipe.Category,
            Rating = recipe.Rating,
            Description = recipe.Description,
            Calories = recipe.Calories,
            PrepTime = recipe.PrepTime,
            Ingredients = recipe.Ingredients,
            Instructions = recipe.Instructions,
            ImageName = recipe.ImageName,
            IsFavorite = favoriteIds.Contains(recipe.Id),
            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
        };
    }

    private byte[] GenerateMockImage(string foodName)
    {
        var seed = foodName.GetHashCode();
        var bytes = new byte[2048];
        new Random(seed).NextBytes(bytes);
        return bytes;
    }

    public void SetLocation(double latitude, double longitude)
    {
        CurrentLocation = (latitude, longitude);
    }

    public List<string> GetAllMockFoodNames()
    {
        return MockFoods.Select(f => f.Name).ToList();
    }

    private class MockRestaurantData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Cuisine { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageName { get; set; } = string.Empty;
    }

    private class MockRecipeData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int PrepTime { get; set; }
        public string Ingredients { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
    }

    private class MockReviewData
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string UserComment { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}