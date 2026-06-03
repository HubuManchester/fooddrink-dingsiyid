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
        if (sender is Button button && button.BindingContext is Recipe recipe)
        {
            recipe.IsFavorite = !recipe.IsFavorite;
            _viewModel.ToggleFavoriteCommand.Execute(Tuple.Create(recipe, recipe.IsFavorite));
        }
    }
}
