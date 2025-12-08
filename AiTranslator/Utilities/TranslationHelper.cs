using System.Windows.Forms;

namespace AiTranslator.Utilities;

/// <summary>
/// Helper class for translation-related utilities
/// </summary>
public static class TranslationHelper
{
    /// <summary>
    /// Parses translation response text that may contain multiple options separated by %%%%%
    /// </summary>
    /// <param name="text">The translation response text</param>
    /// <returns>List of translation options</returns>
    public static List<string> ParseTranslationOptions(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string> { text };

        // Split by %%%%% separator
        var options = text.Split(new[] { "%%%%%" }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(o => o.Trim())
                          .Where(o => !string.IsNullOrWhiteSpace(o))
                          .ToList();

        return options.Count > 0 ? options : new List<string> { text };
    }

    /// <summary>
    /// Calculates the height needed to display text with the given font and width
    /// </summary>
    /// <param name="control">Control to use for graphics context</param>
    /// <param name="text">Text to measure</param>
    /// <param name="font">Font to use</param>
    /// <param name="width">Available width</param>
    /// <returns>Calculated height (minimum 50px)</returns>
    public static int GetTextHeight(Control control, string text, Font font, int width)
    {
        using (var g = control.CreateGraphics())
        {
            var size = g.MeasureString(text, font, width);
            return Math.Max(50, (int)size.Height + 20); // Minimum 50px, add padding
        }
    }
}
