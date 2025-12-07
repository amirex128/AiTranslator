using AiTranslator.Models;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator.Forms;

public class SettingsForm : Form
{
    private readonly IConfigService _configService;
    private TabControl tabControl = null!;
    
    // API Configuration Tab
    private TextBox enToFaEndpointTextBox = null!;
    private TextBox faToEnEndpointTextBox = null!;
    private TextBox grammarFixEndpointTextBox = null!;
    private TextBox persianTtsEndpointTextBox = null!;
    private TextBox englishTtsEndpointTextBox = null!;
    
    // UI Preferences Tab
    private NumericUpDown popupTimeoutNumeric = null!;
    private CheckBox showNotificationsCheckBox = null!;
    private CheckBox showCharacterCountCheckBox = null!;
    private CheckBox rememberWindowPositionCheckBox = null!;
    private CheckBox startWithWindowsCheckBox = null!;
    private NumericUpDown mainPageFontSizeNumeric = null!;
    private NumericUpDown popupFontSizeNumeric = null!;
    private NumericUpDown selectionFormFontSizeNumeric = null!;
    
    // Advanced Tab
    private NumericUpDown timeoutMinutesNumeric = null!;
    private NumericUpDown retryCountNumeric = null!;
    private ComboBox logLevelComboBox = null!;
    private CheckBox enableLoggingCheckBox = null!;
    
    private Button saveButton = null!;
    private Button cancelButton = null!;

    public SettingsForm(IConfigService configService)
    {
        _configService = configService;
        InitializeComponents();
        LoadSettings();
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

        int y = 10;

        // EN to FA Endpoint
        var enToFaLabel = new Label
        {
            Text = "English to Persian Endpoint:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        enToFaEndpointTextBox = new TextBox
        {
            Location = new Point(10, y + 20),
            Size = new Size(540, 20)
        };
        panel.Controls.Add(enToFaLabel);
        panel.Controls.Add(enToFaEndpointTextBox);
        y += 50;

        // FA to EN Endpoint
        var faToEnLabel = new Label
        {
            Text = "Persian to English Endpoint:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        faToEnEndpointTextBox = new TextBox
        {
            Location = new Point(10, y + 20),
            Size = new Size(540, 20)
        };
        panel.Controls.Add(faToEnLabel);
        panel.Controls.Add(faToEnEndpointTextBox);
        y += 50;

        // Grammar Fix Endpoint
        var grammarFixLabel = new Label
        {
            Text = "Grammar Fix Endpoint:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        grammarFixEndpointTextBox = new TextBox
        {
            Location = new Point(10, y + 20),
            Size = new Size(540, 20)
        };
        panel.Controls.Add(grammarFixLabel);
        panel.Controls.Add(grammarFixEndpointTextBox);
        y += 50;

        // Persian TTS Endpoint
        var persianTtsLabel = new Label
        {
            Text = "Persian TTS Endpoint:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        persianTtsEndpointTextBox = new TextBox
        {
            Location = new Point(10, y + 20),
            Size = new Size(540, 20)
        };
        panel.Controls.Add(persianTtsLabel);
        panel.Controls.Add(persianTtsEndpointTextBox);
        y += 50;

        // English TTS Endpoint
        var englishTtsLabel = new Label
        {
            Text = "English TTS Endpoint:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        englishTtsEndpointTextBox = new TextBox
        {
            Location = new Point(10, y + 20),
            Size = new Size(540, 20)
        };
        panel.Controls.Add(englishTtsLabel);
        panel.Controls.Add(englishTtsEndpointTextBox);

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

        // Selection Form Font Size
        var selectionFormFontSizeLabel = new Label
        {
            Text = "Selection Form Font Size:",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        selectionFormFontSizeNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 6,
            Maximum = 24,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = 9
        };
        panel.Controls.Add(selectionFormFontSizeLabel);
        panel.Controls.Add(selectionFormFontSizeNumeric);

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateAdvancedTab()
    {
        var tab = new TabPage("Advanced");
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        int y = 10;

        // API Timeout
        var timeoutLabel = new Label
        {
            Text = "API Timeout (minutes):",
            Location = new Point(10, y),
            Size = new Size(200, 20)
        };
        timeoutMinutesNumeric = new NumericUpDown
        {
            Location = new Point(220, y),
            Size = new Size(100, 20),
            Minimum = 1,
            Maximum = 30,
            Value = 5
        };
        panel.Controls.Add(timeoutLabel);
        panel.Controls.Add(timeoutMinutesNumeric);
        y += 40;

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
        y += 40;

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

    private void LoadSettings()
    {
        var config = _configService.Config;

        // API Configuration
        enToFaEndpointTextBox.Text = config.ApiEndpoints.EnglishToPersian;
        faToEnEndpointTextBox.Text = config.ApiEndpoints.PersianToEnglish;
        grammarFixEndpointTextBox.Text = config.ApiEndpoints.GrammarFix;
        persianTtsEndpointTextBox.Text = config.TtsEndpoints.Persian;
        englishTtsEndpointTextBox.Text = config.TtsEndpoints.English;

        // UI Preferences
        popupTimeoutNumeric.Value = config.Ui.PopupAutoCloseSeconds;
        showNotificationsCheckBox.Checked = config.Ui.ShowNotifications;
        showCharacterCountCheckBox.Checked = config.Ui.ShowCharacterCount;
        rememberWindowPositionCheckBox.Checked = config.Window.RememberPosition;
        startWithWindowsCheckBox.Checked = config.StartWithWindows;
        mainPageFontSizeNumeric.Value = (decimal)config.Ui.MainPageFontSize;
        popupFontSizeNumeric.Value = (decimal)config.Ui.PopupFontSize;
        selectionFormFontSizeNumeric.Value = (decimal)config.Ui.SelectionFormFontSize;

        // Advanced
        timeoutMinutesNumeric.Value = config.Api.TimeoutMinutes;
        retryCountNumeric.Value = config.Api.RetryCount;
        enableLoggingCheckBox.Checked = config.Logging.EnableLogging;
        logLevelComboBox.SelectedItem = config.Logging.LogLevel;
    }

    private void OnSaveButtonClick(object? sender, EventArgs e)
    {
        var config = _configService.Config;

        // API Configuration
        config.ApiEndpoints.EnglishToPersian = enToFaEndpointTextBox.Text;
        config.ApiEndpoints.PersianToEnglish = faToEnEndpointTextBox.Text;
        config.ApiEndpoints.GrammarFix = grammarFixEndpointTextBox.Text;
        config.TtsEndpoints.Persian = persianTtsEndpointTextBox.Text;
        config.TtsEndpoints.English = englishTtsEndpointTextBox.Text;

        // UI Preferences
        config.Ui.PopupAutoCloseSeconds = (int)popupTimeoutNumeric.Value;
        config.Ui.ShowNotifications = showNotificationsCheckBox.Checked;
        config.Ui.ShowCharacterCount = showCharacterCountCheckBox.Checked;
        config.Window.RememberPosition = rememberWindowPositionCheckBox.Checked;
        config.StartWithWindows = startWithWindowsCheckBox.Checked;
        config.Ui.MainPageFontSize = (float)mainPageFontSizeNumeric.Value;
        config.Ui.PopupFontSize = (float)popupFontSizeNumeric.Value;
        config.Ui.SelectionFormFontSize = (float)selectionFormFontSizeNumeric.Value;

        // Advanced
        config.Api.TimeoutMinutes = (int)timeoutMinutesNumeric.Value;
        config.Api.RetryCount = (int)retryCountNumeric.Value;
        config.Logging.EnableLogging = enableLoggingCheckBox.Checked;
        config.Logging.LogLevel = logLevelComboBox.SelectedItem?.ToString() ?? "Information";

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
}

