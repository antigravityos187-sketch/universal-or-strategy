# Extraction Tickets: EPIC-CCN-059

## Overview
- **Total Tickets**: 1
- **Execution Order**: Sequential (TICKET-1)
- **Estimated Effort**: 1-2 hours
- **Target Method**: `AdoptMasterWorkingOrders`
- **Current CYC**: 9
- **Target CYC**: ≤8 (Jane Street strict standard)

## TICKET-1: Extract Order Filtering Logic

### Scope
- **Current Method**: `AdoptMasterWorkingOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 1088-1165 (78 lines)
- **Current CYC**: 9
- **Target CYC**: ≤8
- **Extraction**: Order filtering conditionals into `ShouldAdoptMasterOrder` helper method

### Problem Statement
The method currently has inline conditionals for order filtering (instrument match and state validation) that contribute to cyclomatic complexity. Extracting this logic into a dedicated helper method will:
1. Reduce CYC from 9 to 8 (Jane Street strict standard)
2. Improve readability with clear intent ("should we adopt this order?")
3. Maintain existing lock-free, type-safe patterns

### Implementation

#### Step 1: Create Helper Method
Add new private helper method after existing helpers:

```csharp
private bool ShouldAdoptMasterOrder(Order ord)
{
    if (ord.Instrument?.FullName != Instrument?.FullName)
    {
        return false;
    }
    if (!IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true))
    {
        return false;
    }
    return true;
}
```

#### Step 2: Replace Inline Conditionals
In `AdoptMasterWorkingOrders`, replace the filtering logic (lines ~1092-1103):

**Before**:
```csharp
foreach (Order ord in Account.Orders)
{
    if (ord.Instrument?.FullName != Instrument?.FullName)
        continue;
    if (!IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true))
        continue;
    
    // ... rest of logic
}
```

**After**:
```csharp
foreach (Order ord in Account.Orders)
{
    if (!ShouldAdoptMasterOrder(ord))
    {
        continue;
    }
    
    // ... rest of logic
}
```

#### Step 3: Verify Complexity Reduction
Run complexity audit to confirm CYC≤8:
```bash
python scripts/complexity_audit.py
```

### Acceptance Criteria
- [ ] New `ShouldAdoptMasterOrder` helper method created
- [ ] Inline filtering conditionals replaced with helper call
- [ ] Method complexity reduced to CYC≤8 (verified by complexity_audit.py)
- [ ] All existing tests pass (100% pass rate)
- [ ] Build succeeds (dotnet build)
- [ ] No lock() blocks introduced (grep verification)
- [ ] ASCII-only compliance maintained
- [ ] Hard-link sync completed (deploy-sync.ps1)

### DNA Compliance Checklist
- [ ] **Correctness by Construction**: Type safety maintained (Order parameter, boolean return)
- [ ] **Lock-Free Actor Pattern**: No synchronization primitives added
- [ ] **ASCII-Only**: No Unicode characters in new code
- [ ] **Jane Street Alignment**: CYC≤8 achieved, cognitive simplicity improved

### Testing Strategy
1. **Unit Tests**: Verify existing FSM/Actor tests pass
2. **Complexity Audit**: Run `python scripts/complexity_audit.py` → confirm CYC≤8
3. **Lock-Free Verification**: Run `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` → zero matches
4. **Build Verification**: Run `powershell -File .\scripts\build_readiness.ps1` → success
5. **Hard-Link Sync**: Run `powershell -File .\deploy-sync.ps1` → success

### Dependencies
- None (first and only ticket)

### Risk Assessment
- **Risk Level**: MINIMAL
- **Scope**: Single method, single extraction
- **Breaking Changes**: None (internal implementation only)
- **Rollback**: Checkpointing enabled via Bob CLI

### Notes
- Existing helpers already in place: `ClassifyMasterOrderByPrefix`, `GetOrderDictionaryByName`, `IsOrderStateAdoptable`
- Static lookup table `_orderPrefixMappings` remains unchanged
- ConcurrentDictionary operations remain lock-free
- Method signature unchanged (no caller modifications needed)

---

## Execution Checklist

### Pre-Execution
- [ ] Read audit report (03-audit-report.md)
- [ ] Verify current CYC=9 baseline
- [ ] Confirm checkpointing enabled

### During Execution
- [ ] Create `ShouldAdoptMasterOrder` helper
- [ ] Replace inline conditionals
- [ ] Run complexity audit
- [ ] Run all tests
- [ ] Verify build success

### Post-Execution
- [ ] Confirm CYC≤8 achieved
- [ ] Run deploy-sync.ps1
- [ ] Update manifest.json (Phase 4 completed)
- [ ] Commit with message: "EPIC-CCN-059: Extract order filtering logic (CYC 9→8)"

---

**Generated**: 2026-06-15
**Protocol**: V12.23 Phase 4 Ticket Generation
**Agent**: Bob Shell (v12-engineer mode)
