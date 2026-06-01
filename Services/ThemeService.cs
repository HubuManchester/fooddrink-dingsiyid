using CommunityToolkit.Mvvm.ComponentModel;

namespace CampusEats.Services;

/// <summary>
/// 主题管理服务 - 负责管理应用的深色/浅色模式切换
/// 支持三种模式：跟随系统、浅色模式、深色模式
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
    /// 加载保存的主题设置
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
    /// 切换主题
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
    /// 设置指定主题
    /// </summary>
    public void SetTheme(AppTheme theme)
    {
        SelectedTheme = theme;
        ApplyTheme(theme);
        SaveTheme();
    }

    /// <summary>
    /// 应用主题到应用程序
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
    /// 保存主题设置到偏好设置
    /// </summary>
    private void SaveTheme()
    {
        Preferences.Set(ThemeKey, SelectedTheme.ToString());
    }

    /// <summary>
    /// 获取当前主题的显示名称
    /// </summary>
    public string GetThemeDisplayName()
    {
        return SelectedTheme switch
        {
            AppTheme.System => "跟随系统",
            AppTheme.Light => "浅色模式",
            AppTheme.Dark => "深色模式",
            _ => "跟随系统"
        };
    }

    /// <summary>
    /// 获取主题图标
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
