using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Services;

namespace CampusEats.ViewModels;

/// <summary>
/// Accessibility settings page view model
/// Manages theme switching, font size, text-to-speech, vibration feedback and other accessibility features
/// </summary>
public partial class AccessibilityViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private readonly TextToSpeechService _ttsService;

    // Theme related
    [ObservableProperty]
    private string _themeDisplayName = string.Empty;

    // Font size related
    [ObservableProperty]
    private double _fontScale = 1.0;

    [ObservableProperty]
    private string _fontScaleDisplay = "Standard";

    // Text-to-speech related
    [ObservableProperty]
    private bool _ttsEnabled = true;

    [ObservableProperty]
    private double _ttsSpeed = 1.0;

    // Vibration feedback related
    [ObservableProperty]
    private bool _vibrationEnabled = true;

    // Commands
    public ICommand ToggleThemeCommand { get; }
    public ICommand SetLightThemeCommand { get; }
    public ICommand SetDarkThemeCommand { get; }
    public ICommand SetSystemThemeCommand { get; }
    public ICommand IncreaseFontCommand { get; }
    public ICommand DecreaseFontCommand { get; }
    public ICommand ResetFontCommand { get; }
    public ICommand TestTtsCommand { get; }
    public ICommand TestVibrationCommand { get; }

    public AccessibilityViewModel(ThemeService themeService, TextToSpeechService ttsService)
    {
        _themeService = themeService;
        _ttsService = ttsService;

        // Initialize commands
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        SetLightThemeCommand = new RelayCommand(SetLightTheme);
        SetDarkThemeCommand = new RelayCommand(SetDarkTheme);
        SetSystemThemeCommand = new RelayCommand(SetSystemTheme);
        IncreaseFontCommand = new RelayCommand(IncreaseFont);
        DecreaseFontCommand = new RelayCommand(DecreaseFont);
        ResetFontCommand = new RelayCommand(ResetFont);
        TestTtsCommand = new AsyncRelayCommand(TestTts);
        TestVibrationCommand = new RelayCommand(TestVibration);

        // Initialize state
        UpdateThemeDisplay();
        UpdateFontScaleDisplay();

        // Subscribe to theme changes
        _themeService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_themeService.SelectedTheme))
            {
                UpdateThemeDisplay();
            }
        };
    }

    /// <summary>
    /// Update theme display name
    /// </summary>
    private void UpdateThemeDisplay()
    {
        ThemeDisplayName = _themeService.GetThemeDisplayName();
    }

    /// <summary>
    /// Toggle theme
    /// </summary>
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }

    /// <summary>
    /// Set light theme
    /// </summary>
    private void SetLightTheme()
    {
        _themeService.SetTheme(ThemeService.AppTheme.Light);
    }

    /// <summary>
    /// Set dark theme
    /// </summary>
    private void SetDarkTheme()
    {
        _themeService.SetTheme(ThemeService.AppTheme.Dark);
    }

    /// <summary>
    /// Set system theme
    /// </summary>
    private void SetSystemTheme()
    {
        _themeService.SetTheme(ThemeService.AppTheme.System);
    }

    /// <summary>
    /// Update font size display
    /// </summary>
    private void UpdateFontScaleDisplay()
    {
        FontScaleDisplay = FontScale switch
        {
            < 0.9 => "Small",
            < 1.1 => "Standard",
            < 1.3 => "Large",
            < 1.5 => "Extra Large",
            _ => "Extra Extra Large"
        };
    }

    /// <summary>
    /// Triggered when font scale changes
    /// </summary>
    partial void OnFontScaleChanged(double value)
    {
        // Apply font scaling to all font size resources
        if (Application.Current != null)
        {
            Application.Current.Resources["ExtraSmallFontSize"] = value * 10;
            Application.Current.Resources["SmallFontSize"] = value * 12;
            Application.Current.Resources["DefaultFontSize"] = value * 14;
            Application.Current.Resources["MediumFontSize"] = value * 16;
            Application.Current.Resources["LargeFontSize"] = value * 18;
            Application.Current.Resources["HeaderFontSize"] = value * 20;
            Application.Current.Resources["TitleFontSize"] = value * 24;
            Application.Current.Resources["LargeTitleFontSize"] = value * 28;
        }
        UpdateFontScaleDisplay();
    }
    
    /// <summary>
    /// Increase font size
    /// </summary>
    private void IncreaseFont()
    {
        FontScale = Math.Min(2.0, FontScale + 0.1);
    }

    /// <summary>
    /// Decrease font size
    /// </summary>
    private void DecreaseFont()
    {
        FontScale = Math.Max(0.8, FontScale - 0.1);
    }

    /// <summary>
    /// Reset font size
    /// </summary>
    private void ResetFont()
    {
        FontScale = 1.0;
    }

    /// <summary>
    /// Test text-to-speech
    /// </summary>
    private async Task TestTts()
    {
        if (TtsEnabled && TextToSpeechService.IsSupported)
        {
            await _ttsService.SpeakAsync("Welcome to Campus Eats, this is a text-to-speech test.");
        }
    }

    /// <summary>
    /// Test vibration feedback
    /// </summary>
    private void TestVibration()
    {
        if (VibrationEnabled && Vibration.Default.IsSupported)
        {
            Vibration.Default.Vibrate(500);
        }
    }
}
