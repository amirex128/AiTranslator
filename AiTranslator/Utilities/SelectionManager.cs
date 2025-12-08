using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AiTranslator.Utilities;

/// <summary>
/// Manages text selection and insertion across different applications
/// Enhanced with more robust methods and better timing to prevent side effects
/// </summary>
public class SelectionManager
{
    private readonly ClipboardManager _clipboardManager;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(int dwProcessId);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetMessageExtraInfo();

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    private static extern bool SetKeyboardState(byte[] lpKeyState);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    private const int VK_CONTROL = 0x11;
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_C = 0x43;
    private const int VK_V = 0x56;
    private const int VK_SHIFT = 0x10;
    private const int VK_MENU = 0x12; // Alt key

    private IntPtr _lastActiveWindow;

    public SelectionManager(ClipboardManager clipboardManager)
    {
        _clipboardManager = clipboardManager;
    }

    /// <summary>
    /// Gets the currently selected text from the active application
    /// Uses multiple fallback methods with proper timing to ensure reliability
    /// </summary>
    public async Task<string?> GetSelectedTextAsync()
    {
        // Store the current active window BEFORE any operations
        _lastActiveWindow = GetForegroundWindow();

        // Validate window
        if (_lastActiveWindow == IntPtr.Zero || !IsWindow(_lastActiveWindow) || !IsWindowVisible(_lastActiveWindow))
        {
            return null;
        }

        // Release any stuck keys before attempting to copy
        await ReleaseStuckKeysAsync();

        // Save current clipboard content
        var originalClipboard = _clipboardManager.GetClipboardText();

        string? selectedText = null;

        // Method 1: SendKeys.SendWait - Most reliable for clipboard operations
        selectedText = await TryGetSelectedTextMethod1_SendKeys(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 2: SendKeys with longer delays
        selectedText = await TryGetSelectedTextMethod2_SendKeysLongDelay(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 3: SendKeys with multiple retries
        selectedText = await TryGetSelectedTextMethod3_SendKeysRetry(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 4: SendInput with proper key state check
        selectedText = await TryGetSelectedTextMethod4_SendInputSafe(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 5: SendInput with extended delays
        selectedText = await TryGetSelectedTextMethod5_SendInputExtended(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 6: keybd_event with proper timing
        selectedText = await TryGetSelectedTextMethod6_KeybdEvent(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 7: keybd_event with extended delays
        selectedText = await TryGetSelectedTextMethod7_KeybdEventLong(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 8: Focus restoration with SendKeys
        selectedText = await TryGetSelectedTextMethod8_WithFocusRestore(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 9: Multiple progressive attempts with alternating methods
        selectedText = await TryGetSelectedTextMethod9_ProgressiveRetry(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 10: Last resort - very long delays
        selectedText = await TryGetSelectedTextMethod10_VeryLongDelays(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 11: SendInput with scan codes
        selectedText = await TryGetSelectedTextMethod11_SendInputScanCode(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Method 12: Hybrid approach - SendKeys for Ctrl, keybd_event for C
        selectedText = await TryGetSelectedTextMethod12_Hybrid(originalClipboard);
        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            RestoreClipboardDelayed(originalClipboard);
            return selectedText;
        }

        // Restore original clipboard if no text was selected
        if (!string.IsNullOrEmpty(originalClipboard))
        {
            _clipboardManager.SetClipboardText(originalClipboard, false);
        }

        return null;
    }

    /// <summary>
    /// Release any keys that might be stuck in pressed state
    /// </summary>
    private async Task ReleaseStuckKeysAsync()
    {
        try
        {
            // Check if Ctrl, Shift, or Alt keys are stuck
            var stuckKeys = new List<int>();

            if (IsKeyPressed(VK_CONTROL) || IsKeyPressed(VK_LCONTROL) || IsKeyPressed(VK_RCONTROL))
            {
                stuckKeys.Add(VK_CONTROL);
                stuckKeys.Add(VK_LCONTROL);
                stuckKeys.Add(VK_RCONTROL);
            }

            if (IsKeyPressed(VK_C))
            {
                stuckKeys.Add(VK_C);
            }

            if (IsKeyPressed(VK_SHIFT))
            {
                stuckKeys.Add(VK_SHIFT);
            }

            if (IsKeyPressed(VK_MENU))
            {
                stuckKeys.Add(VK_MENU);
            }

            // Release stuck keys
            foreach (var key in stuckKeys)
            {
                keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                await Task.Delay(10);
            }

            if (stuckKeys.Count > 0)
            {
                await Task.Delay(50); // Extra delay to ensure keys are released
            }
        }
        catch
        {
            // Silent fail
        }
    }

    /// <summary>
    /// Check if a key is currently pressed
    /// </summary>
    private bool IsKeyPressed(int vKey)
    {
        try
        {
            short keyState = GetAsyncKeyState(vKey);
            // Check if the most significant bit is set (key is down)
            return (keyState & 0x8000) != 0;
        }
        catch
        {
            return false;
        }
    }

    private void RestoreClipboardDelayed(string? originalClipboard)
    {
        if (!string.IsNullOrEmpty(originalClipboard))
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(400);
                _clipboardManager.SetClipboardText(originalClipboard, false);
            });
        }
    }

    /// <summary>
    /// Method 1: SendKeys.SendWait - Most reliable for clipboard operations
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod1_SendKeys(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(50);

            // Release any keys that might be pressed
            await ReleaseStuckKeysAsync();
            await Task.Delay(30);

            SendKeys.SendWait("^c");
            await Task.Delay(250);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 2: SendKeys with longer delays
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod2_SendKeysLongDelay(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(100);

            await ReleaseStuckKeysAsync();
            await Task.Delay(50);

            SendKeys.SendWait("^c");
            await Task.Delay(400);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 3: SendKeys with multiple retries
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod3_SendKeysRetry(string? originalClipboard)
    {
        try
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                _clipboardManager.ClearClipboard();
                await Task.Delay(50 + (attempt * 25));

                await ReleaseStuckKeysAsync();
                await Task.Delay(30);

                SendKeys.SendWait("^c");
                await Task.Delay(200 + (attempt * 100));

                var selectedText = _clipboardManager.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
                {
                    return selectedText;
                }

                if (attempt < 3)
                {
                    await Task.Delay(100);
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 4: SendInput with key state checking
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod4_SendInputSafe(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(50);

            // Ensure no keys are stuck
            await ReleaseStuckKeysAsync();
            await Task.Delay(50);

            // Verify keys are released
            if (IsKeyPressed(VK_CONTROL) || IsKeyPressed(VK_C))
            {
                await Task.Delay(100);
            }

            SendCtrlCWithSendInputSafe();
            await Task.Delay(300);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 5: SendInput with extended delays between key events
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod5_SendInputExtended(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(80);

            await ReleaseStuckKeysAsync();
            await Task.Delay(50);

            SendCtrlCWithSendInputExtended();
            await Task.Delay(350);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 6: keybd_event with proper timing and delays
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod6_KeybdEvent(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(60);

            await ReleaseStuckKeysAsync();
            await Task.Delay(50);

            SendCtrlCWithKeyBdEventSafe();
            await Task.Delay(300);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 7: keybd_event with very long delays
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod7_KeybdEventLong(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(100);

            await ReleaseStuckKeysAsync();
            await Task.Delay(80);

            SendCtrlCWithKeyBdEventLong();
            await Task.Delay(400);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 8: With focus restoration
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod8_WithFocusRestore(string? originalClipboard)
    {
        try
        {
            // Check if window is still valid
            if (_lastActiveWindow == IntPtr.Zero || !IsWindow(_lastActiveWindow) || !IsWindowVisible(_lastActiveWindow))
            {
                return null;
            }

            _clipboardManager.ClearClipboard();
            await Task.Delay(60);

            // Check if window is already in foreground
            var currentForeground = GetForegroundWindow();
            if (currentForeground != _lastActiveWindow)
            {
                // Allow our process to set foreground window
                GetWindowThreadProcessId(_lastActiveWindow, out int processId);
                if (processId != 0)
                {
                    AllowSetForegroundWindow(processId);
                }

                // Restore focus
                SetForegroundWindow(_lastActiveWindow);
                await Task.Delay(120);

                // Verify focus was set
                if (GetForegroundWindow() != _lastActiveWindow)
                {
                    return null;
                }
            }

            await ReleaseStuckKeysAsync();
            await Task.Delay(50);

            SendKeys.SendWait("^c");
            await Task.Delay(300);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 9: Progressive retry with alternating methods
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod9_ProgressiveRetry(string? originalClipboard)
    {
        try
        {
            for (int attempt = 1; attempt <= 4; attempt++)
            {
                _clipboardManager.ClearClipboard();
                await Task.Delay(60 + (attempt * 30));

                await ReleaseStuckKeysAsync();
                await Task.Delay(40 + (attempt * 20));

                // Alternate between different methods
                if (attempt % 2 == 1)
                {
                    SendKeys.SendWait("^c");
                }
                else
                {
                    SendCtrlCWithSendInputSafe();
                }

                await Task.Delay(250 + (attempt * 75));

                var selectedText = _clipboardManager.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
                {
                    return selectedText;
                }

                if (attempt < 4)
                {
                    await Task.Delay(80);
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 10: Very long delays for slow applications
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod10_VeryLongDelays(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(150);

            await ReleaseStuckKeysAsync();
            await Task.Delay(100);

            SendKeys.SendWait("^c");
            await Task.Delay(500);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            // Try again with SendInput
            _clipboardManager.ClearClipboard();
            await Task.Delay(150);

            await ReleaseStuckKeysAsync();
            await Task.Delay(100);

            SendCtrlCWithSendInputExtended();
            await Task.Delay(500);

            selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 11: SendInput with scan codes
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod11_SendInputScanCode(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(70);

            await ReleaseStuckKeysAsync();
            await Task.Delay(50);

            SendCtrlCWithScanCode();
            await Task.Delay(320);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Method 12: Hybrid approach - mix different techniques
    /// </summary>
    private async Task<string?> TryGetSelectedTextMethod12_Hybrid(string? originalClipboard)
    {
        try
        {
            _clipboardManager.ClearClipboard();
            await Task.Delay(80);

            await ReleaseStuckKeysAsync();
            await Task.Delay(60);

            // Use SendInput for Ctrl down
            var inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_CONTROL,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            await Task.Delay(50);

            // Use SendKeys for C
            SendKeys.SendWait("c");
            await Task.Delay(50);

            // Release Ctrl with SendInput
            inputs[0].u.ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            await Task.Delay(100);

            await Task.Delay(200);

            var selectedText = _clipboardManager.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
            {
                return selectedText;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Inserts text into the last active application
    /// </summary>
    public async Task<bool> InsertTextAsync(string text)
    {
        try
        {
            // Validate window
            if (_lastActiveWindow == IntPtr.Zero || !IsWindow(_lastActiveWindow) || !IsWindowVisible(_lastActiveWindow))
            {
                return false;
            }

            // Release any stuck keys
            await ReleaseStuckKeysAsync();

            // Check if window is already in foreground
            var currentForeground = GetForegroundWindow();
            if (currentForeground != _lastActiveWindow)
            {
                // Allow our process to set foreground window
                GetWindowThreadProcessId(_lastActiveWindow, out int processId);
                if (processId != 0)
                {
                    AllowSetForegroundWindow(processId);
                }

                // Restore focus
                SetForegroundWindow(_lastActiveWindow);
                await Task.Delay(100);
            }

            // Save current clipboard
            var originalClipboard = _clipboardManager.GetClipboardText();

            // Put the new text in clipboard
            _clipboardManager.SetClipboardText(text, false);

            // Wait for clipboard to be set
            await Task.Delay(80);

            // Send Ctrl+V using SendKeys (most reliable)
            SendKeys.SendWait("^v");

            // Wait for paste to complete
            await Task.Delay(150);

            // Restore original clipboard
            if (!string.IsNullOrEmpty(originalClipboard))
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(400);
                    _clipboardManager.SetClipboardText(originalClipboard, false);
                });
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// SendInput with safety checks
    /// </summary>
    private void SendCtrlCWithSendInputSafe()
    {
        try
        {
            var inputs = new INPUT[4];
            var extraInfo = GetMessageExtraInfo();

            // Press Ctrl
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_CONTROL,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            // Press C
            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_C,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            // Release C
            inputs[2] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_C,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            // Release Ctrl
            inputs[3] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_CONTROL,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            SendInput(4, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(20);
        }
        catch
        {
            // Silent fail
        }
    }

    /// <summary>
    /// SendInput with extended timing
    /// </summary>
    private void SendCtrlCWithSendInputExtended()
    {
        try
        {
            var extraInfo = GetMessageExtraInfo();

            // Press Ctrl
            var input1 = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_CONTROL,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };
            SendInput(1, new[] { input1 }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(60);

            // Press C
            var input2 = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_C,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };
            SendInput(1, new[] { input2 }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(60);

            // Release C
            var input3 = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_C,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };
            SendInput(1, new[] { input3 }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(60);

            // Release Ctrl
            var input4 = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_CONTROL,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };
            SendInput(1, new[] { input4 }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(20);
        }
        catch
        {
            // Silent fail
        }
    }

    /// <summary>
    /// keybd_event with safe timing
    /// </summary>
    private void SendCtrlCWithKeyBdEventSafe()
    {
        try
        {
            // Press Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(50);

            // Press C
            keybd_event(VK_C, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(50);

            // Release C
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(50);

            // Release Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(30);
        }
        catch
        {
            // Silent fail
        }
    }

    /// <summary>
    /// keybd_event with long delays
    /// </summary>
    private void SendCtrlCWithKeyBdEventLong()
    {
        try
        {
            // Press Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(80);

            // Press C
            keybd_event(VK_C, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(80);

            // Release C
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(80);

            // Release Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(50);
        }
        catch
        {
            // Silent fail
        }
    }

    /// <summary>
    /// SendInput with scan codes
    /// </summary>
    private void SendCtrlCWithScanCode()
    {
        try
        {
            var inputs = new INPUT[4];
            var extraInfo = GetMessageExtraInfo();

            // Press Ctrl with scan code
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = 0x1D, // Ctrl scan code
                        dwFlags = KEYEVENTF_SCANCODE,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            // Press C with scan code
            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = 0x2E, // C scan code
                        dwFlags = KEYEVENTF_SCANCODE,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            // Release C with scan code
            inputs[2] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = 0x2E, // C scan code
                        dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            // Release Ctrl with scan code
            inputs[3] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = 0x1D, // Ctrl scan code
                        dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = extraInfo
                    }
                }
            };

            SendInput(4, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(20);
        }
        catch
        {
            // Silent fail
        }
    }
}