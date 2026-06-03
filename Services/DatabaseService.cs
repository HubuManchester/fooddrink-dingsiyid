using SQLite;
using CampusEats.Models;

namespace CampusEats.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

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
            await _database.CreateTableAsync<Dish>();

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

            var dishCount = await _database.Table<Dish>().CountAsync();
            if (dishCount == 0)
            {
                await SeedSampleDishes();
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
        // Coordinates around Wuhan Hubei University: 30.5447, 114.3549
        var restaurants = new List<Restaurant>
        {
            new()
            {
                Name = "Wuhan Hot Dry Noodles",
                Cuisine = "Hubei Cuisine",
                Rating = 4.8,
                Description = "Traditional Wuhan hot dry noodles with sesame paste, pickled vegetables, and chili oil. Authentic local flavor with unique aroma and taste.",
                Latitude = 30.5435,
                Longitude = 114.3535,
                ImageName = "reganmian.jpg"
            },
            new()
            {
                Name = "Luojia Hill Canteen",
                Cuisine = "Chinese Home Style",
                Rating = 4.5,
                Description = "Hubei University campus canteen offering authentic Hubei dishes at affordable prices, popular among students.",
                Latitude = 30.5447,
                Longitude = 114.3549,
                ImageName = "rice_bowl.jpg"
            },
            new()
            {
                Name = "McDonald's",
                Cuisine = "Western Fast Food",
                Rating = 4.3,
                Description = "Classic Big Mac burger, crispy fries, refreshing Coca-Cola. Fast service with quality guarantee.",
                Latitude = 30.5460,
                Longitude = 114.3560,
                ImageName = "mcdonalds.jpg"
            },
            new()
            {
                Name = "Luckin Coffee",
                Cuisine = "Coffee & Drinks",
                Rating = 4.4,
                Description = "Coconut latte, thick milk latte, tiramisu and various coffee drinks, perfect for study breaks.",
                Latitude = 30.5450,
                Longitude = 114.3550,
                ImageName = "luckin.jpg"
            },
            new()
            {
                Name = "Wuhan Duck Neck",
                Cuisine = "Specialty Snacks",
                Rating = 4.6,
                Description = "Spicy duck neck, famous Wuhan snack with numbing spicy flavor, unforgettable taste.",
                Latitude = 30.5470,
                Longitude = 114.3570,
                ImageName = "noodle_soup.jpg"
            },
            new()
            {
                Name = "Tokyo Sushi",
                Cuisine = "Japanese Cuisine",
                Rating = 4.7,
                Description = "Fresh sushi and sashimi using premium ingredients, chef with 30 years of experience.",
                Latitude = 30.5480,
                Longitude = 114.3580,
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

    public async Task<SaveResult> SaveRestaurantAsync(Restaurant restaurant)
    {
        await EnsureInitialized();

        if (restaurant.Id > 0)
        {
            // Update existing restaurant
            var rows = await _database.UpdateAsync(restaurant);
            return SaveResult.UpdateSuccess(rows);
        }
        else
        {
            // Insert new restaurant
            var nameConflict = await _database.Table<Restaurant>()
                .Where(r => r.Name == restaurant.Name)
                .FirstOrDefaultAsync();

            if (nameConflict != null)
            {
                return SaveResult.Conflict(nameConflict,
                    $"Restaurant name '{restaurant.Name}' is already used by ID {nameConflict.Id}. " +
                    $"Please use a different name.");
            }

            var rows = await _database.InsertAsync(restaurant);
            return SaveResult.InsertSuccess(rows);
        }
    }

    public async Task<int> UpdateRestaurantAsync(Restaurant restaurant)
    {
        await EnsureInitialized();
        return await _database.UpdateAsync(restaurant);
    }

    public async Task<int> DeleteRestaurantAsync(Restaurant restaurant)
    {
        await EnsureInitialized();
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
            new()
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
            new()
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
            new()
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
            new()
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
            new()
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
            new()
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

    #region Dish Methods

    private async Task SeedSampleDishes()
    {
        var dishes = new List<Dish>
        {
            new()
            {
                RestaurantId = 1,
                Name = "Traditional Hot Dry Noodles",
                Description = "Wuhan specialty hot dry noodles with sesame paste, pickled vegetables, chili oil and green onions. Authentic local flavor, perfect breakfast choice.",
                Price = 8.50,
                Category = "Main Course",
                IsSpicy = true,
                SpiceLevel = 2
            },
            new()
            {
                RestaurantId = 1,
                Name = "Beef Hot Dry Noodles",
                Description = "Hot dry noodles topped with tender sliced beef, spicy and delicious.",
                Price = 12.00,
                Category = "Main Course",
                IsSpicy = true,
                SpiceLevel = 3
            },
            new()
            {
                RestaurantId = 2,
                Name = "Rice Bowl Combo",
                Description = "Rice with your choice of two dishes and complimentary soup. A complete and satisfying meal at an affordable price. This is a longer description to test text wrapping behavior in various screen orientations.",
                Price = 6.50,
                Category = "Combo",
                IsSpicy = false
            },
            new()
            {
                RestaurantId = 2,
                Name = "Egg Fried Rice",
                Description = "Classic Chinese egg fried rice with fresh vegetables. A simple yet delicious vegetarian option.",
                Price = 5.00,
                Category = "Main Course",
                IsVegetarian = true
            },
            new()
            {
                RestaurantId = 2,
                Name = "Spicy Tofu",
                Description = "Home-style spicy braised tofu with Sichuan peppercorns.",
                Price = 4.00,
                Category = "Side Dish",
                IsSpicy = true,
                SpiceLevel = 2,
                IsVegetarian = true
            },
            new()
            {
                RestaurantId = 3,
                Name = "Big Mac",
                Description = "Two all-beef patties, special sauce, lettuce, cheese, pickles, onions on a sesame seed bun.",
                Price = 18.00,
                Category = "Burger"
            },
            new()
            {
                RestaurantId = 3,
                Name = "McNuggets",
                Description = "Crispy chicken nuggets with your choice of dipping sauce.",
                Price = 12.00,
                Category = "Appetizer"
            },
            new()
            {
                RestaurantId = 3,
                Name = "French Fries",
                Description = "Golden crispy fries.",
                Price = 6.00,
                Category = "Side Dish"
            },
            new()
            {
                RestaurantId = 4,
                Name = "Coconut Latte",
                Description = "Rich espresso with coconut milk and ice.",
                Price = 15.00,
                Category = "Coffee"
            },
            new()
            {
                RestaurantId = 4,
                Name = "Thick Milk Latte",
                Description = "Creamy latte with extra milk foam.",
                Price = 14.00,
                Category = "Coffee"
            },
            new()
            {
                RestaurantId = 4,
                Name = "Matcha Croissant",
                Description = "Flaky croissant filled with matcha cream.",
                Price = 8.00,
                Category = "Pastry"
            },
            new()
            {
                RestaurantId = 5,
                Name = "International Platter",
                Description = "A grand platter featuring appetizers from around the world including Italian bruschetta, Japanese edamame, Mexican guacamole, Greek tzatziki, Indian samosas, and Chinese spring rolls. This substantial dish is perfect for sharing among friends and family, offering a culinary journey across multiple continents in a single meal.",
                Price = 58.00,
                Category = "Appetizer"
            },
            new()
            {
                RestaurantId = 5,
                Name = "World Pasta Combo",
                Description = "Choose from penne, spaghetti, or fettuccine with your choice of sauces including Bolognese, Carbonara, Marinara, Pesto, or Alfredo. Comes with garlic bread and a choice of soup or salad. Served with complimentary beverages.",
                Price = 28.00,
                Category = "Main Course"
            },
            new()
            {
                RestaurantId = 6,
                Name = "Spicy Duck Neck",
                Description = "Spicy duck neck, a famous Wuhan snack that's absolutely addictive and full of flavor. MUST TRY!",
                Price = 15.00,
                Category = "Specialty",
                IsSpicy = true,
                SpiceLevel = 4
            },
            new()
            {
                RestaurantId = 6,
                Name = "Spicy Chicken Giblets",
                Description = "Sichuan style spicy chicken giblets with peanuts and dried chili.",
                Price = 22.00,
                Category = "Main Course",
                IsSpicy = true,
                SpiceLevel = 3
            },
            new()
            {
                RestaurantId = 7,
                Name = "Margherita Pizza",
                Description = "Classic Italian pizza with tomato sauce, fresh mozzarella, and basil. Rated best seller!",
                Price = 15.00,
                Category = "Pizza"
            },
            new()
            {
                RestaurantId = 7,
                Name = "Pepperoni Pizza",
                Description = "Double pepperoni with mozzarella cheese. Special 20% OFF for students!",
                Price = 18.00,
                Category = "Pizza"
            },
            new()
            {
                RestaurantId = 8,
                Name = "Sushi Platter",
                Description = "Fresh sashimi and sushi made with premium ingredients imported directly from Tsukiji Fish Market in Tokyo.",
                Price = 68.00,
                Category = "Sushi"
            },
            new()
            {
                RestaurantId = 8,
                Name = "Sashimi Combo",
                Description = "Premium tuna, bonito, and sweet shrimp sashimi. Chef's special selection.",
                Price = 45.00,
                Category = "Sashimi"
            },
            new()
            {
                RestaurantId = 9,
                Name = "Bibimbap",
                Description = "Mixed rice bowl with vegetables, beef, egg, and spicy gochujang sauce. Korean traditional dish that's both healthy and delicious.",
                Price = 18.00,
                Category = "Korean"
            },
            new()
            {
                RestaurantId = 9,
                Name = "Kimchi Jjigae",
                Description = "Traditional Korean kimchi stew with pork.",
                Price = 15.00,
                Category = "Korean",
                IsSpicy = true,
                SpiceLevel = 2
            },
            new()
            {
                RestaurantId = 10,
                Name = "Long Description Test",
                Description = "This is an extremely long dish description designed specifically for testing text truncation and wrapping behavior across different screen orientations and font scaling levels. The dish features a complex preparation method involving multiple cooking techniques including marinating, slow cooking, grilling, and sauce reduction. Ingredients include premium meats, fresh vegetables, aromatic herbs, and specialty spices sourced from different regions around the world.",
                Price = 88.00,
                Category = "Test"
            },
            new()
            {
                RestaurantId = 10,
                Name = "Multi-language Mix",
                Description = "This dish description contains multiple languages and emoji for testing purposes.",
                Price = 66.00,
                Category = "Test"
            }
        };

        foreach (var d in dishes)
        {
            await _database.InsertAsync(d);
        }
    }

    public async Task<List<Dish>> GetDishesForRestaurantAsync(int restaurantId)
    {
        await EnsureInitialized();
        return await _database.Table<Dish>()
            .Where(d => d.RestaurantId == restaurantId)
            .OrderBy(d => d.Category)
            .ThenBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Dish?> GetDishByIdAsync(int id)
    {
        await EnsureInitialized();
        return await _database.Table<Dish>().Where(d => d.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveDishAsync(Dish dish)
    {
        await EnsureInitialized();
        return dish.Id == 0 ? await _database.InsertAsync(dish) : await _database.UpdateAsync(dish);
    }

    public async Task<int> DeleteDishAsync(Dish dish)
    {
        await EnsureInitialized();
        return await _database.DeleteAsync(dish);
    }

    public async Task<int> DeleteDishByIdAsync(int dishId)
    {
        await EnsureInitialized();
        return await _database.DeleteAsync<Dish>(dishId);
    }

    #endregion
}