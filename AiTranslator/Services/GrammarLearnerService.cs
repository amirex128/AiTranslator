using System.Net.Http.Json;
using System.Text.Json;
using AiTranslator.Models;

namespace AiTranslator.Services;

public interface IGrammarLearnerService
{
    Task<GrammarLearnerResponse?> LearnGrammarAsync(string text, CancellationToken cancellationToken = default);
}

public class GrammarLearnerService : IGrammarLearnerService
{
    private readonly TranslationApiProvider _apiProvider;
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;

    public GrammarLearnerService(
        IConfigService configService,
        ILoggingService loggingService)
    {
        _configService = configService;
        _loggingService = loggingService;
        
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
        
        _apiProvider = new TranslationApiProvider(httpClient, loggingService);
    }

    public async Task<GrammarLearnerResponse?> LearnGrammarAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = _configService.Config.ApiEndpoints.GrammarLearner;
            // Pass returnFullJson = true to get the complete JSON response
            var response = await _apiProvider.TranslateAsync(config, text, "Grammar Learner", cancellationToken, returnFullJson: true);

            if (!response.Success)
            {
                _loggingService.LogError($"Grammar Learner failed: {response.Error}");
                return null;
            }

            // Parse JSON response - the response.Text now contains the full JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            GrammarLearnerResponse? grammarResponse = null;
            try
            {
                // First, try to parse as a wrapper object that might contain "json" field
                var wrapper = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response.Text, options);
                
                if (wrapper != null && wrapper.ContainsKey("json"))
                {
                    // Extract the "json" field which contains the actual GrammarLearnerResponse
                    var jsonElement = wrapper["json"];
                    var jsonString = jsonElement.GetRawText();
                    grammarResponse = JsonSerializer.Deserialize<GrammarLearnerResponse>(jsonString, options);
                }
                else
                {
                    // If no "json" wrapper, try to deserialize directly
                    grammarResponse = JsonSerializer.Deserialize<GrammarLearnerResponse>(response.Text, options);
                }
            }
            catch (JsonException ex)
            {
                _loggingService.LogError($"JSON deserialization error: {ex.Message}", ex);
                _loggingService.LogError($"Response content (first 500 chars): {response.Text?.Substring(0, Math.Min(500, response.Text?.Length ?? 0))}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error parsing Grammar Learner response: {ex.Message}", ex);
            }
            
            if (grammarResponse == null)
            {
                _loggingService.LogError("Failed to deserialize Grammar Learner response");
                _loggingService.LogError($"Response content length: {response.Text?.Length ?? 0}");
                if (response.Text != null && response.Text.Length < 1000)
                {
                    _loggingService.LogError($"Response content: {response.Text}");
                }
                return null;
            }

            // Log successful deserialization for debugging
            _loggingService.LogInformation($"Grammar Learner response deserialized successfully. OriginalText: '{grammarResponse.OriginalText}', CorrectedText: '{grammarResponse.CorrectedText}'");

            return grammarResponse;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error in Grammar Learner service", ex);
            return null;
        }
    }
}
