using System.ComponentModel;

namespace CampusEats.Services;

public partial class NetworkService : INotifyPropertyChanged
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
        try
        {
            IsConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }
        catch
        {
            IsConnected = false;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        try
        {
            IsConnected = e.NetworkAccess == NetworkAccess.Internet;
        }
        catch
        {
            IsConnected = false;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task<bool> CheckConnectionAsync(string message = "Network is currently unavailable. Please check your Wi-Fi or mobile data connection.")
    {
        if (!IsConnected)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Network Error", message, "OK");
                }
            }
            catch (Exception)
            {
                // Alert display failed
            }
            return false;
        }
        return true;
    }

    public static string GetNetworkType()
    {
        try
        {
            var profiles = Connectivity.Current.ConnectionProfiles;
            if (profiles.Contains(ConnectionProfile.WiFi))
                return "Wi-Fi";
            if (profiles.Contains(ConnectionProfile.Cellular))
                return "Mobile Data";
            if (profiles.Contains(ConnectionProfile.Ethernet))
                return "Wired Network";
        }
        catch
        {
            // Connectivity check failed
        }
        return "Offline";
    }
}
