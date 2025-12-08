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
    private Button insertButton = null!;
    private ProgressBar autoCloseProgressBar = null!;
    private readonly IConfigService _configService;
    private readonly ITtsService _ttsService;
    private readonly ILanguageDetector _languageDetector;
    private readonly ILoggingService _loggingService;
    private readonly SelectionManager? _selectionManager;
    private bool _isSelectingBoxToRead = false;
    private bool _isSelectingBoxToInsert = false;
    private CancellationTokenSource? _cancellationTokenSource;
    
    private System.Windows.Forms.Timer autoCloseTimer = null!;
    private System.Windows.Forms.Timer progressTimer = null!;
    private int remainingSeconds;
    private bool isMouseOver = false;

    public SelectionForm(
        List<string> options, 
        IConfigService configService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        ILoggingService loggingService,
        SelectionManager? selectionManager = null)
    {
        _configService = configService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _loggingService = loggingService;
        _selectionManager = selectionManager;
        InitializeComponents();
        InitializeTimers();
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
            Height = 50
        };

        autoCloseProgressBar = new ProgressBar
        {
            Location = new Point(10, 5),
            Size = new Size(640, 5),
            Style = ProgressBarStyle.Continuous,
            ForeColor = Color.FromArgb(0, 120, 215)
        };

        readButton = new Button
        {
            Text = "Read",
            Location = new Point(10, 15),
            Size = new Size(80, 30),
            Font = buttonFont,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        readButton.FlatAppearance.BorderSize = 0;
        readButton.Click += OnReadButtonClick;
        readButton.MouseEnter += OnMouseEnterForm;
        readButton.MouseLeave += OnMouseLeaveForm;

        insertButton = new Button
        {
            Text = "Insert",
            Location = new Point(100, 15),
            Size = new Size(80, 30),
            Font = buttonFont,
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Visible = _selectionManager != null // Only show if SelectionManager is available
        };
        insertButton.FlatAppearance.BorderSize = 0;
        insertButton.Click += OnInsertButtonClick;
        insertButton.MouseEnter += OnMouseEnterForm;
        insertButton.MouseLeave += OnMouseLeaveForm;

        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(190, 15),
            Size = new Size(80, 30),
            Font = buttonFont,
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();
        closeButton.MouseEnter += OnMouseEnterForm;
        closeButton.MouseLeave += OnMouseLeaveForm;

        buttonsPanel.Controls.Add(closeButton);
        buttonsPanel.Controls.Add(insertButton);
        buttonsPanel.Controls.Add(readButton);
        buttonsPanel.Controls.Add(autoCloseProgressBar);
        
        // Mouse events for form
        this.MouseEnter += OnMouseEnterForm;
        this.MouseLeave += OnMouseLeaveForm;
        mainPanel.MouseEnter += OnMouseEnterForm;
        mainPanel.MouseLeave += OnMouseLeaveForm;
        titleLabel.MouseEnter += OnMouseEnterForm;
        titleLabel.MouseLeave += OnMouseLeaveForm;

        containerPanel.Controls.Add(mainPanel);
        containerPanel.Controls.Add(buttonsPanel);
        containerPanel.Controls.Add(titleLabel);

        this.Controls.Add(containerPanel);
        
        // Start auto-close timer after form is shown
        this.Shown += (s, e) => StartAutoCloseTimer();
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
            return;

        var elapsed = _configService.Config.Ui.PopupAutoCloseSeconds - remainingSeconds;
        autoCloseProgressBar.Value = Math.Min(elapsed, autoCloseProgressBar.Maximum);
    }

    private void OnMouseEnterForm(object? sender, EventArgs e)
    {
        isMouseOver = true;
    }

    private void OnMouseLeaveForm(object? sender, EventArgs e)
    {
        isMouseOver = false;
        
        // Use a small delay to check if mouse really left the form
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
            else if (_isSelectingBoxToInsert)
            {
                await InsertTextAsync(text);
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
            else if (_isSelectingBoxToInsert)
            {
                await InsertTextAsync(text);
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
            else if (_isSelectingBoxToInsert)
            {
                await InsertTextAsync(text);
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
            OnMouseEnterForm(s, e);
        };
        panel.MouseLeave += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(250, 250, 250);
            OnMouseLeaveForm(s, e);
        };

        textBox.MouseEnter += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(230, 240, 255);
            OnMouseEnterForm(s, e);
        };
        textBox.MouseLeave += (s, e) =>
        {
            panel.BackColor = Color.FromArgb(250, 250, 250);
            OnMouseLeaveForm(s, e);
        };
        
        numberLabel.MouseEnter += (s, e) => OnMouseEnterForm(s, e);
        numberLabel.MouseLeave += (s, e) => OnMouseLeaveForm(s, e);

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
            EnterReadSelectionMode();
        }
        else
        {
            ExitSelectionMode();
        }
    }

    private async void OnInsertButtonClick(object? sender, EventArgs e)
    {
        if (!_isSelectingBoxToInsert)
        {
            EnterInsertSelectionMode();
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

    private void EnterInsertSelectionMode()
    {
        _isSelectingBoxToInsert = true;
        _isSelectingBoxToRead = false;
        
        insertButton.Text = "Cancel";
        insertButton.BackColor = Color.Orange;
        readButton.Enabled = false;
        
        titleLabel.Text = "Click on a box to insert it into the application";
        titleLabel.ForeColor = Color.FromArgb(40, 167, 69);

        // Highlight all option panels
        foreach (Control control in mainPanel.Controls)
        {
            if (control is Panel panel)
            {
                panel.BorderStyle = BorderStyle.Fixed3D;
            }
        }
    }

    private void ExitSelectionMode()
    {
        _isSelectingBoxToRead = false;
        _isSelectingBoxToInsert = false;
        
        readButton.Text = "Read";
        readButton.BackColor = Color.FromArgb(0, 120, 215);
        readButton.Enabled = true;
        
        insertButton.Text = "Insert";
        insertButton.BackColor = Color.FromArgb(40, 167, 69);
        insertButton.Enabled = true;
        
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
            ExitSelectionMode();
            readButton.Enabled = false;
            insertButton.Enabled = false;
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
            insertButton.Enabled = true;
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

    private async Task InsertTextAsync(string text)
    {
        try
        {
            ExitSelectionMode();
            readButton.Enabled = false;
            insertButton.Enabled = false;
            closeButton.Enabled = false;

            titleLabel.Text = "Inserting text...";
            titleLabel.ForeColor = Color.Blue;

            if (_selectionManager != null)
            {
                var success = await _selectionManager.InsertTextAsync(text);

                if (success)
                {
                    titleLabel.Text = "✓ Text inserted successfully!";
                    titleLabel.ForeColor = Color.Green;
                    
                    _loggingService.LogInformation("Text inserted successfully");
                    
                    // Close the form after a short delay
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        
                        if (!this.IsDisposed)
                        {
                            this.Invoke(() =>
                            {
                                if (!this.IsDisposed)
                                {
                                    this.Close();
                                }
                            });
                        }
                    });
                }
                else
                {
                    titleLabel.Text = "Failed to insert text";
                    titleLabel.ForeColor = Color.Red;
                }
            }
            else
            {
                titleLabel.Text = "Insert feature not available";
                titleLabel.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error inserting text in selection form", ex);
            titleLabel.Text = $"Error: {ex.Message}";
            titleLabel.ForeColor = Color.Red;
        }
        finally
        {
            readButton.Enabled = true;
            insertButton.Enabled = true;
            closeButton.Enabled = true;

            // Reset title after 2 seconds (if not closing)
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
            autoCloseTimer?.Dispose();
            progressTimer?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
        base.Dispose(disposing);
    }
}

