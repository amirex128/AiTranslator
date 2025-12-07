using AiTranslator.Models;

namespace AiTranslator.Services;

public interface ILanguageDetector
{
    Language DetectLanguage(string text);
}

