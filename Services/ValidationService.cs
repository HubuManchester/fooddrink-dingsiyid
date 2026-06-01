namespace CampusEats.Services;

/// <summary>
/// 验证服务 - 提供通用的输入验证功能
/// </summary>
public class ValidationService
{
    /// <summary>
    /// 验证字符串是否为空或空白
    /// </summary>
    public (bool IsValid, string Message) IsNotEmpty(string? value, string fieldName = "输入")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (false, $"{fieldName}不能为空");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// 验证字符串长度
    /// </summary>
    public (bool IsValid, string Message) CheckLength(string? value, int minLength, int maxLength, string fieldName = "输入")
    {
        if (value == null)
        {
            return (false, $"{fieldName}不能为空");
        }
        if (value.Length < minLength)
        {
            return (false, $"{fieldName}至少需要{minLength}个字符");
        }
        if (value.Length > maxLength)
        {
            return (false, $"{fieldName}不能超过{maxLength}个字符");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// 验证数值范围
    /// </summary>
    public (bool IsValid, string Message) CheckRange(int value, int min, int max, string fieldName = "数值")
    {
        if (value < min)
        {
            return (false, $"{fieldName}不能小于{min}");
        }
        if (value > max)
        {
            return (false, $"{fieldName}不能大于{max}");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// 验证数值范围（double）
    /// </summary>
    public (bool IsValid, string Message) CheckRange(double value, double min, double max, string fieldName = "数值")
    {
        if (value < min)
        {
            return (false, $"{fieldName}不能小于{min}");
        }
        if (value > max)
        {
            return (false, $"{fieldName}不能大于{max}");
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// 验证电子邮件格式
    /// </summary>
    public (bool IsValid, string Message) IsEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (false, "邮箱地址不能为空");
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return (addr.Address == value, "请输入有效的邮箱地址");
        }
        catch
        {
            return (false, "请输入有效的邮箱地址");
        }
    }

    /// <summary>
    /// 验证评分范围（1-5）
    /// </summary>
    public (bool IsValid, string Message) IsValidRating(int rating)
    {
        return CheckRange(rating, 1, 5, "评分");
    }

    /// <summary>
    /// 验证评论内容
    /// </summary>
    public (bool IsValid, string Message) IsValidComment(string? comment)
    {
        var notEmpty = IsNotEmpty(comment, "评论");
        if (!notEmpty.IsValid) return notEmpty;
        
        return CheckLength(comment, 1, 500, "评论");
    }

    /// <summary>
    /// 验证卡路里数值
    /// </summary>
    public (bool IsValid, string Message) IsValidCalories(int calories)
    {
        return CheckRange(calories, 0, 5000, "卡路里");
    }

    /// <summary>
    /// 验证时间（分钟）
    /// </summary>
    public (bool IsValid, string Message) IsValidTime(int minutes)
    {
        return CheckRange(minutes, 1, 1440, "时间");
    }
}
