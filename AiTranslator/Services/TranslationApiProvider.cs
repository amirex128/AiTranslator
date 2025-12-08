using System.Net.Http.Json;
using System.Text.Json;
using AiTranslator.Models;

namespace AiTranslator.Services;

/// <summary>
/// Handles translation API calls with fallback mechanism
/// </summary>
public class TranslationApiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILoggingService _loggingService;

    public TranslationApiProvider(HttpClient httpClient, ILoggingService loggingService)
    {
        _httpClient = httpClient;
        _loggingService = loggingService;
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

        var endpoints = config.GetEndpointsWithFallback().ToList();
        
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
            var endpoint = endpoints[i];
            var isLastEndpoint = i == endpoints.Count - 1;

            try
            {
                _loggingService.LogInformation(
                    $"{operationType} - Attempting endpoint {i + 1}/{endpoints.Count}: {endpoint}");

                var response = await CallApiAsync(endpoint, text, cancellationToken, returnFullJson);

                if (response.Success)
                {
                    _loggingService.LogInformation(
                        $"{operationType} - Success with endpoint {i + 1}/{endpoints.Count}");
                    return response;
                }

                // If this is the last endpoint, return the error
                if (isLastEndpoint)
                {
                    return response;
                }

                _loggingService.LogWarning(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} failed: {response.Error}. Trying next endpoint...");
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
                lastException = ex;
                _loggingService.LogError(
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} error: {ex.Message}", ex);

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
                    $"{operationType} - Endpoint {i + 1}/{endpoints.Count} failed. Trying next endpoint...");
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
    /// Calls a single API endpoint
    /// </summary>
    private async Task<TranslationResponse> CallApiAsync(
        string endpoint,
        string text,
        CancellationToken cancellationToken,
        bool returnFullJson = false)
    {
        var request = new TranslationRequest { Question = text };
        var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new TranslationResponse
            {
                Success = false,
                Error = $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
            };
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

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
