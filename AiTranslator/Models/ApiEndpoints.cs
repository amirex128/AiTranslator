namespace AiTranslator.Models;

/// <summary>
/// API endpoints configuration with support for multiple endpoints per translation type
/// </summary>
public class ApiEndpoints
{
    /// <summary>
    /// English to Persian translation API configuration
    /// </summary>
    public TranslationApiConfig EnglishToPersian { get; set; } = new()
    {
        Endpoints = new List<EndpointInfo>
        {
            new() { Name = "API 1", Url = "http://127.0.0.1:3001/api/v1/prediction/2d8919bf-3426-4cf2-9a95-11f2539acff6" }
        },
        DefaultEndpointIndex = 0
    };

    /// <summary>
    /// Persian to English translation API configuration
    /// </summary>
    public TranslationApiConfig PersianToEnglish { get; set; } = new()
    {
        Endpoints = new List<EndpointInfo>
        {
            new() { Name = "API 1", Url = "http://127.0.0.1:3001/api/v1/prediction/1e86b0ba-1193-42e3-b0ac-d56720689b0f" }
        },
        DefaultEndpointIndex = 0
    };

    /// <summary>
    /// Grammar fix API configuration
    /// </summary>
    public TranslationApiConfig GrammarFix { get; set; } = new()
    {
        Endpoints = new List<EndpointInfo>
        {
            new() { Name = "API 1", Url = "http://127.0.0.1:3001/api/v1/prediction/2195ec5a-7b27-4fbb-8384-7c4765a6ae06" }
        },
        DefaultEndpointIndex = 0
    };

    /// <summary>
    /// Grammar Learner API configuration
    /// </summary>
    public TranslationApiConfig GrammarLearner { get; set; } = new()
    {
        Endpoints = new List<EndpointInfo>
        {
            new() { Name = "API 1", Url = "http://127.0.0.1:3001/api/v1/prediction/dfc9832f-8260-4089-91f2-6a6d8ccf6868" },
            new() { Name = "API 2", Url = "http://localhost:3001/api/v1/prediction/9a249ec0-c176-4d27-81c6-a65f39010289" }
        },
        DefaultEndpointIndex = 0
    };
}

