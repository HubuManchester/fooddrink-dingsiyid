namespace CampusEats.Services;

public static class StringHelper
{
    public static string GenerateStars(double rating)
    {
        int full = (int)Math.Floor(rating);
        bool half = (rating - full) >= 0.5;
        int empty = 5 - full - (half ? 1 : 0);
        return new string('★', full) + (half ? "½" : "") + new string('☆', empty);
    }
}