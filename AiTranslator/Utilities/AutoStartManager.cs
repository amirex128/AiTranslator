using Microsoft.Win32;

namespace AiTranslator.Utilities;

public class AutoStartManager
{
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApplicationName = "AiTranslator";

    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Application.ExecutablePath;
                key.SetValue(ApplicationName, $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue(ApplicationName) != null)
                {
                    key.DeleteValue(ApplicationName);
                }
            }
        }
        catch (Exception)
        {
            // Handle registry access errors silently
        }
    }

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            if (key == null) return false;

            var value = key.GetValue(ApplicationName);
            return value != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

