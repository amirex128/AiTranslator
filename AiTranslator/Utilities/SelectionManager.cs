using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AiTranslator.Utilities;

/// <summary>
/// Manages text selection and insertion across different applications
/// Simple clipboard-based approach - no side effects
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
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private IntPtr _lastActiveWindow;

    public SelectionManager(ClipboardManager clipboardManager)
    {
        _clipboardManager = clipboardManager;
    }

    /// <summary>
    /// Gets text from clipboard - simple and reliable, no side effects
    /// </summary>
    public Task<string?> GetSelectedTextAsync()
    {
        // Simply get text from clipboard - no focus, no key simulation, no side effects
        var clipboardText = _clipboardManager.GetClipboardText();
        return Task.FromResult(clipboardText);
    }

    /// <summary>
    /// Inserts text into the last active application
    /// </summary>
    public async Task<bool> InsertTextAsync(string text)
    {
        try
        {
            // Store the current active window
            _lastActiveWindow = GetForegroundWindow();

            // Validate window
            if (_lastActiveWindow == IntPtr.Zero || !IsWindow(_lastActiveWindow) || !IsWindowVisible(_lastActiveWindow))
            {
                return false;
            }

            // Ensure window has focus
            await EnsureWindowFocusedAsync();

            // Save current clipboard
            var originalClipboard = _clipboardManager.GetClipboardText();

            // Put the new text in clipboard
            _clipboardManager.SetClipboardText(text);

            // Wait for clipboard to be set
            await Task.Delay(50);

            // Send Ctrl+V using SendKeys (simple and reliable)
            SendKeys.SendWait("^v");

            // Wait for paste to complete
            await Task.Delay(100);

            // Restore original clipboard after delay
            if (!string.IsNullOrEmpty(originalClipboard))
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(300);
                    _clipboardManager.SetClipboardText(originalClipboard);
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
    /// Ensures the target window has focus for paste operation
    /// </summary>
    private async Task EnsureWindowFocusedAsync()
    {
        try
        {
            // Check if window is already in foreground
            var currentForeground = GetForegroundWindow();
            if (currentForeground == _lastActiveWindow)
            {
                return; // Already focused
            }

            // Allow our process to set foreground window
            GetWindowThreadProcessId(_lastActiveWindow, out uint processId);
            if (processId != 0)
            {
                AllowSetForegroundWindow((int)processId);
                await Task.Delay(10);
            }

            // Set foreground window
            SetForegroundWindow(_lastActiveWindow);
            await Task.Delay(50); // Short delay to ensure focus is set
        }
        catch
        {
            // Silent fail - continue anyway
        }
    }
}
