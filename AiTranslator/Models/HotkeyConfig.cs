namespace AiTranslator.Models;

public class HotkeyConfig
{
    public string Key { get; set; } = string.Empty;
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }

    public override string ToString()
    {
        var parts = new List<string>();
        if (Ctrl) parts.Add("Ctrl");
        if (Alt) parts.Add("Alt");
        if (Shift) parts.Add("Shift");
        if (Win) parts.Add("Win");
        parts.Add(Key);
        return string.Join(" + ", parts);
    }
}

