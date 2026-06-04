# Pre-Branch Cleanup Script
# Ensures working directory is clean before creating epic branches
# Prevents PR pollution from uncommitted changes

param(
    [Parameter(Mandatory=$true)]
    [string]$EpicSlug,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PRE-BRANCH CLEANUP: $EpicSlug" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check git status
Write-Host "[1/5] Checking git status..." -ForegroundColor Yellow
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "WARNING: Working directory is dirty!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Uncommitted changes detected:" -ForegroundColor Red
    git status --short
    Write-Host ""
    
    if (-not $Force) {
        Write-Host "BLOCKING: Cannot create branch with uncommitted changes." -ForegroundColor Red
        Write-Host ""
        Write-Host "Options:" -ForegroundColor Yellow
        Write-Host "  1. Commit changes: git add -A && git commit -m 'message'" -ForegroundColor White
        Write-Host "  2. Stash changes: git stash push -m 'pre-$EpicSlug'" -ForegroundColor White
        Write-Host "  3. Discard changes: git reset --hard HEAD" -ForegroundColor White
        Write-Host "  4. Force cleanup: .\scripts\pre_branch_cleanup.ps1 -EpicSlug $EpicSlug -Force" -ForegroundColor White
        Write-Host ""
        exit 1
    }
    
    Write-Host "FORCE mode enabled. Stashing uncommitted changes..." -ForegroundColor Yellow
    git stash push -m "pre-$EpicSlug-cleanup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "Changes stashed. Retrieve with: git stash pop" -ForegroundColor Green
}
else {
    Write-Host "Working directory is clean." -ForegroundColor Green
}
Write-Host ""

# Step 2: Verify on main branch
Write-Host "[2/5] Verifying current branch..." -ForegroundColor Yellow
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    Write-Host "WARNING: Not on main branch (current: $currentBranch)" -ForegroundColor Red
    Write-Host "Switching to main..." -ForegroundColor Yellow
    git checkout main
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to switch to main branch" -ForegroundColor Red
        exit 1
    }
}
Write-Host "On main branch." -ForegroundColor Green
Write-Host ""

# Step 3: Pull latest changes
Write-Host "[3/5] Pulling latest changes from origin/main..." -ForegroundColor Yellow
git fetch origin main
git pull origin main
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to pull latest changes" -ForegroundColor Red
    exit 1
}
Write-Host "Main branch up to date." -ForegroundColor Green
Write-Host ""

# Step 4: Verify clean state again
Write-Host "[4/5] Final clean state verification..." -ForegroundColor Yellow
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "ERROR: Working directory still dirty after cleanup!" -ForegroundColor Red
    git status --short
    exit 1
}
Write-Host "Working directory verified clean." -ForegroundColor Green
Write-Host ""

# Step 5: Create epic branch
Write-Host "[5/5] Creating epic branch: $EpicSlug..." -ForegroundColor Yellow
git checkout -b $EpicSlug
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create branch $EpicSlug" -ForegroundColor Red
    exit 1
}
Write-Host "Branch created successfully." -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PRE-BRANCH CLEANUP: COMPLETE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Branch: $EpicSlug" -ForegroundColor White
Write-Host "Base: main ($(git rev-parse --short HEAD))" -ForegroundColor White
Write-Host "Status: Clean working directory" -ForegroundColor White
Write-Host ""
Write-Host "Ready for epic execution." -ForegroundColor Green

# Made with Bob
