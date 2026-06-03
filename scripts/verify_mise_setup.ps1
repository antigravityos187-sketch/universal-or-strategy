# V12 Universal OR Strategy - Mise Setup Verification
# Verifies Mise installation and configuration

param(
    [switch]$Install,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "=== V12 Mise Setup Verification ===" -ForegroundColor Cyan
Write-Host ""

# Check if Mise is installed
function Test-MiseInstalled {
    try {
        $version = mise --version 2>$null
        if ($version) {
            Write-Host "✅ Mise installed: $version" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "❌ Mise not installed" -ForegroundColor Red
        return $false
    }
    return $false
}

# Install Mise
function Install-Mise {
    Write-Host "Installing Mise..." -ForegroundColor Yellow
    try {
        irm https://mise.jdx.dev/install.ps1 | iex
        Write-Host "✅ Mise installed successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "⚠️  IMPORTANT: Restart your terminal for PATH changes to take effect" -ForegroundColor Yellow
        return $true
    }
    catch {
        Write-Host "❌ Failed to install Mise: $_" -ForegroundColor Red
        return $false
    }
}

# Verify configuration files
function Test-ConfigFiles {
    Write-Host "Checking configuration files..." -ForegroundColor Cyan
    
    $files = @(
        ".mise.toml",
        "requirements.txt",
        "package.json",
        ".mise/hooks/pre-commit"
    )
    
    $allExist = $true
    foreach ($file in $files) {
        if (Test-Path $file) {
            Write-Host "  ✅ $file" -ForegroundColor Green
        }
        else {
            Write-Host "  ❌ $file missing" -ForegroundColor Red
            $allExist = $false
        }
    }
    
    return $allExist
}

# Verify .gitignore
function Test-GitIgnore {
    Write-Host "Checking .gitignore..." -ForegroundColor Cyan
    
    $content = Get-Content .gitignore -Raw
    if ($content -match "\.mise\.local\.toml" -and $content -match "\.mise/") {
        Write-Host "  ✅ .gitignore configured" -ForegroundColor Green
        return $true
    }
    else {
        Write-Host "  ❌ .gitignore missing Mise entries" -ForegroundColor Red
        return $false
    }
}

# Main execution
$miseInstalled = Test-MiseInstalled

if (-not $miseInstalled) {
    if ($Install) {
        $installed = Install-Mise
        if (-not $installed) {
            exit 1
        }
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Restart your terminal" -ForegroundColor White
        Write-Host "2. Run: mise install" -ForegroundColor White
        Write-Host "3. Run: mise run setup" -ForegroundColor White
        exit 0
    }
    else {
        Write-Host ""
        Write-Host "To install Mise, run:" -ForegroundColor Yellow
        Write-Host "  .\scripts\verify_mise_setup.ps1 -Install" -ForegroundColor White
        Write-Host ""
        Write-Host "Or manually:" -ForegroundColor Yellow
        Write-Host "  irm https://mise.jdx.dev/install.ps1 | iex" -ForegroundColor White
        exit 1
    }
}

Write-Host ""
$configOk = Test-ConfigFiles

Write-Host ""
$gitignoreOk = Test-GitIgnore

Write-Host ""
Write-Host "=== Configuration Summary ===" -ForegroundColor Cyan
Write-Host "Mise Installed: $(if ($miseInstalled) { '✅' } else { '❌' })"
Write-Host "Config Files: $(if ($configOk) { '✅' } else { '❌' })"
Write-Host ".gitignore: $(if ($gitignoreOk) { '✅' } else { '❌' })"

if ($miseInstalled -and $configOk -and $gitignoreOk) {
    Write-Host ""
    Write-Host "✅ Mise setup complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Run: mise install" -ForegroundColor White
    Write-Host "2. Run: mise run setup" -ForegroundColor White
    Write-Host "3. Run: mise run doctor" -ForegroundColor White
    exit 0
}
else {
    Write-Host ""
    Write-Host "❌ Setup incomplete. Fix issues above." -ForegroundColor Red
    exit 1
}

# Made with Bob
