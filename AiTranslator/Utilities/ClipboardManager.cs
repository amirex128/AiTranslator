namespace AiTranslator.Utilities;

public class ClipboardManager
{
    private string? _previousClipboardContent;

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

    public void SetClipboardText(string text, bool saveHistory = true)
    {
        try
        {
            if (saveHistory)
            {
                _previousClipboardContent = GetClipboardText();
            }
            
            Clipboard.SetText(text);
        }
        catch (Exception)
        {
            // Clipboard access can fail for various reasons
        }
    }

    public void UndoClipboard()
    {
        if (!string.IsNullOrEmpty(_previousClipboardContent))
        {
            try
            {
                Clipboard.SetText(_previousClipboardContent);
                _previousClipboardContent = null;
            }
            catch (Exception)
            {
                // Clipboard access can fail for various reasons
            }
        }
    }

    public bool HasPreviousContent()
    {
        return !string.IsNullOrEmpty(_previousClipboardContent);
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

