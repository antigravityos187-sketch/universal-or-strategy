# Ticket Completion: EPIC-CCN-027 - TICKET-2

## Execution Summary
- **Ticket**: TICKET-2 - Extract RegisterBracketState (State Registration)
- **Status**: READY FOR EXECUTION (Windows/.NET required)
- **Duration**: Estimated 2-3 hours
- **Bob CLI Session**: [To be filled during execution]

## Scope
- **Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Lines Extracted**: 712-760 (49 lines)
- **Current CYC**: ~13 (after TICKET-1)
- **Target CYC**: ≤8
- **Method Type**: Controlled side effects (atomic writes only)

## Dependencies
- **TICKET-1** must be completed first (requires `BracketOrderSet` struct)

## Changes to Make

### 1. Extract RegisterBracketState Method
**Location**: `src/V12_002.SIMA.Dispatch.cs` (private method)

```csharp
private void RegisterBracketState(
    BracketOrderSet bracketOrders,
    Account acct,
    OrderAction action,
    PositionInfo fleetPos,
    string fleetEntryName,
    string expectedKey,
    string ocoId,
    int followerQty,
    ref bool syncPending,
    ref int reservedDelta,
    ref bool registeredForCleanup
)
{
    // [Implementation from lines 712-760]
    // See 05-execution-plan.md for full code
}
```

### 2. Update Orchestrator Call Site
**Location**: `src/V12_002.SIMA.Dispatch.cs` (replace lines 712-760)

```csharp
RegisterBracketState(
    bracketOrders,
    acct,
    action,
    fleetPos,
    fleetEntryName,
    expectedKey,
    ocoId,
    followerQty,
    ref syncPending,
    ref reservedDelta,
    ref registeredForCleanup
);
```

### 3. Add Tests
**Location**: `tests/V12_Performance.Tests/Core/SIMADispatchTests.cs`

- [ ] Add 4 test cases for RegisterBracketState
- [ ] Verify RED state (tests fail before extraction)
- [ ] Verify GREEN state (tests pass after extraction)

## Acceptance Criteria
- [ ] RegisterBracketState method extracted with CYC ≤8
- [ ] All 4 tests GREEN after extraction
- [ ] Lock-free verified (zero lock() statements)
- [ ] FSM ordering invariant preserved
- [ ] Build succeeds (zero errors)
- [ ] Complexity audit PASS (CYC ≤8)

## Verification Commands
```powershell
# Lock-free check
grep -n "lock(" src/V12_002.SIMA.Dispatch.cs

# Run tests
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.RegisterBracketState"

# Complexity check
python scripts/complexity_audit.py

# Build verification
dotnet build
```

## Test Results
- **Unit Tests**: [To be filled - 4 tests expected]
- **Lock-Free**: [To be filled - zero lock() expected]
- **Complexity**: [To be filled - CYC ≤8 expected]
- **Build Status**: [To be filled - PASS expected]

## Issues Encountered
[To be filled during execution]

## Next Steps
1. Execute TICKET-3: Extract DispatchToPhotonKernel
2. Verify orchestrator complexity reduced to ~8

---

**Template Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-027
**Phase**: 5 (Ticket Execution)
**Status**: READY FOR EXECUTION
