$lines = Get-Content complexity_audit_fresh_2026-06-14.txt | Select-String '- .+::.+ \(CYC=(\d+)'
$count = 0
foreach ($line in $lines) {
    if ($line -match 'CYC=(\d+)') {
        $cyc = [int]$matches[1]
        if ($cyc -gt 8) {
            $count++
        }
    }
}
Write-Host "Methods with CYC > 8: $count"

# Made with Bob
