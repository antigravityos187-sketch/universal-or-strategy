# Extraction Tickets: EPIC-CCN-057

## Overview
- **Total Tickets**: 1
- **Execution Order**: Single atomic ticket
- **Estimated Effort**: 0.5 hours
- **Risk Level**: 🟢 LOW
- **Complexity Reduction**: 10 → 8 (Jane Street compliant)

---

## TICKET-1: Extract IsBracketOrderName Helper Method

### Scope
- **Current Method**: `ShouldProtectBracketOrder`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 1451-1476 (26 lines total)
- **Current CYC**: 10
- **Target CYC**: ≤8 (Original: 3, Helper: 8)
- **Extraction**: Create helper method to encapsulate 8 bracket order prefix checks

### Problem Statement
The `ShouldProtectBracketOrder` method has cyclomatic complexity of 10, exceeding Jane Street's strict threshold of 8. The complexity stems from 8 OR conditions checking for bracket order name prefixes (Stop_, S_, T1-T5_, Target_).

### Solution
Extract the bracket order detection logic into a dedicated helper method `IsBracketOrderName`, reducing the original method's complexity to 3 while keeping the helper at the acceptable threshold of 8.

### Implementation Steps

#### 1. Create Helper Method
**Location**: Insert immediately after `ShouldProtectBracketOrder` (after line 1476)

```csharp
/// <summary>
/// Determines if an order name matches bracket order naming patterns.
/// EPIC-CCN-057: Extracted from ShouldProtectBracketOrder for complexity reduction.
/// </summary>
/// <param name="orderName">The order name to check</param>
/// <returns>True if the order name matches any bracket order prefix pattern</returns>
private bool IsBracketOrderName(string orderName)
{
    return orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("S_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("Target_", StringComparison.OrdinalIgnoreCase);
}
```

#### 2. Refactor Original Method
**Replace lines 1456-1464** with single helper call:

```csharp
// Before (lines 1456-1464):
if (orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("S_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase)
    || orderName.StartsWith("Target_", StringComparison.OrdinalIgnoreCase))

// After:
if (IsBracketOrderName(orderName))
```

#### 3. Update XML Documentation
Add EPIC reference to original method's XML doc (line ~1448):

```csharp
/// <summary>
/// Determines if a bracket order should be protected from cancellation.
/// EPIC-CCN-057: Complexity reduced from 10 to 3 via IsBracketOrderName extraction.
/// </summary>
```

#### 4. Run CSharpier
```powershell
dotnet csharpier format src/V12_002.SIMA.Lifecycle.cs
```

### Acceptance Criteria
- [ ] Helper method `IsBracketOrderName` created with CYC = 8
- [ ] Original method `ShouldProtectBracketOrder` refactored with CYC = 3
- [ ] XML documentation updated with EPIC-CCN-057 reference
- [ ] CSharpier formatting applied successfully
- [ ] Build succeeds with zero errors (`build_readiness.ps1`)
- [ ] All existing tests pass (100% pass rate)
- [ ] Complexity audit confirms CYC ≤8 for both methods (`complexity_audit.py`)
- [ ] No lock() statements introduced (grep verification)
- [ ] ASCII-only compliance maintained (no Unicode characters)
- [ ] Diff size <500 characters (PR hygiene check)
- [ ] Semantic equivalence verified (zero behavioral changes)

### Validation Commands
```powershell
# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Complexity verification
python scripts/complexity_audit.py

# Build verification
powershell -File .\scripts\build_readiness.ps1

# Lock-free verification
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
```

### Dependencies
- **None** (first and only ticket)

### Risk Mitigation
- ✅ **Checkpointing**: Bob CLI automatic checkpointing enabled
- ✅ **Atomic Commits**: Two separate commits (helper creation, refactoring)
- ✅ **Rollback Ready**: Git restore point available
- ✅ **Semantic Equivalence**: Pure refactoring, zero functional changes
- ✅ **Thread Safety**: Both methods are pure functions (no state mutations)

### DNA Compliance Verification
- ✅ **Correctness by Construction**: Type-safe, immutable parameters
- ✅ **Lock-Free Actor Pattern**: No lock() statements, pure functions
- ✅ **ASCII-Only Compliance**: No Unicode characters in code
- ✅ **Jane Street Alignment**: CYC reduction 10 → 8 (compliant)

### Expected Diff Size
- **Estimated**: ~450 characters (4.5% of 10k limit)
- **Breakdown**:
  - Helper method creation: ~200 chars
  - Original method refactoring: ~150 chars
  - XML documentation update: ~100 chars

### Success Metrics
- **Complexity Reduction**: 10 → max(3, 8) = 8 ✅
- **Build Status**: PASS (zero errors)
- **Test Status**: PASS (100% pass rate)
- **PR Hygiene**: PASS (diff <500 chars)
- **DNA Compliance**: PASS (all 4 pillars)

---

## Execution Notes

### Bob CLI Configuration
- **Mode**: `v12-engineer`
- **Checkpointing**: Enabled (automatic)
- **Rules**: `.bob/rules-v12-engineer/`

### Phase 5 Verification Checklist
After implementation, verify:
1. ✅ Complexity audit shows CYC ≤8 for both methods
2. ✅ Build succeeds with zero compilation errors
3. ✅ All tests pass (no new tests required per scope boundary)
4. ✅ CSharpier formatting applied
5. ✅ No lock() statements introduced
6. ✅ ASCII-only compliance maintained
7. ✅ Diff size <500 characters

### Phase 6 Sign-off Checklist
1. ✅ Run `powershell -File .\deploy-sync.ps1`
2. ✅ F5 in NinjaTrader and verify BUILD_TAG
3. ✅ Update manifest.json with Phase 4 completion
4. ✅ Mark EPIC-CCN-057 as completed

---

## Ticket Metadata

**Created**: 2026-06-15T16:57:25Z
**Epic**: EPIC-CCN-057
**Phase**: 4 (Ticket Generation)
**Ticket Count**: 1
**Execution Model**: Single atomic ticket
**Estimated Duration**: 30 minutes
**Risk Level**: 🟢 LOW
**Approval Status**: ✅ READY FOR IMPLEMENTATION

---

*End of Ticket Breakdown*
