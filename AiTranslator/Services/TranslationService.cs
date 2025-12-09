using AiTranslator.Models;

namespace AiTranslator.Services;

public class TranslationService : ITranslationService
{
    private readonly TranslationApiProvider _apiProvider;
    private readonly IConfigService _configService;

    public TranslationService(
        IConfigService configService,
        ILoggingService loggingService,
        INotificationService? notificationService = null)
    {
        _configService = configService;
        
        // Create HttpClient with optimized settings
        var socketsHandler = new SocketsHttpHandler
        {
            UseProxy = false,
            Proxy = null,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Stream,
                    System.Net.Sockets.ProtocolType.Tcp);
                
                socket.NoDelay = true;
                
                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);
                    return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
        };
        
        // Timeout is now configured per endpoint, so use a very large timeout for HttpClient
        // Individual endpoint timeouts are handled in TranslationApiProvider
        var httpClient = new HttpClient(socketsHandler)
        {
            Timeout = TimeSpan.FromHours(24) // Very large timeout, actual timeout is per endpoint
        };
        
        _apiProvider = new TranslationApiProvider(httpClient, loggingService, notificationService);
    }

    public async Task<TranslationResponse> TranslateEnglishToPersianAsync(string text, CancellationToken cancellationToken = default)
    {
        var config = _configService.Config.ApiEndpoints.EnglishToPersian;
        return await _apiProvider.TranslateAsync(config, text, "EN->FA", cancellationToken);
    }

    public async Task<TranslationResponse> TranslatePersianToEnglishAsync(string text, CancellationToken cancellationToken = default)
    {
        var config = _configService.Config.ApiEndpoints.PersianToEnglish;
        return await _apiProvider.TranslateAsync(config, text, "FA->EN", cancellationToken);
    }

    public async Task<TranslationResponse> FixGrammarAsync(string text, CancellationToken cancellationToken = default)
    {
        var config = _configService.Config.ApiEndpoints.GrammarFix;
        return await _apiProvider.TranslateAsync(config, text, "Grammar Fix", cancellationToken);
    }
}

