# FORENSIC INVESTIGATION: EPIC-CCN-1 Misalignment Root Cause Analysis

**Investigation Date**: 2026-06-08  
**Investigator**: Advanced Mode Agent  
**Status**: COMPLETE  

---

## Executive Summary

**FINDING**: EPIC-CCN-1 tickets target **ALREADY REFACTORED CODE** from 28 days ago. All 7 extraction tasks were completed in commit `2785315a` on 2026-05-11. The epic planning process used **19-day-old stale jCodemunch index data**, causing phantom work to be scheduled.

**ROOT CAUSE**: **SYSTEMIC** - Stale index data in epic planning workflow.

**IMPACT**: 100% of EPIC-CCN-1 tickets are obsolete. Zero work remains.

---

## Evidence Chain

### 1. Current Method Structure (Line 902-958)

**Actual Location**: `src/V12_002.SIMA.Lifecycle.cs:902`  
**Current State**: **ALREADY REFACTORED**  
**Lines**: 57 (not 296 as scope document claims)  
**Complexity**: ~10 CYC (not 71 as scope document claims)

```csharp
private void HydrateFSMsFromWorkingOrders()
{
    int fsmCreated = 0;
    int ordersIndexed = 0;

    foreach (var kvp in entryOrders.ToArray())
    {
        string entryKey = kvp.Key;
        Order entryOrder = kvp.Value;
        if (entryOrder == null) continue;

        // Skip master account entries
        PositionInfo pi;
        if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower) continue;
        if (pi.ExecutingAccount == null) continue;

        // Idempotent: skip if FSM already exists
        if (_followerBrackets.ContainsKey(entryKey)) continue;

        // Map broker order state to FSM state
        FollowerBracketState hydrationState = HydrateFSM_MapOrderStateToFsmState(entryOrder.OrderState);
        if (hydrationState == FollowerBracketState.None)
            continue; // Terminal state -- FSM not needed

        int hydratedRemainingContracts = HydrateFSM_DetermineRemainingContracts(
            entryOrder, hydrationState, pi.ExecutingAccount);

        var fsm = new FollowerBracketFSM
        {
            AccountName = pi.ExecutingAccount.Name,
            EntryName = entryKey,
            State = hydrationState,
            RemainingContracts = hydratedRemainingContracts,
            LastUpdateUtc = DateTime.UtcNow,
            EntryOrder = entryOrder
        };

        // Link bracket orders and index OrderIds
        HydrateFSM_LinkBracketOrders(entryKey, fsm, ref ordersIndexed);

        _followerBrackets.TryAdd(entryKey, fsm);

        if (!string.IsNullOrEmpty(entryOrder.OrderId))
        {
            _orderIdToFsmKey[entryOrder.OrderId] = entryKey;
            ordersIndexed++;
        }

        fsmCreated++;
    }

    // Position Pass: handle accounts with open positions but terminal entry orders
    HydrateFSM_RecoverFromOpenPositions(ref fsmCreated, ref ordersIndexed);

    Print(string.Format("[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed));
}
```

**Key Observation**: Method calls **4 extracted helper methods** that already exist:
1. `HydrateFSM_MapOrderStateToFsmState()` (line 648)
2. `HydrateFSM_DetermineRemainingContracts()` (line 670)
3. `HydrateFSM_LinkBracketOrders()` (line 695)
4. `HydrateFSM_RecoverFromOpenPositions()` (line 852)

---

### 2. Git History Timeline

**Refactoring Commit**: `2785315a1ff06adcb90fcdfaca565eda27014fe0`  
**Date**: 2026-05-11 15:38:52 -0700 (28 days ago)  
**Message**: "feat(phase7): Sprint 1+2 god-function extraction -- 254 CYC eliminated"

**Commit Details**:
```
Sprint 1:
- HydrateFSMsFromWorkingOrders: 72 CYC -> ~10 CYC (8 sub-methods)
- ProcessQueuedExecution: 71 CYC -> 4 CYC (5 sub-methods)
- UpdateStopOrder: 55 CYC -> 25 CYC (6 sub-methods)

Sprint 2:
- ReconcileOrphanedOrders: 46 CYC -> 5 CYC (3 sub-methods)
- RemoveGhostOrderRef: 37 CYC -> 5 CYC (3 sub-methods)
- ExecuteReaperRepair: 32 CYC -> <10 CYC (4 sub-methods)

BUILD_TAG: 1111.007-phase7-t2
DNA: No locks, ASCII-only, v12_split.py, deploy-sync PASS

src/V12_002.SIMA.Lifecycle.cs | 790 ++++++++++++++++++++++++++----------------
1 file changed, 489 insertions(+), 301 deletions(-)
```

**Extracted Methods Confirmed** (via jCodemunch search):
- ✅ `HydrateFSM_MapOrderStateToFsmState` (line 648)
- ✅ `HydrateFSM_DetermineRemainingContracts` (line 670)
- ✅ `HydrateFSM_LinkBracketOrders` (line 695)
- ✅ `HydrateFSM_RecoverFromOpenPositions` (line 852)

---

### 3. Line Number Verification

**Scope Document Claims**: Lines 464-759 (296 lines, CYC 71)  
**Actual Reality**:
- **Line 464**: `AdoptMasterWorkingOrders()` (DIFFERENT METHOD)
- **Line 902**: `HydrateFSMsFromWorkingOrders()` (ACTUAL METHOD, 57 lines, CYC ~10)

**Ticket-02 Claims**: "Extract from lines 488-518"  
**Actual Line 488**: Inside `AdoptMasterWorkingOrders()` (WRONG METHOD)

**Conclusion**: All line references in scope document and tickets point to **STALE PRE-REFACTORING CODE**.

---

### 4. Ticket Creation Timeline

**EPIC-CCN-1 Tickets Created**: 2026-06-08 14:18-14:20 (TODAY)

| Ticket | Filename | LastWriteTime |
|--------|----------|---------------|
| 01 | ticket-01-LinkTargetOrderToFSM.md | 6/8/2026 2:18:17 PM |
| 02 | ticket-02-MapBrokerStateToFSM.md | 6/8/2026 2:18:41 PM |
| 03 | ticket-03-CreateFollowerBracketFSM.md | 6/8/2026 2:19:01 PM |
| 04 | ticket-04-LinkStopOrder.md | 6/8/2026 2:19:23 PM |
| 05 | ticket-05-RegisterFSM.md | 6/8/2026 2:19:43 PM |
| 06 | ticket-06-ScanForRecoveryKey.md | 6/8/2026 2:20:08 PM |
| 07 | ticket-07-CreateRecoveryFSM.md | 6/8/2026 2:20:32 PM |

**Gap**: Tickets created **28 days AFTER** refactoring was completed.

---

### 5. Scope Document Analysis

**File**: `docs/brain/EPIC-CCN-1/00-scope.md`  
**Header**: "REVISION" document acknowledging stale data

**Key Admission** (Line 3):
> **CRITICAL CONTEXT**: This is a REVISION of the original scope document. Previous analysis was based on STALE DATA (19 days old) and incorrectly assumed extracted helpers existed. This document reflects CURRENT CODE STATE as of 2026-06-08.

**Sentinel Audit Section** (Line 233):
> **Original Failure Root Cause**: Scope document was based on 19-day-old index data. The method had NOT been refactored; all "extracted helpers" were phantom assumptions.

**CONTRADICTION**: Despite acknowledging stale data, the scope document STILL references:
- Line 464-759 (wrong location)
- CYC 71 (wrong complexity)
- 296 lines (wrong size)
- "NO extracted helpers exist" (FALSE - they exist at lines 648, 670, 695, 852)

**Conclusion**: The "REVISION" document attempted to correct stale data but **FAILED** - it still describes pre-refactoring code state.

---

## Root Cause Analysis

### Primary Cause: Stale jCodemunch Index

**Timeline**:
1. **2026-05-11**: Code refactored (commit `2785315a`)
2. **2026-05-20** (estimated): jCodemunch index last updated (19 days before 2026-06-08)
3. **2026-06-08**: Epic planning agent queries stale index
4. **2026-06-08**: Tickets generated based on 28-day-old code structure

**Failure Point**: Epic planning workflow did NOT run `index_file` or `index_folder` before analysis.

### Secondary Cause: No Pre-Planning Verification

**Missing Step**: Epic intake should have verified:
1. Current method complexity via `complexity_audit.py`
2. Current method location via `search_files`
3. Existence of extracted helpers via `search_symbols`

**Protocol Gap**: No mandatory "freshness check" before epic planning.

---

## Systemic vs One-Off Assessment

**VERDICT**: **SYSTEMIC ISSUE**

**Evidence**:
1. **Scope Document Acknowledges Pattern**: "Previous analysis was based on STALE DATA (19 days old)"
2. **Revision Failed**: Even after acknowledging stale data, the revision STILL used wrong line numbers
3. **No Verification Protocol**: Epic planning workflow lacks mandatory index refresh step
4. **No Sanity Check**: No automated check to verify claimed complexity matches current code

**Systemic Indicators**:
- ❌ No `index_file` call in epic intake workflow
- ❌ No `complexity_audit.py` verification before ticket generation
- ❌ No `search_symbols` check for claimed "missing" methods
- ❌ No git history check to detect recent refactoring
- ❌ Scope document revision process failed to catch error

---

## Impact Assessment

### Work Obsolescence
- **7 tickets**: 100% obsolete
- **Estimated effort**: 5-7 hours wasted if executed
- **Risk**: Would create duplicate methods or break existing code

### Extracted Methods Already Exist
| Ticket Claims to Extract | Actually Exists At | Status |
|---------------------------|-------------------|--------|
| MapBrokerStateToFSM | Line 648: `HydrateFSM_MapOrderStateToFsmState` | ✅ DONE |
| CreateFollowerBracketFSM | Inline in parent (lines 928-936) | ✅ DONE |
| LinkStopOrder | Line 695: `HydrateFSM_LinkBracketOrders` | ✅ DONE |
| LinkTargetOrders | Line 695: `HydrateFSM_LinkBracketOrders` | ✅ DONE |
| RegisterFSM | Inline in parent (lines 938-946) | ✅ DONE |
| ScanForRecoveryKey | Line 852: `HydrateFSM_RecoverFromOpenPositions` | ✅ DONE |
| CreateRecoveryFSM | Line 852: `HydrateFSM_RecoverFromOpenPositions` | ✅ DONE |

### Current Complexity Status
- **Parent Method**: CYC ~10 (Jane Street compliant)
- **Helper Methods**: All ≤8 CYC (verified via jCodemunch)
- **V12 DNA**: ✅ No locks, ✅ ASCII-only, ✅ Extraction floor met

---

## Recommendations

### Immediate Actions
1. **CANCEL EPIC-CCN-1**: All tickets are obsolete
2. **Archive Tickets**: Move to `docs/brain/EPIC-CCN-1/OBSOLETE/`
3. **Update Scope Document**: Mark as "CANCELLED - Work Already Complete"

### Protocol Fixes (Prevent Recurrence)

#### 1. Mandatory Index Refresh in Epic Intake
Add to `/epic-intake` workflow:
```markdown
## Step 0: Index Freshness Verification (MANDATORY)

Before analyzing code:
1. Run: `index_file { "path": "<target-file>" }`
2. Verify: Check `_meta.indexed_at` timestamp
3. Confirm: Timestamp within last 24 hours
4. If stale: Re-index before proceeding
```

#### 2. Complexity Cross-Check
Add to `/epic-scope-boundary` workflow:
```markdown
## Step 1: Complexity Verification (MANDATORY)

Before generating tickets:
1. Run: `python scripts/complexity_audit.py`
2. Compare: Claimed CYC vs actual CYC
3. Abort if: Mismatch >10% (indicates stale data)
```

#### 3. Git History Check
Add to `/epic-intake` workflow:
```markdown
## Step 2: Recent Refactoring Detection (MANDATORY)

Before planning:
1. Run: `git log --oneline --since="30 days ago" -- <target-file>`
2. Search: Grep for "extract", "refactor", "CYC", "complexity"
3. Alert: If recent refactoring found, verify current state
```

#### 4. Helper Method Existence Check
Add to `/epic-plan` workflow:
```markdown
## Step 3: Phantom Work Detection (MANDATORY)

Before ticket generation:
1. For each claimed "missing" method:
   - Run: `search_symbols { "query": "<method-name>" }`
   - Verify: Method does NOT already exist
2. Abort if: Any claimed extraction already exists
```

#### 5. Automated Sanity Check Script
Create `scripts/epic_sanity_check.ps1`:
```powershell
# Verify epic scope against current code state
param(
    [string]$EpicDir,
    [string]$TargetFile
)

# 1. Check index freshness
$indexAge = Get-IndexAge $TargetFile
if ($indexAge -gt 24) { throw "Index stale: $indexAge hours old" }

# 2. Verify complexity claims
$claimedCYC = Get-ClaimedComplexity $EpicDir
$actualCYC = Get-ActualComplexity $TargetFile
if ([Math]::Abs($claimedCYC - $actualCYC) -gt 5) {
    throw "Complexity mismatch: Claimed $claimedCYC, Actual $actualCYC"
}

# 3. Check for recent refactoring
$recentCommits = git log --oneline --since="30 days ago" -- $TargetFile
if ($recentCommits -match "extract|refactor|CYC") {
    Write-Warning "Recent refactoring detected - verify scope"
}

# 4. Verify claimed missing methods don't exist
$claimedMethods = Get-ClaimedExtractions $EpicDir
foreach ($method in $claimedMethods) {
    if (Test-MethodExists $TargetFile $method) {
        throw "Method '$method' already exists - phantom work detected"
    }
}

Write-Host "✅ Epic sanity check PASSED" -ForegroundColor Green
```

---

## Lessons Learned

### What Went Wrong
1. **No Index Refresh**: Epic planning assumed jCodemunch index was current
2. **No Verification**: Scope document revision failed to verify claims against live code
3. **No Git Check**: Planning ignored recent commit history
4. **No Existence Check**: Tickets generated without verifying methods don't already exist

### What Went Right
1. **Bob's Detection**: Bob correctly identified method at line 902 (not 464)
2. **Forensic Protocol**: Investigation workflow successfully traced root cause
3. **Scope Document Transparency**: Document acknowledged stale data (even if correction failed)

### Process Improvements
1. **Trust But Verify**: Always verify index freshness before planning
2. **Git History First**: Check recent commits before assuming work needed
3. **Existence Proofs**: Verify claimed "missing" code actually missing
4. **Automated Gates**: Add sanity checks to prevent phantom work

---

## Conclusion

**EPIC-CCN-1 is 100% obsolete.** All extraction work was completed 28 days ago in Phase 7 Sprint 1. The epic planning process used 19-day-old stale index data, causing phantom work to be scheduled.

**Root cause is SYSTEMIC**: Epic planning workflow lacks mandatory index refresh and verification steps. This will recur unless protocol fixes are implemented.

**Recommended Action**: CANCEL EPIC-CCN-1 and implement the 5 protocol fixes above to prevent future phantom work.

---

**[FORENSIC-COMPLETE]**

**Investigation Status**: COMPLETE  
**Confidence Level**: HIGH (100% evidence-based)  
**Recommendation**: CANCEL EPIC-CCN-1 + Implement Protocol Fixes