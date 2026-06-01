using SQLite;
using CampusEats.Models;

namespace CampusEats.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

    public DatabaseService()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "campus_eats.db3");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    private async Task EnsureInitialized()
    {
        if (_initialized) return;
        
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            
            await _database.CreateTableAsync<Restaurant>();
            await _database.CreateTableAsync<Review>();
            await _database.CreateTableAsync<Recipe>();

            var restaurantCount = await _database.Table<Restaurant>().CountAsync();
            if (restaurantCount == 0)
            {
                await SeedSampleRestaurants();
            }

            var recipeCount = await _database.Table<Recipe>().CountAsync();
            if (recipeCount == 0)
            {
                await SeedSampleRecipes();
            }
            
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task SeedSampleRestaurants()
    {
        var restaurants = new List<Restaurant>
        {
            new Restaurant
            {
                Name = "McDonald's",
                Cuisine = "Burgers & Fast Food",
                Rating = 4.3,
                Description = "Famous Big Mac, crispy fries, and Coca-Cola. Quick and tasty.",
                Latitude = 53.472,
                Longitude = -2.234,
                ImageName = "mcdonalds.jpg"
            },
            new Restaurant
            {
                Name = "Hot Dry Noodles",
                Cuisine = "Hubei Noodles",
                Rating = 4.6,
                Description = "Traditional Wuhan hot dry noodles with sesame paste, pickled vegetables, and chili oil.",
                Latitude = 53.473,
                Longitude = -2.236,
                ImageName = "reganmian.jpg"
            },
            new Restaurant
            {
                Name = "Luckin Coffee",
                Cuisine = "Coffee & Drinks",
                Rating = 4.4,
                Description = "Popular coconut latte, thick milk latte, and pastries.",
                Latitude = 53.474,
                Longitude = -2.235,
                ImageName = "luckin.jpg"
            },
            new Restaurant
            {
                Name = "Mixue Ice Cream & Tea",
                Cuisine = "Bubble Tea & Desserts",
                Rating = 4.2,
                Description = "Affordable milk tea, lemonade, and soft serve ice cream.",
                Latitude = 53.471,
                Longitude = -2.233,
                ImageName = "mixue.jpg"
            },
            new Restaurant
            {
                Name = "Rice Bowl House",
                Cuisine = "Rice Dishes",
                Rating = 4.5,
                Description = "Various rice bowls with braised pork, chicken, or vegetables.",
                Latitude = 53.470,
                Longitude = -2.237,
                ImageName = "rice_bowl.jpg"
            },
            new Restaurant
            {
                Name = "Noodle Soup King",
                Cuisine = "Noodle Soups",
                Rating = 4.1,
                Description = "Hand-pulled noodles in rich broth, topped with beef or pork.",
                Latitude = 53.475,
                Longitude = -2.238,
                ImageName = "noodle_soup.jpg"
            },
            new Restaurant
            {
                Name = "Pizza Corner",
                Cuisine = "Pizza",
                Rating = 4.7,
                Description = "Italian style thin crust pizza with fresh mozzarella.",
                Latitude = 53.476,
                Longitude = -2.232,
                ImageName = "pizza.jpg"
            },
            new Restaurant
            {
                Name = "Sushi Master",
                Cuisine = "Japanese Sushi",
                Rating = 4.8,
                Description = "Fresh salmon, tuna, and avocado rolls. Served with wasabi and ginger.",
                Latitude = 53.477,
                Longitude = -2.231,
                ImageName = "sushi.jpg"
            }
        };

        foreach (var r in restaurants)
        {
            await _database.InsertAsync(r);
        }
    }

    public async Task<List<Restaurant>> GetRestaurantsAsync()
{
    await EnsureInitialized();
    return await _database.Table<Restaurant>().ToListAsync();
}

    public async Task<Restaurant> GetRestaurantByIdAsync(int id)
    {
        await EnsureInitialized();
        return await _database.Table<Restaurant>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveRestaurantAsync(Restaurant restaurant)
    {
        await EnsureInitialized();
        return restaurant.Id == 0 ? await _database.InsertAsync(restaurant) : await _database.UpdateAsync(restaurant);
    }

    public async Task<int> UpdateRestaurantAsync(Restaurant restaurant)
    {
        await EnsureInitialized();
        return await _database.UpdateAsync(restaurant);
    }

    public async Task<int> DeleteRestaurantAsync(Restaurant restaurant)
    {
        await EnsureInitialized();
        // Delete all reviews for this restaurant first
        var reviews = await _database.Table<Review>().Where(r => r.RestaurantId == restaurant.Id).ToListAsync();
        foreach (var review in reviews)
        {
            await _database.DeleteAsync(review);
        }
        return await _database.DeleteAsync(restaurant);
    }

    public async Task<int> DeleteRestaurantByIdAsync(int restaurantId)
    {
        await EnsureInitialized();
        var restaurant = await _database.Table<Restaurant>().Where(r => r.Id == restaurantId).FirstOrDefaultAsync();
        if (restaurant != null)
        {
            return await DeleteRestaurantAsync(restaurant);
        }
        return 0;
    }

    public async Task<List<Review>> GetReviewsForRestaurantAsync(int restaurantId)
    {
        await EnsureInitialized();
        return await _database.Table<Review>().Where(r => r.RestaurantId == restaurantId).OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<int> SaveReviewAsync(Review review)
    {
        await EnsureInitialized();
        return review.Id == 0 ? await _database.InsertAsync(review) : await _database.UpdateAsync(review);
    }

    public async Task<int> UpdateReviewAsync(Review review)
    {
        await EnsureInitialized();
        return await _database.UpdateAsync(review);
    }

    public async Task<int> DeleteReviewAsync(Review review)
    {
        await EnsureInitialized();
        return await _database.DeleteAsync(review);
    }

    public async Task<int> DeleteReviewByIdAsync(int reviewId)
    {
        await EnsureInitialized();
        var review = await _database.Table<Review>().Where(r => r.Id == reviewId).FirstOrDefaultAsync();
        if (review != null)
        {
            return await _database.DeleteAsync(review);
        }
        return 0;
    }

    #region Recipe Methods

    private async Task SeedSampleRecipes()
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Name = "Tomato Egg Stir Fry",
                Category = "Staples",
                Rating = 4.8,
                Description = "Classic home cooking, sweet and sour, nutritious",
                Ingredients = "2 tomatoes, 3 eggs, salt, sugar, scallion",
                Instructions = "1. Cut tomatoes into pieces, beat eggs. 2. Heat oil, pour egg mixture and cook until set. 3. Leave some oil, add tomatoes and stir-fry. 4. Add cooked eggs, season with salt and sugar. 5. Sprinkle scallion and serve.",
                ImageName = "food_placeholder.jpg",
                Calories = 280,
                PrepTime = 15
            },
            new Recipe
            {
                Name = "Braised Pork",
                Category = "Staples",
                Rating = 4.9,
                Description = "Bright red color, fatty but not greasy, melts in mouth",
                Ingredients = "500g pork belly, ginger, scallion, star anise, cinnamon, bay leaf, cooking wine, light soy sauce, dark soy sauce, rock sugar",
                Instructions = "1. Cut pork into pieces and blanch. 2. Add a little oil, add rock sugar and caramelize. 3. Add pork and stir-fry to color. 4. Add scallion, ginger and spices. 5. Add cooking wine, light and dark soy sauce. 6. Add water and simmer for 1 hour.",
                ImageName = "food_placeholder.jpg",
                Calories = 450,
                PrepTime = 70
            },
            new Recipe
            {
                Name = "Vegetable Salad",
                Category = "Vegetarian",
                Rating = 4.5,
                Description = "Fresh vegetables, healthy and delicious",
                Ingredients = "Lettuce, tomato, cucumber, purple cabbage, salad dressing, olive oil",
                Instructions = "1. Wash and cut all vegetables. 2. Put in bowl, add salad dressing and olive oil. 3. Stir well and serve.",
                ImageName = "food_placeholder.jpg",
                Calories = 150,
                PrepTime = 10
            },
            new Recipe
            {
                Name = "Tiramisu",
                Category = "Desserts",
                Rating = 4.7,
                Description = "Italian classic dessert, perfect combination of coffee and mascarpone",
                Ingredients = "Mascarpone cheese, ladyfingers, coffee, cocoa powder, eggs, sugar",
                Instructions = "1. Beat egg yolks with sugar, mix with mascarpone. 2. Beat egg whites and fold in. 3. Dip ladyfingers in coffee and place at bottom. 4. Pour cheese mixture, refrigerate for 4 hours. 5. Sprinkle cocoa powder on top.",
                ImageName = "food_placeholder.jpg",
                Calories = 350,
                PrepTime = 60
            },
            new Recipe
            {
                Name = "Hot and Sour Soup",
                Category = "Soups",
                Rating = 4.6,
                Description = "Sour and spicy, warms your heart and body",
                Ingredients = "Tofu, wood ear mushrooms, shiitake mushrooms, eggs, vinegar, white pepper, starch",
                Instructions = "1. Prepare all ingredients. 2. Bring water to boil, add ingredients and cook for 5 minutes. 3. Add vinegar and white pepper. 4. Starch thickening, beat in eggs. 5. Drizzle sesame oil and serve.",
                ImageName = "food_placeholder.jpg",
                Calories = 120,
                PrepTime = 20
            },
            new Recipe
            {
                Name = "Bubble Tea",
                Category = "Beverages",
                Rating = 4.4,
                Description = "Sweet and fragrant, chewy pearls",
                Ingredients = "Black tea, milk, tapioca pearls, sugar",
                Instructions = "1. Cook pearls and set aside. 2. Brew black tea and strain. 3. Add milk and sugar. 4. Add pearls and serve.",
                ImageName = "food_placeholder.jpg",
                Calories = 280,
                PrepTime = 30
            }
        };

        foreach (var r in recipes)
        {
            await _database.InsertAsync(r);
        }
    }

    public async Task<List<Recipe>> GetRecipesAsync()
    {
        await EnsureInitialized();
        return await _database.Table<Recipe>().OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<List<Recipe>> GetRecipesByCategoryAsync(string category)
    {
        await EnsureInitialized();
        return await _database.Table<Recipe>()
            .Where(r => r.Category == category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Recipe>> GetFavoriteRecipesAsync()
    {
        await EnsureInitialized();
        return await _database.Table<Recipe>()
            .Where(r => r.IsFavorite)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Recipe>> SearchRecipesAsync(string searchText)
    {
        await EnsureInitialized();
        return await _database.Table<Recipe>()
            .Where(r => r.Name.Contains(searchText) || 
                        r.Description.Contains(searchText) ||
                        r.Ingredients.Contains(searchText))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Recipe> GetRecipeByIdAsync(int id)
    {
        await EnsureInitialized();
        return await _database.Table<Recipe>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveRecipeAsync(Recipe recipe)
    {
        await EnsureInitialized();
        return recipe.Id == 0 ? await _database.InsertAsync(recipe) : await _database.UpdateAsync(recipe);
    }

    public async Task<int> DeleteRecipeAsync(Recipe recipe)
    {
        await EnsureInitialized();
        return await _database.DeleteAsync(recipe);
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        await EnsureInitialized();
        var recipes = await _database.Table<Recipe>().ToListAsync();
        return recipes.Select(r => r.Category).Distinct().ToList();
    }

    #endregion
}
