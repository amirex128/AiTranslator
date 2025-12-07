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
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();

                _config = new AppConfig();
                configuration.Bind(_config);
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

