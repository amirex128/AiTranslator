using AiTranslator.Models;

namespace AiTranslator.Services;

public class LoggingService : ILoggingService
{
    private readonly IConfigService _configService;
    private readonly object _lockObject = new();
    private string? _currentLogFile;

    public LoggingService(IConfigService configService)
    {
        _configService = configService;
        InitializeLogging();
    }

    private void InitializeLogging()
    {
        if (!_configService.Config.Logging.EnableLogging)
            return;

        var logDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            _configService.Config.Logging.LogDirectory
        );

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Clean up old log files
        CleanupOldLogs(logDirectory);

        // Create current log file
        var timestamp = DateTime.Now.ToString("yyyyMMdd");
        _currentLogFile = Path.Combine(logDirectory, $"AiTranslator_{timestamp}.log");
    }

    private void CleanupOldLogs(string logDirectory)
    {
        try
        {
            var logFiles = Directory.GetFiles(logDirectory, "AiTranslator_*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            var maxCount = _configService.Config.Logging.MaxLogFileCount;
            var maxSizeBytes = _configService.Config.Logging.MaxLogFileSizeMB * 1024 * 1024;

            // Remove old files if exceeding count
            foreach (var file in logFiles.Skip(maxCount))
            {
                file.Delete();
            }

            // Check current log file size
            if (logFiles.Any() && logFiles[0].Length > maxSizeBytes)
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _currentLogFile = Path.Combine(logDirectory, $"AiTranslator_{timestamp}.log");
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private void WriteLog(string level, string message)
    {
        if (!_configService.Config.Logging.EnableLogging || string.IsNullOrEmpty(_currentLogFile))
            return;

        var configLevel = _configService.Config.Logging.LogLevel.ToLower();
        if (!ShouldLog(level, configLevel))
            return;

        lock (_lockObject)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_currentLogFile, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    private bool ShouldLog(string level, string configLevel)
    {
        var levels = new[] { "debug", "information", "warning", "error" };
        var levelIndex = Array.IndexOf(levels, level.ToLower());
        var configLevelIndex = Array.IndexOf(levels, configLevel);

        return levelIndex >= configLevelIndex;
    }

    public void LogInformation(string message)
    {
        WriteLog("Information", message);
    }

    public void LogWarning(string message)
    {
        WriteLog("Warning", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message} - Exception: {exception.Message}{Environment.NewLine}{exception.StackTrace}"
            : message;
        WriteLog("Error", fullMessage);
    }

    public void LogDebug(string message)
    {
        WriteLog("Debug", message);
    }
}

