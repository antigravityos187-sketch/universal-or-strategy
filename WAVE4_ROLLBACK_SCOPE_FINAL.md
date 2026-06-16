# Wave 4 Rollback Scope - Final Clarification

**Date**: 2026-06-16  
**Version**: 2.0 (Complete with special epics)

## Executive Summary

**Question**: Do all epics need Phases 5 and 6 again, including files from PR #10?

**Answer**: 
- ✅ **KEEP PR #10** (EPIC-CCN-075) - 0 issues, clean
- ✅ **KEEP EPIC-CCN-027** - Already marked INVALID (method doesn't exist)
- ⚠️ **LOCAL EXECUTION** - EPIC-CCN-24 (DrawingHelpers encoding issues)
- ❌ **ROLLBACK** - 77 other epics need Phase 5-6 retry

---

## The Complete Picture

### Total Wave 4 Scope: 80 Epics

| Category | Count | Epics | Action |
|----------|-------|-------|--------|
| **Clean (Keep)** | 1 | EPIC-CCN-075 | ✅ Merge PR #10 |
| **Invalid (Skip)** | 1 | EPIC-CCN-027 | ✅ Mark INVALID |
| **Local Execution** | 1 | EPIC-CCN-24 | ⚠️ Execute locally |
| **Rollback & Retry** | 77 | All others | ❌ Rollback Phase 5-6 |

---

## Special Epics Breakdown

### 1. EPIC-CCN-075 (Infrastructure) - KEEP ✅

**Status**: CLEAN - 0 issues in Greptile review

**Details**:
- **PR**: #10 (S7 Infrastructure)
- **File**: `src/V12_002.Infrastructure.cs`
- **Issues**: 0 (P0=0, P1=0, P2=0)
- **Greptile**: ✅ Approved

**Action**:
```bash
# Merge PR #10
gh pr merge 10 --squash -t "feat: EPIC-CCN-075 Infrastructure extraction (Wave 4)"

# Keep Phase 5-6 outputs
# ✅ docs/brain/EPIC-CCN-075/ticket-1-completion.md
# ✅ docs/brain/EPIC-CCN-075/06-completion-report.md
```

**Rationale**: This epic executed cleanly with no behavioral changes, no Jane Street violations, and passed all Greptile checks. No reason to rollback.

---

### 2. EPIC-CCN-027 (SIMA Dispatch) - INVALID ❌

**Status**: INVALID - Target method doesn't exist

**Details**:
- **Target Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Expected File**: `src/V12_002.SIMA.Dispatch.cs`
- **Search Result**: NOT FOUND in any `.cs` file
- **Root Cause**: Stale jCodemunch index (method was removed/renamed)

**Evidence**:
- ✅ Phases 0-4 complete (based on stale index)
- ✅ TICKET-1 complete (created `BracketOrderSet` struct)
- ❌ TICKET-2 BLOCKED (target method not found)
- ❌ No Phase 6 completion

**Action**:
```bash
# Mark epic as INVALID in roadmap
# Keep Phase 0-4 outputs (for forensic analysis)
# Delete Phase 5 outputs (incomplete)
rm docs/brain/EPIC-CCN-027/ticket-*-completion.md

# Update roadmap
python scripts/update_epic_status.py EPIC-CCN-027 --status INVALID \
  --reason "Target method Dispatch_PublishMarketBracketToPhoton not found in codebase"
```

**Rationale**: Cannot execute an epic for a method that doesn't exist. This is a data quality issue, not a protocol issue.

**Lesson Learned**: Add Phase -1 (Pre-Flight) validation to verify target method exists before starting epic workflow.

---

### 3. EPIC-CCN-24 (DrawingHelpers) - LOCAL EXECUTION ⚠️

**Status**: REQUIRES LOCAL EXECUTION - Encoding issues

**Details**:
- **Target Method**: `DrawORBox`
- **File**: `src/V12_002.DrawingHelpers.cs`
- **Issue**: File contains non-ASCII characters (UTF-16 encoding)
- **Status in Wave 4**: Pending (not executed yet)

**Why Local Execution**:
1. VM execution may corrupt encoding
2. Git treats file as binary with encoding issues
3. Bob CLI on VM may introduce encoding errors
4. Local execution allows manual encoding verification

**Action**:
```bash
# Execute locally (not on VM)
cd c:/WSGTA/universal-or-strategy

# Run Bob CLI locally with encoding-safe mode
bob --mode v12-engineer --local "Execute Phase 5 for EPIC-CCN-24"

# Verify encoding after extraction
python scripts/verify_ascii_only.py src/V12_002.DrawingHelpers.cs

# If encoding issues detected, fix manually
# Then commit and push
```

**Protocol Reference**: See `docs/protocol/LOCAL_EXECUTION_PROTOCOL.md` (to be created in protocol hardening phase)

---

## Rollback Scope Summary

### KEEP (No Re-execution Needed)

#### Phases 0-4 for ALL 80 Epics (480 files)
```
✅ docs/brain/EPIC-CCN-{001..080}/00-hotspots.md
✅ docs/brain/EPIC-CCN-{001..080}/00-scope.md
✅ docs/brain/EPIC-CCN-{001..080}/01-scope-boundary.md
✅ docs/brain/EPIC-CCN-{001..080}/02-architecture-plan.md
✅ docs/brain/EPIC-CCN-{001..080}/03-audit-report.md
✅ docs/brain/EPIC-CCN-{001..080}/04-tickets.md
```

**Rationale**: These are objective analysis phases - no code changes, no behavioral issues. Reusing saves $8 + 16 hours VM time.

#### Phase 5-6 for EPIC-CCN-075 (2 files)
```
✅ docs/brain/EPIC-CCN-075/ticket-1-completion.md
✅ docs/brain/EPIC-CCN-075/06-completion-report.md
✅ src/V12_002.Infrastructure.cs (via PR #10)
```

**Rationale**: Clean execution, 0 issues, Greptile approved.

---

### ROLLBACK (Re-execute with Fixed Protocol)

#### Phase 5-6 for 77 Epics (154 files)
```
❌ docs/brain/EPIC-CCN-{001..023,025..074,076..080}/ticket-*-completion.md
❌ docs/brain/EPIC-CCN-{001..023,025..074,076..080}/06-completion-report.md
❌ All src/ changes in PRs #11-16 (28 files)
```

**Excluded**:
- EPIC-CCN-024: Execute locally (not on VM)
- EPIC-CCN-027: Invalid (skip entirely)
- EPIC-CCN-075: Already done (keep)

**Rationale**: 28 critical issues (9 P0, 12 P1, 6 P2) due to Bob CLI over-optimization.

---

## Wave 4 Retry Execution Plan

### Phase 1: Rollback (30 minutes)

```bash
# 1. Merge clean PR #10
gh pr merge 10 --squash

# 2. Close buggy PRs #11-16
gh pr close 16 15 14 13 12 11 -c "Rollback: Bob CLI behavioral changes"

# 3. Delete feature branches
git branch -D wave4-pr{1..6}-*
git push origin --delete wave4-pr{1..6}-*

# 4. Reset gitbutler/workspace
git checkout gitbutler/workspace
git reset --hard <commit-before-phase5>
git push origin gitbutler/workspace --force

# 5. Delete Phase 5-6 outputs (77 epics)
rm docs/brain/EPIC-CCN-{001..023,025..074,076..080}/ticket-*-completion.md
rm docs/brain/EPIC-CCN-{001..023,025..074,076..080}/06-completion-report.md

# 6. Mark EPIC-CCN-027 as INVALID
python scripts/update_epic_status.py EPIC-CCN-027 --status INVALID

# 7. Commit rollback
git add docs/brain/
git commit -m "rollback: Wave 4 Phase 5-6 for 77 epics"
git push origin gitbutler/workspace
```

### Phase 2: Protocol Hardening (8 hours)

See `WAVE4_PROTOCOL_HARDENING_PLAN.md` for complete details.

**Critical Updates**:
1. Phase 5 Execution Protocol (SURGICAL ONLY mandate)
2. Phase 5.V Verification Protocol (5 checks not 2)
3. Local Execution Protocol (encoding detection)
4. Test Generation Protocol (xUnit mandatory)
5. Phase -1 Pre-Flight Protocol (method existence check)

### Phase 3: Pilot Test (2 hours)

```bash
# Test EPIC-CCN-001 with hardened protocols
python scripts/pilot_test.py --epic EPIC-CCN-001 --phase 5

# Success criteria:
# ✅ No behavioral changes
# ✅ No Jane Street violations
# ✅ Tests generated and passing
# ✅ Greptile review clean (0 P0/P1 issues)
```

### Phase 4: Wave 4 Retry (28 hours)

#### VM Execution (77 epics)
```bash
# Launch Phase 5-6 for 77 epics on VM
# Exclude: EPIC-CCN-024 (local), EPIC-CCN-027 (invalid), EPIC-CCN-075 (done)

python scripts/launch_wave4_retry.py \
  --epics 001-023,025-074,076-080 \
  --exclude 024,027,075 \
  --phases 5,6 \
  --stagger 12s
```

#### Local Execution (1 epic)
```bash
# Execute EPIC-CCN-024 locally
cd c:/WSGTA/universal-or-strategy
bob --mode v12-engineer --local "Execute Phase 5 for EPIC-CCN-24"

# Verify encoding
python scripts/verify_ascii_only.py src/V12_002.DrawingHelpers.cs

# Commit and push
git add src/V12_002.DrawingHelpers.cs tests/
git commit -m "feat: EPIC-CCN-024 Phase 5 (local execution)"
git push origin gitbutler/workspace
```

---

## Cost Analysis

### Already Paid (KEEP)
- Phases 0-4 (80 epics): $8.00 ✅
- EPIC-CCN-075 Phase 5-6: $0.10 ✅
- **Total Kept**: $8.10

### Sunk Cost (ROLLBACK)
- Phase 5-6 (78 epics): $3.90 ❌
- **Total Lost**: $3.90

### New Cost (RETRY)
- Phase 5-6 (77 epics on VM): $3.85 🔄
- Phase 5-6 (1 epic local): $0.05 🔄
- **Total New**: $3.90

### Grand Total
- Kept: $8.10
- Lost: $3.90
- Retry: $3.90
- **Wave 4 Total**: $15.90

**Savings vs. Full Redo**: $4.10 (20% savings)

---

## Timeline Estimate

| Phase | Duration | Type |
|-------|----------|------|
| Rollback | 30 min | Human |
| Protocol Hardening | 8 hours | Human |
| Pilot Test | 2 hours | Human |
| VM Retry (77 epics) | 24 hours | VM (parallel) |
| Local Execution (1 epic) | 30 min | Human |
| Monitoring | 4 hours | Human (4-min polling) |
| PR Creation | 2 hours | Human |
| PR Merge | 2 hours | Human |
| **Total** | **43 hours** | **19h human + 24h VM** |

---

## Success Criteria

### Rollback Complete When:
- ✅ PR #10 merged to main
- ✅ PRs #11-16 closed
- ✅ Phase 5-6 outputs deleted (77 epics)
- ✅ EPIC-CCN-027 marked INVALID
- ✅ gitbutler/workspace reset to pre-Phase 5

### Retry Complete When:
- ✅ All 77 VM epics execute cleanly
- ✅ EPIC-CCN-024 executes locally with no encoding issues
- ✅ All PRs pass Greptile with 0 P0/P1 issues
- ✅ Build passes after each PR merge
- ✅ No behavioral changes detected
- ✅ All Jane Street rules followed
- ✅ All tests pass (including new xUnit tests)

---

## Final Answer to User's Question

**Q**: Do all epics need phases 5 and 6 again including the files from PR 10?

**A**: 
- **NO** - PR #10 (EPIC-CCN-075) is CLEAN - keep it ✅
- **NO** - EPIC-CCN-027 is INVALID - skip it ❌
- **YES** - 77 other epics need Phase 5-6 retry with fixed protocol 🔄
- **SPECIAL** - EPIC-CCN-24 needs local execution (not VM) ⚠️

**Summary**:
- **Keep**: 1 epic (EPIC-CCN-075)
- **Skip**: 1 epic (EPIC-CCN-027)
- **Retry on VM**: 77 epics
- **Retry locally**: 1 epic (EPIC-CCN-24)
- **Total retry**: 78 epics (not 79, not 80)

---

**Status**: 🟢 COMPLETE CLARIFICATION  
**Next Action**: Execute rollback, then protocol hardening  
**Estimated Completion**: 4 days (19 hours human + 24 hours VM)