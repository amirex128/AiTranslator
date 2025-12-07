namespace AiTranslator.Models;

public class ApiSettings
{
    public int TimeoutMinutes { get; set; } = 5;
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}

