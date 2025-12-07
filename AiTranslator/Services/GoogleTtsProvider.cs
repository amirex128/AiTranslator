using System.Web;

namespace AiTranslator.Services;

public class GoogleTtsProvider : ITtsProvider
{
    private readonly ILoggingService _loggingService;
    private readonly TtsCacheManager _cacheManager;
    private readonly AudioPlayer _audioPlayer;
    private readonly string _tempDirectory;

    public GoogleTtsProvider(
        ILoggingService loggingService,
        TtsCacheManager cacheManager,
        AudioPlayer audioPlayer)
    {
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
            var cachedFile = _cacheManager.GetCachedAudio(text, language, "Google");
            if (cachedFile != null)
            {
                return cachedFile;
            }

            _loggingService.LogInformation($"Generating speech using Google TTS for {language}");

            // Google Translate TTS endpoint
            var languageCode = language.ToLower() == "persian" ? "fa" : "en";
            var encodedText = HttpUtility.UrlEncode(text);
            var url = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl={languageCode}&q={encodedText}";

            // Generate unique filename
            var fileName = $"tts_{Guid.NewGuid()}.mp3";
            var filePath = Path.Combine(_tempDirectory, fileName);

            // Download audio file
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            
            var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var audioData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(filePath, audioData, cancellationToken);

            _loggingService.LogInformation($"Audio file saved: {filePath}");

            // Cache the audio file
            _cacheManager.CacheAudio(text, language, "Google", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error generating speech with Google TTS", ex);
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
