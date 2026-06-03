using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CampusEats.Models;
using CampusEats.Services;

namespace CampusEats.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly MockService _mockService;
    private readonly NetworkService _networkService;
    private readonly LocalStorageService _localStorage;
    private readonly HardwareManager _hardwareManager;
    private Location? _currentLocation;
    private List<Restaurant> _allRestaurants = new();

    [ObservableProperty]
    private ObservableCollection<Restaurant> restaurants = new();

    [ObservableProperty]
    private ObservableCollection<string> cuisineFilters = new();

    [ObservableProperty]
    private string selectedCuisine = "All Cuisines";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string locationStatus = "Getting your location...";

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ShowLocationCommand { get; }

    public MainPageViewModel(DatabaseService databaseService, MockService mockService, NetworkService networkService, LocalStorageService localStorage, HardwareManager hardwareManager)
    {
        _databaseService = databaseService;
        _mockService = mockService;
        _networkService = networkService;
        _localStorage = localStorage;
        _hardwareManager = hardwareManager;
        
        // Force using mock data to verify UI works correctly
        // Remove this line if database is properly initialized
        _mockService.UseMockData = true;
        
        // Subscribe to hardware events
        _hardwareManager.ShakeDetected += OnShakeDetected;
        _hardwareManager.LocationUpdated += OnLocationUpdated;
        
        CuisineFilters.Add("All Cuisines");
        RefreshCommand = new AsyncRelayCommand(LoadData, () => !IsRefreshing);
        ShowLocationCommand = new AsyncRelayCommand(ShowLocation);
        _ = InitializeDataAsync();
    }
    
    private async Task InitializeDataAsync()
    {
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            LocationStatus = $"Initialization failed: {ex.Message}";
        }
    }

    private async Task LoadData()
    {
        try
        {
            IsRefreshing = true;
            
            await GetCurrentLocation();
            
            List<Restaurant> restaurantList;
            
            if (_mockService.UseMockData)
            {
                var mockLocation = _currentLocation != null 
                    ? (_currentLocation.Latitude, _currentLocation.Longitude)
                    : await _mockService.MockGetLocationAsync();
                
                restaurantList = await _mockService.MockGetNearbyRestaurantsAsync(mockLocation);
                LocationStatus = "📍 Using mock data for nearby restaurants";
            }
            else
            {
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

            _allRestaurants = restaurantList.OrderBy(r => r.Distance).ToList();
            
            CuisineFilters.Clear();
            CuisineFilters.Add("All Cuisines");
            foreach (var cuisine in _allRestaurants.Select(r => r.Cuisine).Distinct().OrderBy(c => c))
            {
                CuisineFilters.Add(cuisine);
            }
            
            ApplyCuisineFilter();
        }
        catch (Exception ex)
        {
            LocationStatus = $"Unable to load restaurants: {ex.Message}";
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
                LocationStatus = $"Unable to get location: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task ShakeToRecommend()
    {
        if (Restaurants.Count == 0)
        {
            LocationStatus = "No restaurants available for recommendation";
            return;
        }

        var rand = new Random();
        var recommended = Restaurants[rand.Next(Restaurants.Count)];

        if (Vibration.Default.IsSupported)
            Vibration.Default.Vibrate(100);

        try
        {
            if (Application.Current?.MainPage == null)
            {
                await NavigateToDetail(recommended);
                return;
            }

            bool goToDetail = await Application.Current.MainPage.DisplayAlert(
                "Shake Recommendation",
                $"{recommended.Name}\n\n{recommended.Cuisine}\nRating: {recommended.Rating:F1}\nDistance: {recommended.Distance:F1} km\n\nView details?",
                "Yes", "No");

            if (goToDetail)
            {
                await NavigateToDetail(recommended);
            }
        }
        catch (Exception ex)
        {
            LocationStatus = $"Recommendation failed: {ex.Message}";
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
        if (restaurant == null) return;
        
        try
        {
            AppState.SelectedRestaurant = restaurant;
            await Shell.Current.GoToAsync("detailpage");
        }
        catch (Exception ex)
        {
            LocationStatus = $"Navigation failed: {ex.Message}";
        }
    }

    partial void OnSelectedCuisineChanged(string value)
    {
        ApplyCuisineFilter();
    }

    private void ApplyCuisineFilter()
    {
        Restaurants.Clear();
        
        var filtered = SelectedCuisine == "All Cuisines" 
            ? _allRestaurants 
            : _allRestaurants.Where(r => r.Cuisine == SelectedCuisine).ToList();
        
        foreach (var r in filtered) Restaurants.Add(r);
    }

    [RelayCommand]
    private void ToggleFavorite(Restaurant restaurant)
    {
        if (restaurant == null) return;

        try
        {
            // Toggle favorite status using local storage
            var favoriteIds = _localStorage.GetFavoriteRestaurants();
            if (favoriteIds.Contains(restaurant.Id))
            {
                favoriteIds.Remove(restaurant.Id);
                restaurant.IsFavorite = false;
            }
            else
            {
                favoriteIds.Add(restaurant.Id);
                restaurant.IsFavorite = true;
            }
            _localStorage.SaveFavoriteRestaurants(favoriteIds);
            
            // Provide haptic feedback
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(50);
        }
        catch (Exception ex)
        {
            LocationStatus = $"Failed to toggle favorite: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddRestaurant()
    {
        try
        {
            if (Application.Current?.MainPage == null) return;

            var name = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Restaurant", "Enter restaurant name:", "Add", "Cancel", "New Restaurant");

            if (string.IsNullOrWhiteSpace(name)) return;

            var cuisine = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Restaurant", "Enter cuisine type:", "Next", "Cancel", "General");

            if (string.IsNullOrWhiteSpace(cuisine)) cuisine = "General";

            var description = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Restaurant", "Enter description:", "Next", "Cancel", "");

            string ratingStr = await Application.Current.MainPage.DisplayPromptAsync(
                "Add Restaurant", "Enter rating (1-5):", "Save", "Cancel", "4.0");

            if (!double.TryParse(ratingStr, out double rating) || rating < 1 || rating > 5)
                rating = 4.0;

            var newRestaurant = new Restaurant
            {
                Name = name.Trim(),
                Cuisine = cuisine.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Rating = rating,
                Latitude = _currentLocation?.Latitude ?? 0,
                Longitude = _currentLocation?.Longitude ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveRestaurantAsync(newRestaurant);
            
            // Reload data
            await LoadData();

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(100);

            LocationStatus = $"Restaurant '{name}' added successfully!";
        }
        catch (Exception ex)
        {
            LocationStatus = $"Failed to add restaurant: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditRestaurant(Restaurant restaurant)
    {
        if (restaurant == null) return;

        try
        {
            if (Application.Current?.MainPage == null) return;

            var name = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Restaurant", "Enter restaurant name:", "Save", "Cancel", restaurant.Name);

            if (string.IsNullOrWhiteSpace(name)) return;

            var cuisine = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Restaurant", "Enter cuisine type:", "Next", "Cancel", restaurant.Cuisine);

            if (string.IsNullOrWhiteSpace(cuisine)) cuisine = restaurant.Cuisine;

            var description = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Restaurant", "Enter description:", "Next", "Cancel", restaurant.Description);

            string ratingStr = await Application.Current.MainPage.DisplayPromptAsync(
                "Edit Restaurant", "Enter rating (1-5):", "Save", "Cancel", restaurant.Rating.ToString("F1"));

            if (!double.TryParse(ratingStr, out double rating) || rating < 1 || rating > 5)
                rating = restaurant.Rating;

            var updatedRestaurant = new Restaurant
            {
                Id = restaurant.Id,
                Name = name.Trim(),
                Cuisine = cuisine.Trim(),
                Description = description?.Trim() ?? restaurant.Description,
                Rating = rating,
                Latitude = restaurant.Latitude,
                Longitude = restaurant.Longitude,
                ImageName = restaurant.ImageName,
                IsFavorite = restaurant.IsFavorite,
                CreatedAt = restaurant.CreatedAt,
                ImageData = restaurant.ImageData
            };

            await _databaseService.SaveRestaurantAsync(updatedRestaurant);
            
            // Update in memory
            var index = Restaurants.IndexOf(restaurant);
            if (index >= 0)
            {
                Restaurants[index] = updatedRestaurant;
            }

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(50);

            LocationStatus = $"Restaurant '{name}' updated successfully!";
        }
        catch (Exception ex)
        {
            LocationStatus = $"Failed to edit restaurant: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteRestaurant(Restaurant restaurant)
    {
        if (restaurant == null) return;

        try
        {
            if (Application.Current?.MainPage == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Delete Restaurant",
                $"Are you sure you want to delete '{restaurant.Name}'?",
                "Delete", "Cancel");

            if (!confirm) return;

            await _databaseService.DeleteRestaurantByIdAsync(restaurant.Id);
            
            // Remove from local list
            Restaurants.Remove(restaurant);
            _allRestaurants.Remove(restaurant);

            // Remove from favorites if present
            var favoriteIds = _localStorage.GetFavoriteRestaurants();
            if (favoriteIds.Contains(restaurant.Id))
            {
                favoriteIds.Remove(restaurant.Id);
                _localStorage.SaveFavoriteRestaurants(favoriteIds);
            }

            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));

            LocationStatus = $"Restaurant '{restaurant.Name}' deleted successfully!";
        }
        catch (Exception ex)
        {
            LocationStatus = $"Failed to delete restaurant: {ex.Message}";
        }
    }

    private async void OnShakeDetected(object? sender, ShakeDetectedEventArgs e)
    {
        try
        {
            // Handle shake gesture for restaurant recommendation
            if (Restaurants.Count > 0 && !IsRefreshing)
            {
                await ShakeToRecommend();
            }
        }
        catch (Exception ex)
        {
            LocationStatus = $"Shake detection error: {ex.Message}";
        }
    }

    private async void OnLocationUpdated(object? sender, LocationUpdatedEventArgs e)
    {
        try
        {
            // Update location and reload nearby restaurants
            _currentLocation = new Location(e.Latitude, e.Longitude);
            await LoadData();
        }
        catch (Exception ex)
        {
            LocationStatus = $"Location update error: {ex.Message}";
        }
    }

    private async Task ShowLocation()
    {
        try
        {
            if (Application.Current?.MainPage == null) return;

            // Show Hubei University location info
            LocationStatus = "📍 Hubei University, Wuhan";
            
            await Application.Current.MainPage.DisplayAlert(
                "📍 Current Location",
                "Hubei University\nWuhan, Hubei Province\nChina",
                "OK");
        }
        catch (Exception ex)
        {
            LocationStatus = $"Failed to show location: {ex.Message}";
        }
    }
}
