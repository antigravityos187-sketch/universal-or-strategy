# Phase 1: Scope Definition - EPIC-CCN-110

## Target Method
- **Method**: `AdoptMasterWorkingOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 588-653 (approximately 65 lines)
- **Current Cyclomatic Complexity**: 19
- **Target Complexity**: ≤15 (Jane Street alignment)

## Method Purpose
Adopts working orders from the master account into tracking dictionaries after strategy restart or reconnect. This is Phase 2 of the order hydration process, following fleet account adoption.

## Complexity Analysis

### Current Complexity Sources (CYC=19)
1. Try-catch wrapper (+1)
2. Foreach order iteration (+1)
3. Instrument null/name checks (+2)
4. IsOrderStateAdoptable validation (+1)
5. Order name null coalescing (+1)
6. ClassifyMasterOrderByPrefix call (+1)
7. TargetDict null check (+1)
8. Key null check (+1)
9. Dictionary insertion (+1)
10. Print statement formatting (+1)
11. Exception catch block (+1)
12. Exception message formatting (+1)

### Dependencies
- **Called By**: `HydrateWorkingOrdersFromBroker` (line 314)
- **Calls**:
  - `IsOrderStateAdoptable` (validation helper)
  - `ClassifyMasterOrderByPrefix` (already extracted, lines 63-106)
- **Accesses**:
  - `Account.Orders` (broker state)
  - Tracking dictionaries (stopOrders, target1-5Orders)

## Extraction Strategy

### What to Extract (4 methods, CYC ~5 each)

#### 1. ValidateMasterOrderForAdoption (CYC ~5)
**Purpose**: Validate if order qualifies for adoption
**Logic**:
- Instrument match check
- IsOrderStateAdoptable call
- Return bool

**Rationale**: Isolates validation logic, reduces nesting

#### 2. AdoptSingleMasterOrder (CYC ~6)
**Purpose**: Adopt a single validated order into tracking dict
**Logic**:
- Call ClassifyMasterOrderByPrefix
- Null checks for targetDict/key
- Dictionary insertion
- Diagnostic logging
- Return adoption success bool

**Rationale**: Single responsibility - one order adoption

#### 3. LogMasterOrderAdoption (CYC ~3)
**Purpose**: Centralized adoption logging
**Logic**:
- Format diagnostic message
- Print to console

**Rationale**: Separates I/O from business logic

#### 4. HandleMasterAdoptionError (CYC ~3)
**Purpose**: Centralized error handling
**Logic**:
- Format error message
- Print warning

**Rationale**: Separates error handling from main flow

### What to Keep in AdoptMasterWorkingOrders (CYC ~8)
- Try-catch wrapper (required for broker API safety)
- Foreach loop over Account.Orders
- Calls to extracted validation/adoption methods
- adoptedCount increment
- High-level orchestration

## Boundary Definition (V12.23 No Scope Creep Protocol)

### IN SCOPE
✅ AdoptMasterWorkingOrders method only (lines 588-653)
✅ Extract 4 helper methods from this method
✅ Maintain exact same behavior (no logic changes)
✅ Preserve all diagnostic logging
✅ Keep exception handling semantics

### OUT OF SCOPE
❌ ClassifyMasterOrderByPrefix (already extracted)
❌ IsOrderStateAdoptable (shared validation helper)
❌ AdoptFleetWorkingOrders (separate method)
❌ HydrateWorkingOrdersFromBroker (caller)
❌ Any other lifecycle methods
❌ Tracking dictionary structure changes
❌ Order state validation logic changes

## Success Criteria

### Complexity Targets
- **AdoptMasterWorkingOrders**: CYC ≤8 (from 19)
- **ValidateMasterOrderForAdoption**: CYC ≤5
- **AdoptSingleMasterOrder**: CYC ≤6
- **LogMasterOrderAdoption**: CYC ≤3
- **HandleMasterAdoptionError**: CYC ≤3
- **Total Reduction**: 11 points (19 → 8)

### Functional Requirements
✅ All orders adopted correctly (no behavior change)
✅ All diagnostic messages preserved
✅ Exception handling unchanged
✅ adoptedCount tracking accurate
✅ Dictionary insertion semantics identical

### Quality Gates
✅ Build passes (zero errors)
✅ CSharpier formatting compliant
✅ No new Roslyn warnings
✅ Complexity audit shows CYC ≤15 for all methods
✅ ASCII-only compliance maintained

### Testing Requirements
✅ Manual F5 test in NinjaTrader
✅ Verify order adoption after restart
✅ Verify diagnostic logging output
✅ Verify exception handling on broker errors

## Risk Assessment: MEDIUM

### Risks
1. **Order Adoption Logic**: Critical path for strategy restart - any bug leaves orders orphaned
2. **Dictionary Consistency**: Must maintain exact insertion semantics
3. **Logging Fidelity**: Diagnostic messages used for production debugging
4. **Exception Safety**: Broker API can throw - must preserve try-catch wrapper

### Mitigations
1. **Surgical Extraction**: Extract only validation/logging, keep orchestration in place
2. **Preserve Semantics**: No logic changes, pure refactoring
3. **Incremental Testing**: Test after each extraction
4. **Rollback Ready**: Git checkpoint before each change

### Blast Radius
- **Direct Impact**: Master account order hydration only
- **Indirect Impact**: REAPER audit (depends on complete adoption)
- **Failure Mode**: Orders not adopted → REAPER false alarms → manual intervention required
- **Recovery**: Restart strategy (re-runs hydration)

## V12 DNA Compliance

### Current State
❌ Complexity exceeds threshold (19 > 15)
✅ Lock-free (no synchronization primitives)
✅ ASCII-only (no Unicode in strings)
✅ Actor pattern (called from strategy thread)

### Target State
✅ All methods CYC ≤15
✅ Lock-free maintained
✅ ASCII-only maintained
✅ Actor pattern maintained
✅ Jane Street cognitive simplicity achieved

## Implementation Notes

### Extraction Order
1. **Step 1**: Extract LogMasterOrderAdoption (lowest risk)
2. **Step 2**: Extract HandleMasterAdoptionError (lowest risk)
3. **Step 3**: Extract ValidateMasterOrderForAdoption (medium risk)
4. **Step 4**: Extract AdoptSingleMasterOrder (highest risk - core logic)
5. **Step 5**: Verify complexity with `complexity_audit.py`

### Verification Commands
```powershell
# After each extraction
python scripts/complexity_audit.py
dotnet csharpier check src/
powershell -File .\scripts\build_readiness.ps1
```

## Next Phase
**Phase 2**: Create implementation plan with detailed extraction steps and TDD test cases.

## Metadata
- **Epic**: EPIC-CCN-110
- **Phase**: 1 (Scope Definition)
- **Status**: COMPLETE
- **Created**: 2026-06-13
- **Target Complexity**: ≤15 (Jane Street alignment)
- **Reduction Required**: 11 points (19 → 8)
