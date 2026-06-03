using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly TextToSpeechService _ttsService;
    private readonly HardwareManager _hardwareManager;

    [ObservableProperty]
    private Recipe? recipe;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSpeaking;

    public string IngredientsDisplay => Recipe?.Ingredients.Replace(",", "\n• ") ?? string.Empty;

    public string InstructionsDisplay => Recipe?.Instructions.Replace(".", ".\n\n") ?? string.Empty;

    public RecipeDetailViewModel(DatabaseService databaseService, TextToSpeechService ttsService, HardwareManager hardwareManager)
    {
        _databaseService = databaseService;
        _ttsService = ttsService;
        _hardwareManager = hardwareManager;
        ToggleFavoriteCommand = new AsyncRelayCommand(ToggleFavorite);
        SpeakRecipeCommand = new AsyncRelayCommand(SpeakRecipe);
        StopSpeakingCommand = new RelayCommand(StopSpeaking);
        AddToShoppingListCommand = new AsyncRelayCommand(AddToShoppingList);
        AddToMealPlanCommand = new AsyncRelayCommand(AddToMealPlan);
        GoBackCommand = new AsyncRelayCommand(GoBack);

        Recipe = AppState.SelectedRecipe;
    }

    public ICommand ToggleFavoriteCommand { get; }
    public ICommand SpeakRecipeCommand { get; }
    public ICommand StopSpeakingCommand { get; }
    public ICommand AddToShoppingListCommand { get; }
    public ICommand AddToMealPlanCommand { get; }
    public ICommand GoBackCommand { get; }

    private async Task ToggleFavorite()
    {
        if (Recipe == null) return;
        Recipe.IsFavorite = !Recipe.IsFavorite;
        await _databaseService.SaveRecipeAsync(Recipe);
        OnPropertyChanged(nameof(Recipe));
    }

    private async Task SpeakRecipe()
    {
        if (Recipe == null) return;
        
        IsSpeaking = true;
        StatusMessage = "🔊 Reading recipe...";
        
        if (_hardwareManager.IsSpeaking)
        {
            _hardwareManager.StopSpeech();
        }
        
        await _hardwareManager.SpeakRecipeInstructions(Recipe);
        
        IsSpeaking = false;
        StatusMessage = "✅ Speech completed";
    }

    private void StopSpeaking()
    {
        _hardwareManager.StopSpeech();
        IsSpeaking = false;
        StatusMessage = "🔇 Speech stopped";
    }

    private async Task AddToShoppingList()
    {
        if (Recipe == null || string.IsNullOrEmpty(Recipe.Ingredients))
        {
            StatusMessage = "No ingredients available";
            return;
        }

        var ingredients = Recipe.Ingredients.Split(',');
        foreach (var ingredient in ingredients)
        {
            var item = ingredient.Trim();
            if (!string.IsNullOrEmpty(item) && !AppState.ShoppingList.Contains(item))
            {
                AppState.ShoppingList.Add(item);
            }
        }

        // Haptic feedback
        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(100);

        StatusMessage = $"Added {ingredients.Length} items to shopping list";
        
        await Application.Current!.MainPage!.DisplayAlert(
            "Success!", 
            $"Added {ingredients.Length} ingredients to your shopping list.", 
            "OK");
    }

    private async Task AddToMealPlan()
    {
        if (Recipe == null) return;

        var mealPlanItem = new MealPlanItem
        {
            RecipeName = Recipe.Name,
            RecipeId = Recipe.Id,
            MealType = "Dinner",
            PlannedDate = DateTime.Today.AddDays(1),
            Calories = Recipe.Calories,
            CreatedAt = DateTime.UtcNow
        };

        AppState.MealPlan.Add(mealPlanItem);

        // Haptic feedback
        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(100);

        StatusMessage = $"Added {Recipe.Name} to meal plan";
        
        await Application.Current!.MainPage!.DisplayAlert(
            "Success!", 
            $"Added '{Recipe.Name}' to your meal plan for {mealPlanItem.PlannedDate:MMM dd}.", 
            "OK");
    }

    private async Task GoBack()
    {
        try
        {
            // First try relative navigation
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            // If relative navigation fails, navigate to hardware test page as fallback
            // This handles the case when coming from hardware test page
            await Shell.Current.GoToAsync("///hardwaretestpage");
        }
    }
}
