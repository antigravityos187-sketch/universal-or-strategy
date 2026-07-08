# verify_phase_completion.ps1
# Universal Phase Verification Script (V12.26)
# Prevents false negatives by checking ALL file patterns

param(
    [Parameter(Mandatory=$true)]
    [int]$Phase,
    
    [Parameter(Mandatory=$true)]
    [int[]]$Epics
)

$ErrorActionPreference = "Stop"

function Verify-Phase {
    param(
        [int]$Phase,
        [int[]]$Epics
    )
    
    $expectedCount = $Epics.Count
    Write-Host "`n=== Verifying Phase $Phase for $expectedCount epics ===" -ForegroundColor Cyan
    
    # Step 1: Check manifest status
    Write-Host "`n[1/4] Checking manifest.json status..." -ForegroundColor Yellow
    $manifestComplete = 0
    $manifestMissing = @()
    
    foreach ($epic in $Epics) {
        $manifestPath = "docs/brain/EPIC-CCN-$epic/manifest.json"
        if (Test-Path $manifestPath) {
            try {
                $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
                $phaseStatus = $manifest.phases | Where-Object { $_.phase -eq $Phase.ToString() } | Select-Object -ExpandProperty status -ErrorAction SilentlyContinue
                if ($phaseStatus -eq "completed") {
                    $manifestComplete++
                    Write-Host "  ✅ EPIC-CCN-$epic : completed" -ForegroundColor Green
                } else {
                    Write-Host "  ⚠️  EPIC-CCN-$epic : $phaseStatus" -ForegroundColor Yellow
                    $manifestMissing += $epic
                }
            } catch {
                Write-Host "  ❌ EPIC-CCN-$epic : manifest parse error" -ForegroundColor Red
                $manifestMissing += $epic
            }
        } else {
            Write-Host "  ❌ EPIC-CCN-$epic : manifest not found" -ForegroundColor Red
            $manifestMissing += $epic
        }
    }
    Write-Host "`nManifest Summary: $manifestComplete/$expectedCount completed" -ForegroundColor Cyan
    
    # Step 2: Check file existence (ALL patterns)
    Write-Host "`n[2/4] Checking file existence (multi-pattern)..." -ForegroundColor Yellow
    $fileCount = 0
    $patterns = @()
    
    switch ($Phase) {
        0 {
            $patterns = @("00-hotspots.md")
        }
        1 {
            $patterns = @("*scope.md")
        }
        2 {
            $patterns = @("*implementation-plan.md", "*architecture*.md", "*acceptance*.md")
        }
        3 {
            $patterns = @("*audit*.md")
        }
        4 {
            $patterns = @("*tickets*.md")
        }
        5 {
            $patterns = @("ticket-*-completion.md")
        }
        6 {
            $patterns = @("*completion-report.md", "*final-review.md")
        }
    }
    
    $foundFiles = @()
    foreach ($pattern in $patterns) {
        $files = Get-ChildItem -Path "docs/brain/EPIC-CCN-*" -Filter $pattern -Recurse -ErrorAction SilentlyContinue
        $foundFiles += $files
        if ($files.Count -gt 0) {
            Write-Host "  Pattern '$pattern': $($files.Count) files" -ForegroundColor Gray
        }
    }
    $fileCount = $foundFiles.Count
    Write-Host "`nFile Summary: $fileCount files found (expected >= $expectedCount)" -ForegroundColor Cyan
    
    # Step 3: Per-epic file check
    Write-Host "`n[3/4] Per-epic file verification..." -ForegroundColor Yellow
    $epicsMissingFiles = @()
    
    foreach ($epic in $Epics) {
        $epicPath = "docs/brain/EPIC-CCN-$epic"
        if (Test-Path $epicPath) {
            $epicFiles = Get-ChildItem -Path $epicPath -Filter "*.md" -ErrorAction SilentlyContinue
            if ($epicFiles.Count -eq 0) {
                Write-Host "  ❌ EPIC-CCN-$epic : NO FILES FOUND" -ForegroundColor Red
                $epicsMissingFiles += $epic
            } else {
                # Check if any file matches phase patterns
                $hasPhaseFile = $false
                foreach ($pattern in $patterns) {
                    if ($epicFiles | Where-Object { $_.Name -like $pattern }) {
                        $hasPhaseFile = $true
                        break
                    }
                }
                if ($hasPhaseFile) {
                    Write-Host "  ✅ EPIC-CCN-$epic : Phase $Phase file exists" -ForegroundColor Green
                } else {
                    Write-Host "  ⚠️  EPIC-CCN-$epic : Files exist but no Phase $Phase match" -ForegroundColor Yellow
                    Write-Host "     Files: $($epicFiles.Name -join ', ')" -ForegroundColor Gray
                }
            }
        } else {
            Write-Host "  ❌ EPIC-CCN-$epic : Directory not found" -ForegroundColor Red
            $epicsMissingFiles += $epic
        }
    }
    
    # Step 4: Verdict
    Write-Host "`n[4/4] Final Verdict..." -ForegroundColor Yellow
    
    $manifestPass = ($manifestComplete -eq $expectedCount)
    $filesPass = ($fileCount -ge $expectedCount)
    $noMissingEpics = ($epicsMissingFiles.Count -eq 0)
    
    Write-Host "`nChecks:" -ForegroundColor Cyan
    Write-Host "  Manifest: $manifestComplete/$expectedCount $(if ($manifestPass) { '✅' } else { '❌' })" -ForegroundColor $(if ($manifestPass) { 'Green' } else { 'Red' })
    Write-Host "  Files: $fileCount/$expectedCount $(if ($filesPass) { '✅' } else { '❌' })" -ForegroundColor $(if ($filesPass) { 'Green' } else { 'Red' })
    Write-Host "  Per-Epic: $($expectedCount - $epicsMissingFiles.Count)/$expectedCount $(if ($noMissingEpics) { '✅' } else { '❌' })" -ForegroundColor $(if ($noMissingEpics) { 'Green' } else { 'Red' })
    
    if ($manifestPass -and $filesPass -and $noMissingEpics) {
        Write-Host "`n✅ PASS: Phase $Phase complete for all $expectedCount epics" -ForegroundColor Green
        return $true
    } else {
        Write-Host "`n❌ FAIL: Phase $Phase incomplete" -ForegroundColor Red
        
        if ($manifestMissing.Count -gt 0) {
            Write-Host "`nManifest issues: $($manifestMissing -join ', ')" -ForegroundColor Red
        }
        if ($epicsMissingFiles.Count -gt 0) {
            Write-Host "Missing files: EPIC-CCN-$($epicsMissingFiles -join ', EPIC-CCN-')" -ForegroundColor Red
        }
        
        Write-Host "`n⚠️  INVESTIGATION REQUIRED" -ForegroundColor Yellow
        Write-Host "1. Check VM logs for errors" -ForegroundColor Gray
        Write-Host "2. Verify ALL file patterns (not just expected one)" -ForegroundColor Gray
        Write-Host "3. Review manifest.json for phase status" -ForegroundColor Gray
        Write-Host "4. DO NOT RELAUNCH without investigation" -ForegroundColor Gray
        
        return $false
    }
}

# Execute verification
$result = Verify-Phase -Phase $Phase -Epics $Epics

# Exit with appropriate code
if ($result) {
    exit 0
} else {
    exit 1
}

# Made with Bob
