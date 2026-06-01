using System.Globalization;

namespace CampusEats.Services;

/// <summary>
/// 语音合成服务 - 负责将文本转换为语音
/// 支持多种语言和语速调节
/// </summary>
public class TextToSpeechService
{
    private CancellationTokenSource? _cts;

    /// <summary>
    /// 检查语音合成是否可用
    /// </summary>
    public bool IsSupported => TextToSpeech.Default.IsSupported;

    /// <summary>
    /// 获取可用的语音列表
    /// </summary>
    public async Task<IEnumerable<Locale>> GetAvailableVoicesAsync()
    {
        if (!IsSupported) return Enumerable.Empty<Locale>();
        return await TextToSpeech.Default.GetLocalesAsync();
    }

    /// <summary>
    /// 朗读文本
    /// </summary>
    /// <param name="text">要朗读的文本</param>
    /// <param name="language">语言代码（如 "zh-CN", "en-US"）</param>
    /// <param name="speed">语速（0.5-2.0，默认1.0）</param>
    /// <param name="pitch">音调（0.5-2.0，默认1.0）</param>
    public async Task SpeakAsync(string text, string language = "zh-CN", double speed = 1.0, double pitch = 1.0)
    {
        if (!IsSupported || string.IsNullOrWhiteSpace(text)) return;

        // 取消之前的朗读
        Stop();

        _cts = new CancellationTokenSource();

        var settings = new SpeechOptions
        {
            Locale = language,
            Rate = Math.Clamp(speed, 0.5, 2.0),
            Pitch = Math.Clamp(pitch, 0.5, 2.0),
            Volume = 1.0
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, settings, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // 正常取消
        }
        catch (Exception)
        {
            // 忽略语音合成错误
        }
    }

    /// <summary>
    /// 朗读餐厅信息
    /// </summary>
    public async Task SpeakRestaurantInfo(string name, string cuisine, double rating, string description)
    {
        var text = $"餐厅：{name}。菜系：{cuisine}。评分：{rating}分。{description}";
        await SpeakAsync(text);
    }

    /// <summary>
    /// 朗读评论内容
    /// </summary>
    public async Task SpeakReview(string comment, int rating)
    {
        var text = $"评论评分：{rating}星。评论内容：{comment}";
        await SpeakAsync(text);
    }

    /// <summary>
    /// 停止当前朗读
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// 检查是否正在朗读
    /// </summary>
    public bool IsSpeaking => _cts != null;

    /// <summary>
    /// 获取系统语言代码
    /// </summary>
    public string GetSystemLanguage()
    {
        var culture = CultureInfo.CurrentCulture;
        return $"{culture.TwoLetterISOLanguageName}-{culture.Name.Split('-').LastOrDefault()?.ToUpper() ?? "CN"}";
    }
}
