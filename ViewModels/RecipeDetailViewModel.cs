using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly TextToSpeechService _ttsService;

    [ObservableProperty]
    private Recipe? recipe;

    public string IngredientsDisplay => Recipe?.Ingredients.Replace(",", "\n• ") ?? string.Empty;

    public string InstructionsDisplay => Recipe?.Instructions.Replace("。", "。\n\n") ?? string.Empty;

    public RecipeDetailViewModel(DatabaseService databaseService, TextToSpeechService ttsService)
    {
        _databaseService = databaseService;
        _ttsService = ttsService;
        ToggleFavoriteCommand = new AsyncRelayCommand(ToggleFavorite);
        SpeakRecipeCommand = new AsyncRelayCommand(SpeakRecipe);

        Recipe = AppState.SelectedRecipe;
    }

    public ICommand ToggleFavoriteCommand { get; }
    public ICommand SpeakRecipeCommand { get; }

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
        
        if (_ttsService.IsSupported)
        {
            var text = $"食谱：{Recipe.Name}。分类：{Recipe.Category}。评分：{Recipe.Rating}分。简介：{Recipe.Description}。食材：{Recipe.Ingredients}。做法：{Recipe.Instructions}";
            await _ttsService.SpeakAsync(text);
        }
    }
}
