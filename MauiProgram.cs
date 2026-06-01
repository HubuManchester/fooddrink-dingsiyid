using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using CampusEats.Services;
using CampusEats.ViewModels;
using CampusEats.Views;

namespace CampusEats;

/// <summary>
/// MAUI application entry class
/// Using MVVM architecture pattern, implementing layered architecture design:
/// - Models: Data model layer (Restaurant, Recipe, Review, etc.)
/// - Services: Service layer (database, theme, text-to-speech, validation, network, etc.)
/// - ViewModels: View model layer (business logic and data binding)
/// - Views: View layer (XAML interface)
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Create MAUI application instance
    /// Configure dependency injection container, register all services, view models and views
    /// </summary>
    /// <returns>MauiApp instance</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()                           // Set main app class
            .UseMauiCommunityToolkit()                   // Integrate CommunityToolkit.Maui
            .ConfigureFonts(fonts =>                      // Configure app fonts
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ========== Service layer registration (Singleton) ==========
        // Database service - provides SQLite local data storage
        builder.Services.AddSingleton<DatabaseService>();
        
        // Theme service - manages dark/light mode switching
        builder.Services.AddSingleton<ThemeService>();
        
        // Text-to-speech service - provides TTS accessibility features
        builder.Services.AddSingleton<TextToSpeechService>();
        
        // Validation service - provides input validation and error handling
        builder.Services.AddSingleton<ValidationService>();
        
        // Network service - detects network status and offline management
        builder.Services.AddSingleton<NetworkService>();
        
        // Mock service - provides simulated camera, location and food recognition
        builder.Services.AddSingleton<MockService>();

        // ========== View model registration (Transient) ==========
        // Main page view model
        builder.Services.AddTransient<MainPageViewModel>();
        
        // Restaurant detail page view model
        builder.Services.AddTransient<DetailPageViewModel>();
        
        // Accessibility settings page view model
        builder.Services.AddTransient<AccessibilityViewModel>();
        
        // Recipe library page view model
        builder.Services.AddTransient<RecipePageViewModel>();
        
        // Recipe detail page view model
        builder.Services.AddTransient<RecipeDetailViewModel>();
        
        // Mock test page view model
        builder.Services.AddTransient<MockTestViewModel>();
        
        // CRUD test page view model
        builder.Services.AddTransient<CrudTestViewModel>();

        // ========== View registration ==========
        // Main page (singleton, maintains state)
        builder.Services.AddSingleton<MainPage>();
        
        // Detail pages (transient, new instance per navigation)
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<AccessibilityPage>();
        builder.Services.AddTransient<RecipePage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<MockTestPage>();
        builder.Services.AddTransient<LocationTestPage>();
        builder.Services.AddTransient<CrudTestPage>();

#if DEBUG
        // Add logging in debug mode
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
