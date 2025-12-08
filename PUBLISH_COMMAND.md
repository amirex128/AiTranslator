# دستور Publish برای AiTranslator

## دستور اصلی

```bash
cd d:\projects\charp\AiTranslator\AiTranslator
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output ..\bin\portable -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

## دستور کوتاه شده

```bash
cd d:\projects\charp\AiTranslator\AiTranslator
dotnet publish -c Release -r win-x64 --self-contained -o ..\bin\portable -p:PublishSingleFile=true
```

## توضیحات پارامترها

- `--configuration Release` یا `-c Release`: Build در حالت Release (بهینه‌سازی شده)
- `--runtime win-x64` یا `-r win-x64`: Runtime برای Windows 64-bit
- `--self-contained true`: شامل تمام dependencies (portable - بدون نیاز به نصب .NET)
- `--output ..\bin\portable` یا `-o ..\bin\portable`: مسیر خروجی فایل EXE
- `-p:PublishSingleFile=true`: ایجاد یک فایل EXE واحد (همه چیز در یک فایل)
- `-p:IncludeNativeLibrariesForSelfExtract=true`: شامل کتابخانه‌های native
- `-p:EnableCompressionInSingleFile=true`: فشرده‌سازی فایل (کوچکتر می‌شود)

## استفاده از فایل‌های Script

### Windows Batch (.bat)
```bash
publish.bat
```

### PowerShell (.ps1)
```powershell
.\publish.ps1
```

## مسیر خروجی

فایل EXE در مسیر زیر ساخته می‌شود:
```
d:\projects\charp\AiTranslator\bin\portable\AiTranslator.exe
```

## نکات مهم

1. **اولین بار**: ممکن است چند دقیقه طول بکشد (دانلود runtime)
2. **دفعات بعدی**: سریع‌تر است (runtime در cache است)
3. **حجم فایل**: معمولاً بین 50-100 MB (بسته به dependencies)
4. **قابل حمل**: فایل EXE به تنهایی قابل اجرا است (بدون نیاز به نصب .NET)

## عیب‌یابی

### خطا: Runtime not found
```bash
# نصب runtime
dotnet --list-runtimes
```

### خطا: Build failed
```bash
# پاک کردن و rebuild
dotnet clean
dotnet restore
dotnet build -c Release
```

### بررسی فایل خروجی
```powershell
Test-Path "d:\projects\charp\AiTranslator\bin\portable\AiTranslator.exe"
```

