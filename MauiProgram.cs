using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using CampusEats.Services;
using CampusEats.ViewModels;
using CampusEats.Views;

namespace CampusEats;

/// <summary>
/// MAUI 应用程序入口类
/// 采用 MVVM 架构模式，实现分层架构设计：
/// - Models: 数据模型层（Restaurant, Recipe, Review 等）
/// - Services: 服务层（数据库、主题、语音合成、验证、网络等）
/// - ViewModels: 视图模型层（业务逻辑与数据绑定）
/// - Views: 视图层（XAML 界面）
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// 创建 MAUI 应用程序实例
    /// 配置依赖注入容器，注册所有服务、视图模型和视图
    /// </summary>
    /// <returns>MauiApp 实例</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()                           // 设置主应用类
            .UseMauiCommunityToolkit()                   // 集成 CommunityToolkit.Maui
            .ConfigureFonts(fonts =>                      // 配置应用字体
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ========== 服务层注册 (Singleton) ==========
        // 数据库服务 - 提供 SQLite 本地数据存储
        builder.Services.AddSingleton<DatabaseService>();
        
        // 主题服务 - 管理深色/浅色模式切换
        builder.Services.AddSingleton<ThemeService>();
        
        // 语音合成服务 - 提供 TTS 无障碍功能
        builder.Services.AddSingleton<TextToSpeechService>();
        
        // 验证服务 - 提供输入验证和错误处理
        builder.Services.AddSingleton<ValidationService>();
        
        // 网络服务 - 检测网络状态和离线管理
        builder.Services.AddSingleton<NetworkService>();
        
        // 模拟服务 - 提供模拟的摄像头、定位和食物识别
        builder.Services.AddSingleton<MockService>();

        // ========== 视图模型注册 (Transient) ==========
        // 主页面视图模型
        builder.Services.AddTransient<MainPageViewModel>();
        
        // 餐厅详情页视图模型
        builder.Services.AddTransient<DetailPageViewModel>();
        
        // 无障碍设置页视图模型
        builder.Services.AddTransient<AccessibilityViewModel>();
        
        // 食谱库页面视图模型
        builder.Services.AddTransient<RecipePageViewModel>();
        
        // 食谱详情页视图模型
        builder.Services.AddTransient<RecipeDetailViewModel>();
        
        // 模拟测试页视图模型
        builder.Services.AddTransient<MockTestViewModel>();

        // ========== 视图注册 ==========
        // 主页面（单例，保持状态）
        builder.Services.AddSingleton<MainPage>();
        
        // 详情页面（瞬态，每次导航创建新实例）
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<AccessibilityPage>();
        builder.Services.AddTransient<RecipePage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<MockTestPage>();
        builder.Services.AddTransient<LocationTestPage>();

#if DEBUG
        // 调试模式下添加日志记录
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
