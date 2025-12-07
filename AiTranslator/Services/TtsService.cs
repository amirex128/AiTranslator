namespace AiTranslator.Services;

public class TtsService : ITtsService
{
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private CancellationTokenSource? _currentPlaybackCancellation;

    public TtsService(
        IConfigService configService,
        ILoggingService loggingService)
    {
        _configService = configService;
        _loggingService = loggingService;
    }

    public async Task ReadPersianAsync(string text, CancellationToken cancellationToken = default)
    {
        StopReading();

        var endpoint = _configService.Config.TtsEndpoints.Persian;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _loggingService.LogWarning("Persian TTS endpoint not configured");
            return;
        }

        await ReadTextAsync(endpoint, text, "Persian", cancellationToken);
    }

    public async Task ReadEnglishAsync(string text, CancellationToken cancellationToken = default)
    {
        StopReading();

        var endpoint = _configService.Config.TtsEndpoints.English;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _loggingService.LogWarning("English TTS endpoint not configured");
            return;
        }

        await ReadTextAsync(endpoint, text, "English", cancellationToken);
    }

    private async Task ReadTextAsync(string endpoint, string text, string language, CancellationToken cancellationToken)
    {
        _currentPlaybackCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            _loggingService.LogInformation($"Starting {language} TTS playback");
            
            // TODO: Implement actual TTS API call when endpoints are provided
            // For now, this is just a skeleton implementation
            
            await Task.Delay(100, _currentPlaybackCancellation.Token);
            
            _loggingService.LogInformation($"{language} TTS playback completed");
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation($"{language} TTS playback stopped");
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"{language} TTS playback error", ex);
        }
        finally
        {
            _currentPlaybackCancellation?.Dispose();
            _currentPlaybackCancellation = null;
        }
    }

    public void StopReading()
    {
        if (_currentPlaybackCancellation != null && !_currentPlaybackCancellation.IsCancellationRequested)
        {
            _currentPlaybackCancellation.Cancel();
        }
    }
}

