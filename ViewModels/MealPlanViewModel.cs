using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class MealPlanViewModel : ObservableObject
{
    private readonly LocalStorageService _localStorage;
    private readonly MockService _mockService;
    private readonly HardwareManager _hardwareManager;

    [ObservableProperty]
    private ObservableCollection<MealPlanItem> mealPlanItems = new();

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private string mealType = "Breakfast";

    [ObservableProperty]
    private int totalCalories;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    public List<string> MealTypes => new() { "Breakfast", "Lunch", "Dinner", "Snack" };

    public MealPlanViewModel(LocalStorageService localStorage, MockService mockService, HardwareManager hardwareManager)
    {
        _localStorage = localStorage;
        _mockService = mockService;
        _hardwareManager = hardwareManager;
        LoadMealPlanCommand = new AsyncRelayCommand(LoadMealPlan);
        AddMealCommand = new AsyncRelayCommand(AddMeal);
        RemoveMealCommand = new AsyncRelayCommand<MealPlanItem>(RemoveMeal);
        EditMealCommand = new AsyncRelayCommand<MealPlanItem>(EditMeal);
        SelectDateCommand = new RelayCommand<DateTime>(SelectDate);
        SpeakMealPlanCommand = new AsyncRelayCommand(SpeakMealPlan);
        RefreshCommand = new AsyncRelayCommand(Refresh);

        Task.Run(LoadMealPlan);
    }

    public ICommand LoadMealPlanCommand { get; }
    public ICommand AddMealCommand { get; }
    public ICommand RemoveMealCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand EditMealCommand { get; }
    public ICommand SelectDateCommand { get; }
    public ICommand SpeakMealPlanCommand { get; }

    private async Task Refresh()
    {
        IsRefreshing = true;
        await LoadMealPlan();
        IsRefreshing = false;
    }

    private async Task LoadMealPlan()
    {
        try
        {
            var allItems = await _localStorage.GetMealPlanAsync();
            
            // Ensure UI update on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MealPlanItems.Clear();
                
                foreach (var item in allItems)
                {
                    MealPlanItems.Add(item);
                }
                
                CalculateTotalCalories();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadMealPlan error: {ex.Message}");
            StatusMessage = "Failed to load meal plan";
        }
    }

    private async Task AddMeal()
    {
        try
        {
            var recipes = await _mockService.MockGetRecipesAsync();
            if (recipes.Count == 0)
            {
                StatusMessage = "No recipes available";
                return;
            }

            // Get a random recipe for demonstration
            var randomRecipe = recipes[new Random().Next(recipes.Count)];

            var newItem = new MealPlanItem
            {
                Id = MealPlanItems.Count + 1,
                RecipeName = randomRecipe.Name,
                RecipeId = randomRecipe.Id,
                MealType = MealType,
                PlannedDate = SelectedDate,
                Calories = randomRecipe.Calories,
                CreatedAt = DateTime.UtcNow
            };

            // Ensure UI update on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MealPlanItems.Add(newItem);
            });
            
            await SaveMealPlan();
            
            // Haptic feedback
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(100);

            StatusMessage = $"Added {randomRecipe.Name} to {MealType}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddMeal error: {ex.Message}");
            StatusMessage = "Failed to add meal";
        }
    }

    private async Task RemoveMeal(MealPlanItem? item)
    {
        if (item == null) return;

        // Ensure UI update on main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MealPlanItems.Remove(item);
        });
        
        await SaveMealPlan();
        
        // Haptic feedback
        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(50);

        StatusMessage = $"Removed {item.RecipeName}";
    }

    private async Task EditMeal(MealPlanItem? item)
    {
        if (item == null) return;

        try
        {
            if (Application.Current?.MainPage == null) return;

            // First, let user choose what to edit
            var editOption = await Application.Current.MainPage.DisplayActionSheet(
                "Edit Meal", "Cancel", null, "Edit Name", "Edit Meal Type", "Edit Calories");

            if (editOption == "Cancel" || string.IsNullOrEmpty(editOption))
                return;

            if (editOption == "Edit Name")
            {
                var newName = await Application.Current.MainPage.DisplayPromptAsync(
                    "Edit Recipe Name", "Enter new name:", "OK", "Cancel", 
                    initialValue: item.RecipeName, maxLength: 50);
                
                if (!string.IsNullOrEmpty(newName) && newName != "Cancel")
                {
                    item.RecipeName = newName;
                    await SaveMealPlan();
                    
                    if (Vibration.Default.IsSupported)
                        Vibration.Default.Vibrate(50);

                    StatusMessage = $"Updated name to {newName}";
                }
            }
            else if (editOption == "Edit Meal Type")
            {
                var newMealType = await Application.Current.MainPage.DisplayActionSheet(
                    "Select Meal Type", "Cancel", null, MealTypes.ToArray());

                if (newMealType != "Cancel" && !string.IsNullOrEmpty(newMealType))
                {
                    item.MealType = newMealType;
                    await SaveMealPlan();
                    
                    if (Vibration.Default.IsSupported)
                        Vibration.Default.Vibrate(50);

                    StatusMessage = $"Updated {item.RecipeName} to {newMealType}";
                }
            }
            else if (editOption == "Edit Calories")
            {
                var newCaloriesStr = await Application.Current.MainPage.DisplayPromptAsync(
                    "Edit Calories", "Enter calories:", "OK", "Cancel", 
                    initialValue: item.Calories.ToString(), maxLength: 10, keyboard: Keyboard.Numeric);
                
                if (!string.IsNullOrEmpty(newCaloriesStr) && newCaloriesStr != "Cancel")
                {
                    if (int.TryParse(newCaloriesStr, out int newCalories))
                    {
                        item.Calories = newCalories;
                        await SaveMealPlan();
                        
                        if (Vibration.Default.IsSupported)
                            Vibration.Default.Vibrate(50);

                        StatusMessage = $"Updated calories to {newCalories}";
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid number", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EditMeal error: {ex.Message}");
            StatusMessage = "Failed to edit meal";
        }
    }

    private void SelectDate(DateTime date)
    {
        SelectedDate = date;
    }

    private void CalculateTotalCalories()
    {
        TotalCalories = MealPlanItems.Sum(item => item.Calories);
    }

    private async Task SaveMealPlan()
    {
        var items = MealPlanItems.ToList();
        await _localStorage.SaveMealPlanAsync(items);
        CalculateTotalCalories();
    }

    private async Task SpeakMealPlan()
    {
        if (MealPlanItems.Count == 0)
        {
            StatusMessage = "No meal plan items to speak";
            return;
        }

        var mealPlanText = $"Your meal plan for {SelectedDate.ToShortDateString()}: ";
        
        foreach (var mealType in MealTypes)
        {
            var itemsForMeal = MealPlanItems.Where(item => item.MealType == mealType).ToList();
            if (itemsForMeal.Any())
            {
                mealPlanText += $"{mealType}: ";
                mealPlanText += string.Join(", ", itemsForMeal.Select(item => $"{item.RecipeName} with {item.Calories} calories"));
                mealPlanText += ". ";
            }
        }
        
        mealPlanText += $"Total calories: {TotalCalories}";

        StatusMessage = "🔊 Speaking meal plan...";
        await _hardwareManager.SpeakAsync(mealPlanText);
        StatusMessage = "✅ Meal plan spoken";
    }
}