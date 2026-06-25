# Phase 0: Hotspot Analysis - EPIC-W7-023

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 1.53
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:38:35Z to 2026-06-23T02:39:13Z

## Target Method
- **Method**: HandleFlatPositionUpdate
- **File**: src/V12_002.Orders.Callbacks.Execution.cs
- **Line**: 69
- **Cyclomatic Complexity**: 19
- **Max Nesting Depth**: 4
- **Parameter Count**: 1

## Complexity Metrics

### Hotspot Score Analysis
- **Hotspot Score**: 61.16 (HIGH)
- **Churn (90 days)**: 24 commits
- **Assessment**: HIGH RISK
- **Ranking**: #19 out of top 50 hotspots

### Complexity Breakdown
- **Cyclomatic Complexity**: 19 (Target: ≤8 per Jane Street standard)
- **Reduction Needed**: 11 points (19 → 8)
- **Max Nesting Depth**: 4 levels
- **Parameter Count**: 1 (acceptable)

### Method Purpose
Build 935 [CB-B935-001]: Flat-position cleanup extracted from OnPositionUpdate. Handles cleanup when positions go flat, including:
- Syncing expectedPositions on flat
- Detecting external closes
- Cancelling orphaned orders
- Cleaning up position tracking

## Blast Radius

### Direct Impact
- **Importer Count**: 0 files
- **Direct Dependents**: 0 symbols
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Consumers**: 0
- **Potential Consumers**: 0

### Analysis
The method has **zero external blast radius** - it is only called internally by `ProcessOnPositionUpdate` in the same file. This makes it an **ideal refactoring candidate** with minimal risk of breaking external dependencies.

## Call Hierarchy

### Callers (Depth 1)
1. **ProcessOnPositionUpdate** (src/V12_002.Orders.Callbacks.Execution.cs:50)
   - Single caller in same file
   - AST-resolved relationship

### Callees (Depth 1-2)
The method calls **49 downstream symbols**, including:

**Core Logic (Depth 1)**:
- `ExpKey()` - Key generation
- `IsDispatchSyncPending()` - Sync state check
- `HasPendingEntryOrderForAccount()` - Entry order validation
- `HasUnfilledPositionForAccount()` - Position validation
- `SetExpectedPositionLocked()` - State mutation
- `ReconcileOrphanedOrders()` - Order cleanup
- `CancelOrphanedOrdersForPosition()` - Order cancellation
- `CleanupPosition()` - Position cleanup

**Supporting Infrastructure (Depth 2)**:
- `IsOrderTerminal()` - Order state check
- `StampAccountFillGrace()` - Grace period tracking
- `ValidateOrphanedMasterOrders()` - Master order validation
- `BuildLiveBrokerOrderIndex()` - Broker state indexing
- `PurgeGhostOrderReferences()` - Ghost order cleanup
- `CancelOrderSafe()` - Safe cancellation wrapper
- `CancelAllOrdersForEntry()` - Bulk cancellation
- `EvaluateFollowerRepairBlock()` - Follower repair logic
- `PurgePositionIfEligible()` - Position purge logic
- `TryTerminateFollowerBracket()` - Bracket termination

### Complexity Drivers
The high cyclomatic complexity (19) is driven by:
1. **Nested conditionals** for position state validation
2. **Loop with conditionals** iterating activePositions
3. **Multiple cleanup paths** based on position state
4. **Guard clauses** for edge cases

## Risk Assessment

### Overall Risk: **MEDIUM-LOW**

**Risk Factors**:
- ✅ **Zero external blast radius** (only 1 internal caller)
- ✅ **Well-isolated** in callback file
- ⚠️ **High complexity** (CYC 19, needs -11 reduction)
- ⚠️ **High churn** (24 commits in 90 days)
- ⚠️ **49 downstream callees** (complex internal logic)

**Mitigation Factors**:
- Single caller makes testing straightforward
- No cross-file dependencies to break
- Clear functional boundaries (flat position handling)
- Recent refactoring history (Build 935 extraction)

### Refactoring Strategy
**Recommended Approach**: Extract nested logic into helper methods

**Extraction Candidates**:
1. **Position validation logic** (HasPendingEntryOrderForAccount, HasUnfilledPositionForAccount checks)
2. **Cleanup list building** (loop that populates positionsToCleanup)
3. **Orphaned order detection** (CancelOrphanedOrdersForPosition logic)
4. **Cleanup execution** (loop that calls CleanupPosition)

**Target Outcome**:
- Main method: CYC ≤8 (orchestration only)
- Helper methods: CYC ≤5 each (single responsibility)
- Total reduction: 11+ points

## Sequential Thinking Analysis

### Problem Decomposition
The method performs **4 distinct responsibilities**:
1. **Guard validation** - Check if cleanup is needed
2. **State synchronization** - Sync expectedPositions
3. **Orphan detection** - Find positions needing cleanup
4. **Cleanup execution** - Execute cleanup and log results

### Extraction Plan
Each responsibility should become a separate method:
- `ShouldSkipFlatPositionCleanup(acctName)` → CYC 3
- `SyncExpectedPositionsOnFlat(acctName)` → CYC 2
- `DetectOrphanedPositions(acctName)` → CYC 6
- `ExecutePositionCleanup(positionsToCleanup)` → CYC 3
- Main orchestrator → CYC 5

**Total**: 5 methods, max CYC 6, all ≤8 ✅

## Recommendations

### Phase 1 (Scope Definition)
- Focus on **single-responsibility extraction**
- Target **4 helper methods** from main logic
- Maintain **exact behavioral equivalence**
- Preserve **all logging and error handling**

### Phase 2 (Architecture Planning)
- Use **Extract Method** refactoring pattern
- Apply **Guard Clause** pattern for early returns
- Consider **Strategy Pattern** for cleanup logic variants
- Ensure **lock-free Actor pattern** compliance

### Phase 3 (DNA Audit)
- Verify **no lock() usage** (already compliant)
- Confirm **ASCII-only strings** (already compliant)
- Validate **CYC ≤8 per method** (target of refactoring)
- Check **Correctness by Construction** principles

### Success Criteria
- ✅ Main method CYC ≤8
- ✅ All extracted methods CYC ≤8
- ✅ Zero behavioral changes
- ✅ All tests pass
- ✅ Build succeeds
- ✅ deploy-sync.ps1 completes

## Next Steps
1. Proceed to **Phase 1: Scope Definition**
2. Generate detailed extraction plan
3. Create atomic tickets for each helper method
4. Execute extractions in dependency order
5. Verify with unit tests and integration tests
