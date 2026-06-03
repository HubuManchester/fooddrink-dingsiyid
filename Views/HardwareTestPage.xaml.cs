using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class HardwareTestPage : ContentPage
{
    public HardwareTestPage(HardwareTestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}