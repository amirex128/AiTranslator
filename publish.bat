@echo off
echo ========================================
echo   AiTranslator - Publish to EXE
echo ========================================
echo.

cd /d "%~dp0AiTranslator"

echo Building and publishing...
echo.

dotnet publish ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output ..\bin\portable ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   SUCCESS: Build completed!
    echo ========================================
    echo.
    echo Output location: ..\bin\portable\AiTranslator.exe
    echo.
    
    if exist "..\bin\portable\AiTranslator.exe" (
        for %%A in ("..\bin\portable\AiTranslator.exe") do (
            echo File size: %%~zA bytes
            echo File size: %%~zA / 1048576 MB
        )
        echo.
        echo You can now run: ..\bin\portable\AiTranslator.exe
    ) else (
        echo ERROR: EXE file not found!
    )
) else (
    echo.
    echo ========================================
    echo   ERROR: Build failed!
    echo ========================================
    echo.
)

pause

