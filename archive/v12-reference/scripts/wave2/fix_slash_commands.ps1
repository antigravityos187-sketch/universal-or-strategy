# Fix Wave 2 Scripts to Use Slash Commands
# Replaces --chat-mode with proper slash commands

$ErrorActionPreference = "Stop"

Write-Host "=== Wave 2 Slash Command Fix ===" -ForegroundColor Cyan
Write-Host ""

$totalFixed = 0

# Phase 1: epic-intake
Write-Host "Processing Phase 1..." -ForegroundColor Yellow
$p1Files = Get-ChildItem -Path . -Filter "_p1_*.sh"
$p1Fixed = 0
foreach ($file in $p1Files) {
    if ($file.Name -match '_p1_(\d+)\.sh') {
        $epicNum = $matches[1]
        $content = Get-Content $file.FullName -Raw
        $oldCmd = "bob --yolo --chat-mode plan `"`$(cat /tmp/phase1_msg_$epicNum.txt)`""
        $newCmd = "bob --yolo /epic-intake EPIC-CCN-$epicNum"
        if ($content -match [regex]::Escape($oldCmd)) {
            $content = $content.Replace($oldCmd, $newCmd)
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
            $p1Fixed++
        }
    }
}
Write-Host "  Phase 1: $p1Fixed files fixed" -ForegroundColor Cyan
$totalFixed += $p1Fixed
Write-Host ""

# Phase 1.5: epic-scope-boundary
Write-Host "Processing Phase 1.5..." -ForegroundColor Yellow
$p15Files = Get-ChildItem -Path . -Filter "_p1_5_*.sh"
$p15Fixed = 0
foreach ($file in $p15Files) {
    if ($file.Name -match '_p1_5_(\d+)\.sh') {
        $epicNum = $matches[1]
        $content = Get-Content $file.FullName -Raw
        $oldCmd = "bob --yolo --chat-mode plan `"`$(cat /tmp/phase1_5_msg_$epicNum.txt)`""
        $newCmd = "bob --yolo /epic-scope-boundary EPIC-CCN-$epicNum --phase 1.5"
        if ($content -match [regex]::Escape($oldCmd)) {
            $content = $content.Replace($oldCmd, $newCmd)
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
            $p15Fixed++
        }
    }
}
Write-Host "  Phase 1.5: $p15Fixed files fixed" -ForegroundColor Cyan
$totalFixed += $p15Fixed
Write-Host ""

# Phase 2: epic-plan
Write-Host "Processing Phase 2..." -ForegroundColor Yellow
$p2Files = Get-ChildItem -Path . -Filter "_p2_*.sh"
$p2Fixed = 0
foreach ($file in $p2Files) {
    if ($file.Name -match '_p2_(\d+)\.sh') {
        $epicNum = $matches[1]
        $content = Get-Content $file.FullName -Raw
        $oldCmd = "bob --yolo --chat-mode plan `"`$(cat /tmp/phase2_msg_$epicNum.txt)`""
        $newCmd = "bob --yolo /epic-plan EPIC-CCN-$epicNum"
        if ($content -match [regex]::Escape($oldCmd)) {
            $content = $content.Replace($oldCmd, $newCmd)
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
            $p2Fixed++
        }
    }
}
Write-Host "  Phase 2: $p2Fixed files fixed" -ForegroundColor Cyan
$totalFixed += $p2Fixed
Write-Host ""

Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Total files fixed: $totalFixed" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Verify changes: git diff _p*.sh"
Write-Host "2. Deploy to VM"
Write-Host "3. Run phases sequentially"

# Made with Bob
