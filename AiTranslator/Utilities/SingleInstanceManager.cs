using System.Runtime.InteropServices;

namespace AiTranslator.Utilities;

public class SingleInstanceManager : IDisposable
{
    private readonly Mutex _mutex;
    private bool _isOwned;

    public SingleInstanceManager(string applicationId)
    {
        _mutex = new Mutex(true, $"Global\\{applicationId}", out _isOwned);
    }

    public bool IsFirstInstance => _isOwned;

    public static void BringExistingInstanceToFront()
    {
        // Find the existing window and bring it to front
        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);

        foreach (var process in processes)
        {
            if (process.Id != currentProcess.Id)
            {
                var handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    ShowWindow(handle, SW_RESTORE);
                    SetForegroundWindow(handle);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_isOwned)
        {
            _mutex.ReleaseMutex();
        }
        _mutex.Dispose();
    }

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
}

