# AiTranslator - Publish to EXE Script
# This script builds and publishes the application to a single EXE file

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AiTranslator - Publish to EXE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to the project directory
$projectDir = Join-Path $PSScriptRoot "AiTranslator"
Set-Location $projectDir

Write-Host "Building and publishing..." -ForegroundColor Yellow
Write-Host ""

# Run publish command
$publishResult = dotnet publish `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output ..\bin\portable `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  SUCCESS: Build completed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    
    $exePath = Join-Path $PSScriptRoot "bin\portable\AiTranslator.exe"
    
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        
        Write-Host "Output location: $exePath" -ForegroundColor Green
        Write-Host "File size: $fileSizeMB MB" -ForegroundColor Green
        Write-Host "Last modified: $($fileInfo.LastWriteTime)" -ForegroundColor Green
        Write-Host ""
        Write-Host "You can now run: $exePath" -ForegroundColor Cyan
    } else {
        Write-Host "ERROR: EXE file not found!" -ForegroundColor Red
    }
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ERROR: Build failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

