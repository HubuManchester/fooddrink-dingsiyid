namespace CampusEats.Models;

public class MealPlanItem
{
    public int Id { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public int RecipeId { get; set; }
    public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
    public DateTime PlannedDate { get; set; }
    public int Calories { get; set; }
    public DateTime CreatedAt { get; set; }
}
