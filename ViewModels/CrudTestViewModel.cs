using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class CrudTestViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string statusMessage = "CRUD Test Ready!";

    [ObservableProperty]
    private bool isBusy;

    // Restaurant section
    [ObservableProperty]
    private ObservableCollection<Restaurant> restaurants = new();

    [ObservableProperty]
    private Restaurant? selectedRestaurant;

    [ObservableProperty]
    private string newRestaurantName = string.Empty;

    [ObservableProperty]
    private string newRestaurantCuisine = string.Empty;

    [ObservableProperty]
    private double newRestaurantRating = 4.0;

    [ObservableProperty]
    private string newRestaurantDescription = string.Empty;

    [ObservableProperty]
    private int? manualRestaurantId = null;

    [ObservableProperty]
    private bool useManualId = false;

    // Test log section
    [ObservableProperty]
    private ObservableCollection<string> testLog = new();

    // Review section
    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private Review? selectedReview;

    [ObservableProperty]
    private string newReviewComment = string.Empty;

    [ObservableProperty]
    private int newReviewRating = 5;

    // Recipe section (already complete, but show for completeness)
    [ObservableProperty]
    private ObservableCollection<Recipe> recipes = new();

    [ObservableProperty]
    private Recipe? selectedRecipe;

    public CrudTestViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    private async Task LoadAllData()
    {
        IsBusy = true;
        try
        {
            StatusMessage = "Loading data...";
            
            var restaurantList = await _databaseService.GetRestaurantsAsync();
            Restaurants.Clear();
            foreach (var r in restaurantList)
            {
                Restaurants.Add(r);
            }

            var recipeList = await _databaseService.GetRecipesAsync();
            Recipes.Clear();
            foreach (var r in recipeList)
            {
                Recipes.Add(r);
            }

            // Load reviews for first restaurant if any
            if (Restaurants.Count > 0)
            {
                await LoadReviews(Restaurants[0].Id);
            }

            StatusMessage = $"Loaded {Restaurants.Count} restaurants and {Recipes.Count} recipes!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadReviews(int restaurantId)
    {
        try
        {
            var reviewList = await _databaseService.GetReviewsForRestaurantAsync(restaurantId);
            Reviews.Clear();
            foreach (var r in reviewList)
            {
                Reviews.Add(r);
            }
            StatusMessage = $"Loaded {Reviews.Count} reviews!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load reviews failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddRestaurant()
    {
        if (string.IsNullOrWhiteSpace(NewRestaurantName))
        {
            StatusMessage = "Restaurant name is required!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Adding restaurant...";

            var restaurant = new Restaurant
            {
                Name = NewRestaurantName,
                Cuisine = NewRestaurantCuisine,
                Rating = NewRestaurantRating,
                Description = NewRestaurantDescription,
                Latitude = 53.475,
                Longitude = -2.235,
                ImageName = "food_placeholder.jpg"
            };

            await _databaseService.SaveRestaurantAsync(restaurant);

            // Clear form
            NewRestaurantName = string.Empty;
            NewRestaurantCuisine = string.Empty;
            NewRestaurantRating = 4.0;
            NewRestaurantDescription = string.Empty;

            await LoadAllData();
            StatusMessage = "Restaurant added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Add failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateRestaurant()
    {
        if (SelectedRestaurant == null)
        {
            StatusMessage = "Please select a restaurant first!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Updating restaurant...";

            SelectedRestaurant.Description = $"{SelectedRestaurant.Description} (Updated!)";
            await _databaseService.UpdateRestaurantAsync(SelectedRestaurant);

            await LoadAllData();
            StatusMessage = "Restaurant updated successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Update failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRestaurant()
    {
        if (SelectedRestaurant == null)
        {
            StatusMessage = "Please select a restaurant first!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Deleting restaurant...";

            await _databaseService.DeleteRestaurantAsync(SelectedRestaurant);

            await LoadAllData();
            StatusMessage = "Restaurant deleted successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddReview()
    {
        if (SelectedRestaurant == null)
        {
            StatusMessage = "Please select a restaurant first!";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewReviewComment))
        {
            StatusMessage = "Review comment is required!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Adding review...";

            var review = new Review
            {
                RestaurantId = SelectedRestaurant.Id,
                UserComment = NewReviewComment,
                Rating = NewReviewRating,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveReviewAsync(review);

            NewReviewComment = string.Empty;
            NewReviewRating = 5;

            await LoadReviews(SelectedRestaurant.Id);
            StatusMessage = "Review added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Add review failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateReview()
    {
        if (SelectedReview == null)
        {
            StatusMessage = "Please select a review first!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Updating review...";

            SelectedReview.UserComment = $"{SelectedReview.UserComment} (Updated!)";
            await _databaseService.UpdateReviewAsync(SelectedReview);

            if (SelectedRestaurant != null)
            {
                await LoadReviews(SelectedRestaurant.Id);
            }
            StatusMessage = "Review updated successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Update review failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteReview()
    {
        if (SelectedReview == null)
        {
            StatusMessage = "Please select a review first!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Deleting review...";

            await _databaseService.DeleteReviewAsync(SelectedReview);

            if (SelectedRestaurant != null)
            {
                await LoadReviews(SelectedRestaurant.Id);
            }
            StatusMessage = "Review deleted successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete review failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddRecipe()
    {
        IsBusy = true;
        try
        {
            StatusMessage = "Adding recipe...";

            var recipe = new Recipe
            {
                Name = "Test Recipe " + DateTime.Now.Second,
                Category = "Test",
                Rating = 4.5,
                Description = "A test recipe",
                Ingredients = "Test ingredient 1, Test ingredient 2",
                Instructions = "Step 1: Test\nStep 2: Test",
                ImageName = "food_placeholder.jpg",
                Calories = 200,
                PrepTime = 15
            };

            await _databaseService.SaveRecipeAsync(recipe);
            await LoadAllData();
            StatusMessage = "Recipe added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Add recipe failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRecipe()
    {
        if (SelectedRecipe == null)
        {
            StatusMessage = "Please select a recipe first!";
            return;
        }

        IsBusy = true;
        try
        {
            StatusMessage = "Deleting recipe...";

            await _databaseService.DeleteRecipeAsync(SelectedRecipe);
            await LoadAllData();
            StatusMessage = "Recipe deleted successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete recipe failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetAllData()
    {
        var confirm = await Application.Current!.MainPage!.DisplayAlert("Confirm", 
            "Are you sure you want to reset all data? This will delete everything and re-seed sample data.", 
            "Yes", "No");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            StatusMessage = "Resetting data...";

            // Delete all data
            var allRestaurants = await _databaseService.GetRestaurantsAsync();
            foreach (var r in allRestaurants)
            {
                await _databaseService.DeleteRestaurantAsync(r);
            }

            var allRecipes = await _databaseService.GetRecipesAsync();
            foreach (var r in allRecipes)
            {
                await _databaseService.DeleteRecipeAsync(r);
            }

            // Re-seed will happen automatically on next load
            await LoadAllData();
            StatusMessage = "Data reset complete!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Reset failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
