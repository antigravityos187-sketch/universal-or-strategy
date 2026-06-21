#!/usr/bin/env pwsh
# Fix Threshold 15->8 and Make Custom Modes Phase-Specific
# Created: 2026-06-17

Write-Host "=== Threshold 15->8 Fix + Custom Mode Update ===" -ForegroundColor Cyan
Write-Host ""

$filesChanged = 0

# Step 1: Update AGENTS.md
Write-Host "Step 1: Updating AGENTS.md..." -ForegroundColor Yellow
$agentsPath = "AGENTS.md"
if (Test-Path $agentsPath) {
    $content = Get-Content $agentsPath -Raw
    $content = $content -replace 'CYC <= 15', 'CYC <= 8'
    $content = $content -replace 'threshold 15', 'threshold 8'
    $content = $content -replace 'complexity <= 15', 'complexity <= 8'
    $content = $content -replace '>15 are harder', '>8 are harder'
    $content = $content -replace 'V12 uses CYC <= 15', 'V12 uses CYC <= 8'
    Set-Content -Path $agentsPath -Value $content -NoNewline
    Write-Host "  Updated: AGENTS.md" -ForegroundColor Green
    $filesChanged++
}

# Step 2: Update .codacy.yml
Write-Host "Step 2: Updating .codacy.yml..." -ForegroundColor Yellow
$codacyPath = ".codacy.yml"
if (Test-Path $codacyPath) {
    $content = Get-Content $codacyPath -Raw
    $content = $content -replace 'threshold: 15', 'threshold: 8'
    $content = $content -replace 'complexity <=15', 'complexity <=8'
    Set-Content -Path $codacyPath -Value $content -NoNewline
    Write-Host "  Updated: .codacy.yml" -ForegroundColor Green
    $filesChanged++
}

# Step 3: Update .coderabbit.yaml
Write-Host "Step 3: Updating .coderabbit.yaml..." -ForegroundColor Yellow
$coderabbitPath = ".coderabbit.yaml"
if (Test-Path $coderabbitPath) {
    $content = Get-Content $coderabbitPath -Raw
    $content = $content -replace 'complexity >15', 'complexity >8'
    Set-Content -Path $coderabbitPath -Value $content -NoNewline
    Write-Host "  Updated: .coderabbit.yaml" -ForegroundColor Green
    $filesChanged++
}

# Step 4: Update .codeant.yml
Write-Host "Step 4: Updating .codeant.yml..." -ForegroundColor Yellow
$codeantPath = ".codeant.yml"
if (Test-Path $codeantPath) {
    $content = Get-Content $codeantPath -Raw
    $content = $content -replace 'complexity_threshold: 15', 'complexity_threshold: 8'
    Set-Content -Path $codeantPath -Value $content -NoNewline
    Write-Host "  Updated: .codeant.yml" -ForegroundColor Green
    $filesChanged++
}

# Step 5: Update pre_push_validation.ps1
Write-Host "Step 5: Updating scripts/pre_push_validation.ps1..." -ForegroundColor Yellow
$prePushPath = "scripts/pre_push_validation.ps1"
if (Test-Path $prePushPath) {
    $content = Get-Content $prePushPath -Raw
    $content = $content -replace 'CYC <= 15', 'CYC <= 8'
    $content = $content -replace '--threshold 15', '--threshold 8'
    $content = $content -replace 'Complexity \(<=15\)', 'Complexity (<=8)'
    $content = $content -replace 'CYC 15 threshold', 'CYC 8 threshold'
    Set-Content -Path $prePushPath -Value $content -NoNewline
    Write-Host "  Updated: scripts/pre_push_validation.ps1" -ForegroundColor Green
    $filesChanged++
}

# Step 6: Update .bob/custom_modes.yaml
Write-Host "Step 6: Updating .bob/custom_modes.yaml..." -ForegroundColor Yellow
$customModesPath = ".bob/custom_modes.yaml"
if (Test-Path $customModesPath) {
    $content = Get-Content $customModesPath -Raw
    
    # Remove wave-specific references
    $content = $content -replace 'Wave 5 Execution Lead', 'Phase Execution Lead'
    $content = $content -replace 'Wave 5', 'V12'
    $content = $content -replace 'wave 5', 'phase'
    $content = $content -replace 'wave execution', 'phase execution'
    $content = $content -replace 'Wave orchestration', 'Phase orchestration'
    
    # Update threshold
    $content = $content -replace 'CYC <= 15', 'CYC <= 8'
    $content = $content -replace 'complexity <= 15', 'complexity <= 8'
    
    Set-Content -Path $customModesPath -Value $content -NoNewline
    Write-Host "  Updated: .bob/custom_modes.yaml" -ForegroundColor Green
    $filesChanged++
}

# Step 7: Add davidgreen77 API
Write-Host "Step 7: Adding davidgreen77 API to rotation..." -ForegroundColor Yellow

$apiDir = "docs/API"
if (-not (Test-Path $apiDir)) {
    New-Item -ItemType Directory -Path $apiDir -Force | Out-Null
}

$rotationPath = Join-Path $apiDir "api_rotation.json"
if (Test-Path $rotationPath) {
    $rotation = Get-Content $rotationPath -Raw | ConvertFrom-Json
    $exists = $rotation.apis | Where-Object { $_.name -eq "davidgreen77" }
    if (-not $exists) {
        $newApi = @{
            name = "davidgreen77"
            bobcoins = 160
            createdAt = "2026-06-17T21:22:42.310803Z"
            status = "active"
        }
        $rotation.apis += $newApi
        $rotation | ConvertTo-Json -Depth 10 | Set-Content $rotationPath
        Write-Host "  Added davidgreen77 to api_rotation.json" -ForegroundColor Green
        $filesChanged++
    } else {
        Write-Host "  davidgreen77 already exists in rotation" -ForegroundColor Yellow
    }
} else {
    $newRotation = @{
        version = "1.0"
        updated = (Get-Date -Format "yyyy-MM-ddTHH:mm:ss.fffZ")
        apis = @(
            @{
                name = "davidgreen77"
                bobcoins = 160
                createdAt = "2026-06-17T21:22:42.310803Z"
                status = "active"
            }
        )
    }
    $newRotation | ConvertTo-Json -Depth 10 | Set-Content $rotationPath
    Write-Host "  Created api_rotation.json with davidgreen77" -ForegroundColor Green
    $filesChanged++
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Files updated: $filesChanged" -ForegroundColor Green
Write-Host "Threshold changes: 15 -> 8" -ForegroundColor Green
Write-Host "Custom modes: Wave-specific -> Phase-specific" -ForegroundColor Green
Write-Host "API added: davidgreen77 (160 bobcoins)" -ForegroundColor Green
Write-Host ""
Write-Host "Fix complete!" -ForegroundColor Green

# Made with Bob
