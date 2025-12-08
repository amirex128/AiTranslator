namespace AiTranslator.Models;

/// <summary>
/// Configuration for a translation type with multiple API endpoints and fallback support
/// </summary>
public class TranslationApiConfig
{
    /// <summary>
    /// List of API endpoints (up to 4) with names
    /// </summary>
    public List<EndpointInfo> Endpoints { get; set; } = new();

    /// <summary>
    /// Index of the default/primary endpoint (0-3)
    /// </summary>
    public int DefaultEndpointIndex { get; set; } = 0;

    /// <summary>
    /// Gets the default endpoint URL
    /// </summary>
    public string? GetDefaultEndpoint()
    {
        if (Endpoints == null || Endpoints.Count == 0)
            return null;

        var index = Math.Max(0, Math.Min(DefaultEndpointIndex, Endpoints.Count - 1));
        var endpoint = Endpoints[index];
        return endpoint?.Url;
    }

    /// <summary>
    /// Gets all available endpoints starting from the default one
    /// </summary>
    public IEnumerable<string> GetEndpointsWithFallback()
    {
        if (Endpoints == null || Endpoints.Count == 0)
            yield break;

        // Start from default endpoint
        var startIndex = Math.Max(0, Math.Min(DefaultEndpointIndex, Endpoints.Count - 1));
        
        // Yield default endpoint first
        for (int i = startIndex; i < Endpoints.Count; i++)
        {
            var endpoint = Endpoints[i];
            if (endpoint != null && !string.IsNullOrWhiteSpace(endpoint.Url))
                yield return endpoint.Url;
        }

        // Then yield remaining endpoints before default
        for (int i = 0; i < startIndex; i++)
        {
            var endpoint = Endpoints[i];
            if (endpoint != null && !string.IsNullOrWhiteSpace(endpoint.Url))
                yield return endpoint.Url;
        }
    }
}
