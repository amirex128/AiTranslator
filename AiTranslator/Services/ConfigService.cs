using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiTranslator.Models;
using Microsoft.Extensions.Configuration;

namespace AiTranslator.Services;

public class ConfigService : IConfigService
{
    private readonly string _configFilePath;
    private readonly string _templateConfigFilePath;
    private readonly string _configHashFilePath;
    private AppConfig _config;

    public AppConfig Config => _config;

    public ConfigService()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _configFilePath = Path.Combine(appDirectory, "appsettings.json");
        
        // Find template appsettings.json in the project directory
        var projectDirectory = FindProjectDirectory();
        _templateConfigFilePath = Path.Combine(projectDirectory, "appsettings.json");
        
        // Hash file to track template changes
        _configHashFilePath = Path.Combine(appDirectory, "appsettings.json.hash");
        
        _config = new AppConfig();
        LoadConfiguration();
    }

    /// <summary>
    /// Finds the project directory containing appsettings.json template
    /// </summary>
    private string FindProjectDirectory()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // First, try to find it in the project source directory
        // Go up from bin/Debug or bin/Release to find the project folder
        var dir = new DirectoryInfo(baseDir);
        while (dir != null)
        {
            // Check if we're in a bin or obj folder
            if (dir.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) || 
                dir.Name.Equals("obj", StringComparison.OrdinalIgnoreCase))
            {
                // Go up one more level to get to the project folder
                var projectDir = dir.Parent;
                if (projectDir != null)
                {
                    var templatePath = Path.Combine(projectDir.FullName, "appsettings.json");
                    if (File.Exists(templatePath))
                    {
                        return projectDir.FullName;
                    }
                }
            }
            
            // Also check current directory for appsettings.json (if not in bin/obj)
            var currentTemplatePath = Path.Combine(dir.FullName, "appsettings.json");
            if (File.Exists(currentTemplatePath) && 
                !dir.FullName.Contains("bin") && 
                !dir.FullName.Contains("obj"))
            {
                return dir.FullName;
            }
            
            dir = dir.Parent;
        }
        
        // If not found, try common project structure paths
        var commonPaths = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "AiTranslator"),
            Path.Combine(baseDir, "..", "..", "AiTranslator"),
            Path.Combine(baseDir, "..", "AiTranslator"),
        };
        
        foreach (var path in commonPaths)
        {
            var normalizedPath = Path.GetFullPath(path);
            var templatePath = Path.Combine(normalizedPath, "appsettings.json");
            if (File.Exists(templatePath))
            {
                return normalizedPath;
            }
        }
        
        // Fallback to base directory (template might be in output directory)
        return baseDir;
    }

    /// <summary>
    /// Calculates SHA256 hash of a file
    /// </summary>
    private string CalculateFileHash(string filePath)
    {
        if (!File.Exists(filePath))
            return string.Empty;
        
        using (var sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }
    }

    /// <summary>
    /// Checks if template appsettings.json has changed
    /// </summary>
    private bool HasTemplateChanged()
    {
        if (!File.Exists(_templateConfigFilePath))
            return false;
        
        var currentHash = CalculateFileHash(_templateConfigFilePath);
        var savedHash = File.Exists(_configHashFilePath) ? File.ReadAllText(_configHashFilePath).Trim() : string.Empty;
        
        return currentHash != savedHash;
    }

    /// <summary>
    /// Saves the current template hash
    /// </summary>
    private void SaveTemplateHash()
    {
        if (File.Exists(_templateConfigFilePath))
        {
            var hash = CalculateFileHash(_templateConfigFilePath);
            File.WriteAllText(_configHashFilePath, hash);
        }
    }

    public void LoadConfiguration()
    {
        try
        {
            // Check if template has changed
            bool templateChanged = HasTemplateChanged();
            
            // If template changed or config file doesn't exist, load from template
            if (templateChanged || !File.Exists(_configFilePath))
            {
                if (File.Exists(_templateConfigFilePath))
                {
                    // Copy template to config file (reset user settings)
                    File.Copy(_templateConfigFilePath, _configFilePath, true);
                    Console.WriteLine("Template appsettings.json has changed. User settings have been reset.");
                }
                else if (!File.Exists(_configFilePath))
                {
                    // No template and no config - create default
                    _config = new AppConfig();
                    SaveConfiguration();
                    SaveTemplateHash();
                    return;
                }
            }
            
            // Load configuration from file
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                
                // Check if migration is needed BEFORE deserialization
                var needsMigration = CheckIfMigrationNeeded(json);
                
                if (needsMigration)
                {
                    // Migrate the JSON string first, then deserialize
                    json = MigrateJsonString(json);
                    // Save migrated JSON to file
                    File.WriteAllText(_configFilePath, json);
                }
                
                // Now deserialize the (possibly migrated) JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                _config = JsonSerializer.Deserialize<AppConfig>(json, options) ?? new AppConfig();
                
                // Migrate old format (List<string>) to new format (List<EndpointInfo>)
                MigrateEndpointsIfNeeded(_config);
                
                // Ensure all endpoints have TimeoutSeconds set (migrate if missing)
                MigrateTimeoutSecondsIfNeeded(_config);
            }
            else
            {
                // Create default configuration
                _config = new AppConfig();
                SaveConfiguration();
            }
            
            // Save template hash after successful load
            SaveTemplateHash();
        }
        catch (Exception ex)
        {
            // If config fails to load, try migration as fallback
            try
            {
                var json = File.ReadAllText(_configFilePath);
                json = MigrateJsonString(json);
                File.WriteAllText(_configFilePath, json);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };
                _config = JsonSerializer.Deserialize<AppConfig>(json, options) ?? new AppConfig();
                SaveTemplateHash();
            }
            catch
            {
                // If migration also fails, use defaults
                _config = new AppConfig();
                Console.WriteLine($"Error loading configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Checks if the JSON file needs migration from old format (List<string>) to new format (List<EndpointInfo>)
    /// </summary>
    private bool CheckIfMigrationNeeded(string json)
    {
        try
        {
            using (var doc = JsonDocument.Parse(json))
            {
                // Get apiEndpoints property (case-insensitive)
                JsonElement apiEndpointsElement;
                if (!doc.RootElement.TryGetProperty("apiEndpoints", out apiEndpointsElement) &&
                    !doc.RootElement.TryGetProperty("ApiEndpoints", out apiEndpointsElement))
                {
                    return false;
                }
                
                // Check each API endpoint config
                foreach (var apiProp in apiEndpointsElement.EnumerateObject())
                {
                    var endpointConfig = apiProp.Value;
                    
                    // Check for endpoints property (both cases)
                    JsonElement endpointsArray;
                    bool hasEndpoints = endpointConfig.TryGetProperty("endpoints", out endpointsArray) ||
                                      endpointConfig.TryGetProperty("Endpoints", out endpointsArray);
                    
                    if (hasEndpoints)
                    {
                        // Check if it's an array of strings (old format)
                        if (endpointsArray.ValueKind == System.Text.Json.JsonValueKind.Array && 
                            endpointsArray.GetArrayLength() > 0)
                        {
                            var firstElement = endpointsArray[0];
                            if (firstElement.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                return true; // Old format detected
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // If parsing fails, assume no migration needed (will be handled by deserialization)
        }
        
        return false;
    }

    /// <summary>
    /// Migrates JSON string from old format (List<string>) to new format (List<EndpointInfo>)
    /// Uses JsonDocument manipulation to preserve all other properties
    /// </summary>
    private string MigrateJsonString(string json)
    {
        try
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                
                // Get apiEndpoints property (case-insensitive)
                JsonElement apiEndpointsElement;
                if (!root.TryGetProperty("apiEndpoints", out apiEndpointsElement) &&
                    !root.TryGetProperty("ApiEndpoints", out apiEndpointsElement))
                {
                    // No apiEndpoints found, return original
                    return json;
                }
                
                var modified = false;
                var apiEndpointsDict = new Dictionary<string, object?>();
                
                // Process each API endpoint config
                foreach (var apiProp in apiEndpointsElement.EnumerateObject())
                {
                    var endpointConfig = apiProp.Value;
                    var endpointConfigDict = new Dictionary<string, object?>();
                    
                    // Copy all existing properties except endpoints
                    foreach (var epProp in endpointConfig.EnumerateObject())
                    {
                        if (!epProp.Name.Equals("endpoints", StringComparison.OrdinalIgnoreCase) &&
                            !epProp.Name.Equals("Endpoints", StringComparison.OrdinalIgnoreCase))
                        {
                            endpointConfigDict[epProp.Name] = JsonSerializer.Deserialize<object>(epProp.Value.GetRawText());
                        }
                    }
                    
                    // Check for endpoints property (both cases)
                    JsonElement endpointsArray;
                    bool hasEndpoints = endpointConfig.TryGetProperty("endpoints", out endpointsArray) ||
                                      endpointConfig.TryGetProperty("Endpoints", out endpointsArray);
                    
                    if (hasEndpoints)
                    {
                        // Check if it's an array of strings (old format)
                        if (endpointsArray.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            if (endpointsArray.GetArrayLength() > 0)
                            {
                                var firstElement = endpointsArray[0];
                                if (firstElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    // Old format detected - migrate it
                                    var migratedEndpoints = new List<Dictionary<string, object>>();
                                    
                                    for (int i = 0; i < endpointsArray.GetArrayLength() && i < 4; i++)
                                    {
                                        var url = endpointsArray[i].GetString() ?? string.Empty;
                                        migratedEndpoints.Add(new Dictionary<string, object>
                                        {
                                            { "name", $"API {i + 1}" },
                                            { "url", url },
                                            { "timeoutSeconds", 30 }
                                        });
                                    }
                                    
                                    // Ensure we have 4 endpoints
                                    while (migratedEndpoints.Count < 4)
                                    {
                                        migratedEndpoints.Add(new Dictionary<string, object>
                                        {
                                            { "name", $"API {migratedEndpoints.Count + 1}" },
                                            { "url", string.Empty },
                                            { "timeoutSeconds", 30 }
                                        });
                                    }
                                    
                                    endpointConfigDict["endpoints"] = migratedEndpoints;
                                    modified = true;
                                }
                                else
                                {
                                    // Already in new format, copy as-is
                                    endpointConfigDict["endpoints"] = JsonSerializer.Deserialize<object>(endpointsArray.GetRawText());
                                }
                            }
                            else
                            {
                                // Empty array, create default structure
                                var migratedEndpoints = new List<Dictionary<string, object>>();
                                for (int i = 0; i < 4; i++)
                                {
                                    migratedEndpoints.Add(new Dictionary<string, object>
                                    {
                                        { "name", $"API {i + 1}" },
                                        { "url", string.Empty },
                                        { "timeoutSeconds", 30 }
                                    });
                                }
                                endpointConfigDict["endpoints"] = migratedEndpoints;
                                modified = true;
                            }
                        }
                    }
                    else
                    {
                        // No endpoints property, create default structure
                        var migratedEndpoints = new List<Dictionary<string, object>>();
                        for (int i = 0; i < 4; i++)
                        {
                            migratedEndpoints.Add(new Dictionary<string, object>
                            {
                                { "name", $"API {i + 1}" },
                                { "url", string.Empty },
                                { "timeoutSeconds", 30 }
                            });
                        }
                        endpointConfigDict["endpoints"] = migratedEndpoints;
                        modified = true;
                    }
                    
                    apiEndpointsDict[apiProp.Name] = endpointConfigDict;
                }
                
                if (modified)
                {
                    // Rebuild the entire config with migrated apiEndpoints
                    var rootDict = new Dictionary<string, object?>();
                    
                    // Copy all root properties
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (!prop.Name.Equals("apiEndpoints", StringComparison.OrdinalIgnoreCase) &&
                            !prop.Name.Equals("ApiEndpoints", StringComparison.OrdinalIgnoreCase))
                        {
                            rootDict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                        }
                    }
                    
                    // Add migrated apiEndpoints
                    rootDict["apiEndpoints"] = apiEndpointsDict;
                    
                    // Serialize back to JSON
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    return JsonSerializer.Serialize(rootDict, options);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during migration: {ex.Message}");
            // Return original JSON if migration fails
        }
        
        return json;
    }

    /// <summary>
    /// Ensures all endpoints have TimeoutSeconds set (migrates if missing)
    /// </summary>
    private void MigrateTimeoutSecondsIfNeeded(AppConfig config)
    {
        try
        {
            var needsSave = false;
            var configs = new[]
            {
                config.ApiEndpoints.EnglishToPersian,
                config.ApiEndpoints.PersianToEnglish,
                config.ApiEndpoints.GrammarFix,
                config.ApiEndpoints.GrammarLearner
            };

            foreach (var apiConfig in configs)
            {
                if (apiConfig?.Endpoints == null)
                    continue;

                foreach (var endpoint in apiConfig.Endpoints)
                {
                    if (endpoint != null && endpoint.TimeoutSeconds <= 0)
                    {
                        endpoint.TimeoutSeconds = 30; // Default timeout
                        needsSave = true;
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
                                                    Url = url,
                                                    TimeoutSeconds = 30 // Default timeout
                                                });
                                            }
                                            
                                            // Ensure we have 4 endpoints
                                            while (apiConfig.Endpoints.Count < 4)
                                            {
                                                apiConfig.Endpoints.Add(new EndpointInfo
                                                {
                                                    Name = $"API {apiConfig.Endpoints.Count + 1}",
                                                    Url = string.Empty,
                                                    TimeoutSeconds = 30 // Default timeout
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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
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