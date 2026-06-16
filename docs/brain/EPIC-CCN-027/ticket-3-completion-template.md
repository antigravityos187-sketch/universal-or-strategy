# Ticket Completion: EPIC-CCN-027 - TICKET-3

## Execution Summary
- **Ticket**: TICKET-3 - Extract DispatchToPhotonKernel (Zero-Allocation Dispatch)
- **Status**: READY FOR EXECUTION (Windows/.NET required)
- **Duration**: Estimated 2-3 hours
- **Bob CLI Session**: [To be filled during execution]

## Scope
- **Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Lines Extracted**: 762-795 (34 lines)
- **Current CYC**: ~8 (after TICKET-2)
- **Target CYC**: ≤8 (orchestrator + helper)
- **Method Type**: Controlled side effects (lock-free enqueue)

## Dependencies
- **TICKET-1** must be completed first (requires `BracketOrderSet` struct)
- **TICKET-2** must be completed first (requires state registration)

## Changes to Make

### 1. Extract DispatchToPhotonKernel Method
**Location**: `src/V12_002.SIMA.Dispatch.cs` (private method)

```csharp
private void DispatchToPhotonKernel(
    BracketOrderSet bracketOrders,
    double entryPrice,
    double stopPrice,
    int followerQty,
    int dispatchTargetCount,
    OrderAction action,
    int reservedDelta
)
{
    // [Implementation from lines 762-795]
    // See 05-execution-plan.md for full code
}
```

### 2. Update Orchestrator Call Site
**Location**: `src/V12_002.SIMA.Dispatch.cs` (replace lines 762-795)

```csharp
DispatchToPhotonKernel(
    bracketOrders,
    entryPrice,
    stopPrice,
    followerQty,
    dispatchTargetCount,
    action,
    reservedDelta
);
```

### 3. Add Tests
**Location**: `tests/V12_Performance.Tests/Core/SIMADispatchTests.cs`

- [ ] Add 6 unit tests for DispatchToPhotonKernel
- [ ] Add 3 integration tests for end-to-end flow
- [ ] Verify RED state (tests fail before extraction)
- [ ] Verify GREEN state (tests pass after extraction)

## Acceptance Criteria
- [ ] DispatchToPhotonKernel method extracted with CYC ≤8
- [ ] All 6 unit tests GREEN after extraction
- [ ] All 3 integration tests GREEN
- [ ] Orchestrator complexity ≤8 (final verification)
- [ ] Zero-allocation pattern preserved
- [ ] PhotonPool fallback logic intact
- [ ] Build succeeds (zero errors)
- [ ] Hard-link sync succeeds (deploy-sync.ps1)
- [ ] Complexity audit PASS (CYC ≤8 for all methods)

## Final Orchestrator Structure
After all 3 extractions, orchestrator should be:

```csharp
private void Dispatch_PublishMarketBracketToPhoton(...)
{
    // Step 1: Create bracket orders (pure function)
    var bracketOrders = CreateBracketOrders(...);

    // Step 2: Register state (controlled side effects)
    RegisterBracketState(...);

    // Step 3: Dispatch to kernel (zero-allocation)
    DispatchToPhotonKernel(...);
}
```

**Expected CYC**: 3-5 (three sequential calls + minimal branching)

## Verification Commands
```powershell
# Full test suite
dotnet test --filter "FullyQualifiedName~SIMADispatchTests"

# Complexity audit (all methods should be ≤8)
python scripts/complexity_audit.py

# Format code
dotnet csharpier format src/

# Build
dotnet build

# Sync hard links
powershell -File .\deploy-sync.ps1
```

## Test Results
- **Unit Tests**: [To be filled - 6 tests expected]
- **Integration Tests**: [To be filled - 3 tests expected]
- **Total Tests**: [To be filled - 19 tests expected]
- **Orchestrator CYC**: [To be filled - ≤8 expected]
- **Helper Methods CYC**: [To be filled - all ≤8 expected]
- **Build Status**: [To be filled - PASS expected]
- **Deploy Sync**: [To be filled - PASS expected]

## Issues Encountered
[To be filled during execution]

## Next Steps
1. Proceed to Phase 5.V (Verification)
2. Submit for Arena AI audit (P4 Gate)
3. Create PR for merge

---

**Template Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-027
**Phase**: 5 (Ticket Execution)
**Status**: READY FOR EXECUTION
