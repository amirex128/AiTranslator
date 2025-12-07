using AiTranslator.Models;

namespace AiTranslator.Services;

public class LanguageDetector : ILanguageDetector
{
    public Language DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Language.Unknown;

        // Count Persian and English characters
        int persianCount = 0;
        int englishCount = 0;

        foreach (char c in text)
        {
            // Persian Unicode range: 0600-06FF (Arabic/Persian script)
            if (c >= '\u0600' && c <= '\u06FF')
            {
                persianCount++;
            }
            // English alphabet
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                englishCount++;
            }
        }

        // Determine language based on character counts
        if (persianCount > englishCount)
            return Language.Persian;
        else if (englishCount > persianCount)
            return Language.English;
        else
            return Language.Unknown;
    }
}

