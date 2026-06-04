# PR #23 Fix Queue (C# Files Only)

**PR**: #23 - EPIC-CCN-13: Extract MonitorRMAProximity
**Branch**: `epic-ccn-13-extract-monitor-rma-proximity`
**Date**: 2026-06-03
**Protocol**: PR Loop V2 - Step 2 (Filtered)

---

## Filtering Summary

**Total Issues Found**: 6
**C# Issues (.cs files)**: 6 ✅
**Non-C# Issues (filtered out)**: 0
**Files with Issues**: 1
- `src/V12_002.Entries.RMA.cs` (6 issues)

**Files in PR (8 total)**:
- ✅ `src/V12_002.Entries.RMA.cs` - C# (6 issues)
- ❌ `.bob/commands/epic-run.md` - Markdown (no issues)
- ❌ `.bob/commands/epic-tdd.md` - Markdown (no issues)
- ❌ `.bob/commands/pr-loop.md` - Markdown (no issues)
- ❌ `.github/workflows/codescene-quality-gate.yml` - YAML (no issues)
- ❌ `docs/protocol/CODESCENE_INTEGRATION.md` - Markdown (no issues)
- ❌ `scripts/epic_planner.py` - Python (no issues)
- ❌ `scripts/pre_push_validation.ps1` - PowerShell (no issues)

---

## C# Fix Queue (6 items)

### Fix #1: Line 393 - Missing braces (Codacy)
**Location**: `src/V12_002.Entries.RMA.cs:393`
**Issue**: Missing curly braces around nested if statement
**Priority**: P0
**Type**: [VALID-FIX]
**Action**: Add braces around if block

---

### Fix #2: Line 420 - Missing braces (CodeFactor)
**Location**: `src/V12_002.Entries.RMA.cs:420`
**Issue**: `return false;` - Missing braces around single-statement if block
**Priority**: P0
**Type**: [VALID-FIX]
**Action**: Add braces around return statement

---

### Fix #3: Line 422 - Missing braces (Codacy)
**Location**: `src/V12_002.Entries.RMA.cs:422`
**Issue**: Missing curly braces around nested if statement
**Priority**: P0
**Type**: [VALID-FIX]
**Action**: Add braces around if block

---

### Fix #4: Line 423 - Missing braces (CodeFactor)
**Location**: `src/V12_002.Entries.RMA.cs:423`
**Issue**: `return false;` - Missing braces around single-statement if block
**Priority**: P0
**Type**: [VALID-FIX]
**Action**: Add braces around return statement

---

### Fix #5: Line 502 - Missing braces (Codacy)
**Location**: `src/V12_002.Entries.RMA.cs:502`
**Issue**: Missing curly braces around nested if statement
**Priority**: P0
**Type**: [VALID-FIX]
**Action**: Add braces around if block

---

### Fix #6: Line 503 - Missing braces (CodeFactor)
**Location**: `src/V12_002.Entries.RMA.cs:503`
**Issue**: `RemoveDrawObject("Prox_" + entryName);` - Missing braces around single-statement if block
**Priority**: P0
**Type**: [VALID-FIX]
**Action**: Add braces around RemoveDrawObject call

---

## Execution Plan for Bob CLI

**Tool**: `apply_diff` (surgical edits)
**Target File**: `src/V12_002.Entries.RMA.cs`
**Strategy**: 
1. Read file to get exact context around each line
2. Apply diffs in ascending line order (393, 420, 422, 423, 502, 503)
3. Run CSharpier to verify formatting
4. Run build to verify no compilation errors
5. Commit with message: "fix: Add missing braces (PR #23 bot findings)"

**Verification**:
- [ ] All 6 fixes applied
- [ ] CSharpier check passes
- [ ] Build succeeds
- [ ] No new bot findings

---

## Status

- [x] Forensics extracted
- [x] Fix queue created
- [x] C# filtering complete
- [ ] Fixes applied (Step 2 - Bob CLI)
- [ ] Verified and pushed (Step 3)
- [ ] PHS loop complete (Step 4)

**Next Action**: Delegate to Bob CLI (`v12-engineer` mode) with this filtered fix queue