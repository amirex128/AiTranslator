namespace AiTranslator.Models;

public class HotkeySettings
{
    public HotkeyConfig TranslatePersianToEnglish { get; set; } = new() { Key = "F1", Ctrl = true, Alt = true };
    public HotkeyConfig TranslateEnglishToPersian { get; set; } = new() { Key = "F2", Ctrl = true, Alt = true };
    public HotkeyConfig TranslateGrammarFix { get; set; } = new() { Key = "F3", Ctrl = true, Alt = true };
    public HotkeyConfig ReadPersian { get; set; } = new() { Key = "F7", Ctrl = true, Alt = true };
    public HotkeyConfig ReadEnglish { get; set; } = new() { Key = "F8", Ctrl = true, Alt = true };
}

