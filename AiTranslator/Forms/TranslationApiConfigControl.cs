using AiTranslator.Models;

namespace AiTranslator.Forms;

/// <summary>
/// Control group for managing a translation API configuration (4 endpoints + default selector)
/// </summary>
public class TranslationApiConfigControl
{
    public Label TitleLabel { get; }
    public ComboBox DefaultEndpointComboBox { get; }
    public TextBox[] EndpointTextBoxes { get; }
    public Label[] EndpointLabels { get; }

    private readonly TranslationApiConfig _config;

    public TranslationApiConfigControl(
        string title,
        TranslationApiConfig config,
        int startY,
        Panel parentPanel)
    {
        _config = config;
        EndpointTextBoxes = new TextBox[4];
        EndpointLabels = new Label[4];

        int y = startY;

        // Title
        TitleLabel = new Label
        {
            Text = title,
            Location = new Point(10, y),
            Size = new Size(300, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        parentPanel.Controls.Add(TitleLabel);
        y += 30;

        // Default Endpoint Selector
        var defaultLabel = new Label
        {
            Text = "Default API:",
            Location = new Point(10, y),
            Size = new Size(100, 20)
        };
        DefaultEndpointComboBox = new ComboBox
        {
            Location = new Point(120, y),
            Size = new Size(200, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        DefaultEndpointComboBox.Items.AddRange(new object[] { "API 1", "API 2", "API 3", "API 4" });
        DefaultEndpointComboBox.SelectedIndex = 0;
        parentPanel.Controls.Add(defaultLabel);
        parentPanel.Controls.Add(DefaultEndpointComboBox);
        y += 35;

        // 4 Endpoint TextBoxes
        for (int i = 0; i < 4; i++)
        {
            EndpointLabels[i] = new Label
            {
                Text = $"API {i + 1}:",
                Location = new Point(10, y),
                Size = new Size(100, 20)
            };
            EndpointTextBoxes[i] = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(430, 20)
            };
            parentPanel.Controls.Add(EndpointLabels[i]);
            parentPanel.Controls.Add(EndpointTextBoxes[i]);
            y += 30;
        }
    }

    public void LoadFromConfig()
    {
        // Ensure Endpoints list exists
        if (_config.Endpoints == null)
        {
            _config.Endpoints = new List<string>();
        }

        // Load endpoints - always load 4 textboxes
        // If config has fewer than 4 endpoints, fill the rest with empty strings
        for (int i = 0; i < 4; i++)
        {
            if (i < _config.Endpoints.Count)
            {
                EndpointTextBoxes[i].Text = _config.Endpoints[i];
            }
            else
            {
                EndpointTextBoxes[i].Text = string.Empty;
            }
        }

        // Load default endpoint index
        // The combo box shows "API 1", "API 2", "API 3", "API 4" which maps directly to indices 0, 1, 2, 3
        var validIndex = Math.Max(0, Math.Min(_config.DefaultEndpointIndex, 3));
        DefaultEndpointComboBox.SelectedIndex = validIndex;
    }

    public void SaveToConfig()
    {
        // Ensure Endpoints list exists
        if (_config.Endpoints == null)
        {
            _config.Endpoints = new List<string>();
        }

        // Save all 4 endpoints (including empty ones) to preserve indices
        // This ensures that each textbox position maps to the same index in the config
        _config.Endpoints.Clear();
        for (int i = 0; i < 4; i++)
        {
            var endpoint = EndpointTextBoxes[i].Text.Trim();
            _config.Endpoints.Add(endpoint); // Always add, even if empty
        }

        // Save default endpoint index
        // The combo box shows "API 1", "API 2", "API 3", "API 4" which maps directly to indices 0, 1, 2, 3
        var selectedIndex = DefaultEndpointComboBox.SelectedIndex;
        _config.DefaultEndpointIndex = Math.Max(0, Math.Min(selectedIndex, 3));
    }
}
