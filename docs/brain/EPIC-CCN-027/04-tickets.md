# Extraction Tickets: EPIC-CCN-027

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 6-8 hours
- **Strategy**: TDD cycle (Red → Green → Refactor) per ticket
- **Target**: CYC ≤8 per method (Jane Street strict standard)

---

## TICKET-1: Extract CreateBracketOrders (Pure Function)

### Scope
- **Current Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Current CYC**: 21
- **Target CYC**: ≤8 (helper method)
- **Extraction**: Lines 606-710 (Order creation and validation logic)
- **Method Type**: Pure function (no side effects, deterministic)

### Implementation

#### Phase 1: Red (Write Failing Tests)
1. Create test file: `tests/V12_Performance.Tests/Core/SIMADispatchTests.cs`
2. Write 6 test cases:
   - `CreateBracketOrders_ValidInputs_ReturnsCompleteOrderSet`
   - `CreateBracketOrders_InvalidTargetPrice_SkipsTarget`
   - `CreateBracketOrders_InvalidTargetQuantity_SkipsTarget`
   - `CreateBracketOrders_RunnerTarget_ExcludesFromTargets`
   - `CreateBracketOrders_MultipleTargets_AssignsCorrectOCOGroups`
   - `CreateBracketOrders_ZeroDispatchTargetCount_ReturnsEmptyTargets`
3. Run tests → Verify RED (method not implemented)

#### Phase 2: Green (Extract Method)
1. Extract lines 606-710 into private method `CreateBracketOrders()`
2. Define return type: `BracketOrderSet` struct with fields:
   - `Order Entry`
   - `Order Stop`
   - `List<Order> Targets`
   - `int NonRunnerLimitQty`
   - `int RunnerQty`
3. Pass 16 parameters from orchestrator (preserve original signature)
4. Return structured order set
5. Update orchestrator to call `CreateBracketOrders()` and destructure result
6. Run tests → Verify GREEN (all 6 tests pass)

#### Phase 3: Refactor (Optimize)
1. Run `python scripts/complexity_audit.py` → Verify CYC ≤8
2. Run `dotnet csharpier format src/` → Format code
3. Run `dotnet build` → Verify zero errors
4. Run `dotnet test` → Verify 100% pass
5. Code review: Verify pure function (no side effects)

### Acceptance Criteria
- [ ] Test file created with 6 test cases
- [ ] All tests RED before extraction
- [ ] Method extracted with CYC ≤8
- [ ] All tests GREEN after extraction
- [ ] Pure function verified (no side effects)
- [ ] Build succeeds (zero errors)
- [ ] Formatting applied (CSharpier)
- [ ] Complexity audit PASS (CYC ≤8)

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Build check
dotnet build

# Test check
dotnet test --filter "FullyQualifiedName~SIMADispatchTests"

# Format check
dotnet csharpier check src/V12_002.SIMA.Dispatch.cs
```

---

## TICKET-2: Extract RegisterBracketState (State Registration)

### Scope
- **Current Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Current CYC**: 21 → ~13 (after TICKET-1)
- **Target CYC**: ≤8 (helper method)
- **Extraction**: Lines 712-760 (Dictionary registration and FSM creation)
- **Method Type**: Controlled side effects (atomic writes only)

### Implementation

#### Phase 1: Red (Write Failing Tests)
1. Add 4 test cases to `SIMADispatchTests.cs`:
   - `RegisterBracketState_ValidOrders_RegistersInAllDictionaries`
   - `RegisterBracketState_NewBracket_CreatesFSMWithPendingSubmitState`
   - `RegisterBracketState_DuplicateCall_IdempotentBehavior` (TryAdd)
   - `RegisterBracketState_Success_SetsSyncPendingFlag`
2. Run tests → Verify RED (method not implemented)

#### Phase 2: Green (Extract Method)
1. Extract lines 712-760 into private method `RegisterBracketState()`
2. Method signature:
   - Parameters: `BracketOrderSet orders`, `string bracketId`, `int dispatchTargetCount`, plus shared state references
   - Return: `void` (side effects via ConcurrentDictionary)
3. Preserve exact ordering:
   - Dictionary registration BEFORE `AddExpectedPositionDeltaLocked`
   - FSM creation with `TryAdd` (atomic, idempotent)
4. Update orchestrator to call `RegisterBracketState()` after `CreateBracketOrders()`
5. Run tests → Verify GREEN (all 4 tests pass)

#### Phase 3: Refactor (Optimize)
1. Run `python scripts/complexity_audit.py` → Verify CYC ≤8
2. Verify lock-free compliance:
   - `grep -n "lock(" src/V12_002.SIMA.Dispatch.cs` → Zero matches
   - All writes via `ConcurrentDictionary.TryAdd()` or indexer
3. Run `dotnet build` → Verify zero errors
4. Run `dotnet test` → Verify 100% pass
5. Code review: Verify FSM ordering invariant preserved

### Acceptance Criteria
- [ ] 4 test cases added (total 10 tests)
- [ ] All new tests RED before extraction
- [ ] Method extracted with CYC ≤8
- [ ] All tests GREEN after extraction
- [ ] Lock-free verified (zero lock() statements)
- [ ] FSM ordering invariant preserved
- [ ] Build succeeds (zero errors)
- [ ] Complexity audit PASS (CYC ≤8)

### Dependencies
- **TICKET-1** must be completed first (requires `BracketOrderSet` struct)

### Verification Commands
```bash
# Lock-free check
grep -n "lock(" src/V12_002.SIMA.Dispatch.cs

# Complexity check
python scripts/complexity_audit.py

# Build + test
dotnet build && dotnet test --filter "FullyQualifiedName~SIMADispatchTests"
```

---

## TICKET-3: Extract DispatchToPhotonKernel (Zero-Allocation Dispatch)

### Scope
- **Current Method**: `Dispatch_PublishMarketBracketToPhoton`
- **Current CYC**: ~13 → ≤8 (after TICKET-2)
- **Target CYC**: ≤8 (helper method + orchestrator)
- **Extraction**: Lines 762-795 (PhotonPool claim and kernel enqueue)
- **Method Type**: Controlled side effects (lock-free enqueue)

### Implementation

#### Phase 1: Red (Write Failing Tests)
1. Add 5 test cases to `SIMADispatchTests.cs`:
   - `DispatchToPhotonKernel_PoolAvailable_ClaimsSlotSuccessfully`
   - `DispatchToPhotonKernel_ValidOrders_PopulatesProxyArray`
   - `DispatchToPhotonKernel_Success_BuildsFleetDispatchSlot`
   - `DispatchToPhotonKernel_Success_ComputesShadowHash`
   - `DispatchToPhotonKernel_Success_EnqueuesToKernel`
2. Add 1 stress test:
   - `DispatchToPhotonKernel_PoolExhausted_FallsBackToHeapAllocation`
3. Run tests → Verify RED (method not implemented)

#### Phase 2: Green (Extract Method)
1. Extract lines 762-795 into private method `DispatchToPhotonKernel()`
2. Method signature:
   - Parameters: `BracketOrderSet orders`, `string bracketId`, `int dispatchTargetCount`, plus PhotonPool reference
   - Return: `void` (side effects via kernel enqueue)
3. Preserve zero-allocation pattern:
   - PhotonPool.Claim() for slot reuse
   - Fallback to heap allocation if pool exhausted
4. Update orchestrator to call `DispatchToPhotonKernel()` after `RegisterBracketState()`
5. Run tests → Verify GREEN (all 6 tests pass)

#### Phase 3: Refactor (Final Orchestrator Optimization)
1. Verify orchestrator complexity:
   - Run `python scripts/complexity_audit.py` → Verify CYC ≤8 for orchestrator
   - Orchestrator should now be 3 sequential helper calls + minimal branching
2. Run full test suite:
   - `dotnet test` → Verify all 16 tests pass (6 + 4 + 6)
3. Run integration tests:
   - Add 3 integration tests for end-to-end flow
   - Verify FSM state transitions (PendingSubmit → Submitted)
   - Verify REAPER cleanup with multiple bracket lifecycles
4. Run `dotnet csharpier format src/` → Format code
5. Run `dotnet build` → Verify zero errors
6. Run `powershell -File .\deploy-sync.ps1` → Sync hard links

### Acceptance Criteria
- [ ] 6 test cases added (total 16 unit tests)
- [ ] All new tests RED before extraction
- [ ] Method extracted with CYC ≤8
- [ ] All tests GREEN after extraction
- [ ] Orchestrator complexity ≤8 (final verification)
- [ ] Zero-allocation pattern preserved
- [ ] PhotonPool fallback logic intact
- [ ] 3 integration tests added and passing
- [ ] Build succeeds (zero errors)
- [ ] Hard-link sync succeeds (deploy-sync.ps1)
- [ ] Complexity audit PASS (CYC ≤8 for all methods)

### Dependencies
- **TICKET-1** must be completed first (requires `BracketOrderSet` struct)
- **TICKET-2** must be completed first (requires state registration)

### Verification Commands
```bash
# Full complexity audit
python scripts/complexity_audit.py

# Full test suite
dotnet test --filter "FullyQualifiedName~SIMADispatchTests"

# Build + sync
dotnet build && powershell -File .\deploy-sync.ps1

# Format check
dotnet csharpier check src/
```

---

## Post-Extraction Validation

### Final Checklist
- [ ] All 16 unit tests passing
- [ ] All 3 integration tests passing
- [ ] Orchestrator CYC ≤8
- [ ] All helper methods CYC ≤8
- [ ] Zero lock() statements (lock-free validation)
- [ ] Build succeeds (zero errors)
- [ ] Hard-link sync succeeds
- [ ] Formatting applied (CSharpier)
- [ ] Complexity audit PASS

### Arena AI Audit (P4 Gate)
- [ ] Submit for adversarial audit
- [ ] Address any findings
- [ ] Obtain PASS verdict

### PR Submission
- [ ] Create PR with title: "EPIC-CCN-027: Extract Dispatch_PublishMarketBracketToPhoton (CYC 21→8)"
- [ ] Link architecture plan in PR description
- [ ] Verify diff <10k chars (surgical change)
- [ ] Request code review
- [ ] Merge after approval

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC 21 (189 LOC, single method)
- **After**: CYC ≤8 (orchestrator) + 3 helpers with CYC ≤8 each
- **Reduction**: 62% complexity reduction in orchestrator

### Test Coverage
- **Unit Tests**: 16 (6 + 4 + 6)
- **Integration Tests**: 3
- **Total**: 19 test cases
- **Coverage Target**: 100% for extracted methods

### Jane Street Alignment
- ✅ Cognitive simplicity (CYC ≤8)
- ✅ Pure function extraction (CreateBracketOrders)
- ✅ Microsecond latency optimization (zero-allocation dispatch)
- ✅ Lock-free compliance (zero lock() statements)

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-027
**Phase**: 4 (Ticket Generation)
**Total Tickets**: 3
**Estimated Effort**: 6-8 hours
**Next Phase**: Phase 5 (Ticket Execution via TDD)
