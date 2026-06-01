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
    private string statusMessage = "模拟功能已启用，可以开始测试！";

    // 拍照识别
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

    // 定位
    [ObservableProperty]
    private bool hasLocation;

    [ObservableProperty]
    private string currentLocation = string.Empty;

    // 附近餐厅
    [ObservableProperty]
    private bool hasNearbyRestaurants;

    [ObservableProperty]
    private ObservableCollection<Restaurant> nearbyRestaurants = new();

    // 摇一摇
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
        StatusMessage = value ? "模拟功能已启用！" : "模拟功能已禁用！";
    }

    [RelayCommand]
    private async Task MockCapture()
    {
        try
        {
            StatusMessage = "正在模拟拍照...";
            var result = await _mockService.MockCaptureAndRecognizeAsync();
            
            RecognizedFoodName = result.FoodName;
            RecognizedCategory = result.Category;
            RecognizedCalories = result.Calories;
            RecognizedDescription = result.Description;
            HasRecognitionResult = true;
            
            StatusMessage = $"识别成功！这是 {result.FoodName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"模拟失败: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("错误", ex.Message, "确定");
        }
    }

    [RelayCommand]
    private async Task MockLocation()
    {
        try
        {
            StatusMessage = "正在获取位置...";
            var location = await _mockService.MockGetLocationAsync();
            
            CurrentLocation = $"纬度: {location.Latitude:F4}, 经度: {location.Longitude:F4}";
            HasLocation = true;
            
            StatusMessage = "位置获取成功！";
        }
        catch (Exception ex)
        {
            StatusMessage = $"定位失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task MockNearbyRestaurants()
    {
        try
        {
            StatusMessage = "正在搜索附近餐厅...";
            
            var location = await _mockService.MockGetLocationAsync();
            var restaurants = await _mockService.MockGetNearbyRestaurantsAsync(location);
            
            NearbyRestaurants.Clear();
            foreach (var r in restaurants)
            {
                NearbyRestaurants.Add(r);
            }
            
            HasNearbyRestaurants = NearbyRestaurants.Count > 0;
            StatusMessage = $"找到 {NearbyRestaurants.Count} 家餐厅！";
        }
        catch (Exception ex)
        {
            StatusMessage = $"搜索失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task MockShake()
    {
        try
        {
            StatusMessage = "摇晃中...";
            var recipe = await _mockService.MockShakeForRecipeAsync();
            
            ShakedRecipe = recipe;
            HasShakeRecipe = true;
            
            StatusMessage = $"摇到了: {recipe.Name}！";
        }
        catch (Exception ex)
        {
            StatusMessage = $"摇晃失败: {ex.Message}";
        }
    }
}
