# Extraction Tickets: EPIC-CCN-052

## Overview
- **Total Tickets**: 1 (comprehensive extraction)
- **Execution Order**: Sequential (3 helpers + main method refactor)
- **Estimated Effort**: 2-3 hours (including testing)
- **Target Method**: `CleanupStalePendingReplacements`
- **Target File**: `src/V12_002.Trailing.StopUpdate.cs`
- **Complexity Reduction**: 9 → 4 (55% reduction)

---

## TICKET-1: Extract Three Helpers from CleanupStalePendingReplacements

### Scope
- **Current Method**: `CleanupStalePendingReplacements`
- **Current CYC**: 9
- **Target CYC**: 4 (main method)
- **Extraction**: Split into 3 private helper methods + refactored main orchestrator

### Complexity Breakdown

**Before Extraction**:
- Single method: CYC = 9
- Nested conditionals (3 levels deep)
- Multiple responsibilities mixed

**After Extraction**:
- Main method: CYC = 4 (orchestrator)
- Helper 1 (ShouldRemoveStalePendingReplacement): CYC = 1
- Helper 2 (CreateEmergencyStopForUnprotectedPosition): CYC = 4
- Helper 3 (RestoreBracketTargetsIfNeeded): CYC = 2
- **All methods ≤8** (Jane Street compliant)

### Implementation Steps

#### Step 1: Create Helper Method 1 - Staleness Check

**Location**: Insert after line 36 (before CleanupStalePendingReplacements)

**Method Signature**:
```csharp
private bool ShouldRemoveStalePendingReplacement(DateTime now, PendingStopReplacement pending)
```

**Implementation**:
```csharp
private bool ShouldRemoveStalePendingReplacement(DateTime now, PendingStopReplacement pending)
{
    return (now - pending.CreatedTime).TotalSeconds > 5;
}
```

**Complexity**: CYC = 1 (pure function)
**Responsibility**: Determines if a pending replacement is stale (>5 seconds old)

**Verification**:
- Compile: `dotnet build`
- Complexity: `python scripts/complexity_audit.py` (expect CYC=1)

---

#### Step 2: Create Helper Method 2 - Emergency Stop Creation

**Location**: Insert after ShouldRemoveStalePendingReplacement

**Method Signature**:
```csharp
private void CreateEmergencyStopForUnprotectedPosition(string entryName, PendingStopReplacement pending)
```

**Implementation**:
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

**Complexity**: CYC = 4 (compound condition + method call)
**Responsibility**: Creates emergency stop order if position exists and needs protection

**Lock-Free Patterns**:
- ✅ Uses `TryGetValue` (lock-free read)
- ✅ Delegates to existing `CreateNewStopOrder` method
- ✅ No `lock()` statements

**Verification**:
- Compile: `dotnet build`
- Complexity: `python scripts/complexity_audit.py` (expect CYC=4)
- Lock-free: `grep -n "lock(" src/V12_002.Trailing.StopUpdate.cs` (expect zero matches)

---

#### Step 3: Create Helper Method 3 - Bracket Target Restoration

**Location**: Insert after CreateEmergencyStopForUnprotectedPosition

**Method Signature**:
```csharp
private void RestoreBracketTargetsIfNeeded(string entryName, PendingStopReplacement pending)
```

**Implementation**:
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

**Complexity**: CYC = 2 (compound condition + event trigger)
**Responsibility**: Restores bracket targets if needed after emergency stop creation

**Lock-Free Patterns**:
- ✅ Uses `TriggerCustomEvent` (Actor/FSM pattern)
- ✅ Event-based asynchronous execution
- ✅ No `lock()` statements

**Verification**:
- Compile: `dotnet build`
- Complexity: `python scripts/complexity_audit.py` (expect CYC=2)
- Lock-free: `grep -n "lock(" src/V12_002.Trailing.StopUpdate.cs` (expect zero matches)

---

#### Step 4: Refactor Main Method

**Location**: Replace lines 37-81 in CleanupStalePendingReplacements

**Refactored Implementation**:
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

**New Complexity**: CYC = 4 (foreach + 2 if statements + helper calls)
**Reduction**: 9 → 4 (55% reduction)

**Lock-Free Patterns Preserved**:
- ✅ `ToArray()` snapshot (ConcurrentDictionary)
- ✅ `TryRemove` (lock-free removal)
- ✅ `Interlocked.Decrement` (atomic operation)
- ✅ Helper methods maintain lock-free patterns

**Verification**:
- Compile: `dotnet build`
- Complexity: `python scripts/complexity_audit.py` (expect CYC=4)
- Lock-free: `grep -r "lock(" src/V12_002.Trailing.StopUpdate.cs` (expect zero matches)

---

### Acceptance Criteria

#### Build & Compilation
- [ ] `dotnet build` completes without errors
- [ ] No new compiler warnings introduced
- [ ] CSharpier formatting passes: `dotnet csharpier check src/`

#### Complexity Metrics
- [ ] Main method CYC ≤ 4 (verified via `python scripts/complexity_audit.py`)
- [ ] Helper 1 CYC = 1
- [ ] Helper 2 CYC = 4
- [ ] Helper 3 CYC = 2
- [ ] All methods ≤8 (Jane Street compliant)

#### Lock-Free Compliance
- [ ] Zero `lock()` statements in file (verified via `grep -r "lock(" src/V12_002.Trailing.StopUpdate.cs`)
- [ ] All atomic operations preserved (`Interlocked.Decrement`)
- [ ] All concurrent collections usage preserved (`TryRemove`, `TryGetValue`, `ToArray`)
- [ ] Actor/FSM pattern preserved (`TriggerCustomEvent`)

#### DNA Compliance
- [ ] ASCII-only strings (no Unicode, emoji, or curly quotes)
- [ ] No breaking changes to API surface
- [ ] All helpers are `private` (no public API changes)
- [ ] Type safety maintained (no nullable reference warnings)

#### PR Hygiene
- [ ] Diff size <10,000 characters (estimated: ~1,200 chars)
- [ ] No whitespace mutations outside extraction scope
- [ ] No unrelated changes to other methods
- [ ] All changes trace to complexity reduction goal

#### Hard-Link Sync
- [ ] `powershell -File .\deploy-sync.ps1` completes successfully
- [ ] NinjaTrader hard links synchronized

#### Behavioral Preservation
- [ ] NinjaTrader F5 test shows identical behavior
- [ ] No changes to method execution flow
- [ ] All existing log messages preserved
- [ ] Emergency stop creation logic unchanged
- [ ] Bracket restoration logic unchanged

#### Testing (Post-Implementation)
- [ ] Unit test created: `Test_ShouldRemoveStalePendingReplacement_StaleEntry`
- [ ] Unit test created: `Test_ShouldRemoveStalePendingReplacement_FreshEntry`
- [ ] Unit test created: `Test_CreateEmergencyStopForUnprotectedPosition_PositionExists`
- [ ] Unit test created: `Test_CreateEmergencyStopForUnprotectedPosition_PositionMissing`
- [ ] Unit test created: `Test_RestoreBracketTargetsIfNeeded_RestorationNeeded`
- [ ] Unit test created: `Test_RestoreBracketTargetsIfNeeded_RestorationNotNeeded`
- [ ] All unit tests pass

---

### Dependencies

**None** - This is a standalone extraction ticket.

**Prerequisites**:
- Phase 2 (Architecture Plan) completed ✅
- Phase 3 (DNA & PR Audit) passed ✅

---

### Verification Commands

**Full Verification Suite**:
```bash
# 1. Build & Compilation
powershell -File .\scripts\build_readiness.ps1

# 2. Complexity Audit
python scripts/complexity_audit.py

# 3. Lock-Free Scan
grep -r "lock(" src/V12_002.Trailing.StopUpdate.cs

# 4. CSharpier Formatting
dotnet csharpier check src/

# 5. Hard-Link Sync
powershell -File .\deploy-sync.ps1

# 6. Pre-Push Validation (Fast Mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

**Expected Results**:
- Build: ✅ Zero errors
- Complexity: CleanupStalePendingReplacements CYC=4, Helpers CYC=1/4/2
- Lock-free: Zero matches
- Formatting: Zero issues
- Sync: Success
- Pre-push: All checks pass

---

### Rollback Plan

**If Issues Arise**:

1. **Compilation Errors**:
   - Revert: `git checkout src/V12_002.Trailing.StopUpdate.cs`
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

---

### Testing Strategy

**Unit Tests** (6 test cases across 3 helpers):

#### Helper 1: ShouldRemoveStalePendingReplacement

**Test 1: Stale Entry (>5 seconds)**
```csharp
[Test]
public void Test_ShouldRemoveStalePendingReplacement_StaleEntry()
{
    // Arrange
    DateTime now = DateTime.Now;
    var pending = new PendingStopReplacement { CreatedTime = now.AddSeconds(-10) };
    
    // Act
    bool result = ShouldRemoveStalePendingReplacement(now, pending);
    
    // Assert
    Assert.IsTrue(result, "Stale entry (>5s) should return true");
}
```

**Test 2: Fresh Entry (<5 seconds)**
```csharp
[Test]
public void Test_ShouldRemoveStalePendingReplacement_FreshEntry()
{
    // Arrange
    DateTime now = DateTime.Now;
    var pending = new PendingStopReplacement { CreatedTime = now.AddSeconds(-3) };
    
    // Act
    bool result = ShouldRemoveStalePendingReplacement(now, pending);
    
    // Assert
    Assert.IsFalse(result, "Fresh entry (<5s) should return false");
}
```

#### Helper 2: CreateEmergencyStopForUnprotectedPosition

**Test 3: Position Exists with RemainingContracts**
```csharp
[Test]
public void Test_CreateEmergencyStopForUnprotectedPosition_PositionExists()
{
    // Arrange
    var mockPositions = new ConcurrentDictionary<string, PositionState>();
    mockPositions["TEST"] = new PositionState 
    { 
        EntryFilled = true, 
        RemainingContracts = 2 
    };
    
    var pending = new PendingStopReplacement 
    { 
        StopPrice = 100.0, 
        Direction = OrderDirection.Sell, 
        Quantity = 2 
    };
    
    // Act
    CreateEmergencyStopForUnprotectedPosition("TEST", pending);
    
    // Assert
    // Verify CreateNewStopOrder was called with correct parameters
    // (Requires mocking framework or spy pattern)
}
```

**Test 4: Position Missing**
```csharp
[Test]
public void Test_CreateEmergencyStopForUnprotectedPosition_PositionMissing()
{
    // Arrange
    var mockPositions = new ConcurrentDictionary<string, PositionState>();
    var pending = new PendingStopReplacement 
    { 
        StopPrice = 100.0, 
        Direction = OrderDirection.Sell 
    };
    
    // Act
    CreateEmergencyStopForUnprotectedPosition("MISSING", pending);
    
    // Assert
    // Verify CreateNewStopOrder was NOT called
    // (Requires mocking framework or spy pattern)
}
```

#### Helper 3: RestoreBracketTargetsIfNeeded

**Test 5: Restoration Needed**
```csharp
[Test]
public void Test_RestoreBracketTargetsIfNeeded_RestorationNeeded()
{
    // Arrange
    var pending = new PendingStopReplacement 
    { 
        BracketRestorationNeeded = true, 
        CapturedTargets = new TargetSnapshot[] { /* mock data */ } 
    };
    
    // Act
    RestoreBracketTargetsIfNeeded("TEST", pending);
    
    // Assert
    // Verify TriggerCustomEvent was called
    // (Requires event spy or mock)
}
```

**Test 6: Restoration Not Needed**
```csharp
[Test]
public void Test_RestoreBracketTargetsIfNeeded_RestorationNotNeeded()
{
    // Arrange
    var pending = new PendingStopReplacement 
    { 
        BracketRestorationNeeded = false 
    };
    
    // Act
    RestoreBracketTargetsIfNeeded("TEST", pending);
    
    // Assert
    // Verify TriggerCustomEvent was NOT called
    // (Requires event spy or mock)
}
```

---

### Jane Street Compliance Summary

**Cognitive Simplicity**: ✅
- Main method: Simple orchestrator (CYC=4)
- Each helper: Single, clear responsibility
- No nested conditionals in main method
- Easy to reason about under microsecond-latency constraints

**Correctness by Construction**: ✅
- All parameters strongly typed
- No nullable reference warnings
- Compiler enforces correct usage
- No changes to API surface

**Testing Straightforward**: ✅
- Pure function (Helper 1) easy to unit test
- Mockable dependencies (Helper 2, Helper 3)
- Reduced path complexity (2^4 vs 2^9 paths)

**No Clever Abstractions**: ✅
- Straightforward method extraction
- No new design patterns introduced
- Each helper has obvious, single purpose

---

### Estimated Effort

**Implementation**: 1.5 hours
- Helper 1: 15 minutes
- Helper 2: 30 minutes
- Helper 3: 20 minutes
- Main method refactor: 25 minutes

**Testing**: 1 hour
- Unit test creation: 45 minutes
- Test execution & debugging: 15 minutes

**Verification**: 30 minutes
- Build & complexity checks: 10 minutes
- Lock-free scan: 5 minutes
- Hard-link sync: 5 minutes
- NinjaTrader F5 test: 10 minutes

**Total**: 2-3 hours (including buffer for unexpected issues)

---

### Success Metrics

**Complexity Reduction**: 55% (9 → 4)
**Lock-Free Compliance**: 100% (zero `lock()` statements)
**Jane Street Alignment**: 100% (all methods ≤8 complexity)
**Build Success**: 100% (zero errors)
**Behavioral Preservation**: 100% (identical NinjaTrader behavior)
**Test Coverage**: 6 unit tests (3 helpers × 2 test cases each)

---

**Ticket Status**: 🟡 READY FOR IMPLEMENTATION

**Assigned To**: Bob CLI (`v12-engineer`) or Codex CLI (`codex-rescue`)

**Priority**: P5 (Surgical Extraction)

**Epic**: EPIC-CCN-052

**Phase**: Phase 4 → Phase 5 (Ticket Execution)
