using AiTranslator.Models;

namespace AiTranslator.Forms;

/// <summary>
/// Control group for managing a translation API configuration (4 endpoints + default selector)
/// </summary>
public class TranslationApiConfigControl
{
    public Label TitleLabel { get; }
    public ComboBox DefaultEndpointComboBox { get; }
    public TextBox[] EndpointNameTextBoxes { get; }
    public TextBox[] EndpointUrlTextBoxes { get; }
    public Label[] EndpointLabels { get; }

    private readonly TranslationApiConfig _config;
    private bool _isUpdatingComboBox = false;

    public TranslationApiConfigControl(
        string title,
        TranslationApiConfig config,
        int startY,
        Panel parentPanel)
    {
        _config = config;
        EndpointNameTextBoxes = new TextBox[4];
        EndpointUrlTextBoxes = new TextBox[4];
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
        // Don't subscribe to SelectedIndexChanged - it causes infinite loop when updating items
        parentPanel.Controls.Add(defaultLabel);
        parentPanel.Controls.Add(DefaultEndpointComboBox);
        y += 35;

        // 4 Endpoint rows (Name + URL)
        for (int i = 0; i < 4; i++)
        {
            EndpointLabels[i] = new Label
            {
                Text = $"API {i + 1}:",
                Location = new Point(10, y),
                Size = new Size(100, 20)
            };
            
            // Name TextBox
            EndpointNameTextBoxes[i] = new TextBox
            {
                Location = new Point(120, y),
                Size = new Size(120, 20)
            };
            // Update combo box when name changes (with debounce to prevent excessive updates)
            EndpointNameTextBoxes[i].TextChanged += OnEndpointNameTextChanged;
            
            // URL TextBox
            EndpointUrlTextBoxes[i] = new TextBox
            {
                Location = new Point(250, y),
                Size = new Size(300, 20)
            };
            
            parentPanel.Controls.Add(EndpointLabels[i]);
            parentPanel.Controls.Add(EndpointNameTextBoxes[i]);
            parentPanel.Controls.Add(EndpointUrlTextBoxes[i]);
            y += 30;
        }
    }

    private System.Windows.Forms.Timer? _updateTimer;

    private void OnEndpointNameTextChanged(object? sender, EventArgs e)
    {
        // Debounce updates to prevent excessive calls
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        
        _updateTimer = new System.Windows.Forms.Timer { Interval = 300 };
        _updateTimer.Tick += (s, args) =>
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            _updateTimer = null;
            UpdateDefaultComboBoxItems();
        };
        _updateTimer.Start();
    }

    private void UpdateDefaultComboBoxItems()
    {
        // Prevent infinite loop
        if (_isUpdatingComboBox)
            return;

        _isUpdatingComboBox = true;
        try
        {
            var items = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                var name = EndpointNameTextBoxes[i].Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = $"API {i + 1}";
                }
                items.Add(name);
            }
            
            // Check if items actually changed to avoid unnecessary updates
            var currentItems = new List<string>();
            foreach (var item in DefaultEndpointComboBox.Items)
            {
                currentItems.Add(item?.ToString() ?? string.Empty);
            }
            
            if (currentItems.SequenceEqual(items))
            {
                // Items haven't changed, no need to update
                return;
            }
            
            var selectedIndex = DefaultEndpointComboBox.SelectedIndex;
            
            DefaultEndpointComboBox.Items.Clear();
            DefaultEndpointComboBox.Items.AddRange(items.ToArray());
            
            if (selectedIndex >= 0 && selectedIndex < DefaultEndpointComboBox.Items.Count)
            {
                DefaultEndpointComboBox.SelectedIndex = selectedIndex;
            }
            else if (DefaultEndpointComboBox.Items.Count > 0)
            {
                DefaultEndpointComboBox.SelectedIndex = 0;
            }
        }
        finally
        {
            _isUpdatingComboBox = false;
        }
    }

    public void LoadFromConfig()
    {
        // Ensure Endpoints list exists
        if (_config.Endpoints == null)
        {
            _config.Endpoints = new List<EndpointInfo>();
        }

        // Temporarily disable TextChanged events to prevent infinite loop during loading
        for (int i = 0; i < 4; i++)
        {
            EndpointNameTextBoxes[i].TextChanged -= OnEndpointNameTextChanged;
        }

        try
        {
            // Load endpoints - always load 4 textboxes
            // If config has fewer than 4 endpoints, fill the rest with empty values
            for (int i = 0; i < 4; i++)
            {
                if (i < _config.Endpoints.Count && _config.Endpoints[i] != null)
                {
                    EndpointNameTextBoxes[i].Text = _config.Endpoints[i].Name ?? string.Empty;
                    EndpointUrlTextBoxes[i].Text = _config.Endpoints[i].Url ?? string.Empty;
                }
                else
                {
                    EndpointNameTextBoxes[i].Text = string.Empty;
                    EndpointUrlTextBoxes[i].Text = string.Empty;
                }
            }

            // Update combo box items and load default endpoint index
            UpdateDefaultComboBoxItems();
            var validIndex = Math.Max(0, Math.Min(_config.DefaultEndpointIndex, 3));
            if (validIndex < DefaultEndpointComboBox.Items.Count)
            {
                DefaultEndpointComboBox.SelectedIndex = validIndex;
            }
        }
        finally
        {
            // Re-enable TextChanged events
            for (int i = 0; i < 4; i++)
            {
                EndpointNameTextBoxes[i].TextChanged += OnEndpointNameTextChanged;
            }
        }
    }

    public void SaveToConfig()
    {
        // Ensure Endpoints list exists
        if (_config.Endpoints == null)
        {
            _config.Endpoints = new List<EndpointInfo>();
        }

        // Save all 4 endpoints (including empty ones) to preserve indices
        // This ensures that each textbox position maps to the same index in the config
        _config.Endpoints.Clear();
        for (int i = 0; i < 4; i++)
        {
            var name = EndpointNameTextBoxes[i].Text.Trim();
            var url = EndpointUrlTextBoxes[i].Text.Trim();
            
            // If name is empty, use default name
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"API {i + 1}";
            }
            
            _config.Endpoints.Add(new EndpointInfo
            {
                Name = name,
                Url = url
            });
        }

        // Save default endpoint index
        var selectedIndex = DefaultEndpointComboBox.SelectedIndex;
        _config.DefaultEndpointIndex = Math.Max(0, Math.Min(selectedIndex, 3));
    }
}
