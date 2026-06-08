# Epic: EPIC-CCN-1 -- Sentinel Audit (Semantic Scan)

**Date:** 2026-06-08  
**Auditor:** V12 Epic Planner (Sentinel Mode)  
**Protocol:** V12 Photon Kernel -- Phase 2.3 Independent Review  
**Tools Used:** jCodemunch-MCP (fallback), complexity_audit.py, direct file inspection

---

## Executive Summary

**VERDICT: REVISION REQUIRED**

The analysis and approach documents are based on **STALE CODE** that does not match the current codebase state. The claimed refactoring from Build 1108.004 (CYC 71 → 10) **NEVER OCCURRED**. Three of the four target methods **DO NOT EXIST** in the current codebase.

**Critical Findings:**
1. ❌ **Scope Document Mismatch**: Analysis claims `HydrateFSMsFromWorkingOrders` is 57 lines (CYC 10). Reality: **188 lines (CYC 71)**.
2. ❌ **Missing Methods**: 3 of 4 target methods (`HydrateFSM_MapOrderStateToFsmState`, `HydrateFSM_RecoverFromOpenPositions`, `RecoverFSM_FindAccountWithPosition`) **do not exist**.
3. ❌ **False Extraction Claims**: Analysis document claims 9 helper methods were already extracted. Reality: The code is **INLINE** (not extracted).
4. ⚠️ **Index Staleness**: jCodemunch index is from May 19, 2026 and does not reflect current code state.

**Impact:** The entire epic scope is invalid. The Director must re-run `/epic-intake` and `/epic-plan` against the **ACTUAL CURRENT CODE**.

---

## Semantic Gap Analysis

### Gap 1: Phantom Refactoring (Build 1108.004)

**Analysis Document Claims:**
> "Build 1108.004: Extracted 9 helper methods → CYC 71 → 10 (86% reduction)"

**Reality Check:**
```bash
$ python scripts/complexity_audit.py | grep HydrateFSMsFromWorkingOrders
V12_002.SIMA.Lifecycle.cs::HydrateFSMsFromWorkingOrders (CYC=71, LOC=188)
```

**Evidence:**
- Current file: `src/V12_002.SIMA.Lifecycle.cs` lines 464-651 (188 lines)
- Complexity: **CYC 71** (unchanged from original scope document)
- Structure: **INLINE CODE** (no extracted helpers)

**Conclusion:** The claimed Build 1108.004 refactoring either:
1. Never happened
2. Was reverted
3. Exists in a different branch not merged to current working branch

### Gap 2: Missing Target Methods

**Analysis Document Target Methods:**
1. `HydrateFSM_MapOrderStateToFsmState` (CYC 9 → 2) ❌ **NOT FOUND**
2. `HydrateFSMsFromWorkingOrders` (CYC 10 → 5) ✅ **EXISTS** (but CYC 71, not 10)
3. `HydrateFSM_RecoverFromOpenPositions` (CYC 11 → 6) ❌ **NOT FOUND**
4. `RecoverFSM_FindAccountWithPosition` (CYC 9 → 4) ❌ **NOT FOUND**

**Search Results:**
```bash
$ Select-String -Path 'src/V12_002.SIMA.Lifecycle.cs' -Pattern 'HydrateFSM_MapOrderStateToFsmState|HydrateFSM_RecoverFromOpenPositions|RecoverFSM_FindAccountWithPosition'
# No matches found
```

**Actual Code Structure:**
The `HydrateFSMsFromWorkingOrders` method (lines 464-651) contains:
- **Entry Order Pass** (lines 469-599): Inline foreach loop with state mapping logic
- **Position Pass** (lines 601-750): Inline foreach loop with recovery logic
- **NO EXTRACTED HELPER METHODS**

**State Mapping Logic** (lines 488-503):
```csharp
// This is INLINE, not extracted to HydrateFSM_MapOrderStateToFsmState
FollowerBracketState hydrationState;
OrderState entryState = entryOrder.OrderState;
if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
    hydrationState = FollowerBracketState.Active;
else if (entryState == OrderState.Accepted)
    hydrationState = FollowerBracketState.Accepted;
else if (entryState == OrderState.Working || entryState == OrderState.Submitted || ...)
    hydrationState = FollowerBracketState.Submitted;
else
    continue; // Terminal state
```

**Position Recovery Logic** (lines 603-656):
```csharp
// This is INLINE, not extracted to HydrateFSM_RecoverFromOpenPositions
foreach (Account acct in Account.All)
{
    if (!IsFleetAccount(acct)) continue;
    
    // Account finder logic INLINE (not extracted to RecoverFSM_FindAccountWithPosition)
    if (_followerBrackets.Values.Any(f => string.Equals(f.AccountName, acct.Name, ...)))
        continue;
    
    Position acctPos = acct.Positions.FirstOrDefault(p => ...);
    if (acctPos == null) continue;
    
    // ... 50+ lines of inline recovery logic
}
```

### Gap 3: Approach Document Extraction Strategy

**Approach Document Claims:**
> "Decision 2: Extract loop body → `ProcessSinglePositionRecovery()` returning `(bool success, bool shouldContinue)`"

**Reality:** There is no `while(true)` loop to extract from. The Position Pass uses a **foreach loop** (line 603), not a `while(true)` loop.

**Approach Document Claims:**
> "Decision 3: Dictionary lookup for state mapping → CYC 9 → 2"

**Reality:** The state mapping is already a simple if-else chain (CYC ~5), not the complex 7-way branching described in the analysis.

---

## Integration Risks

### Risk 1: Scope Creep from Stale Analysis

**Issue:** The approach document proposes extracting 4 new helper methods based on code that doesn't exist in the current state.

**Impact:** If executed as written, the epic would:
1. Attempt to extract methods that are already inline
2. Miss the actual complexity drivers in the 188-line method
3. Fail to achieve Jane Street GODMODE compliance (CYC ≤8)

**Mitigation Required:** Re-analyze the **ACTUAL 188-line method** to identify real extraction candidates.

### Risk 2: Hidden Dependencies Not Analyzed

**Issue:** The analysis document claims to have used jCodemunch tools (`get_blast_radius`, `find_references`, `get_dependency_graph`) but the index is stale (May 19, 2026).

**Evidence:**
```json
{
  "indexed_at": "2026-05-19T22:21:26.659098",
  "symbol_count": 20614,
  "file_count": 2000
}
```

**Impact:** Any blast radius or dependency analysis is based on **OLD CODE** and cannot be trusted.

**Mitigation Required:** Re-index the repository before proceeding:
```bash
jcodemunch index_folder --path . --use_ai_summaries true
```

### Risk 3: Position Pass Grace Window Logic

**Actual Code Finding** (lines 651-656):
```csharp
// Build 999: Mark account for REAPER grace window
_positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
```

**Analysis Document Claims:** This logic is preserved in the extraction plan.

**Reality Check:** ✅ **CORRECT** - This logic does exist and must be preserved during any refactoring.

### Risk 4: FSM State Transitions

**Actual Code Finding** (lines 520-528):
```csharp
var fsm = new FollowerBracketFSM
{
    AccountName = pi.ExecutingAccount.Name,
    EntryName = entryKey,
    State = hydrationState,
    RemainingContracts = hydratedRemainingContracts,
    LastUpdateUtc = DateTime.UtcNow,
    EntryOrder = entryOrder,
};
```

**Analysis Document Claims:** FSM creation logic must be preserved exactly.

**Reality Check:** ✅ **CORRECT** - This is the actual FSM creation logic and must be preserved.

---

## DNA Violation Detection

### Violation 1: Complexity Threshold Exceeded

**V12 DNA Mandate:** Jane Street GODMODE threshold = CYC ≤8

**Current State:**
- `HydrateFSMsFromWorkingOrders`: **CYC 71** (8.9x over threshold)
- Status: **BLOCKING VIOLATION**

**Analysis Document Claims:** Only 4 methods exceed threshold (CYC 9-11).

**Reality:** The **ENTIRE METHOD** is a God-method (CYC 71) that requires full extraction, not "final polish".

### Violation 2: Lock-Free Compliance

**Analysis Document Claims:** Zero `lock()` statements verified.

**Reality Check:**
```bash
$ grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
# No matches (CORRECT)
```

**Status:** ✅ **COMPLIANT** - No lock statements found.

### Violation 3: ASCII-Only Compliance

**Analysis Document Claims:** No Unicode in string literals.

**Reality Check:** Not verified in this audit (requires deploy-sync.ps1 ASCII gate).

**Status:** ⚠️ **UNVERIFIED** - Must run `deploy-sync.ps1` to confirm.

### Violation 4: Extraction Floor (≥15 LOC)

**Analysis Document Claims:** All proposed extractions meet ≥15 LOC floor.

**Reality:** Cannot verify because the proposed extractions are based on non-existent code structure.

**Status:** ⚠️ **UNVERIFIABLE** - Requires re-analysis of actual code.

---

## Actual Code Structure Analysis

### Current Method Structure (Lines 464-651, 188 LOC)

**Entry Order Pass** (lines 469-599, ~130 LOC):
```
foreach (var kvp in entryOrders.ToArray())
├── Guard clauses (lines 473-485): 4 early-continue conditions
├── State mapping (lines 488-503): 5-way if-else chain
├── Live position lookup (lines 505-518): LINQ query with null checks
├── FSM creation (lines 520-528): Struct initialization
├── Stop order linking (lines 531-540): Dictionary lookup + indexing
└── Target order linking (lines 543-588): 5x repeated blocks (T1-T5)
```

**Position Pass** (lines 601-750, ~150 LOC):
```
foreach (Account acct in Account.All)
├── Fleet account filter (lines 605-606)
├── Existing FSM check (lines 609-614): LINQ Any() with string comparison
├── Open position lookup (lines 617-621): LINQ FirstOrDefault()
├── Stop order scan (lines 624-641): Nested foreach with account matching
├── Grace window logic (lines 643-656): REAPER deferral for CancelPending
├── FSM creation (lines 662-670): Struct initialization for recovery
├── Stop order linking (lines 673-681)
└── Target order linking (lines 684-750): 5x repeated blocks (T1-T5)
```

**Actual Complexity Drivers:**
1. **Nested loops** (Position Pass: outer foreach + inner foreach)
2. **LINQ predicates** (Any(), FirstOrDefault() with complex lambdas)
3. **Repeated target linking blocks** (5x identical structure for T1-T5)
4. **Guard clause chains** (4+ early-continue conditions)
5. **State mapping logic** (5-way if-else chain)

**Recommended Extraction Candidates** (based on actual code):
1. `ValidateEntryForHydration(kvp, out PositionInfo pi)` → Extract lines 473-485 (guard clauses)
2. `MapOrderStateToFsmState(OrderState state)` → Extract lines 488-503 (state mapping)
3. `LinkTargetOrdersToFsm(ref FollowerBracketFSM fsm, string entryKey, ref int ordersIndexed)` → Extract lines 543-588 (repeated T1-T5 blocks)
4. `FindAccountWithOpenPosition()` → Extract lines 603-621 (account finder logic)
5. `RecoverFsmFromPosition(Account acct, ref int fsmCreated, ref int ordersIndexed)` → Extract lines 624-750 (recovery logic)

**Expected Reduction:** CYC 71 → ~15-20 (5 extractions, each reducing CYC by 10-15 points)

---

## Sentinel Verdict

**REVISION REQUIRED**

### Critical Blockers

1. **Scope Mismatch**: Analysis and approach documents describe code that does not exist in the current codebase.
2. **Phantom Refactoring**: Claimed Build 1108.004 extraction (CYC 71 → 10) never occurred or was reverted.
3. **Missing Methods**: 3 of 4 target methods do not exist in the current file.
4. **Stale Index**: jCodemunch index is from May 19, 2026 and cannot be trusted for dependency analysis.

### Required Actions

**IMMEDIATE (Director):**
1. ❌ **HALT EPIC EXECUTION** - Do not proceed to `/epic-validate` or `/epic-tickets`
2. 🔄 **RE-RUN /epic-intake** - Verify current code state against scope document
3. 🔄 **RE-RUN /epic-plan** - Generate new analysis and approach based on **ACTUAL CODE**
4. 🔧 **RE-INDEX REPOSITORY** - Run `jcodemunch index_folder --path . --use_ai_summaries true`

**BEFORE RE-PLANNING:**
1. Verify current BUILD_TAG in `src/V12_002.cs`
2. Check git history for Build 1108.004 commits
3. Confirm which branch contains the claimed refactoring (if any)
4. Run `complexity_audit.py` to establish baseline metrics

**NEW EPIC SCOPE (Recommended):**
- **Target Method:** `HydrateFSMsFromWorkingOrders` (CYC 71, 188 LOC)
- **Goal:** Extract 5-6 helper methods to achieve CYC ≤8 compliance
- **Effort:** 6-8 hours (full God-method extraction, not "final polish")
- **Risk:** MEDIUM-HIGH (188-line method with nested loops and LINQ)

---

## Appendix: Tool Output

### Complexity Audit Output (Relevant Section)

```
=== FILE: V12_002.SIMA.Lifecycle.cs ===
| Method                                   |   LOC | Est. CYC | M5 Candidate?  | Action               |
|------------------------------------------|-------|----------|----------------|----------------------|
| HydrateFSMsFromWorkingOrders             |   188 |       71 |                | REFACTOR, LOC>80     |
| AdoptFleetOrders                         |    95 |       24 |                | REFACTOR             |
| SweepBrokerOrders                        |    67 |       24 |                | REFACTOR             |
| HydrateWorkingOrdersFromBroker           |   110 |       19 |                | REFACTOR, LOC>80     |
| AdoptMasterOrders                        |    42 |       19 |                | REFACTOR             |
| HydrateExpectedPositionsFromBroker       |    65 |       17 |                | REFACTOR             |
| ClassifyOrderByPrefix                    |    21 |       17 |                | REFACTOR             |
| ProcessShutdownSIMA                      |    25 |       11 |                | REFACTOR             |
| SweepTrackedOrders                       |    32 |       10 |                | REFACTOR             |
```

### File Search Output

```powershell
PS> Select-String -Path 'src/V12_002.SIMA.Lifecycle.cs' -Pattern 'HydrateFSM_MapOrderStateToFsmState|HydrateFSMsFromWorkingOrders|HydrateFSM_RecoverFromOpenPositions|RecoverFSM_FindAccountWithPosition'

LineNumber Line
---------- ----
       445             HydrateFSMsFromWorkingOrders();
       464         private void HydrateFSMsFromWorkingOrders()
```

**Interpretation:** Only `HydrateFSMsFromWorkingOrders` exists. The other 3 methods are not present.

### jCodemunch Index Status

```json
{
  "found": true,
  "indexed": true,
  "repo": "local/universal-or-strategy-17657650",
  "source_root": "C:\\WSGTA\\universal-or-strategy",
  "display_name": "universal-or-strategy",
  "symbol_count": 20614,
  "file_count": 2000,
  "indexed_at": "2026-05-19T22:21:26.659098"
}
```

**Interpretation:** Index is **19 days stale** (May 19 → June 8). Any semantic analysis based on this index is unreliable.

---

## Conclusion

The Sentinel Audit has identified a **CRITICAL SCOPE MISMATCH** between the planning documents and the actual codebase. The analysis and approach documents are based on code that does not exist in the current working branch.

**The epic cannot proceed to validation or ticket generation until the Director re-runs `/epic-intake` and `/epic-plan` against the ACTUAL CURRENT CODE.**

This is not a minor discrepancy - it represents a fundamental misalignment between planned work and reality. Proceeding with the current plan would result in:
1. Wasted engineering effort (extracting non-existent methods)
2. Failed acceptance criteria (CYC targets based on wrong baseline)
3. Potential regression (modifying code without understanding current state)

**Recommendation:** Treat this as a **SCOPE RESET** and restart the epic planning process from Phase 1 (/epic-intake).

---

**[SENTINEL-GATE] Semantic Scan complete. Awaiting Director acknowledgment of REVISION REQUIRED verdict.**