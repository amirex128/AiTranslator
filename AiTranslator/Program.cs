using AiTranslator.Forms;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator;

static class Program
{
    private const string AppGuid = "8B5C4F2E-9D3A-4E1B-A7F6-3C9E8D2F1A5B";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Single instance enforcement
        using var singleInstance = new SingleInstanceManager(AppGuid);
        
        if (!singleInstance.IsFirstInstance)
        {
            // Another instance is running, bring it to front and exit
            SingleInstanceManager.BringExistingInstanceToFront();
            return;
        }

        // Initialize application configuration
        ApplicationConfiguration.Initialize();

        try
        {
            // Initialize services (Dependency Injection by hand)
            var configService = new ConfigService();
            var loggingService = new LoggingService(configService);
            
            loggingService.LogInformation("Application starting");

            var languageDetector = new LanguageDetector();
            var notificationService = new NotificationService(configService);
            var translationService = new TranslationService(configService, loggingService, notificationService);
            var grammarLearnerService = new GrammarLearnerService(configService, loggingService, notificationService);
            
            // Initialize TTS components
            var ttsCacheManager = new TtsCacheManager(configService, loggingService);
            var audioPlayer = new AudioPlayer(loggingService);
            var ttsProviderFactory = new TtsProviderFactory(configService, loggingService, ttsCacheManager, audioPlayer);
            var ttsService = new TtsService(ttsProviderFactory, loggingService);
            
            var hotkeyManager = new HotkeyManager();
            var clipboardManager = new ClipboardManager();
            var selectionManager = new SelectionManager(clipboardManager);
            
            var hotkeyActions = new HotkeyActions(
                translationService,
                ttsService,
                languageDetector,
                configService,
                loggingService,
                notificationService,
                clipboardManager,
                selectionManager,
                grammarLearnerService
            );

            var mainForm = new MainForm(
                configService,
                translationService,
                ttsService,
                languageDetector,
                loggingService,
                hotkeyManager,
                clipboardManager,
                grammarLearnerService
            );

            var appContext = new ApplicationContext(
                mainForm,
                hotkeyManager,
                hotkeyActions,
                configService,
                loggingService,
                notificationService
            );

            // Set up auto-start if configured
            if (configService.Config.StartWithWindows && !AutoStartManager.IsAutoStartEnabled())
            {
                AutoStartManager.SetAutoStart(true);
            }

            Application.Run(appContext);

            loggingService.LogInformation("Application exiting normally");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fatal error: {ex.Message}\n\nPlease check the logs for details.",
                "Application Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}