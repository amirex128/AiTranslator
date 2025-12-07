using AiTranslator.Models;
using AiTranslator.Services;
using AiTranslator.Utilities;

namespace AiTranslator.Forms;

public class SelectionForm : Form
{
    private Panel mainPanel = null!;
    private Label titleLabel = null!;
    private Button closeButton = null!;
    private Button readButton = null!;
    private readonly IConfigService _configService;
    private readonly ITtsService _ttsService;
    private readonly ILanguageDetector _languageDetector;
    private readonly ILoggingService _loggingService;
    private bool _isSelectingBoxToRead = false;
    private CancellationTokenSource? _cancellationTokenSource;

    public SelectionForm(
        List<string> options, 
        IConfigService configService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        ILoggingService loggingService)
    {
        _configService = configService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _loggingService = loggingService;
        InitializeComponents();
        PopulateOptions(options);
    }

    private void InitializeComponents()
    {
        this.Text = "Select Translation";
        this.Size = new Size(700, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.TopMost = true;
        this.BackColor = Color.White;

        var containerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };

        var fontSize = _configService.Config.Ui.SelectionFormFontSize;
        var titleFont = new Font("Segoe UI", fontSize + 1, FontStyle.Bold);
        var buttonFont = new Font("Segoe UI", fontSize, FontStyle.Bold);

        titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = "Multiple translation options found. Click on any option to copy to clipboard:",
            Font = titleFont,
            Height = 50,
            ForeColor = Color.FromArgb(0, 120, 215),
            TextAlign = ContentAlignment.MiddleLeft
        };

        mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(5)
        };

        var buttonsPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40
        };

        readButton = new Button
        {
            Text = "Read",
            Location = new Point(10, 5),
            Size = new Size(100, 30),
            Font = buttonFont,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        readButton.FlatAppearance.BorderSize = 0;
        readButton.Click += OnReadButtonClick;

        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(120, 5),
            Size = new Size(100, 30),
            Font = buttonFont,
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();

        buttonsPanel.Controls.Add(closeButton);
        buttonsPanel.Controls.Add(readButton);

        containerPanel.Controls.Add(mainPanel);
        containerPanel.Controls.Add(buttonsPanel);
        containerPanel.Controls.Add(titleLabel);

        this.Controls.Add(containerPanel);
    }

    private void PopulateOptions(List<string> options)
    {
        int yPosition = 10;
        int optionNumber = 1;

        foreach (var option in options)
        {
            var optionPanel = CreateOptionPanel(option.Trim(), optionNumber, yPosition);
            mainPanel.Controls.Add(optionPanel);
            yPosition += optionPanel.Height + 15;
            optionNumber++;
        }
    }

    private Panel CreateOptionPanel(string text, int number, int yPosition)
    {
        var fontSize = _configService.Config.Ui.SelectionFormFontSize;
        var font = new Font("Segoe UI", fontSize);
        var labelFont = new Font("Segoe UI", fontSize, FontStyle.Bold);

        var panel = new Panel
        {
            Location = new Point(10, yPosition),
            Size = new Size(640, 100),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(250, 250, 250),
            Cursor = Cursors.Hand,
            Tag = text
        };

        var numberLabel = new Label
        {
            Text = $"Option {number}",
            Location = new Point(10, 5),
            Size = new Size(620, 20),
            Font = labelFont,
            ForeColor = Color.FromArgb(0, 120, 215),
            BackColor = Color.Transparent
        };

        var textBox = new TextBox
        {
            Text = text,
            Location = new Point(10, 30),
            Size = new Size(620, 60),
            Multiline = true,
            ReadOnly = true,
            Font = font,
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            Cursor = Cursors.Hand,
            ScrollBars = ScrollBars.Vertical,
            Tag = text
        };

        // Detect RTL for Persian text
        if (ContainsPersian(text))
        {
            textBox.RightToLeft = RightToLeft.Yes;
        }

        // Click events
        panel.Click += async (s, e) =>
        {
            if (_isSelectingBoxToRead)
            {
                await ReadTextAsync(text);
            }
            else
            {
                CopyToClipboard(text, panel);
            }
        };
        textBox.Click += async (s, e) =>
        {
            if (_isSelectingBoxToRead)
            {
                await ReadTextAsync(text);
            }
            else
            {
                CopyToClipboard(text, panel);
            }
        };
        numberLabel.Click += async (s, e) =>
        {
            if (_isSelectingBoxToRead)
            {
                await ReadTextAsync(text);
            }
            else
            {
                CopyToClipboard(text, panel);
            }
        };

        // Hover effect
        panel.MouseEnter += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(230, 240, 255);
            panel.BorderStyle = BorderStyle.FixedSingle;
        };
        panel.MouseLeave += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(250, 250, 250);
        };

        textBox.MouseEnter += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(230, 240, 255);
        };
        textBox.MouseLeave += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(250, 250, 250);
        };

        panel.Controls.Add(textBox);
        panel.Controls.Add(numberLabel);

        return panel;
    }

    private void CopyToClipboard(string text, Panel panel)
    {
        try
        {
            Clipboard.SetText(text);
            
            // Visual feedback
            var originalColor = panel.BackColor;
            panel.BackColor = Color.FromArgb(200, 255, 200);
            
            // Flash effect
            var timer = new System.Windows.Forms.Timer { Interval = 200 };
            timer.Tick += (s, e) =>
            {
                panel.BackColor = originalColor;
                timer.Stop();
                timer.Dispose();
                
                // Show success message
                titleLabel.Text = "✓ Copied to clipboard! Click another option or close this window.";
                titleLabel.ForeColor = Color.FromArgb(0, 150, 0);
                
                // Reset message after 2 seconds
                var resetTimer = new System.Windows.Forms.Timer { Interval = 2000 };
                resetTimer.Tick += (s2, e2) =>
                {
                    titleLabel.Text = "Multiple translation options found. Click on any option to copy to clipboard:";
                    titleLabel.ForeColor = Color.FromArgb(0, 120, 215);
                    resetTimer.Stop();
                    resetTimer.Dispose();
                };
                resetTimer.Start();
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ContainsPersian(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        foreach (char c in text)
        {
            if (c >= '\u0600' && c <= '\u06FF')
                return true;
        }
        return false;
    }

    private async void OnReadButtonClick(object? sender, EventArgs e)
    {
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

        // Highlight all option panels
        foreach (Control control in mainPanel.Controls)
        {
            if (control is Panel panel)
            {
                panel.BorderStyle = BorderStyle.Fixed3D;
            }
        }
    }

    private void ExitBoxSelectionMode()
    {
        _isSelectingBoxToRead = false;
        readButton.Text = "Read";
        readButton.BackColor = Color.FromArgb(0, 120, 215);
        titleLabel.Text = "Multiple translation options found. Click on any option to copy to clipboard:";
        titleLabel.ForeColor = Color.FromArgb(0, 120, 215);

        // Remove highlight from all option panels
        foreach (Control control in mainPanel.Controls)
        {
            if (control is Panel panel)
            {
                panel.BorderStyle = BorderStyle.FixedSingle;
            }
        }
    }

    private async Task ReadTextAsync(string text)
    {
        try
        {
            ExitBoxSelectionMode();
            readButton.Enabled = false;
            closeButton.Enabled = false;

            _cancellationTokenSource = new CancellationTokenSource();
            var language = _languageDetector.DetectLanguage(text);

            titleLabel.Text = "Reading...";
            titleLabel.ForeColor = Color.Blue;

            if (language == Language.Persian)
                await _ttsService.ReadPersianAsync(text, _cancellationTokenSource.Token);
            else
                await _ttsService.ReadEnglishAsync(text, _cancellationTokenSource.Token);

            titleLabel.Text = "✓ Reading completed!";
            titleLabel.ForeColor = Color.Green;
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation("Reading cancelled in selection form");
            titleLabel.Text = "Reading cancelled";
            titleLabel.ForeColor = Color.Orange;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error reading text in selection form", ex);
            titleLabel.Text = $"Error: {ex.Message}";
            titleLabel.ForeColor = Color.Red;
        }
        finally
        {
            readButton.Enabled = true;
            closeButton.Enabled = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Reset title after 2 seconds
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                
                if (this.IsDisposed || titleLabel.IsDisposed)
                    return;

                this.Invoke(() =>
                {
                    if (!titleLabel.IsDisposed)
                    {
                        titleLabel.Text = "Multiple translation options found. Click on any option to copy to clipboard:";
                        titleLabel.ForeColor = Color.FromArgb(0, 120, 215);
                    }
                });
            });
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource?.Dispose();
        }
        base.Dispose(disposing);
    }
}

