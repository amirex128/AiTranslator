using System.Net.Http.Json;
using System.Text.Json;
using AiTranslator.Models;

namespace AiTranslator.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;

    public TranslationService(
        IConfigService configService,
        ILoggingService loggingService)
    {
        _configService = configService;
        _loggingService = loggingService;
        
        // استفاده از SocketsHttpHandler برای کنترل بهتر روی اتصالات
        var socketsHandler = new SocketsHttpHandler
        {
            UseProxy = false,            // مهم: از پراکسی سیستم استفاده نکن
            Proxy = null,
            // حل مشکل تاخیر 30 ثانیه‌ای localhost در ویندوز
            // با اولویت دادن به IPv4 به جای IPv6
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork, // فقط IPv4
                    System.Net.Sockets.SocketType.Stream,
                    System.Net.Sockets.ProtocolType.Tcp);
                
                socket.NoDelay = true; // غیرفعال کردن الگوریتم Nagle برای کاهش تاخیر
                
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
            PooledConnectionLifetime = TimeSpan.FromMinutes(2), // بازیافت اتصالات قدیمی
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
        };
        
        _httpClient = new HttpClient(socketsHandler)
        {
            Timeout = TimeSpan.FromMinutes(_configService.Config.Api.TimeoutMinutes)
        };
    }

    public async Task<TranslationResponse> TranslateEnglishToPersianAsync(string text, CancellationToken cancellationToken = default)
    {
        var endpoint = _configService.Config.ApiEndpoints.EnglishToPersian;
        return await TranslateWithRetryAsync(endpoint, text, "EN->FA", cancellationToken);
    }

    public async Task<TranslationResponse> TranslatePersianToEnglishAsync(string text, CancellationToken cancellationToken = default)
    {
        var endpoint = _configService.Config.ApiEndpoints.PersianToEnglish;
        return await TranslateWithRetryAsync(endpoint, text, "FA->EN", cancellationToken);
    }

    public async Task<TranslationResponse> FixGrammarAsync(string text, CancellationToken cancellationToken = default)
    {
        var endpoint = _configService.Config.ApiEndpoints.GrammarFix;
        return await TranslateWithRetryAsync(endpoint, text, "Grammar Fix", cancellationToken);
    }

    private async Task<TranslationResponse> TranslateWithRetryAsync(
        string endpoint,
        string text,
        string operationType,
        CancellationToken cancellationToken)
    {
        var retryCount = _configService.Config.Api.RetryCount;
        var retryDelay = TimeSpan.FromSeconds(_configService.Config.Api.RetryDelaySeconds);

        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            try
            {
                _loggingService.LogInformation($"{operationType} attempt {attempt + 1}/{retryCount + 1}");

                var request = new TranslationRequest { Question = text };
                var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    
                    // Try to parse as JSON first
                    try
                    {
                        var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                        if (jsonResponse != null && jsonResponse.ContainsKey("text"))
                        {
                            var translatedText = jsonResponse["text"].ToString() ?? content;
                            _loggingService.LogInformation($"{operationType} succeeded");
                            return new TranslationResponse
                            {
                                Text = translatedText,
                                Success = true
                            };
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, use raw content
                    }

                    _loggingService.LogInformation($"{operationType} succeeded");
                    return new TranslationResponse
                    {
                        Text = content,
                        Success = true
                    };
                }
                else
                {
                    _loggingService.LogWarning($"{operationType} failed with status code: {response.StatusCode}");
                }
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogInformation($"{operationType} was cancelled");
                return new TranslationResponse
                {
                    Success = false,
                    Error = "Operation was cancelled"
                };
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"{operationType} error on attempt {attempt + 1}", ex);
                
                if (attempt == retryCount)
                {
                    return new TranslationResponse
                    {
                        Success = false,
                        Error = $"Failed after {retryCount + 1} attempts: {ex.Message}"
                    };
                }
            }

            if (attempt < retryCount)
            {
                await Task.Delay(retryDelay * (attempt + 1), cancellationToken); // Exponential backoff
            }
        }

        return new TranslationResponse
        {
            Success = false,
            Error = "Translation failed"
        };
    }
}

