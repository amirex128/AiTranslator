using AiTranslator.Models;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator.Forms;

public partial class MainForm : Form
{
    private readonly IConfigService _configService;
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;
    private readonly ILanguageDetector _languageDetector;
    private readonly ILoggingService _loggingService;
    private readonly HotkeyManager _hotkeyManager;
    private readonly ClipboardManager _clipboardManager;
    
    private CancellationTokenSource? _currentOperationCancellation;
    private TextBox? _activeTextBox;
    private int _resultTabCounter = 0;
    private bool _isClosing = false;
    private bool _isSelectingBoxToRead = false;

    public MainForm(
        IConfigService configService,
        ITranslationService translationService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        ILoggingService loggingService,
        HotkeyManager hotkeyManager,
        ClipboardManager clipboardManager)
    {
        _configService = configService;
        _translationService = translationService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _loggingService = loggingService;
        _hotkeyManager = hotkeyManager;
        _clipboardManager = clipboardManager;

        InitializeComponent();
        InitializeEventHandlers();
        InitializeWindowSettings();
        LoadShortcutsList();
    }

    private void InitializeEventHandlers()
    {
        // Text changed events for smart collapse behavior
        persianToEnglishTextBox.TextChanged += OnTextBoxTextChanged;
        persianToEnglishTextBox.Enter += OnTextBoxEnter;
        englishToPersianTextBox.TextChanged += OnTextBoxTextChanged;
        englishToPersianTextBox.Enter += OnTextBoxEnter;
        grammarFixTextBox.TextChanged += OnTextBoxTextChanged;
        grammarFixTextBox.Enter += OnTextBoxEnter;

        // Button events
        translateButton.Click += OnTranslateButtonClick;
        readButton.Click += OnReadButtonClick;
        cancelButton.Click += OnCancelButtonClick;
        copyResultButton.Click += OnCopyResultButtonClick;
        toggleShortcutsButton.Click += OnToggleShortcutsButtonClick;

        // Menu events
        exitToolStripMenuItem.Click += OnExitMenuItemClick;
        settingsToolStripMenuItem.Click += OnSettingsMenuItemClick;
        copyResultToolStripMenuItem.Click += OnCopyResultButtonClick;
        alwaysOnTopToolStripMenuItem.Click += OnAlwaysOnTopMenuItemClick;
        aboutToolStripMenuItem.Click += OnAboutMenuItemClick;
        shortcutsToolStripMenuItem.Click += OnShortcutsMenuItemClick;

        // Form events
        this.FormClosing += OnFormClosing;
        this.Resize += OnFormResize;
        this.LocationChanged += OnFormLocationChanged;
    }

    private void InitializeWindowSettings()
    {
        var windowSettings = _configService.Config.Window;
        
        if (windowSettings.RememberPosition && 
            windowSettings.LastWidth.HasValue && 
            windowSettings.LastHeight.HasValue)
        {
            this.Width = windowSettings.LastWidth.Value;
            this.Height = windowSettings.LastHeight.Value;

            if (windowSettings.LastX.HasValue && windowSettings.LastY.HasValue)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(windowSettings.LastX.Value, windowSettings.LastY.Value);
            }
        }
        else
        {
            this.Width = windowSettings.DefaultWidth;
            this.Height = windowSettings.DefaultHeight;
        }

        this.MinimumSize = new Size(windowSettings.MinWidth, windowSettings.MinHeight);
        
        // Apply modern styling
        ApplyModernStyling();
        
        // Apply font sizes
        ApplyFontSizes();
    }

    private void ApplyFontSizes()
    {
        var fontSize = _configService.Config.Ui.MainPageFontSize;
        var font = new Font("Segoe UI", fontSize);

        // Apply to input text boxes
        persianToEnglishTextBox.Font = font;
        englishToPersianTextBox.Font = font;
        grammarFixTextBox.Font = font;

        // Apply to result tabs (will be applied when tabs are created)
    }

    private void ApplyModernStyling()
    {
        // Set modern colors
        this.BackColor = UIConstants.Colors.Background;
        mainPanel.BackColor = UIConstants.Colors.Background;
        
        // Style buttons
        translateButton.BackColor = UIConstants.Colors.Primary;
        translateButton.ForeColor = Color.White;
        translateButton.FlatStyle = FlatStyle.Flat;
        translateButton.FlatAppearance.BorderSize = 0;
        
        readButton.BackColor = UIConstants.Colors.Secondary;
        readButton.ForeColor = Color.White;
        readButton.FlatStyle = FlatStyle.Flat;
        readButton.FlatAppearance.BorderSize = 0;
        
        cancelButton.BackColor = UIConstants.Colors.Error;
        cancelButton.ForeColor = Color.White;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.FlatAppearance.BorderSize = 0;
        
        copyResultButton.BackColor = UIConstants.Colors.Primary;
        copyResultButton.ForeColor = Color.White;
        copyResultButton.FlatStyle = FlatStyle.Flat;
        copyResultButton.FlatAppearance.BorderSize = 0;
        
        // Style input panels
        persianToEnglishPanel.BackColor = UIConstants.Colors.Surface;
        englishToPersianPanel.BackColor = UIConstants.Colors.Surface;
        grammarFixPanel.BackColor = UIConstants.Colors.Surface;
        
        // Style result panel
        resultPanel.BackColor = UIConstants.Colors.Surface;
        resultTabControl.BackColor = UIConstants.Colors.Background;
        
        // Style shortcuts panel
        shortcutsPanel.BackColor = UIConstants.Colors.Surface;
    }

    private void LoadShortcutsList()
    {
        shortcutsListView.Items.Clear();
        var hotkeys = _configService.Config.Hotkeys;

        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.PopupTranslatePersianToEnglish.ToString(), 
            "Quick Translate: Persian to English (Popup)" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.PopupTranslateEnglishToPersian.ToString(), 
            "Quick Translate: English to Persian (Popup)" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.PopupTranslateGrammarFix.ToString(), 
            "Quick Translate: Grammar Fix (Popup)" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.ClipboardReplacePersianToEnglish.ToString(), 
            "Clipboard Replace: Persian to English" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.ClipboardReplaceEnglishToPersian.ToString(), 
            "Clipboard Replace: English to Persian" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.ClipboardReplaceGrammarFix.ToString(), 
            "Clipboard Replace: Grammar Fix" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.ReadPersian.ToString(), 
            "Read Persian Text" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.ReadEnglish.ToString(), 
            "Read English Text" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.AutoDetectTranslate.ToString(), 
            "Auto-detect and Translate (Popup)" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.AutoDetectRead.ToString(), 
            "Auto-detect and Read" 
        }));
        shortcutsListView.Items.Add(new ListViewItem(new[] { 
            hotkeys.UndoClipboard.ToString(), 
            "Undo Clipboard Replace" 
        }));
    }

    private void OnTextBoxTextChanged(object? sender, EventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox == null) return;

        // Update character count
        UpdateCharacterCount(textBox);

        // Handle RTL for Persian text
        UpdateTextDirection(textBox);

        // Smart collapse behavior
        if (!string.IsNullOrEmpty(textBox.Text) && textBox != _activeTextBox)
        {
            _activeTextBox = textBox;
            CollapseOtherInputs(textBox);
        }
    }

    private void OnTextBoxEnter(object? sender, EventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox == null) return;

        _activeTextBox = textBox;
        ExpandInput(textBox);
    }

    private void UpdateCharacterCount(TextBox textBox)
    {
        if (!_configService.Config.Ui.ShowCharacterCount) return;

        var text = textBox.Text;
        var charCount = text.Length;
        var wordCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

        Label? countLabel = null;
        if (textBox == persianToEnglishTextBox) countLabel = persianToEnglishCountLabel;
        else if (textBox == englishToPersianTextBox) countLabel = englishToPersianCountLabel;
        else if (textBox == grammarFixTextBox) countLabel = grammarFixCountLabel;

        if (countLabel != null)
        {
            countLabel.Text = $"Characters: {charCount} | Words: {wordCount}";
        }
    }

    private void UpdateTextDirection(TextBox textBox)
    {
        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            textBox.RightToLeft = RightToLeft.No;
            return;
        }

        var language = _languageDetector.DetectLanguage(textBox.Text);
        textBox.RightToLeft = language == Language.Persian ? RightToLeft.Yes : RightToLeft.No;
    }

    private void CollapseOtherInputs(TextBox activeTextBox)
    {
        if (activeTextBox != persianToEnglishTextBox)
        {
            persianToEnglishTextBox.Text = "";
            persianToEnglishPanel.Height = 35;
        }
        if (activeTextBox != englishToPersianTextBox)
        {
            englishToPersianTextBox.Text = "";
            englishToPersianPanel.Height = 35;
        }
        if (activeTextBox != grammarFixTextBox)
        {
            grammarFixTextBox.Text = "";
            grammarFixPanel.Height = 35;
        }

        ExpandInput(activeTextBox);
    }

    private void ExpandInput(TextBox textBox)
    {
        Panel? panel = null;
        if (textBox == persianToEnglishTextBox) panel = persianToEnglishPanel;
        else if (textBox == englishToPersianTextBox) panel = englishToPersianPanel;
        else if (textBox == grammarFixTextBox) panel = grammarFixPanel;

        if (panel != null)
        {
            panel.Height = 100;
        }
    }

    private async void OnTranslateButtonClick(object? sender, EventArgs e)
    {
        var activeInput = GetActiveInput();
        if (activeInput.textBox == null || string.IsNullOrWhiteSpace(activeInput.textBox.Text))
        {
            UpdateStatus("Please enter text to translate");
            return;
        }

        await TranslateTextAsync(activeInput.textBox.Text, activeInput.type);
    }

    private async void OnReadButtonClick(object? sender, EventArgs e)
    {
        // Check if we have result tabs with multiple boxes
        if (resultTabControl.TabCount > 0 && resultTabControl.SelectedTab != null)
        {
            var selectedTab = resultTabControl.SelectedTab;
            var resultsPanel = FindResultsPanel(selectedTab);
            
            if (resultsPanel != null && resultsPanel.Controls.Count > 1)
            {
                // Multiple boxes - enter selection mode
                if (!_isSelectingBoxToRead)
                {
                    EnterBoxSelectionMode(resultsPanel);
                }
                else
                {
                    ExitBoxSelectionMode(resultsPanel);
                }
                return;
            }
        }

        // Fallback: read from active input
        var activeInput = GetActiveInput();
        if (activeInput.textBox == null || string.IsNullOrWhiteSpace(activeInput.textBox.Text))
        {
            UpdateStatus("Please enter text to read");
            return;
        }

        await ReadTextAsync(activeInput.textBox.Text);
    }

    private Panel? FindResultsPanel(TabPage tabPage)
    {
        foreach (Control control in tabPage.Controls)
        {
            if (control is Panel panel)
            {
                foreach (Control innerControl in panel.Controls)
                {
                    if (innerControl is Panel innerPanel && innerPanel.AutoScroll)
                    {
                        return innerPanel;
                    }
                }
            }
        }
        return null;
    }

    private void EnterBoxSelectionMode(Panel resultsPanel)
    {
        _isSelectingBoxToRead = true;
        readButton.Text = "Cancel Selection";
        readButton.BackColor = Color.Orange;
        UpdateStatus("Click on a box to read it");

        // Highlight all result boxes
        foreach (Control control in resultsPanel.Controls)
        {
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.Fixed3D;
            }
        }
    }

    private void ExitBoxSelectionMode(Panel resultsPanel)
    {
        _isSelectingBoxToRead = false;
        readButton.Text = "Read";
        readButton.BackColor = UIConstants.Colors.Secondary;
        UpdateStatus("Selection cancelled");

        // Remove highlight from all result boxes
        foreach (Control control in resultsPanel.Controls)
        {
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }
    }

    private void OnCancelButtonClick(object? sender, EventArgs e)
    {
        _currentOperationCancellation?.Cancel();
        _ttsService.StopReading();
        SetLoadingState(false);
        UpdateStatus("Operation cancelled");
    }

    private void OnCopyResultButtonClick(object? sender, EventArgs e)
    {
        if (resultTabControl.TabCount == 0 || resultTabControl.SelectedTab == null)
        {
            UpdateStatus("No result to copy");
            return;
        }

        var textBox = resultTabControl.SelectedTab.Controls.OfType<TextBox>().FirstOrDefault();
        if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
        {
            _clipboardManager.SetClipboardText(textBox.Text, false);
            UpdateStatus("Result copied to clipboard");
        }
    }

    private void OnToggleShortcutsButtonClick(object? sender, EventArgs e)
    {
        if (shortcutsListView.Visible)
        {
            shortcutsListView.Visible = false;
            shortcutsLabel.Visible = false;
            shortcutsPanel.Height = 25;
            toggleShortcutsButton.Text = "Show Shortcuts";
        }
        else
        {
            shortcutsListView.Visible = true;
            shortcutsLabel.Visible = true;
            shortcutsPanel.Height = 182;
            toggleShortcutsButton.Text = "Hide Shortcuts";
        }
    }

    private void OnExitMenuItemClick(object? sender, EventArgs e)
    {
        _isClosing = true;
        Application.Exit();
    }

    private void OnSettingsMenuItemClick(object? sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(_configService);
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            // Settings saved, reload shortcuts list
            LoadShortcutsList();
        }
    }

    private void OnAlwaysOnTopMenuItemClick(object? sender, EventArgs e)
    {
        this.TopMost = !this.TopMost;
        alwaysOnTopToolStripMenuItem.Checked = this.TopMost;
    }

    private void OnAboutMenuItemClick(object? sender, EventArgs e)
    {
        MessageBox.Show("AI Translator\nVersion 1.0\n\nPowered by AI APIs", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnShortcutsMenuItemClick(object? sender, EventArgs e)
    {
        if (!shortcutsListView.Visible)
        {
            OnToggleShortcutsButtonClick(sender, e);
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isClosing)
        {
            e.Cancel = true;
            this.Hide();
            return;
        }

        // Save window state
        if (this.WindowState == FormWindowState.Normal)
        {
            _configService.SaveWindowState(this.Location.X, this.Location.Y, this.Width, this.Height);
        }

        _hotkeyManager.Dispose();
        _currentOperationCancellation?.Cancel();
    }

    private void OnFormResize(object? sender, EventArgs e)
    {
        // Will be handled by system tray logic
    }

    private void OnFormLocationChanged(object? sender, EventArgs e)
    {
        // Window state will be saved on closing
    }

    private (TextBox? textBox, TranslationType type) GetActiveInput()
    {
        if (_activeTextBox == persianToEnglishTextBox && !string.IsNullOrWhiteSpace(persianToEnglishTextBox.Text))
            return (persianToEnglishTextBox, TranslationType.PersianToEnglish);
        if (_activeTextBox == englishToPersianTextBox && !string.IsNullOrWhiteSpace(englishToPersianTextBox.Text))
            return (englishToPersianTextBox, TranslationType.EnglishToPersian);
        if (_activeTextBox == grammarFixTextBox && !string.IsNullOrWhiteSpace(grammarFixTextBox.Text))
            return (grammarFixTextBox, TranslationType.GrammarFix);

        // Fallback: check all inputs
        if (!string.IsNullOrWhiteSpace(persianToEnglishTextBox.Text))
            return (persianToEnglishTextBox, TranslationType.PersianToEnglish);
        if (!string.IsNullOrWhiteSpace(englishToPersianTextBox.Text))
            return (englishToPersianTextBox, TranslationType.EnglishToPersian);
        if (!string.IsNullOrWhiteSpace(grammarFixTextBox.Text))
            return (grammarFixTextBox, TranslationType.GrammarFix);

        return (null, TranslationType.PersianToEnglish);
    }

    public async Task TranslateTextAsync(string text, TranslationType type)
    {
        SetLoadingState(true);
        UpdateStatus($"Translating...");

        _currentOperationCancellation = new CancellationTokenSource();

        try
        {
            TranslationResponse response;

            switch (type)
            {
                case TranslationType.PersianToEnglish:
                    response = await _translationService.TranslatePersianToEnglishAsync(text, _currentOperationCancellation.Token);
                    break;
                case TranslationType.EnglishToPersian:
                    response = await _translationService.TranslateEnglishToPersianAsync(text, _currentOperationCancellation.Token);
                    break;
                case TranslationType.GrammarFix:
                    response = await _translationService.FixGrammarAsync(text, _currentOperationCancellation.Token);
                    break;
                default:
                    throw new ArgumentException("Invalid translation type");
            }

            if (response.Success)
            {
                AddResultTab(text, response.Text, type);
                UpdateStatus("Translation completed successfully");
            }
            else
            {
                AddErrorTab(text, response.Error, type);
                UpdateStatus($"Translation failed: {response.Error}");
                
                if (_configService.Config.Ui.ShowNotifications)
                {
                    MessageBox.Show(response.Error, "Translation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Translation cancelled");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Translation error in main window", ex);
            AddErrorTab(text, ex.Message, type);
            UpdateStatus($"Translation error: {ex.Message}");
            
            if (_configService.Config.Ui.ShowNotifications)
            {
                MessageBox.Show(ex.Message, "Translation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            SetLoadingState(false);
            _currentOperationCancellation?.Dispose();
            _currentOperationCancellation = null;
        }
    }

    private async Task ReadTextAsync(string text)
    {
        var language = _languageDetector.DetectLanguage(text);
        
        if (language == Language.Unknown)
        {
            UpdateStatus("Cannot detect language for reading");
            return;
        }

        SetLoadingState(true);
        UpdateStatus($"Reading {language} text...");

        _currentOperationCancellation = new CancellationTokenSource();

        try
        {
            if (language == Language.Persian)
                await _ttsService.ReadPersianAsync(text, _currentOperationCancellation.Token);
            else
                await _ttsService.ReadEnglishAsync(text, _currentOperationCancellation.Token);

            UpdateStatus("Reading completed");
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Reading cancelled");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("TTS error in main window", ex);
            UpdateStatus($"Reading error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
            _currentOperationCancellation?.Dispose();
            _currentOperationCancellation = null;
        }
    }

    private void AddResultTab(string sourceText, string resultText, TranslationType type)
    {
        var fontSize = _configService.Config.Ui.MainPageFontSize;
        var font = new Font("Segoe UI", fontSize);
        var labelFont = new Font("Segoe UI", fontSize - 1, FontStyle.Bold);

        var tabPage = new TabPage($"Result {++_resultTabCounter}");
        
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        
        var sourceLabel = new Label 
        { 
            Dock = DockStyle.Top, 
            Text = $"Source ({type}):", 
            Font = labelFont,
            Height = 20
        };
        
        var translationLabel = new Label 
        { 
            Dock = DockStyle.Top, 
            Text = "Translation:", 
            Font = labelFont,
            Height = 20
        };

        // Parse result text by %%%%% separator
        var results = ParseTranslationOptions(resultText);

        // Create results panel with auto-scroll
        var resultsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White,
            Padding = new Padding(5)
        };

        if (results.Count > 1)
        {
            // Multiple results - create boxes like in TranslationPopupForm
            DisplayResultsInPanel(resultsPanel, results, font);
        }
        else
        {
            // Single result - create a single TextBox (backward compatible)
            var resultTextBox = new TextBox 
            { 
                Dock = DockStyle.Fill, 
                Text = resultText, 
                Multiline = true, 
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = font
            };

            // Set RTL if needed
            var language = _languageDetector.DetectLanguage(resultText);
            if (language == Language.Persian)
            {
                resultTextBox.RightToLeft = RightToLeft.Yes;
            }

            resultsPanel.Controls.Add(resultTextBox);
        }

        panel.Controls.Add(resultsPanel);
        panel.Controls.Add(translationLabel);
        panel.Controls.Add(sourceLabel);
        
        tabPage.Controls.Add(panel);
        resultTabControl.TabPages.Add(tabPage);
        resultTabControl.SelectedTab = tabPage;
    }

    private List<string> ParseTranslationOptions(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string> { text };

        // Split by %%%%% separator
        var options = text.Split(new[] { "%%%%%" }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(o => o.Trim())
                          .Where(o => !string.IsNullOrWhiteSpace(o))
                          .ToList();

        return options.Count > 0 ? options : new List<string> { text };
    }

    private void DisplayResultsInPanel(Panel resultsPanel, List<string> results, Font font)
    {
        resultsPanel.Controls.Clear();

        if (results.Count == 0)
            return;

        var spacing = 10;
        var yPosition = spacing;

        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var language = _languageDetector.DetectLanguage(result);
            var originalColor = i % 2 == 0 ? Color.LightYellow : Color.LightBlue;

            // Create a container panel for each result
            var resultBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Text = result,
                Font = font,
                BackColor = originalColor,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                RightToLeft = language == Language.Persian ? RightToLeft.Yes : RightToLeft.No,
                Cursor = Cursors.Hand,
                Location = new Point(spacing, yPosition),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Width = resultsPanel.ClientSize.Width - (spacing * 2) - SystemInformation.VerticalScrollBarWidth,
                Height = Math.Min(150, GetTextHeight(result, font, resultsPanel.ClientSize.Width - (spacing * 2) - SystemInformation.VerticalScrollBarWidth))
            };

            var originalColorForClosure = originalColor;

            // Add click handler to copy to clipboard or read
            resultBox.Click += async (s, e) =>
            {
                if (_isSelectingBoxToRead)
                {
                    var panel = FindResultsPanel(resultTabControl.SelectedTab!);
                    if (panel != null)
                    {
                        ExitBoxSelectionMode(panel);
                    }
                    await ReadTextAsync(result);
                }
                else
                {
                    CopyResultToClipboard(result, resultBox);
                }
            };
            
            resultBox.MouseEnter += (s, e) => 
            {
                resultBox.BackColor = Color.FromArgb(255, 255, 200); // Highlight on hover
            };
            resultBox.MouseLeave += (s, e) => 
            {
                resultBox.BackColor = originalColorForClosure; // Restore original color
            };

            resultsPanel.Controls.Add(resultBox);
            yPosition += resultBox.Height + spacing;
        }

        // Handle panel resize to adjust result box widths
        resultsPanel.Resize += (s, e) =>
        {
            foreach (Control control in resultsPanel.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Width = resultsPanel.ClientSize.Width - (spacing * 2) - SystemInformation.VerticalScrollBarWidth;
                }
            }
        };
    }

    private int GetTextHeight(string text, Font font, int width)
    {
        using (var g = this.CreateGraphics())
        {
            var size = g.MeasureString(text, font, width);
            return Math.Max(50, (int)size.Height + 20); // Minimum 50px, add padding
        }
    }

    private void CopyResultToClipboard(string text, TextBox resultBox)
    {
        try
        {
            _clipboardManager.SetClipboardText(text, false);
            
            // Visual feedback
            var originalColor = resultBox.BackColor;
            resultBox.BackColor = Color.FromArgb(200, 255, 200); // Green flash
            
            var timer = new System.Windows.Forms.Timer { Interval = 300 };
            timer.Tick += (s, e) =>
            {
                resultBox.BackColor = originalColor;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();

            _loggingService.LogInformation("Result copied to clipboard from main form");
            UpdateStatus("Result copied to clipboard");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error copying to clipboard", ex);
            MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddErrorTab(string sourceText, string errorMessage, TranslationType type)
    {
        var tabPage = new TabPage($"Error {++_resultTabCounter}");
        tabPage.BackColor = Color.LightPink;
        
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        
        var errorLabel = new Label 
        { 
            Dock = DockStyle.Top, 
            Text = $"Error in {type}:", 
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.DarkRed,
            Height = 20
        };
        
        var errorTextBox = new TextBox 
        { 
            Dock = DockStyle.Fill, 
            Text = errorMessage, 
            Multiline = true, 
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.LightPink
        };

        panel.Controls.Add(errorTextBox);
        panel.Controls.Add(errorLabel);
        
        tabPage.Controls.Add(panel);
        resultTabControl.TabPages.Add(tabPage);
        resultTabControl.SelectedTab = tabPage;
    }

    private void SetLoadingState(bool isLoading)
    {
        translateButton.Enabled = !isLoading;
        readButton.Enabled = !isLoading;
        cancelButton.Visible = isLoading;
        loadingLabel.Visible = isLoading;
        
        persianToEnglishTextBox.Enabled = !isLoading;
        englishToPersianTextBox.Enabled = !isLoading;
        grammarFixTextBox.Enabled = !isLoading;
    }

    private void UpdateStatus(string message)
    {
        statusLabel.Text = message;
        _loggingService.LogInformation($"Status: {message}");
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == HotkeyManager.WM_HOTKEY)
        {
            _hotkeyManager.ProcessHotkey((int)m.WParam);
        }
        base.WndProc(ref m);
    }

    public void ShowMainWindow()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }
}

