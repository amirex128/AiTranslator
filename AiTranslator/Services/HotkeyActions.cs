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
    
    // Track the current popup form to close it before opening a new one
    private TranslationPopupForm? _currentPopupForm;
    private readonly object _popupLock = new object();

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
    /// Unified translation method that uses clipboard content
    /// </summary>
    public async void Translate(TranslationType type)
    {
        LoadingPopupForm? loadingPopup = null;
        
        try
        {
            // Close any existing popup form to prevent showing old data
            await CloseExistingPopupFormAsync();
            
            // Get text from clipboard - read it fresh each time
            var clipboardText = await _selectionManager.GetSelectedTextAsync();
            
            // If clipboard is empty, show notification and return
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                ShowNotification("Clipboard is empty", "Please copy text to clipboard first");
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
                case TranslationType.SentenceSuggestion:
                    response = await _translationService.SuggestSentenceAsync(clipboardText);
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

                // Always show unified popup form so user can use "Translate En to Fa" button
                ShowUnifiedPopupForm(options, true, type, clipboardText);
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


    private async void ShowUnifiedPopupForm(List<string> options, bool hasSelectedText, TranslationType type, string sourceText)
    {
        try
        {
            // Close any existing popup form first
            await CloseExistingPopupFormAsync();
            
            TranslationPopupForm? popup = null;
            
            lock (_popupLock)
            {
                popup = new TranslationPopupForm(
                    _translationService,
                    _ttsService,
                    _languageDetector,
                    _configService,
                    _loggingService,
                    _clipboardManager,
                    _selectionManager,
                    _grammarLearnerService);
                
                // Store reference to the new popup form
                _currentPopupForm = popup;
            }
            
            // Handle form closed event to clear reference
            popup.FormClosed += OnPopupFormClosed;
            
            // Show with unified design - selection mode with pre-translated options
            popup.ShowPopup(sourceText, type, null, options, true);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error showing unified popup form", ex);
            ShowNotification("Error", $"Failed to show translation options: {ex.Message}");
        }
    }
    
    private void OnPopupFormClosed(object? sender, FormClosedEventArgs e)
    {
        lock (_popupLock)
        {
            if (_currentPopupForm == sender)
            {
                _currentPopupForm = null;
            }
        }
    }
    
    /// <summary>
    /// Closes any existing popup form to prevent showing old data
    /// </summary>
    private async Task CloseExistingPopupFormAsync()
    {
        TranslationPopupForm? formToClose = null;
        
        lock (_popupLock)
        {
            if (_currentPopupForm != null && !_currentPopupForm.IsDisposed)
            {
                formToClose = _currentPopupForm;
                _currentPopupForm = null;
            }
        }
        
        if (formToClose != null)
        {
            try
            {
                // Unsubscribe from events to prevent memory leaks
                formToClose.FormClosed -= OnPopupFormClosed;
                
                // Close and dispose on UI thread
                if (formToClose.InvokeRequired)
                {
                    formToClose.Invoke(new Action(() =>
                    {
                        try
                        {
                            if (!formToClose.IsDisposed)
                            {
                                formToClose.Close();
                                formToClose.Dispose();
                            }
                        }
                        catch
                        {
                            // Ignore disposal errors
                        }
                    }));
                }
                else
                {
                    if (!formToClose.IsDisposed)
                    {
                        formToClose.Close();
                        formToClose.Dispose();
                    }
                }
                
                // Give it a moment to fully close (non-blocking)
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error closing existing popup form", ex);
            }
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

    private void ShowNotification(string title, string message)
    {
        _loggingService.LogInformation($"Notification: {title} - {message}");
        _notificationService.ShowNotification(title, message);
    }
}

