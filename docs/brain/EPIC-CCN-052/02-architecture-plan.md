# Phase 2: Architecture Planning - EPIC-CCN-052

## Target Method Analysis

**Method**: `CleanupStalePendingReplacements`
**File**: `src/V12_002.Trailing.StopUpdate.cs`
**Current Complexity**: 9 (cyclomatic complexity)
**Current LOC**: 26 lines
**Target Complexity**: ≤8 (Jane Street strict standard)
**Tier**: 2 (Medium complexity)

## Extraction Strategy

### Current Method Structure

The method performs three distinct operations in a nested structure:
1. **Stale Entry Detection & Removal** (lines 42-49): Iterates through pending replacements, checks staleness (>5 seconds), removes stale entries
2. **Emergency Stop Creation** (lines 52-68): Creates emergency stops for unprotected positions when stale replacements are removed
3. **Bracket Target Restoration** (lines 70-75): Restores bracket targets if needed after emergency stop creation

### Complexity Breakdown

Current cyclomatic complexity contributors:
- `foreach` loop: +1
- `if ((now - kvp.Value.CreatedTime).TotalSeconds > 5)`: +1
- `if (pendingStopReplacements.TryRemove(...))`: +1
- `if (activePositions.TryGetValue(...) && pos.EntryFilled && pos.RemainingContracts > 0)`: +3 (compound condition)
- `if (pending.BracketRestorationNeeded && pending.CapturedTargets != null)`: +2 (compound condition)
- **Total**: 9

### Proposed Extraction

Extract three private helper methods to reduce main method complexity to ~4:

1. **ShouldRemoveStalePendingReplacement**: Encapsulates staleness check logic
2. **CreateEmergencyStopForUnprotectedPosition**: Encapsulates emergency stop creation logic
3. **RestoreBracketTargetsIfNeeded**: Encapsulates bracket target restoration logic

## Method Signatures

### Original Method

```csharp
private void CleanupStalePendingReplacements()
```

**Parameters**: None
**Return Type**: `void`
**Access Modifier**: `private`
**Complexity**: 9

### Proposed Helper Method 1: Staleness Check

```csharp
private bool ShouldRemoveStalePendingReplacement(DateTime now, PendingStopReplacement pending)
```

**Parameters**:
- `now` (DateTime): Current timestamp for comparison
- `pending` (PendingStopReplacement): The pending replacement to check

**Return Type**: `bool` (true if stale and should be removed)
**Access Modifier**: `private`
**Complexity**: 1 (single conditional)
**Responsibility**: Determines if a pending replacement is older than 5 seconds

### Proposed Helper Method 2: Emergency Stop Creation

```csharp
private void CreateEmergencyStopForUnprotectedPosition(string entryName, PendingStopReplacement pending)
```

**Parameters**:
- `entryName` (string): The position entry name/key
- `pending` (PendingStopReplacement): The removed pending replacement containing stop details

**Return Type**: `void`
**Access Modifier**: `private`
**Complexity**: 4 (compound condition + method call)
**Responsibility**: Creates emergency stop order if position exists and needs protection

### Proposed Helper Method 3: Bracket Target Restoration

```csharp
private void RestoreBracketTargetsIfNeeded(string entryName, PendingStopReplacement pending)
```

**Parameters**:
- `entryName` (string): The position entry name/key
- `pending` (PendingStopReplacement): The pending replacement containing bracket target data

**Return Type**: `void`
**Access Modifier**: `private`
**Complexity**: 2 (compound condition + event trigger)
**Responsibility**: Restores bracket targets if needed after emergency stop creation

### Refactored Main Method

```csharp
private void CleanupStalePendingReplacements()
{
    DateTime now = DateTime.Now;
    
    // V8.30: Safe iteration with snapshot
    foreach (var kvp in pendingStopReplacements.ToArray())
    {
        if (ShouldRemoveStalePendingReplacement(now, kvp.Value))
        {
            if (pendingStopReplacements.TryRemove(kvp.Key, out var pending))
            {
                Interlocked.Decrement(ref pendingReplacementCount);
                Print(string.Format("V8.30: Stale pending replacement REMOVED for {0} (>5sec old)", kvp.Key));
                
                CreateEmergencyStopForUnprotectedPosition(kvp.Key, pending);
                RestoreBracketTargetsIfNeeded(kvp.Key, pending);
            }
        }
    }
}
```

**New Complexity**: 4 (foreach + 2 if statements + helper calls)
**Reduction**: 9 → 4 (55% reduction)

## Call Graph

```
CleanupStalePendingReplacements (main orchestrator)
├── ShouldRemoveStalePendingReplacement (staleness check)
├── CreateEmergencyStopForUnprotectedPosition (emergency stop)
│   └── CreateNewStopOrder (existing method, not modified)
└── RestoreBracketTargetsIfNeeded (bracket restoration)
    └── TriggerCustomEvent → RestoreCascadedTargets (existing pattern, not modified)
```

### Data Flow

1. **Main Method** → **ShouldRemoveStalePendingReplacement**:
   - Input: `DateTime now`, `PendingStopReplacement pending`
   - Output: `bool` (staleness decision)

2. **Main Method** → **CreateEmergencyStopForUnprotectedPosition**:
   - Input: `string entryName`, `PendingStopReplacement pending`
   - Accesses: `activePositions` (ConcurrentDictionary, read-only)
   - Calls: `CreateNewStopOrder` (existing method)

3. **Main Method** → **RestoreBracketTargetsIfNeeded**:
   - Input: `string entryName`, `PendingStopReplacement pending`
   - Calls: `TriggerCustomEvent` (Actor/FSM pattern)

### Shared State

**Read-Only Access**:
- `activePositions` (ConcurrentDictionary): Read via `TryGetValue` in helper method 2
- `pendingStopReplacements` (ConcurrentDictionary): Iterated in main method, removed via `TryRemove`

**Atomic Mutations**:
- `pendingReplacementCount` (int): Decremented via `Interlocked.Decrement` in main method

**No Shared Mutable State Between Helpers**: Each helper method operates independently on its input parameters.

## Lock-Free Validation

### Current Method Analysis

✅ **No `lock()` Statements**: Verified by code inspection (lines 37-81)

✅ **Uses FSM/Actor Enqueue Pattern**: 
- Line 75: `TriggerCustomEvent(o => RestoreCascadedTargets(_tKey, _tSnap), null)`
- Event-based asynchronous execution

✅ **Atomic Primitives Only**:
- Line 48: `Interlocked.Decrement(ref pendingReplacementCount)`
- Atomic decrement operation

✅ **Concurrent Collections**:
- Line 42: `pendingStopReplacements.ToArray()` (ConcurrentDictionary snapshot)
- Line 46: `pendingStopReplacements.TryRemove(kvp.Key, out var pending)` (lock-free removal)
- Line 52: `activePositions.TryGetValue(kvp.Key, out var pos)` (lock-free read)

### Extraction Preservation

The proposed extraction **preserves all lock-free patterns**:

1. **ShouldRemoveStalePendingReplacement**: Pure function, no state access
2. **CreateEmergencyStopForUnprotectedPosition**: Uses `TryGetValue` (lock-free) and delegates to `CreateNewStopOrder`
3. **RestoreBracketTargetsIfNeeded**: Uses `TriggerCustomEvent` (Actor pattern)

**Lock-Free Compliance**: ✅ VERIFIED

## Jane Street Compliance

### Cognitive Simplicity ✅

**Before Extraction**:
- Single method with 9 cyclomatic complexity
- Nested conditionals (3 levels deep)
- Multiple responsibilities mixed together
- Difficult to reason about under microsecond-latency constraints

**After Extraction**:
- Main method: 4 cyclomatic complexity (simple orchestrator)
- Helper 1: 1 complexity (pure staleness check)
- Helper 2: 4 complexity (emergency stop logic)
- Helper 3: 2 complexity (bracket restoration)
- Each method has single, clear responsibility
- Easier to reason about and verify correctness

**Jane Street Principle**: "Keep functions simple" - Complexity ≤8 per function
**Compliance**: ✅ All methods ≤8 complexity

### Correctness by Construction ✅

**Extraction Strategy**:
- No new state management complexity introduced
- Preserves existing lock-free patterns
- No changes to method signatures or API surface
- No changes to caller/callee contracts

**Type Safety**:
- All parameters strongly typed
- No nullable reference warnings
- Compiler enforces correct usage

**Jane Street Principle**: "Make illegal states unrepresentable"
**Compliance**: ✅ Extraction maintains existing type safety

### Testing Strategy ✅

**Before Extraction**:
- Single 26-line method with 9 complexity
- Difficult to test all code paths (2^9 = 512 potential paths)
- Mocking required for multiple dependencies

**After Extraction**:
- **Helper 1** (ShouldRemoveStalePendingReplacement): Pure function, easy to unit test
  - Test cases: stale (>5s), fresh (<5s), edge cases (exactly 5s)
- **Helper 2** (CreateEmergencyStopForUnprotectedPosition): Testable with mocked `activePositions`
  - Test cases: position exists, position missing, position filled, position empty
- **Helper 3** (RestoreBracketTargetsIfNeeded): Testable with mocked event system
  - Test cases: restoration needed, restoration not needed, null targets

**Jane Street Principle**: "Testing should be straightforward" (from "Why Testing Is Hard")
**Compliance**: ✅ Extraction enables independent unit testing

### No Clever Abstractions ✅

**Extraction Approach**:
- Straightforward method extraction
- No new design patterns introduced
- No over-engineering or premature optimization
- Each helper has obvious, single purpose

**Jane Street Principle**: "Avoid clever code that's hard to understand"
**Compliance**: ✅ Simple, direct extraction

## Implementation Plan

### Step 1: Create Helper Method 1 (ShouldRemoveStalePendingReplacement)

**Location**: Insert after line 36 (before CleanupStalePendingReplacements)

**Code**:
```csharp
private bool ShouldRemoveStalePendingReplacement(DateTime now, PendingStopReplacement pending)
{
    return (now - pending.CreatedTime).TotalSeconds > 5;
}
```

**Verification**:
- Compile: `dotnet build`
- Complexity check: `python scripts/complexity_audit.py` (expect CYC=1)

### Step 2: Create Helper Method 2 (CreateEmergencyStopForUnprotectedPosition)

**Location**: Insert after ShouldRemoveStalePendingReplacement

**Code**:
```csharp
private void CreateEmergencyStopForUnprotectedPosition(string entryName, PendingStopReplacement pending)
{
    // If position still exists and needs protection, create emergency stop
    if (
        activePositions.TryGetValue(entryName, out var pos)
        && pos.EntryFilled
        && pos.RemainingContracts > 0
    )
    {
        Print(string.Format("[1104.2] Recovery: force-initiating stop for {0}", entryName));
        // V12.1101E [F-02]: Use live RemainingContracts under stateLock instead of stale pending.Quantity
        int replacementQty = pos.RemainingContracts;
        CreateNewStopOrder(
            entryName,
            replacementQty,
            pending.StopPrice,
            pending.Direction,
            isRecovery: true
        );
    }
}
```

**Verification**:
- Compile: `dotnet build`
- Complexity check: `python scripts/complexity_audit.py` (expect CYC=4)

### Step 3: Create Helper Method 3 (RestoreBracketTargetsIfNeeded)

**Location**: Insert after CreateEmergencyStopForUnprotectedPosition

**Code**:
```csharp
private void RestoreBracketTargetsIfNeeded(string entryName, PendingStopReplacement pending)
{
    // Build 950: Also restore bracket targets after V8.30 emergency stop.
    if (pending.BracketRestorationNeeded && pending.CapturedTargets != null)
    {
        TargetSnapshot[] _tSnap = pending.CapturedTargets;
        string _tKey = entryName;
        TriggerCustomEvent(o => RestoreCascadedTargets(_tKey, _tSnap), null);
    }
}
```

**Verification**:
- Compile: `dotnet build`
- Complexity check: `python scripts/complexity_audit.py` (expect CYC=2)

### Step 4: Refactor Main Method

**Location**: Replace lines 37-81 in CleanupStalePendingReplacements

**Code**: (See "Refactored Main Method" section above)

**Verification**:
- Compile: `dotnet build`
- Complexity check: `python scripts/complexity_audit.py` (expect CYC=4)
- Lock-free scan: `grep -n "lock(" src/V12_002.Trailing.StopUpdate.cs` (expect zero matches)

### Step 5: Verification & Testing

**Build Verification**:
```bash
powershell -File .\scriptsuild_readiness.ps1
```

**Complexity Audit**:
```bash
python scripts/complexity_audit.py
```

**Expected Results**:
- CleanupStalePendingReplacements: CYC=4 (down from 9)
- ShouldRemoveStalePendingReplacement: CYC=1
- CreateEmergencyStopForUnprotectedPosition: CYC=4
- RestoreBracketTargetsIfNeeded: CYC=2

**Lock-Free Verification**:
```bash
grep -r "lock(" src/V12_002.Trailing.StopUpdate.cs
```
Expected: Zero matches

**Hard-Link Sync**:
```bash
powershell -File .\deploy-sync.ps1
```

### Step 6: Test Strategy

**Unit Tests** (to be created in Phase 3):

1. **Test_ShouldRemoveStalePendingReplacement_StaleEntry**
   - Input: now=10s, pending.CreatedTime=0s
   - Expected: true

2. **Test_ShouldRemoveStalePendingReplacement_FreshEntry**
   - Input: now=3s, pending.CreatedTime=0s
   - Expected: false

3. **Test_CreateEmergencyStopForUnprotectedPosition_PositionExists**
   - Mock: activePositions contains entry with RemainingContracts=2
   - Expected: CreateNewStopOrder called with correct parameters

4. **Test_CreateEmergencyStopForUnprotectedPosition_PositionMissing**
   - Mock: activePositions does not contain entry
   - Expected: CreateNewStopOrder NOT called

5. **Test_RestoreBracketTargetsIfNeeded_RestorationNeeded**
   - Input: pending.BracketRestorationNeeded=true, CapturedTargets!=null
   - Expected: TriggerCustomEvent called

6. **Test_RestoreBracketTargetsIfNeeded_RestorationNotNeeded**
   - Input: pending.BracketRestorationNeeded=false
   - Expected: TriggerCustomEvent NOT called

## Rollback Plan

**If Issues Arise**:

1. **Compilation Errors**:
   - Revert to checkpoint: `git checkout src/V12_002.Trailing.StopUpdate.cs`
   - Review error messages
   - Fix and retry

2. **Complexity Regression**:
   - If any method exceeds CYC=8, further extract logic
   - Use `python scripts/complexity_audit.py` to identify hotspots

3. **Behavioral Changes**:
   - Run NinjaTrader F5 test
   - Compare logs before/after extraction
   - If behavior differs, revert and re-analyze

4. **Lock-Free Violations**:
   - If `grep -r "lock(" src/` finds matches, immediately revert
   - Review extraction logic for accidental lock introduction

## Success Criteria

✅ **Complexity Reduction**: CleanupStalePendingReplacements CYC ≤8 (target: 4)
✅ **Lock-Free Compliance**: Zero `lock()` statements in file
✅ **Build Success**: `dotnet build` completes without errors
✅ **Hard-Link Sync**: `deploy-sync.ps1` completes successfully
✅ **Behavioral Preservation**: NinjaTrader F5 test shows identical behavior
✅ **Jane Street Alignment**: All methods ≤8 complexity, cognitive simplicity maintained

## Phase 3 Preview

**Next Phase**: Implementation (P5 Surgical)

**Agent**: Bob CLI (`v12-engineer`) or Codex CLI (`codex-rescue`)

**Tasks**:
1. Execute Step 1-4 (method extraction)
2. Run verification suite (Step 5)
3. Create unit tests (Step 6)
4. Submit PR with complexity metrics

**Estimated Effort**: 2-3 hours (including testing)

---

**Phase 2 Status**: ✅ COMPLETE

**Approval Gate**: Ready for Phase 3 (Implementation)
