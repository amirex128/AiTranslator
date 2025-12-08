using AiTranslator.Forms;
using AiTranslator.Models;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator;

public class ApplicationContext : System.Windows.Forms.ApplicationContext
{
    private static ApplicationContext? _instance;
    public static ApplicationContext? Instance => _instance;

    private readonly MainForm _mainForm;
    private NotifyIcon? _notifyIcon;
    private readonly HotkeyManager _hotkeyManager;
    private readonly HotkeyActions _hotkeyActions;
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private readonly NotificationService _notificationService;

    public ApplicationContext(
        MainForm mainForm,
        HotkeyManager hotkeyManager,
        HotkeyActions hotkeyActions,
        IConfigService configService,
        ILoggingService loggingService,
        NotificationService notificationService)
    {
        _mainForm = mainForm;
        _hotkeyManager = hotkeyManager;
        _hotkeyActions = hotkeyActions;
        _configService = configService;
        _loggingService = loggingService;
        _notificationService = notificationService;

        _instance = this;

        InitializeSystemTray();
        RegisterHotkeys();

        // Show main form on startup
        _mainForm.Show();
    }

    private void InitializeSystemTray()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "AI Translator",
            Visible = true
        };

        // Set the NotifyIcon in the notification service
        _notificationService.SetNotifyIcon(_notifyIcon);

        var contextMenu = new ContextMenuStrip();
        
        var openMenuItem = new ToolStripMenuItem("Open", null, OnOpenMenuItem);
        var exitMenuItem = new ToolStripMenuItem("Exit", null, OnExitMenuItem);
        
        contextMenu.Items.Add(openMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitMenuItem);
        
        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
    }

    public void RegisterHotkeys()
    {
        // Unregister all existing hotkeys first
        _hotkeyManager.UnregisterAllHotkeys();

        _hotkeyManager.Initialize(_mainForm.Handle);

        var hotkeys = _configService.Config.Hotkeys;

        try
        {
            // Unified translations (handles both edit and replace modes)
            _hotkeyManager.RegisterHotkey(
                hotkeys.TranslatePersianToEnglish,
                () => _hotkeyActions.Translate(TranslationType.PersianToEnglish)
            );

            _hotkeyManager.RegisterHotkey(
                hotkeys.TranslateEnglishToPersian,
                () => _hotkeyActions.Translate(TranslationType.EnglishToPersian)
            );

            _hotkeyManager.RegisterHotkey(
                hotkeys.TranslateGrammarFix,
                () => _hotkeyActions.Translate(TranslationType.GrammarFix)
            );

            // Read text
            _hotkeyManager.RegisterHotkey(
                hotkeys.ReadPersian,
                () => _hotkeyActions.ReadClipboardText(Language.Persian)
            );

            _hotkeyManager.RegisterHotkey(
                hotkeys.ReadEnglish,
                () => _hotkeyActions.ReadClipboardText(Language.English)
            );

            // Auto-detect
            _hotkeyManager.RegisterHotkey(
                hotkeys.AutoDetectTranslate,
                () => _hotkeyActions.AutoDetectAndTranslate()
            );

            _hotkeyManager.RegisterHotkey(
                hotkeys.AutoDetectRead,
                () => _hotkeyActions.AutoDetectAndRead()
            );

            // Undo clipboard
            _hotkeyManager.RegisterHotkey(
                hotkeys.UndoClipboard,
                () => _hotkeyActions.UndoClipboard()
            );

            _loggingService.LogInformation("All hotkeys registered successfully");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error registering hotkeys", ex);
            MessageBox.Show(
                "Some hotkeys could not be registered. They may be in use by another application.",
                "Hotkey Registration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }

    private void OnOpenMenuItem(object? sender, EventArgs e)
    {
        _mainForm.ShowMainWindow();
    }

    private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
    {
        _mainForm.ShowMainWindow();
    }

    private void OnExitMenuItem(object? sender, EventArgs e)
    {
        _loggingService.LogInformation("Application exiting");
        
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
        }
        _hotkeyManager.UnregisterAllHotkeys();
        
        _mainForm.Close();
        Application.Exit();
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon?.Dispose();
        }
        base.Dispose(disposing);
    }
}

