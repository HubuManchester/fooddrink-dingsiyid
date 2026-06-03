using CampusEats.Models;
using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class RecipePage : ContentPage
{
    private readonly RecipePageViewModel _viewModel;

    public RecipePage(RecipePageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Recipe recipe)
        {
            _viewModel.ToggleFavoriteCommand.Execute(recipe);
        }
    }

    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        _viewModel.SearchCommand.Execute(null);
    }
}
