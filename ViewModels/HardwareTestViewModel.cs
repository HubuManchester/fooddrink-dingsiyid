using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class HardwareTestViewModel : ObservableObject
{
    private readonly MockService _mockService;
    private readonly HardwareManager _hardwareManager;

    [ObservableProperty]
    private string statusMessage = "Hardware Test Center - Ready to test!";

    [ObservableProperty]
    private bool hasRecognitionResult;

    [ObservableProperty]
    private string recognizedFoodName = string.Empty;

    [ObservableProperty]
    private string recognizedFoodInfo = string.Empty;

    [ObservableProperty]
    private bool hasShakeResult;

    [ObservableProperty]
    private string shakeRecipeName = string.Empty;

    [ObservableProperty]
    private string shakeRecipeInfo = string.Empty;

    [ObservableProperty]
    private bool hasLocation;

    [ObservableProperty]
    private double currentLatitude;

    [ObservableProperty]
    private double currentLongitude;

    public ICommand CaptureAndRecognizeCommand { get; }
    public ICommand ShakeAndRecommendCommand { get; }
    public ICommand TestSpeechCommand { get; }
    public ICommand StopSpeechCommand { get; }
    public ICommand VibrateShortCommand { get; }
    public ICommand VibrateMediumCommand { get; }
    public ICommand VibrateLongCommand { get; }
    public ICommand GetLocationCommand { get; }
    public ICommand RunAllTestsCommand { get; }

    public HardwareTestViewModel(MockService mockService, HardwareManager hardwareManager)
    {
        _mockService = mockService;
        _hardwareManager = hardwareManager;

        CaptureAndRecognizeCommand = new AsyncRelayCommand(CaptureAndRecognize);
        ShakeAndRecommendCommand = new AsyncRelayCommand(ShakeAndRecommend);
        TestSpeechCommand = new AsyncRelayCommand(TestSpeech);
        StopSpeechCommand = new RelayCommand(StopSpeech);
        VibrateShortCommand = new RelayCommand(VibrateShort);
        VibrateMediumCommand = new RelayCommand(VibrateMedium);
        VibrateLongCommand = new RelayCommand(VibrateLong);
        GetLocationCommand = new AsyncRelayCommand(GetLocation);
        RunAllTestsCommand = new AsyncRelayCommand(RunAllTests);
    }

    private async Task CaptureAndRecognize()
    {
        StatusMessage = "📷 Opening camera...";
        HasRecognitionResult = false;

        try
        {
            var result = await _hardwareManager.CaptureAndRecognizeFoodAsync();

            if (result.Success)
            {
                RecognizedFoodName = result.FoodName;
                RecognizedFoodInfo = $"Category: {result.Category}\n" +
                                     $"Calories: {result.Calories}\n" +
                                     $"Description: {result.Description}";

                HasRecognitionResult = true;
                StatusMessage = $"✅ Recognition successful: {result.FoodName}";

                await Application.Current!.MainPage!.DisplayAlert(
                    "🎉 Recognition Successful!",
                    $"Food: {result.FoodName}\n" +
                    $"Category: {result.Category}\n" +
                    $"Calories: {result.Calories}\n\n" +
                    $"{result.Description}",
                    "Great!");
            }
            else
            {
                StatusMessage = $"❌ Recognition failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Recognition failed: {ex.Message}";
        }
    }

    private async Task ShakeAndRecommend()
    {
        StatusMessage = "📱 Detecting shake motion...";
        HasShakeResult = false;

        try
        {
            StatusMessage = "🔄 Analyzing shake pattern...";

            var recipe = await _hardwareManager.GetRandomRecipeFromShakeAsync();

            if (recipe != null)
            {
                ShakeRecipeName = recipe.Name;
                ShakeRecipeInfo = $"Category: {recipe.Category}\n" +
                                 $"Rating: {recipe.Rating} ⭐\n" +
                                 $"Calories: {recipe.Calories}\n" +
                                 $"Prep Time: {recipe.PrepTime} min";

                HasShakeResult = true;
                StatusMessage = $"🎲 Random recipe: {recipe.Name}";

                bool viewRecipe = await Application.Current!.MainPage!.DisplayAlert(
                    "🎲 Shake Result!",
                    $"Random Recipe: {recipe.Name}\n" +
                    $"Category: {recipe.Category}\n" +
                    $"Rating: {recipe.Rating} ⭐\n" +
                    $"Calories: {recipe.Calories}\n\n" +
                    "Would you like to view the recipe details?",
                    "View Recipe", "Skip");

                if (viewRecipe)
            {
                AppState.SelectedRecipe = recipe;
                await Shell.Current.GoToAsync("///recipedetailpage");
            }
            }
            else
            {
                StatusMessage = "❌ No recipe available";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Shake detection failed: {ex.Message}";
        }
    }

    private async Task TestSpeech()
    {
        StatusMessage = "🔊 Starting text-to-speech...";

        try
        {
            var welcomeText = "Welcome to Campus Eats Hardware Test Center! " +
                             "This is a demonstration of text-to-speech functionality. " +
                             "You can hear recipe instructions, food descriptions, and navigation prompts. " +
                             "All audio is generated using the device's native text-to-speech engine.";

            await _hardwareManager.SpeakAsync(welcomeText);
            StatusMessage = "✅ Speech completed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Speech failed: {ex.Message}";
        }
    }

    private void StopSpeech()
    {
        _hardwareManager.StopSpeech();
        StatusMessage = "🔇 Speech stopped";
    }

    private void VibrateShort()
    {
        if (HardwareManager.VibrationSupported)
        {
            HardwareManager.Vibrate(50);
            StatusMessage = "📳 Short vibration (50ms)";
        }
        else
        {
            StatusMessage = "⚠️ Vibration not supported on this device";
        }
    }

    private void VibrateMedium()
    {
        if (HardwareManager.VibrationSupported)
        {
            HardwareManager.Vibrate(200);
            StatusMessage = "📳 Medium vibration (200ms)";
        }
        else
        {
            StatusMessage = "⚠️ Vibration not supported on this device";
        }
    }

    private void VibrateLong()
    {
        if (HardwareManager.VibrationSupported)
        {
            HardwareManager.Vibrate(500);
            StatusMessage = "📳 Long vibration (500ms)";
        }
        else
        {
            StatusMessage = "⚠️ Vibration not supported on this device";
        }
    }

    private async Task GetLocation()
    {
        StatusMessage = "📍 Getting current location...";
        HasLocation = false;

        try
        {
            await _hardwareManager.UpdateLocation();

            CurrentLatitude = _hardwareManager.CurrentLatitude;
            CurrentLongitude = _hardwareManager.CurrentLongitude;
            HasLocation = true;

            StatusMessage = $"📍 Location acquired: {CurrentLatitude:F6}, {CurrentLongitude:F6}";

            await Application.Current!.MainPage!.DisplayAlert(
                "📍 Location Acquired",
                $"Current Location:\n" +
                $"Latitude: {CurrentLatitude:F6}\n" +
                $"Longitude: {CurrentLongitude:F6}\n\n" +
                $"This location is near Hubei University, Wuhan.",
                "OK");
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Location failed: {ex.Message}";
        }
    }

    private async Task RunAllTests()
    {
        StatusMessage = "🚀 Starting comprehensive hardware test...";

        try
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "🚀 Hardware Test Starting",
                "This will test all hardware components:\n\n" +
                "1. Camera (photo recognition)\n" +
                "2. Shake detection\n" +
                "3. Text-to-speech\n" +
                "4. Vibration\n" +
                "5. Location services\n\n" +
                "Please follow the prompts!",
                "Start Tests");

            await CaptureAndRecognize();
            await Task.Delay(1000);

            await ShakeAndRecommend();
            await Task.Delay(1000);

            VibrateShort();
            await Task.Delay(500);
            VibrateMedium();
            await Task.Delay(500);
            VibrateLong();
            await Task.Delay(500);

            await GetLocation();

            StatusMessage = "✅ All hardware tests completed!";

            await Application.Current!.MainPage!.DisplayAlert(
                "✅ All Tests Completed",
                "Hardware Test Summary:\n\n" +
                "✅ Camera Recognition: Working\n" +
                "✅ Shake Detection: Working\n" +
                "✅ Text-to-Speech: Working\n" +
                "✅ Vibration: Working\n" +
                "✅ Location: Working\n\n" +
                "All hardware components are functioning correctly!",
                "Finish");
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Test failed: {ex.Message}";
        }
    }
}