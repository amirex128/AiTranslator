using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using AiTranslator.Models;
using AiTranslator.Services;

namespace AiTranslator.Services;

/// <summary>
/// Handles translation API calls with fallback mechanism
/// </summary>
public class TranslationApiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILoggingService _loggingService;
    private readonly INotificationService? _notificationService;

    public TranslationApiProvider(HttpClient httpClient, ILoggingService loggingService, INotificationService? notificationService = null)
    {
        _httpClient = httpClient;
        _loggingService = loggingService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Translates text using the provided API configuration with automatic fallback
    /// </summary>
    public async Task<TranslationResponse> TranslateAsync(
        TranslationApiConfig config,
        string text,
        string operationType,
        CancellationToken cancellationToken = default,
        bool returnFullJson = false)
    {
        if (config == null || config.Endpoints == null || config.Endpoints.Count == 0)
        {
            return new TranslationResponse
            {
                Success = false,
                Error = "No API endpoints configured"
            };
        }

        var endpoints = config.GetEndpointsInfoWithFallback().ToList();
        
        if (endpoints.Count == 0)
        {
            return new TranslationResponse
            {
                Success = false,
                Error = "No valid API endpoints available"
            };
        }

        Exception? lastException = null;

        // Try each endpoint in order
        for (int i = 0; i < endpoints.Count; i++)
        {
            var endpointInfo = endpoints[i];
            var isLastEndpoint = i == endpoints.Count - 1;

            try
            {
                _loggingService.LogInformation(
                    $"{operationType} - Attempting endpoint {i + 1}/{endpoints.Count}: {endpointInfo.Name} ({endpointInfo.Url}) [Timeout: {endpointInfo.TimeoutSeconds}s]");

                var response = await CallApiAsync(endpointInfo, text, cancellationToken, returnFullJson);

                if (response.Success)
                {
                    _loggingService.LogInformation(
                        $"{operationType} - Success with endpoint {i + 1}/{endpoints.Count}: {endpointInfo.Name}");
                    return response;
                }

                // If this is the last endpoint, return the error
                if (isLastEndpoint)
                {
                    return response;
                }

                _loggingService.LogWarning(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} ({endpointInfo.Name}) failed: {response.Error}. Trying next endpoint...");
                
                // Show notification when switching to next API
                _notificationService?.ShowNotification(
                    "API Switch",
                    $"{endpointInfo.Name} failed. Trying next API...");
            }
            catch (OperationCanceledException)
            {
                // Check if it was cancelled by user or timeout
                if (cancellationToken.IsCancellationRequested)
                {
                    _loggingService.LogInformation($"{operationType} was cancelled by user");
                    return new TranslationResponse
                    {
                        Success = false,
                        Error = "Operation was cancelled"
                    };
                }
                
                // Timeout occurred - try next endpoint
                if (isLastEndpoint)
                {
                    _loggingService.LogError($"{operationType} - All endpoints timed out. Last endpoint: {endpointInfo.Name}");
                    return new TranslationResponse
                    {
                        Success = false,
                        Error = $"All endpoints timed out. Last endpoint: {endpointInfo.Name} (timeout: {endpointInfo.TimeoutSeconds}s)"
                    };
                }
                
                _loggingService.LogWarning(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} ({endpointInfo.Name}) timed out after {endpointInfo.TimeoutSeconds}s. Trying next endpoint...");
                
                // Show notification when switching to next API due to timeout
                _notificationService?.ShowNotification(
                    "API Timeout",
                    $"{endpointInfo.Name} timed out ({endpointInfo.TimeoutSeconds}s). Trying next API...");
            }
            catch (HttpRequestException ex)
            {
                // Network errors - try next endpoint
                lastException = ex;
                if (isLastEndpoint)
                {
                    _loggingService.LogError($"{operationType} - All endpoints failed. Last error: {ex.Message}");
                    return new TranslationResponse
                    {
                        Success = false,
                        Error = $"All endpoints failed. Last error: {ex.Message}"
                    };
                }
                
                _loggingService.LogWarning(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} ({endpointInfo.Name}) network error: {ex.Message}. Trying next endpoint...");
                
                // Show notification when switching to next API due to network error
                _notificationService?.ShowNotification(
                    "API Network Error",
                    $"{endpointInfo.Name} connection failed. Trying next API...");
            }
            catch (SocketException ex)
            {
                // Socket errors - try next endpoint
                lastException = ex;
                if (isLastEndpoint)
                {
                    _loggingService.LogError($"{operationType} - All endpoints failed. Last error: {ex.Message}");
                    return new TranslationResponse
                    {
                        Success = false,
                        Error = $"All endpoints failed. Last error: {ex.Message}"
                    };
                }
                
                _loggingService.LogWarning(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} ({endpointInfo.Name}) connection error: {ex.Message}. Trying next endpoint...");
                
                // Show notification when switching to next API due to socket error
                _notificationService?.ShowNotification(
                    "API Connection Error",
                    $"{endpointInfo.Name} connection failed. Trying next API...");
            }
            catch (Exception ex)
            {
                lastException = ex;
                _loggingService.LogError(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} ({endpointInfo.Name}) error: {ex.Message}", ex);

                // If this is the last endpoint, return the error
                if (isLastEndpoint)
                {
                    return new TranslationResponse
                    {
                        Success = false,
                        Error = $"All endpoints failed. Last error: {ex.Message}"
                    };
                }

                _loggingService.LogWarning(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} ({endpointInfo.Name}) failed. Trying next endpoint...");
                
                // Show notification when switching to next API due to general error
                _notificationService?.ShowNotification(
                    "API Error",
                    $"{endpointInfo.Name} failed. Trying next API...");
            }
        }

        // Should not reach here, but just in case
        return new TranslationResponse
        {
            Success = false,
            Error = lastException != null
                ? $"All endpoints failed. Last error: {lastException.Message}"
                : "All endpoints failed"
        };
    }

    /// <summary>
    /// Calls a single API endpoint with specific timeout
    /// </summary>
    private async Task<TranslationResponse> CallApiAsync(
        EndpointInfo endpointInfo,
        string text,
        CancellationToken cancellationToken,
        bool returnFullJson = false)
    {
        var request = new TranslationRequest { Question = text };
        
        // Create a timeout cancellation token for this specific endpoint
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(endpointInfo.TimeoutSeconds));
        
        var response = await _httpClient.PostAsJsonAsync(endpointInfo.Url, request, timeoutCts.Token);

        if (!response.IsSuccessStatusCode)
        {
            return new TranslationResponse
            {
                Success = false,
                Error = $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
            };
        }

        var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);

        // If returnFullJson is true (for Grammar Learner), return the full JSON
        if (returnFullJson)
        {
            return new TranslationResponse
            {
                Text = content,
                Success = true
            };
        }

        // Try to parse as JSON first
        try
        {
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            if (jsonResponse != null && jsonResponse.ContainsKey("text"))
            {
                var translatedText = jsonResponse["text"].ToString() ?? content;
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

        return new TranslationResponse
        {
            Text = content,
            Success = true
        };
    }
}
