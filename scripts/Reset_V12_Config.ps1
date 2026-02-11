Write-Host "Resetting V12 Config..."
$configPath = "$env:USERPROFILE\Documents\NinjaTrader 8\bin\Custom\Global\V12_Config"
if (Test-Path $configPath) {
    Remove-Item $configPath -Recurse -Force
    Write-Host "Config cleared."
} else {
    Write-Host "No config found to clear."
}
Write-Host "Please restart NinjaTrader to load the default V12 UI layout."
