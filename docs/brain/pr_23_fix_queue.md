# PR #23 Fix Queue

**PR**: #23 - EPIC-CCN-13: Extract MonitorRMAProximity
**Branch**: `epic-ccn-13-extract-monitor-rma-proximity`
**Date**: 2026-06-03
**Protocol**: PR Loop V2 - Step 2 Preparation

---

## Fix Queue (6 items)

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

## Execution Plan

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
- [ ] Fixes applied (Step 2)
- [ ] Verified and pushed (Step 3)
- [ ] PHS loop complete (Step 4)

**Next Action**: Switch to v12-engineer mode and execute Step 2 (Local Repair)