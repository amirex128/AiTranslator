using AiTranslator.Models;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator.Forms;

public class SettingsForm : Form
{
    private readonly IConfigService _configService;
    private TabControl tabControl = null!;
    
    // API Configuration Tab
    private TranslationApiConfigControl enToFaConfigControl = null!;
    private TranslationApiConfigControl faToEnConfigControl = null!;
    private TranslationApiConfigControl grammarFixConfigControl = null!;
    private TranslationApiConfigControl grammarLearnerConfigControl = null!;
    
    // UI Preferences Tab
    private NumericUpDown popupTimeoutNumeric = null!;
    private CheckBox showNotificationsCheckBox = null!;
    private CheckBox showCharacterCountCheckBox = null!;
    private CheckBox rememberWindowPositionCheckBox = null!;
    private CheckBox startWithWindowsCheckBox = null!;
    private NumericUpDown mainPageFontSizeNumeric = null!;
    private NumericUpDown popupFontSizeNumeric = null!;
    private NumericUpDown grammarLearnerFontSizeNumeric = null!;
    
    // Advanced Tab
    private NumericUpDown retryCountNumeric = null!;
    private ComboBox logLevelComboBox = null!;
    private CheckBox enableLoggingCheckBox = null!;
    private ComboBox ttsProviderComboBox = null!;
    private TextBox localAIEndpointTextBox = null!;
    private TextBox localAIModelTextBox = null!;
    private ComboBox localAIFormatComboBox = null!;
    private NumericUpDown cacheExpirationDaysNumeric = null!;
    private Button clearCacheButton = null!;
    
    // Shortcuts Tab
    private HotkeyConfigControl translatePersianToEnglishControl = null!;
    private HotkeyConfigControl translateEnglishToPersianControl = null!;
    private HotkeyConfigControl translateGrammarFixControl = null!;
    private HotkeyConfigControl readPersianControl = null!;
    private HotkeyConfigControl readEnglishControl = null!;
    
    private Button saveButton = null!;
    private Button cancelButton = null!;

    public SettingsForm(IConfigService configService)
    {
        _configService = configService;
        InitializeComponents();
        LoadSettings();
    }

    public void SelectShortcutsTab()
    {
        // Find and select the Shortcuts tab
        foreach (TabPage tab in tabControl.TabPages)
        {
            if (tab.Text == "Shortcuts")
            {
                tabControl.SelectedTab = tab;
                break;
            }
        }
    }

    private void InitializeComponents()
    {
        this.Text = "Settings";
        this.Size = new Size(600, 650);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Create tabs
        tabControl.TabPages.Add(CreateApiConfigTab());
        tabControl.TabPages.Add(CreateUiPreferencesTab());
        tabControl.TabPages.Add(CreateShortcutsTab());
        tabControl.TabPages.Add(CreateAdvancedTab());

        var buttonsPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50
        };

        saveButton = new Button
        {
            Text = "Save",
            Location = new Point(400, 10),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        saveButton.Click += OnSaveButtonClick;

        cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(490, 10),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };

        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(saveButton);

        mainPanel.Controls.Add(tabControl);
        this.Controls.Add(mainPanel);
        this.Controls.Add(buttonsPanel);

        this.AcceptButton = saveButton;
        this.CancelButton = cancelButton;
    }

    private TabPage CreateApiConfigTab()
    {
        var tab = new TabPage("API Configuration");
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };

        var config = _configService.Config;
        int y = 10;

        // English to Persian
        enToFaConfigControl = new TranslationApiConfigControl(
            "English to Persian Translation",
            config.ApiEndpoints.EnglishToPersian,
            y,
            panel);
        enToFaConfigControl.LoadFromConfig();
        y += 185; // Space for one config control

        // Persian to English
        faToEnConfigControl = new TranslationApiConfigControl(
            "Persian to English Translation",
            config.ApiEndpoints.PersianToEnglish,
            y,
            panel);
        faToEnConfigControl.LoadFromConfig();
        y += 185;

        // Grammar Fix
        grammarFixConfigControl = new TranslationApiConfigControl(
            "Grammar Fix",
            config.ApiEndpoints.GrammarFix,
            y,
            panel);
        grammarFixConfigControl.LoadFromConfig();
        y += 185;

        // Grammar Learner
        grammarLearnerConfigControl = new TranslationApiConfigControl(
            "Grammar Learner",
            config.ApiEndpoints.GrammarLearner,
            y,
            panel);
        grammarLearnerConfigControl.LoadFromConfig();

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateUiPreferencesTab()
    {
        var tab = new TabPage("UI Preferences");
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        int y = 10;

        // Popup Timeout
        var popupTimeoutLabel = new Label
        {
            Text = "Popup Auto-close Timeout (seconds):",
            Location = new Point(10, y),
            Size = new Size(250, 20)
        };
        popupTimeoutNumeric = new NumericUpDown
        {
            Location = new Point(270, y),
            Size = new Size(100, 20),
            Minimum = 1,
            Maximum = 60,
            Value = 10
        };
        panel.Controls.Add(popupTimeoutLabel);
        panel.Controls.Add(popupTimeoutNumeric);
        y += 40;

        // Show Notifications
        showNotificationsCheckBox = new CheckBox
        {
            Text = "Show notifications for errors and completions",
            Location = new Point(10, y),
            Size = new Size(400, 20),
            Checked = true
        };
        panel.Controls.Add(showNotificationsCheckBox);
        y += 30;

        // Show Character Count
        showCharacterCountCheckBox = new CheckBox
        {
            Text = "Show character and word count",
            Location = new Point(10, y),
            Size = new Size(400, 20),
            Checked = true
        };
        panel.Controls.Add(showCharacterCountCheckBox);
        y += 30;

        // Remember Window Position
        rememberWindowPositionCheckBox = new CheckBox
        {
            Text = "Remember window size and position",
            Location = new Point(10, y),
            Size = new Size(400, 20),
            Checked = true
        };
        panel.Controls.Add(rememberWindowPositionCheckBox);
        y += 30;

        // Start with Windows
        startWithWindowsCheckBox = new CheckBox
        {
            Text = "Start with Windows",
            Location = new Point(10, y),
            Size = new Size(400, 20),
            Checked = false
        };
        panel.Controls.Add(startWithWindowsCheckBox);
        y += 40;

        // Font Sizes Section
        var fontSizesLabel = new Label
        {
            Text = "Font Sizes:",
            Location = new Point(10, y),
            Size = new Size(200, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        panel.Controls.Add(fontSizesLabel);
        y += 30;

        // Main Page Font Size
        var mainPageFontSizeLabel = new Label
        {
            Text = "Main Page Font Size:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        mainPageFontSizeNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 6,
            Maximum = 24,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = 10
        };
        panel.Controls.Add(mainPageFontSizeLabel);
        panel.Controls.Add(mainPageFontSizeNumeric);
        y += 30;

        // Popup Font Size
        var popupFontSizeLabel = new Label
        {
            Text = "Popup Font Size:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        popupFontSizeNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 6,
            Maximum = 24,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = 10
        };
        panel.Controls.Add(popupFontSizeLabel);
        panel.Controls.Add(popupFontSizeNumeric);
        y += 30;

        // Grammar Learner Font Size
        var grammarLearnerFontSizeLabel = new Label
        {
            Text = "Grammar Learner Font Size:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        grammarLearnerFontSizeNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 6,
            Maximum = 24,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = 10
        };
        panel.Controls.Add(grammarLearnerFontSizeLabel);
        panel.Controls.Add(grammarLearnerFontSizeNumeric);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateAdvancedTab()
    {
        var tab = new TabPage("Advanced");
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };

        int y = 10;

        // TTS Settings Section
        var ttsSectionLabel = new Label
        {
            Text = "Text-to-Speech Settings:",
            Location = new Point(10, y),
            Size = new Size(300, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        panel.Controls.Add(ttsSectionLabel);
        y += 30;

        // TTS Provider
        var ttsProviderLabel = new Label
        {
            Text = "TTS Provider:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        ttsProviderComboBox = new ComboBox
        {
            Location = new Point(220, y),
            Size = new Size(150, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        ttsProviderComboBox.Items.AddRange(new object[] { "Google", "LocalAI" });
        ttsProviderComboBox.SelectedIndex = 0;
        ttsProviderComboBox.SelectedIndexChanged += OnTtsProviderChanged;
        panel.Controls.Add(ttsProviderLabel);
        panel.Controls.Add(ttsProviderComboBox);
        y += 40;

        // LocalAI Endpoint
        var localAIEndpointLabel = new Label
        {
            Text = "LocalAI Endpoint:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        localAIEndpointTextBox = new TextBox
        {
            Location = new Point(10, y + 20),
            Size = new Size(540, 20),
            Text = "http://127.0.0.1:3002/tts"
        };
        panel.Controls.Add(localAIEndpointLabel);
        panel.Controls.Add(localAIEndpointTextBox);
        y += 50;

        // LocalAI Model
        var localAIModelLabel = new Label
        {
            Text = "LocalAI Model:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        localAIModelTextBox = new TextBox
        {
            Location = new Point(220, y),
            Size = new Size(200, 20),
            Text = "tts-english"
        };
        panel.Controls.Add(localAIModelLabel);
        panel.Controls.Add(localAIModelTextBox);
        y += 40;

        // LocalAI Response Format
        var localAIFormatLabel = new Label
        {
            Text = "LocalAI Response Format:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        localAIFormatComboBox = new ComboBox
        {
            Location = new Point(220, y),
            Size = new Size(150, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        localAIFormatComboBox.Items.AddRange(new object[] { "wav", "mp3", "aac", "flac", "opus" });
        localAIFormatComboBox.SelectedIndex = 0;
        panel.Controls.Add(localAIFormatLabel);
        panel.Controls.Add(localAIFormatComboBox);
        y += 40;

        // Cache Expiration Days
        var cacheExpirationLabel = new Label
        {
            Text = "Cache Expiration (days):",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        cacheExpirationDaysNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 1,
            Maximum = 365,
            Value = 30
        };
        panel.Controls.Add(cacheExpirationLabel);
        panel.Controls.Add(cacheExpirationDaysNumeric);
        y += 40;

        // Clear Cache Button
        clearCacheButton = new Button
        {
            Text = "Clear TTS Cache",
            Location = new Point(10, y),
            Size = new Size(150, 30),
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        clearCacheButton.FlatAppearance.BorderSize = 0;
        clearCacheButton.Click += OnClearCacheButtonClick;
        panel.Controls.Add(clearCacheButton);
        y += 50;

        // Separator
        var separator = new Label
        {
            Text = "",
            Location = new Point(10, y),
            Size = new Size(540, 2),
            BorderStyle = BorderStyle.Fixed3D
        };
        panel.Controls.Add(separator);
        y += 20;

        // API Settings Section
        var apiSectionLabel = new Label
        {
            Text = "API Settings:",
            Location = new Point(10, y),
            Size = new Size(300, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        panel.Controls.Add(apiSectionLabel);
        y += 30;

        // Note: Timeout is now configured per endpoint in API Configuration tab
        y += 10;

        // Retry Count
        var retryLabel = new Label
        {
            Text = "Retry Count:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        retryCountNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 0,
            Maximum = 10,
            Value = 3
        };
        panel.Controls.Add(retryLabel);
        panel.Controls.Add(retryCountNumeric);
        y += 50;

        // Separator
        var separator2 = new Label
        {
            Text = "",
            Location = new Point(10, y),
            Size = new Size(540, 2),
            BorderStyle = BorderStyle.Fixed3D
        };
        panel.Controls.Add(separator2);
        y += 20;

        // Logging Settings Section
        var loggingSectionLabel = new Label
        {
            Text = "Logging Settings:",
            Location = new Point(10, y),
            Size = new Size(300, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        panel.Controls.Add(loggingSectionLabel);
        y += 30;

        // Enable Logging
        enableLoggingCheckBox = new CheckBox
        {
            Text = "Enable logging",
            Location = new Point(10, y),
            Size = new Size(200, 20),
            Checked = true
        };
        panel.Controls.Add(enableLoggingCheckBox);
        y += 30;

        // Log Level
        var logLevelLabel = new Label
        {
            Text = "Log Level:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        logLevelComboBox = new ComboBox
        {
            Location = new Point(220, y),
            Size = new Size(150, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        logLevelComboBox.Items.AddRange(new object[] { "Debug", "Information", "Warning", "Error" });
        logLevelComboBox.SelectedIndex = 1;
        panel.Controls.Add(logLevelLabel);
        panel.Controls.Add(logLevelComboBox);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateShortcutsTab()
    {
        var tab = new TabPage("Shortcuts");
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };

        var config = _configService.Config;
        int y = 10;

        // Translate Persian to English
        translatePersianToEnglishControl = new HotkeyConfigControl(
            "Translate: Persian to English",
            config.Hotkeys.TranslatePersianToEnglish,
            y,
            panel);
        y += 70;

        // Translate English to Persian
        translateEnglishToPersianControl = new HotkeyConfigControl(
            "Translate: English to Persian",
            config.Hotkeys.TranslateEnglishToPersian,
            y,
            panel);
        y += 70;

        // Translate Grammar Fix
        translateGrammarFixControl = new HotkeyConfigControl(
            "Translate: Grammar Fix",
            config.Hotkeys.TranslateGrammarFix,
            y,
            panel);
        y += 70;

        // Read Persian
        readPersianControl = new HotkeyConfigControl(
            "Read Persian Text",
            config.Hotkeys.ReadPersian,
            y,
            panel);
        y += 70;

        // Read English
        readEnglishControl = new HotkeyConfigControl(
            "Read English Text",
            config.Hotkeys.ReadEnglish,
            y,
            panel);
        tab.Controls.Add(panel);
        return tab;
    }

    private void OnTtsProviderChanged(object? sender, EventArgs e)
    {
        var isLocalAI = ttsProviderComboBox.SelectedItem?.ToString() == "LocalAI";
        localAIEndpointTextBox.Enabled = isLocalAI;
        localAIModelTextBox.Enabled = isLocalAI;
        localAIFormatComboBox.Enabled = isLocalAI;
    }

    private void LoadSettings()
    {
        var config = _configService.Config;

        // API Configuration (loaded in CreateApiConfigTab)

        // UI Preferences
        popupTimeoutNumeric.Value = config.Ui.PopupAutoCloseSeconds;
        showNotificationsCheckBox.Checked = config.Ui.ShowNotifications;
        showCharacterCountCheckBox.Checked = config.Ui.ShowCharacterCount;
        rememberWindowPositionCheckBox.Checked = config.Window.RememberPosition;
        startWithWindowsCheckBox.Checked = config.StartWithWindows;
        mainPageFontSizeNumeric.Value = (decimal)config.Ui.MainPageFontSize;
        popupFontSizeNumeric.Value = (decimal)config.Ui.PopupFontSize;
        grammarLearnerFontSizeNumeric.Value = (decimal)config.Ui.GrammarLearnerFontSize;

        // Advanced - TTS Settings
        ttsProviderComboBox.SelectedItem = config.TtsSettings.Provider.ToString();
        localAIEndpointTextBox.Text = config.TtsSettings.LocalAIEndpoint;
        localAIModelTextBox.Text = config.TtsSettings.LocalAIModel;
        localAIFormatComboBox.SelectedItem = config.TtsSettings.LocalAIResponseFormat;
        cacheExpirationDaysNumeric.Value = config.TtsSettings.CacheExpirationDays;
        OnTtsProviderChanged(null, EventArgs.Empty); // Update enabled state

        // Advanced - API Settings
        retryCountNumeric.Value = config.Api.RetryCount;
        
        // Advanced - Logging Settings
        enableLoggingCheckBox.Checked = config.Logging.EnableLogging;
        logLevelComboBox.SelectedItem = config.Logging.LogLevel;

        // Shortcuts (loaded in CreateShortcutsTab)
    }

    private void OnSaveButtonClick(object? sender, EventArgs e)
    {
        var config = _configService.Config;

        // API Configuration
        enToFaConfigControl.SaveToConfig();
        faToEnConfigControl.SaveToConfig();
        grammarFixConfigControl.SaveToConfig();
        grammarLearnerConfigControl.SaveToConfig();

        // UI Preferences
        config.Ui.PopupAutoCloseSeconds = (int)popupTimeoutNumeric.Value;
        config.Ui.ShowNotifications = showNotificationsCheckBox.Checked;
        config.Ui.ShowCharacterCount = showCharacterCountCheckBox.Checked;
        config.Window.RememberPosition = rememberWindowPositionCheckBox.Checked;
        config.StartWithWindows = startWithWindowsCheckBox.Checked;
        config.Ui.MainPageFontSize = (float)mainPageFontSizeNumeric.Value;
        config.Ui.PopupFontSize = (float)popupFontSizeNumeric.Value;
        config.Ui.GrammarLearnerFontSize = (float)grammarLearnerFontSizeNumeric.Value;

        // Advanced - TTS Settings
        var selectedProvider = ttsProviderComboBox.SelectedItem?.ToString() ?? "Google";
        config.TtsSettings.Provider = Enum.Parse<TtsProvider>(selectedProvider);
        config.TtsSettings.LocalAIEndpoint = localAIEndpointTextBox.Text;
        config.TtsSettings.LocalAIModel = localAIModelTextBox.Text;
        config.TtsSettings.LocalAIResponseFormat = localAIFormatComboBox.SelectedItem?.ToString() ?? "wav";
        config.TtsSettings.CacheExpirationDays = (int)cacheExpirationDaysNumeric.Value;

        // Advanced - API Settings
        config.Api.RetryCount = (int)retryCountNumeric.Value;
        
        // Advanced - Logging Settings
        config.Logging.EnableLogging = enableLoggingCheckBox.Checked;
        config.Logging.LogLevel = logLevelComboBox.SelectedItem?.ToString() ?? "Information";

        // Shortcuts
        translatePersianToEnglishControl.SaveToConfig(config.Hotkeys.TranslatePersianToEnglish);
        translateEnglishToPersianControl.SaveToConfig(config.Hotkeys.TranslateEnglishToPersian);
        translateGrammarFixControl.SaveToConfig(config.Hotkeys.TranslateGrammarFix);
        readPersianControl.SaveToConfig(config.Hotkeys.ReadPersian);
        readEnglishControl.SaveToConfig(config.Hotkeys.ReadEnglish);

        try
        {
            _configService.SaveConfiguration();
            
            // Update auto-start registry
            AutoStartManager.SetAutoStart(config.StartWithWindows);
            
            MessageBox.Show("Settings saved successfully. Some changes may require restart.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnClearCacheButtonClick(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to clear all TTS cache? This will delete all cached audio files.",
            "Clear TTS Cache",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            try
            {
                var cacheManager = new TtsCacheManager(_configService, new LoggingService(_configService));
                cacheManager.ClearAllCache();
                MessageBox.Show("TTS cache cleared successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing cache: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

