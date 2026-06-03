using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class DetailPage : ContentPage
{
    private readonly DetailPageViewModel _viewModel;

    public DetailPage(DetailPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload restaurant data every time the page appears
        // This ensures the correct restaurant is displayed when navigating from different items
        _viewModel.ReloadRestaurant();
        
        // Ensure back button is shown when not in tab navigation
        if (Navigation != null && Navigation.NavigationStack.Count > 0)
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsEnabled = true,
                IsVisible = true
            });
        }
    }
}