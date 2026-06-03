using System.Globalization;
using Microsoft.Maui.Media;

namespace CampusEats.Services;

public class TextToSpeechService
{
    private CancellationTokenSource? _cts;

    public static bool IsSupported => Microsoft.Maui.Media.TextToSpeech.Default != null;

    public static Task<IEnumerable<string>> GetAvailableVoicesAsync()
    {
        return Task.FromResult<IEnumerable<string>>(new List<string> { "en-US", "zh-CN", "es-ES" });
    }

    public async Task SpeakAsync(string text, double pitch = 1.0, double volume = 1.0)
    {
        if (string.IsNullOrWhiteSpace(text) || !IsSupported) return;

        Stop();
        _cts = new CancellationTokenSource();

        try
        {
            var options = new SpeechOptions
            {
                Pitch = (float)pitch,
                Volume = (float)volume
                // Locale is optional; to set language, use Locale.FromCultureInfo() if needed
            };
            await Microsoft.Maui.Media.TextToSpeech.Default.SpeakAsync(text, options, _cts.Token);
        }
        catch (OperationCanceledException) { }
        catch { }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public async Task SpeakRestaurantInfo(string name, string cuisine, double rating, string description)
    {
        var text = $"Restaurant: {name}. Cuisine: {cuisine}. Rating: {rating} stars. {description}";
        await SpeakAsync(text);
    }

    public async Task SpeakReview(string comment, int rating)
    {
        var text = $"Review rating: {rating} stars. Review content: {comment}";
        await SpeakAsync(text);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public bool IsSpeaking => _cts != null;

    public static string GetSystemLanguage()
    {
        var culture = CultureInfo.CurrentCulture;
        return $"{culture.TwoLetterISOLanguageName}-{culture.Name.Split('-').LastOrDefault()?.ToUpper() ?? "US"}";
    }
}