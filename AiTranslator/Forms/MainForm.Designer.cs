namespace AiTranslator.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.editToolStripMenuItem = new ToolStripMenuItem();
            this.settingsToolStripMenuItem = new ToolStripMenuItem();
            this.copyResultToolStripMenuItem = new ToolStripMenuItem();
            this.viewToolStripMenuItem = new ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new ToolStripMenuItem();
            this.helpToolStripMenuItem = new ToolStripMenuItem();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            this.shortcutsToolStripMenuItem = new ToolStripMenuItem();
            
            this.mainPanel = new Panel();
            this.inputsPanel = new Panel();
            
            this.persianToEnglishPanel = new Panel();
            this.persianToEnglishLabel = new Label();
            this.persianToEnglishTextBox = new TextBox();
            this.persianToEnglishCountLabel = new Label();
            
            this.englishToPersianPanel = new Panel();
            this.englishToPersianLabel = new Label();
            this.englishToPersianTextBox = new TextBox();
            this.englishToPersianCountLabel = new Label();
            
            this.grammarFixPanel = new Panel();
            this.grammarFixLabel = new Label();
            this.grammarFixTextBox = new TextBox();
            this.grammarFixCountLabel = new Label();
            
            this.controlsPanel = new Panel();
            this.translateButton = new Button();
            this.readButton = new Button();
            this.cancelButton = new Button();
            this.loadingLabel = new Label();
            
            this.resultPanel = new Panel();
            this.resultLabel = new Label();
            this.resultTabControl = new TabControl();
            this.copyResultButton = new Button();
            
            this.shortcutsPanel = new Panel();
            this.shortcutsLabel = new Label();
            this.shortcutsListView = new ListView();
            this.toggleShortcutsButton = new Button();
            
            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel();
            
            this.menuStrip.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.inputsPanel.SuspendLayout();
            this.persianToEnglishPanel.SuspendLayout();
            this.englishToPersianPanel.SuspendLayout();
            this.grammarFixPanel.SuspendLayout();
            this.controlsPanel.SuspendLayout();
            this.resultPanel.SuspendLayout();
            this.shortcutsPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            
            // menuStrip
            this.menuStrip.Items.AddRange(new ToolStripItem[] {
                this.fileToolStripMenuItem,
                this.editToolStripMenuItem,
                this.viewToolStripMenuItem,
                this.helpToolStripMenuItem
            });
            this.menuStrip.Location = new Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new Size(900, 24);
            this.menuStrip.TabIndex = 0;
            
            // fileToolStripMenuItem
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.exitToolStripMenuItem
            });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            
            // exitToolStripMenuItem
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(93, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            
            // editToolStripMenuItem
            this.editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.settingsToolStripMenuItem,
                this.copyResultToolStripMenuItem
            });
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            
            // settingsToolStripMenuItem
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new Size(144, 22);
            this.settingsToolStripMenuItem.Text = "&Settings";
            
            // copyResultToolStripMenuItem
            this.copyResultToolStripMenuItem.Name = "copyResultToolStripMenuItem";
            this.copyResultToolStripMenuItem.Size = new Size(144, 22);
            this.copyResultToolStripMenuItem.Text = "&Copy Result";
            
            // viewToolStripMenuItem
            this.viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.alwaysOnTopToolStripMenuItem
            });
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            
            // alwaysOnTopToolStripMenuItem
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new Size(159, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "&Always On Top";
            
            // helpToolStripMenuItem
            this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.aboutToolStripMenuItem,
                this.shortcutsToolStripMenuItem
            });
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            
            // aboutToolStripMenuItem
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new Size(128, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            
            // shortcutsToolStripMenuItem
            this.shortcutsToolStripMenuItem.Name = "shortcutsToolStripMenuItem";
            this.shortcutsToolStripMenuItem.Size = new Size(128, 22);
            this.shortcutsToolStripMenuItem.Text = "S&hortcuts";
            
            // mainPanel
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 24);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.Size = new Size(900, 654);
            this.mainPanel.TabIndex = 1;
            this.mainPanel.Controls.Add(this.shortcutsPanel);
            this.mainPanel.Controls.Add(this.resultPanel);
            this.mainPanel.Controls.Add(this.controlsPanel);
            this.mainPanel.Controls.Add(this.inputsPanel);
            
            // inputsPanel
            this.inputsPanel.Dock = DockStyle.Top;
            this.inputsPanel.Location = new Point(10, 10);
            this.inputsPanel.Name = "inputsPanel";
            this.inputsPanel.Size = new Size(880, 300);
            this.inputsPanel.TabIndex = 0;
            this.inputsPanel.Controls.Add(this.grammarFixPanel);
            this.inputsPanel.Controls.Add(this.englishToPersianPanel);
            this.inputsPanel.Controls.Add(this.persianToEnglishPanel);
            
            // persianToEnglishPanel
            this.persianToEnglishPanel.Dock = DockStyle.Top;
            this.persianToEnglishPanel.Location = new Point(0, 0);
            this.persianToEnglishPanel.Name = "persianToEnglishPanel";
            this.persianToEnglishPanel.Padding = new Padding(5);
            this.persianToEnglishPanel.Size = new Size(880, 100);
            this.persianToEnglishPanel.TabIndex = 0;
            this.persianToEnglishPanel.BorderStyle = BorderStyle.FixedSingle;
            this.persianToEnglishPanel.Controls.Add(this.persianToEnglishCountLabel);
            this.persianToEnglishPanel.Controls.Add(this.persianToEnglishTextBox);
            this.persianToEnglishPanel.Controls.Add(this.persianToEnglishLabel);
            
            // persianToEnglishLabel
            this.persianToEnglishLabel.Dock = DockStyle.Top;
            this.persianToEnglishLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.persianToEnglishLabel.Location = new Point(5, 5);
            this.persianToEnglishLabel.Name = "persianToEnglishLabel";
            this.persianToEnglishLabel.Size = new Size(868, 20);
            this.persianToEnglishLabel.TabIndex = 0;
            this.persianToEnglishLabel.Text = "Persian to English";
            
            // persianToEnglishTextBox
            this.persianToEnglishTextBox.Dock = DockStyle.Fill;
            this.persianToEnglishTextBox.Font = new Font("Segoe UI", 10F);
            this.persianToEnglishTextBox.Location = new Point(5, 25);
            this.persianToEnglishTextBox.Multiline = true;
            this.persianToEnglishTextBox.Name = "persianToEnglishTextBox";
            this.persianToEnglishTextBox.ScrollBars = ScrollBars.Vertical;
            this.persianToEnglishTextBox.Size = new Size(868, 48);
            this.persianToEnglishTextBox.TabIndex = 1;
            
            // persianToEnglishCountLabel
            this.persianToEnglishCountLabel.Dock = DockStyle.Bottom;
            this.persianToEnglishCountLabel.Font = new Font("Segoe UI", 8F);
            this.persianToEnglishCountLabel.ForeColor = Color.Gray;
            this.persianToEnglishCountLabel.Location = new Point(5, 73);
            this.persianToEnglishCountLabel.Name = "persianToEnglishCountLabel";
            this.persianToEnglishCountLabel.Size = new Size(868, 20);
            this.persianToEnglishCountLabel.TabIndex = 2;
            this.persianToEnglishCountLabel.Text = "Characters: 0 | Words: 0";
            
            // englishToPersianPanel
            this.englishToPersianPanel.Dock = DockStyle.Top;
            this.englishToPersianPanel.Location = new Point(0, 100);
            this.englishToPersianPanel.Name = "englishToPersianPanel";
            this.englishToPersianPanel.Padding = new Padding(5);
            this.englishToPersianPanel.Size = new Size(880, 100);
            this.englishToPersianPanel.TabIndex = 1;
            this.englishToPersianPanel.BorderStyle = BorderStyle.FixedSingle;
            this.englishToPersianPanel.Controls.Add(this.englishToPersianCountLabel);
            this.englishToPersianPanel.Controls.Add(this.englishToPersianTextBox);
            this.englishToPersianPanel.Controls.Add(this.englishToPersianLabel);
            
            // englishToPersianLabel
            this.englishToPersianLabel.Dock = DockStyle.Top;
            this.englishToPersianLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.englishToPersianLabel.Location = new Point(5, 5);
            this.englishToPersianLabel.Name = "englishToPersianLabel";
            this.englishToPersianLabel.Size = new Size(868, 20);
            this.englishToPersianLabel.TabIndex = 0;
            this.englishToPersianLabel.Text = "English to Persian";
            
            // englishToPersianTextBox
            this.englishToPersianTextBox.Dock = DockStyle.Fill;
            this.englishToPersianTextBox.Font = new Font("Segoe UI", 10F);
            this.englishToPersianTextBox.Location = new Point(5, 25);
            this.englishToPersianTextBox.Multiline = true;
            this.englishToPersianTextBox.Name = "englishToPersianTextBox";
            this.englishToPersianTextBox.ScrollBars = ScrollBars.Vertical;
            this.englishToPersianTextBox.Size = new Size(868, 48);
            this.englishToPersianTextBox.TabIndex = 1;
            
            // englishToPersianCountLabel
            this.englishToPersianCountLabel.Dock = DockStyle.Bottom;
            this.englishToPersianCountLabel.Font = new Font("Segoe UI", 8F);
            this.englishToPersianCountLabel.ForeColor = Color.Gray;
            this.englishToPersianCountLabel.Location = new Point(5, 73);
            this.englishToPersianCountLabel.Name = "englishToPersianCountLabel";
            this.englishToPersianCountLabel.Size = new Size(868, 20);
            this.englishToPersianCountLabel.TabIndex = 2;
            this.englishToPersianCountLabel.Text = "Characters: 0 | Words: 0";
            
            // grammarFixPanel
            this.grammarFixPanel.Dock = DockStyle.Top;
            this.grammarFixPanel.Location = new Point(0, 200);
            this.grammarFixPanel.Name = "grammarFixPanel";
            this.grammarFixPanel.Padding = new Padding(5);
            this.grammarFixPanel.Size = new Size(880, 100);
            this.grammarFixPanel.TabIndex = 2;
            this.grammarFixPanel.BorderStyle = BorderStyle.FixedSingle;
            this.grammarFixPanel.Controls.Add(this.grammarFixCountLabel);
            this.grammarFixPanel.Controls.Add(this.grammarFixTextBox);
            this.grammarFixPanel.Controls.Add(this.grammarFixLabel);
            
            // grammarFixLabel
            this.grammarFixLabel.Dock = DockStyle.Top;
            this.grammarFixLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.grammarFixLabel.Location = new Point(5, 5);
            this.grammarFixLabel.Name = "grammarFixLabel";
            this.grammarFixLabel.Size = new Size(868, 20);
            this.grammarFixLabel.TabIndex = 0;
            this.grammarFixLabel.Text = "Grammar Fix";
            
            // grammarFixTextBox
            this.grammarFixTextBox.Dock = DockStyle.Fill;
            this.grammarFixTextBox.Font = new Font("Segoe UI", 10F);
            this.grammarFixTextBox.Location = new Point(5, 25);
            this.grammarFixTextBox.Multiline = true;
            this.grammarFixTextBox.Name = "grammarFixTextBox";
            this.grammarFixTextBox.ScrollBars = ScrollBars.Vertical;
            this.grammarFixTextBox.Size = new Size(868, 48);
            this.grammarFixTextBox.TabIndex = 1;
            
            // grammarFixCountLabel
            this.grammarFixCountLabel.Dock = DockStyle.Bottom;
            this.grammarFixCountLabel.Font = new Font("Segoe UI", 8F);
            this.grammarFixCountLabel.ForeColor = Color.Gray;
            this.grammarFixCountLabel.Location = new Point(5, 73);
            this.grammarFixCountLabel.Name = "grammarFixCountLabel";
            this.grammarFixCountLabel.Size = new Size(868, 20);
            this.grammarFixCountLabel.TabIndex = 2;
            this.grammarFixCountLabel.Text = "Characters: 0 | Words: 0";
            
            // controlsPanel
            this.controlsPanel.Dock = DockStyle.Top;
            this.controlsPanel.Location = new Point(10, 310);
            this.controlsPanel.Name = "controlsPanel";
            this.controlsPanel.Size = new Size(880, 50);
            this.controlsPanel.TabIndex = 1;
            this.controlsPanel.Controls.Add(this.loadingLabel);
            this.controlsPanel.Controls.Add(this.cancelButton);
            this.controlsPanel.Controls.Add(this.readButton);
            this.controlsPanel.Controls.Add(this.translateButton);
            
            // translateButton
            this.translateButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.translateButton.Location = new Point(5, 10);
            this.translateButton.Name = "translateButton";
            this.translateButton.Size = new Size(120, 35);
            this.translateButton.TabIndex = 0;
            this.translateButton.Text = "Translate";
            this.translateButton.UseVisualStyleBackColor = true;
            
            // readButton
            this.readButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.readButton.Location = new Point(130, 10);
            this.readButton.Name = "readButton";
            this.readButton.Size = new Size(120, 35);
            this.readButton.TabIndex = 1;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            
            // cancelButton
            this.cancelButton.Font = new Font("Segoe UI", 10F);
            this.cancelButton.Location = new Point(255, 10);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(120, 35);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Visible = false;
            
            // loadingLabel
            this.loadingLabel.AutoSize = true;
            this.loadingLabel.Font = new Font("Segoe UI", 10F);
            this.loadingLabel.ForeColor = Color.Blue;
            this.loadingLabel.Location = new Point(380, 18);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new Size(100, 19);
            this.loadingLabel.TabIndex = 3;
            this.loadingLabel.Text = "Processing...";
            this.loadingLabel.Visible = false;
            
            // resultPanel
            this.resultPanel.Dock = DockStyle.Fill;
            this.resultPanel.Location = new Point(10, 360);
            this.resultPanel.Name = "resultPanel";
            this.resultPanel.Padding = new Padding(5);
            this.resultPanel.Size = new Size(880, 200);
            this.resultPanel.TabIndex = 2;
            this.resultPanel.Controls.Add(this.copyResultButton);
            this.resultPanel.Controls.Add(this.resultTabControl);
            this.resultPanel.Controls.Add(this.resultLabel);
            
            // resultLabel
            this.resultLabel.Dock = DockStyle.Top;
            this.resultLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.resultLabel.Location = new Point(5, 5);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new Size(870, 20);
            this.resultLabel.TabIndex = 0;
            this.resultLabel.Text = "Result";
            
            // resultTabControl
            this.resultTabControl.Dock = DockStyle.Fill;
            this.resultTabControl.Location = new Point(5, 25);
            this.resultTabControl.Name = "resultTabControl";
            this.resultTabControl.Size = new Size(870, 135);
            this.resultTabControl.TabIndex = 1;
            
            // copyResultButton
            this.copyResultButton.Dock = DockStyle.Bottom;
            this.copyResultButton.Font = new Font("Segoe UI", 9F);
            this.copyResultButton.Location = new Point(5, 160);
            this.copyResultButton.Name = "copyResultButton";
            this.copyResultButton.Size = new Size(870, 35);
            this.copyResultButton.TabIndex = 2;
            this.copyResultButton.Text = "Copy to Clipboard";
            this.copyResultButton.UseVisualStyleBackColor = true;
            
            // shortcutsPanel
            this.shortcutsPanel.Dock = DockStyle.Bottom;
            this.shortcutsPanel.Location = new Point(10, 617);
            this.shortcutsPanel.Name = "shortcutsPanel";
            this.shortcutsPanel.Size = new Size(880, 25);
            this.shortcutsPanel.TabIndex = 3;
            this.shortcutsPanel.Controls.Add(this.shortcutsListView);
            this.shortcutsPanel.Controls.Add(this.shortcutsLabel);
            this.shortcutsPanel.Controls.Add(this.toggleShortcutsButton);
            
            // shortcutsLabel
            this.shortcutsLabel.Dock = DockStyle.Top;
            this.shortcutsLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.shortcutsLabel.Location = new Point(0, 0);
            this.shortcutsLabel.Name = "shortcutsLabel";
            this.shortcutsLabel.Size = new Size(880, 20);
            this.shortcutsLabel.TabIndex = 0;
            this.shortcutsLabel.Text = "Keyboard Shortcuts";
            this.shortcutsLabel.Visible = false;
            
            // shortcutsListView
            this.shortcutsListView.Dock = DockStyle.Fill;
            this.shortcutsListView.Font = new Font("Segoe UI", 8F);
            this.shortcutsListView.Location = new Point(0, 20);
            this.shortcutsListView.Name = "shortcutsListView";
            this.shortcutsListView.Size = new Size(880, 137);
            this.shortcutsListView.TabIndex = 1;
            this.shortcutsListView.View = View.Details;
            this.shortcutsListView.GridLines = true;
            this.shortcutsListView.FullRowSelect = true;
            this.shortcutsListView.Columns.Add("Shortcut", 200);
            this.shortcutsListView.Columns.Add("Action", 680);
            this.shortcutsListView.Visible = false;
            
            // toggleShortcutsButton
            this.toggleShortcutsButton.Dock = DockStyle.Bottom;
            this.toggleShortcutsButton.Font = new Font("Segoe UI", 8F);
            this.toggleShortcutsButton.Location = new Point(0, 0);
            this.toggleShortcutsButton.Name = "toggleShortcutsButton";
            this.toggleShortcutsButton.Size = new Size(880, 25);
            this.toggleShortcutsButton.TabIndex = 2;
            this.toggleShortcutsButton.Text = "Show Shortcuts";
            this.toggleShortcutsButton.UseVisualStyleBackColor = true;
            
            // statusStrip
            this.statusStrip.Items.AddRange(new ToolStripItem[] {
                this.statusLabel
            });
            this.statusStrip.Location = new Point(0, 678);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new Size(900, 22);
            this.statusStrip.TabIndex = 2;
            
            // statusLabel
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(39, 17);
            this.statusLabel.Text = "Ready";
            
            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 700);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new Size(600, 400);
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "AI Translator";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            this.inputsPanel.ResumeLayout(false);
            this.persianToEnglishPanel.ResumeLayout(false);
            this.persianToEnglishPanel.PerformLayout();
            this.englishToPersianPanel.ResumeLayout(false);
            this.englishToPersianPanel.PerformLayout();
            this.grammarFixPanel.ResumeLayout(false);
            this.grammarFixPanel.PerformLayout();
            this.controlsPanel.ResumeLayout(false);
            this.controlsPanel.PerformLayout();
            this.resultPanel.ResumeLayout(false);
            this.shortcutsPanel.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem copyResultToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem shortcutsToolStripMenuItem;
        
        private Panel mainPanel;
        private Panel inputsPanel;
        private Panel persianToEnglishPanel;
        private Label persianToEnglishLabel;
        private TextBox persianToEnglishTextBox;
        private Label persianToEnglishCountLabel;
        private Panel englishToPersianPanel;
        private Label englishToPersianLabel;
        private TextBox englishToPersianTextBox;
        private Label englishToPersianCountLabel;
        private Panel grammarFixPanel;
        private Label grammarFixLabel;
        private TextBox grammarFixTextBox;
        private Label grammarFixCountLabel;
        
        private Panel controlsPanel;
        private Button translateButton;
        private Button readButton;
        private Button cancelButton;
        private Label loadingLabel;
        
        private Panel resultPanel;
        private Label resultLabel;
        private TabControl resultTabControl;
        private Button copyResultButton;
        
        private Panel shortcutsPanel;
        private Label shortcutsLabel;
        private ListView shortcutsListView;
        private Button toggleShortcutsButton;
        
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}

