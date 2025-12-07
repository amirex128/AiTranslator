using AiTranslator.Models;

namespace AiTranslator.Services;

public class TtsProviderFactory
{
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private readonly TtsCacheManager _cacheManager;
    private readonly AudioPlayer _audioPlayer;
    private GoogleTtsProvider? _googleProvider;
    private LocalAITtsProvider? _localAIProvider;

    public TtsProviderFactory(
        IConfigService configService,
        ILoggingService loggingService,
        TtsCacheManager cacheManager,
        AudioPlayer audioPlayer)
    {
        _configService = configService;
        _loggingService = loggingService;
        _cacheManager = cacheManager;
        _audioPlayer = audioPlayer;
    }

    public ITtsProvider GetProvider()
    {
        var provider = _configService.Config.TtsSettings.Provider;

        return provider switch
        {
            TtsProvider.Google => GetGoogleProvider(),
            TtsProvider.LocalAI => GetLocalAIProvider(),
            _ => throw new NotSupportedException($"TTS provider {provider} is not supported")
        };
    }

    private ITtsProvider GetGoogleProvider()
    {
        if (_googleProvider == null)
        {
            _googleProvider = new GoogleTtsProvider(_loggingService, _cacheManager, _audioPlayer);
        }
        return _googleProvider;
    }

    private ITtsProvider GetLocalAIProvider()
    {
        if (_localAIProvider == null)
        {
            _localAIProvider = new LocalAITtsProvider(_configService, _loggingService, _cacheManager, _audioPlayer);
        }
        return _localAIProvider;
    }

    public void StopAllProviders()
    {
        _audioPlayer.Stop();
    }
}
