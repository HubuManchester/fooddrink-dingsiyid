using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
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