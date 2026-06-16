# Fix Phase 1 scripts to use correct JSON field name
# API key files use "apikey" not "key"

$scripts = @(
    "_p1_107.sh",
    "_p1_108.sh",
    "_p1_109.sh",
    "_p1_110.sh",
    "_p1_111.sh",
    "_p1_112.sh",
    "_p1_113.sh",
    "_p1_114.sh",
    "_p1_115.sh"
)

foreach ($script in $scripts) {
    Write-Host "Fixing $script..."
    
    # Read content
    $content = Get-Content $script -Raw
    
    # Replace: jq -r '.key'
    # With: jq -r '.apikey'
    $content = $content -replace "jq -r '\.key'", "jq -r '.apikey'"
    
    # Write back
    Set-Content -Path $script -Value $content -NoNewline
    
    Write-Host "[OK] Fixed $script"
}

Write-Host ""
Write-Host "[OK] Fixed all 9 Phase 1 scripts - changed .key to .apikey"

# Made with Bob
