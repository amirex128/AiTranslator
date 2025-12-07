namespace AiTranslator.Services;

public interface ITtsProvider
{
    Task<string> GenerateSpeechAsync(string text, string language, CancellationToken cancellationToken = default);
    Task PlayAudioAsync(string audioFilePath, CancellationToken cancellationToken = default);
    void StopPlayback();
}
