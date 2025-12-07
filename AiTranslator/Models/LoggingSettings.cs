namespace AiTranslator.Models;

public class LoggingSettings
{
    public bool EnableLogging { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public string LogDirectory { get; set; } = "Logs";
    public int MaxLogFileSizeMB { get; set; } = 10;
    public int MaxLogFileCount { get; set; } = 5;
}

