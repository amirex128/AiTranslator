namespace AiTranslator.Models;

public class WindowSettings
{
    public int DefaultWidth { get; set; } = 900;
    public int DefaultHeight { get; set; } = 700;
    public int MinWidth { get; set; } = 600;
    public int MinHeight { get; set; } = 400;
    public bool RememberPosition { get; set; } = true;
    public int? LastX { get; set; }
    public int? LastY { get; set; }
    public int? LastWidth { get; set; }
    public int? LastHeight { get; set; }
}

