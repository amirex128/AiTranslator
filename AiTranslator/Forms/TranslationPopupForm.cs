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

    private TextBox sourceTextBox = null!;
    private Panel resultsPanel = null!;
    private Button translateButton = null!;
    private Button readButton = null!;
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
    private TextBox? _selectedBoxForReading = null;

    public TranslationPopupForm(
        ITranslationService translationService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        IConfigService configService,
        ILoggingService loggingService,
        ClipboardManager clipboardManager)
    {
        _translationService = translationService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _configService = configService;
        _loggingService = loggingService;
        _clipboardManager = clipboardManager;

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
            Font = font
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
            Height = 40
        };

        translateButton = new Button
        {
            Text = "Translate",
            Location = new Point(10, 5),
            Size = new Size(100, 30),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        translateButton.Click += OnTranslateButtonClick;

        readButton = new Button
        {
            Text = "Read",
            Location = new Point(120, 5),
            Size = new Size(100, 30),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        readButton.Click += OnReadButtonClick;

        autoCloseProgressBar = new ProgressBar
        {
            Dock = DockStyle.Bottom,
            Height = 10,
            Style = ProgressBarStyle.Continuous
        };

        buttonsPanel.Controls.Add(readButton);
        buttonsPanel.Controls.Add(translateButton);

        mainPanel.Controls.Add(resultsPanel);
        mainPanel.Controls.Add(resultLabel);
        mainPanel.Controls.Add(sourceTextBox);
        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(buttonsPanel);

        this.Controls.Add(mainPanel);
        this.Controls.Add(autoCloseProgressBar);

        // Handle mouse enter/leave for all controls to properly track mouse position
        this.MouseEnter += OnMouseEnterForm;
        this.MouseLeave += OnMouseLeaveForm;
        sourceTextBox.MouseEnter += OnMouseEnterForm;
        sourceTextBox.MouseLeave += OnMouseLeaveForm;
        resultsPanel.MouseEnter += OnMouseEnterForm;
        resultsPanel.MouseLeave += OnMouseLeaveForm;
        translateButton.MouseEnter += OnMouseEnterForm;
        translateButton.MouseLeave += OnMouseLeaveForm;
        readButton.MouseEnter += OnMouseEnterForm;
        readButton.MouseLeave += OnMouseLeaveForm;
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

    public void ShowPopup(string text, TranslationType type, Point? location = null)
    {
        _translationType = type;
        sourceTextBox.Text = text;
        titleLabel.Text = type.ToString().Replace("To", " → ");

        // Update RTL based on source text
        var language = _languageDetector.DetectLanguage(text);
        sourceTextBox.RightToLeft = language == Language.Persian ? RightToLeft.Yes : RightToLeft.No;

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

        this.Show();
        this.Activate();

        // Start translation automatically
        _ = TranslateAsync();

        // Start auto-close timer
        StartAutoCloseTimer();
    }

    private void OnMouseEnterForm(object? sender, EventArgs e)
    {
        isMouseOver = true;
    }

    private void OnMouseLeaveForm(object? sender, EventArgs e)
    {
        // Use a small delay to check if mouse is actually leaving the form
        // This prevents false triggers when moving between controls
        var timer = new System.Windows.Forms.Timer { Interval = 100 };
        timer.Tick += (s, args) =>
        {
            var mousePos = this.PointToClient(Cursor.Position);
            if (!this.ClientRectangle.Contains(mousePos))
            {
                isMouseOver = false;
                // Restart timer when mouse leaves
                StartAutoCloseTimer();
            }
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    private void StartAutoCloseTimer()
    {
        remainingSeconds = _configService.Config.Ui.PopupAutoCloseSeconds;
        autoCloseProgressBar.Maximum = remainingSeconds;
        autoCloseProgressBar.Value = 0;
        autoCloseTimer.Start();
        progressTimer.Start();
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
        if (isMouseOver)
        {
            autoCloseProgressBar.Value = 0;
            return;
        }

        var totalSeconds = _configService.Config.Ui.PopupAutoCloseSeconds;
        var elapsed = totalSeconds - remainingSeconds;
        autoCloseProgressBar.Value = Math.Min(elapsed, autoCloseProgressBar.Maximum);
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
            EnterBoxSelectionMode();
        }
        else
        {
            ExitBoxSelectionMode();
        }
    }

    private void EnterBoxSelectionMode()
    {
        _isSelectingBoxToRead = true;
        readButton.Text = "Cancel";
        readButton.BackColor = Color.Orange;
        titleLabel.Text = "Click on a box to read it";
        titleLabel.ForeColor = Color.Orange;

        // Highlight all result boxes
        foreach (Control control in resultsPanel.Controls)
        {
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.Fixed3D;
            }
        }
    }

    private void ExitBoxSelectionMode()
    {
        _isSelectingBoxToRead = false;
        readButton.Text = "Read";
        readButton.BackColor = SystemColors.Control;
        titleLabel.Text = _translationType.ToString().Replace("To", " → ");
        titleLabel.ForeColor = SystemColors.ControlText;

        // Remove highlight from all result boxes
        foreach (Control control in resultsPanel.Controls)
        {
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }
    }

    private async Task ReadTextAsync(string text)
    {
        try
        {
            ExitBoxSelectionMode();
            readButton.Enabled = false;
            translateButton.Enabled = false;

            _cancellationTokenSource = new CancellationTokenSource();
            var language = _languageDetector.DetectLanguage(text);

            if (language == Language.Persian)
                await _ttsService.ReadPersianAsync(text, _cancellationTokenSource.Token);
            else
                await _ttsService.ReadEnglishAsync(text, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation("Reading cancelled in popup");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error reading text in popup", ex);
            MessageBox.Show($"Error reading text: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            readButton.Enabled = true;
            translateButton.Enabled = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
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
                    response = await _translationService.TranslatePersianToEnglishAsync(sourceTextBox.Text, _cancellationTokenSource.Token);
                    break;
                case TranslationType.EnglishToPersian:
                    response = await _translationService.TranslateEnglishToPersianAsync(sourceTextBox.Text, _cancellationTokenSource.Token);
                    break;
                case TranslationType.GrammarFix:
                    response = await _translationService.FixGrammarAsync(sourceTextBox.Text, _cancellationTokenSource.Token);
                    break;
                default:
                    throw new ArgumentException("Invalid translation type");
            }

            if (response.Success)
            {
                // Split response by %%%%% separator
                var results = ParseTranslationOptions(response.Text);
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
        var originalColors = new List<Color>();

        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var language = _languageDetector.DetectLanguage(result);
            var originalColor = i % 2 == 0 ? Color.LightYellow : Color.LightBlue;
            originalColors.Add(originalColor);

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

            var index = i; // Capture for closure
            var originalColorForClosure = originalColor;

            // Add click handler to copy to clipboard or read
            resultBox.Click += async (s, e) =>
            {
                if (_isSelectingBoxToRead)
                {
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

            _loggingService.LogInformation("Result copied to clipboard from popup");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error copying to clipboard", ex);
            MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            autoCloseTimer?.Dispose();
            progressTimer?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
        base.Dispose(disposing);
    }
}

