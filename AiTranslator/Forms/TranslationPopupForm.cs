using AiTranslator.Models;
using AiTranslator.Services;

namespace AiTranslator.Forms;

public class TranslationPopupForm : Form
{
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;
    private readonly ILanguageDetector _languageDetector;
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;

    private TextBox sourceTextBox = null!;
    private TextBox resultTextBox = null!;
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

    public TranslationPopupForm(
        ITranslationService translationService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        IConfigService configService,
        ILoggingService loggingService)
    {
        _translationService = translationService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _configService = configService;
        _loggingService = loggingService;

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

        resultTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = font,
            BackColor = Color.LightYellow
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

        mainPanel.Controls.Add(resultTextBox);
        mainPanel.Controls.Add(resultLabel);
        mainPanel.Controls.Add(sourceTextBox);
        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(buttonsPanel);

        this.Controls.Add(mainPanel);
        this.Controls.Add(autoCloseProgressBar);

        this.MouseEnter += (s, e) => { isMouseOver = true; };
        this.MouseLeave += (s, e) => { isMouseOver = false; };
        sourceTextBox.MouseEnter += (s, e) => { isMouseOver = true; };
        resultTextBox.MouseEnter += (s, e) => { isMouseOver = true; };
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
        if (string.IsNullOrWhiteSpace(resultTextBox.Text))
            return;

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var language = _languageDetector.DetectLanguage(resultTextBox.Text);

            if (language == Language.Persian)
                await _ttsService.ReadPersianAsync(resultTextBox.Text, _cancellationTokenSource.Token);
            else
                await _ttsService.ReadEnglishAsync(resultTextBox.Text, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error reading text in popup", ex);
            MessageBox.Show($"Error reading text: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
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
            resultTextBox.Text = "Translating...";
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
                resultTextBox.Text = response.Text;
                
                // Update RTL for result
                var language = _languageDetector.DetectLanguage(response.Text);
                resultTextBox.RightToLeft = language == Language.Persian ? RightToLeft.Yes : RightToLeft.No;
            }
            else
            {
                resultTextBox.Text = $"Error: {response.Error}";
                resultTextBox.BackColor = Color.LightPink;
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Translation error in popup", ex);
            resultTextBox.Text = $"Error: {ex.Message}";
            resultTextBox.BackColor = Color.LightPink;
        }
        finally
        {
            translateButton.Enabled = true;
            readButton.Enabled = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
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

