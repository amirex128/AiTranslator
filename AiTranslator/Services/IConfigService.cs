using AiTranslator.Models;

namespace AiTranslator.Services;

public interface IConfigService
{
    AppConfig Config { get; }
    void LoadConfiguration();
    void SaveConfiguration();
    void SaveWindowState(int x, int y, int width, int height);
}

