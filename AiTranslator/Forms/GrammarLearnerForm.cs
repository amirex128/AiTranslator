using AiTranslator.Models;
using AiTranslator.Services;
using System.Windows.Forms;

namespace AiTranslator.Forms;

public class GrammarLearnerForm : Form
{
    private readonly GrammarLearnerResponse _response;
    private readonly IConfigService _configService;
    private TabControl _mainTabControl = null!;
    private float _fontSize;

    public GrammarLearnerForm(GrammarLearnerResponse response, IConfigService? configService = null)
    {
        _response = response;
        _configService = configService ?? new ConfigService();
        _fontSize = _configService.Config.Ui.GrammarLearnerFontSize;
        InitializeComponents();
        PopulateData();
    }

    private void InitializeComponents()
    {
        this.Text = "Grammar Learner - Learning Results";
        this.Size = new Size(1000, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(800, 600);
        this.MaximizeBox = true;
        this.BackColor = Color.FromArgb(245, 245, 250);

        _mainTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", _fontSize),
            Appearance = TabAppearance.FlatButtons,
            Padding = new Point(20, 5)
        };

        // Create tabs
        _mainTabControl.TabPages.Add(CreateOverviewTab());
        _mainTabControl.TabPages.Add(CreateGrammarTab());
        _mainTabControl.TabPages.Add(CreateIdiomsTab());
        _mainTabControl.TabPages.Add(CreateStructureTab());

        this.Controls.Add(_mainTabControl);
    }

    private TabPage CreateOverviewTab()
    {
        var tab = new TabPage("Overview");
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true, BackColor = Color.FromArgb(250, 250, 255) };

        int y = 15;

        // Original Text GroupBox
        var originalGroupBox = CreateGroupBox("Original Text", _response.OriginalText, y, Color.FromArgb(240, 240, 250), Color.FromArgb(255, 255, 255), parentPanel: mainPanel);
        mainPanel.Controls.Add(originalGroupBox);
        y += originalGroupBox.Height + 15;

        // Corrected Text GroupBox
        var correctedGroupBox = CreateGroupBox("Corrected Text", _response.CorrectedText, y, Color.FromArgb(230, 255, 240), Color.FromArgb(255, 255, 255), parentPanel: mainPanel);
        mainPanel.Controls.Add(correctedGroupBox);
        y += correctedGroupBox.Height + 15;

        // Translation GroupBox
        var translationGroupBox = CreateGroupBox("Translation (ترجمه فارسی)", _response.FullTranslationFa, y, Color.FromArgb(230, 240, 255), Color.FromArgb(255, 255, 255), isPersian: true, parentPanel: mainPanel);
        mainPanel.Controls.Add(translationGroupBox);
        y += translationGroupBox.Height + 15;

        // Learning Tips GroupBox
        var tipsGroupBox = CreateGroupBox("Learning Tips (نکات یادگیری)", _response.LearningTipsFa, y, Color.FromArgb(255, 250, 240), Color.FromArgb(255, 255, 255), isPersian: true, parentPanel: mainPanel);
        mainPanel.Controls.Add(tipsGroupBox);

        tab.Controls.Add(mainPanel);
        return tab;
    }

    private int CalculateTextHeight(string text, Font font, int availableWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (int)(font.Height * 2);

        using (var g = this.CreateGraphics())
        {
            var size = TextRenderer.MeasureText(g, text, font, new Size(availableWidth, int.MaxValue), 
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            return Math.Max((int)(font.Height * 1.5), size.Height + 20);
        }
    }

    private GroupBox CreateGroupBox(string title, string content, int y, Color borderColor, Color backgroundColor, bool isPersian = false, Panel? parentPanel = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            content = "(No content available)";
        }

        var titleFont = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold);
        var contentFont = new Font("Segoe UI", _fontSize);
        
        // Calculate available width based on parent panel or form width
        // Use form's client width minus padding (20px on each side = 40px total)
        var formClientWidth = parentPanel != null ? parentPanel.ClientSize.Width : this.ClientSize.Width;
        var groupBoxWidth = formClientWidth - 40; // 20px padding on each side
        var availableWidth = groupBoxWidth - 50; // Account for GroupBox internal padding and margins
        var contentHeight = CalculateTextHeight(content, contentFont, availableWidth);
        var minHeight = (int)(contentFont.Height * 2) + 50;
        var maxHeight = 400;
        var estimatedHeight = Math.Max(minHeight, Math.Min(maxHeight, contentHeight + 50));

        var groupBox = new GroupBox
        {
            Text = title,
            Location = new Point(10, y),
            Size = new Size(groupBoxWidth, estimatedHeight), // Responsive width
            Font = titleFont,
            ForeColor = Color.FromArgb(0, 0, 120),
            Padding = new Padding(5, 20, 5, 5), // Extra top padding for Persian text in header
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var contentTextBox = new TextBox
        {
            Text = content,
            Location = new Point(10, 30), // Increased from 25 to account for header padding
            Size = new Size(groupBox.Width - 30, estimatedHeight - 45), // Adjusted for padding
            Font = contentFont,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = backgroundColor,
            RightToLeft = isPersian ? RightToLeft.Yes : RightToLeft.No,
            WordWrap = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        groupBox.Controls.Add(contentTextBox);
        return groupBox;
    }

    private TabPage CreateGrammarTab()
    {
        var tab = new TabPage("Grammar Teaching");
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true, BackColor = Color.FromArgb(250, 250, 255) };

        int y = 15;
        var grammar = _response.GrammarTeaching;

        // Overview Section
        var overviewGroupBox = new GroupBox
        {
            Text = "Overview",
            Location = new Point(10, y),
            Size = new Size(mainPanel.ClientSize.Width - 40, 200), // Responsive width
            Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 0, 120),
            Padding = new Padding(5, 20, 5, 5), // Extra top padding for header
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var overviewEnTextBox = new TextBox
        {
            Text = grammar.OverviewEn ?? "(No English overview)",
            Location = new Point(10, 30), // Adjusted for padding
            Size = new Size(overviewGroupBox.Width - 30, 70),
            Font = new Font("Segoe UI", _fontSize),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(250, 250, 255),
            WordWrap = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        overviewGroupBox.Controls.Add(overviewEnTextBox);

        var overviewFaTextBox = new TextBox
        {
            Text = grammar.OverviewFa ?? "(No Persian overview)",
            Location = new Point(10, 105), // Adjusted for padding
            Size = new Size(overviewGroupBox.Width - 30, 70),
            Font = new Font("Segoe UI", _fontSize),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(250, 250, 255),
            RightToLeft = RightToLeft.Yes,
            WordWrap = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        overviewGroupBox.Controls.Add(overviewFaTextBox);

        mainPanel.Controls.Add(overviewGroupBox);
        y += overviewGroupBox.Height + 15;

        // Tense and Structure Section
        var tenseStructureGroupBox = new GroupBox
        {
            Text = "Tense & Structure",
            Location = new Point(10, y),
            Size = new Size(mainPanel.ClientSize.Width - 40, 180), // Responsive width
            Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 0, 120),
            Padding = new Padding(5, 20, 5, 5), // Extra top padding for header
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        int innerY = 30; // Adjusted for padding

        // Tense Info - Separate English and Persian
        if (!string.IsNullOrWhiteSpace(grammar.SentenceTenseEn) || !string.IsNullOrWhiteSpace(grammar.SentenceTenseFa))
        {
            // English Tense
            if (!string.IsNullOrWhiteSpace(grammar.SentenceTenseEn))
            {
                var tenseEnText = $"Tense: {grammar.SentenceTenseEn}";
                var tenseFont = new Font("Segoe UI", _fontSize, FontStyle.Bold);
                var tenseHeight = CalculateTextHeight(tenseEnText, tenseFont, tenseStructureGroupBox.Width - 30);
                
                var tenseEnLabel = new Label
                {
                    Text = tenseEnText,
                    Location = new Point(10, innerY),
                    Size = new Size(tenseStructureGroupBox.Width - 30, tenseHeight),
                    Font = tenseFont,
                    ForeColor = Color.FromArgb(0, 100, 200),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                tenseStructureGroupBox.Controls.Add(tenseEnLabel);
                innerY += tenseHeight + 10;
            }
            
            // Persian Tense - Separate TextBox with RTL
            if (!string.IsNullOrWhiteSpace(grammar.SentenceTenseFa))
            {
                var tenseFaText = $"زمان: {grammar.SentenceTenseFa}";
                var tenseFaFont = new Font("Segoe UI", _fontSize, FontStyle.Bold);
                var tenseFaHeight = CalculateTextHeight(tenseFaText, tenseFaFont, tenseStructureGroupBox.Width - 30);
                var tenseFaTextBoxHeight = Math.Max(30, Math.Min(80, tenseFaHeight));
                
                var tenseFaTextBox = new TextBox
                {
                    Text = tenseFaText,
                    Location = new Point(10, innerY),
                    Size = new Size(tenseStructureGroupBox.Width - 30, tenseFaTextBoxHeight),
                    Font = tenseFaFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(245, 250, 255),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                tenseStructureGroupBox.Controls.Add(tenseFaTextBox);
                innerY += tenseFaTextBoxHeight + 10;
            }

            if (!string.IsNullOrWhiteSpace(grammar.TenseExplanationFa))
            {
                var explanationFont = new Font("Segoe UI", _fontSize - 1);
                var explanationHeight = CalculateTextHeight(grammar.TenseExplanationFa, explanationFont, tenseStructureGroupBox.Width - 30);
                var explanationTextBoxHeight = Math.Max(50, Math.Min(150, explanationHeight));
                
                var tenseExplanationTextBox = new TextBox
                {
                    Text = grammar.TenseExplanationFa,
                    Location = new Point(10, innerY),
                    Size = new Size(tenseStructureGroupBox.Width - 30, explanationTextBoxHeight),
                    Font = explanationFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(245, 250, 255),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                tenseStructureGroupBox.Controls.Add(tenseExplanationTextBox);
                innerY += explanationTextBoxHeight + 10;
            }
        }

        // Structure Pattern
        if (!string.IsNullOrWhiteSpace(grammar.StructurePatternEn) || !string.IsNullOrWhiteSpace(grammar.StructurePatternFa))
        {
            // Separate English and Persian for better display
            var patternEn = grammar.StructurePatternEn ?? "";
            var patternFa = grammar.StructurePatternFa ?? "";
            
            if (!string.IsNullOrWhiteSpace(patternEn))
            {
                var patternFont = new Font("Segoe UI", _fontSize - 1, FontStyle.Bold);
                var patternHeight = CalculateTextHeight(patternEn, patternFont, tenseStructureGroupBox.Width - 30);
                
                var patternLabel = new Label
                {
                    Text = patternEn,
                    Location = new Point(10, innerY),
                    Size = new Size(tenseStructureGroupBox.Width - 30, patternHeight),
                    Font = patternFont,
                    ForeColor = Color.FromArgb(0, 0, 150),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                tenseStructureGroupBox.Controls.Add(patternLabel);
                innerY += patternHeight + 10;
            }

            if (!string.IsNullOrWhiteSpace(patternFa))
            {
                var patternFaFont = new Font("Segoe UI", _fontSize - 1);
                var patternFaHeight = CalculateTextHeight(patternFa, patternFaFont, tenseStructureGroupBox.Width - 30);
                var patternFaTextBoxHeight = Math.Max(50, Math.Min(150, patternFaHeight));
                
                var patternFaTextBox = new TextBox
                {
                    Text = patternFa,
                    Location = new Point(10, innerY),
                    Size = new Size(tenseStructureGroupBox.Width - 30, patternFaTextBoxHeight),
                    Font = patternFaFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(255, 250, 240),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                tenseStructureGroupBox.Controls.Add(patternFaTextBox);
                innerY += patternFaTextBoxHeight + 10;
            }
        }
        
        // Update GroupBox height based on actual content
        tenseStructureGroupBox.Height = innerY + 15;

        mainPanel.Controls.Add(tenseStructureGroupBox);
        y += tenseStructureGroupBox.Height + 15;

        // Difficulty Level
        if (!string.IsNullOrWhiteSpace(grammar.DifficultyLevel))
        {
            var levelGroupBox = new GroupBox
            {
                Text = "Difficulty Level",
                Location = new Point(10, y),
                Size = new Size(mainPanel.ClientSize.Width - 40, 60), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var levelLabel = new Label
            {
                Text = grammar.DifficultyLevel,
                Location = new Point(10, 25),
                Size = new Size(levelGroupBox.Width - 30, 25),
                Font = new Font("Segoe UI", _fontSize + 2, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 0),
                BackColor = GetDifficultyColor(grammar.DifficultyLevel),
                TextAlign = ContentAlignment.MiddleCenter
            };
            levelGroupBox.Controls.Add(levelLabel);
            mainPanel.Controls.Add(levelGroupBox);
            y += levelGroupBox.Height + 15;
        }

        // Similar Examples GroupBox
        if (grammar.SimilarExamples != null && grammar.SimilarExamples.Count > 0)
        {
            var examplesGroupBox = new GroupBox
            {
                Text = $"Similar Examples (مثال‌های مشابه) - {grammar.SimilarExamples.Count}",
                Location = new Point(10, y),
                Size = new Size(mainPanel.ClientSize.Width - 40, Math.Min(300, 80 + (grammar.SimilarExamples.Count * 80))), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int exampleY = 30;
            foreach (var example in grammar.SimilarExamples)
            {
                var exampleEnText = example.ExampleEn ?? "(no example)";
                var exampleFaText = example.ExampleFa ?? "(no translation)";
                
                var enFont = new Font("Segoe UI", _fontSize, FontStyle.Bold);
                var faFont = new Font("Segoe UI", _fontSize - 1, FontStyle.Italic);
                var panelWidth = examplesGroupBox.Width - 30;
                
                var enHeight = CalculateTextHeight(exampleEnText, enFont, panelWidth - 20);
                var faHeight = CalculateTextHeight(exampleFaText, faFont, panelWidth - 20);
                var panelHeight = Math.Max(60, enHeight + faHeight + 20);
                
                var examplePanel = new Panel
                {
                    Location = new Point(10, exampleY),
                    Size = new Size(panelWidth, panelHeight),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(250, 250, 255),
                    Padding = new Padding(10),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                // English Example - Label
                var enLabel = new Label
                {
                    Text = exampleEnText,
                    Location = new Point(5, 5),
                    Size = new Size(examplePanel.Width - 20, enHeight),
                    Font = enFont,
                    ForeColor = Color.FromArgb(0, 0, 150),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                examplePanel.Controls.Add(enLabel);

                // Persian Example - Separate TextBox with RTL
                var faTextBox = new TextBox
                {
                    Text = exampleFaText,
                    Location = new Point(5, enHeight + 10),
                    Size = new Size(examplePanel.Width - 20, faHeight),
                    Font = faFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(240, 255, 240),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                examplePanel.Controls.Add(faTextBox);

                examplesGroupBox.Controls.Add(examplePanel);
                exampleY += panelHeight + 10;
            }
            
            // Update GroupBox height
            examplesGroupBox.Height = exampleY + 10;

            mainPanel.Controls.Add(examplesGroupBox);
            y += examplesGroupBox.Height + 15;
        }

        // Key Points GroupBox
        if (grammar.KeyPoints != null && grammar.KeyPoints.Count > 0)
        {
            var keyPointsGroupBox = new GroupBox
            {
                Text = $"Key Points (نکات کلیدی) - {grammar.KeyPoints.Count}",
                Location = new Point(10, y),
                Size = new Size(mainPanel.ClientSize.Width - 40, Math.Min(400, 80 + (grammar.KeyPoints.Count * 90))), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int pointY = 30;
            foreach (var point in grammar.KeyPoints)
            {
                var titleText = point.TitleEn ?? "(no title)";
                var explanationText = point.ExplanationFa ?? "(no explanation)";
                
                var titleFont = new Font("Segoe UI", _fontSize, FontStyle.Bold);
                var explanationFont = new Font("Segoe UI", _fontSize - 1);
                var panelWidth = keyPointsGroupBox.Width - 30;
                
                var titleHeight = CalculateTextHeight(titleText, titleFont, panelWidth - 20);
                var explanationHeight = CalculateTextHeight(explanationText, explanationFont, panelWidth - 20);
                var explanationTextBoxHeight = Math.Max(40, Math.Min(150, explanationHeight));
                var panelHeight = titleHeight + explanationTextBoxHeight + 20;
                
                var pointPanel = new Panel
                {
                    Location = new Point(10, pointY),
                    Size = new Size(panelWidth, panelHeight),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(255, 255, 240),
                    Padding = new Padding(10),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var titleLabel = new Label
                {
                    Text = titleText,
                    Location = new Point(5, 5),
                    Size = new Size(pointPanel.Width - 20, titleHeight),
                    Font = titleFont,
                    ForeColor = Color.FromArgb(150, 0, 0),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                pointPanel.Controls.Add(titleLabel);

                var explanationTextBox = new TextBox
                {
                    Text = explanationText,
                    Location = new Point(5, titleHeight + 10),
                    Size = new Size(pointPanel.Width - 20, explanationTextBoxHeight),
                    Font = explanationFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(255, 255, 240),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                pointPanel.Controls.Add(explanationTextBox);

                keyPointsGroupBox.Controls.Add(pointPanel);
                pointY += panelHeight + 10;
            }
            
            // Update GroupBox height
            keyPointsGroupBox.Height = pointY + 10;

            mainPanel.Controls.Add(keyPointsGroupBox);
            y += keyPointsGroupBox.Height + 15;
        }

        // Common Mistakes GroupBox
        if (grammar.CommonMistakes != null && grammar.CommonMistakes.Count > 0)
        {
            var mistakesGroupBox = new GroupBox
            {
                Text = $"Common Mistakes (اشتباهات رایج) - {grammar.CommonMistakes.Count}",
                Location = new Point(10, y),
                Size = new Size(mainPanel.ClientSize.Width - 40, Math.Min(450, 80 + (grammar.CommonMistakes.Count * 100))), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int mistakeY = 30;
            foreach (var mistake in grammar.CommonMistakes)
            {
                var originalText = $"✗ Original: {mistake.OriginalSegmentEn ?? "(no original)"}";
                var correctedText = $"✓ Corrected: {mistake.CorrectedSegmentEn ?? "(no corrected)"}";
                var explanationText = mistake.ExplanationFa ?? "(no explanation)";
                
                var mistakeFont = new Font("Segoe UI", _fontSize - 1);
                var explanationFont = new Font("Segoe UI", _fontSize - 1);
                var panelWidth = mistakesGroupBox.Width - 30;
                
                var originalHeight = CalculateTextHeight(originalText, mistakeFont, panelWidth - 20);
                var correctedHeight = CalculateTextHeight(correctedText, mistakeFont, panelWidth - 20);
                var explanationHeight = CalculateTextHeight(explanationText, explanationFont, panelWidth - 20);
                var explanationTextBoxHeight = Math.Max(30, Math.Min(100, explanationHeight));
                var panelHeight = originalHeight + correctedHeight + explanationTextBoxHeight + 20;
                
                var mistakePanel = new Panel
                {
                    Location = new Point(10, mistakeY),
                    Size = new Size(panelWidth, panelHeight),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(255, 245, 245),
                    Padding = new Padding(10),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var originalLabel = new Label
                {
                    Text = originalText,
                    Location = new Point(5, 5),
                    Size = new Size(mistakePanel.Width - 20, originalHeight),
                    Font = mistakeFont,
                    ForeColor = Color.FromArgb(200, 0, 0),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                mistakePanel.Controls.Add(originalLabel);

                var correctedLabel = new Label
                {
                    Text = correctedText,
                    Location = new Point(5, originalHeight + 10),
                    Size = new Size(mistakePanel.Width - 20, correctedHeight),
                    Font = new Font("Segoe UI", _fontSize - 1, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 150, 0),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                mistakePanel.Controls.Add(correctedLabel);

                var explanationTextBox = new TextBox
                {
                    Text = explanationText,
                    Location = new Point(5, originalHeight + correctedHeight + 15),
                    Size = new Size(mistakePanel.Width - 20, explanationTextBoxHeight),
                    Font = explanationFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(255, 245, 245),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                mistakePanel.Controls.Add(explanationTextBox);

                mistakesGroupBox.Controls.Add(mistakePanel);
                mistakeY += panelHeight + 10;
            }
            
            // Update GroupBox height
            mistakesGroupBox.Height = mistakeY + 10;

            mainPanel.Controls.Add(mistakesGroupBox);
        }

        tab.Controls.Add(mainPanel);
        return tab;
    }

    private TabPage CreateIdiomsTab()
    {
        var tab = new TabPage("Idioms & Phrases");
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true, BackColor = Color.FromArgb(250, 250, 255) };

        if (_response.IdiomPhrases == null || _response.IdiomPhrases.Count == 0)
        {
            var noDataGroupBox = new GroupBox
            {
                Text = "Idioms & Phrases",
                Location = new Point(10, 15),
                Size = new Size(mainPanel.ClientSize.Width - 40, 100), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var noDataLabel = new Label
            {
                Text = "No idioms or phrases found in this text.",
                Location = new Point(10, 25),
                Size = new Size(noDataGroupBox.Width - 30, 50),
                Font = new Font("Segoe UI", _fontSize, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            noDataGroupBox.Controls.Add(noDataLabel);
            mainPanel.Controls.Add(noDataGroupBox);
        }
        else
        {
            int y = 15;
            foreach (var idiom in _response.IdiomPhrases)
            {
                var idiomGroupBox = new GroupBox
                {
                    Text = $"Idiom/Phrase #{_response.IdiomPhrases.IndexOf(idiom) + 1} - Type: {idiom.Type ?? "unknown"}",
                    Location = new Point(10, y),
                    Size = new Size(mainPanel.ClientSize.Width - 40, 240), // Responsive width
                    Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 0, 120),
                    BackColor = GetIdiomTypeColor(idiom.Type ?? "idiom"),
                    Padding = new Padding(5, 20, 5, 5),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                int innerY = 30;

                // Phrase (English)
                var phraseLabel = new Label
                {
                    Text = idiom.PhraseEn ?? "(no phrase)",
                    Location = new Point(10, innerY),
                    Size = new Size(idiomGroupBox.Width - 30, 30),
                    Font = new Font("Segoe UI", _fontSize + 2, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 0, 150),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                idiomGroupBox.Controls.Add(phraseLabel);
                innerY += 35;

                // Meaning - Separate Persian TextBox with RTL
                if (!string.IsNullOrWhiteSpace(idiom.MeaningFa))
                {
                    var meaningLabel = new Label
                    {
                        Text = "Meaning (معنی):",
                        Location = new Point(10, innerY),
                        Size = new Size(idiomGroupBox.Width - 30, 25),
                        Font = new Font("Segoe UI", _fontSize, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 120, 0),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    idiomGroupBox.Controls.Add(meaningLabel);
                    innerY += 30;
                    
                    var meaningFont = new Font("Segoe UI", _fontSize);
                    var meaningHeight = CalculateTextHeight(idiom.MeaningFa, meaningFont, idiomGroupBox.Width - 30);
                    var meaningTextBoxHeight = Math.Max(30, Math.Min(80, meaningHeight));
                    
                    var meaningTextBox = new TextBox
                    {
                        Text = idiom.MeaningFa,
                        Location = new Point(10, innerY),
                        Size = new Size(idiomGroupBox.Width - 30, meaningTextBoxHeight),
                        Font = meaningFont,
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.FromArgb(240, 255, 240),
                        RightToLeft = RightToLeft.Yes,
                        WordWrap = true,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    idiomGroupBox.Controls.Add(meaningTextBox);
                    innerY += meaningTextBoxHeight + 10;
                }

                // Explanation
                var explanationText = idiom.ExplanationFa ?? "(no explanation)";
                var explanationFont = new Font("Segoe UI", _fontSize - 1);
                var explanationHeight = CalculateTextHeight(explanationText, explanationFont, idiomGroupBox.Width - 30);
                var explanationTextBoxHeight = Math.Max(60, Math.Min(150, explanationHeight));
                
                var explanationTextBox = new TextBox
                {
                    Text = explanationText,
                    Location = new Point(10, innerY),
                    Size = new Size(idiomGroupBox.Width - 30, explanationTextBoxHeight),
                    Font = explanationFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(255, 255, 255),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                idiomGroupBox.Controls.Add(explanationTextBox);
                innerY += explanationTextBoxHeight + 10;

                // Example - Separate English and Persian
                if (!string.IsNullOrWhiteSpace(idiom.ExampleEn))
                {
                    var exampleEnLabel = new Label
                    {
                        Text = "Example:",
                        Location = new Point(10, innerY),
                        Size = new Size(idiomGroupBox.Width - 30, 25),
                        Font = new Font("Segoe UI", _fontSize - 1, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    idiomGroupBox.Controls.Add(exampleEnLabel);
                    innerY += 30;
                    
                    var exampleEnFont = new Font("Segoe UI", _fontSize - 1, FontStyle.Italic);
                    var exampleEnHeight = CalculateTextHeight(idiom.ExampleEn, exampleEnFont, idiomGroupBox.Width - 30);
                    var exampleEnTextBoxHeight = Math.Max(30, Math.Min(60, exampleEnHeight));
                    
                    var exampleEnTextBox = new TextBox
                    {
                        Text = idiom.ExampleEn,
                        Location = new Point(10, innerY),
                        Size = new Size(idiomGroupBox.Width - 30, exampleEnTextBoxHeight),
                        Font = exampleEnFont,
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.FromArgb(250, 250, 255),
                        WordWrap = true,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    idiomGroupBox.Controls.Add(exampleEnTextBox);
                    innerY += exampleEnTextBoxHeight + 10;
                }

                if (!string.IsNullOrWhiteSpace(idiom.ExampleFa))
                {
                    var exampleFaLabel = new Label
                    {
                        Text = "مثال:",
                        Location = new Point(10, innerY),
                        Size = new Size(idiomGroupBox.Width - 30, 25),
                        Font = new Font("Segoe UI", _fontSize - 1, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        RightToLeft = RightToLeft.Yes,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    idiomGroupBox.Controls.Add(exampleFaLabel);
                    innerY += 30;
                    
                    var exampleFaFont = new Font("Segoe UI", _fontSize - 1, FontStyle.Italic);
                    var exampleFaHeight = CalculateTextHeight(idiom.ExampleFa, exampleFaFont, idiomGroupBox.Width - 30);
                    var exampleFaTextBoxHeight = Math.Max(30, Math.Min(60, exampleFaHeight));
                    
                    var exampleFaTextBox = new TextBox
                    {
                        Text = idiom.ExampleFa,
                        Location = new Point(10, innerY),
                        Size = new Size(idiomGroupBox.Width - 30, exampleFaTextBoxHeight),
                        Font = exampleFaFont,
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.FromArgb(240, 255, 240),
                        RightToLeft = RightToLeft.Yes,
                        WordWrap = true,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    idiomGroupBox.Controls.Add(exampleFaTextBox);
                    innerY += exampleFaTextBoxHeight + 10;
                }
                
                // Update GroupBox height
                idiomGroupBox.Height = innerY + 15;

                mainPanel.Controls.Add(idiomGroupBox);
                y += idiomGroupBox.Height + 15;
            }
        }

        tab.Controls.Add(mainPanel);
        return tab;
    }

    private TabPage CreateStructureTab()
    {
        var tab = new TabPage("Sentence Structure");
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true, BackColor = Color.FromArgb(250, 250, 255) };

        if (_response.SentenceStructure == null || _response.SentenceStructure.Count == 0)
        {
            var noDataGroupBox = new GroupBox
            {
                Text = "Sentence Structure",
                Location = new Point(10, 15),
                Size = new Size(mainPanel.ClientSize.Width - 40, 100), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var noDataLabel = new Label
            {
                Text = "No sentence structure data available.",
                Location = new Point(10, 25),
                Size = new Size(noDataGroupBox.Width - 30, 50),
                Font = new Font("Segoe UI", _fontSize, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            noDataGroupBox.Controls.Add(noDataLabel);
            mainPanel.Controls.Add(noDataGroupBox);
            tab.Controls.Add(mainPanel);
            return tab;
        }

        int y = 15;
        foreach (var sentence in _response.SentenceStructure)
        {
            var sentenceGroupBox = new GroupBox
            {
                Text = $"Sentence {sentence.SentenceIndex + 1}",
                Location = new Point(10, y),
                Size = new Size(mainPanel.ClientSize.Width - 40, 400), // Responsive width
                Font = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 120),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int innerY = 30;

            // Sentence Text
            var sentenceText = sentence.SentenceText ?? "(no sentence text)";
            var sentenceFont = new Font("Segoe UI", _fontSize + 1, FontStyle.Bold);
            var sentenceHeight = CalculateTextHeight(sentenceText, sentenceFont, sentenceGroupBox.Width - 30);
            
            var sentenceTextLabel = new Label
            {
                Text = sentenceText,
                Location = new Point(10, innerY),
                Size = new Size(sentenceGroupBox.Width - 30, sentenceHeight),
                Font = sentenceFont,
                ForeColor = Color.FromArgb(0, 0, 150),
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            sentenceGroupBox.Controls.Add(sentenceTextLabel);
            innerY += sentenceHeight + 10;

            // Translation - Separate Persian TextBox with RTL
            if (!string.IsNullOrWhiteSpace(sentence.SentenceTranslationFa))
            {
                var translationLabel = new Label
                {
                    Text = "Translation (ترجمه):",
                    Location = new Point(10, innerY),
                    Size = new Size(sentenceGroupBox.Width - 30, 25),
                    Font = new Font("Segoe UI", _fontSize, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 120, 0),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                sentenceGroupBox.Controls.Add(translationLabel);
                innerY += 30;
                
                var translationFont = new Font("Segoe UI", _fontSize, FontStyle.Italic);
                var translationHeight = CalculateTextHeight(sentence.SentenceTranslationFa, translationFont, sentenceGroupBox.Width - 30);
                var translationTextBoxHeight = Math.Max(30, Math.Min(80, translationHeight));
                
                var translationTextBox = new TextBox
                {
                    Text = sentence.SentenceTranslationFa,
                    Location = new Point(10, innerY),
                    Size = new Size(sentenceGroupBox.Width - 30, translationTextBoxHeight),
                    Font = translationFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(240, 255, 240),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                sentenceGroupBox.Controls.Add(translationTextBox);
                innerY += translationTextBoxHeight + 10;
            }

            // Pattern - Separate Persian TextBox with RTL
            if (!string.IsNullOrWhiteSpace(sentence.PatternFa))
            {
                var patternLabel = new Label
                {
                    Text = "Pattern (الگو):",
                    Location = new Point(10, innerY),
                    Size = new Size(sentenceGroupBox.Width - 30, 25),
                    Font = new Font("Segoe UI", _fontSize - 1, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                sentenceGroupBox.Controls.Add(patternLabel);
                innerY += 30;
                
                var patternFont = new Font("Segoe UI", _fontSize - 1);
                var patternHeight = CalculateTextHeight(sentence.PatternFa, patternFont, sentenceGroupBox.Width - 30);
                var patternTextBoxHeight = Math.Max(30, Math.Min(80, patternHeight));
                
                var patternTextBox = new TextBox
                {
                    Text = sentence.PatternFa,
                    Location = new Point(10, innerY),
                    Size = new Size(sentenceGroupBox.Width - 30, patternTextBoxHeight),
                    Font = patternFont,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(250, 250, 255),
                    RightToLeft = RightToLeft.Yes,
                    WordWrap = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                sentenceGroupBox.Controls.Add(patternTextBox);
                innerY += patternTextBoxHeight + 15;
            }
            else
            {
                innerY += 10;
            }

            // Tokens GroupBox with DataGridView Table
            var tokensGroupBox = new GroupBox
            {
                Text = $"Tokens ({sentence.Tokens?.Count ?? 0})",
                Location = new Point(10, innerY),
                Size = new Size(sentenceGroupBox.Width - 30, 300),
                Font = new Font("Segoe UI", _fontSize, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                Padding = new Padding(5, 20, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            if (sentence.Tokens != null && sentence.Tokens.Count > 0)
            {
                var tokensDataGridView = new DataGridView
                {
                    Location = new Point(10, 30),
                    Size = new Size(tokensGroupBox.Width - 30, 260),
                    Font = new Font("Segoe UI", _fontSize - 1),
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    RowHeadersVisible = false,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    GridColor = Color.FromArgb(200, 200, 200),
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Padding = new Padding(5),
                        WrapMode = DataGridViewTriState.True
                    },
                    AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.FromArgb(250, 250, 255)
                    },
                    ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font = new Font("Segoe UI", _fontSize, FontStyle.Bold),
                        BackColor = Color.FromArgb(240, 240, 240),
                        ForeColor = Color.FromArgb(0, 0, 120),
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    },
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                };

                // Add columns
                var tokenColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Token",
                    HeaderText = "Token",
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font = new Font("Segoe UI", _fontSize - 1, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 0, 150),
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                tokensDataGridView.Columns.Add(tokenColumn);

                var posColumn = new DataGridViewTextBoxColumn
                {
                    Name = "PartOfSpeech",
                    HeaderText = "Part of Speech",
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font = new Font("Segoe UI", _fontSize - 2, FontStyle.Italic),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    }
                };
                tokensDataGridView.Columns.Add(posColumn);

                var meaningColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Meaning",
                    HeaderText = "Meaning (معنی)",
                    Width = 200,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font = new Font("Segoe UI", _fontSize - 1),
                        ForeColor = Color.FromArgb(0, 100, 0),
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        BackColor = Color.FromArgb(240, 255, 240)
                    }
                };
                tokensDataGridView.Columns.Add(meaningColumn);

                var roleColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Role",
                    HeaderText = "Role (نقش)",
                    Width = 200,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font = new Font("Segoe UI", _fontSize - 2, FontStyle.Italic),
                        ForeColor = Color.FromArgb(80, 80, 80),
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        BackColor = Color.FromArgb(250, 250, 255)
                    }
                };
                tokensDataGridView.Columns.Add(roleColumn);

                // Add rows
                foreach (var token in sentence.Tokens)
                {
                    var tokenText = !string.IsNullOrWhiteSpace(token.TokenText) ? token.TokenText : "(empty)";
                    var partOfSpeech = !string.IsNullOrWhiteSpace(token.PartOfSpeech) ? token.PartOfSpeech : "unknown";
                    var meaningFa = !string.IsNullOrWhiteSpace(token.MeaningFa) ? token.MeaningFa : "(no meaning)";
                    var roleExplanation = !string.IsNullOrWhiteSpace(token.RoleExplanationFa) ? token.RoleExplanationFa : "(no role)";

                    var rowIndex = tokensDataGridView.Rows.Add(
                        tokenText,
                        partOfSpeech,
                        meaningFa,
                        roleExplanation
                    );

                    // RTL is already set via DefaultCellStyle for Meaning and Role columns
                    // Alignment is set to MiddleRight which provides RTL-like appearance
                }

                // Adjust column widths
                tokenColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                posColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                meaningColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                roleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                tokensGroupBox.Controls.Add(tokensDataGridView);
                
                // Update GroupBox height based on content
                var estimatedHeight = Math.Min(400, 100 + (sentence.Tokens.Count * 35));
                tokensGroupBox.Height = estimatedHeight;
                tokensDataGridView.Height = estimatedHeight - 40;
            }
            else
            {
                var noTokensLabel = new Label
                {
                    Text = "(No tokens available)",
                    Location = new Point(10, 30),
                    Size = new Size(tokensGroupBox.Width - 30, 50),
                    Font = new Font("Segoe UI", _fontSize, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                tokensGroupBox.Controls.Add(noTokensLabel);
            }
            sentenceGroupBox.Controls.Add(tokensGroupBox);
            
            // Update sentenceGroupBox height based on actual content
            var totalHeight = innerY + tokensGroupBox.Height + 20;
            sentenceGroupBox.Height = totalHeight;
            
            mainPanel.Controls.Add(sentenceGroupBox);
            y += sentenceGroupBox.Height + 15;
        }

        tab.Controls.Add(mainPanel);
        return tab;
    }


    private void AddLabel(Panel panel, string text, int y, FontStyle fontStyle, float fontSize = 9F, Color? color = null)
    {
        var label = new Label
        {
            Text = text,
            Location = new Point(0, y),
            Size = new Size(panel.Width - 20, 25),
            Font = new Font("Segoe UI", fontSize, fontStyle),
            ForeColor = color ?? Color.Black,
            AutoSize = false
        };
        panel.Controls.Add(label);
    }

    private Color GetDifficultyColor(string level)
    {
        return level switch
        {
            "A1" or "A2" => Color.FromArgb(200, 255, 200), // Light green
            "B1" or "B2" => Color.FromArgb(255, 255, 200), // Light yellow
            "C1" or "C2" => Color.FromArgb(255, 200, 200), // Light red
            _ => Color.White
        };
    }

    private Color GetIdiomTypeColor(string type)
    {
        return type switch
        {
            "idiom" => Color.FromArgb(255, 250, 240),
            "phrasalVerb" => Color.FromArgb(240, 250, 255),
            "collocation" => Color.FromArgb(250, 240, 255),
            "fixedExpression" => Color.FromArgb(240, 255, 240),
            _ => Color.White
        };
    }

    private void PopulateData()
    {
        // Data is already populated in tab creation
    }
}
