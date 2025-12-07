namespace AiTranslator.Models;

public class AppConfig
{
    public ApiEndpoints ApiEndpoints { get; set; } = new();
    public TtsEndpoints TtsEndpoints { get; set; } = new();
    public HotkeySettings Hotkeys { get; set; } = new();
    public WindowSettings Window { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public UiSettings Ui { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public bool StartWithWindows { get; set; } = false;
}

