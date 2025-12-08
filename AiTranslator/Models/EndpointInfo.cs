namespace AiTranslator.Models;

/// <summary>
/// Represents an API endpoint with a name, URL, and timeout
/// </summary>
public class EndpointInfo
{
    /// <summary>
    /// Display name for the endpoint (e.g., "Primary API", "Backup Server")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// API endpoint URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for this endpoint (default: 30 seconds)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Checks if the endpoint is valid (has both name and URL)
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Url);
}
