using System.Text.Json;
using AiTranslator.Models;
using Microsoft.Extensions.Configuration;

namespace AiTranslator.Services;

public class ConfigService : IConfigService
{
    private readonly string _configFilePath;
    private AppConfig _config;

    public AppConfig Config => _config;

    public ConfigService()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _configFilePath = Path.Combine(appDirectory, "appsettings.json");
        _config = new AppConfig();
        LoadConfiguration();
    }

    public void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                
                // Try to deserialize with migration support
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                // First, try to parse as dynamic to check structure
                using (var doc = JsonDocument.Parse(json))
                {
                    _config = JsonSerializer.Deserialize<AppConfig>(json, options) ?? new AppConfig();
                    
                    // Migrate old format (List<string>) to new format (List<EndpointInfo>)
                    MigrateEndpointsIfNeeded(_config);
                }
            }
            else
            {
                // Create default configuration
                _config = new AppConfig();
                SaveConfiguration();
            }
        }
        catch (Exception ex)
        {
            // If config fails to load, use defaults
            _config = new AppConfig();
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }

    private void MigrateEndpointsIfNeeded(AppConfig config)
    {
        try
        {
            var json = File.ReadAllText(_configFilePath);
            var needsSave = false;
            
            // Check if the JSON contains old format (array of strings)
            if (json.Contains("\"Endpoints\": [") && json.Contains("\"http://"))
            {
                // Try to detect old format by checking if endpoints are strings
                using (var doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.TryGetProperty("apiEndpoints", out var apiEndpoints))
                    {
                        var configs = new[]
                        {
                            ("englishToPersian", config.ApiEndpoints.EnglishToPersian),
                            ("persianToEnglish", config.ApiEndpoints.PersianToEnglish),
                            ("grammarFix", config.ApiEndpoints.GrammarFix),
                            ("grammarLearner", config.ApiEndpoints.GrammarLearner)
                        };

                        foreach (var (key, apiConfig) in configs)
                        {
                            if (apiEndpoints.TryGetProperty(key, out var endpointConfig))
                            {
                                if (endpointConfig.TryGetProperty("endpoints", out var endpointsArray))
                                {
                                    // Check if it's an array of strings (old format)
                                    if (endpointsArray.ValueKind == System.Text.Json.JsonValueKind.Array && 
                                        endpointsArray.GetArrayLength() > 0)
                                    {
                                        var firstElement = endpointsArray[0];
                                        if (firstElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                        {
                                            // Migrate from List<string> to List<EndpointInfo>
                                            apiConfig.Endpoints.Clear();
                                            
                                            for (int i = 0; i < endpointsArray.GetArrayLength() && i < 4; i++)
                                            {
                                                var url = endpointsArray[i].GetString() ?? string.Empty;
                                                apiConfig.Endpoints.Add(new EndpointInfo
                                                {
                                                    Name = $"API {i + 1}",
                                                    Url = url
                                                });
                                            }
                                            
                                            // Ensure we have 4 endpoints
                                            while (apiConfig.Endpoints.Count < 4)
                                            {
                                                apiConfig.Endpoints.Add(new EndpointInfo
                                                {
                                                    Name = $"API {apiConfig.Endpoints.Count + 1}",
                                                    Url = string.Empty
                                                });
                                            }
                                            
                                            needsSave = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (needsSave)
            {
                SaveConfiguration();
            }
        }
        catch
        {
            // If migration fails, continue with current config
        }
    }

    public void SaveConfiguration()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
            throw;
        }
    }

    public void SaveWindowState(int x, int y, int width, int height)
    {
        if (_config.Window.RememberPosition)
        {
            _config.Window.LastX = x;
            _config.Window.LastY = y;
            _config.Window.LastWidth = width;
            _config.Window.LastHeight = height;
            SaveConfiguration();
        }
    }
}

