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
    private readonly SelectionManager _selectionManager;
    private readonly IGrammarLearnerService _grammarLearnerService;

    public HotkeyActions(
        ITranslationService translationService,
        ITtsService ttsService,
        ILanguageDetector languageDetector,
        IConfigService configService,
        ILoggingService loggingService,
        INotificationService notificationService,
        ClipboardManager clipboardManager,
        SelectionManager selectionManager,
        IGrammarLearnerService grammarLearnerService)
    {
        _translationService = translationService;
        _ttsService = ttsService;
        _languageDetector = languageDetector;
        _configService = configService;
        _loggingService = loggingService;
        _notificationService = notificationService;
        _clipboardManager = clipboardManager;
        _selectionManager = selectionManager;
        _grammarLearnerService = grammarLearnerService;
    }

    /// <summary>
    /// Unified translation method that uses only selected text (no clipboard fallback)
    /// </summary>
    public async void Translate(TranslationType type)
    {
        LoadingPopupForm? loadingPopup = null;
        
        try
        {
            // Get selected text - this is the only source
            var selectedText = await _selectionManager.GetSelectedTextAsync();
            
            // If no text selected, show notification and return
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                ShowNotification("No text selected", "Please select text in the application first");
                return;
            }

            var hasSelectedText = true; // Always true since we only use selected text

            loadingPopup = new LoadingPopupForm();
            loadingPopup.ShowLoading("Translating...");

            TranslationResponse response;

            switch (type)
            {
                case TranslationType.PersianToEnglish:
                    response = await _translationService.TranslatePersianToEnglishAsync(selectedText);
                    break;
                case TranslationType.EnglishToPersian:
                    response = await _translationService.TranslateEnglishToPersianAsync(selectedText);
                    break;
                case TranslationType.GrammarFix:
                    response = await _translationService.FixGrammarAsync(selectedText);
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
                var options = TranslationHelper.ParseTranslationOptions(response.Text);

                if (options.Count > 1)
                {
                    // Multiple options: show unified popup form in selection mode
                    ShowUnifiedPopupForm(options, true, type, selectedText);
                }
                else
                {
                    // Single option: insert directly into the application
                    await _selectionManager.InsertTextAsync(options[0]);
                    ShowNotification("Translation Complete", "Text replaced in application");
                }
            }
            else
            {
                ShowNotification("Translation Failed", response.Error);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error in unified translate", ex);
            ShowNotification("Error", $"Translation failed: {ex.Message}");
        }
        finally
        {
            loadingPopup?.Close();
            loadingPopup?.Dispose();
        }
    }


    private void ShowUnifiedPopupForm(List<string> options, bool hasSelectedText, TranslationType type, string sourceText)
    {
        try
        {
            var popup = new TranslationPopupForm(
                _translationService,
                _ttsService,
                _languageDetector,
                _configService,
                _loggingService,
                _clipboardManager,
                _selectionManager, // Always pass SelectionManager since we only use selected text
                _grammarLearnerService); // Pass GrammarLearnerService for Learn button
            
            // Show with unified design - selection mode with pre-translated options
            popup.ShowPopup(sourceText, type, null, options, true);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error showing unified popup form", ex);
            ShowNotification("Error", $"Failed to show translation options: {ex.Message}");
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

    public async void AutoDetectAndTranslate()
    {
        try
        {
            // Get selected text - this is the only source
            var selectedText = await _selectionManager.GetSelectedTextAsync();
            
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                ShowNotification("No text selected", "Please select text in the application first");
                return;
            }

            var language = _languageDetector.DetectLanguage(selectedText);
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
                ShowNotification("Unknown Language", "Cannot detect language of selected text");
                return;
            }

            Translate(type);
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

