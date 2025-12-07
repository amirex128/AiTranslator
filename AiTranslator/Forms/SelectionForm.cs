using AiTranslator.Services;

namespace AiTranslator.Forms;

public class SelectionForm : Form
{
    private Panel mainPanel = null!;
    private Label titleLabel = null!;
    private Button closeButton = null!;
    private readonly IConfigService _configService;

    public SelectionForm(List<string> options, IConfigService configService)
    {
        _configService = configService;
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

        closeButton = new Button
        {
            Dock = DockStyle.Bottom,
            Text = "Close",
            Height = 40,
            Font = buttonFont,
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();

        containerPanel.Controls.Add(mainPanel);
        containerPanel.Controls.Add(closeButton);
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
        panel.Click += (s, e) => CopyToClipboard(text, panel);
        textBox.Click += (s, e) => CopyToClipboard(text, panel);
        numberLabel.Click += (s, e) => CopyToClipboard(text, panel);

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
}

