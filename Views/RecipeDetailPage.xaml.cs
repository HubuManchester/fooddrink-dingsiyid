using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload recipe data every time the page appears
        // This ensures the correct recipe is displayed when navigating from different sources
        _viewModel.ReloadRecipe();
    }
}