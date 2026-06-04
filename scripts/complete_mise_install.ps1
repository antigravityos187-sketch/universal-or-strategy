# Complete Mise Installation
# Moves mise.exe from temp to permanent location and adds to PATH

$ErrorActionPreference = "Stop"

$installDir = "$env:LOCALAPPDATA\Programs\mise"
$sourcePath = "$env:TEMP\mise\mise\bin\mise.exe"
$destPath = "$installDir\mise.exe"

# Create install directory
Write-Host "Creating installation directory: $installDir"
New-Item -ItemType Directory -Path $installDir -Force | Out-Null

# Copy mise.exe
Write-Host "Copying mise.exe from $sourcePath to $destPath"
Copy-Item -Path $sourcePath -Destination $destPath -Force

# Verify installation
if (Test-Path $destPath) {
    Write-Host "✅ Mise installed successfully to: $destPath"
    
    # Check if already in PATH
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($currentPath -notlike "*$installDir*") {
        Write-Host "Adding $installDir to user PATH..."
        [Environment]::SetEnvironmentVariable(
            "Path",
            "$currentPath;$installDir",
            "User"
        )
        Write-Host "✅ Added to PATH (restart terminal to use 'mise' command)"
    } else {
        Write-Host "✅ Already in PATH"
    }
    
    # Clean up temp
    Write-Host "Cleaning up temporary files..."
    Remove-Item -Path "$env:TEMP\mise" -Recurse -Force -ErrorAction SilentlyContinue
    
    # Test mise
    Write-Host "`nTesting mise installation..."
    & $destPath --version
    
    Write-Host "`n✅ Installation complete!"
    Write-Host "Run 'mise --version' in a new terminal to verify"
} else {
    Write-Host "❌ Installation failed - mise.exe not found at $destPath"
    exit 1
}

# Made with Bob
