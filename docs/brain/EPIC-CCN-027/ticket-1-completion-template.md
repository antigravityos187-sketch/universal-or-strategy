# Ticket Completion: EPIC-CCN-027 - TICKET-1

## Execution Summary
- **Ticket**: TICKET-1 - Extract CreateBracketOrders (Pure Function)
- **Status**: READY FOR EXECUTION (Windows/.NET required)
- **Duration**: Estimated 2-3 hours
- **Bob CLI Session**: [To be filled during execution]

## Scope
- **Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Lines Extracted**: 606-710 (105 lines)
- **Current CYC**: 21
- **Target CYC**: ≤8
- **Method Type**: Pure function (no side effects, deterministic)

## Changes to Make

### 1. Define BracketOrderSet Struct
**Location**: `src/V12_002.SIMA.Dispatch.cs` (near other structs)

```csharp
private struct BracketOrderSet
{
    public Order Entry;
    public Order Stop;
    public List<Order> OrdersToSubmit;
    public List<StagedTarget> StagedTargets;
    public int NonRunnerLimitQty;
    public int RunnerQty;
}
```

### 2. Extract CreateBracketOrders Method
**Location**: `src/V12_002.SIMA.Dispatch.cs` (private method)

```csharp
private BracketOrderSet CreateBracketOrders(
    Account acct,
    OrderAction action,
    Order entry,
    PositionInfo fleetPos,
    string fleetEntryName,
    string ocoId,
    int dispatchTargetCount,
    StringBuilder dispatchLog
)
{
    // [Implementation from lines 606-710]
    // See 05-execution-plan.md for full code
}
```

### 3. Update Orchestrator Call Site
**Location**: `src/V12_002.SIMA.Dispatch.cs` (replace lines 606-710)

```csharp
var bracketOrders = CreateBracketOrders(
    acct,
    action,
    entry,
    fleetPos,
    fleetEntryName,
    ocoId,
    dispatchTargetCount,
    dispatchLog
);

var ordersToSubmit = bracketOrders.OrdersToSubmit;
var stop = bracketOrders.Stop;
var stagedTargets = bracketOrders.StagedTargets;
int nonRunnerLimitQty = bracketOrders.NonRunnerLimitQty;
int runnerQty = bracketOrders.RunnerQty;
```

### 4. Implement Tests
**Location**: `tests/V12_Performance.Tests/Core/SIMADispatchTests.cs`

- [x] Test file created with 6 test case stubs
- [ ] Implement test mocks and assertions
- [ ] Verify RED state (tests fail before extraction)
- [ ] Verify GREEN state (tests pass after extraction)

## Acceptance Criteria
- [ ] BracketOrderSet struct defined
- [ ] CreateBracketOrders method extracted with CYC ≤8
- [ ] All 6 tests GREEN after extraction
- [ ] Pure function verified (no side effects)
- [ ] Build succeeds (zero errors)
- [ ] Formatting applied (CSharpier)
- [ ] Complexity audit PASS (CYC ≤8)

## Verification Commands
```powershell
# Run tests
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.CreateBracketOrders"

# Complexity check
python scripts/complexity_audit.py

# Format code
dotnet csharpier format src/V12_002.SIMA.Dispatch.cs

# Build verification
dotnet build
```

## Test Results
- **Unit Tests**: [To be filled - 6 tests expected]
- **Complexity**: [To be filled - CYC ≤8 expected]
- **Build Status**: [To be filled - PASS expected]
- **Format Status**: [To be filled - PASS expected]

## Issues Encountered
[To be filled during execution]

## Next Steps
1. Execute TICKET-2: Extract RegisterBracketState
2. Verify orchestrator complexity reduced to ~13

---

**Template Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-027
**Phase**: 5 (Ticket Execution)
**Status**: READY FOR EXECUTION
