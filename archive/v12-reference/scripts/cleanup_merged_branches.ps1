# Cleanup Merged Branches Script
# Deletes local branches that have been merged into main
# Excludes: main, gitbutler/*, current branch

Write-Host "=== Merged Branch Cleanup ===" -ForegroundColor Cyan
Write-Host ""

# Get current branch
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "Current branch: $currentBranch" -ForegroundColor Yellow
Write-Host ""

# Get all branches merged into main
$mergedBranches = git branch --merged main | ForEach-Object { $_.Trim() }

# Filter out branches to keep
$branchesToDelete = $mergedBranches | Where-Object {
    $_ -notmatch '^\*' -and           # Not current branch marker
    $_ -ne 'main' -and                # Not main
    $_ -notmatch '^gitbutler/' -and   # Not gitbutler branches
    $_ -ne $currentBranch             # Not current branch
}

Write-Host "Found $($branchesToDelete.Count) branches to delete:" -ForegroundColor Green
$branchesToDelete | ForEach-Object { Write-Host "  - $_" }
Write-Host ""

if ($branchesToDelete.Count -eq 0) {
    Write-Host "No branches to delete." -ForegroundColor Yellow
    exit 0
}

# Confirm deletion
$confirm = Read-Host "Delete these branches? (y/N)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 0
}

# Delete branches
$deleted = 0
$failed = 0

foreach ($branch in $branchesToDelete) {
    try {
        git branch -d $branch 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Deleted: $branch" -ForegroundColor Green
            $deleted++
        } else {
            Write-Host "Failed: $branch (use -D to force)" -ForegroundColor Red
            $failed++
        }
    } catch {
        Write-Host "Error deleting $branch : $_" -ForegroundColor Red
        $failed++
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Deleted: $deleted" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red
Write-Host ""

# Made with Bob
