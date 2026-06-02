using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class RecipePageViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly MockService _mockService;
    private readonly TextToSpeechService _ttsService;

    [ObservableProperty]
    private ObservableCollection<Recipe> recipes = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedCategory = "All";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private List<string> categories = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public RecipePageViewModel(DatabaseService databaseService, MockService mockService, TextToSpeechService ttsService)
    {
        _databaseService = databaseService;
        _mockService = mockService;
        _ttsService = ttsService;
        SearchCommand = new AsyncRelayCommand(SearchRecipes);
        RefreshCommand = new AsyncRelayCommand(LoadRecipes);
        FilterCommand = new AsyncRelayCommand(FilterByCategory);
        ToggleFavoriteCommand = new AsyncRelayCommand<Tuple<Recipe, bool>?>(ToggleFavorite);
        SelectRecipeCommand = new RelayCommand<Recipe?>(SelectRecipe);
        CaptureAndRecognizeCommand = new AsyncRelayCommand(CaptureAndRecognize);
        ShakeToRecommendCommand = new AsyncRelayCommand(ShakeToRecommend);

        Task.Run(LoadCategories);
        Task.Run(LoadRecipes);
    }

    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand FilterCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand SelectRecipeCommand { get; }
    public ICommand CaptureAndRecognizeCommand { get; }
    public ICommand ShakeToRecommendCommand { get; }

    private async void LoadCategories()
    {
        var cats = await _databaseService.GetCategoriesAsync();
        Categories = new List<string> { "All", "Favorites" };
        Categories.AddRange(cats);
    }

    private async Task LoadRecipes()
    {
        IsRefreshing = true;
        try
        {
            var recipeList = await _databaseService.GetRecipesAsync();
            Recipes.Clear();
            foreach (var r in recipeList)
            {
                Recipes.Add(r);
            }
        }
        catch (Exception)
        {
            // Ignore errors
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task SearchRecipes()
    {
        IsRefreshing = true;
        try
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadRecipes();
            }
            else
            {
                var recipeList = await _databaseService.SearchRecipesAsync(SearchText);
                Recipes.Clear();
                foreach (var r in recipeList)
                {
                    Recipes.Add(r);
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task FilterByCategory()
    {
        IsRefreshing = true;
        try
        {
            List<Recipe> recipeList;

            if (SelectedCategory == "All")
            {
                recipeList = await _databaseService.GetRecipesAsync();
            }
            else if (SelectedCategory == "Favorites")
            {
                recipeList = await _databaseService.GetFavoriteRecipesAsync();
            }
            else
            {
                recipeList = await _databaseService.GetRecipesByCategoryAsync(SelectedCategory);
            }

            Recipes.Clear();
            foreach (var r in recipeList)
            {
                Recipes.Add(r);
            }
        }
        catch (Exception)
        {
            // Ignore errors
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task ToggleFavorite(Tuple<Recipe, bool>? args)
    {
        if (args != null)
        {
            var recipe = args.Item1;
            recipe.IsFavorite = args.Item2;
            await _databaseService.SaveRecipeAsync(recipe);
        }
    }

    private void SelectRecipe(Recipe? recipe)
    {
        if (recipe != null)
        {
            AppState.SelectedRecipe = recipe;
            Shell.Current.GoToAsync("recipecdetailpage");
        }
    }

    private async Task CaptureAndRecognize()
    {
        IsBusy = true;
        StatusMessage = "Preparing to take photo...";

        try
        {
            // Check permission first
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (permissionStatus != PermissionStatus.Granted && !_mockService.UseMockData)
            {
                await Application.Current!.MainPage!.DisplayAlert("Permission Denied", "Camera permission is required to recognize food", "OK");
                IsBusy = false;
                return;
            }

            if (_mockService.UseMockData)
            {
                // Use mock data
                StatusMessage = "Simulating photo recognition...";
                var mockResult = await _mockService.MockCaptureAndRecognizeAsync();

                // Find matching recipe
                var recipeList = await _databaseService.SearchRecipesAsync(mockResult.FoodName);
                Recipe? matchedRecipe = recipeList.FirstOrDefault();

                if (matchedRecipe == null)
                {
                    // Create new temporary recipe
                    matchedRecipe = new Recipe
                    {
                        Name = mockResult.FoodName,
                        Category = mockResult.Category,
                        Rating = 4.5,
                        Description = mockResult.Description,
                        Calories = mockResult.Calories,
                        PrepTime = 30,
                        Ingredients = "See details",
                        Instructions = "See details",
                        ImageName = "food_placeholder.jpg",
                        ImageData = mockResult.MockImage,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Haptic feedback
                if (Vibration.Default.IsSupported)
                    Vibration.Default.Vibrate(200);

                // Show recognition result
                bool goToDetail = await Application.Current!.MainPage!.DisplayAlert(
                    "🎉 Recognition Successful!",
                    $"{mockResult.FoodName}\n\n{mockResult.Description}\n\n{mockResult.Calories} calories\n\nView recipe details?",
                    "Yes", "No");

                if (goToDetail)
                {
                    AppState.SelectedRecipe = matchedRecipe;
                    await Shell.Current.GoToAsync("recipecdetailpage");
                }

                StatusMessage = $"Recognized: {mockResult.FoodName}";
            }
            else
            {
                // Use real camera
                StatusMessage = "Taking photo...";
                
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    
                    if (photo != null)
                    {
                        StatusMessage = "Recognizing food...";
                        
                        // Here you could integrate real AI recognition API
                        // Currently using mock recognition
                        var mockResult = await _mockService.MockCaptureAndRecognizeAsync();
                        
                        // Haptic feedback
                        if (Vibration.Default.IsSupported)
                            Vibration.Default.Vibrate(200);

                        // Show recognition result
                        bool goToDetail = await Application.Current!.MainPage!.DisplayAlert(
                            "🎉 Recognition Successful!",
                            $"{mockResult.FoodName}\n\n{mockResult.Description}\n\n{mockResult.Calories} calories\n\nView recipe details?",
                            "Yes", "No");

                        if (goToDetail)
                        {
                            // Find or create recipe
                            var recipeList = await _databaseService.SearchRecipesAsync(mockResult.FoodName);
                            var matchedRecipe = recipeList.FirstOrDefault() ?? new Recipe
                            {
                                Name = mockResult.FoodName,
                                Category = mockResult.Category,
                                Rating = 4.5,
                                Description = mockResult.Description,
                                Calories = mockResult.Calories,
                                PrepTime = 30,
                                Ingredients = "See details",
                                Instructions = "See details",
                                ImageName = "food_placeholder.jpg",
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            AppState.SelectedRecipe = matchedRecipe;
                            await Shell.Current.GoToAsync("recipecdetailpage");
                        }

                        StatusMessage = $"Recognized: {mockResult.FoodName}";
                    }
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", "Device does not support camera", "OK");
                }
            }
        }
        catch (FeatureNotSupportedException)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Device does not support this feature", "OK");
        }
        catch (PermissionException)
        {
            await Application.Current!.MainPage!.DisplayAlert("Permission Denied", "Please enable camera permission in settings", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShakeToRecommend()
    {
        if (Recipes.Count == 0)
        {
            StatusMessage = "No recipes available to recommend";
            return;
        }

        IsBusy = true;
        StatusMessage = "Shaking...";

        try
        {
            Recipe recommended;

            if (_mockService.UseMockData)
            {
                recommended = await _mockService.MockShakeForRecipeAsync();
            }
            else
            {
                var rand = new Random();
                recommended = Recipes[rand.Next(Recipes.Count)];
            }

            // Haptic feedback
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(300);

            // Show recommendation
            bool goToDetail = await Application.Current!.MainPage!.DisplayAlert(
                "🎲 Shake Recommendation!",
                $"{recommended.Name}\n\n{recommended.Description}\n\nView recipe details?",
                "Yes", "No");

            if (goToDetail)
            {
                AppState.SelectedRecipe = recommended;
                await Shell.Current.GoToAsync("recipecdetailpage");
            }

            StatusMessage = $"Recommended: {recommended.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Recommendation failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
