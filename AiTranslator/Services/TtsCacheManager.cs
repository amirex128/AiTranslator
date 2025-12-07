using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AiTranslator.Services;

public class TtsCacheManager
{
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private readonly string _cacheDirectory;
    private readonly string _cacheIndexFile;

    public TtsCacheManager(IConfigService configService, ILoggingService loggingService)
    {
        _configService = configService;
        _loggingService = loggingService;
        _cacheDirectory = Path.Combine(Path.GetTempPath(), "AiTranslator", "TTS_Cache");
        _cacheIndexFile = Path.Combine(_cacheDirectory, "cache_index.json");
        
        Directory.CreateDirectory(_cacheDirectory);
        CleanupExpiredCache();
    }

    public string? GetCachedAudio(string text, string language, string provider)
    {
        try
        {
            var cacheKey = GenerateCacheKey(text, language, provider);
            var cacheFilePath = Path.Combine(_cacheDirectory, $"{cacheKey}.audio");

            if (File.Exists(cacheFilePath))
            {
                var cacheInfo = GetCacheInfo(cacheKey);
                if (cacheInfo != null)
                {
                    var expirationDays = _configService.Config.TtsSettings.CacheExpirationDays;
                    var expirationDate = cacheInfo.CreatedAt.AddDays(expirationDays);

                    if (DateTime.UtcNow < expirationDate)
                    {
                        _loggingService.LogInformation($"Cache hit for: {text.Substring(0, Math.Min(50, text.Length))}...");
                        return cacheFilePath;
                    }
                    else
                    {
                        // Cache expired, delete it
                        DeleteCacheEntry(cacheKey);
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error retrieving cached audio", ex);
            return null;
        }
    }

    public void CacheAudio(string text, string language, string provider, string audioFilePath)
    {
        try
        {
            var cacheKey = GenerateCacheKey(text, language, provider);
            var cacheFilePath = Path.Combine(_cacheDirectory, $"{cacheKey}.audio");

            // Copy audio file to cache
            File.Copy(audioFilePath, cacheFilePath, true);

            // Update cache index
            var cacheInfo = new CacheInfo
            {
                CacheKey = cacheKey,
                Text = text.Substring(0, Math.Min(100, text.Length)), // Store only first 100 chars for reference
                Language = language,
                Provider = provider,
                CreatedAt = DateTime.UtcNow,
                FilePath = cacheFilePath
            };

            SaveCacheInfo(cacheInfo);
            _loggingService.LogInformation($"Audio cached with key: {cacheKey}");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error caching audio", ex);
        }
    }

    private string GenerateCacheKey(string text, string language, string provider)
    {
        var input = $"{text}|{language}|{provider}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    private CacheInfo? GetCacheInfo(string cacheKey)
    {
        try
        {
            if (!File.Exists(_cacheIndexFile))
                return null;

            var json = File.ReadAllText(_cacheIndexFile);
            var cacheIndex = JsonSerializer.Deserialize<Dictionary<string, CacheInfo>>(json);

            return cacheIndex?.GetValueOrDefault(cacheKey);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error reading cache index", ex);
            return null;
        }
    }

    private void SaveCacheInfo(CacheInfo cacheInfo)
    {
        try
        {
            Dictionary<string, CacheInfo> cacheIndex;

            if (File.Exists(_cacheIndexFile))
            {
                var json = File.ReadAllText(_cacheIndexFile);
                cacheIndex = JsonSerializer.Deserialize<Dictionary<string, CacheInfo>>(json) ?? new Dictionary<string, CacheInfo>();
            }
            else
            {
                cacheIndex = new Dictionary<string, CacheInfo>();
            }

            cacheIndex[cacheInfo.CacheKey] = cacheInfo;

            var updatedJson = JsonSerializer.Serialize(cacheIndex, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_cacheIndexFile, updatedJson);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error saving cache index", ex);
        }
    }

    private void DeleteCacheEntry(string cacheKey)
    {
        try
        {
            var cacheFilePath = Path.Combine(_cacheDirectory, $"{cacheKey}.audio");
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }

            // Remove from index
            if (File.Exists(_cacheIndexFile))
            {
                var json = File.ReadAllText(_cacheIndexFile);
                var cacheIndex = JsonSerializer.Deserialize<Dictionary<string, CacheInfo>>(json);

                if (cacheIndex != null && cacheIndex.ContainsKey(cacheKey))
                {
                    cacheIndex.Remove(cacheKey);
                    var updatedJson = JsonSerializer.Serialize(cacheIndex, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_cacheIndexFile, updatedJson);
                }
            }

            _loggingService.LogInformation($"Cache entry deleted: {cacheKey}");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error deleting cache entry", ex);
        }
    }

    private void CleanupExpiredCache()
    {
        try
        {
            if (!File.Exists(_cacheIndexFile))
                return;

            var json = File.ReadAllText(_cacheIndexFile);
            var cacheIndex = JsonSerializer.Deserialize<Dictionary<string, CacheInfo>>(json);

            if (cacheIndex == null)
                return;

            var expirationDays = _configService.Config.TtsSettings.CacheExpirationDays;
            var expiredKeys = new List<string>();

            foreach (var kvp in cacheIndex)
            {
                var expirationDate = kvp.Value.CreatedAt.AddDays(expirationDays);
                if (DateTime.UtcNow >= expirationDate)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                DeleteCacheEntry(key);
            }

            if (expiredKeys.Count > 0)
            {
                _loggingService.LogInformation($"Cleaned up {expiredKeys.Count} expired cache entries");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error cleaning up expired cache", ex);
        }
    }

    public void ClearAllCache()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
                Directory.CreateDirectory(_cacheDirectory);
                _loggingService.LogInformation("All TTS cache cleared");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error clearing cache", ex);
        }
    }

    private class CacheInfo
    {
        public string CacheKey { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
