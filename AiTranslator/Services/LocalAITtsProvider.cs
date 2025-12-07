using System.Text;
using System.Text.Json;

namespace AiTranslator.Services;

public class LocalAITtsProvider : ITtsProvider
{
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private readonly TtsCacheManager _cacheManager;
    private readonly AudioPlayer _audioPlayer;
    private readonly string _tempDirectory;

    public LocalAITtsProvider(
        IConfigService configService,
        ILoggingService loggingService,
        TtsCacheManager cacheManager,
        AudioPlayer audioPlayer)
    {
        _configService = configService;
        _loggingService = loggingService;
        _cacheManager = cacheManager;
        _audioPlayer = audioPlayer;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "AiTranslator", "TTS");
        Directory.CreateDirectory(_tempDirectory);
    }

    public async Task<string> GenerateSpeechAsync(string text, string language, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check cache first
            var cachedFile = _cacheManager.GetCachedAudio(text, language, "LocalAI");
            if (cachedFile != null)
            {
                return cachedFile;
            }

            _loggingService.LogInformation($"Generating speech using LocalAI for {language}");

            var settings = _configService.Config.TtsSettings;
            var endpoint = settings.LocalAIEndpoint;

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("LocalAI endpoint is not configured");
            }

            // Only supports English
            if (language.ToLower() != "english")
            {
                throw new NotSupportedException("LocalAI TTS only supports English text");
            }

            // Prepare request
            var requestBody = new
            {
                model = settings.LocalAIModel,
                input = text,
                response_format = settings.LocalAIResponseFormat
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Generate unique filename
            var extension = settings.LocalAIResponseFormat.ToLower();
            var fileName = $"tts_{Guid.NewGuid()}.{extension}";
            var filePath = Path.Combine(_tempDirectory, fileName);

            // Send request
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Save audio file
            var audioData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(filePath, audioData, cancellationToken);

            _loggingService.LogInformation($"Audio file saved: {filePath}");

            // Cache the audio file
            _cacheManager.CacheAudio(text, language, "LocalAI", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error generating speech with LocalAI", ex);
            throw;
        }
    }

    public async Task PlayAudioAsync(string audioFilePath, CancellationToken cancellationToken = default)
    {
        await _audioPlayer.PlayAsync(audioFilePath, cancellationToken);
    }

    public void StopPlayback()
    {
        _audioPlayer.Stop();
    }
}
