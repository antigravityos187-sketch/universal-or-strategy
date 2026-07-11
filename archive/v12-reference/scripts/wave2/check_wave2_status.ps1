# Wave 2 Status Checker
# Checks final verification status for all Wave 2 epics (107-114)

$epics = @(107, 108, 109, 111, 112, 113, 114)
$results = @()

foreach ($epic in $epics) {
    $epicDir = "docs/brain/EPIC-CCN-$epic"
    
    if (-not (Test-Path $epicDir)) {
        $results += [PSCustomObject]@{
            Epic = "EPIC-CCN-$epic"
            Status = "NOT_FOUND"
            LastTicket = "N/A"
            Verdict = "N/A"
        }
        continue
    }
    
    # Find all verification files
    $verifications = Get-ChildItem -Path $epicDir -Filter "*verification.md" | Sort-Object Name -Descending
    
    if ($verifications.Count -eq 0) {
        $results += [PSCustomObject]@{
            Epic = "EPIC-CCN-$epic"
            Status = "NO_VERIFICATIONS"
            LastTicket = "N/A"
            Verdict = "N/A"
        }
        continue
    }
    
    # Check last ticket verification
    $lastVerification = $verifications[0]
    $content = Get-Content $lastVerification.FullName -Raw
    
    # Extract verdict
    $verdict = "UNKNOWN"
    if ($content -match "(?:VERDICT|Final Verdict|Overall Assessment):\s*[✅❌⚠️]*\s*\*\*([A-Z\s]+)\*\*") {
        $verdict = $matches[1].Trim()
    }
    
    # Determine status
    $status = switch -Regex ($verdict) {
        "PASS|COMPLETE|APPROVED" { "[OK] COMPLETE" }
        "FAIL|BLOCKED" { "[X] BLOCKED" }
        "PENDING|MANUAL" { "[!] PENDING" }
        default { "[?] UNKNOWN" }
    }
    
    $results += [PSCustomObject]@{
        Epic = "EPIC-CCN-$epic"
        Status = $status
        LastTicket = $lastVerification.Name
        Verdict = $verdict
    }
}

Write-Host "`n=== Wave 2 Status Summary ===" -ForegroundColor Cyan
Write-Host "Checked: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n" -ForegroundColor Gray

$results | Format-Table -AutoSize

# Summary counts
$complete = ($results | Where-Object { $_.Status -like "*COMPLETE*" }).Count
$blocked = ($results | Where-Object { $_.Status -like "*BLOCKED*" }).Count
$pending = ($results | Where-Object { $_.Status -like "*PENDING*" }).Count
$unknown = ($results | Where-Object { $_.Status -like "*UNKNOWN*" }).Count

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "[OK] Complete: $complete" -ForegroundColor Green
Write-Host "[X] Blocked: $blocked" -ForegroundColor Red
Write-Host "[!] Pending: $pending" -ForegroundColor Yellow
Write-Host "[?] Unknown: $unknown" -ForegroundColor Gray
Write-Host "Total: $($results.Count)" -ForegroundColor White

# Export to JSON for programmatic access
$results | ConvertTo-Json | Out-File "scripts/wave2/wave2_status.json"
Write-Host "`nStatus exported to: scripts/wave2/wave2_status.json" -ForegroundColor Gray

# Made with Bob
