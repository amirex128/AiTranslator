using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AiTranslator.Utilities;

/// <summary>
/// Manages text selection and insertion across different applications
/// </summary>
public class SelectionManager
{
    private readonly ClipboardManager _clipboardManager;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private const int VK_CONTROL = 0x11;
    private const int VK_C = 0x43;
    private const int VK_V = 0x56;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const int MAX_RETRIES = 3;
    private const int RETRY_DELAY_MS = 150;

    private IntPtr _lastActiveWindow;
    private string _lastActiveWindowTitle = string.Empty;

    public SelectionManager(ClipboardManager clipboardManager)
    {
        _clipboardManager = clipboardManager;
    }

    /// <summary>
    /// Gets the currently selected text from the active application
    /// Uses multiple methods with retry logic for maximum reliability
    /// </summary>
    public async Task<string?> GetSelectedTextAsync()
    {
        // Store the current active window BEFORE any operations
        _lastActiveWindow = GetForegroundWindow();
        _lastActiveWindowTitle = GetActiveWindowTitle();

        // Save current clipboard content
        var originalClipboard = _clipboardManager.GetClipboardText();

        // Try multiple methods with retries
        string? selectedText = null;

        // Method 1: keybd_event with retries (most reliable)
        for (int attempt = 1; attempt <= MAX_RETRIES && string.IsNullOrWhiteSpace(selectedText); attempt++)
        {
            selectedText = await TryGetSelectedTextWithKeybdEvent(originalClipboard, attempt);
            if (!string.IsNullOrWhiteSpace(selectedText))
                break;
            
            if (attempt < MAX_RETRIES)
                await Task.Delay(RETRY_DELAY_MS);
        }

        // Method 2: SendKeys (fallback)
        if (string.IsNullOrWhiteSpace(selectedText))
        {
            selectedText = await TryGetSelectedTextWithSendKeys(originalClipboard);
        }

        // Method 3: Enhanced focus method
        if (string.IsNullOrWhiteSpace(selectedText))
        {
            selectedText = await TryGetSelectedTextWithEnhancedFocus(originalClipboard);
        }

        // Restore original clipboard if no text was selected
        if (string.IsNullOrWhiteSpace(selectedText))
        {
            if (!string.IsNullOrEmpty(originalClipboard))
            {
                _clipboardManager.SetClipboardText(originalClipboard, false);
            }
            return null;
        }

        // Restore original clipboard after a delay
        if (!string.IsNullOrEmpty(originalClipboard))
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                _clipboardManager.SetClipboardText(originalClipboard, false);
            });
        }

        return selectedText;
    }

    /// <summary>
    /// Method 1: Try to get selected text using keybd_event
    /// </summary>
    private async Task<string?> TryGetSelectedTextWithKeybdEvent(string? originalClipboard, int attempt)
    {
        try
        {
            // Clear clipboard
            _clipboardManager.ClearClipboard();
            await Task.Delay(50 + (attempt * 20)); // Progressive delay

            // Ensure window is valid and visible
            if (_lastActiveWindow == IntPtr.Zero || !IsWindow(_lastActiveWindow) || !IsWindowVisible(_lastActiveWindow))
            {
                _lastActiveWindow = GetForegroundWindow();
            }

            // Enhanced focus restoration
            if (_lastActiveWindow != IntPtr.Zero && IsWindow(_lastActiveWindow))
            {
                // Use AttachThreadInput for better focus control
                var foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
                var targetThread = GetWindowThreadProcessId(_lastActiveWindow, IntPtr.Zero);
                
                if (foregroundThread != targetThread)
                {
                    AttachThreadInput(foregroundThread, targetThread, true);
                }

                SetForegroundWindow(_lastActiveWindow);
                await Task.Delay(100 + (attempt * 30)); // Progressive delay

                if (foregroundThread != targetThread)
                {
                    AttachThreadInput(foregroundThread, targetThread, false);
                }
            }

            // Send Ctrl+C using keybd_event
            SendCtrlC();

            // Wait for clipboard to update with progressive delay
            await Task.Delay(200 + (attempt * 50));

            // Get the copied text
            var selectedText = _clipboardManager.GetClipboardText();

            // Verify we got new text (not the original)
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
    /// Method 2: Try to get selected text using SendKeys (fallback)
    /// </summary>
    private async Task<string?> TryGetSelectedTextWithSendKeys(string? originalClipboard)
    {
        try
        {
            // Clear clipboard
            _clipboardManager.ClearClipboard();
            await Task.Delay(100);

            // Restore focus
            if (_lastActiveWindow != IntPtr.Zero && IsWindow(_lastActiveWindow))
            {
                SetForegroundWindow(_lastActiveWindow);
                await Task.Delay(150);
            }

            // Use SendKeys as fallback
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
    /// Method 3: Enhanced focus method with multiple attempts
    /// </summary>
    private async Task<string?> TryGetSelectedTextWithEnhancedFocus(string? originalClipboard)
    {
        try
        {
            // Clear clipboard
            _clipboardManager.ClearClipboard();
            await Task.Delay(100);

            // Multiple focus attempts
            for (int focusAttempt = 0; focusAttempt < 3; focusAttempt++)
            {
                if (_lastActiveWindow != IntPtr.Zero && IsWindow(_lastActiveWindow))
                {
                    // Try different focus methods
                    if (focusAttempt == 0)
                    {
                        SetForegroundWindow(_lastActiveWindow);
                    }
                    else if (focusAttempt == 1)
                    {
                        // Use ShowWindow to ensure visibility
                        ShowWindow(_lastActiveWindow, 9); // SW_RESTORE
                        SetForegroundWindow(_lastActiveWindow);
                    }
                    else
                    {
                        // Force focus with AttachThreadInput
                        var foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
                        var targetThread = GetWindowThreadProcessId(_lastActiveWindow, IntPtr.Zero);
                        
                        if (foregroundThread != targetThread)
                        {
                            AttachThreadInput(foregroundThread, targetThread, true);
                            SetForegroundWindow(_lastActiveWindow);
                            await Task.Delay(50);
                            AttachThreadInput(foregroundThread, targetThread, false);
                        }
                    }

                    await Task.Delay(150);

                    // Send Ctrl+C
                    SendCtrlC();
                    await Task.Delay(300);

                    var selectedText = _clipboardManager.GetClipboardText();

                    if (!string.IsNullOrWhiteSpace(selectedText) && selectedText != originalClipboard)
                    {
                        return selectedText;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// Inserts text into the last active application, replacing the selected text
    /// </summary>
    public async Task<bool> InsertTextAsync(string text)
    {
        try
        {
            // Restore focus to the original window
            if (_lastActiveWindow != IntPtr.Zero)
            {
                SetForegroundWindow(_lastActiveWindow);
                await Task.Delay(150);
            }

            // Save current clipboard
            var originalClipboard = _clipboardManager.GetClipboardText();

            // Put the new text in clipboard
            _clipboardManager.SetClipboardText(text, false);

            // Wait a bit
            await Task.Delay(100);

            // Send Ctrl+V using keybd_event (more reliable than SendKeys)
            SendCtrlV();

            // Wait a bit
            await Task.Delay(150);

            // Restore original clipboard
            if (!string.IsNullOrEmpty(originalClipboard))
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
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
    /// Sends Ctrl+C using Windows API with enhanced reliability
    /// </summary>
    private void SendCtrlC()
    {
        try
        {
            // Press Ctrl
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            Thread.Sleep(15); // Slightly longer delay
            
            // Press C
            keybd_event(VK_C, 0, 0, UIntPtr.Zero);
            Thread.Sleep(15);
            
            // Release C
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(15);
            
            // Release Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(10); // Small delay after release
        }
        catch
        {
            // If keybd_event fails, try SendKeys as fallback
            try
            {
                SendKeys.SendWait("^c");
            }
            catch
            {
                // Ignore
            }
        }
    }

    /// <summary>
    /// Sends Ctrl+V using Windows API
    /// </summary>
    private void SendCtrlV()
    {
        // Press Ctrl
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        Thread.Sleep(10);
        
        // Press V
        keybd_event(VK_V, 0, 0, UIntPtr.Zero);
        Thread.Sleep(10);
        
        // Release V
        keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        Thread.Sleep(10);
        
        // Release Ctrl
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    /// <summary>
    /// Gets the title of the active window
    /// </summary>
    public string GetActiveWindowTitle()
    {
        const int nChars = 256;
        var buff = new StringBuilder(nChars);
        var handle = GetForegroundWindow();

        if (GetWindowText(handle, buff, nChars) > 0)
        {
            return buff.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets information about the last active window
    /// </summary>
    public (IntPtr Handle, string Title) GetLastActiveWindow()
    {
        return (_lastActiveWindow, _lastActiveWindowTitle);
    }
}
