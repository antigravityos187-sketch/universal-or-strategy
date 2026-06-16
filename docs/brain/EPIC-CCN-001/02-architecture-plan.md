# Phase 2: Architecture Planning - EPIC-CCN-001

## Target Method Analysis

**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Current Metrics**:
- Cyclomatic Complexity: 18
- Lines of Code: 76
- Tier: 1 (High Priority)

**Target Metrics** (Jane Street Strict Standard):
- Cyclomatic Complexity: ≤8 per method
- Cognitive Simplicity: Single responsibility per function
- Lock-Free: FSM/Actor pattern compliance

---

## Extraction Strategy

### Current Method Structure

The method has three distinct logical branches:

1. **Cancellation Branch** (Lines 15-31): If target should be cancelled (filled/runner/invalid qty)
   - Checks: isFilled OR isRunner OR qty <= 0
   - Action: Cancel stale order if cancellable, remove from dict
   - Complexity: ~6 (nested conditionals + OrderState checks)

2. **Early Exit Branch** (Lines 33-34): If no old target exists
   - Check: NOT dict.TryGetValue OR oldTarget == null
   - Action: Return early
   - Complexity: ~2

3. **Replacement Branch** (Lines 43-74): If old target needs price update
   - Check: OrderState is cancellable
   - Action: Create FollowerTargetReplaceSpec, stamp grace window, cancel
   - Complexity: ~10 (OrderState checks + spec creation + conditional logic)

### Proposed Extraction

Extract **3 helper methods** to reduce complexity:

1. **ShouldCancelTarget** - Pure decision function
2. **IsOrderCancellable** - Pure state check function
3. **CreateFollowerTargetReplaceSpec** - Spec builder function

---

## Method Signatures

### Original Method (Unchanged)

```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
```

**Complexity After Extraction**: ~7-8
- Early returns: 3 paths
- TryGetValue checks: 2 paths
- Helper calls: 3 (no nested conditionals)

### Helper Method 1: ShouldCancelTarget

```csharp
private static bool ShouldCancelTarget(bool isFilled, bool isRunner, int qty)
```

**Purpose**: Consolidates the cancellation decision logic  
**Complexity**: 2 (single compound conditional)  
**Returns**: true if target should be cancelled  
**Logic**: isFilled OR isRunner OR qty <= 0

**Rationale**: 
- Pure function (no side effects)
- Single responsibility (decision only)
- Testable in isolation
- Jane Street principle: Make illegal states unrepresentable - encapsulates invalid target states

### Helper Method 2: IsOrderCancellable

```csharp
private static bool IsOrderCancellable(Order order)
```

**Purpose**: Consolidates OrderState validation logic  
**Complexity**: 2 (compound conditional with 4 OR clauses)  
**Returns**: true if order can be cancelled  
**Logic**: OrderState == Working OR Accepted OR Submitted OR ChangePending

**Rationale**:
- Eliminates code duplication (appears twice in original method)
- Pure function (no side effects)
- Single responsibility (state validation)
- Testable in isolation
- Cognitive simplicity: Is this order cancellable vs nested conditionals

### Helper Method 3: CreateFollowerTargetReplaceSpec

```csharp
private FollowerTargetReplaceSpec CreateFollowerTargetReplaceSpec(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    int qty,
    string targetTag,
    Order oldTarget
)
```

**Purpose**: Builds the FollowerTargetReplaceSpec object  
**Complexity**: 4 (conditional logic for price validation and exit action)  
**Returns**: FollowerTargetReplaceSpec or null if invalid price

**Rationale**:
- Encapsulates spec creation logic
- Single responsibility (object construction)
- Testable in isolation
- Reduces main method complexity by extracting 15+ lines
- Jane Street principle: Separate data construction from control flow

---

## Call Graph

```
SymmetryGuardReplaceExistingFollowerTarget (Main Method)
│
├─> IsRunnerTarget(targetNumber)                    [Existing helper]
├─> IsTargetFilled(pos, targetNumber)               [Existing helper]
├─> GetTargetContracts(pos, targetNumber)           [Existing helper]
│
├─> ShouldCancelTarget(isFilled, isRunner, qty)     [NEW - Helper 1]
│   └─> Returns: bool
│
├─> IsOrderCancellable(staleTarget)                 [NEW - Helper 2]
│   └─> Returns: bool
│
├─> IsOrderCancellable(oldTarget)                   [NEW - Helper 2]
│   └─> Returns: bool
│
├─> CreateFollowerTargetReplaceSpec(...)            [NEW - Helper 3]
│   ├─> GetTargetPrice(pos, targetNumber)           [Existing helper]
│   ├─> SymmetryTrim(...)                           [Existing helper]
│   └─> Returns: FollowerTargetReplaceSpec or null
│
└─> StampReaperMoveGrace()                          [Existing helper]
```

### Data Flow

1. **Input**: fleetEntryName, pos, targetNumber, dict
2. **Compute State**: isRunner, isFilled, qty (via existing helpers)
3. **Decision 1**: ShouldCancelTarget(isFilled, isRunner, qty)
   - If true → Cancel path (check IsOrderCancellable(staleTarget))
   - If false → Continue to replacement logic
4. **Decision 2**: Check if oldTarget exists in dict
   - If false → Early return
   - If true → Continue to replacement logic
5. **Decision 3**: IsOrderCancellable(oldTarget)
   - If true → Create spec via CreateFollowerTargetReplaceSpec(...)
   - If false → No action (implicit return)
6. **Action**: If spec created → Store in _followerTargetReplaceSpecs, stamp grace, cancel

### Shared State

**Read-Only Access**:
- pos.ExecutingAccount (Account reference)
- pos.Direction (MarketPosition enum)
- Instrument.MasterInstrument (Instrument reference)

**Mutated State** (FSM/Actor Pattern Compliant):
- dict (ConcurrentDictionary) - Thread-safe operations only (TryGetValue, TryRemove)
- _followerTargetReplaceSpecs (ConcurrentDictionary) - Thread-safe write
- pos.ExecutingAccount.Cancel(...) - Enqueues cancel action (Actor pattern)

**No Locks**: ✅ All state mutations use lock-free primitives or Actor Enqueue pattern

---

## Lock-Free Validation

### Current Implementation Analysis

✅ **PASS**: No lock() statements in method  
✅ **PASS**: Uses ConcurrentDictionary for thread-safe dict operations  
✅ **PASS**: Uses pos.ExecutingAccount.Cancel(...) (Actor Enqueue pattern)  
✅ **PASS**: Uses _followerTargetReplaceSpecs (ConcurrentDictionary)  
✅ **PASS**: FSM two-phase pattern (Phase 1: Cancel, Phase 2: Submit via event)

### Post-Extraction Validation

**Helper Method 1** (ShouldCancelTarget):
- ✅ Pure function (no state access)
- ✅ No locks required

**Helper Method 2** (IsOrderCancellable):
- ✅ Pure function (reads immutable Order.OrderState)
- ✅ No locks required

**Helper Method 3** (CreateFollowerTargetReplaceSpec):
- ✅ Reads immutable properties (pos.Direction, Instrument.MasterInstrument)
- ✅ Calls existing helpers (already lock-free)
- ✅ Returns new object (no shared state mutation)
- ✅ No locks required

**Main Method** (after extraction):
- ✅ Maintains ConcurrentDictionary usage
- ✅ Maintains Actor Enqueue pattern for Cancel
- ✅ No new locks introduced
- ✅ FSM pattern preserved

---

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)

**Before Extraction**:
- Main method: CYC 18 ❌ (exceeds threshold)

**After Extraction**:
- Main method: CYC ~7-8 ✅ (within threshold)
- Helper 1: CYC 2 ✅
- Helper 2: CYC 2 ✅
- Helper 3: CYC 4 ✅

**Total Complexity**: 15 (distributed across 4 methods)  
**Max Per Method**: 8 ✅

### HFT Microsecond-Latency Requirements

**Performance Considerations**:
1. **No New Allocations**: Helper methods are static where possible (no closure allocations)
2. **Inline Candidates**: All helpers are small (<20 LOC) and likely to be inlined by JIT
3. **No Virtual Calls**: All helpers are private (direct calls, no vtable lookup)
4. **Cache-Friendly**: Reduced branching in main method improves CPU branch prediction
5. **Lock-Free**: No contention delays (maintains existing Actor pattern)

**Latency Impact**: Negligible (likely <1 microsecond overhead from extraction)

### Testability

**Before Extraction**:
- Main method: Hard to test (76 LOC, 18 branches, requires full setup)

**After Extraction**:
- Helper 1: Easy to test (pure function, 2 branches)
- Helper 2: Easy to test (pure function, 4 branches)
- Helper 3: Moderate to test (requires Instrument mock, 4 branches)
- Main method: Easier to test (reduced branches, can mock helpers)

**Test Coverage Target**: 100% branch coverage for all extracted methods

### Make Illegal States Unrepresentable

**Enforcement**:
1. ShouldCancelTarget - Encapsulates invalid target states (filled/runner/zero qty)
2. IsOrderCancellable - Encapsulates valid cancellable states (Working/Accepted/Submitted/ChangePending)
3. CreateFollowerTargetReplaceSpec - Returns null for invalid price (≤0), preventing illegal spec creation

**Result**: Impossible to cancel non-cancellable orders or create invalid specs (compiler-enforced via null checks)

---

## Complexity Reduction Path

### Step-by-Step Transformation

**Step 1**: Extract ShouldCancelTarget
- **Before**: CYC 18
- **After**: CYC 16 (removes 1 compound conditional)
- **Verification**: Run existing tests, verify no behavior change

**Step 2**: Extract IsOrderCancellable
- **Before**: CYC 16
- **After**: CYC 12 (removes 2 compound conditionals with 4 OR clauses each)
- **Verification**: Run existing tests, verify no behavior change

**Step 3**: Extract CreateFollowerTargetReplaceSpec
- **Before**: CYC 12
- **After**: CYC 7-8 (removes nested conditional + spec creation logic)
- **Verification**: Run existing tests, verify no behavior change

**Final State**:
- Main method: CYC 7-8 ✅
- Helper 1: CYC 2 ✅
- Helper 2: CYC 2 ✅
- Helper 3: CYC 4 ✅

---

## Risk Assessment

### Regression Risk: LOW

**Mitigation**:
1. Existing tests provide safety net (FSMActorTests.cs)
2. Extraction preserves exact behavior (no semantic changes)
3. Step-by-step verification after each extraction
4. CSharpier formatting ensures no whitespace mutations

### Performance Risk: NONE

**Rationale**:
1. No new allocations (static helpers where possible)
2. Likely JIT inlining (small methods <20 LOC)
3. No virtual calls (private methods)
4. Maintains lock-free Actor pattern

### Integration Risk: NONE

**Rationale**:
1. Method signature unchanged (no caller impact)
2. No changes to callees (extracted helpers are private)
3. Single-file refactoring (no cross-file dependencies)

### Scope Creep Risk: LOW

**Enforcement**:
1. V12.23 Protocol compliance (single method, single file)
2. No while-we-are-here improvements
3. No fixing pre-existing issues in other methods
4. Clear boundary: Only SymmetryGuardReplaceExistingFollowerTarget modified

---

## Implementation Checklist

### Pre-Implementation

- [ ] Review architecture plan with Director (Human approval required)
- [ ] Verify existing tests pass (dotnet test)
- [ ] Run complexity audit (python scripts/complexity_audit.py)
- [ ] Check CodeScene hotspot status (VS Code extension)

### Implementation (Step-by-Step)

- [ ] **Step 1**: Extract ShouldCancelTarget
  - [ ] Create helper method
  - [ ] Replace inline logic with helper call
  - [ ] Run tests (dotnet test)
  - [ ] Verify complexity reduced (CYC 18 → 16)
  
- [ ] **Step 2**: Extract IsOrderCancellable
  - [ ] Create helper method
  - [ ] Replace both inline checks with helper calls
  - [ ] Run tests (dotnet test)
  - [ ] Verify complexity reduced (CYC 16 → 12)
  
- [ ] **Step 3**: Extract CreateFollowerTargetReplaceSpec
  - [ ] Create helper method
  - [ ] Replace inline spec creation with helper call
  - [ ] Handle null return case
  - [ ] Run tests (dotnet test)
  - [ ] Verify complexity reduced (CYC 12 → 7-8)

### Post-Implementation

- [ ] Run CSharpier formatting (dotnet csharpier format src/)
- [ ] Run complexity audit (python scripts/complexity_audit.py)
- [ ] Verify all tests pass (dotnet test)
- [ ] Run pre-push validation (powershell -File .\scripts\pre_push_validation.ps1)
- [ ] Sync NinjaTrader hard links (powershell -File .\deploy-sync.ps1)
- [ ] Verify in NinjaTrader (F5 compile + runtime test)
- [ ] Update EPIC-CCN-001 manifest with completion status

### Test Coverage (New Tests Required)

- [ ] Unit test: ShouldCancelTarget (all branches)
- [ ] Unit test: IsOrderCancellable (all OrderState values)
- [ ] Unit test: CreateFollowerTargetReplaceSpec (valid/invalid price, Long/Short)
- [ ] Integration test: Main method with mocked helpers (verify call sequence)

---

## Success Criteria

### Functional Requirements

✅ Method signature unchanged (no caller impact)  
✅ Exact behavior preserved (no semantic changes)  
✅ All existing tests pass  
✅ Lock-free Actor pattern maintained  
✅ FSM two-phase pattern preserved

### Quality Requirements

✅ Main method complexity: ≤8 (Jane Street strict)  
✅ Helper method complexity: ≤5 each  
✅ No new locks introduced  
✅ No new allocations in hot path  
✅ CSharpier formatting applied

### Process Requirements

✅ V12.23 Protocol compliance (single method, single file)  
✅ Pre-push validation passes (all 13 checks)  
✅ NinjaTrader hard links synced  
✅ CodeScene hotspot status improved  
✅ Test coverage: 100% branch coverage for helpers

---

## Approval Gate

**Status**: ⏳ PENDING DIRECTOR APPROVAL

**Required Actions**:
1. Director reviews architecture plan
2. Director approves extraction strategy
3. Director confirms Jane Street alignment
4. Director authorizes Phase 3 (Implementation)

**Approval Authority**: Director (Human)  
**Next Phase**: Phase 3 - Implementation (Bob CLI v12-engineer mode)

---

**Document Version**: 1.0  
**Created**: 2026-06-15  
**Author**: Bob Shell (Plan Mode)  
**Epic**: EPIC-CCN-001  
**Protocol**: V12.23 (Boundary Validation)
