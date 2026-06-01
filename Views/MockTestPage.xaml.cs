using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class MockTestPage : ContentPage
{
    public MockTestPage(MockTestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
