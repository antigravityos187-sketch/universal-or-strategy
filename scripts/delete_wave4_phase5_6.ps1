# Delete Phase 5-6 files for 78 epics (all except 027)
$epics = 1..26 + 28..80 | ForEach-Object { "EPIC-CCN-{0:D3}" -f $_ }

foreach ($epic in $epics) {
    $brainDir = "docs/brain/$epic"
    if (Test-Path $brainDir) {
        # Delete Phase 5 ticket files
        Remove-Item "$brainDir/ticket-*-completion.md" -ErrorAction SilentlyContinue
        
        # Delete Phase 6 completion report (Wave 4 used this name)
        Remove-Item "$brainDir/06-completion-report.md" -ErrorAction SilentlyContinue
        
        # Also delete verification report if it exists (for completeness)
        Remove-Item "$brainDir/06-verification-report.md" -ErrorAction SilentlyContinue
        
        Write-Host "Deleted Phase 5-6 files for $epic"
    }
}

Write-Host "`nRollback complete. Deleted Phase 5-6 files for 78 epics."

# Made with Bob
