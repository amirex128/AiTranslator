namespace AiTranslator.Models;

public class TranslationResponse
{
    public string Text { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}

