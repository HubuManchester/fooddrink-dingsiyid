using CampusEats.Models;
using CampusEats.Services;
using System.Collections.Generic;

namespace CampusEats;

public static class AppState
{
    private static LocalStorageService? _storageService;
    private static LocalStorageService StorageService => _storageService ??= new LocalStorageService();

    public static Restaurant? SelectedRestaurant { get; set; }
    public static Recipe? SelectedRecipe { get; set; }

    private static List<string> _shoppingList = new();
    public static List<string> ShoppingList
    {
        get => _shoppingList;
        set { _shoppingList = value; SaveShoppingList(); }
    }

    private static List<MealPlanItem> _mealPlan = new();
    public static List<MealPlanItem> MealPlan
    {
        get => _mealPlan;
        set { _mealPlan = value; SaveMealPlan(); }
    }

    private static List<int> _favoriteRecipeIds = new();
    public static List<int> FavoriteRecipeIds
    {
        get => _favoriteRecipeIds;
        set { _favoriteRecipeIds = value; SaveFavoriteRecipeIds(); }
    }

    private static List<int> _favoriteRestaurantIds = new();
    public static List<int> FavoriteRestaurantIds
    {
        get => _favoriteRestaurantIds;
        set { _favoriteRestaurantIds = value; SaveFavoriteRestaurantIds(); }
    }

    public static async Task InitializeAsync()
    {
        _shoppingList = await StorageService.GetShoppingListAsync();
        _mealPlan = await StorageService.GetMealPlanAsync();
        _favoriteRecipeIds = await StorageService.GetFavoriteRecipeIdsAsync();
        _favoriteRestaurantIds = await StorageService.GetFavoriteRestaurantIdsAsync();
    }

    public static void AddToShoppingList(string item)
    {
        if (!_shoppingList.Contains(item))
        {
            _shoppingList.Add(item);
            SaveShoppingList();
        }
    }

    public static void RemoveFromShoppingList(string item)
    {
        _shoppingList.Remove(item);
        SaveShoppingList();
    }

    public static void ClearShoppingList()
    {
        _shoppingList.Clear();
        SaveShoppingList();
    }

    public static void AddToMealPlan(MealPlanItem item)
    {
        _mealPlan.Add(item);
        SaveMealPlan();
    }

    public static void RemoveFromMealPlan(MealPlanItem item)
    {
        _mealPlan.Remove(item);
        SaveMealPlan();
    }

    public static void ClearMealPlan()
    {
        _mealPlan.Clear();
        SaveMealPlan();
    }

    public static void ToggleFavoriteRecipe(int recipeId)
    {
        if (_favoriteRecipeIds.Contains(recipeId))
            _favoriteRecipeIds.Remove(recipeId);
        else
            _favoriteRecipeIds.Add(recipeId);
        SaveFavoriteRecipeIds();
    }

    public static bool IsFavoriteRecipe(int recipeId) => _favoriteRecipeIds.Contains(recipeId);

    public static void ToggleFavoriteRestaurant(int restaurantId)
    {
        if (_favoriteRestaurantIds.Contains(restaurantId))
            _favoriteRestaurantIds.Remove(restaurantId);
        else
            _favoriteRestaurantIds.Add(restaurantId);
        SaveFavoriteRestaurantIds();
    }

    public static bool IsFavoriteRestaurant(int restaurantId) => _favoriteRestaurantIds.Contains(restaurantId);

    private static async void SaveShoppingList() => await StorageService.SaveShoppingListAsync(_shoppingList);
    private static async void SaveMealPlan() => await StorageService.SaveMealPlanAsync(_mealPlan);
    private static async void SaveFavoriteRecipeIds() => await StorageService.SaveFavoriteRecipeIdsAsync(_favoriteRecipeIds);
    private static async void SaveFavoriteRestaurantIds() => await StorageService.SaveFavoriteRestaurantIdsAsync(_favoriteRestaurantIds);
}