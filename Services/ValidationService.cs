namespace CampusEats.Services;

/// <summary>
/// Validation service - provides general input validation functions
/// </summary>
public class ValidationService
{
    /// <summary>
    /// Validate if string is empty or whitespace
    /// </summary>
    public (bool IsValid, string Message) IsNotEmpty(string? value, string fieldName = "Input")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (false, $"{fieldName} cannot be empty");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// Validate string length
    /// </summary>
    public (bool IsValid, string Message) CheckLength(string? value, int minLength, int maxLength, string fieldName = "Input")
    {
        if (value == null)
        {
            return (false, $"{fieldName} cannot be empty");
        }
        if (value.Length < minLength)
        {
            return (false, $"{fieldName} needs at least {minLength} characters");
        }
        if (value.Length > maxLength)
        {
            return (false, $"{fieldName} cannot exceed {maxLength} characters");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// Validate numeric range
    /// </summary>
    public (bool IsValid, string Message) CheckRange(int value, int min, int max, string fieldName = "Number")
    {
        if (value < min)
        {
            return (false, $"{fieldName} cannot be less than {min}");
        }
        if (value > max)
        {
            return (false, $"{fieldName} cannot be greater than {max}");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// Validate numeric range (double)
    /// </summary>
    public (bool IsValid, string Message) CheckRange(double value, double min, double max, string fieldName = "Number")
    {
        if (value < min)
        {
            return (false, $"{fieldName} cannot be less than {min}");
        }
        if (value > max)
        {
            return (false, $"{fieldName} cannot be greater than {max}");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    public (bool IsValid, string Message) IsEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (false, "Email address cannot be empty");
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return (addr.Address == value, "Please enter a valid email address");
        }
        catch
        {
            return (false, "Please enter a valid email address");
        }
    }

    /// <summary>
    /// Validate rating range (1-5)
    /// </summary>
    public (bool IsValid, string Message) IsValidRating(int rating)
    {
        return CheckRange(rating, 1, 5, "Rating");
    }

    /// <summary>
    /// Validate comment content
    /// </summary>
    public (bool IsValid, string Message) IsValidComment(string? comment)
    {
        var notEmpty = IsNotEmpty(comment, "Comment");
        if (!notEmpty.IsValid) return notEmpty;
        
        return CheckLength(comment, 1, 500, "Comment");
    }

    /// <summary>
    /// Validate calories value
    /// </summary>
    public (bool IsValid, string Message) IsValidCalories(int calories)
    {
        return CheckRange(calories, 0, 5000, "Calories");
    }

    /// <summary>
    /// Validate time (minutes)
    /// </summary>
    public (bool IsValid, string Message) IsValidTime(int minutes)
    {
        return CheckRange(minutes, 1, 1440, "Time");
    }
}
