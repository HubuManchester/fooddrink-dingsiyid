using System.Text.Json;
using CampusEats.Models;

namespace CampusEats.Services;

public class LocalStorageService
{
    private const string FavoriteRecipesKey = "favorite_recipes";
    private const string FavoriteRestaurantsKey = "favorite_restaurants";
    private const string ShoppingListKey = "shopping_list";
    private const string MealPlanKey = "meal_plan";
    private const string DarkModeKey = "dark_mode";
    private const string FontSizeKey = "font_size";

    // Synchronous methods (original)
    public List<int> GetFavoriteRecipes() => JsonSerializer.Deserialize<List<int>>(Preferences.Get(FavoriteRecipesKey, "[]")) ?? new List<int>();
    public void SaveFavoriteRecipes(List<int> recipeIds) => Preferences.Set(FavoriteRecipesKey, JsonSerializer.Serialize(recipeIds));
    public List<int> GetFavoriteRestaurants() => JsonSerializer.Deserialize<List<int>>(Preferences.Get(FavoriteRestaurantsKey, "[]")) ?? new List<int>();
    public void SaveFavoriteRestaurants(List<int> restaurantIds) => Preferences.Set(FavoriteRestaurantsKey, JsonSerializer.Serialize(restaurantIds));
    public List<string> GetShoppingList() => JsonSerializer.Deserialize<List<string>>(Preferences.Get(ShoppingListKey, "[]")) ?? new List<string>();
    public void SaveShoppingList(List<string> items) => Preferences.Set(ShoppingListKey, JsonSerializer.Serialize(items));
    public List<MealPlanItem> GetMealPlan() => JsonSerializer.Deserialize<List<MealPlanItem>>(Preferences.Get(MealPlanKey, "[]")) ?? new List<MealPlanItem>();
    public void SaveMealPlan(List<MealPlanItem> items) => Preferences.Set(MealPlanKey, JsonSerializer.Serialize(items));
    public bool GetDarkMode() => Preferences.Get(DarkModeKey, false);
    public void SaveDarkMode(bool enabled) => Preferences.Set(DarkModeKey, enabled);
    public double GetFontSize() => Preferences.Get(FontSizeKey, 1.0);
    public void SaveFontSize(double size) => Preferences.Set(FontSizeKey, size);
    public void ClearAll() => Preferences.Clear();

    // Async wrapper methods (new)
    public Task<List<int>> GetFavoriteRecipeIdsAsync() => Task.FromResult(GetFavoriteRecipes());
    public Task SaveFavoriteRecipeIdsAsync(List<int> ids) { SaveFavoriteRecipes(ids); return Task.CompletedTask; }
    public Task<List<int>> GetFavoriteRestaurantIdsAsync() => Task.FromResult(GetFavoriteRestaurants());
    public Task SaveFavoriteRestaurantIdsAsync(List<int> ids) { SaveFavoriteRestaurants(ids); return Task.CompletedTask; }
    public Task<List<string>> GetShoppingListAsync() => Task.FromResult(GetShoppingList());
    public Task SaveShoppingListAsync(List<string> items) { SaveShoppingList(items); return Task.CompletedTask; }
    public Task<List<MealPlanItem>> GetMealPlanAsync() => Task.FromResult(GetMealPlan());
    public Task SaveMealPlanAsync(List<MealPlanItem> items) { SaveMealPlan(items); return Task.CompletedTask; }
}
