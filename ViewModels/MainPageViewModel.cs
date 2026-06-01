using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices.Sensors;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly MockService _mockService;
    private readonly NetworkService _networkService;
    private Location? _currentLocation;

    [ObservableProperty]
    private ObservableCollection<Restaurant> restaurants = new();

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string locationStatus = "Getting your location...";

    public MainPageViewModel(DatabaseService databaseService, MockService mockService, NetworkService networkService)
    {
        _databaseService = databaseService;
        _mockService = mockService;
        _networkService = networkService;
        Task.Run(async () => await LoadData());
    }

    [RelayCommand]
    private async Task LoadData()
    {
        IsRefreshing = true;
        try
        {
            await GetCurrentLocation();
            
            List<Restaurant> restaurantList;
            
            if (_mockService.UseMockData)
            {
                // 启用模拟模式，始终使用模拟数据
                var mockLocation = _currentLocation != null 
                    ? (_currentLocation.Latitude, _currentLocation.Longitude)
                    : await _mockService.MockGetLocationAsync();
                
                restaurantList = await _mockService.MockGetNearbyRestaurantsAsync(mockLocation);
                LocationStatus = "📍 Using mock data for nearby restaurants";
            }
            else
            {
                // 不使用模拟数据，使用真实数据库
                restaurantList = await _databaseService.GetRestaurantsAsync();
                
                if (_currentLocation != null)
                {
                    foreach (var r in restaurantList)
                    {
                        var restaurantLocation = new Location(r.Latitude, r.Longitude);
                        r.Distance = _currentLocation.CalculateDistance(restaurantLocation, DistanceUnits.Kilometers);
                    }
                    LocationStatus = "📍 Showing nearby restaurants";
                }
                else
                {
                    foreach (var r in restaurantList)
                    {
                        r.Distance = 0;
                    }
                    LocationStatus = "📍 No location available";
                }
            }

            // 按距离排序并更新UI
            var sorted = restaurantList.OrderBy(r => r.Distance).ToList();
            Restaurants.Clear();
            foreach (var r in sorted) Restaurants.Add(r);
        }
        catch (Exception ex)
        {
            LocationStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task GetCurrentLocation()
    {
        try
        {
            // 先尝试真实定位
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                _currentLocation = await Geolocation.Default.GetLocationAsync(request);
            }
            else if (_mockService.UseMockData)
            {
                // 权限被拒绝时使用模拟位置
                var mockLoc = await _mockService.MockGetLocationAsync();
                _currentLocation = new Location(mockLoc.Latitude, mockLoc.Longitude);
                LocationStatus = "📍 Using mock location";
            }
        }
        catch (FeatureNotSupportedException)
        {
            if (_mockService.UseMockData)
            {
                var mockLoc = await _mockService.MockGetLocationAsync();
                _currentLocation = new Location(mockLoc.Latitude, mockLoc.Longitude);
                LocationStatus = "📍 Using mock location";
            }
            else
            {
                LocationStatus = "GPS not supported";
            }
        }
        catch (PermissionException)
        {
            if (_mockService.UseMockData)
            {
                var mockLoc = await _mockService.MockGetLocationAsync();
                _currentLocation = new Location(mockLoc.Latitude, mockLoc.Longitude);
                LocationStatus = "📍 Using mock location";
            }
            else
            {
                LocationStatus = "Location permission denied";
            }
        }
        catch (Exception ex)
        {
            if (_mockService.UseMockData)
            {
                var mockLoc = await _mockService.MockGetLocationAsync();
                _currentLocation = new Location(mockLoc.Latitude, mockLoc.Longitude);
                LocationStatus = "📍 Using mock location";
            }
            else
            {
                LocationStatus = $"Location error: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task ShakeToRecommend()
    {
        if (Restaurants.Count == 0) return;

        var rand = new Random();
        var recommended = Restaurants[rand.Next(Restaurants.Count)];

        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(100);

        bool goToDetail = await Application.Current!.MainPage!.DisplayAlert(
            "🎲 Shake to Recommend 🎲",
            $"{recommended.Name}\n\n{recommended.Cuisine}\n⭐ {recommended.Rating:F1}\n📍 {recommended.Distance:F1} km away\n\nSee details?",
            "Yes", "No");

        if (goToDetail)
        {
            await NavigateToDetail(recommended);
        }
    }

    [RelayCommand]
    private async Task SelectRestaurant(Restaurant restaurant)
    {
        if (restaurant != null)
        {
            await NavigateToDetail(restaurant);
        }
    }

    private async Task NavigateToDetail(Restaurant restaurant)
    {
        AppState.SelectedRestaurant = restaurant;
        await Shell.Current.GoToAsync("detailpage");
    }

    // Expose RefreshCommand that binds to LoadDataCommand for XAML compatibility
    public IAsyncRelayCommand RefreshCommand => LoadDataCommand;
}