namespace AiTranslator.Models;

/// <summary>
/// Represents an API endpoint with a name and URL
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
    /// Checks if the endpoint is valid (has both name and URL)
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Url);
}
