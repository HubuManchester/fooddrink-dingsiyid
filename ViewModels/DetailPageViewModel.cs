using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;
using Microsoft.Maui.ApplicationModel;

namespace CampusEats.ViewModels;

public partial class DetailPageViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly TextToSpeechService _ttsService;
    private readonly ValidationService _validationService;

    [ObservableProperty]
    private Restaurant? restaurant;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private ObservableCollection<Dish> dishes = new();

    [ObservableProperty]
    private ObservableCollection<string> dishCategories = new();

    [ObservableProperty]
    private string selectedDishCategory = "All";

    [ObservableProperty]
    private Dish? selectedDish;

    [ObservableProperty]
    private string newComment = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string pageTitle = "Restaurant Details";

    public List<string> RatingOptions { get; } = new() { "5", "4", "3", "2", "1" };

    [ObservableProperty]
    private string selectedRatingString = "5";

    public DetailPageViewModel(DatabaseService databaseService, TextToSpeechService ttsService, ValidationService validationService)
    {
        _databaseService = databaseService;
        _ttsService = ttsService;
        _validationService = validationService;
        SubmitCommentCommand = new AsyncRelayCommand(SubmitComment, CanSubmitComment);
        TakePhotoCommand = new AsyncRelayCommand(TakePhoto);
        SpeakRestaurantCommand = new AsyncRelayCommand(SpeakRestaurantInfo);
        StopTtsCommand = new RelayCommand(StopTts);
        SpeakDishCommand = new AsyncRelayCommand<Dish>(SpeakDishInfo);
        StopDishTtsCommand = new RelayCommand(StopDishTts);
        AddDishCommand = new AsyncRelayCommand(AddDish);
        EditDishCommand = new AsyncRelayCommand<Dish>(EditDish);
        DeleteDishCommand = new AsyncRelayCommand<Dish>(DeleteDish);

        LoadRestaurant();
    }

    private void LoadRestaurant()
    {
        try
        {
            Restaurant = AppState.SelectedRestaurant;
            if (Restaurant != null)
            {
                PageTitle = Restaurant.Name;
                _ = LoadReviews();
                _ = LoadDishes();
            }
            else
            {
                StatusMessage = "No restaurant selected";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load restaurant: {ex.Message}";
        }
    }

    private async Task LoadDishes()
    {
        if (Restaurant == null) return;
        try
        {
            var dishList = await _databaseService.GetDishesForRestaurantAsync(Restaurant.Id);
            Dishes.Clear();
            DishCategories.Clear();
            DishCategories.Add("All");
            
            var categories = dishList.Select(d => d.Category).Distinct().OrderBy(c => c);
            foreach (var cat in categories)
            {
                DishCategories.Add(cat);
            }
            
            foreach (var d in dishList) Dishes.Add(d);
        }
        catch (Exception)
        {
            StatusMessage = "Failed to load dishes";
        }
    }

    partial void OnSelectedDishCategoryChanged(string value)
    {
        ApplyDishCategoryFilter();
    }

    private async void ApplyDishCategoryFilter()
    {
        if (Restaurant == null) return;
        
        try
        {
            var allDishes = await _databaseService.GetDishesForRestaurantAsync(Restaurant.Id);
            Dishes.Clear();
            
            var filtered = SelectedDishCategory == "All" 
                ? allDishes 
                : allDishes.Where(d => d.Category == SelectedDishCategory).ToList();
            
            foreach (var d in filtered) Dishes.Add(d);
        }
        catch (Exception)
        {
            StatusMessage = "Failed to filter dishes";
        }
    }

    private bool CanSubmitComment()
    {
        return !IsBusy && Restaurant != null;
    }

    public ICommand SubmitCommentCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand GoBackCommand => new AsyncRelayCommand(GoBack);
    public ICommand SpeakRestaurantCommand { get; }
    public ICommand StopTtsCommand { get; }
    public ICommand SpeakDishCommand { get; }
    public ICommand StopDishTtsCommand { get; }
    public ICommand AddDishCommand { get; }
    public ICommand EditDishCommand { get; }
    public ICommand DeleteDishCommand { get; }

    private async Task LoadReviews()
    {
        if (Restaurant == null) return;
        try
        {
            var reviewList = await _databaseService.GetReviewsForRestaurantAsync(Restaurant.Id);
            Reviews.Clear();
            foreach (var r in reviewList) Reviews.Add(r);
        }
        catch (Exception)
        {
            StatusMessage = "Failed to load reviews";
            await SafeDisplayAlertAsync("Error", "Unable to load reviews. Please try again later.");
        }
    }

    private async Task SubmitComment()
    {
        if (Restaurant == null)
        {
            StatusMessage = "Unable to submit: restaurant information not found";
            await SafeDisplayAlertAsync("Error", "Unable to submit review. Restaurant information is missing.");
            return;
        }

        var commentValidation = _validationService.IsValidComment(NewComment);
        if (!commentValidation.IsValid)
        {
            StatusMessage = commentValidation.Message;
            await SafeDisplayAlertAsync("Input Error", commentValidation.Message);
            return;
        }

        if (!int.TryParse(SelectedRatingString, out int ratingValue))
        {
            StatusMessage = "Invalid rating format";
            await SafeDisplayAlertAsync("Input Error", "Please select a valid rating.");
            return;
        }

        var ratingValidation = _validationService.IsValidRating(ratingValue);
        if (!ratingValidation.IsValid)
        {
            StatusMessage = ratingValidation.Message;
            await SafeDisplayAlertAsync("Input Error", ratingValidation.Message);
            return;
        }

        IsBusy = true;
        StatusMessage = "Submitting review...";
        try
        {
            var review = new Review
            {
                RestaurantId = Restaurant.Id,
                UserComment = NewComment.Trim(),
                Rating = ratingValue,
                CreatedAt = DateTime.UtcNow
            };
            await _databaseService.SaveReviewAsync(review);
            await LoadReviews();
            NewComment = string.Empty;
            SelectedRatingString = "5";
            StatusMessage = "Review submitted successfully!";
        }
        catch (Exception)
        {
            StatusMessage = "Failed to save review";
            await SafeDisplayAlertAsync("Error", "Unable to save your review. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TakePhoto()
    {
        if (Restaurant == null)
        {
            await SafeDisplayAlertAsync("Error", "Unable to take photo: restaurant information is missing.");
            return;
        }

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            
            if (status == PermissionStatus.Denied)
            {
                bool openSettings = await SafeDisplayConfirmAsync("Permission Required", 
                    "Camera permission is needed to take photos. Would you like to open app settings to enable it?", 
                    "Yes", "No");
                if (openSettings)
                {
                    await OpenAppSettings();
                }
                return;
            }
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    bool openSettings = await SafeDisplayConfirmAsync("Permission Required", 
                        "Camera permission was not granted. Would you like to open app settings to enable it?", 
                        "Yes", "No");
                    if (openSettings)
                    {
                        await OpenAppSettings();
                    }
                    return;
                }
            }

            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await SafeDisplayAlertAsync("Not Supported", 
                    "Your device does not support camera capture.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null)
            {
                StatusMessage = "Photo capture was cancelled";
                return;
            }

            using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            
            var updatedRestaurant = new Restaurant
            {
                Id = Restaurant.Id,
                Name = Restaurant.Name,
                Cuisine = Restaurant.Cuisine,
                Rating = Restaurant.Rating,
                Description = Restaurant.Description,
                Latitude = Restaurant.Latitude,
                Longitude = Restaurant.Longitude,
                ImageName = Restaurant.ImageName,
                IsFavorite = Restaurant.IsFavorite,
                CreatedAt = Restaurant.CreatedAt,
                ImageData = memoryStream.ToArray(),
                Distance = Restaurant.Distance
            };
            
            await _databaseService.SaveRestaurantAsync(updatedRestaurant);
            Restaurant = updatedRestaurant;
            StatusMessage = "Photo saved successfully!";
        }
        catch (PermissionException)
        {
            StatusMessage = "Camera permission error";
            await SafeDisplayAlertAsync("Permission Error", 
                "Unable to access camera due to permission restrictions.");
        }
        catch (NotImplementedException)
        {
            StatusMessage = "Feature unavailable";
            await SafeDisplayAlertAsync("Not Supported", 
                "Camera functionality is not available on this platform.");
        }
        catch (Exception)
        {
            StatusMessage = "Photo capture failed";
            await SafeDisplayAlertAsync("Error", 
                "An error occurred while capturing photo. Please try again.");
        }
    }

    private async Task GoBack()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Navigation failed: {ex.Message}";
        }
    }

    private async Task OpenAppSettings()
    {
        try
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                #if ANDROID
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                intent.SetData(Android.Net.Uri.Parse($"package:{Android.App.Application.Context.PackageName}"));
                Android.App.Application.Context.StartActivity(intent);
                #endif
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                await Launcher.OpenAsync("app-settings:");
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                await Launcher.OpenAsync("ms-settings:appsfeatures");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to open settings: {ex.Message}";
        }
    }

    private async Task SpeakRestaurantInfo()
    {
        if (Restaurant == null)
        {
            StatusMessage = "No restaurant information to read";
            return;
        }
        
        if (!TextToSpeechService.IsSupported)
        {
            StatusMessage = "Text-to-speech is not available on this device";
            await SafeDisplayAlertAsync("Not Supported", 
                "Text-to-speech is not available on your device.");
            return;
        }

        try
        {
            StatusMessage = "Reading restaurant information...";
            
            // Provide vibration feedback
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(50);
            
            await _ttsService.SpeakRestaurantInfo(
                Restaurant.Name,
                Restaurant.Cuisine,
                Restaurant.Rating,
                Restaurant.Description
            );
            StatusMessage = "Reading complete";
        }
        catch (Exception)
        {
            StatusMessage = "Text-to-speech failed";
            await SafeDisplayAlertAsync("Error", 
                "Unable to read restaurant information. Please try again.");
        }
    }

    private void StopTts()
    {
        _ttsService.Stop();
        StatusMessage = "Reading stopped";
        
        // Provide vibration feedback
        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(30);
    }

    private async Task SpeakDishInfo(Dish? dish)
    {
        if (dish == null)
        {
            StatusMessage = "No dish selected";
            return;
        }
        
        if (!TextToSpeechService.IsSupported)
        {
            StatusMessage = "Text-to-speech is not available on this device";
            await SafeDisplayAlertAsync("Not Supported", 
                "Text-to-speech is not available on your device.");
            return;
        }

        try
        {
            StatusMessage = $"Reading {dish.Name}...";
            
            // Provide vibration feedback
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(50);
            
            var text = $"Dish: {dish.Name}. Price: {dish.PriceDisplay}. Description: {dish.Description}";
            await _ttsService.SpeakAsync(text);
            StatusMessage = "Reading complete";
        }
        catch (Exception)
        {
            StatusMessage = "Text-to-speech failed";
        }
    }

    private void StopDishTts()
    {
        _ttsService.Stop();
        StatusMessage = "Dish reading stopped";
        
        // Provide vibration feedback
        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(30);
    }

    private async Task AddDish()
    {
        if (Restaurant == null)
        {
            await SafeDisplayAlertAsync("Error", "Restaurant information is missing.");
            return;
        }

        try
        {
            if (Application.Current?.MainPage == null) return;

            var name = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Dish", "Enter dish name:", "Add", "Cancel", "New Dish");

            if (string.IsNullOrWhiteSpace(name)) return;

            var description = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Dish", "Enter description:", "Next", "Cancel", "");

            string priceStr = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Dish", "Enter price:", "Save", "Cancel", "10.00");

            if (!double.TryParse(priceStr, out double price) || price < 0)
                price = 10.00;

            var category = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Dish", "Enter category:", "Save", "Cancel", "Main");

            var newDish = new Dish
            {
                RestaurantId = Restaurant.Id,
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Price = price,
                Category = category?.Trim() ?? "Main",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveDishAsync(newDish);
            await LoadDishes();

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(100);

            StatusMessage = $"Dish '{name}' added successfully!";
        }
        catch (Exception)
        {
            StatusMessage = "Failed to add dish";
        }
    }

    private async Task EditDish(Dish? dish)
    {
        if (dish == null) return;

        try
        {
            if (Application.Current?.MainPage == null) return;

            var name = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Dish", "Enter dish name:", "Save", "Cancel", dish.Name);

            if (string.IsNullOrWhiteSpace(name)) return;

            var description = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Dish", "Enter description:", "Next", "Cancel", dish.Description);

            string priceStr = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Dish", "Enter price:", "Save", "Cancel", dish.Price.ToString("F2"));

            if (!double.TryParse(priceStr, out double price) || price < 0)
                price = dish.Price;

            var updatedDish = new Dish
            {
                Id = dish.Id,
                RestaurantId = dish.RestaurantId,
                Name = name.Trim(),
                Description = description?.Trim() ?? dish.Description,
                Price = price,
                Category = dish.Category,
                ImageName = dish.ImageName,
                IsAvailable = dish.IsAvailable,
                IsSpicy = dish.IsSpicy,
                IsVegetarian = dish.IsVegetarian,
                SpiceLevel = dish.SpiceLevel,
                CreatedAt = dish.CreatedAt
            };

            await _databaseService.SaveDishAsync(updatedDish);
            await LoadDishes();

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(50);

            StatusMessage = $"Dish '{name}' updated successfully!";
        }
        catch (Exception)
        {
            StatusMessage = "Failed to edit dish";
        }
    }

    private async Task DeleteDish(Dish? dish)
    {
        if (dish == null) return;

        try
        {
            if (Application.Current?.MainPage == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Delete Dish",
                $"Are you sure you want to delete '{dish.Name}'?",
                "Delete", "Cancel");

            if (!confirm) return;

            await _databaseService.DeleteDishAsync(dish);
            Dishes.Remove(dish);

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));

            StatusMessage = $"Dish '{dish.Name}' deleted successfully!";
        }
        catch (Exception)
        {
            StatusMessage = "Failed to delete dish";
        }
    }

    private async Task SafeDisplayAlertAsync(string title, string message)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
            else
            {
                StatusMessage = message;
            }
        }
        catch
        {
            StatusMessage = message;
        }
    }

    private async Task<bool> SafeDisplayConfirmAsync(string title, string message, string accept, string cancel)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            else
            {
                StatusMessage = message;
                return false;
            }
        }
        catch
        {
            StatusMessage = message;
            return false;
        }
    }
}
