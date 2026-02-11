# Rovo Direct-Link Launcher
# 1. Jump to correct Project Root
Set-Location "C:\WSGTA\universal-or-strategy"

# 2. Force Environment Protections
$env:PYTHONUTF8 = 1
$env:PYTHONIOENCODING = "utf-8:replace"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 3. Visual Confirmation
Write-Host "---" -ForegroundColor Green
Write-Host "PATH FIXED: $(Get-Location)" -ForegroundColor Green
Write-Host "ENCODING PROTECTIONS: ACTIVE" -ForegroundColor Green
Write-Host "---" -ForegroundColor Green

# 4. Launch Rovo
.\acli.exe rovodev run
