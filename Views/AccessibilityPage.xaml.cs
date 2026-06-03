using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class AccessibilityPage : ContentPage
{
    public AccessibilityPage(AccessibilityViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AccessibilityPage init error: {ex}");
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(20),
                    Children =
                    {
                        new Label { Text = $"Failed to load page: {ex.Message}", TextColor = Colors.Red, FontSize = 18 },
                        new Label { Text = $"Exception type: {ex.GetType().Name}", TextColor = Colors.Red, FontSize = 14, Margin = new Thickness(0, 10, 0, 0) },
                        new Label { Text = $"Stack trace:\n{ex.StackTrace}", TextColor = Colors.Red, FontSize = 12, Margin = new Thickness(0, 10, 0, 0) }
                    }
                }
            };
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            // If ViewModel has hardware-related initialization code, place it here
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AccessibilityPage OnAppearing error: {ex}");
        }
    }
}
