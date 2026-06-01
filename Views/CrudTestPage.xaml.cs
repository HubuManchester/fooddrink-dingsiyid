using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class CrudTestPage : ContentPage
{
    public CrudTestPage(CrudTestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CrudTestViewModel viewModel)
        {
            await viewModel.LoadAllDataCommand.ExecuteAsync(null);
        }
    }
}
