# Analyze Deleted EPIC-CCN Branches
# Check what was in deleted epic-ccn-13, epic-ccn-14, epic-ccn-15 branches

Write-Host "=== Analyzing Deleted EPIC-CCN Branches ===" -ForegroundColor Cyan
Write-Host ""

$epicBranches = @(
    @{Name='epic-ccn-13-extract-monitor-rma-proximity'; Hash='2f859446'},
    @{Name='epic-ccn-13-retroactive-pr'; Hash='6a427eca'},
    @{Name='epic-ccn-14-src-only'; Hash='ce7e9421'}
)

foreach ($branch in $epicBranches) {
    Write-Host "=== $($branch.Name) ===" -ForegroundColor Yellow
    Write-Host "Commit: $($branch.Hash)"
    
    # Get files changed from main
    $files = git diff --name-only main...$($branch.Hash) 2>$null
    
    # Categorize files
    $csFiles = $files | Where-Object { $_ -match '\.cs$' -and $_ -notmatch 'test' }
    $testFiles = $files | Where-Object { $_ -match '\.cs$' -and $_ -match 'test' }
    $nonCsFiles = $files | Where-Object { $_ -notmatch '\.cs$' }
    
    Write-Host "  .cs src files: $($csFiles.Count)"
    Write-Host "  .cs test files: $($testFiles.Count)"
    Write-Host "  non-.cs files: $($nonCsFiles.Count)"
    
    if ($csFiles.Count -gt 0) {
        Write-Host "  Source files:" -ForegroundColor Green
        $csFiles | ForEach-Object { Write-Host "    $_" }
    }
    
    if ($testFiles.Count -gt 0) {
        Write-Host "  Test files:" -ForegroundColor Green
        $testFiles | ForEach-Object { Write-Host "    $_" }
    }
    
    # Check if these changes are in workspace
    Write-Host "  Checking if in workspace..." -ForegroundColor Cyan
    $workspaceFiles = git diff --name-only main...gitbutler/workspace 2>$null
    $missing = $csFiles | Where-Object { $workspaceFiles -notcontains $_ }
    
    if ($missing.Count -gt 0) {
        Write-Host "  MISSING FROM WORKSPACE:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    } else {
        Write-Host "  All .cs files present in workspace" -ForegroundColor Green
    }
    
    Write-Host ""
}

# Made with Bob
