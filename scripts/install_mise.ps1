# Install Mise v2026.6.0 for Windows
$ErrorActionPreference = "Stop"

$version = "v2026.6.0"
$url = "https://github.com/jdx/mise/releases/download/$version/mise-$version-windows-x64.zip"
$tempZip = "$env:TEMP\mise.zip"
$tempDir = "$env:TEMP\mise"
$installDir = "$env:LOCALAPPDATA\Programs\mise"

Write-Host "Downloading Mise $version from GitHub..."
Write-Host "URL: $url"
Invoke-WebRequest -Uri $url -OutFile $tempZip
Write-Host "Download complete"

Write-Host "Extracting archive..."
Expand-Archive -Path $tempZip -DestinationPath $tempDir -Force
Write-Host "Extraction complete"

Write-Host "Installing to $installDir..."
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Move-Item -Path "$tempDir\mise.exe" -Destination "$installDir\mise.exe" -Force
Write-Host "Installation complete"

Write-Host "Cleaning up temporary files..."
Remove-Item -Path $tempZip, $tempDir -Recurse -Force
Write-Host "Cleanup complete"

Write-Host ""
Write-Host "Mise installed successfully to: $installDir\mise.exe"
Write-Host "Add to PATH: `$env:PATH += ';$installDir'"

# Made with Bob
