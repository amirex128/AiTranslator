using AiTranslator.Models;
using System.Windows.Forms;

namespace AiTranslator.Forms;

public class HotkeyConfigControl : UserControl
{
    private readonly Label _actionLabel;
    private readonly TextBox _hotkeyTextBox;
    private readonly CheckBox _ctrlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly ComboBox _keyComboBox;
    
    private HotkeyConfig _config = new();

    public HotkeyConfigControl(string actionName, HotkeyConfig config, int y, Panel parent)
    {
        _config = config ?? new HotkeyConfig();

        this.Location = new Point(10, y);
        this.Size = new Size(560, 60);
        this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Action Label
        _actionLabel = new Label
        {
            Text = actionName,
            Location = new Point(0, 5),
            Size = new Size(200, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        this.Controls.Add(_actionLabel);

        // Modifier CheckBoxes
        _ctrlCheckBox = new CheckBox
        {
            Text = "Ctrl",
            Location = new Point(0, 30),
            Size = new Size(60, 20)
        };
        this.Controls.Add(_ctrlCheckBox);

        _altCheckBox = new CheckBox
        {
            Text = "Alt",
            Location = new Point(70, 30),
            Size = new Size(60, 20)
        };
        this.Controls.Add(_altCheckBox);

        _shiftCheckBox = new CheckBox
        {
            Text = "Shift",
            Location = new Point(140, 30),
            Size = new Size(60, 20)
        };
        this.Controls.Add(_shiftCheckBox);

        _winCheckBox = new CheckBox
        {
            Text = "Win",
            Location = new Point(210, 30),
            Size = new Size(60, 20)
        };
        this.Controls.Add(_winCheckBox);

        // Key ComboBox
        var keyLabel = new Label
        {
            Text = "Key:",
            Location = new Point(280, 32),
            Size = new Size(40, 20)
        };
        this.Controls.Add(keyLabel);

        _keyComboBox = new ComboBox
        {
            Location = new Point(330, 30),
            Size = new Size(100, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        
        // Add keys to combo box
        var keys = new List<string>();
        // Function keys
        for (int i = 1; i <= 24; i++)
            keys.Add($"F{i}");
        // Letters
        for (char c = 'A'; c <= 'Z'; c++)
            keys.Add(c.ToString());
        // Numbers
        for (int i = 0; i <= 9; i++)
            keys.Add(i.ToString());
        // Special keys
        keys.AddRange(new[] { "Space", "Enter", "Tab", "Esc", "Delete", "Insert", "Home", "End", "PageUp", "PageDown" });
        
        _keyComboBox.Items.AddRange(keys.ToArray());
        this.Controls.Add(_keyComboBox);

        // Hotkey Display TextBox (read-only, shows current combination)
        _hotkeyTextBox = new TextBox
        {
            Location = new Point(440, 30),
            Size = new Size(120, 20),
            ReadOnly = true,
            BackColor = Color.White
        };
        this.Controls.Add(_hotkeyTextBox);

        // Load initial values
        LoadFromConfig();

        // Update display when checkboxes or key changes
        _ctrlCheckBox.CheckedChanged += OnConfigChanged;
        _altCheckBox.CheckedChanged += OnConfigChanged;
        _shiftCheckBox.CheckedChanged += OnConfigChanged;
        _winCheckBox.CheckedChanged += OnConfigChanged;
        _keyComboBox.SelectedIndexChanged += OnConfigChanged;

        parent.Controls.Add(this);
    }

    private void OnConfigChanged(object? sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var parts = new List<string>();
        if (_ctrlCheckBox.Checked) parts.Add("Ctrl");
        if (_altCheckBox.Checked) parts.Add("Alt");
        if (_shiftCheckBox.Checked) parts.Add("Shift");
        if (_winCheckBox.Checked) parts.Add("Win");
        if (_keyComboBox.SelectedItem != null)
            parts.Add(_keyComboBox.SelectedItem.ToString()!);
        
        _hotkeyTextBox.Text = parts.Count > 0 ? string.Join(" + ", parts) : "(Not set)";
    }

    public void LoadFromConfig()
    {
        if (_config == null)
        {
            _config = new HotkeyConfig();
        }

        _ctrlCheckBox.Checked = _config.Ctrl;
        _altCheckBox.Checked = _config.Alt;
        _shiftCheckBox.Checked = _config.Shift;
        _winCheckBox.Checked = _config.Win;

        if (!string.IsNullOrEmpty(_config.Key))
        {
            var keyIndex = _keyComboBox.Items.IndexOf(_config.Key);
            if (keyIndex >= 0)
            {
                _keyComboBox.SelectedIndex = keyIndex;
            }
        }

        UpdateDisplay();
    }

    public void SaveToConfig(HotkeyConfig config)
    {
        config.Ctrl = _ctrlCheckBox.Checked;
        config.Alt = _altCheckBox.Checked;
        config.Shift = _shiftCheckBox.Checked;
        config.Win = _winCheckBox.Checked;
        config.Key = _keyComboBox.SelectedItem?.ToString() ?? string.Empty;
    }

    public HotkeyConfig GetConfig()
    {
        var config = new HotkeyConfig();
        SaveToConfig(config);
        return config;
    }
}
