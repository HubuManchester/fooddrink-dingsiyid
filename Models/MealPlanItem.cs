using CommunityToolkit.Mvvm.ComponentModel;

namespace CampusEats.Models;

public partial class MealPlanItem : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string recipeName = string.Empty;

    [ObservableProperty]
    private int recipeId;

    [ObservableProperty]
    private string mealType = string.Empty; // Breakfast, Lunch, Dinner, Snack

    [ObservableProperty]
    private DateTime plannedDate;

    [ObservableProperty]
    private int calories;

    [ObservableProperty]
    private DateTime createdAt;
}
