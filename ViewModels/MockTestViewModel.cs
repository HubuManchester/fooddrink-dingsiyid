using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class MockTestViewModel : ObservableObject
{
    private readonly MockService _mockService;

    [ObservableProperty]
    private bool isMockEnabled = true;

    [ObservableProperty]
    private string statusMessage = "Mock feature enabled, ready to test!";

    [ObservableProperty]
    private bool hasRecognitionResult;

    [ObservableProperty]
    private string recognizedFoodName = string.Empty;

    [ObservableProperty]
    private string recognizedCategory = string.Empty;

    [ObservableProperty]
    private int recognizedCalories;

    [ObservableProperty]
    private string recognizedDescription = string.Empty;

    [ObservableProperty]
    private bool hasLocation;

    [ObservableProperty]
    private string currentLocation = string.Empty;

    [ObservableProperty]
    private bool hasNearbyRestaurants;

    [ObservableProperty]
    private ObservableCollection<Restaurant> nearbyRestaurants = new();

    [ObservableProperty]
    private bool hasShakeRecipe;

    [ObservableProperty]
    private Recipe shakedRecipe = new();

    public MockTestViewModel(MockService mockService)
    {
        _mockService = mockService;
        IsMockEnabled = mockService.UseMockData;
    }

    partial void OnIsMockEnabledChanged(bool value)
    {
        _mockService.UseMockData = value;
        StatusMessage = value ? "Mock feature enabled!" : "Mock feature disabled!";
    }

    [RelayCommand]
    private async Task MockCapture()
    {
        try
        {
            StatusMessage = "Simulating photo capture...";
            var result = await _mockService.MockCaptureAndRecognizeAsync();
            
            RecognizedFoodName = result.FoodName;
            RecognizedCategory = result.Category;
            RecognizedCalories = result.Calories;
            RecognizedDescription = result.Description;
            HasRecognitionResult = true;
            
            StatusMessage = $"Recognition successful! This is {result.FoodName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Mock failed: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task MockLocation()
    {
        try
        {
            StatusMessage = "Getting location...";
            var location = await _mockService.MockGetLocationAsync();
            
            CurrentLocation = $"Latitude: {location.Latitude:F4}, Longitude: {location.Longitude:F4}";
            HasLocation = true;
            
            StatusMessage = "Location obtained successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Location failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task MockNearbyRestaurants()
    {
        try
        {
            StatusMessage = "Searching nearby restaurants...";
            
            var location = await _mockService.MockGetLocationAsync();
            var restaurants = await _mockService.MockGetNearbyRestaurantsAsync(location);
            
            NearbyRestaurants.Clear();
            foreach (var r in restaurants)
            {
                NearbyRestaurants.Add(r);
            }
            
            HasNearbyRestaurants = NearbyRestaurants.Count > 0;
            StatusMessage = $"Found {NearbyRestaurants.Count} restaurants!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task MockShake()
    {
        try
        {
            StatusMessage = "Shaking...";
            var recipe = await _mockService.MockShakeForRecipeAsync();
            
            ShakedRecipe = recipe;
            HasShakeRecipe = true;
            
            StatusMessage = $"Shook and got: {recipe.Name}!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Shake failed: {ex.Message}";
        }
    }
}
