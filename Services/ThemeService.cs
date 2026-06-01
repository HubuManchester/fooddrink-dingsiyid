using CommunityToolkit.Mvvm.ComponentModel;

namespace CampusEats.Services;

/// <summary>
/// Theme management service - responsible for managing app dark/light mode switching
/// Supports three modes: follow system, light mode, dark mode
/// </summary>
public partial class ThemeService : ObservableObject
{
    public enum AppTheme
    {
        System,
        Light,
        Dark
    }

    private const string ThemeKey = "SelectedTheme";

    [ObservableProperty]
    private AppTheme _selectedTheme;

    public ThemeService()
    {
        LoadTheme();
    }

    /// <summary>
    /// Load saved theme settings
    /// </summary>
    private void LoadTheme()
    {
        if (Preferences.ContainsKey(ThemeKey))
        {
            var themeString = Preferences.Get(ThemeKey, "System");
            if (Enum.TryParse<AppTheme>(themeString, out var theme))
            {
                SelectedTheme = theme;
            }
        }
        ApplyTheme(SelectedTheme);
    }

    /// <summary>
    /// Toggle theme
    /// </summary>
    public void ToggleTheme()
    {
        switch (SelectedTheme)
        {
            case AppTheme.System:
                SelectedTheme = AppTheme.Light;
                break;
            case AppTheme.Light:
                SelectedTheme = AppTheme.Dark;
                break;
            case AppTheme.Dark:
                SelectedTheme = AppTheme.System;
                break;
        }
        ApplyTheme(SelectedTheme);
        SaveTheme();
    }

    /// <summary>
    /// Set specified theme
    /// </summary>
    public void SetTheme(AppTheme theme)
    {
        SelectedTheme = theme;
        ApplyTheme(theme);
        SaveTheme();
    }

    /// <summary>
    /// Apply theme to application
    /// </summary>
    private void ApplyTheme(AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Light:
                Application.Current!.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Light;
                break;
            case AppTheme.Dark:
                Application.Current!.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Dark;
                break;
            case AppTheme.System:
                Application.Current!.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
                break;
        }
    }

    /// <summary>
    /// Save theme settings to preferences
    /// </summary>
    private void SaveTheme()
    {
        Preferences.Set(ThemeKey, SelectedTheme.ToString());
    }

    /// <summary>
    /// Get display name of current theme
    /// </summary>
    public string GetThemeDisplayName()
    {
        return SelectedTheme switch
        {
            AppTheme.System => "System",
            AppTheme.Light => "Light Mode",
            AppTheme.Dark => "Dark Mode",
            _ => "System"
        };
    }

    /// <summary>
    /// Get theme icon
    /// </summary>
    public string GetThemeIcon()
    {
        return SelectedTheme switch
        {
            AppTheme.System => "🔄",
            AppTheme.Light => "☀️",
            AppTheme.Dark => "🌙",
            _ => "🔄"
        };
    }
}
