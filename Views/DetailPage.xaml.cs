using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}