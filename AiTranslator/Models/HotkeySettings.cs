namespace AiTranslator.Models;

public class HotkeySettings
{
    public HotkeyConfig PopupTranslatePersianToEnglish { get; set; } = new() { Key = "F1", Ctrl = true, Alt = true };
    public HotkeyConfig PopupTranslateEnglishToPersian { get; set; } = new() { Key = "F2", Ctrl = true, Alt = true };
    public HotkeyConfig PopupTranslateGrammarFix { get; set; } = new() { Key = "F3", Ctrl = true, Alt = true };
    public HotkeyConfig ClipboardReplacePersianToEnglish { get; set; } = new() { Key = "F4", Ctrl = true, Alt = true };
    public HotkeyConfig ClipboardReplaceEnglishToPersian { get; set; } = new() { Key = "F5", Ctrl = true, Alt = true };
    public HotkeyConfig ClipboardReplaceGrammarFix { get; set; } = new() { Key = "F6", Ctrl = true, Alt = true };
    public HotkeyConfig ReadPersian { get; set; } = new() { Key = "F7", Ctrl = true, Alt = true };
    public HotkeyConfig ReadEnglish { get; set; } = new() { Key = "F8", Ctrl = true, Alt = true };
    public HotkeyConfig AutoDetectTranslate { get; set; } = new() { Key = "F9", Ctrl = true, Alt = true };
    public HotkeyConfig AutoDetectRead { get; set; } = new() { Key = "F10", Ctrl = true, Alt = true };
    public HotkeyConfig UndoClipboard { get; set; } = new() { Key = "Z", Ctrl = true, Alt = true, Shift = true };
}

