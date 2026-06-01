using System.ComponentModel;

namespace CampusEats.Services;

/// <summary>
/// 网络状态服务 - 提供网络连接状态检测和离线缓存管理
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
        // 初始检查网络状态
        IsConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;

        // 订阅网络状态变化
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
    /// 检查网络连接并显示提示
    /// </summary>
    public async Task<bool> CheckConnectionAsync(string message = "当前网络不可用，请检查网络连接")
    {
        if (!IsConnected)
        {
            await Application.Current?.MainPage?.DisplayAlert("网络错误", message, "确定")!;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 获取网络类型描述
    /// </summary>
    public string GetNetworkType()
    {
        var profiles = Connectivity.Current.ConnectionProfiles;
        if (profiles.Contains(ConnectionProfile.WiFi))
            return "Wi-Fi";
        if (profiles.Contains(ConnectionProfile.Cellular))
            return "移动数据";
        if (profiles.Contains(ConnectionProfile.Ethernet))
            return "有线网络";
        return "离线";
    }
}
