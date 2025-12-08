namespace AiTranslator.Utilities;

public class ClipboardManager
{
    public string? GetClipboardText()
    {
        try
        {
            if (Clipboard.ContainsText())
            {
                return Clipboard.GetText();
            }
        }
        catch (Exception)
        {
            // Clipboard access can fail for various reasons
        }
        return null;
    }

    public void SetClipboardText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception)
        {
            // Clipboard access can fail for various reasons
        }
    }

    public void ClearClipboard()
    {
        try
        {
            Clipboard.Clear();
        }
        catch (Exception)
        {
            // Ignore clipboard errors
        }
    }
}

