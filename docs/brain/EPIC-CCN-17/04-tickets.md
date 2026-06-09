# EPIC-CCN-17: Ticket Manifest

**Epic**: EPIC-CCN-17  
**Phase**: 4 (Ticket Generation)  
**Date**: 2026-06-09  
**Generator**: V12 Epic Planner  
**Status**: ✅ TICKETS GENERATED

---

## Executive Summary

**Ticket Count**: 3 (optimized from 5)  
**Total Effort**: 7 hours (reduced from 10 hours)  
**Execution Order**: Sequential (Ticket 1 → Ticket 2 → Ticket 3)  
**Mode**: `v12-engineer` (Bob CLI)

**Optimization Highlights**:
- ✅ 50% code reuse (2 existing helpers leveraged)
- ✅ 40% ticket reduction (5 → 3 tickets)
- ✅ 30% effort reduction (10h → 7h)
- ✅ Zero new lock() statements
- ✅ All helpers ≤15 CYC (Jane Street aligned)

---

## Ticket Breakdown

### Ticket 1: Extract RouteOrderToTargetDict()
**File**: [`ticket-01-extract-route-dict.md`](docs/brain/EPIC-CCN-17/ticket-01-extract-route-dict.md)  
**Objective**: Extract switch statement routing logic  
**Complexity**: CYC 8, ~45 LOC  
**Estimated Effort**: 2 hours  
**Dependencies**: None  
**Status**: Pending

**Scope**:
- Create new helper method `RouteOrderToTargetDict()`
- Extract lines 755-791 from [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713)
- Implement 7-case switch statement
- Return dictionary reference + key + dictName via out parameters
- Add TDD tests for all 7 classification cases

**Success Criteria**:
- ✅ Helper method compiles without errors
- ✅ All 7 classification cases tested
- ✅ Dictionary references validated (not copies)
- ✅ Build passes
- ✅ Zero new lock() statements

---

### Ticket 2: Extract AdoptSingleOrder()
**File**: [`ticket-02-extract-adopt-order.md`](docs/brain/EPIC-CCN-17/ticket-02-extract-adopt-order.md)  
**Objective**: Extract per-order adoption logic  
**Complexity**: CYC 10, ~60 LOC  
**Estimated Effort**: 3 hours  
**Dependencies**: Ticket 1 (requires `RouteOrderToTargetDict()`)  
**Status**: Pending

**Scope**:
- Create new helper method `AdoptSingleOrder()`
- Extract lines 793-838 from [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713)
- Call `RouteOrderToTargetDict()` for dictionary routing
- Call existing [`RebuildFleetPositionFromEntry()`](src/V12_002.SIMA.Lifecycle.cs:858) for position tracking
- Add TDD tests for entry/non-entry order paths

**Success Criteria**:
- ✅ Helper method compiles without errors
- ✅ Entry order path tested (position rebuild)
- ✅ Non-entry order path tested (force-sync)
- ✅ Integration with Ticket 1 helper verified
- ✅ Build passes
- ✅ Zero new lock() statements

---

### Ticket 3: Refactor Main Method
**File**: [`ticket-03-refactor-main.md`](docs/brain/EPIC-CCN-17/ticket-03-refactor-main.md)  
**Objective**: Refactor main method to orchestrate via helpers  
**Complexity**: CYC 8, ~51 LOC  
**Estimated Effort**: 2 hours  
**Dependencies**: Ticket 1 + Ticket 2 (requires both helpers)  
**Status**: Pending

**Scope**:
- Update [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713) to call helpers
- Remove inline switch statement (lines 755-791)
- Remove inline position tracking logic (lines 793-838)
- Preserve exact control flow and logging
- Run full test suite
- Verify F5 in NinjaTrader

**Success Criteria**:
- ✅ Main method reduced to CYC 8
- ✅ Main method reduced to ~51 LOC
- ✅ All tests pass
- ✅ F5 in NinjaTrader successful
- ✅ Complexity audit shows 78% reduction
- ✅ deploy-sync.ps1 successful

---

## Execution Strategy

### Sequential Execution (Required)

**Order**: Ticket 1 → Ticket 2 → Ticket 3

**Rationale**:
- Ticket 2 depends on Ticket 1 (`RouteOrderToTargetDict()`)
- Ticket 3 depends on Ticket 1 + Ticket 2 (both helpers)
- Cannot parallelize due to dependencies

### Per-Ticket Workflow

**For Each Ticket**:
1. Open new Bob CLI session in `v12-engineer` mode
2. Load ticket file: `/ticket docs/brain/EPIC-CCN-17/ticket-0X-*.md`
3. Execute ticket (extraction + tests)
4. Run pre-push validation: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
5. Commit changes: `git commit -m "EPIC-CCN-17: Ticket X - [description]"`
6. Run deploy-sync: `powershell -File .\deploy-sync.ps1`
7. Verify F5 in NinjaTrader
8. Close session

### Verification Checkpoints

**After Ticket 1**:
- ✅ `RouteOrderToTargetDict()` compiles
- ✅ 7 classification tests pass
- ✅ Build passes

**After Ticket 2**:
- ✅ `AdoptSingleOrder()` compiles
- ✅ Entry/non-entry tests pass
- ✅ Integration with Ticket 1 verified
- ✅ Build passes

**After Ticket 3**:
- ✅ Main method CYC reduced to 8
- ✅ All tests pass
- ✅ F5 in NinjaTrader successful
- ✅ Complexity audit shows 78% reduction
- ✅ deploy-sync.ps1 successful

---

## Code Reuse Summary

### Existing Helpers (Leveraged)

1. **[`ClassifyOrderByPrefix()`](src/V12_002.SIMA.Lifecycle.cs:993)**
   - **Status**: ✅ ALREADY EXISTS
   - **Reuse**: Called by main method (no extraction needed)
   - **Benefit**: Eliminates need for `IsAdoptableOrderState()` helper

2. **[`RebuildFleetPositionFromEntry()`](src/V12_002.SIMA.Lifecycle.cs:858)**
   - **Status**: ✅ ALREADY EXISTS
   - **Reuse**: Called by `AdoptSingleOrder()` (no extraction needed)
   - **Benefit**: Eliminates need for `EnsurePositionTracking()` helper

### New Helpers (Created)

3. **`RouteOrderToTargetDict()`**
   - **Status**: ❌ DOES NOT EXIST
   - **Ticket**: Ticket 1
   - **Purpose**: Extract switch statement routing logic

4. **`AdoptSingleOrder()`**
   - **Status**: ❌ DOES NOT EXIST
   - **Ticket**: Ticket 2
   - **Purpose**: Extract per-order adoption logic

**Reuse Efficiency**: 50% (2 of 4 helpers already exist)

---

## TDD Test Requirements

### Test Coverage Targets

**Ticket 1 Tests** (RouteOrderToTargetDict):
- ✅ Test all 7 classification cases (stop, target1-5, entry)
- ✅ Test stop order key extraction (both "Stop_" and "S_" prefixes)
- ✅ Test dictionary reference returns (not copies)
- ✅ Test null return for invalid classification
- ✅ Test out parameter population (key, dictName)

**Ticket 2 Tests** (AdoptSingleOrder):
- ✅ Test entry order path (position rebuild)
- ✅ Test non-entry order path (force-sync)
- ✅ Test adoptedCount increment
- ✅ Test integration with RouteOrderToTargetDict()
- ✅ Test integration with RebuildFleetPositionFromEntry()

**Ticket 3 Tests** (Main Method):
- ✅ Run existing test suite (no new tests required)
- ✅ Verify F5 in NinjaTrader
- ✅ Verify complexity reduction (CYC 37 → 8)

### Test File Location

**Target**: `tests/V12_Performance.Tests/SIMA/AdoptFleetOrdersTests.cs` (new file)

**Test Structure**:
```csharp
namespace V12_Performance.Tests.SIMA
{
    [TestFixture]
    public class AdoptFleetOrdersTests
    {
        // Ticket 1 tests
        [Test]
        public void RouteOrderToTargetDict_StopOrder_ReturnsStopDict() { }
        
        [Test]
        public void RouteOrderToTargetDict_Target1Order_ReturnsTarget1Dict() { }
        
        // ... (5 more classification tests)
        
        // Ticket 2 tests
        [Test]
        public void AdoptSingleOrder_EntryOrder_RebuildsPosition() { }
        
        [Test]
        public void AdoptSingleOrder_NonEntryOrder_ForceSyncs() { }
        
        // ... (3 more integration tests)
    }
}
```

---

## DNA Compliance Checklist

### Thread-Safety (Lock-Free Mandate)

- [x] Zero lock() statements in original method
- [x] Zero lock() statements in Ticket 1 helper
- [x] Zero lock() statements in Ticket 2 helper
- [x] All dictionary operations use ConcurrentDictionary
- [x] Actor-serialized execution model preserved

### ASCII-Only Compliance

- [x] All string literals are ASCII-only
- [x] No Unicode characters, emoji, or curly quotes
- [x] String operations use StringComparison.OrdinalIgnoreCase

### Extraction Floor (≥15 LOC)

- [x] RouteOrderToTargetDict(): ~45 LOC (300% above minimum)
- [x] AdoptSingleOrder(): ~60 LOC (400% above minimum)
- [x] Main method: ~51 LOC (340% above minimum)

### Correctness by Construction

- [x] Enum-based classification (OrderState, OrderAction)
- [x] Switch statement exhaustively handles all order types
- [x] Null checks prevent invalid state propagation
- [x] Dictionary key extraction uses safe Substring() operations

---

## Risk Assessment

### Technical Risks

**Risk 1**: Helper method signature mismatch
- **Likelihood**: LOW
- **Impact**: MEDIUM
- **Mitigation**: Validate signatures against existing helpers before extraction

**Risk 2**: Position tracking logic divergence
- **Likelihood**: LOW
- **Impact**: HIGH
- **Mitigation**: Preserve exact logic flow; call existing RebuildFleetPositionFromEntry()

**Risk 3**: Dictionary reference vs. copy confusion
- **Likelihood**: LOW
- **Impact**: HIGH
- **Mitigation**: Return dictionary reference (not copy); document in helper docstring

### PR Hygiene Risks

**Risk 1**: Diff size exceeds 10k chars
- **Likelihood**: LOW
- **Impact**: MEDIUM
- **Mitigation**: Estimated diff ~3,500 chars (well below limit)

**Risk 2**: Whitespace mutation bloat
- **Likelihood**: LOW
- **Impact**: LOW
- **Mitigation**: Run CSharpier before commit; verify with deploy-sync.ps1

---

## Success Metrics

### Complexity Reduction

**Current State**:
- CYC: 37
- LOC: 136
- Nesting: 8 levels

**Target State**:
- CYC: 8 (78% reduction)
- LOC: 51 (62% reduction)
- Nesting: 3 levels (62% reduction)

### Code Quality

**Before**:
- Codacy Grade: B
- Complexity Issues: 32% of files exceed threshold
- Dead Code: 0% (verified)

**After** (Expected):
- Codacy Grade: A (target)
- Complexity Issues: 31% of files exceed threshold (1 file fixed)
- Dead Code: 0% (maintained)

### Effort Efficiency

**Original Plan**: 10 hours (5 tickets)  
**Optimized Plan**: 7 hours (3 tickets)  
**Efficiency Gain**: 30% effort reduction

---

## Execution Guide

### Phase 5.1: Execute Ticket 1

**Command**:
```bash
bob --mode v12-engineer
/ticket docs/brain/EPIC-CCN-17/ticket-01-extract-route-dict.md
```

**Expected Duration**: 2 hours

**Deliverables**:
- `RouteOrderToTargetDict()` method created
- 7 classification tests added
- Build passes
- Commit: "EPIC-CCN-17: Ticket 1 - Extract RouteOrderToTargetDict()"

---

### Phase 5.2: Execute Ticket 2

**Command**:
```bash
bob --mode v12-engineer
/ticket docs/brain/EPIC-CCN-17/ticket-02-extract-adopt-order.md
```

**Expected Duration**: 3 hours

**Deliverables**:
- `AdoptSingleOrder()` method created
- Entry/non-entry tests added
- Integration with Ticket 1 verified
- Build passes
- Commit: "EPIC-CCN-17: Ticket 2 - Extract AdoptSingleOrder()"

---

### Phase 5.3: Execute Ticket 3

**Command**:
```bash
bob --mode v12-engineer
/ticket docs/brain/EPIC-CCN-17/ticket-03-refactor-main.md
```

**Expected Duration**: 2 hours

**Deliverables**:
- Main method refactored to CYC 8
- All tests pass
- F5 in NinjaTrader successful
- Complexity audit shows 78% reduction
- deploy-sync.ps1 successful
- Commit: "EPIC-CCN-17: Ticket 3 - Refactor AdoptFleetOrders()"

---

### Phase 6: Final Review

**Command**:
```bash
# Run full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1

# Verify complexity reduction
python scripts/complexity_audit.py

# Create PR
gh pr create --title "EPIC-CCN-17: Reduce AdoptFleetOrders() complexity (CYC 37→8)" --body "See docs/brain/EPIC-CCN-17/05-completion-report.md"
```

**Expected Duration**: 1 hour

**Deliverables**:
- Pre-push validation passes
- Complexity audit confirms 78% reduction
- PR created
- `05-completion-report.md` written

---

## Ticket Files

1. [`ticket-01-extract-route-dict.md`](docs/brain/EPIC-CCN-17/ticket-01-extract-route-dict.md)
2. [`ticket-02-extract-adopt-order.md`](docs/brain/EPIC-CCN-17/ticket-02-extract-adopt-order.md)
3. [`ticket-03-refactor-main.md`](docs/brain/EPIC-CCN-17/ticket-03-refactor-main.md)

---

**Manifest Generated**: 2026-06-09T08:07:00Z  
**Generator**: V12 Epic Planner  
**Next Action**: Generate individual ticket files