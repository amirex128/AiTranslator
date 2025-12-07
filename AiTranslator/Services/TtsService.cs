namespace AiTranslator.Services;

public class TtsService : ITtsService
{
    private readonly TtsProviderFactory _providerFactory;
    private readonly ILoggingService _loggingService;
    private CancellationTokenSource? _currentPlaybackCancellation;

    public TtsService(
        TtsProviderFactory providerFactory,
        ILoggingService loggingService)
    {
        _providerFactory = providerFactory;
        _loggingService = loggingService;
    }

    public async Task ReadPersianAsync(string text, CancellationToken cancellationToken = default)
    {
        await ReadTextAsync(text, "Persian", cancellationToken);
    }

    public async Task ReadEnglishAsync(string text, CancellationToken cancellationToken = default)
    {
        await ReadTextAsync(text, "English", cancellationToken);
    }

    private async Task ReadTextAsync(string text, string language, CancellationToken cancellationToken)
    {
        StopReading();
        _currentPlaybackCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            _loggingService.LogInformation($"Starting {language} TTS playback");

            var provider = _providerFactory.GetProvider();
            
            // Generate speech audio file
            var audioFilePath = await provider.GenerateSpeechAsync(text, language, _currentPlaybackCancellation.Token);
            
            // Play audio file
            await provider.PlayAudioAsync(audioFilePath, _currentPlaybackCancellation.Token);
            
            _loggingService.LogInformation($"{language} TTS playback completed");
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation($"{language} TTS playback cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"{language} TTS playback error", ex);
            throw;
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
        
        _providerFactory.StopAllProviders();
    }
}

