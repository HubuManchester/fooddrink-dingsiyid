using Microsoft.Maui.Devices.Sensors;
using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            if (!Accelerometer.Default.IsMonitoring)
                Accelerometer.Default.Start(SensorSpeed.UI);
        }
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading.Acceleration;
        double threshold = 2.5;
        if (Math.Abs(data.X) > threshold || Math.Abs(data.Y) > threshold || Math.Abs(data.Z) > threshold)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.ShakeToRecommendCommand.Execute(null);
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Accelerometer.Default.IsMonitoring)
        {
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.Stop();
        }
    }
}