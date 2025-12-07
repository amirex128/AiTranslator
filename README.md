# AI Translator

A comprehensive Windows desktop application for AI-powered translation with global hotkey support, system tray integration, and text-to-speech capabilities.

## Features

### Core Translation Features
- **Three Translation Modes**:
  - Persian to English translation
  - English to Persian translation
  - Grammar correction for English text

### User Interface
- **Smart Input Behavior**: When typing in one input box, the other two automatically collapse to save space
- **Result History**: Tabbed interface to view multiple translation results
- **Character/Word Counter**: Real-time statistics for each input
- **RTL Support**: Automatic right-to-left text direction for Persian text
- **Collapsible Shortcuts Panel**: Quick reference for all keyboard shortcuts

### Global Hotkeys (Customizable)
The application supports 11 customizable global hotkeys:

1. **Popup Translations** (3 hotkeys):
   - FA→EN popup: Ctrl+Alt+F1
   - EN→FA popup: Ctrl+Alt+F2
   - Grammar Fix popup: Ctrl+Alt+F3

2. **Clipboard Replace** (3 hotkeys):
   - FA→EN replace: Ctrl+Alt+F4
   - EN→FA replace: Ctrl+Alt+F5
   - Grammar Fix replace: Ctrl+Alt+F6

3. **Text-to-Speech** (2 hotkeys):
   - Read Persian: Ctrl+Alt+F7
   - Read English: Ctrl+Alt+F8

4. **Auto-Detect** (2 hotkeys):
   - Auto-detect translate: Ctrl+Alt+F9
   - Auto-detect read: Ctrl+Alt+F10

5. **Undo**:
   - Undo clipboard: Ctrl+Alt+Shift+Z

### System Integration
- **System Tray**: Minimize to system tray instead of closing
- **Auto-start**: Optional startup with Windows
- **Single Instance**: Only one instance can run at a time
- **Window State Memory**: Remembers window size and position

### Advanced Features
- **Configurable API Endpoints**: Easily change translation and TTS service URLs
- **Retry Logic**: Automatic retry with exponential backoff for failed API calls
- **Timeout Management**: Configurable 5-minute timeout for API requests
- **Comprehensive Logging**: File-based logging with rotation
- **Error Notifications**: Toast notifications for errors and completions

## Configuration

All settings are stored in `appsettings.json`. You can edit this file directly or use the Settings dialog in the application.

### Configuration Sections

- **ApiEndpoints**: URLs for translation services
- **TtsEndpoints**: URLs for text-to-speech services
- **Hotkeys**: Customize all keyboard shortcuts
- **Window**: Default size, position, and behavior
- **Api**: Timeout, retry count, and delay settings
- **Ui**: Popup timeout, notifications, character count display
- **Logging**: Log level, directory, and file management
- **StartWithWindows**: Auto-start configuration

## System Requirements

- Windows 10 or later
- .NET 10.0 Runtime
- Internet connection for API access

## API Configuration

The application requires three API endpoints for translation:

1. **English to Persian**: `/api/v1/prediction/2d8919bf-3426-4cf2-9a95-11f2539acff6`
2. **Persian to English**: `/api/v1/prediction/1e86b0ba-1193-42e3-b0ac-d56720689b0f`
3. **Grammar Fix**: `/api/v1/prediction/2195ec5a-7b27-4fbb-8384-7c4765a6ae06`

TTS endpoints can be configured when available.

## How to Use

### Main Window
1. Type or paste text into one of the three input boxes
2. Click "Translate" to translate the text
3. Click "Read" to have the text read aloud (when TTS is configured)
4. Results appear in the tabbed result panel at the bottom
5. Click "Copy to Clipboard" to copy the current result

### Quick Translation (Popup)
1. Copy text to clipboard
2. Press the appropriate hotkey (e.g., Ctrl+Alt+F9 for auto-detect)
3. A popup window appears with the translation
4. The popup auto-closes after 10 seconds (configurable)

### Clipboard Replace
1. Copy text to clipboard
2. Press the appropriate hotkey (e.g., Ctrl+Alt+F4 for FA→EN)
3. A loading indicator appears
4. Clipboard is automatically replaced with the translation
5. A notification confirms completion

## Building from Source

```bash
# Clone the repository
git clone <repository-url>

# Navigate to project directory
cd AiTranslator/AiTranslator

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## Project Structure

```
AiTranslator/
├── Models/              # Data models and DTOs
├── Services/            # Core business logic
│   ├── ConfigService
│   ├── TranslationService
│   ├── TtsService
│   ├── LanguageDetector
│   ├── LoggingService
│   └── NotificationService
├── Utilities/           # Helper classes
│   ├── HotkeyManager
│   ├── ClipboardManager
│   ├── SingleInstanceManager
│   └── AutoStartManager
├── Forms/               # UI components
│   ├── MainForm
│   ├── TranslationPopupForm
│   ├── LoadingPopupForm
│   └── SettingsForm
└── appsettings.json     # Configuration file
```

## Architecture

The application follows SOLID principles and clean architecture:

- **Separation of Concerns**: Services, UI, and utilities are clearly separated
- **Dependency Injection**: Manual DI in Program.cs for service initialization
- **Interface-based Design**: All services implement interfaces for testability
- **Error Handling**: Comprehensive try-catch blocks with logging
- **Async/Await**: Proper asynchronous operations for API calls

## Troubleshooting

### Hotkeys Not Working
- Check if another application is using the same key combination
- Try changing the hotkey in Settings
- Run the application as administrator if needed

### API Connection Errors
- Verify API endpoints in Settings
- Check internet connection
- Review logs in the Logs directory

### Application Won't Start
- Check if another instance is already running (look in system tray)
- Review error logs
- Verify .NET 10.0 Runtime is installed

## Credits

Developed with .NET 10.0 and Windows Forms.

