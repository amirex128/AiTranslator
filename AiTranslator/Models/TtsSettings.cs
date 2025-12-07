namespace AiTranslator.Models;

public class TtsSettings
{
    public TtsProvider Provider { get; set; } = TtsProvider.Google;
    public string LocalAIEndpoint { get; set; } = "http://127.0.0.1:3002/tts";
    public string LocalAIModel { get; set; } = "tts-english";
    public string LocalAIResponseFormat { get; set; } = "wav";
    public int CacheExpirationDays { get; set; } = 30;
}
