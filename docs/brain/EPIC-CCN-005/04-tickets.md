# Extraction Tickets: EPIC-CCN-005

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours
- **Method**: ClassifyAndRouteFleetOrder
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 16
- **Target Complexity**: ≤8 (Jane Street standard)

## TICKET-1: Extract DetermineOrderRouting Helper

### Scope
- **Current Method**: `ClassifyAndRouteFleetOrder`
- **Current CYC**: 16
- **Target CYC**: ≤4 (helper method)
- **Extraction**: Prefix-based routing logic into dedicated helper method

### Method Signature
```csharp
private (ConcurrentDictionary<string, Order> targetDict, string dictName, int prefixLength) 
    DetermineOrderRouting(string orderName)
```

### Implementation (TDD Workflow)

#### Step 1: Write Unit Tests
Create test file: `tests/V12_Performance.Tests/SIMA/ClassifyAndRouteFleetOrderTests.cs`

Test cases:
1. **Stop_ prefix** → Returns stopOrders dictionary, "stopOrders", 5
2. **S_ prefix** → Returns stopOrders dictionary, "stopOrders", 2
3. **T1_ prefix** → Returns target1Orders dictionary, "target1Orders", 3
4. **T2_ prefix** → Returns target2Orders dictionary, "target2Orders", 3
5. **T3_ prefix** → Returns target3Orders dictionary, "target3Orders", 3
6. **T4_ prefix** → Returns target4Orders dictionary, "target4Orders", 3
7. **Unknown prefix** → Returns null/default, empty string, 0
8. **Null/empty name** → Returns null/default, empty string, 0

#### Step 2: Implement Helper Method
```csharp
private (ConcurrentDictionary<string, Order> targetDict, string dictName, int prefixLength) 
    DetermineOrderRouting(string orderName)
{
    if (string.IsNullOrEmpty(orderName))
    {
        return (null, string.Empty, 0);
    }

    if (orderName.StartsWith("Stop_"))
    {
        return (stopOrders, "stopOrders", 5);
    }
    if (orderName.StartsWith("S_"))
    {
        return (stopOrders, "stopOrders", 2);
    }
    if (orderName.StartsWith("T1_"))
    {
        return (target1Orders, "target1Orders", 3);
    }
    if (orderName.StartsWith("T2_"))
    {
        return (target2Orders, "target2Orders", 3);
    }
    if (orderName.StartsWith("T3_"))
    {
        return (target3Orders, "target3Orders", 3);
    }
    if (orderName.StartsWith("T4_"))
    {
        return (target4Orders, "target4Orders", 3);
    }

    return (null, string.Empty, 0);
}
```

#### Step 3: Verify Complexity
Run: `python scripts/complexity_audit.py`
Expected: CYC ≤4 for DetermineOrderRouting

#### Step 4: Run Tests
Run: `dotnet test --filter "FullyQualifiedName~ClassifyAndRouteFleetOrderTests"`
Expected: 100% pass rate

### Acceptance Criteria
- [ ] Unit tests written for all 8 test cases
- [ ] Helper method implemented with correct signature
- [ ] Method complexity ≤4 (verified by complexity_audit.py)
- [ ] All unit tests pass (100% pass rate)
- [ ] No lock() statements introduced (forensic scan)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting passes (dotnet csharpier check src/)

### Dependencies
- None (first ticket)

### Git Checkpoint
After completion: `git add . && git commit -m "EPIC-CCN-005 TICKET-1: Extract DetermineOrderRouting helper"`

---

## TICKET-2: Extract ExtractOrderKey Helper

### Scope
- **Current Method**: `ClassifyAndRouteFleetOrder`
- **Current CYC**: 16 (unchanged from TICKET-1)
- **Target CYC**: ≤4 (helper method)
- **Extraction**: Order key extraction logic into dedicated helper method

### Method Signature
```csharp
private string ExtractOrderKey(string orderName, int prefixLength)
```

### Implementation (TDD Workflow)

#### Step 1: Write Unit Tests
Add to: `tests/V12_Performance.Tests/SIMA/ClassifyAndRouteFleetOrderTests.cs`

Test cases:
1. **Valid name + prefix** → Returns substring after prefix
   - Input: ("Stop_ABC123", 5) → Output: "ABC123"
   - Input: ("S_XYZ", 2) → Output: "XYZ"
   - Input: ("T1_Order1", 3) → Output: "Order1"
2. **Null name** → Returns empty string
3. **Empty name** → Returns empty string
4. **Prefix length = name length** → Returns empty string
5. **Prefix length > name length** → Returns empty string (edge case)

#### Step 2: Implement Helper Method
```csharp
private string ExtractOrderKey(string orderName, int prefixLength)
{
    if (string.IsNullOrEmpty(orderName) || prefixLength <= 0)
    {
        return string.Empty;
    }

    if (prefixLength >= orderName.Length)
    {
        return string.Empty;
    }

    return orderName.Substring(prefixLength);
}
```

#### Step 3: Verify Complexity
Run: `python scripts/complexity_audit.py`
Expected: CYC ≤4 for ExtractOrderKey

#### Step 4: Run Tests
Run: `dotnet test --filter "FullyQualifiedName~ClassifyAndRouteFleetOrderTests"`
Expected: 100% pass rate (including TICKET-1 tests)

### Acceptance Criteria
- [ ] Unit tests written for all 5 test cases
- [ ] Helper method implemented with correct signature
- [ ] Method complexity ≤4 (verified by complexity_audit.py)
- [ ] All unit tests pass (100% pass rate)
- [ ] No lock() statements introduced (forensic scan)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting passes (dotnet csharpier check src/)

### Dependencies
- TICKET-1 must be completed first (DetermineOrderRouting helper exists)

### Git Checkpoint
After completion: `git add . && git commit -m "EPIC-CCN-005 TICKET-2: Extract ExtractOrderKey helper"`

---

## TICKET-3: Refactor Main Method

### Scope
- **Current Method**: `ClassifyAndRouteFleetOrder`
- **Current CYC**: 16
- **Target CYC**: ≤8 (Jane Street standard)
- **Refactoring**: Replace if-else chain with helper method calls

### Implementation (TDD Workflow)

#### Step 1: Verify Existing Tests
Run: `dotnet test`
Expected: 100% pass rate (baseline behavior)

#### Step 2: Refactor Main Method
Replace the existing if-else chain (lines 531-573) with helper calls:

```csharp
private ConcurrentDictionary<string, Order> ClassifyAndRouteFleetOrder(
    Order ord,
    out string orderKey,
    out string dictName)
{
    // Use helper to determine routing
    var (targetDict, dictNameResult, prefixLength) = DetermineOrderRouting(ord.Name);
    
    // Use helper to extract key
    orderKey = ExtractOrderKey(ord.Name, prefixLength);
    dictName = dictNameResult;
    
    return targetDict;
}
```

#### Step 3: Verify Complexity
Run: `python scripts/complexity_audit.py`
Expected: CYC ≤8 for ClassifyAndRouteFleetOrder

#### Step 4: Run Full Test Suite
Run: `dotnet test`
Expected: 100% pass rate (behavior preservation verified)

#### Step 5: Forensic Scan
Run: `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs`
Expected: Zero matches (no lock() blocks)

#### Step 6: Format Check
Run: `dotnet csharpier check src/`
Expected: Zero formatting issues

#### Step 7: Build Verification
Run: `dotnet build`
Expected: Zero errors

#### Step 8: Hard-Link Sync
Run: `powershell -File .\deploy-sync.ps1`
Expected: Success (NinjaTrader hard links synchronized)

### Acceptance Criteria
- [ ] Main method refactored to use helper methods
- [ ] Method complexity ≤8 (verified by complexity_audit.py)
- [ ] All existing tests pass (100% pass rate)
- [ ] No lock() statements in file (forensic scan)
- [ ] No formatting issues (CSharpier)
- [ ] Build succeeds (dotnet build)
- [ ] Hard-link sync succeeds (deploy-sync.ps1)
- [ ] Behavior identical to pre-extraction (verified by tests)

### Dependencies
- TICKET-1 must be completed (DetermineOrderRouting helper exists)
- TICKET-2 must be completed (ExtractOrderKey helper exists)

### Git Checkpoint
After completion: `git add . && git commit -m "EPIC-CCN-005 TICKET-3: Refactor main method to use helpers"`

---

## Post-Extraction Verification Checklist

### Complexity Verification
- [ ] Main method CYC ≤8 (Jane Street standard)
- [ ] Helper 1 CYC ≤4 (Jane Street standard)
- [ ] Helper 2 CYC ≤4 (Jane Street standard)
- [ ] Total complexity distributed: 16 → (8 + 4 + 4)

### DNA Compliance
- [ ] Zero lock() blocks (forensic scan)
- [ ] ASCII-only strings (no Unicode)
- [ ] Correctness by construction (type-safe tuples)
- [ ] Actor/FSM pattern preserved

### PR Hygiene
- [ ] Diff size <10k characters
- [ ] No whitespace mutations
- [ ] No unrelated changes
- [ ] Surgical scope (single method)

### Build & Test
- [ ] All tests pass (100% pass rate)
- [ ] Zero compilation errors
- [ ] Zero formatting issues
- [ ] Hard-link sync successful

### Jane Street Alignment
- [ ] Cognitive simplicity: CYC ≤8 (main), ≤4 (helpers)
- [ ] Microsecond latency preserved (no architectural changes)
- [ ] Exhaustive testing feasible (reduced path explosion)
- [ ] JIT inlining eligible (small helpers)

---

## Execution Summary

### Estimated Timeline
- **TICKET-1**: 1.5-2 hours (write tests + implement + verify)
- **TICKET-2**: 1-1.5 hours (write tests + implement + verify)
- **TICKET-3**: 1.5-2 hours (refactor + full verification)
- **Total**: 4-6 hours

### Risk Mitigation
- **TDD Workflow**: Tests written before implementation (behavior preservation)
- **Incremental Extraction**: One helper at a time, test after each
- **Git Checkpoints**: Rollback capability after each ticket
- **Automated Verification**: Complexity audit + forensic scan + format check

### Success Metrics
- **Complexity Reduction**: 16 → 8 (50% reduction in main method)
- **Cognitive Load**: 2^16 paths → 2^8 paths (99.6% reduction)
- **Test Coverage**: 100% pass rate maintained
- **Build Health**: Zero errors, zero warnings

---

**Document Version**: 1.0  
**Created**: 2026-06-15  
**Epic**: EPIC-CCN-005  
**Phase**: 4 (Ticket Generation)  
**Status**: COMPLETE  
**Next Phase**: Phase 5 (Ticket Execution)
