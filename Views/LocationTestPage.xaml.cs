using CampusEats.Models;
using CampusEats.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace CampusEats.Views;

public partial class LocationTestPage : ContentPage
{
    private readonly MockService _mockService;
    private (double Lat, double Lon) _testLocation = (39.9042, 116.4074); // Beijing Tiananmen

    public LocationTestPage(MockService mockService)
    {
        InitializeComponent();
        _mockService = mockService;
        _testLocation = mockService.CurrentLocation;
        UpdateLocationLabels();
    }

    private void UpdateLocationLabels()
    {
        LatitudeLabel.Text = $"Latitude: {_testLocation.Lat:F6}";
        LongitudeLabel.Text = $"Longitude: {_testLocation.Lon:F6}";
        LatEntry.Text = _testLocation.Lat.ToString("F6");
        LonEntry.Text = _testLocation.Lon.ToString("F6");
    }

    private async void OnUpdateLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            var loc = await _mockService.MockGetLocationAsync();
            _testLocation = loc;
            UpdateLocationLabels();
            await RefreshRestaurants();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnSetMockLocationClicked(object? sender, EventArgs e)
    {
        if (double.TryParse(LatEntry.Text, out double lat) && 
            double.TryParse(LonEntry.Text, out double lon))
        {
            _testLocation = (lat, lon);
            _mockService.SetLocation(lat, lon);
            UpdateLocationLabels();
            RefreshRestaurants().Wait();
        }
        else
        {
            DisplayAlert("Error", "Please enter valid latitude and longitude values", "OK");
        }
    }

    private async void OnRefreshRestaurantsClicked(object? sender, EventArgs e)
    {
        await RefreshRestaurants();
    }

    private async Task RefreshRestaurants()
    {
        try
        {
            var restaurants = await _mockService.MockGetNearbyRestaurantsAsync(_testLocation);
            RestaurantsCollection.ItemsSource = restaurants;
            
            // Verify sorting
            VerifySorting(restaurants);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void VerifySorting(List<Restaurant> restaurants)
    {
        bool isSorted = true;
        double lastDistance = 0;
        var verificationMsg = new System.Text.StringBuilder();
        verificationMsg.AppendLine("Sorting verification result:");
        verificationMsg.AppendLine();

        for (int i = 0; i < restaurants.Count; i++)
        {
            var r = restaurants[i];
            verificationMsg.AppendLine($"{i+1}. {r.Name}");
            verificationMsg.AppendLine($"   Distance: {r.Distance:F3} km");
            
            if (i > 0 && r.Distance < lastDistance)
            {
                isSorted = false;
            }
            lastDistance = r.Distance;
        }

        verificationMsg.AppendLine();
        verificationMsg.AppendLine(isSorted ? "✅ Sorting is correct!" : "❌ Sorting is incorrect!");
        verificationMsg.AppendLine();
        verificationMsg.AppendLine($"Reference location: {_testLocation.Lat:F4}, {_testLocation.Lon:F4}");
        
        VerificationLabel.Text = verificationMsg.ToString();
    }
}
