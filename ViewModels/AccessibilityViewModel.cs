using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Services;

namespace CampusEats.ViewModels;

/// <summary>
/// 无障碍设置页面视图模型
/// 管理主题切换、字体大小、语音合成、震动反馈等无障碍功能
/// </summary>
public partial class AccessibilityViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private readonly TextToSpeechService _ttsService;

    // 主题相关
    [ObservableProperty]
    private string _themeDisplayName;

    // 字体大小相关
    [ObservableProperty]
    private double _fontScale = 1.0;

    [ObservableProperty]
    private string _fontScaleDisplay = "标准";

    // 语音合成相关
    [ObservableProperty]
    private bool _ttsEnabled = true;

    [ObservableProperty]
    private double _ttsSpeed = 1.0;

    // 震动反馈相关
    [ObservableProperty]
    private bool _vibrationEnabled = true;

    // 命令
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

        // 初始化命令
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        SetLightThemeCommand = new RelayCommand(SetLightTheme);
        SetDarkThemeCommand = new RelayCommand(SetDarkTheme);
        SetSystemThemeCommand = new RelayCommand(SetSystemTheme);
        IncreaseFontCommand = new RelayCommand(IncreaseFont);
        DecreaseFontCommand = new RelayCommand(DecreaseFont);
        ResetFontCommand = new RelayCommand(ResetFont);
        TestTtsCommand = new AsyncRelayCommand(TestTts);
        TestVibrationCommand = new RelayCommand(TestVibration);

        // 初始化状态
        UpdateThemeDisplay();
        UpdateFontScaleDisplay();

        // 订阅主题变化
        _themeService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_themeService.SelectedTheme))
            {
                UpdateThemeDisplay();
            }
        };
    }

    /// <summary>
    /// 更新主题显示名称
    /// </summary>
    private void UpdateThemeDisplay()
    {
        ThemeDisplayName = _themeService.GetThemeDisplayName();
    }

    /// <summary>
    /// 切换主题
    /// </summary>
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }

    /// <summary>
    /// 设置浅色主题
    /// </summary>
    private void SetLightTheme()
    {
        _themeService.SetTheme(ThemeService.AppTheme.Light);
    }

    /// <summary>
    /// 设置深色主题
    /// </summary>
    private void SetDarkTheme()
    {
        _themeService.SetTheme(ThemeService.AppTheme.Dark);
    }

    /// <summary>
    /// 设置跟随系统主题
    /// </summary>
    private void SetSystemTheme()
    {
        _themeService.SetTheme(ThemeService.AppTheme.System);
    }

    /// <summary>
    /// 更新字体大小显示
    /// </summary>
    private void UpdateFontScaleDisplay()
    {
        FontScaleDisplay = FontScale switch
        {
            < 0.9 => "小",
            < 1.1 => "标准",
            < 1.3 => "大",
            < 1.5 => "较大",
            _ => "最大"
        };
    }

    /// <summary>
    /// 字体大小变化时触发
    /// </summary>
    partial void OnFontScaleChanged(double value)
    {
        // 应用字体缩放
        Application.Current?.Resources["DefaultFontSize"] = value * 14;
        UpdateFontScaleDisplay();
    }

    /// <summary>
    /// 增大字体
    /// </summary>
    private void IncreaseFont()
    {
        FontScale = Math.Min(2.0, FontScale + 0.1);
    }

    /// <summary>
    /// 减小字体
    /// </summary>
    private void DecreaseFont()
    {
        FontScale = Math.Max(0.8, FontScale - 0.1);
    }

    /// <summary>
    /// 重置字体大小
    /// </summary>
    private void ResetFont()
    {
        FontScale = 1.0;
    }

    /// <summary>
    /// 测试语音合成
    /// </summary>
    private async Task TestTts()
    {
        if (TtsEnabled && _ttsService.IsSupported)
        {
            await _ttsService.SpeakAsync("欢迎使用校园美食应用，这是一个语音合成测试。");
        }
    }

    /// <summary>
    /// 测试震动反馈
    /// </summary>
    private void TestVibration()
    {
        if (VibrationEnabled && Vibration.Default.IsSupported)
        {
            Vibration.Default.Vibrate(500);
        }
    }
}
