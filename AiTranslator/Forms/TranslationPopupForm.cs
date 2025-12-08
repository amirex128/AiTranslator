using AiTranslator.Models;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator.Forms;

public class TranslationPopupForm : Form
{
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;
    private readonly ILanguageDetector _languageDetector;
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private readonly ClipboardManager _clipboardManager;
    private readonly SelectionManager? _selectionManager;
    private readonly IGrammarLearnerService? _grammarLearnerService;

    private TextBox sourceTextBox = null!;
    private Panel resultsPanel = null!;
    private Button translateButton = null!;
    private Button readButton = null!;
    private Button insertButton = null!;
    private Button learnButton = null!;
    private Button closeButton = null!;
    private ProgressBar autoCloseProgressBar = null!;
    private Label titleLabel = null!;

    private System.Windows.Forms.Timer autoCloseTimer = null!;
    private System.Windows.Forms.Timer progressTimer = null!;
    private int remainingSeconds;
    private bool isMouseOver = false;
    private TranslationType _translationType;
    private CancellationTokenSource? _cancellationTokenSource;
    private List<string> _currentResults = new();
    private bool _isSelectingBoxToRead = false;
    private bool _isSelectingBoxToInsert = false;
    private bool _isSelectingBoxToLearn = false;
    private bool _isSelectionMode = false;

    public TranslationPopupForm(
        ITranslationService translationService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        IConfigService configService,
        ILoggingService loggingService,
        ClipboardManager clipboardManager,
        SelectionManager? selectionManager = null,
        IGrammarLearnerService? grammarLearnerService = null)
    {
        _translationService = translationService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _configService = configService;
        _loggingService = loggingService;
        _clipboardManager = clipboardManager;
        _selectionManager = selectionManager;
        _grammarLearnerService = grammarLearnerService;

        InitializeComponents();
        InitializeTimers();
    }

    private void InitializeComponents()
    {
        var fontSize = _configService.Config.Ui.PopupFontSize;
        var font = new Font("Segoe UI", fontSize);
        var labelFont = new Font("Segoe UI", fontSize, FontStyle.Bold);
        var titleFont = new Font("Segoe UI", fontSize + 1, FontStyle.Bold);

        this.Text = "Quick Translation";
        this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        this.StartPosition = FormStartPosition.Manual;
        this.TopMost = true;
        this.Size = new Size(500, 400);
        this.MinimumSize = new Size(400, 300);
        this.BackColor = Color.White;

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Font = titleFont,
            Height = 25,
            Text = "Translation"
        };

        sourceTextBox = new TextBox
        {
            Dock = DockStyle.Top,
            Multiline = true,
            Height = 100,
            ScrollBars = ScrollBars.Vertical,
            Font = font,
            ReadOnly = false,
            BackColor = Color.White
        };

        var resultLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = "Result:",
            Font = labelFont,
            Height = 20
        };

        resultsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White,
            Padding = new Padding(5)
        };

        var buttonsPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50
        };

        autoCloseProgressBar = new ProgressBar
        {
            Location = new Point(10, 5),
            Size = new Size(480, 5),
            Style = ProgressBarStyle.Continuous,
            ForeColor = Color.FromArgb(0, 120, 215)
        };

        translateButton = CreateButton("Translate", new Point(10, 15), Color.FromArgb(0, 120, 215),
            OnTranslateButtonClick);
        readButton = CreateButton("Read", new Point(110, 15), Color.FromArgb(0, 120, 215), OnReadButtonClick);
        insertButton = CreateButton("Insert", new Point(210, 15), Color.FromArgb(40, 167, 69), OnInsertButtonClick);
        insertButton.Visible = _selectionManager != null;
        learnButton = CreateButton("Learn", new Point(310, 15), Color.FromArgb(138, 43, 226), OnLearnButtonClick);
        learnButton.Visible = _grammarLearnerService != null;
        closeButton = CreateButton("Close", new Point(410, 15), Color.FromArgb(200, 200, 200), (s, e) => this.Close());
        closeButton.ForeColor = Color.Black;

        buttonsPanel.Controls.Add(autoCloseProgressBar);
        buttonsPanel.Controls.Add(translateButton);
        buttonsPanel.Controls.Add(readButton);
        buttonsPanel.Controls.Add(insertButton);
        buttonsPanel.Controls.Add(learnButton);
        buttonsPanel.Controls.Add(closeButton);

        mainPanel.Controls.Add(resultsPanel);
        mainPanel.Controls.Add(resultLabel);
        mainPanel.Controls.Add(sourceTextBox);
        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(buttonsPanel);

        this.Controls.Add(mainPanel);

        // Handle mouse enter/leave for all controls to properly track mouse position
        AttachMouseEvents(this);
        AttachMouseEvents(sourceTextBox);
        AttachMouseEvents(resultsPanel);
        AttachMouseEvents(translateButton);
        AttachMouseEvents(readButton);
        AttachMouseEvents(insertButton);
        AttachMouseEvents(learnButton);
        AttachMouseEvents(closeButton);
        AttachMouseEvents(titleLabel);
    }

    private void InitializeTimers()
    {
        autoCloseTimer = new System.Windows.Forms.Timer();
        autoCloseTimer.Interval = 1000; // 1 second
        autoCloseTimer.Tick += OnAutoCloseTimerTick;

        progressTimer = new System.Windows.Forms.Timer();
        progressTimer.Interval = 100;
        progressTimer.Tick += OnProgressTimerTick;
    }

    private void AttachMouseEvents(Control control)
    {
        control.MouseEnter += OnMouseEnterForm;
        control.MouseLeave += OnMouseLeaveForm;
    }

    private Button CreateButton(string text, Point location, Color backColor, EventHandler? clickHandler = null)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(90, 30),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;

        if (clickHandler != null)
            button.Click += clickHandler;

        return button;
    }

    private void SetButtonsEnabled(bool enabled)
    {
        if (this.IsDisposed || this.Disposing)
            return;

        try
        {
            if (!translateButton.IsDisposed) translateButton.Enabled = enabled;
            if (!readButton.IsDisposed) readButton.Enabled = enabled;
            if (!insertButton.IsDisposed) insertButton.Enabled = enabled;
            if (!learnButton.IsDisposed) learnButton.Enabled = enabled;
            if (!closeButton.IsDisposed) closeButton.Enabled = enabled;
        }
        catch (ObjectDisposedException)
        {
            // Controls were disposed, ignore
        }
    }

    private void ResetTitleLabel()
    {
        if (titleLabel.IsDisposed)
            return;

        if (_isSelectionMode)
        {
            titleLabel.Text = "Multiple translation options found. Click on any option to copy to clipboard:";
            titleLabel.ForeColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            titleLabel.Text = _translationType.ToString().Replace("To", " → ");
            titleLabel.ForeColor = SystemColors.ControlText;
        }
    }

    private void ResetTitleAfterDelay()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);

            if (this.IsDisposed || this.Disposing || titleLabel.IsDisposed)
                return;

            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
                            ResetTitleLabel();
                    });
                }
                else
                {
                    if (!titleLabel.IsDisposed)
                        ResetTitleLabel();
                }
            }
            catch (ObjectDisposedException)
            {
                // Form was disposed, ignore
            }
            catch (InvalidOperationException)
            {
                // Form handle was destroyed, ignore
            }
        });
    }

    /// <summary>
    /// Shows popup with unified design for both edit and selection modes
    /// </summary>
    public void ShowPopup(string text, TranslationType type, Point? location = null,
        List<string>? preTranslatedOptions = null, bool hasSelectedText = false)
    {
        _translationType = type;

        // Determine if we're in selection mode (pre-translated options provided)
        _isSelectionMode = preTranslatedOptions != null && preTranslatedOptions.Count > 0;

        if (_isSelectionMode)
        {
            // Selection mode: show source text as read-only, display pre-translated options
            sourceTextBox.Text = text;
            sourceTextBox.ReadOnly = true;
            sourceTextBox.BackColor = Color.FromArgb(245, 245, 245);
            titleLabel.Text = "Multiple translation options found. Click on any option to copy to clipboard:";
            titleLabel.ForeColor = Color.FromArgb(0, 120, 215);

            // Display pre-translated results
            DisplayResults(preTranslatedOptions!);

            // Button visibility
            translateButton.Visible = true;
            translateButton.Enabled = false; // Disabled in selection mode
            insertButton.Visible = _selectionManager != null && hasSelectedText;
            learnButton.Visible = _grammarLearnerService != null;

            // Form settings for selection mode
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(700, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }
        else
        {
            // Edit mode: show source text as editable, translate automatically
            sourceTextBox.Text = text;
            sourceTextBox.ReadOnly = false;
            sourceTextBox.BackColor = Color.White;
            ResetTitleLabel();

            // Button visibility
            translateButton.Visible = true;
            translateButton.Enabled = true;
            insertButton.Visible = _selectionManager != null;
            learnButton.Visible = _grammarLearnerService != null;

            // Form settings for edit mode
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(500, 400);
            this.MinimumSize = new Size(400, 300);
            this.MaximizeBox = true;
            this.MinimizeBox = true;

            // Position near mouse cursor
            if (location.HasValue)
            {
                this.Location = location.Value;
            }
            else
            {
                var cursorPosition = Cursor.Position;
                this.Location = new Point(cursorPosition.X + 10, cursorPosition.Y + 10);
            }

            // Ensure popup is on screen
            var screen = Screen.FromPoint(this.Location);
            if (this.Right > screen.WorkingArea.Right)
                this.Left = screen.WorkingArea.Right - this.Width;
            if (this.Bottom > screen.WorkingArea.Bottom)
                this.Top = screen.WorkingArea.Bottom - this.Height;

            // Start translation automatically (only if no pre-translated options)
            if (preTranslatedOptions == null || preTranslatedOptions.Count == 0)
            {
                _ = TranslateAsync();
            }
            else
            {
                // Pre-filled result: display it immediately
                DisplayResults(preTranslatedOptions);
            }
        }

        // Update RTL based on source text
        var language = _languageDetector.DetectLanguage(text);
        sourceTextBox.RightToLeft = language == Language.Persian ? RightToLeft.Yes : RightToLeft.No;

        // Always show source text box
        sourceTextBox.Visible = true;

        this.Show();
        this.Activate();

        // Start auto-close timer
        StartAutoCloseTimer();
    }

    private void OnMouseEnterForm(object? sender, EventArgs e)
    {
        isMouseOver = true;
    }

    private void OnMouseLeaveForm(object? sender, EventArgs e)
    {
        if (this.IsDisposed || this.Disposing)
            return;

        // Use a small delay to check if mouse is actually leaving the form
        // This prevents false triggers when moving between controls
        var timer = new System.Windows.Forms.Timer { Interval = 100 };
        timer.Tick += (s, args) =>
        {
            if (this.IsDisposed || this.Disposing)
            {
                timer.Stop();
                timer.Dispose();
                return;
            }

            try
            {
                var mousePos = this.PointToClient(Cursor.Position);
                if (!this.ClientRectangle.Contains(mousePos))
                {
                    isMouseOver = false;
                    // Restart timer when mouse leaves
                    StartAutoCloseTimer();
                }
            }
            catch (ObjectDisposedException)
            {
                // Form was disposed, ignore
            }

            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    private void StartAutoCloseTimer()
    {
        if (this.IsDisposed || this.Disposing)
            return;

        try
        {
            remainingSeconds = _configService.Config.Ui.PopupAutoCloseSeconds;
            autoCloseProgressBar.Maximum = remainingSeconds;
            autoCloseProgressBar.Value = 0;
            autoCloseTimer.Start();
            progressTimer.Start();
        }
        catch (ObjectDisposedException)
        {
            // Form was disposed, ignore
        }
    }

    private void OnAutoCloseTimerTick(object? sender, EventArgs e)
    {
        if (isMouseOver)
            return;

        remainingSeconds--;
        if (remainingSeconds <= 0)
        {
            autoCloseTimer.Stop();
            progressTimer.Stop();
            this.Close();
        }
    }

    private void OnProgressTimerTick(object? sender, EventArgs e)
    {
        if (this.IsDisposed || this.Disposing)
        {
            progressTimer?.Stop();
            return;
        }

        try
        {
            if (isMouseOver)
            {
                autoCloseProgressBar.Value = 0;
                return;
            }

            var totalSeconds = _configService.Config.Ui.PopupAutoCloseSeconds;
            var elapsed = totalSeconds - remainingSeconds;
            autoCloseProgressBar.Value = Math.Min(elapsed, autoCloseProgressBar.Maximum);
        }
        catch (ObjectDisposedException)
        {
            progressTimer?.Stop();
        }
    }

    private async void OnTranslateButtonClick(object? sender, EventArgs e)
    {
        await TranslateAsync();
    }

    private async void OnReadButtonClick(object? sender, EventArgs e)
    {
        if (_currentResults.Count == 0)
            return;

        // If only one result, read it directly
        if (_currentResults.Count == 1)
        {
            await ReadTextAsync(_currentResults[0]);
            return;
        }

        // Multiple results - enter selection mode
        if (!_isSelectingBoxToRead)
        {
            EnterReadSelectionMode();
        }
        else
        {
            ExitSelectionMode();
        }
    }

    private async void OnInsertButtonClick(object? sender, EventArgs e)
    {
        if (_currentResults.Count == 0 || _selectionManager == null)
            return;

        // If only one result, insert it directly
        if (_currentResults.Count == 1)
        {
            await InsertTextAsync(_currentResults[0]);
            return;
        }

        // Multiple results - enter selection mode
        if (!_isSelectingBoxToInsert)
        {
            EnterInsertSelectionMode();
        }
        else
        {
            ExitSelectionMode();
        }
    }

    private async void OnLearnButtonClick(object? sender, EventArgs e)
    {
        if (_currentResults.Count == 0 || _grammarLearnerService == null)
            return;

        // If only one result, learn it directly
        if (_currentResults.Count == 1)
        {
            await LearnGrammarAsync(_currentResults[0]);
            return;
        }

        // Multiple results - enter selection mode
        if (!_isSelectingBoxToLearn)
        {
            EnterLearnSelectionMode();
        }
        else
        {
            ExitSelectionMode();
        }
    }

    private void EnterReadSelectionMode()
    {
        _isSelectingBoxToRead = true;
        _isSelectingBoxToInsert = false;

        readButton.Text = "Cancel";
        readButton.BackColor = Color.Orange;
        insertButton.Enabled = false;
        translateButton.Enabled = false;

        titleLabel.Text = "Click on a box to read it";
        titleLabel.ForeColor = Color.Orange;

        // Highlight all result boxes
        HighlightResultBoxes(true);
    }

    private void EnterInsertSelectionMode()
    {
        _isSelectingBoxToInsert = true;
        _isSelectingBoxToRead = false;
        _isSelectingBoxToLearn = false;

        insertButton.Text = "Cancel";
        insertButton.BackColor = Color.Orange;
        readButton.Enabled = false;
        learnButton.Enabled = false;
        translateButton.Enabled = false;

        titleLabel.Text = "Click on a box to insert it into the application";
        titleLabel.ForeColor = Color.FromArgb(40, 167, 69);

        // Highlight all result boxes
        HighlightResultBoxes(true);
    }

    private void EnterLearnSelectionMode()
    {
        _isSelectingBoxToLearn = true;
        _isSelectingBoxToRead = false;
        _isSelectingBoxToInsert = false;

        learnButton.Text = "Cancel";
        learnButton.BackColor = Color.Orange;
        readButton.Enabled = false;
        insertButton.Enabled = false;
        translateButton.Enabled = false;

        titleLabel.Text = "Click on a box to learn grammar";
        titleLabel.ForeColor = Color.FromArgb(138, 43, 226);

        // Highlight all result boxes
        HighlightResultBoxes(true);
    }

    private void ExitSelectionMode()
    {
        _isSelectingBoxToRead = false;
        _isSelectingBoxToInsert = false;
        _isSelectingBoxToLearn = false;

        readButton.Text = "Read";
        readButton.BackColor = Color.FromArgb(0, 120, 215);
        readButton.Enabled = true;

        insertButton.Text = "Insert";
        insertButton.BackColor = Color.FromArgb(40, 167, 69);
        insertButton.Enabled = true;

        learnButton.Text = "Learn";
        learnButton.BackColor = Color.FromArgb(138, 43, 226);
        learnButton.Enabled = true;

        translateButton.Enabled = true;

        ResetTitleLabel();

        // Remove highlight from all result boxes
        HighlightResultBoxes(false);
    }

    private void HighlightResultBoxes(bool highlight)
    {
        foreach (Control control in resultsPanel.Controls)
        {
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = highlight ? BorderStyle.Fixed3D : BorderStyle.FixedSingle;
            }
        }
    }

    private async Task ReadTextAsync(string text)
    {
        try
        {
            ExitSelectionMode();
            SetButtonsEnabled(false);

            _cancellationTokenSource = new CancellationTokenSource();
            var language = _languageDetector.DetectLanguage(text);

            if (!titleLabel.IsDisposed)
            {
                titleLabel.Text = "Reading...";
                titleLabel.ForeColor = Color.Blue;
            }

            if (language == Language.Persian)
                await _ttsService.ReadPersianAsync(text, _cancellationTokenSource.Token);
            else
                await _ttsService.ReadEnglishAsync(text, _cancellationTokenSource.Token);

            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = "✓ Reading completed!";
                titleLabel.ForeColor = Color.Green;
            }
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation("Reading cancelled in popup");
            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = "Reading cancelled";
                titleLabel.ForeColor = Color.Orange;
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error reading text in popup", ex);
            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = $"Error: {ex.Message}";
                titleLabel.ForeColor = Color.Red;
            }
        }
        finally
        {
            SetButtonsEnabled(true);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Reset title after delay
            ResetTitleAfterDelay();
        }
    }

    private async Task InsertTextAsync(string text)
    {
        try
        {
            ExitSelectionMode();
            SetButtonsEnabled(false);

            if (!titleLabel.IsDisposed)
            {
                titleLabel.Text = "Inserting text...";
                titleLabel.ForeColor = Color.Blue;
            }

            if (_selectionManager != null)
            {
                var success = await _selectionManager.InsertTextAsync(text);

                if (success && !this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
                {
                    titleLabel.Text = "✓ Text inserted successfully!";
                    titleLabel.ForeColor = Color.Green;

                    _loggingService.LogInformation("Text inserted successfully");

                    // Close the form after a short delay
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);

                        if (this.IsDisposed || this.Disposing)
                            return;

                        try
                        {
                            if (this.InvokeRequired)
                            {
                                this.Invoke(() =>
                                {
                                    if (!this.IsDisposed && !this.Disposing)
                                        this.Close();
                                });
                            }
                            else
                            {
                                if (!this.IsDisposed && !this.Disposing)
                                    this.Close();
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            // Form was disposed, ignore
                        }
                        catch (InvalidOperationException)
                        {
                            // Form handle was destroyed, ignore
                        }
                    });
                    return; // Don't reset title if closing
                }
                else if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
                {
                    titleLabel.Text = "Failed to insert text";
                    titleLabel.ForeColor = Color.Red;
                }
            }
            else if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = "Insert feature not available";
                titleLabel.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error inserting text in popup", ex);
            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = $"Error: {ex.Message}";
                titleLabel.ForeColor = Color.Red;
            }
        }
        finally
        {
            SetButtonsEnabled(true);

            // Reset title after delay (if not closing)
            ResetTitleAfterDelay();
        }
    }

    private async Task LearnGrammarAsync(string text)
    {
        try
        {
            ExitSelectionMode();
            SetButtonsEnabled(false);

            if (!titleLabel.IsDisposed)
            {
                titleLabel.Text = "Learning grammar...";
                titleLabel.ForeColor = Color.Blue;
            }

            if (_grammarLearnerService == null)
            {
                if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
                {
                    titleLabel.Text = "Grammar Learner service not available";
                    titleLabel.ForeColor = Color.Red;
                }

                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var response = await _grammarLearnerService.LearnGrammarAsync(text, _cancellationTokenSource.Token);

            if (response == null)
            {
                if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
                {
                    titleLabel.Text = "Failed to learn grammar";
                    titleLabel.ForeColor = Color.Red;
                }

                return;
            }

            // Show Grammar Learner Form
            if (!this.IsDisposed && !this.Disposing)
            {
                var learnerForm = new GrammarLearnerForm(response, _configService);
                learnerForm.Show();
            }

            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = "✓ Grammar learning completed!";
                titleLabel.ForeColor = Color.Green;
            }

            _loggingService.LogInformation("Grammar learning completed successfully");
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation("Grammar learning cancelled in popup");
            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = "Grammar learning cancelled";
                titleLabel.ForeColor = Color.Orange;
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error learning grammar in popup", ex);
            if (!this.IsDisposed && !this.Disposing && !titleLabel.IsDisposed)
            {
                titleLabel.Text = $"Error: {ex.Message}";
                titleLabel.ForeColor = Color.Red;
            }
        }
        finally
        {
            SetButtonsEnabled(true);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Reset title after delay
            ResetTitleAfterDelay();
        }
    }

    private async Task TranslateAsync()
    {
        if (string.IsNullOrWhiteSpace(sourceTextBox.Text))
            return;

        try
        {
            ShowLoadingMessage();
            translateButton.Enabled = false;
            readButton.Enabled = false;

            _cancellationTokenSource = new CancellationTokenSource();
            TranslationResponse response;

            switch (_translationType)
            {
                case TranslationType.PersianToEnglish:
                    response = await _translationService.TranslatePersianToEnglishAsync(sourceTextBox.Text,
                        _cancellationTokenSource.Token);
                    break;
                case TranslationType.EnglishToPersian:
                    response = await _translationService.TranslateEnglishToPersianAsync(sourceTextBox.Text,
                        _cancellationTokenSource.Token);
                    break;
                case TranslationType.GrammarFix:
                    response = await _translationService.FixGrammarAsync(sourceTextBox.Text,
                        _cancellationTokenSource.Token);
                    break;
                default:
                    throw new ArgumentException("Invalid translation type");
            }

            if (response.Success)
            {
                // Split response by %%%%% separator
                var results = TranslationHelper.ParseTranslationOptions(response.Text);
                DisplayResults(results);
            }
            else
            {
                ShowErrorMessage($"Error: {response.Error}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Translation error in popup", ex);
            ShowErrorMessage($"Error: {ex.Message}");
        }
        finally
        {
            translateButton.Enabled = true;
            readButton.Enabled = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }


    private void ShowLoadingMessage()
    {
        resultsPanel.Controls.Clear();
        _currentResults.Clear();

        var loadingLabel = new Label
        {
            Text = "Translating...",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", _configService.Config.Ui.PopupFontSize),
            ForeColor = Color.Gray
        };

        resultsPanel.Controls.Add(loadingLabel);
    }

    private void ShowErrorMessage(string message)
    {
        resultsPanel.Controls.Clear();
        _currentResults.Clear();

        var errorLabel = new Label
        {
            Text = message,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", _configService.Config.Ui.PopupFontSize),
            ForeColor = Color.Red,
            BackColor = Color.LightPink
        };

        resultsPanel.Controls.Add(errorLabel);
    }

    /// <summary>
    /// Sets the translation result directly without re-translating (used when result is already available)
    /// </summary>
    public void SetTranslationResult(string result)
    {
        DisplayResults(new List<string> { result });
    }

    private void DisplayResults(List<string> results)
    {
        resultsPanel.Controls.Clear();
        _currentResults = results;

        if (results.Count == 0)
        {
            ShowErrorMessage("No results found");
            return;
        }

        var fontSize = _configService.Config.Ui.PopupFontSize;
        var font = new Font("Segoe UI", fontSize);
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
                Height = Math.Min(150,
                    TranslationHelper.GetTextHeight(this, result, font,
                        resultsPanel.ClientSize.Width - (spacing * 2) - SystemInformation.VerticalScrollBarWidth))
            };

            var originalColorForClosure = originalColor;

            // Add click handler to copy to clipboard, read, insert, or learn
            resultBox.Click += async (s, e) =>
            {
                if (_isSelectingBoxToRead)
                {
                    await ReadTextAsync(result);
                }
                else if (_isSelectingBoxToInsert)
                {
                    await InsertTextAsync(result);
                }
                else if (_isSelectingBoxToLearn)
                {
                    await LearnGrammarAsync(result);
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
                    textBox.Width = resultsPanel.ClientSize.Width - (spacing * 2) -
                                    SystemInformation.VerticalScrollBarWidth;
                }
            }
        };
    }

    private void CopyResultToClipboard(string text, TextBox resultBox)
    {
        if (this.IsDisposed || this.Disposing || resultBox.IsDisposed)
            return;

        try
        {
            _clipboardManager.SetClipboardText(text);

            // Visual feedback
            var originalColor = resultBox.BackColor;
            resultBox.BackColor = Color.FromArgb(200, 255, 200); // Green flash

            var timer = new System.Windows.Forms.Timer { Interval = 300 };
            timer.Tick += (s, e) =>
            {
                if (this.IsDisposed || this.Disposing || resultBox.IsDisposed)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

                try
                {
                    resultBox.BackColor = originalColor;
                }
                catch (ObjectDisposedException)
                {
                    // Control was disposed, ignore
                }

                timer.Stop();
                timer.Dispose();
            };
            timer.Start();

            // Update title if in selection mode
            if (_isSelectionMode && !titleLabel.IsDisposed)
            {
                titleLabel.Text = "✓ Copied to clipboard! Click another option or close this window.";
                titleLabel.ForeColor = Color.FromArgb(0, 150, 0);

                var resetTimer = new System.Windows.Forms.Timer { Interval = 2000 };
                resetTimer.Tick += (s2, e2) =>
                {
                    if (this.IsDisposed || this.Disposing || titleLabel.IsDisposed)
                    {
                        resetTimer.Stop();
                        resetTimer.Dispose();
                        return;
                    }

                    try
                    {
                        titleLabel.Text =
                            "Multiple translation options found. Click on any option to copy to clipboard:";
                        titleLabel.ForeColor = Color.FromArgb(0, 120, 215);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Control was disposed, ignore
                    }

                    resetTimer.Stop();
                    resetTimer.Dispose();
                };
                resetTimer.Start();
            }

            _loggingService.LogInformation("Result copied to clipboard from popup");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error copying to clipboard", ex);
            if (!this.IsDisposed && !this.Disposing)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Stop all timers before disposing
            try
            {
                autoCloseTimer?.Stop();
                progressTimer?.Stop();
            }
            catch
            {
                // Ignore errors when stopping timers
            }

            // Cancel any ongoing operations
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            catch
            {
                // Ignore errors when canceling operations
            }

            // Dispose timers
            try
            {
                autoCloseTimer?.Dispose();
                progressTimer?.Dispose();
            }
            catch
            {
                // Ignore errors when disposing timers
            }
        }

        base.Dispose(disposing);
    }
}

