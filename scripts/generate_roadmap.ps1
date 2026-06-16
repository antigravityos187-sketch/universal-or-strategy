# Generate Wave 4 Epic Roadmap with Two-Tier System
$methods = @()
$currentFile = ""

foreach ($line in Get-Content complexity_audit_fresh_2026-06-14.txt) {
    if ($line -match "^=== FILE: (.+) ===$") {
        $currentFile = $matches[1].Trim()
    }
    elseif ($line -match "\| (.+?) \|\s+(\d+) \|\s+(\d+) \|.*REFACTOR") {
        $methods += @{
            method = $matches[1].Trim()
            loc = [int]$matches[2]
            cyc = [int]$matches[3]
            file = $currentFile
        }
    }
}

Write-Host "Total methods needing refactoring: $($methods.Count)"

# Sort by complexity (descending)
$sorted = $methods | Sort-Object -Property cyc -Descending

# Two-tier selection
$tier1 = $sorted | Where-Object { $_.cyc -ge 15 }  # High complexity
$tier2 = $sorted | Where-Object { $_.cyc -ge 9 -and $_.cyc -le 14 }  # Medium complexity

Write-Host "Tier 1 (CYC 15-30): $($tier1.Count) methods"
Write-Host "Tier 2 (CYC 9-14): $($tier2.Count) methods"

# Select top 80 (40 from each tier, or adjust)
$tier1Count = [Math]::Min(40, $tier1.Count)
$tier2Count = [Math]::Min(40, $tier2.Count)

# Adjust if one tier is smaller
if ($tier1Count -lt 40) {
    $tier2Count = [Math]::Min(80 - $tier1Count, $tier2.Count)
}
elseif ($tier2Count -lt 40) {
    $tier1Count = [Math]::Min(80 - $tier2Count, $tier1.Count)
}

$selected = @()
$selected += $tier1 | Select-Object -First $tier1Count
$selected += $tier2 | Select-Object -First $tier2Count

Write-Host "Selected: $tier1Count from Tier 1 + $tier2Count from Tier 2 = $($selected.Count) epics"

# Create epic roadmap
$epics = @()
$epicNum = 1

foreach ($method in $selected) {
    $tier = if ($method.cyc -ge 15) { 1 } else { 2 }
    $epics += @{
        epic_number = "EPIC-CCN-{0:D3}" -f $epicNum
        method = $method.method
        file = $method.file
        cyclomatic = $method.cyc
        loc = $method.loc
        tier = $tier
        status = "pending"
    }
    $epicNum++
}

# Display top 20
Write-Host "`nTop 20 epics:"
$epics | Select-Object -First 20 | ForEach-Object {
    $tierLabel = "T$($_.tier)"
    Write-Host "$($_.epic_number) | $tierLabel | CYC=$($_.cyclomatic) | $($_.method) | $($_.file)"
}

# Save to JSON
$json = $epics | ConvertTo-Json -Depth 10
$json | Out-File -FilePath "epic_roadmap_wave4_fresh.json" -Encoding UTF8

Write-Host "`nSaved $($epics.Count) epics to epic_roadmap_wave4_fresh.json"
Write-Host "Epic range: $($epics[0].epic_number) to $($epics[-1].epic_number)"
Write-Host "`nTier breakdown:"
Write-Host "  Tier 1 (CYC 15-30): $(($epics | Where-Object { $_.tier -eq 1 }).Count) epics"
Write-Host "  Tier 2 (CYC 9-14): $(($epics | Where-Object { $_.tier -eq 2 }).Count) epics"

# Made with Bob
