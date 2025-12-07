namespace AiTranslator.Models;

public class UiSettings
{
    public int PopupAutoCloseSeconds { get; set; } = 10;
    public bool ShowNotifications { get; set; } = true;
    public bool ShowCharacterCount { get; set; } = true;
    public string Theme { get; set; } = "Modern";
    public float MainPageFontSize { get; set; } = 10F;
    public float PopupFontSize { get; set; } = 10F;
    public float SelectionFormFontSize { get; set; } = 9F;
}

