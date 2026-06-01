using CampusEats.ViewModels;

namespace CampusEats.Views;

public partial class AccessibilityPage : ContentPage
{
    public AccessibilityPage(AccessibilityViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
