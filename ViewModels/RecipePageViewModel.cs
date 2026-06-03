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
    private readonly LocalStorageService _localStorage;
    private readonly HardwareManager _hardwareManager;

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

    public RecipePageViewModel(DatabaseService databaseService, MockService mockService, TextToSpeechService ttsService, LocalStorageService localStorage, HardwareManager hardwareManager)
    {
        _databaseService = databaseService;
        _mockService = mockService;
        _ttsService = ttsService;
        _localStorage = localStorage;
        _hardwareManager = hardwareManager;
        
        // Subscribe to hardware events
        _hardwareManager.ShakeDetected += OnShakeDetected;
        _hardwareManager.FoodRecognized += OnFoodRecognized;
        
        SearchCommand = new AsyncRelayCommand(SearchRecipes);
        RefreshCommand = new AsyncRelayCommand(LoadRecipes);
        FilterCommand = new AsyncRelayCommand<string?>(FilterByCategory);
        ToggleFavoriteCommand = new AsyncRelayCommand<Recipe?>(ToggleFavorite);
        SelectRecipeCommand = new AsyncRelayCommand<Recipe?>(SelectRecipe);
        CaptureAndRecognizeCommand = new AsyncRelayCommand(CaptureAndRecognize);
        ShakeToRecommendCommand = new AsyncRelayCommand(ShakeToRecommend);

        // Force mock data to ensure recipes are shown
        _mockService.UseMockData = true;
        
        // Load data immediately
        LoadCategories();
        _ = LoadRecipes();
        
        // Add temporary test data to ensure UI works
        AddTestRecipes();
    }
    
    private void AddTestRecipes()
    {
        // Add test recipes immediately for UI testing
        if (Recipes.Count == 0)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Recipes.Add(new Recipe { Id = 999, Name = "Kung Pao Chicken", Category = "Staples", Rating = 4.7, Calories = 320, Description = "Famous Sichuan dish, spicy and fragrant." });
                Recipes.Add(new Recipe { Id = 998, Name = "Tomato Egg Stir Fry", Category = "Staples", Rating = 4.5, Calories = 280, Description = "Classic Chinese home cooking." });
                Recipes.Add(new Recipe { Id = 997, Name = "Vegetable Salad", Category = "Vegetarian", Rating = 4.3, Calories = 150, Description = "Fresh and healthy green salad." });
                Recipes.Add(new Recipe { Id = 996, Name = "Tiramisu", Category = "Desserts", Rating = 4.8, Calories = 350, Description = "Italian classic dessert." });
                Recipes.Add(new Recipe { Id = 995, Name = "Bubble Tea", Category = "Beverages", Rating = 4.4, Calories = 280, Description = "Taiwanese milk tea with chewy tapioca." });
            });
        }
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
        var newCategories = new List<string> { "All", "Favorites" };
        newCategories.AddRange(cats);
        
        // Ensure UI update on main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Categories = newCategories;
        });
    }

    private async Task LoadRecipes()
    {
        IsRefreshing = true;
        try
        {
            List<Recipe> recipeList;
            
            if (_mockService.UseMockData)
            {
                // Use mock data for testing
                recipeList = await _mockService.MockGetRecipesAsync();
            }
            else
            {
                recipeList = await _databaseService.GetRecipesAsync();
            }
            
            // Ensure UI update on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Recipes.Clear();
                foreach (var r in recipeList)
                {
                    Recipes.Add(r);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadRecipes error: {ex.Message}");
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
            List<Recipe> recipeList;
            
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Use LoadRecipes logic
                if (_mockService.UseMockData)
                {
                    recipeList = await _mockService.MockGetRecipesAsync();
                }
                else
                {
                    recipeList = await _databaseService.GetRecipesAsync();
                }
            }
            else
            {
                if (_mockService.UseMockData)
                {
                    // Search in mock data
                    var allRecipes = await _mockService.MockGetRecipesAsync();
                    recipeList = allRecipes.Where(r => 
                        r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        r.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        r.Ingredients.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                else
                {
                    recipeList = await _databaseService.SearchRecipesAsync(SearchText);
                }
            }
            
            // Ensure UI update on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Recipes.Clear();
                foreach (var r in recipeList)
                {
                    Recipes.Add(r);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SearchRecipes error: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task FilterByCategory(string? category = null)
    {
        IsRefreshing = true;
        try
        {
            // Use parameter if provided, otherwise use SelectedCategory
            string filterCategory = category ?? SelectedCategory ?? "All";
            
            List<Recipe> recipeList;

            if (_mockService.UseMockData)
            {
                // Use mock data
                var allRecipes = await _mockService.MockGetRecipesAsync();
                
                if (filterCategory == "All")
                {
                    recipeList = allRecipes;
                }
                else if (filterCategory == "Favorites")
                {
                    recipeList = allRecipes.Where(r => r.IsFavorite).ToList();
                }
                else
                {
                    recipeList = allRecipes.Where(r => r.Category == filterCategory).ToList();
                }
            }
            else
            {
                // Use database
                if (filterCategory == "All")
                {
                    recipeList = await _databaseService.GetRecipesAsync();
                }
                else if (filterCategory == "Favorites")
                {
                    recipeList = await _databaseService.GetFavoriteRecipesAsync();
                }
                else
                {
                    recipeList = await _databaseService.GetRecipesByCategoryAsync(filterCategory);
                }
            }

            // Ensure UI update on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SelectedCategory = filterCategory;
                Recipes.Clear();
                foreach (var r in recipeList)
                {
                    Recipes.Add(r);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FilterByCategory error: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task ToggleFavorite(Recipe? recipe)
    {
        if (recipe != null)
        {
            // Toggle favorite status using local storage
            var favoriteIds = _localStorage.GetFavoriteRecipes();
            if (favoriteIds.Contains(recipe.Id))
            {
                favoriteIds.Remove(recipe.Id);
                recipe.IsFavorite = false;
            }
            else
            {
                favoriteIds.Add(recipe.Id);
                recipe.IsFavorite = true;
            }
            _localStorage.SaveFavoriteRecipes(favoriteIds);
            
            // Also save to database if not using mock data
            if (!_mockService.UseMockData)
            {
                await _databaseService.SaveRecipeAsync(recipe);
            }
            
            // Update UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var index = Recipes.IndexOf(recipe);
                if (index >= 0)
                {
                    Recipes[index] = recipe;
                }
            });
        }
    }

    private async Task SelectRecipe(Recipe? recipe)
    {
        if (recipe != null)
        {
            AppState.SelectedRecipe = recipe;
            await Shell.Current.GoToAsync("///recipedetailpage");
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
                    // Create new temporary recipe with more meaningful content
                    matchedRecipe = new Recipe
                    {
                        Name = mockResult.FoodName,
                        Category = mockResult.Category,
                        Rating = 4.5,
                        Description = mockResult.Description,
                        Calories = mockResult.Calories,
                        PrepTime = 30,
                        Ingredients = "This food was recognized but no recipe is available in our database. Please check back later for recipe details.",
                        Instructions = "Recipe instructions will be available soon. Meanwhile, you can search for similar recipes or check our recipe section.",
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
                    await Shell.Current.GoToAsync("///recipedetailpage");
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
                                Ingredients = "This food was recognized but no recipe is available in our database. Please check back later for recipe details.",
                                Instructions = "Recipe instructions will be available soon. Meanwhile, you can search for similar recipes or check our recipe section.",
                                ImageName = "food_placeholder.jpg",
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            AppState.SelectedRecipe = matchedRecipe;
                            await Shell.Current.GoToAsync("///recipedetailpage");
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
                await Shell.Current.GoToAsync("///recipedetailpage");
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

    private async void OnShakeDetected(object? sender, ShakeDetectedEventArgs e)
    {
        // Handle shake gesture for recipe recommendation
        if (Recipes.Count > 0 && !IsBusy)
        {
            await ShakeToRecommend();
        }
    }

    private async void OnFoodRecognized(object? sender, FoodRecognizedEventArgs e)
    {
        // Handle food recognition result
        if (e.Result.Success)
        {
            StatusMessage = $"Food recognized: {e.Result.FoodName}";
            
            // Find matching recipe or create new one
            var recipeList = await _databaseService.SearchRecipesAsync(e.Result.FoodName);
            var matchedRecipe = recipeList.FirstOrDefault() ?? new Recipe
            {
                Name = e.Result.FoodName,
                Category = e.Result.Category,
                Rating = 4.5,
                Description = e.Result.Description,
                Calories = e.Result.Calories,
                PrepTime = 30,
                Ingredients = "See details",
                Instructions = "See details",
                ImageData = e.Result.ImageData,
                CreatedAt = DateTime.UtcNow
            };
            
            // Speak the recognition result
            await _ttsService.SpeakAsync($"I found {e.Result.FoodName}. It has {e.Result.Calories} calories.");
            
            // Navigate to recipe detail
            AppState.SelectedRecipe = matchedRecipe;
            await Shell.Current.GoToAsync("///recipedetailpage");
        }
        else
        {
            StatusMessage = $"Recognition failed: {e.Result.ErrorMessage}";
        }
    }
}
