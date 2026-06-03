using CampusEats.Views;

namespace CampusEats;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("mainpage", typeof(MainPage));
        Routing.RegisterRoute("detailpage", typeof(DetailPage));
        Routing.RegisterRoute("recipedetailpage", typeof(RecipeDetailPage));
        Routing.RegisterRoute("recipepage", typeof(RecipePage));
        Routing.RegisterRoute("hardwaretestpage", typeof(HardwareTestPage));
        Routing.RegisterRoute("mealplanpage", typeof(MealPlanPage));
        Routing.RegisterRoute("accessibilitypage", typeof(AccessibilityPage));
    }
}