namespace AiTranslator.Forms;

public class LoadingPopupForm : Form
{
    private Label messageLabel = null!;
    private ProgressBar progressBar = null!;

    public LoadingPopupForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "";
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.TopMost = true;
        this.Size = new Size(300, 100);
        this.BackColor = Color.White;
        this.ControlBox = false;

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            BorderStyle = BorderStyle.FixedSingle
        };

        messageLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = "Processing...",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 30
        };

        progressBar = new ProgressBar
        {
            Dock = DockStyle.Bottom,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Height = 20
        };

        mainPanel.Controls.Add(progressBar);
        mainPanel.Controls.Add(messageLabel);

        this.Controls.Add(mainPanel);
    }

    public void ShowLoading(string message = "Processing...")
    {
        messageLabel.Text = message;
        this.Show();
        this.Refresh();
        Application.DoEvents();
    }

    public void HideLoading()
    {
        this.Hide();
    }

    public void UpdateMessage(string message)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => messageLabel.Text = message));
        }
        else
        {
            messageLabel.Text = message;
            this.Refresh();
        }
    }
}

