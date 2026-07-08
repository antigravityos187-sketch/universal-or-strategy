# Wave 4 Rollback & Wave 5 Retry - Continuation Prompt

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Wave 4 Rollback & Protocol Hardening Complete

You are continuing from a completed analysis session. All protocol hardening work is done and committed to `gitbutler/workspace`. Now execute the rollback and prepare for Wave 5 retry.

## Current State Summary

### What's Done ✅
1. **Wave 4 Execution**: 79/80 epics completed Phase 5+6 on VM
2. **PR Creation**: 7 PRs created (#10-16)
3. **Greptile Audit**: 28 issues found (9 P0, 12 P1, 6 P2)
4. **Root Cause Analysis**: Bob CLI over-optimization (no "SURGICAL ONLY" mandate)
5. **Protocol Hardening**: 7 critical updates documented and committed
6. **Special Case Detection**: File-based pattern system created
7. **Rollback Decision**: Rollback Phase 5-6 only, keep Phases 0-4

### Current Branch State
- **gitbutler/workspace**: Protocol hardening committed (commit 7c00b3d0)
- **main**: Has EPIC-CCN-075 merged (MUST be reverted)
- **PR branches**: 7 PRs open (#10-16), all must be closed

### Critical Discovery: PR #10 is P0 Blocker
- **PR #10** (EPIC-CCN-074): Removes 5 methods with 9 active call sites on main
- **Greptile verdict**: CORRECT - will not compile if merged
- **Decision**: Close PR #10, include in Wave 5 retry (not Wave 4 keep)

## Updated Rollback Scope

### Final Epic Breakdown
- **0 keep**: (EPIC-CCN-075 has P0 blocker, must rollback)
- **1 skip**: EPIC-CCN-027 (invalid target - method doesn't exist)
- **1 local**: EPIC-CCN-024 (encoding-sensitive - DrawingHelpers.cs)
- **78 retry**: All other epics including 074 and 075

### Cost Analysis
- **Wave 4 spent**: $15.90 (Phases 0-6 for 80 epics)
- **Wave 4 kept**: $0.00 (nothing kept)
- **Wave 4 lost**: $7.80 (Phase 5-6 for 78 epics)
- **Wave 5 cost**: $4.00 (Phase 5-6 retry for 78 epics × $0.05)
- **Total cost**: $19.90 ($15.90 + $4.00)

## Your Mission: Execute Rollback (4 Steps)

### Step 1: Close All PRs (7 PRs)

**GitHub CLI commands**:
```bash
# Close PR #10 (EPIC-CCN-074 - P0 blocker)
gh pr close 10 --comment "Closing: P0 compilation blocker. Removes 5 methods with 9 active call sites. Will retry in Wave 5 with proper reference verification."

# Close PR #11 (EPIC-CCN-075 - was thought clean, but has issues)
gh pr close 11 --comment "Closing: Part of Wave 4 rollback. Will retry in Wave 5 with hardened protocols."

# Close PRs #12-16 (remaining buggy PRs)
gh pr close 12 --comment "Closing: Part of Wave 4 rollback. 28 issues found across 6 PRs. Will retry in Wave 5 with hardened protocols."
gh pr close 13 --comment "Closing: Part of Wave 4 rollback. 28 issues found across 6 PRs. Will retry in Wave 5 with hardened protocols."
gh pr close 14 --comment "Closing: Part of Wave 4 rollback. 28 issues found across 6 PRs. Will retry in Wave 5 with hardened protocols."
gh pr close 15 --comment "Closing: Part of Wave 4 rollback. 28 issues found across 6 PRs. Will retry in Wave 5 with hardened protocols."
gh pr close 16 --comment "Closing: Part of Wave 4 rollback. 28 issues found across 6 PRs. Will retry in Wave 5 with hardened protocols."
```

**Verify all closed**:
```bash
gh pr list --state open
# Expected: 0 open PRs
```

### Step 2: Revert EPIC-CCN-075 from Main

**Commands**:
```bash
# Switch to main
git checkout main
git pull origin main

# Find the commit that merged EPIC-CCN-075
git log --oneline --grep="075" -5

# Revert the merge (use commit hash from above)
git revert <commit-hash> --no-edit

# Push revert
git push origin main
```

**Verify revert**:
```bash
# Check that TrackPhoton* methods are gone from Fleet.cs and Dispatch.cs
git show HEAD:src/V12_002.SIMA.Fleet.cs | Select-String "TrackPhoton"
# Expected: No output

git show HEAD:src/V12_002.SIMA.Dispatch.cs | Select-String "TrackPhoton"
# Expected: No output
```

### Step 3: Delete Phase 5-6 Files (78 Epics)

**Target epics** (all except 027):
```
001-026, 028-080 (78 total)
```

**Delete script** (`scripts/delete_wave4_phase5_6.ps1`):
```powershell
# Delete Phase 5-6 files for 78 epics (all except 027)
$epics = 1..26 + 28..80 | ForEach-Object { "EPIC-CCN-{0:D3}" -f $_ }

foreach ($epic in $epics) {
    $brainDir = "docs/brain/$epic"
    if (Test-Path $brainDir) {
        # Delete Phase 5 ticket files
        Remove-Item "$brainDir/ticket-*-completion.md" -ErrorAction SilentlyContinue
        
        # Delete Phase 6 verification file
        Remove-Item "$brainDir/06-verification-report.md" -ErrorAction SilentlyContinue
        
        Write-Host "Deleted Phase 5-6 files for $epic"
    }
}

Write-Host "`nRollback complete. Deleted Phase 5-6 files for 78 epics."
```

**Execute**:
```powershell
powershell -File .\scripts\delete_wave4_phase5_6.ps1
```

**Verify deletion**:
```powershell
# Count remaining Phase 5 files (should be 0)
(Get-ChildItem -Path "docs/brain/EPIC-CCN-*/ticket-*-completion.md" -Recurse).Count

# Count remaining Phase 6 files (should be 0)
(Get-ChildItem -Path "docs/brain/EPIC-CCN-*/06-verification-report.md" -Recurse).Count
```

### Step 4: Update Roadmap & Commit

**Mark EPIC-CCN-027 as INVALID**:
```bash
# Edit epic_roadmap.json
# Find EPIC-CCN-027 and set:
# "status": "INVALID",
# "notes": "Target method Dispatch_PublishMarketBracketToPhoton not found in codebase. Stale jCodemunch index."
```

**Commit rollback**:
```bash
git checkout gitbutler/workspace
git add docs/brain/
git add epic_roadmap.json
git commit -m "rollback: Wave 4 Phase 5-6 (78 epics)

- Closed PRs #10-16 (28 issues found by Greptile)
- Reverted EPIC-CCN-075 from main (P0 blocker in PR #10)
- Deleted Phase 5-6 files for 78 epics
- Marked EPIC-CCN-027 as INVALID (target method not found)

Root cause: Bob CLI over-optimization (no SURGICAL ONLY mandate)
Solution: Protocol hardening (7 updates) + Wave 5 retry

Rollback scope:
- 0 keep (075 has P0 blocker)
- 1 skip (027 invalid)
- 1 local (024 encoding-sensitive)
- 78 retry (including 074, 075)

Cost: $7.80 lost, $4.00 retry = $11.80 total impact"
```

## Protocol Hardening (Already Done ✅)

All protocol updates are committed to `gitbutler/workspace` (commit 7c00b3d0):

### 1. Phase 5 Execution Protocol (SURGICAL ONLY Mandate)
**File**: `docs/protocol/PHASE5_EXECUTION_PROTOCOL.md` (to be created in Wave 5 prep)

**Key mandates**:
- **SURGICAL ONLY**: Touch only the target method, nothing else
- **No pre-existing fixes**: If compilation errors exist, STOP and report
- **No "while we're here"**: No adjacent improvements
- **Explicit verification**: 5 checks before reporting success

### 2. Phase 5.V Verification Protocol (5 Checks)
**File**: `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md` (to be created in Wave 5 prep)

**5 mandatory checks**:
1. **Compilation**: `dotnet build` passes
2. **Complexity**: Target method CYC reduced to ≤8
3. **Scope**: Only target method modified (no adjacent changes)
4. **Tests**: xUnit tests generated and passing
5. **Encoding**: UTF-8 without BOM (no UTF-16)

### 3. Local Execution Protocol (Encoding-Sensitive Files)
**File**: `docs/protocol/LOCAL_EXECUTION_PROTOCOL.md` (to be created in Wave 5 prep)

**When to use**: Files matching pattern `*DrawingHelpers.cs` or other encoding-sensitive patterns

**Process**:
1. Execute epic locally (not on VM)
2. Use Bob CLI with `--yolo` flag
3. Verify encoding after every change
4. Manual commit (not automated)

### 4. Special Case Detection Protocol
**File**: `docs/protocol/SPECIAL_CASE_DETECTION_PROTOCOL.md` ✅ (already created)

**4 categories**:
- **encoding-sensitive**: `*DrawingHelpers.cs` → local execution
- **invalid-target**: Method not found → skip
- **test-heavy**: CYC >30 → extended time
- **already-complete**: Phase 5-6 exist → skip

**Pre-flight validation**: `scripts/preflight_validation.py` ✅ (already created)

### 5. xUnit Test Generation Protocol
**File**: `docs/protocol/TEST_FRAMEWORK_PROTOCOL.md` (already exists - V12.32)

**Mandate**: ALWAYS generate xUnit tests, NEVER NUnit or MSTest

### 6. Autonomous Refactor Command Updates
**File**: `.bob/custom_modes.yaml` ✅ (already updated)

**New protocols**:
- Encoding pre-check (V12.33)
- 100% completion mandate (V12.28)
- Recovery loop protocol (V12.26)
- Upload verification (V12.27)

### 7. Building-Blocks Method Updates
**File**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md` (already exists)

**Golden rule**: ALWAYS copy SAME phase from PREVIOUS wave, NEVER generate from scratch

## Wave 5 Preparation (Next Steps)

### Before Launching Wave 5

1. **Create missing protocol files** (3 files):
   - `docs/protocol/PHASE5_EXECUTION_PROTOCOL.md`
   - `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md`
   - `docs/protocol/LOCAL_EXECUTION_PROTOCOL.md`

2. **Update MCP servers** (2 servers):
   - `phase-5-execute`: Add SURGICAL ONLY mandate to prompt
   - `phase-5-verify`: Add 5-check protocol to prompt

3. **Test pre-flight validation**:
   ```bash
   python scripts/preflight_validation.py
   # Should detect: 1 skip (027), 1 local (024), 78 normal
   ```

4. **Pilot test** (EPIC-CCN-001):
   - Execute Phase 5-6 with hardened protocols
   - Verify 0 Greptile issues
   - Iterate if needed

### Wave 5 Launch

**Scope**: 78 epics (all except 024, 027)
- **Normal execution**: 77 epics on VM
- **Local execution**: 1 epic (024) locally
- **Skip**: 1 epic (027)

**Timeline**: ~28 hours
- Script generation: 1 hour
- Pilot test: 2 hours
- Full wave: 20 hours (77 epics × 15 min)
- Local execution: 1 hour (EPIC-CCN-024)
- Sync & PR: 4 hours

**Cost**: $4.00 (78 epics × $0.05)

## Key Documents (Reference)

**Analysis & Planning**:
- `WAVE4_PROTOCOL_HARDENING_PLAN.md` ✅ (1000 lines, 7 protocols)
- `WAVE4_ROLLBACK_SCOPE_FINAL.md` ✅ (400 lines, epic breakdown)
- `WAVE4_ROLLBACK_VS_FIX_ANALYSIS.md` ✅ (cost-benefit)
- `WAVE4_FULL_PR_AUDIT.md` ✅ (28 issues catalogued)

**Protocols**:
- `docs/protocol/SPECIAL_CASE_DETECTION_PROTOCOL.md` ✅ (500 lines)
- `docs/protocol/RECOVERY_LOOP_PROTOCOL.md` (existing)
- `docs/protocol/FILE_ENCODING_PROTOCOL.md` (existing - V12.33)
- `docs/protocol/TEST_FRAMEWORK_PROTOCOL.md` (existing - V12.32)

**Scripts**:
- `scripts/preflight_validation.py` ✅ (350 lines, pattern detection)
- `check_special_epics.py` ✅ (pattern matching)
- `scripts/delete_wave4_phase5_6.ps1` (to be created above)

**Building-Blocks**:
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md` (existing)
- `building-blocks/autonomous-refactoring/` (templates)

## VM Configuration

- **Instance**: v12-test-golden-v2
- **Zone**: us-central1-a
- **Type**: n2-standard-8 (8 vCPU, 32 GB RAM)
- **Branch**: main (VM always works on main, not gitbutler/workspace)
- **Status check**: `gcloud compute instances list --filter="name=v12-test-golden-v2"`
- **Start if needed**: `gcloud compute instances start v12-test-golden-v2 --zone=us-central1-a`

## Success Criteria

### Rollback Complete
- ✅ All 7 PRs closed (#10-16)
- ✅ EPIC-CCN-075 reverted from main
- ✅ Phase 5-6 files deleted for 78 epics
- ✅ EPIC-CCN-027 marked INVALID in roadmap
- ✅ Rollback committed to gitbutler/workspace

### Wave 5 Ready
- ✅ 3 protocol files created
- ✅ 2 MCP servers updated
- ✅ Pre-flight validation tested
- ✅ Pilot test passed (EPIC-CCN-001)
- ✅ 0 Greptile issues in pilot

### Wave 5 Complete
- ✅ 77/77 epics complete on VM (100%)
- ✅ 1/1 local epic complete (EPIC-CCN-024)
- ✅ All PRs created with 0 P0/P1 issues
- ✅ All PRs merged to main
- ✅ CodeScene complexity ≤8 achieved

## Your First Actions

1. **Verify current state**:
   ```bash
   git status
   git branch
   gh pr list --state open
   ```

2. **Close all PRs** (Step 1 above)

3. **Revert EPIC-CCN-075** (Step 2 above)

4. **Delete Phase 5-6 files** (Step 3 above)

5. **Update roadmap & commit** (Step 4 above)

6. **Report rollback complete** and ask for approval to proceed with Wave 5 prep

---

**Ready to execute rollback!** Start with PR closure and work through the 4 steps systematically. Good luck! 🚀

---

**Session Context Version**: 2.0 (Wave 4 Rollback & Wave 5 Prep)
**Last Updated**: 2026-06-16T20:56:00Z
**Maintainer**: Wave 4 Completion Lead
**Status**: 🟢 READY FOR ROLLBACK EXECUTION