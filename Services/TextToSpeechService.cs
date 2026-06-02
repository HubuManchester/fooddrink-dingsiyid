using System.Globalization;

namespace CampusEats.Services;

/// <summary>
/// Text-to-speech service - responsible for converting text to speech
/// </summary>
public class TextToSpeechService
{
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Check if text-to-speech is supported
    /// </summary>
    public bool IsSupported => true;

    /// <summary>
    /// Get available voices
    /// </summary>
    public Task<IEnumerable<string>> GetAvailableVoicesAsync()
    {
        return Task.FromResult<IEnumerable<string>>(new List<string> { "en-US", "zh-CN", "es-ES" });
    }

    /// <summary>
    /// Speak text
    /// </summary>
    public async Task SpeakAsync(string text, string language = "en-US", double speed = 1.0, double pitch = 1.0)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Cancel previous speech
        Stop();

        _cts = new CancellationTokenSource();

        try
        {
            await TextToSpeech.Default.SpeakAsync(text);
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception)
        {
            // Ignore text-to-speech errors
        }
    }

    /// <summary>
    /// Speak restaurant information
    /// </summary>
    public async Task SpeakRestaurantInfo(string name, string cuisine, double rating, string description)
    {
        var text = $"Restaurant: {name}. Cuisine: {cuisine}. Rating: {rating} stars. {description}";
        await SpeakAsync(text);
    }

    /// <summary>
    /// Speak review content
    /// </summary>
    public async Task SpeakReview(string comment, int rating)
    {
        var text = $"Review rating: {rating} stars. Review content: {comment}";
        await SpeakAsync(text);
    }

    /// <summary>
    /// Stop current speech
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// Check if currently speaking
    /// </summary>
    public bool IsSpeaking => _cts != null;

    /// <summary>
    /// Get system language code
    /// </summary>
    public string GetSystemLanguage()
    {
        var culture = CultureInfo.CurrentCulture;
        return $"{culture.TwoLetterISOLanguageName}-{culture.Name.Split('-').LastOrDefault()?.ToUpper() ?? "US"}";
    }
}
