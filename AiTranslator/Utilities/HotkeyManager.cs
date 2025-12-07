using System.Runtime.InteropServices;
using AiTranslator.Models;

namespace AiTranslator.Utilities;

public class HotkeyManager : IDisposable
{
    private readonly Dictionary<int, Action> _hotkeyActions = new();
    private int _currentId = 1;
    private IntPtr _windowHandle;

    public event EventHandler<int>? HotkeyPressed;

    public void Initialize(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    public int RegisterHotkey(HotkeyConfig config, Action action)
    {
        if (_windowHandle == IntPtr.Zero)
            throw new InvalidOperationException("HotkeyManager not initialized");

        var modifiers = GetModifiers(config);
        var key = GetVirtualKeyCode(config.Key);

        var hotkeyId = _currentId++;
        
        if (RegisterHotKey(_windowHandle, hotkeyId, modifiers, key))
        {
            _hotkeyActions[hotkeyId] = action;
            return hotkeyId;
        }

        return -1;
    }

    public void UnregisterHotkey(int hotkeyId)
    {
        if (_windowHandle != IntPtr.Zero && _hotkeyActions.ContainsKey(hotkeyId))
        {
            UnregisterHotKey(_windowHandle, hotkeyId);
            _hotkeyActions.Remove(hotkeyId);
        }
    }

    public void UnregisterAllHotkeys()
    {
        foreach (var hotkeyId in _hotkeyActions.Keys.ToList())
        {
            UnregisterHotkey(hotkeyId);
        }
    }

    public bool ProcessHotkey(int hotkeyId)
    {
        if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
        {
            action?.Invoke();
            HotkeyPressed?.Invoke(this, hotkeyId);
            return true;
        }
        return false;
    }

    private uint GetModifiers(HotkeyConfig config)
    {
        uint modifiers = 0;
        if (config.Alt) modifiers |= MOD_ALT;
        if (config.Ctrl) modifiers |= MOD_CONTROL;
        if (config.Shift) modifiers |= MOD_SHIFT;
        if (config.Win) modifiers |= MOD_WIN;
        return modifiers;
    }

    private uint GetVirtualKeyCode(string key)
    {
        if (string.IsNullOrEmpty(key))
            return 0;

        // Function keys
        if (key.StartsWith("F") && int.TryParse(key.Substring(1), out int fNum) && fNum >= 1 && fNum <= 24)
            return (uint)(0x70 + fNum - 1); // VK_F1 to VK_F24

        // Letter keys
        if (key.Length == 1)
        {
            char c = char.ToUpper(key[0]);
            if (c >= 'A' && c <= 'Z')
                return c;
        }

        // Number keys
        if (key.Length == 1 && key[0] >= '0' && key[0] <= '9')
            return key[0];

        // Special keys
        return key.ToUpper() switch
        {
            "SPACE" => VK_SPACE,
            "ENTER" => VK_RETURN,
            "TAB" => VK_TAB,
            "ESC" => VK_ESCAPE,
            "ESCAPE" => VK_ESCAPE,
            "DELETE" => VK_DELETE,
            "INSERT" => VK_INSERT,
            "HOME" => VK_HOME,
            "END" => VK_END,
            "PAGEUP" => VK_PRIOR,
            "PAGEDOWN" => VK_NEXT,
            _ => 0
        };
    }

    public void Dispose()
    {
        UnregisterAllHotkeys();
    }

    // Windows API constants
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    private const uint VK_SPACE = 0x20;
    private const uint VK_RETURN = 0x0D;
    private const uint VK_TAB = 0x09;
    private const uint VK_ESCAPE = 0x1B;
    private const uint VK_DELETE = 0x2E;
    private const uint VK_INSERT = 0x2D;
    private const uint VK_HOME = 0x24;
    private const uint VK_END = 0x23;
    private const uint VK_PRIOR = 0x21;  // Page Up
    private const uint VK_NEXT = 0x22;   // Page Down

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public const int WM_HOTKEY = 0x0312;
}

