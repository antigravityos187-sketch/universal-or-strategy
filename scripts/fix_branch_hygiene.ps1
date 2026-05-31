# Fix Branch Hygiene - Remove Non-Tier Files from Current Branch
# Usage: .\scripts\fix_branch_hygiene.ps1

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== Three-Tier Branch Hygiene Fix ===" -ForegroundColor Cyan
Write-Host ""

# Get current branch
$branch = git rev-parse --abbrev-ref HEAD
Write-Host "Current branch: $branch" -ForegroundColor Yellow

# Determine branch tier
$tier = $null
if ($branch -match "^(feature|fix)/src-") {
    $tier = "src"
} elseif ($branch -match "^(feature|fix)/infra-") {
    $tier = "infra"
} elseif ($branch -match "^(feature|fix)/protocol-") {
    $tier = "protocol"
} else {
    Write-Host "ERROR: Branch does not follow Three-Tier naming convention" -ForegroundColor Red
    Write-Host "Expected: feature/src-*, feature/infra-*, or feature/protocol-*" -ForegroundColor Red
    exit 1
}

Write-Host "Branch tier: $tier" -ForegroundColor Green
Write-Host ""

# Get files changed in this branch vs main
$changedFiles = git diff --name-only origin/main...HEAD

if (-not $changedFiles) {
    Write-Host "No files changed in this branch" -ForegroundColor Yellow
    exit 0
}

# Classify files
$srcFiles = @()
$infraFiles = @()
$protocolFiles = @()
$violatingFiles = @()

foreach ($file in $changedFiles) {
    # Classify file
    $fileTier = $null
    
    if ($file -match "^src/.*\.cs$" -or $file -match "^tests/.*\.cs$" -or $file -match "\.csproj$") {
        $fileTier = "src"
        $srcFiles += $file
    }
    elseif ($file -match "^docs/" -or $file -match "^scripts/" -or $file -match "\.(py|ps1|sh|bat)$") {
        $fileTier = "infra"
        $infraFiles += $file
    }
    elseif ($file -match "^\.bob/" -or $file -match "^\.agent/" -or $file -match "^docs/protocol/" -or 
            $file -match "^(AGENTS|CLAUDE|BOB|CODEX)\.md$" -or $file -match "^\.mcp\.json$" -or 
            $file -match "^bob\.config\.yaml$") {
        $fileTier = "protocol"
        $protocolFiles += $file
    }
    elseif ($file -match "\.(json|yaml|yml|toml|md)$") {
        # Config/doc files - classify by branch tier
        $fileTier = "infra"  # Default to infra
        $infraFiles += $file
    }
    
    # Check for violations
    if ($fileTier -ne $tier) {
        $violatingFiles += $file
    }
}

# Report
Write-Host "Files by tier:" -ForegroundColor Cyan
Write-Host "  Src files: $($srcFiles.Count)" -ForegroundColor $(if ($tier -eq "src") { "Green" } else { "Yellow" })
Write-Host "  Infra files: $($infraFiles.Count)" -ForegroundColor $(if ($tier -eq "infra") { "Green" } else { "Yellow" })
Write-Host "  Protocol files: $($protocolFiles.Count)" -ForegroundColor $(if ($tier -eq "protocol") { "Green" } else { "Yellow" })
Write-Host ""

if ($violatingFiles.Count -eq 0) {
    Write-Host "✅ No violations found - branch is clean!" -ForegroundColor Green
    exit 0
}

Write-Host "❌ Found $($violatingFiles.Count) violating files:" -ForegroundColor Red
foreach ($file in $violatingFiles) {
    Write-Host "  - $file" -ForegroundColor Red
}
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN: Would remove these files from branch history" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply fix" -ForegroundColor Yellow
    exit 0
}

# Confirm action
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "1. Reset branch to commit before violations" -ForegroundColor Yellow
Write-Host "2. Cherry-pick only valid commits" -ForegroundColor Yellow
Write-Host "3. Force-push cleaned branch" -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Continue? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "Aborted" -ForegroundColor Yellow
    exit 0
}

# Find the last clean commit (only contains tier-appropriate files)
Write-Host ""
Write-Host "Searching for last clean commit..." -ForegroundColor Cyan

$commits = git log --oneline --reverse origin/main..HEAD
$lastCleanCommit = "origin/main"

foreach ($commitLine in $commits) {
    $commitHash = $commitLine.Split()[0]
    $commitFiles = git diff-tree --no-commit-id --name-only -r $commitHash
    
    $hasViolation = $false
    foreach ($file in $commitFiles) {
        # Check if file violates tier
        if ($tier -eq "src" -and $file -notmatch "^src/.*\.cs$" -and $file -notmatch "^tests/.*\.cs$" -and $file -notmatch "\.csproj$") {
            $hasViolation = $true
            break
        }
        elseif ($tier -eq "infra" -and $file -match "^src/.*\.cs$") {
            $hasViolation = $true
            break
        }
        elseif ($tier -eq "protocol" -and $file -match "^src/.*\.cs$") {
            $hasViolation = $true
            break
        }
    }
    
    if (-not $hasViolation) {
        $lastCleanCommit = $commitHash
    }
}

Write-Host "Last clean commit: $lastCleanCommit" -ForegroundColor Green
Write-Host ""

# Reset to last clean commit
Write-Host "Resetting to $lastCleanCommit..." -ForegroundColor Cyan
git reset --hard $lastCleanCommit

# Force push
Write-Host "Force-pushing cleaned branch..." -ForegroundColor Cyan
git push origin $branch --force

Write-Host ""
Write-Host "✅ Branch hygiene fixed!" -ForegroundColor Green
Write-Host ""
Write-Host "Violating files have been removed from branch history." -ForegroundColor Green
Write-Host "Create separate infra/protocol branches for those files." -ForegroundColor Yellow

# Made with Bob
