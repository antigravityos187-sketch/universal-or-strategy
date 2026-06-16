# Generate Phase 1 scripts using Building Blocks method
# Copies Phase 0 template and modifies only phase-specific content

$ErrorActionPreference = "Stop"

Write-Host "=========================================="
Write-Host "Generating Phase 1 Scripts (Building Blocks Method)"
Write-Host "=========================================="
Write-Host ""

# Read the Phase 0 template
$templatePath = "scripts/wave1/_p0_template.sh"
if (-not (Test-Path $templatePath)) {
    Write-Error "Template not found: $templatePath"
    exit 1
}

$template = Get-Content $templatePath -Raw

Write-Host "✅ Loaded Phase 0 template"
Write-Host ""

# Generate Phase 1 scripts for all 15 epics
Write-Host "Generating 15 Phase 1 scripts..."
Write-Host ""

for ($i = 1; $i -le 15; $i++) {
    $epicNum = "EPIC-{0:D3}" -f $i
    $scriptNum = "{0:D2}" -f $i
    
    # Start with template
    $phase1Script = $template
    
    # Replace ALL occurrences of phase0 with phase1
    $phase1Script = $phase1Script -replace 'phase0', 'phase1'
    $phase1Script = $phase1Script -replace 'Phase 0', 'Phase 1'
    
    # Replace EPIC-003 with current epic number
    $phase1Script = $phase1Script -replace 'EPIC-003', $epicNum
    
    # Replace task descriptions
    $phase1Script = $phase1Script -replace 'Hotspot Analysis', 'Scope Definition'
    $phase1Script = $phase1Script -replace '00-hotspots\.md', '00-scope.md'
    
    # Replace chat mode (CRITICAL: must be 'plan' for Phase 1)
    $phase1Script = $phase1Script -replace 'v12-phase0-hotspot', 'plan'
    $phase1Script = $phase1Script -replace 'v12-phase1-hotspot', 'plan'
    
    # Update manifest phase number
    $phase1Script = $phase1Script -replace '"0":', '"1":'
    $phase1Script = $phase1Script -replace 'phase 0 completed', 'phase 1 completed'
    
    # Write to file
    $outputPath = "scripts/wave1/_p1_$scriptNum.sh"
    $phase1Script | Out-File -FilePath $outputPath -Encoding ASCII -NoNewline
    
    Write-Host "  ✅ Created _p1_$scriptNum.sh for $epicNum"
}

Write-Host ""
Write-Host "✅ Created 15 Phase 1 scripts"
Write-Host ""
Write-Host "Verification:"
Write-Host "  Total scripts: $(( Get-ChildItem 'scripts/wave1/_p1_*.sh' ).Count)"
Write-Host ""
Write-Host "Sample (first 5):"
Get-ChildItem "scripts/wave1/_p1_*.sh" | Select-Object -First 5 | ForEach-Object { Write-Host "  $_" }
Write-Host "  ... (10 more files)"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Review one script to verify correctness"
Write-Host "2. Upload to 3 VMs (5 epics each)"
Write-Host "3. Launch execution"

# Made with Bob