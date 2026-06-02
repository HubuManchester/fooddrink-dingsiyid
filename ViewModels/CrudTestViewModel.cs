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

    private void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        TestLog.Insert(0, $"[{timestamp}] {message}");
        // Keep only last 50 logs
        if (TestLog.Count > 50)
        {
            TestLog.RemoveAt(50);
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

            // Use manual ID if specified
            if (UseManualId && ManualRestaurantId.HasValue)
            {
                restaurant.Id = ManualRestaurantId.Value;
                Log($"📝 Creating restaurant with manual ID: {restaurant.Id}, Name: {restaurant.Name}");
            }
            else
            {
                Log($"📝 Creating restaurant with auto ID, Name: {restaurant.Name}");
            }

            var result = await _databaseService.SaveRestaurantAsync(restaurant);
            
            if (result.Status == SaveStatus.Conflict)
            {
                Log($"⚠️  CONFLICT DETECTED!");
                Log($"   {result.Message}");
                if (result.ConflictingItem is Restaurant existingRestaurant)
                {
                    Log($"   Conflicting restaurant details:");
                    Log($"     - ID: {existingRestaurant.Id}");
                    Log($"     - Name: {existingRestaurant.Name}");
                    Log($"     - Cuisine: {existingRestaurant.Cuisine}");
                    Log($"     - Rating: {existingRestaurant.Rating:F1}");
                }
                StatusMessage = $"Conflict: {result.Message}";
                
                // Ask user how to proceed
                var choice = await Application.Current!.MainPage!.DisplayAlert(
                    "ID Conflict Detected",
                    result.Message + "\n\nDo you want to update the existing restaurant?",
                    "Yes, Update",
                    "Cancel");
                
                if (choice)
                {
                    // Update the existing one
                    restaurant.Id = ((Restaurant)result.ConflictingItem!).Id;
                    var updateResult = await _databaseService.SaveRestaurantAsync(restaurant);
                    Log($"✅ Updated existing restaurant, status: {updateResult.Status}");
                    StatusMessage = "Restaurant updated successfully!";
                }
                else
                {
                    return;
                }
            }
            else
            {
                Log($"✅ Save completed - Status: {result.Status}, Rows affected: {result.RowsAffected}");
                Log($"   Restaurant ID after save: {restaurant.Id}");
                StatusMessage = result.Message;
            }

            // Clear form
            NewRestaurantName = string.Empty;
            NewRestaurantCuisine = string.Empty;
            NewRestaurantRating = 4.0;
            NewRestaurantDescription = string.Empty;
            ManualRestaurantId = null;

            await LoadAllData();
            StatusMessage = "Restaurant added successfully!";
        }
        catch (Exception ex)
        {
            Log($"❌ Error: {ex.Message}");
            StatusMessage = $"Add failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestDuplicateId()
    {
        if (Restaurants.Count == 0)
        {
            Log("❌ No restaurants to test! Please load data first.");
            return;
        }

        var testId = Restaurants[0].Id;
        Log($"🧪 Starting duplicate ID test for ID: {testId}");
        Log($"   Original restaurant: {Restaurants[0].Name}");

        IsBusy = true;
        try
        {
            // Try to add a new restaurant with the same ID
            var duplicateRestaurant = new Restaurant
            {
                Id = testId,
                Name = "DUPLICATE - This Should Overwrite",
                Cuisine = "Test",
                Rating = 1.0,
                Description = "This is a duplicate ID test",
                Latitude = 0,
                Longitude = 0,
                ImageName = "food_placeholder.jpg"
            };

            Log($"📝 Attempting to save with ID {testId}...");
            var result = await _databaseService.SaveRestaurantAsync(duplicateRestaurant);
            
            Log($"✅ Save returned: {result}");
            
            await LoadAllData();
            
            var afterSave = await _databaseService.GetRestaurantByIdAsync(testId);
            if (afterSave != null)
            {
                Log($"📋 After operation - Restaurant name: {afterSave.Name}");
                Log($"⚠️  CONCLUSION: Data was OVERWRITTEN (Update), no error thrown!");
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Exception caught: {ex.Message}");
            Log($"⚠️  CONCLUSION: Error was thrown!");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestEdgeCases()
    {
        Log("🧪 Starting comprehensive edge case testing...");
        
        IsBusy = true;
        try
        {
            // Test 1: ID = 0
            Log("\n=== Test 1: ID = 0 ===");
            var test1 = new Restaurant { Name = "Test Zero ID", Id = 0, Cuisine = "Test" };
            var result1 = await _databaseService.SaveRestaurantAsync(test1);
            Log($"   Result: {result1}, New ID: {test1.Id}");

            // Test 2: Negative ID
            Log("\n=== Test 2: Negative ID ===");
            var test2 = new Restaurant { Name = "Test Negative ID", Id = -999, Cuisine = "Test" };
            var result2 = await _databaseService.SaveRestaurantAsync(test2);
            Log($"   Result: {result2}, New ID: {test2.Id}");

            // Test 3: Very large ID
            Log("\n=== Test 3: Very Large ID ===");
            var test3 = new Restaurant { Name = "Test Large ID", Id = 999999, Cuisine = "Test" };
            var result3 = await _databaseService.SaveRestaurantAsync(test3);
            Log($"   Result: {result3}, New ID: {test3.Id}");

            // Test 4: Multiple sequential adds
            Log("\n=== Test 4: Multiple Sequential Adds ===");
            for (int i = 1; i <= 3; i++)
            {
                var test = new Restaurant { Name = $"Batch Test {i}", Cuisine = "Test" };
                await _databaseService.SaveRestaurantAsync(test);
                Log($"   Added: {test.Name}, ID: {test.Id}");
            }

            await LoadAllData();
            Log("\n✅ All edge case tests completed!");
        }
        catch (Exception ex)
        {
            Log($"\n❌ Edge case test failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestNameConflict()
    {
        Log("🧪 Starting name conflict testing...");
        if (Restaurants.Count == 0)
        {
            Log("❌ No restaurants to test! Please load data first.");
            return;
        }

        IsBusy = true;
        try
        {
            var existingName = Restaurants[0].Name;
            Log($"📝 Attempting to add a new restaurant with existing name: '{existingName}'");

            var conflictRestaurant = new Restaurant
            {
                Name = existingName,
                Cuisine = "Test Cuisine",
                Rating = 3.0,
                Description = "This should trigger name conflict",
                ImageName = "food_placeholder.jpg"
            };

            var result = await _databaseService.SaveRestaurantAsync(conflictRestaurant);

            if (result.Status == SaveStatus.Conflict)
            {
                Log($"✅ Conflict detected correctly!");
                Log($"   {result.Message}");
                if (result.ConflictingItem is Restaurant conflict)
                {
                    Log($"   Existing: ID={conflict.Id}, Name={conflict.Name}");
                }
            }
            else
            {
                Log($"⚠️ Unexpected: Save succeeded with status {result.Status}");
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Exception: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestUpdateNonExistent()
    {
        Log("🧪 Starting non-existent ID update test...");
        IsBusy = true;
        try
        {
            Log("📝 Attempting to update a non-existent restaurant (ID: 99999)...");
            var ghost = new Restaurant
            {
                Id = 99999,
                Name = "Ghost Restaurant",
                Cuisine = "Ghost",
                Rating = 1.0,
                Description = "This should not exist"
            };

            var rows = await _databaseService.UpdateRestaurantAsync(ghost);
            Log($"   Update returned: {rows} rows affected");
            if (rows == 0)
            {
                Log("✅ Update correctly returned 0 rows for non-existent ID");
            }
            else
            {
                Log($"⚠️ Unexpected: Update affected {rows} rows");
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Exception: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestDeleteNonExistent()
    {
        Log("🧪 Starting non-existent ID delete test...");
        IsBusy = true;
        try
        {
            Log("📝 Attempting to delete a non-existent restaurant (ID: 88888)...");
            var rows = await _databaseService.DeleteRestaurantByIdAsync(88888);
            Log($"   Delete returned: {rows} rows affected");
            if (rows == 0)
            {
                Log("✅ Delete correctly returned 0 for non-existent ID");
            }
            else
            {
                Log($"⚠️ Unexpected: Delete affected {rows} rows");
            }
        }
        catch (Exception ex)
        {
            Log($"❌ Exception: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestEmptyValues()
    {
        Log("🧪 Testing empty/null values...");
        IsBusy = true;
        try
        {
            Log("\n=== Test: Empty Name ===");
            var emptyName = new Restaurant { Name = "", Cuisine = "Test" };
            try
            {
                var result = await _databaseService.SaveRestaurantAsync(emptyName);
                Log($"   Status: {result.Status}, ID: {emptyName.Id}");
            }
            catch (Exception ex)
            {
                Log($"   Error: {ex.Message}");
            }

            Log("\n=== Test: Whitespace Name ===");
            var wsName = new Restaurant { Name = "   ", Cuisine = "Test" };
            try
            {
                var result = await _databaseService.SaveRestaurantAsync(wsName);
                Log($"   Status: {result.Status}, ID: {wsName.Id}");
            }
            catch (Exception ex)
            {
                Log($"   Error: {ex.Message}");
            }

            Log("\n=== Test: Extreme Rating Values ===");
            var extremeRating = new Restaurant { Name = "Extreme Rating", Cuisine = "Test", Rating = 999.0 };
            var r = await _databaseService.SaveRestaurantAsync(extremeRating);
            Log($"   Status: {r.Status}, ID: {extremeRating.Id}, Rating stored: {extremeRating.Rating}");
        }
        catch (Exception ex)
        {
            Log($"❌ Exception: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestPerformance()
    {
        Log("🧪 Performance test: Adding 50 restaurants...");
        IsBusy = true;
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 50; i++)
            {
                var r = new Restaurant
                {
                    Name = $"Perf Test {i}",
                    Cuisine = "Performance",
                    Rating = 4.0,
                    Description = $"Bulk insert test #{i}"
                };
                await _databaseService.SaveRestaurantAsync(r);
            }
            stopwatch.Stop();
            Log($"✅ Added 50 restaurants in {stopwatch.ElapsedMilliseconds}ms");
            Log($"   Average: {stopwatch.ElapsedMilliseconds / 50.0:F2}ms per insert");
            await LoadAllData();
        }
        catch (Exception ex)
        {
            Log($"❌ Exception: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        TestLog.Clear();
        Log("Test log cleared");
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
