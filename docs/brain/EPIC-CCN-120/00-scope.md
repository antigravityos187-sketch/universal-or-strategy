# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-120

## Epic Metadata
- **Epic ID**: EPIC-CCN-120
- **Phase**: 1 (Scope + Boundary)
- **Target Method**: AuditMaster_HandleNakedPosition
- **File**: src/V12_002.REAPER.Audit.cs
- **Current Complexity**: 15
- **Target Complexity**: <= 8 (Jane Street HFT alignment)
- **Date**: 2026-06-13

## Target Method Analysis

### Method Signature
```csharp
private void AuditMaster_HandleNakedPosition(
    Position masterPos,
    int masterActualQty,
    string masterExpectedKey)
```

### Current Responsibilities
1. **Position Validation**: Check if master account has non-zero position
2. **Order Snapshot**: Snapshot broker orders to prevent collection modification exceptions (H13-FIX)
3. **Working Stop Detection**: Scan orders for active stop protection
4. **Grace Period Management**: Track naked position first-seen timestamp
5. **Emergency Stop Enqueue**: Queue emergency stop after grace period expires
6. **Error Handling**: Handle TriggerCustomEvent failures and clear in-flight flags

### Complexity Breakdown
- **Cyclomatic Complexity**: 15
- **Nesting Depth**: 4 levels (if > if > if > else)
- **Lines of Code**: ~60 lines
- **Decision Points**: 6 branches

## Extraction Strategy

### What to Extract

#### 1. Order Snapshot + Working Stop Check (CYC: 3)
**New Method**: `AuditMaster_CheckWorkingStop()`
```csharp
private bool AuditMaster_CheckWorkingStop()
```
- Snapshot Account.Orders.ToArray()
- Iterate and check for working stop orders
- Return boolean result
- **Rationale**: Isolates broker interaction and collection safety logic

#### 2. Grace Period Tracking (CYC: 3)
**New Method**: `AuditMaster_TrackNakedGrace(string accountName, int actualQty, out bool graceExpired, out DateTime firstSeen)`
```csharp
private void AuditMaster_TrackNakedGrace(
    string accountName,
    int actualQty,
    out bool graceExpired,
    out DateTime firstSeen)
```
- Check _nakedPositionFirstSeen dictionary
- Initialize grace window if first detection
- Calculate elapsed time and grace expiration
- **Rationale**: Separates temporal logic from order management

#### 3. Emergency Stop Enqueue (CYC: 2)
**Existing Method**: `EnqueueReaperMasterNakedStop()` (already extracted)
- Keep as-is, already handles enqueue logic
- **Rationale**: Already follows single-responsibility principle

### What to Keep
- **Orchestration Logic**: Main if/else flow
- **Position Check**: `if (masterActualQty != 0)`
- **Method Calls**: Delegate to extracted helpers
- **Error Handling**: TriggerCustomEvent try/catch
- **Grace Cleanup**: `_nakedPositionFirstSeen.TryRemove()`

### Post-Extraction Structure
```csharp
private void AuditMaster_HandleNakedPosition(
    Position masterPos,
    int masterActualQty,
    string masterExpectedKey)
{
    if (masterActualQty != 0)
    {
        bool hasWorkingStop = AuditMaster_CheckWorkingStop();
        if (!hasWorkingStop)
        {
            bool graceExpired;
            DateTime firstSeen;
            AuditMaster_TrackNakedGrace(Account.Name, masterActualQty, out graceExpired, out firstSeen);
            
            if (graceExpired && EnqueueReaperMasterNakedStop(masterPos, masterActualQty, masterExpectedKey, firstSeen))
            {
                try
                {
                    TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
                }
                catch (Exception tcEx)
                {
                    _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
                    Print($"[REAPER][NAKED_STOP] TriggerCustomEvent failed: {tcEx.Message}");
                }
            }
        }
        else
        {
            _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
        }
    }
}
```
**Expected Complexity**: 5 (well under target of 8)

## Boundary Definition

### Scope Boundaries
- **Single Method**: `AuditMaster_HandleNakedPosition` ONLY
- **No Lateral Expansion**: Do NOT touch adjacent methods
- **No Caller Modification**: Do NOT modify `AuditMasterAccountIfNeeded`
- **No Callee Modification**: Do NOT modify `EnqueueReaperMasterNakedStop`

### Dependencies Within Boundary
**Allowed (Internal to method)**:
- Extract order snapshot logic
- Extract grace period tracking
- Refactor control flow
- Add helper method calls

**Forbidden (Outside boundary)**:
- Modify `_nakedPositionFirstSeen` dictionary structure
- Change `EnqueueReaperMasterNakedStop` signature
- Alter `ProcessReaperNakedStopQueue` behavior
- Touch `AuditFleet_HandleNakedPosition` (separate epic)

### Boundary Validation

#### Single-Method Scope Confirmation
✅ **Boundary Validated: YES**

**Verification**:
1. **Target**: Only `AuditMaster_HandleNakedPosition` will be modified
2. **Extractions**: New methods are pure helpers (no external side effects)
3. **Callers**: `AuditMasterAccountIfNeeded` unchanged (line 453 call site preserved)
4. **Callees**: `EnqueueReaperMasterNakedStop` unchanged (existing helper)
5. **State**: No shared state structure changes (_nakedPositionFirstSeen remains ConcurrentDictionary)

#### Dependency Violations Check
**None Detected**:
- ✅ No cross-file dependencies
- ✅ No FSM state machine changes
- ✅ No IPC protocol changes
- ✅ No broker API changes
- ✅ No shared collection structure changes

## Success Criteria

### Functional Requirements
1. **Complexity Target**: Post-extraction CYC <= 8 (Jane Street alignment)
2. **Behavior Preservation**: Identical naked position detection logic
3. **Grace Period**: 5-second minimum grace window maintained
4. **Error Handling**: TriggerCustomEvent failure handling preserved
5. **Thread Safety**: H13-FIX order snapshot pattern preserved

### Non-Functional Requirements
1. **Zero Regressions**: All existing tests pass
2. **ASCII-Only**: No Unicode in new code
3. **Lock-Free**: No lock(stateLock) introduced
4. **Build Success**: Zero compilation errors
5. **Lint Clean**: Zero new Roslyn warnings

### Verification Steps
1. **Complexity Audit**: Run `python scripts/complexity_audit.py` - verify CYC <= 8
2. **Build**: Run `powershell -File .\scripts\build_readiness.ps1` - zero errors
3. **Unit Tests**: Run `dotnet test` - 100% pass rate
4. **Behavioral Test**: F5 in NinjaTrader - naked position detection works
5. **Grace Period Test**: Verify 5-second window before emergency stop

## Risk Assessment

### Risk Level: LOW

### Rationale
1. **Isolated Scope**: Single method, no ripple effects
2. **Pure Extractions**: New methods are stateless helpers
3. **Existing Pattern**: Mirrors `AuditFleet_CheckWorkingStop` (Build 935)
4. **Thread Safety**: H13-FIX snapshot pattern already proven
5. **Rollback Simple**: Single-file change, easy to revert

### Mitigation Strategies
1. **Checkpointing**: Bob CLI auto-checkpoint before each extraction
2. **Incremental Testing**: Test after each helper extraction
3. **Behavioral Verification**: Manual NinjaTrader test before commit
4. **Complexity Monitoring**: Run audit after each extraction step
5. **Emergency Rollback**: `/restore 0` if any test fails

## Jane Street Alignment

### Cognitive Simplicity Principles
1. **Single Responsibility**: Each helper does ONE thing
2. **Shallow Nesting**: Max 2 levels after extraction
3. **Obvious Flow**: Linear orchestration in main method
4. **Testable Units**: Each helper independently verifiable
5. **No Cleverness**: Straightforward imperative code

### HFT Latency Considerations
- **Zero Allocation**: No new objects in hot path
- **Inline Candidates**: Small helpers eligible for JIT inlining
- **Cache Friendly**: Sequential logic, no pointer chasing
- **Branch Predictable**: Consistent control flow patterns

## Next Steps (Phase 2)

1. **Read Full Method**: Confirm line-by-line logic
2. **Identify Extraction Points**: Mark exact line ranges
3. **Create Mini-Spec**: Detailed refactoring plan
4. **Validate Against DNA**: Verify lock-free, ASCII-only, atomic
5. **Generate Mermaid Diagram**: Visualize before/after flow

## Metadata
- **Phase**: 1 (Scope + Boundary)
- **Status**: Completed
- **Boundary Validated**: YES
- **Target Complexity**: <= 8
- **Risk Level**: LOW
- **Estimated Effort**: 2 hours (3 extractions + tests)
