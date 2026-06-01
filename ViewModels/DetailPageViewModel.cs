using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

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
    private string newComment = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    // Rating options for the picker
    public List<string> RatingOptions { get; } = new() { "5", "4", "3", "2", "1" };

    // Selected rating as string (for binding)
    [ObservableProperty]
    private string selectedRatingString = "5";

    public DetailPageViewModel(DatabaseService databaseService, TextToSpeechService ttsService, ValidationService validationService)
    {
        _databaseService = databaseService;
        _ttsService = ttsService;
        _validationService = validationService;
        SubmitCommentCommand = new AsyncRelayCommand(SubmitComment);
        TakePhotoCommand = new AsyncRelayCommand(TakePhoto);
        SpeakRestaurantCommand = new AsyncRelayCommand(SpeakRestaurantInfo);

        // Get the restaurant passed from static variable
        Restaurant = AppState.SelectedRestaurant;
        if (Restaurant != null)
        {
            Task.Run(async () => await LoadReviews());
        }
    }

    public ICommand SubmitCommentCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand GoBackCommand => new AsyncRelayCommand(GoBack);
    public ICommand SpeakRestaurantCommand { get; }

    private async Task LoadReviews()
    {
        if (Restaurant == null) return;
        var reviewList = await _databaseService.GetReviewsForRestaurantAsync(Restaurant.Id);
        Reviews.Clear();
        foreach (var r in reviewList) Reviews.Add(r);
    }

    private async Task SubmitComment()
    {
        // Validate input using validation service
        var commentValidation = _validationService.IsValidComment(NewComment);
        if (!commentValidation.IsValid)
        {
            StatusMessage = commentValidation.Message;
            await Application.Current!.MainPage!.DisplayAlert("Input Validation", commentValidation.Message, "OK");
            return;
        }

        int ratingValue;
        if (!int.TryParse(SelectedRatingString, out ratingValue))
        {
            StatusMessage = "Invalid rating format";
            await Application.Current!.MainPage!.DisplayAlert("Input Validation", "Please select a valid rating", "OK");
            return;
        }

        var ratingValidation = _validationService.IsValidRating(ratingValue);
        if (!ratingValidation.IsValid)
        {
            StatusMessage = ratingValidation.Message;
            await Application.Current!.MainPage!.DisplayAlert("Input Validation", ratingValidation.Message, "OK");
            return;
        }

        if (Restaurant == null)
        {
            StatusMessage = "Cannot get restaurant information";
            await Application.Current!.MainPage!.DisplayAlert("Error", "Cannot get restaurant information", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var review = new Review
            {
                RestaurantId = Restaurant.Id,
                UserComment = NewComment,
                Rating = ratingValue,
                CreatedAt = DateTime.UtcNow
            };
            await _databaseService.SaveReviewAsync(review);
            await LoadReviews();
            NewComment = string.Empty;
            SelectedRatingString = "5";
            StatusMessage = "Comment submitted successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to save comment, please try again", "OK");
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
            await Application.Current!.MainPage!.DisplayAlert("Error", "Cannot get restaurant information", "OK");
            return;
        }

        try
        {
            // Check camera permission
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            
            // If permission denied, guide user to settings
            if (status == PermissionStatus.Denied)
            {
                var result = await Application.Current!.MainPage!.DisplayAlert(
                    "Permission Denied", 
                    "Camera permission is required to take photos. Would you like to go to settings to enable it?", 
                    "Yes", "No");
                if (result)
                {
                    await Permissions.OpenAppSettingsAsync();
                }
                return;
            }
            
            // Request permission
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await Application.Current!.MainPage!.DisplayAlert("Insufficient Permission", "Cannot access camera, please enable permission in settings", "OK");
                return;
            }

            // Check if camera is available
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Application.Current!.MainPage!.DisplayAlert("Hardware Unavailable", "Device does not support camera functionality", "OK");
                return;
            }

            // Take photo
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                
                // Create copy to trigger property change notification
                var updatedRestaurant = Restaurant with
                {
                    ImageData = memoryStream.ToArray()
                };
                
                await _databaseService.SaveRestaurantAsync(updatedRestaurant);
                
                // Update bound property to trigger UI refresh
                Restaurant = updatedRestaurant;
                StatusMessage = "Photo saved successfully!";
            }
            else
            {
                StatusMessage = "Photo cancelled";
            }
        }
        catch (PermissionException ex)
        {
            StatusMessage = "Permission error";
            await Application.Current!.MainPage!.DisplayAlert("Permission Error", $"Failed to get camera permission: {ex.Message}", "OK");
        }
        catch (NotImplementedException)
        {
            StatusMessage = "Feature not implemented yet";
            await Application.Current!.MainPage!.DisplayAlert("Notice", "Camera functionality is not supported on this platform", "OK");
        }
        catch (Exception ex)
        {
            StatusMessage = "Photo capture failed";
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Error occurred while taking photo: {ex.Message}", "OK");
        }
    }

    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Use text-to-speech to read restaurant information
    /// </summary>
    private async Task SpeakRestaurantInfo()
    {
        if (Restaurant == null) return;
        
        if (_ttsService.IsSupported)
        {
            StatusMessage = "Reading restaurant information...";
            await _ttsService.SpeakRestaurantInfo(
                Restaurant.Name,
                Restaurant.Cuisine,
                Restaurant.Rating,
                Restaurant.Description
            );
            StatusMessage = "Reading complete";
        }
        else
        {
            StatusMessage = "Text-to-speech not available";
        }
    }
}