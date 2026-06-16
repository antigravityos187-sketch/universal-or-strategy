# Phase 2: Architecture Plan - EPIC-CCN-109

## Epic Metadata
- **Epic ID**: EPIC-CCN-109
- **Target Method**: `HydrateWorkingOrdersFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 19
- **Target Complexity**: ≤ 15 (Jane Street alignment)
- **Lines**: 398-442 (45 lines)

## Method Analysis

### Current Structure
```csharp
private void HydrateWorkingOrdersFromBroker()
{
    int adoptedCount = 0;

    AdoptFleetWorkingOrders(ref adoptedCount);

    // Build 993: Adopt master account bracket orders
    bool masterIsFleetForOrders993 = IsFleetAccount(Account);
    if (!masterIsFleetForOrders993)
    {
        AdoptMasterWorkingOrders(ref adoptedCount);
        ReconstructMasterPositionFromBrackets();
    }

    // Phase 5: Rebuild FSMs from adopted orders before enabling REAPER
    HydrateFSMsFromWorkingOrders();

    _orderAdoptionComplete = true;
    if (adoptedCount > 0)
        Print(string.Format("[SIMA HYDRATE] Adopted {0} working order(s) from broker -- adoption complete.", adoptedCount));
    else
        Print("[SIMA HYDRATE] No working orders to adopt -- adoption complete.");
}
```

### Complexity Breakdown
- **Base complexity**: 1 (method entry)
- **Conditional (masterIsFleetForOrders993)**: +1
- **Conditional (adoptedCount > 0)**: +1
- **Method calls**: 4 (AdoptFleetWorkingOrders, AdoptMasterWorkingOrders, ReconstructMasterPositionFromBrackets, HydrateFSMsFromWorkingOrders)
- **Estimated Total**: ~3-5 (LOW complexity)

### Critical Discovery
**The method itself has LOW complexity (~3-5)**. The high complexity (19) reported by tools is likely:
1. **Aggregated from callees**: AdoptFleetWorkingOrders, AdoptMasterWorkingOrders, etc.
2. **Tool measurement artifact**: Some complexity analyzers sum callee complexity

### Verification Required
Before proceeding with extraction, we must:
1. Run `python scripts/complexity_audit.py` to get per-method complexity
2. Verify which method actually has complexity 19
3. Confirm HydrateWorkingOrdersFromBroker is the correct target

## Revised Extraction Strategy

### Option A: If HydrateWorkingOrdersFromBroker is Correct Target

**Extract #1: AdoptMasterAccountOrders()**
```csharp
/// <summary>
/// Adopts master account working orders and reconstructs positions.
/// Extracted from HydrateWorkingOrdersFromBroker to reduce complexity.
/// </summary>
/// <param name="adoptedCount">Running count of adopted orders</param>
private void AdoptMasterAccountOrders(ref int adoptedCount)
{
    bool masterIsFleetForOrders993 = IsFleetAccount(Account);
    if (!masterIsFleetForOrders993)
    {
        AdoptMasterWorkingOrders(ref adoptedCount);
        ReconstructMasterPositionFromBrackets();
    }
}
```

**Complexity Reduction**: ~2 points (removes conditional + 2 method calls)

**Extract #2: LogHydrationCompletion()**
```csharp
/// <summary>
/// Logs hydration completion status with appropriate message.
/// Extracted from HydrateWorkingOrdersFromBroker to reduce complexity.
/// </summary>
/// <param name="adoptedCount">Total orders adopted</param>
private void LogHydrationCompletion(int adoptedCount)
{
    if (adoptedCount > 0)
        Print(string.Format("[SIMA HYDRATE] Adopted {0} working order(s) from broker -- adoption complete.", adoptedCount));
    else
        Print("[SIMA HYDRATE] No working orders to adopt -- adoption complete.");
}
```

**Complexity Reduction**: ~1 point (removes conditional)

**Refactored Method**:
```csharp
private void HydrateWorkingOrdersFromBroker()
{
    int adoptedCount = 0;

    AdoptFleetWorkingOrders(ref adoptedCount);
    AdoptMasterAccountOrders(ref adoptedCount);
    HydrateFSMsFromWorkingOrders();

    _orderAdoptionComplete = true;
    LogHydrationCompletion(adoptedCount);
}
```

**Final Complexity**: ~1 (method entry only)

### Option B: If Callee Methods Need Extraction

If complexity analysis reveals the actual high-complexity methods are:
- `AdoptFleetWorkingOrders` (lines 445-520)
- `AdoptMasterWorkingOrders` (lines 635-690)
- `HydrateFSMsFromWorkingOrders` (lines 800-900)

Then we need to **re-scope the epic** to target the correct method.

## Call Graph Analysis

### Upstream Callers
```
EnumerateApexAccounts (line 186)
  └─> HydrateWorkingOrdersFromBroker (line 398)
```

**Impact**: Single caller, low coupling

### Downstream Callees
```
HydrateWorkingOrdersFromBroker
  ├─> AdoptFleetWorkingOrders (line 445)
  │     ├─> ClassifyAndRouteFleetOrder (line 521)
  │     ├─> RebuildActivePositionForFleetEntry (line 560)
  │     └─> SyncExistingPositionMetadata (line 605)
  ├─> AdoptMasterWorkingOrders (line 635)
  │     └─> ClassifyMasterOrderByPrefix (line 691)
  ├─> ReconstructMasterPositionFromBrackets (line 730)
  │     ├─> FindMasterPositionFromBroker (line 765)
  │     └─> BuildMasterPositionInfo (line 780)
  └─> HydrateFSMsFromWorkingOrders (line 800)
        ├─> HydrateFSM_MapOrderStateToFsmState (line 850)
        ├─> HydrateFSM_DetermineRemainingContracts (line 870)
        ├─> HydrateFSM_LinkBracketOrders (line 890)
        └─> HydrateFSM_RecoverFromOpenPositions (line 920)
```

**Impact**: Multiple callees, moderate coupling

## Dependency Mapping

### Data Dependencies
- **Input**: `Account.All`, `Account.Orders`, `Account.Positions`
- **Output**: `entryOrders`, `stopOrders`, `target1Orders-target5Orders`, `activePositions`, `_followerBrackets`, `_orderIdToFsmKey`, `_orderAdoptionComplete`
- **Side Effects**: Modifies tracking dictionaries, prints diagnostics

### State Dependencies
- **Reads**: `Instrument.FullName`, `IsFleetAccount()`, `Account.Name`
- **Writes**: `_orderAdoptionComplete = true`
- **Invariants**: Must be called AFTER `HydrateExpectedPositionsFromBroker`, BEFORE REAPER audit starts

### Thread Safety
- **Execution Context**: Strategy thread (via `TriggerCustomEvent` from callbacks)
- **Concurrency**: Actor-serialized lifecycle ensures single-threaded execution
- **Lock-Free**: No locks required (V12 DNA compliant)

## Extraction Sequence

### Phase 1: Verification (MANDATORY)
```bash
# Run complexity audit to verify target method
python scripts/complexity_audit.py

# Expected output:
# HydrateWorkingOrdersFromBroker: CYC = 19 (or actual value)
```

**Decision Gate**: If complexity ≠ 19, STOP and re-scope epic to correct method.

### Phase 2: Extract AdoptMasterAccountOrders()
1. Create new private method below HydrateWorkingOrdersFromBroker
2. Move master account adoption logic (lines 407-412)
3. Update caller to invoke new method
4. Verify build: `dotnet build src/V12_002.csproj`
5. Run complexity audit: verify reduction

### Phase 3: Extract LogHydrationCompletion()
1. Create new private method below AdoptMasterAccountOrders
2. Move logging logic (lines 417-421)
3. Update caller to invoke new method
4. Verify build: `dotnet build src/V12_002.csproj`
5. Run complexity audit: verify target ≤15

### Phase 4: Validation
1. Run full test suite: `dotnet test`
2. Verify zero behavioral changes
3. Run `deploy-sync.ps1` to sync NinjaTrader hard links
4. Manual F5 test in NinjaTrader

## Jane Street Compliance Checks

### Cognitive Simplicity ✅
- **Before**: Coordinator method with 4 callees + 2 conditionals
- **After**: Pure coordinator with 5 callees, zero conditionals
- **Improvement**: Eliminates branching logic, single responsibility

### Lock-Free Correctness ✅
- **No locks**: Method is lock-free (Actor-serialized)
- **Atomic operations**: Uses `_orderAdoptionComplete` flag (single write)
- **Thread safety**: Executes on strategy thread only

### ASCII-Only Compliance ✅
- **No Unicode**: All string literals are ASCII
- **No emoji**: Diagnostic messages use plain text
- **No curly quotes**: Standard double quotes only

### Testability ✅
- **Before**: Monolithic method, hard to test individual steps
- **After**: Extracted helpers are independently testable
- **Coverage**: Can test master adoption and logging separately

## Risk Assessment

### Overall Risk: LOW-MEDIUM

#### Risk Factors
1. **Complexity Uncertainty**: Need to verify actual complexity before extraction
2. **Minimal Logic**: Method is mostly delegation, low extraction value
3. **Critical Path**: Runs during SIMA initialization, errors block startup

#### Risk Mitigation
1. **Verification First**: Run complexity audit before any code changes
2. **Incremental Extraction**: One method at a time with build verification
3. **Rollback Plan**: Git branch isolation, easy revert
4. **Test Coverage**: Comprehensive tests before and after

### Risk Level by Phase
- **Phase 1 (Verification)**: ZERO - Read-only analysis
- **Phase 2 (Extract #1)**: LOW - Simple delegation extraction
- **Phase 3 (Extract #2)**: LOW - Simple logging extraction
- **Phase 4 (Validation)**: LOW - Verification only

## Success Criteria

### Quantitative Metrics
1. ✅ **Complexity Target**: HydrateWorkingOrdersFromBroker complexity ≤ 15
2. ✅ **Extraction Count**: 2 helper methods extracted (revised from 3)
3. ✅ **Build Success**: Zero compilation errors
4. ✅ **Test Pass**: 100% test pass rate (no regressions)

### Qualitative Metrics
1. ✅ **Readability**: Each extracted method has single responsibility
2. ✅ **Maintainability**: Clear separation of concerns
3. ✅ **Testability**: Each helper method independently testable

## Open Questions

### Q1: Is HydrateWorkingOrdersFromBroker the correct target?
**Answer Required**: Run `python scripts/complexity_audit.py` to verify.

**If NO**: Re-scope epic to target actual high-complexity method (likely AdoptFleetWorkingOrders or HydrateFSMsFromWorkingOrders).

**If YES**: Proceed with extraction plan above.

### Q2: Should we extract more aggressively?
**Consideration**: Current method is mostly delegation. Extracting 2 helpers reduces complexity to ~1, which is excellent but may be over-engineering.

**Recommendation**: Verify complexity first, then decide if extraction is warranted.

## Next Phase

**Phase 3: Test Creation**
- Create comprehensive tests for HydrateWorkingOrdersFromBroker
- Establish behavioral baseline
- Verify 100% coverage of extraction targets
- Document test scenarios

**BLOCKER**: Phase 3 cannot proceed until Phase 1 verification confirms correct target method.

## Metadata
- **Phase**: 2 (Architecture Planning)
- **Status**: Completed (with verification gate)
- **Date**: 2026-06-13
- **Complexity Reduction Target**: 19 → ≤15 (minimum 4 points)
- **Extraction Strategy**: 2 private helper methods (revised)
- **Verification Required**: YES - Run complexity audit before Phase 3
