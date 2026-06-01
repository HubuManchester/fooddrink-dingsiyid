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
                Name = "Re Gan Mian (Hot Dry Noodles)",
                Cuisine = "Hubei Noodles",
                Rating = 4.6,
                Description = "Traditional Wuhan hot dry noodles with sesame paste, pickled vegetables, and chili oil.",
                Latitude = 53.473,
                Longitude = -2.236,
                ImageName = "reganmian.jpg"      // 小写文件名
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

    public async Task<List<Review>> GetReviewsForRestaurantAsync(int restaurantId)
    {
        await EnsureInitialized();
        return await _database.Table<Review>().Where(r => r.RestaurantId == restaurantId).OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<int> SaveReviewAsync(Review review)
    {
        await EnsureInitialized();
        return await _database.InsertAsync(review);
    }

    #region Recipe Methods

    private async Task SeedSampleRecipes()
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Name = "番茄炒蛋",
                Category = "主食",
                Rating = 4.8,
                Description = "经典家常菜，酸甜可口，营养丰富",
                Ingredients = "番茄2个,鸡蛋3个,盐适量,糖少许,葱花",
                Instructions = "1. 番茄切块，鸡蛋打散备用。2. 热锅放油，倒入鸡蛋液炒至凝固盛出。3. 锅中留底油，放入番茄翻炒出汁。4. 加入炒好的鸡蛋，加盐和糖调味。5. 撒葱花出锅。",
                ImageName = "food_placeholder.jpg",
                Calories = 280,
                PrepTime = 15
            },
            new Recipe
            {
                Name = "红烧肉",
                Category = "主食",
                Rating = 4.9,
                Description = "色泽红亮，肥而不腻，入口即化",
                Ingredients = "五花肉500g,生姜,葱段,八角,桂皮,香叶,料酒,生抽,老抽,冰糖",
                Instructions = "1. 五花肉切块焯水。2. 锅中放少许油，加入冰糖炒出糖色。3. 放入五花肉翻炒上色。4. 加入葱姜和香料炒香。5. 加料酒、生抽、老抽调味。6. 加水炖煮1小时。",
                ImageName = "food_placeholder.jpg",
                Calories = 450,
                PrepTime = 70
            },
            new Recipe
            {
                Name = "蔬菜沙拉",
                Category = "素食",
                Rating = 4.5,
                Description = "新鲜蔬菜，健康美味",
                Ingredients = "生菜,番茄,黄瓜,紫甘蓝,沙拉酱,橄榄油",
                Instructions = "1. 各种蔬菜洗净切块。2. 放入碗中，加入沙拉酱和橄榄油。3. 搅拌均匀即可。",
                ImageName = "food_placeholder.jpg",
                Calories = 150,
                PrepTime = 10
            },
            new Recipe
            {
                Name = "提拉米苏",
                Category = "甜点",
                Rating = 4.7,
                Description = "意大利经典甜品，咖啡与马斯卡彭的完美结合",
                Ingredients = "马斯卡彭芝士,手指饼干,咖啡,可可粉,鸡蛋,糖",
                Instructions = "1. 蛋黄加糖打发，加入马斯卡彭芝士搅拌。2. 蛋白打发后加入。3. 手指饼干蘸咖啡后铺在底部。4. 倒入芝士糊，冷藏4小时。5. 撒可可粉装饰。",
                ImageName = "food_placeholder.jpg",
                Calories = 350,
                PrepTime = 60
            },
            new Recipe
            {
                Name = "酸辣汤",
                Category = "汤品",
                Rating = 4.6,
                Description = "酸辣开胃，温暖身心",
                Ingredients = "豆腐,木耳,香菇,鸡蛋,醋,胡椒粉,淀粉",
                Instructions = "1. 食材切好备用。2. 锅中加水烧开，放入食材煮5分钟。3. 加入醋和胡椒粉调味。4. 淀粉勾芡，打入鸡蛋花。5. 出锅淋香油。",
                ImageName = "food_placeholder.jpg",
                Calories = 120,
                PrepTime = 20
            },
            new Recipe
            {
                Name = "珍珠奶茶",
                Category = "饮品",
                Rating = 4.4,
                Description = "香甜可口，Q弹珍珠",
                Ingredients = "红茶,牛奶,珍珠,糖",
                Instructions = "1. 珍珠煮熟备用。2. 红茶冲泡后过滤。3. 加入牛奶和糖调味。4. 放入珍珠即可。",
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