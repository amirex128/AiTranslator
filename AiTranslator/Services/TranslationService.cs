using AiTranslator.Models;

namespace AiTranslator.Services;

public class TranslationService : ITranslationService
{
    private readonly TranslationApiProvider _apiProvider;
    private readonly IConfigService _configService;

    public TranslationService(
        IConfigService configService,
        ILoggingService loggingService)
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
        
        var httpClient = new HttpClient(socketsHandler)
        {
            Timeout = TimeSpan.FromMinutes(_configService.Config.Api.TimeoutMinutes)
        };
        
        _apiProvider = new TranslationApiProvider(httpClient, loggingService);
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

