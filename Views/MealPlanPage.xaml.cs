using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class MealPlanPage : ContentPage
{
    public MealPlanPage(MealPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}