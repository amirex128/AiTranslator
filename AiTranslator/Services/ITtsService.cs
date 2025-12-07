namespace AiTranslator.Services;

public interface ITtsService
{
    Task ReadPersianAsync(string text, CancellationToken cancellationToken = default);
    Task ReadEnglishAsync(string text, CancellationToken cancellationToken = default);
    void StopReading();
}

