using CampusEats.Views;

namespace CampusEats;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("mainpage", typeof(MainPage));
        Routing.RegisterRoute("detailpage", typeof(DetailPage));
    }
}