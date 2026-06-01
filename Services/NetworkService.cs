using System.ComponentModel;

namespace CampusEats.Services;

/// <summary>
/// Network status service - provides network connection status detection and offline cache management
/// </summary>
public class NetworkService : INotifyPropertyChanged
{
    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsOffline));
        }
    }

    public bool IsOffline => !IsConnected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public NetworkService()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Initial check of network status
        IsConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;

        // Subscribe to network status changes
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        IsConnected = e.NetworkAccess == NetworkAccess.Internet;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Check network connection and display alert
    /// </summary>
    public async Task<bool> CheckConnectionAsync(string message = "Network is currently unavailable, please check your connection")
    {
        if (!IsConnected)
        {
            await Application.Current?.MainPage?.DisplayAlert("Network Error", message, "OK")!;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Get network type description
    /// </summary>
    public string GetNetworkType()
    {
        var profiles = Connectivity.Current.ConnectionProfiles;
        if (profiles.Contains(ConnectionProfile.WiFi))
            return "Wi-Fi";
        if (profiles.Contains(ConnectionProfile.Cellular))
            return "Mobile Data";
        if (profiles.Contains(ConnectionProfile.Ethernet))
            return "Wired Network";
        return "Offline";
    }
}
