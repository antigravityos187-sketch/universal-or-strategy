# Phase 1: Scope Boundary - EPIC-W7-044

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:31:13Z

## Epic Metadata
- **Epic ID**: EPIC-W7-044
- **Target Method**: SymmetryGuardCascadeFollowerCleanup
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Blast Radius**: LOW (0 external callers)

## Scope Definition

### IN SCOPE ✅

#### 1. Extract Dispatch Lookup Logic
**Rationale**: Reduce nesting depth from 6 to ≤4
- Extract symmetryMasterEntryToDispatch lookup
- Extract symmetryDispatchById lookup
- Return early if dispatch not found
- **Target CYC Reduction**: -2 points

#### 2. Extract Follower Cleanup Loop
**Rationale**: Isolate iteration logic, single responsibility
- Extract loop over follower entries
- Extract position cleanup logic
- Extract order cleanup logic
- **Target CYC Reduction**: -3 points

#### 3. Extract Order Cancellation Logic
**Rationale**: Separate concern, improve testability
- Extract order terminal state check
- Extract CancelOrderSafe call
- Handle null/invalid orders
- **Target CYC Reduction**: -1 point

#### 4. Reduce Logging Noise
**Rationale**: Improve readability, reduce line count
- Keep critical logging only
- Remove redundant LogBuffer.Format calls
- **Target Line Reduction**: 46 → ~25 lines

### OUT OF SCOPE ❌

#### 1. Thread Safety Model
**Rationale**: No lock-free violations detected
- LogBuffer.ValidateThreadAffinity already present
- No lock() blocks found
- Actor/FSM pattern not required here

#### 2. State Dictionary Refactoring
**Rationale**: Beyond method-level scope
- symmetryMasterEntryToDispatch structure unchanged
- symmetryDispatchById structure unchanged
- activePositions structure unchanged
- entryOrders structure unchanged

#### 3. Caller Integration
**Rationale**: 0 external callers (dead code investigation separate)
- No need to update call sites
- No signature changes required
- Reflection/dynamic dispatch investigation deferred

#### 4. Cross-File Changes
**Rationale**: Blast radius = 0
- No changes to other files
- No interface changes
- No public API modifications

## Extraction Plan

### Helper Method 1: TryGetSymmetryDispatch
```csharp
private SymmetryDispatch TryGetSymmetryDispatch(string masterEntryName)
{
    // Extract dispatch lookup logic
    // Return null if not found
    // CYC: 2
}
```

### Helper Method 2: CleanupFollowerEntry
```csharp
private void CleanupFollowerEntry(string followerEntryName, SymmetryDispatch dispatch)
{
    // Extract follower cleanup logic
    // Handle position cleanup
    // Handle order cleanup
    // CYC: 4
}
```

### Helper Method 3: CancelFollowerOrderIfActive
```csharp
private void CancelFollowerOrderIfActive(Order order)
{
    // Extract order cancellation logic
    // Check terminal state
    // Call CancelOrderSafe
    // CYC: 2
}
```

### Refactored Main Method
```csharp
private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
{
    // 1. Get dispatch (early return if null)
    // 2. Loop over followers, call CleanupFollowerEntry
    // 3. Log completion
    // Target CYC: 3
}
```

## Complexity Target

| Metric | Before | After | Delta |
|--------|--------|-------|-------|
| **Cyclomatic Complexity** | 11 | ≤8 | -3+ |
| **Max Nesting Depth** | 6 | ≤4 | -2 |
| **Lines of Code** | 46 | ~25 | -21 |
| **Method Count** | 1 | 4 | +3 |

## Risk Mitigation

### Testing Strategy
- **Unit Tests**: Test each helper method independently
- **Integration Test**: Test refactored main method
- **Regression Test**: Verify no behavior changes

### Rollback Plan
- **Git Branch**: EPIC-W7-044 (isolated)
- **Backup**: Original method preserved in git history
- **Verification**: F5 in NinjaTrader IDE

## Jane Street Alignment

### Principles Applied
1. ✅ **Cognitive Simplicity**: CYC 11 → ≤8
2. ✅ **Single Responsibility**: Each method does one thing
3. ✅ **Testability**: Smaller methods = exhaustive testing
4. ✅ **Early Returns**: Reduce nesting depth

### Principles Deferred
- **Correctness by Construction**: No type-level changes needed
- **Lock-Free Actor Pattern**: No concurrency issues detected

## Success Criteria

### Phase 1 Complete ✅
- [x] Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- [x] Extraction plan documented
- [x] Helper method signatures planned
- [x] Complexity targets set
- [x] Risk mitigation strategy defined

### Phase 2 Prerequisites
- [ ] Architecture plan approved
- [ ] Helper method signatures validated
- [ ] Test strategy confirmed

## Next Phase
Proceed to **Phase 2: Architecture Planning** to detail implementation approach.
