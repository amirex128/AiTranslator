using AiTranslator.Models;

namespace AiTranslator.Services;

public interface ITranslationService
{
    Task<TranslationResponse> TranslateEnglishToPersianAsync(string text, CancellationToken cancellationToken = default);
    Task<TranslationResponse> TranslatePersianToEnglishAsync(string text, CancellationToken cancellationToken = default);
    Task<TranslationResponse> FixGrammarAsync(string text, CancellationToken cancellationToken = default);
}

