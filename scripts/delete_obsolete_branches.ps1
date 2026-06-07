# GitHub Branch Cleanup Script
# Deletes 80 obsolete branches from origin, keeping only 4 essential branches
# Run this script from the repository root

param(
    [switch]$DryRun = $false,
    [switch]$Force = $false
)

# Branches to KEEP (will NOT be deleted)
$branchesToKeep = @(
    "main",
    "gitbutler/workspace",
    "feature/src-epic-ccn-51-reaper-restore",
    "epic-ccn-14-propagate-master"
)

# All 80 branches to DELETE
$branchesToDelete = @(
    "1111.010-epic5-perf",
    "1111.010-epic5-perf-docs-only",
    "1111.010-epic5-perf-fix-actual",
    "1111.010-epic5-perf-linq-opt",
    "1111.010-epic5-perf-src-only",
    "1111.010-epic5-perf-v2",
    "feature/epic5-perf-optimization",
    "backup/photon-spsc-hardening-clean-pre-rebase",
    "codacy-phase2-errorprone-clean",
    "codacy-phase2-security-errorprone",
    "fix/codacy-phase2-src",
    "fix/codacy-phase2-src-clean",
    "consolidation/tier6-complete",
    "dependabot/nuget/all-88f86858a1",
    "docs/epic-posinfo-phase2-protocol",
    "docs/epic-posinfo-phase3-validation",
    "docs/epic-posinfo-phase4-tickets",
    "docs/phase-10-godmode-reactivation",
    "epic-7-quality/ticket-002-circuit-breaker-rollback",
    "epic-quality-critical-clean",
    "epic-quality-curly-braces",
    "epic-quality-p0-currentbar-guards",
    "epic-quality-p1-struct-false-positives",
    "epic-ccn-13-extract-monitor-rma-proximity",
    "epic-ccn-13-retroactive-pr",
    "epic-ccn-14-src-only",
    "src/epic-13-extraction-clean",
    "feat/phase7-final-validation",
    "feature/phase7-sprint1-sprint2-extraction",
    "feature/phase7-sprint5-extraction",
    "feat/reaper-expansion-phase2",
    "feature/reaper-expansion",
    "feature/docs-codacy-analysis",
    "feature/docs-pr10-forensics",
    "feature/epic6-cicd-docs",
    "feature/epic6-testing",
    "feature/epic6-testing-clean",
    "feature/epic6-tests-only",
    "feature/infra-bob-mode-tooling",
    "feature/infra-fix-compilation-errors",
    "feature/infra-github-migration-skill",
    "feature/infra-linear-sync",
    "feature/infra-pr18-phs-simple-methodology",
    "feature/infra-session-2026-05-31",
    "infra-pr-pollution-fix",
    "feature/photon-spsc-hardening",
    "feature/photon-spsc-hardening-clean",
    "feature/photon-spsc-hardening-perfection",
    "feature/photon-spsc-hardening-repair",
    "feature/photon-spsc-hardening-verified",
    "feature/protocol-branch-guard",
    "feature/protocol-epic-readiness",
    "feature/src-epic-ccn-12-shadowpropagatestop",
    "feature/src-fix-compilation-errors",
    "feature/src-phase7-new1-callbacks",
    "feature/src-tw1-perf-linq-optimization",
    "feature/telemetry-instrumentation",
    "fix/opencode-config-v2",
    "fix/pr17-critical-violations",
    "fix/pr3-clean-cs-only",
    "fix/pr5-clean-cs-only",
    "fix/signalbroadcaster-struct-validation",
    "infra/cleanup-and-docs",
    "infra/cleanup-final",
    "infra/p5-foundation",
    "infra/pr14-forensics-and-cleanup",
    "p5/order-callbacks",
    "p5/sima-core",
    "phase-5-distributed-pipeline",
    "phase-6-t0-roadmap-registration",
    "pr-8-clean",
    "pre-refactor-baseline",
    "protocol/pr-21-round3-documentation",
    "src/epic-13-extraction-v2",
    "src/epic-13-handle-entry-order-filled-extraction",
    "src/epic-posinfo-ticket-01",
    "test-curly-braces-tool",
    "test-workflow-trigger",
    "v12-memory-plane"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GitHub Branch Cleanup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verify repository
$currentRepo = git remote get-url origin 2>$null
if ($currentRepo -notmatch "antigravityos187-sketch/universal-or-strategy") {
    Write-Host "ERROR: Not in the correct repository!" -ForegroundColor Red
    Write-Host "Expected: antigravityos187-sketch/universal-or-strategy" -ForegroundColor Red
    Write-Host "Current: $currentRepo" -ForegroundColor Red
    exit 1
}

Write-Host "Repository: $currentRepo" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "Branches to KEEP (4):" -ForegroundColor Green
foreach ($branch in $branchesToKeep) {
    Write-Host "  + $branch" -ForegroundColor Green
}
Write-Host ""

Write-Host "Branches to DELETE (80):" -ForegroundColor Yellow
Write-Host "  Total: $($branchesToDelete.Count) branches" -ForegroundColor Yellow
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN MODE - No branches will be deleted" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "Would delete the following branches:" -ForegroundColor Yellow
    foreach ($branch in $branchesToDelete) {
        Write-Host "  - $branch" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "To actually delete, run: .\scripts\delete_obsolete_branches.ps1 -Force" -ForegroundColor Cyan
    exit 0
}

if (-not $Force) {
    Write-Host "WARNING: This will permanently delete 80 branches from GitHub!" -ForegroundColor Red
    Write-Host ""
    $confirmation = Read-Host "Type 'DELETE' to confirm"
    
    if ($confirmation -ne "DELETE") {
        Write-Host "Aborted." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Starting deletion..." -ForegroundColor Yellow
Write-Host ""

$successCount = 0
$failCount = 0
$failedBranches = @()

foreach ($branch in $branchesToDelete) {
    Write-Host "Deleting: $branch..." -NoNewline
    
    $result = git push origin --delete $branch 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " SUCCESS" -ForegroundColor Green
        $successCount++
    } else {
        Write-Host " FAILED" -ForegroundColor Red
        $failCount++
        $failedBranches += $branch
        Write-Host "  Error: $result" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deletion Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Successfully deleted: $successCount branches" -ForegroundColor Green
Write-Host "Failed: $failCount branches" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
Write-Host ""

if ($failCount -gt 0) {
    Write-Host "Failed branches:" -ForegroundColor Red
    foreach ($branch in $failedBranches) {
        Write-Host "  - $branch" -ForegroundColor Red
    }
    Write-Host ""
}

# Verify final state
Write-Host "Fetching updated branch list..." -ForegroundColor Cyan
git fetch --prune origin | Out-Null

$remainingBranches = git branch -r | Select-String "^  origin/" | Where-Object { $_ -notmatch "origin/HEAD" }
$branchCount = ($remainingBranches | Measure-Object).Count

Write-Host ""
Write-Host "Final branch count: $branchCount" -ForegroundColor $(if ($branchCount -eq 4) { "Green" } else { "Yellow" })

if ($branchCount -eq 4) {
    Write-Host "SUCCESS: Repository cleaned to 4 essential branches!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Expected 4 branches, found $branchCount" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Remaining branches:" -ForegroundColor Yellow
    $remainingBranches | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
}

Write-Host ""
Write-Host "Cleanup complete!" -ForegroundColor Cyan

# Made with Bob
