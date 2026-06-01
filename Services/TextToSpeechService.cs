using System.Globalization;

namespace CampusEats.Services;

/// <summary>
/// Text-to-speech service - responsible for converting text to speech
/// Supports multiple languages and speed adjustment
/// </summary>
public class TextToSpeechService
{
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Check if text-to-speech is supported
    /// </summary>
    public bool IsSupported => TextToSpeech.Default.IsSupported;

    /// <summary>
    /// Get available voices
    /// </summary>
    public async Task<IEnumerable<Locale>> GetAvailableVoicesAsync()
    {
        if (!IsSupported) return Enumerable.Empty<Locale>();
        return await TextToSpeech.Default.GetLocalesAsync();
    }

    /// <summary>
    /// Speak text
    /// </summary>
    /// <param name="text">Text to speak</param>
    /// <param name="language">Language code (e.g., "en-US", "zh-CN")</param>
    /// <param name="speed">Speech rate (0.5-2.0, default 1.0)</param>
    /// <param name="pitch">Pitch (0.5-2.0, default 1.0)</param>
    public async Task SpeakAsync(string text, string language = "en-US", double speed = 1.0, double pitch = 1.0)
    {
        if (!IsSupported || string.IsNullOrWhiteSpace(text)) return;

        // Cancel previous speech
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
