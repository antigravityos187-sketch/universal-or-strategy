# Generate Wave 5 Phase 5 Scripts (Building-Blocks Method)
# V12.44 Protocol: Copy Wave 4 scripts, add --chat-mode v12-engineer flag
# Exclude: EPIC-CCN-024 (local execution), EPIC-CCN-027 (skip - invalid target)

$ErrorActionPreference = "Stop"

Write-Host "=== Wave 5 Script Generation (Building-Blocks Method) ===" -ForegroundColor Cyan
Write-Host "Source: scripts/wave4/_p5_*.sh" -ForegroundColor Gray
Write-Host "Target: scripts/wave5/_p5_EPIC-CCN-*.sh" -ForegroundColor Gray
Write-Host "Exclusions: 024 (local), 027 (skip)" -ForegroundColor Yellow
Write-Host ""

# Create wave5 directory if it doesn't exist
$wave5Dir = "scripts/wave5"
if (-not (Test-Path $wave5Dir)) {
    New-Item -ItemType Directory -Path $wave5Dir | Out-Null
    Write-Host "Created directory: $wave5Dir" -ForegroundColor Green
}

# Epic list (001-080, excluding 024 and 027)
$epics = 1..80 | Where-Object { $_ -ne 24 -and $_ -ne 27 }
$generated = 0
$skipped = 0

foreach ($epicNum in $epics) {
    $epicId = "EPIC-CCN-{0:D3}" -f $epicNum
    $wave4Script = "scripts/wave4/_p5_{0:D3}.sh" -f $epicNum
    $wave5Script = "scripts/wave5/_p5_$epicId.sh"
    
    # Check if Wave 4 script exists
    if (-not (Test-Path $wave4Script)) {
        Write-Host "  [SKIP] $epicId - Wave 4 script not found" -ForegroundColor Yellow
        $skipped++
        continue
    }
    
    # Read Wave 4 script
    $content = Get-Content $wave4Script -Raw
    
    # Find the bob command line (should be around line 55)
    # OLD: bob --yolo "$(cat /tmp/phase5_msg_XXX.txt)"
    # NEW: bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_XXX.txt)"
    
    # Replace bob command to add mode flag
    $content = $content -replace 'bob --yolo "', 'bob --yolo --chat-mode v12-engineer "'
    
    # Verify the replacement worked
    if ($content -notmatch '--chat-mode v12-engineer') {
        Write-Host "  [ERROR] $epicId - Failed to add mode flag" -ForegroundColor Red
        continue
    }
    
    # Write Wave 5 script
    Set-Content -Path $wave5Script -Value $content -NoNewline
    
    Write-Host "  [OK] $epicId - Generated with mode flag" -ForegroundColor Green
    $generated++
}

Write-Host ""
Write-Host "=== Generation Complete ===" -ForegroundColor Cyan
Write-Host "Generated: $generated scripts" -ForegroundColor Green
Write-Host "Skipped: $skipped scripts" -ForegroundColor Yellow
Write-Host "Expected: 77 scripts (78 epics - 024 local - 027 skip)" -ForegroundColor Gray

# Verify count
$wave5Count = (Get-ChildItem "$wave5Dir/_p5_EPIC-CCN-*.sh").Count
Write-Host ""
Write-Host "Verification:" -ForegroundColor Cyan
Write-Host "  Wave 5 scripts: $wave5Count" -ForegroundColor $(if ($wave5Count -eq 77) { "Green" } else { "Red" })
Write-Host "  Target: 77" -ForegroundColor Gray

if ($wave5Count -eq 77) {
    Write-Host ""
    Write-Host "SUCCESS: All 77 scripts generated!" -ForegroundColor Green
    Write-Host "Next: Upload to VM with verification (V12.27 protocol)" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "ERROR: Script count mismatch!" -ForegroundColor Red
    Write-Host "Expected 77, got $wave5Count" -ForegroundColor Red
    exit 1
}

# Made with Bob
