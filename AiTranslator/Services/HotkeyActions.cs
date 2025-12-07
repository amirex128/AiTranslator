using AiTranslator.Forms;
using AiTranslator.Models;
using AiTranslator.Utilities;

namespace AiTranslator.Services;

public class HotkeyActions
{
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;
    private readonly ILanguageDetector _languageDetector;
    private readonly IConfigService _configService;
    private readonly ILoggingService _loggingService;
    private readonly INotificationService _notificationService;
    private readonly ClipboardManager _clipboardManager;

    public HotkeyActions(
        ITranslationService translationService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        IConfigService configService,
        ILoggingService loggingService,
        INotificationService notificationService,
        ClipboardManager clipboardManager)
    {
        _translationService = translationService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _configService = configService;
        _loggingService = loggingService;
        _notificationService = notificationService;
        _clipboardManager = clipboardManager;
    }

    public void ShowPopupTranslation(TranslationType type)
    {
        try
        {
            var clipboardText = _clipboardManager.GetClipboardText();
            
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                ShowNotification("Clipboard is empty", "Cannot translate empty clipboard");
                return;
            }

            var popup = new TranslationPopupForm(
                _translationService,
                _ttsService,
                _languageDetector,
                _configService,
                _loggingService
            );

            popup.ShowPopup(clipboardText, type);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error showing popup translation", ex);
            ShowNotification("Error", $"Failed to show translation popup: {ex.Message}");
        }
    }

    public async void ReplaceClipboardWithTranslation(TranslationType type)
    {
        LoadingPopupForm? loadingPopup = null;
        
        try
        {
            var clipboardText = _clipboardManager.GetClipboardText();
            
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                ShowNotification("Clipboard is empty", "Cannot translate empty clipboard");
                return;
            }

            loadingPopup = new LoadingPopupForm();
            loadingPopup.ShowLoading("Translating...");

            TranslationResponse response;

            switch (type)
            {
                case TranslationType.PersianToEnglish:
                    response = await _translationService.TranslatePersianToEnglishAsync(clipboardText);
                    break;
                case TranslationType.EnglishToPersian:
                    response = await _translationService.TranslateEnglishToPersianAsync(clipboardText);
                    break;
                case TranslationType.GrammarFix:
                    response = await _translationService.FixGrammarAsync(clipboardText);
                    break;
                default:
                    throw new ArgumentException("Invalid translation type");
            }

            loadingPopup.HideLoading();
            loadingPopup.Close();
            loadingPopup.Dispose();
            loadingPopup = null;

            if (response.Success)
            {
                // Parse response to check for multiple options separated by %%%%%
                var options = ParseTranslationOptions(response.Text);

                if (options.Count > 1)
                {
                    // Show selection form for multiple options
                    ShowSelectionForm(options);
                }
                else
                {
                    // Single option - directly copy to clipboard
                    _clipboardManager.SetClipboardText(options[0], true);
                    ShowNotification("Translation Complete", "Clipboard updated with translation");
                }
            }
            else
            {
                ShowNotification("Translation Failed", response.Error);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error in clipboard replace", ex);
            ShowNotification("Error", $"Translation failed: {ex.Message}");
        }
        finally
        {
            loadingPopup?.Close();
            loadingPopup?.Dispose();
        }
    }

    private List<string> ParseTranslationOptions(string text)
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

    private void ShowSelectionForm(List<string> options)
    {
        try
        {
            var selectionForm = new SelectionForm(options, _configService);
            selectionForm.ShowDialog();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error showing selection form", ex);
            
            // Fallback: copy first option
            _clipboardManager.SetClipboardText(options[0], true);
            ShowNotification("Translation Complete", $"Clipboard updated (showing {options.Count} options)");
        }
    }

    public async void ReadClipboardText(Language language)
    {
        try
        {
            var clipboardText = _clipboardManager.GetClipboardText();
            
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                ShowNotification("Clipboard is empty", "Cannot read empty clipboard");
                return;
            }

            if (language == Language.Persian)
            {
                await _ttsService.ReadPersianAsync(clipboardText);
            }
            else if (language == Language.English)
            {
                await _ttsService.ReadEnglishAsync(clipboardText);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error reading {language} text", ex);
            ShowNotification("Error", $"Failed to read text: {ex.Message}");
        }
    }

    public void AutoDetectAndTranslate()
    {
        try
        {
            var clipboardText = _clipboardManager.GetClipboardText();
            
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                ShowNotification("Clipboard is empty", "Cannot translate empty clipboard");
                return;
            }

            var language = _languageDetector.DetectLanguage(clipboardText);
            TranslationType type;

            if (language == Language.Persian)
            {
                type = TranslationType.PersianToEnglish;
            }
            else if (language == Language.English)
            {
                type = TranslationType.EnglishToPersian;
            }
            else
            {
                ShowNotification("Unknown Language", "Cannot detect language of clipboard text");
                return;
            }

            ShowPopupTranslation(type);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error in auto-detect translate", ex);
            ShowNotification("Error", $"Failed to translate: {ex.Message}");
        }
    }

    public async void AutoDetectAndRead()
    {
        try
        {
            var clipboardText = _clipboardManager.GetClipboardText();
            
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                ShowNotification("Clipboard is empty", "Cannot read empty clipboard");
                return;
            }

            var language = _languageDetector.DetectLanguage(clipboardText);

            if (language == Language.Persian)
            {
                await _ttsService.ReadPersianAsync(clipboardText);
            }
            else if (language == Language.English)
            {
                await _ttsService.ReadEnglishAsync(clipboardText);
            }
            else
            {
                ShowNotification("Unknown Language", "Cannot detect language of clipboard text");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error in auto-detect read", ex);
            ShowNotification("Error", $"Failed to read text: {ex.Message}");
        }
    }

    public void UndoClipboard()
    {
        try
        {
            if (_clipboardManager.HasPreviousContent())
            {
                _clipboardManager.UndoClipboard();
                ShowNotification("Undo Complete", "Clipboard restored to previous content");
            }
            else
            {
                ShowNotification("No History", "No previous clipboard content to restore");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error undoing clipboard", ex);
            ShowNotification("Error", $"Failed to undo clipboard: {ex.Message}");
        }
    }

    private void ShowNotification(string title, string message)
    {
        _loggingService.LogInformation($"Notification: {title} - {message}");
        _notificationService.ShowNotification(title, message);
    }
}

