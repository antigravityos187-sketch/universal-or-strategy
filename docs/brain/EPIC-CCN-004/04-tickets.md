# Extraction Tickets: EPIC-CCN-004

## Overview
- **Epic**: EPIC-CCN-004
- **Method**: HandleFleetTargetFill
- **File**: src/V12_002.UI.Compliance.cs
- **Current Complexity**: 16 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 8-12 hours (2 days)

## Complexity Reduction Strategy
- **Original CYC**: 16
- **Target CYC**: 6-7
- **Reduction**: 57%
- **Approach**: Extract 3 helper methods + refactor main method

---

## TICKET-1: Extract ValidateFleetTarget (Pure Function)

### Scope
- **Current Method**: `HandleFleetTargetFill`
- **Current CYC**: 16
- **Target Helper CYC**: 3-4
- **Extraction**: Target key parsing and position lookup logic

### Method Signature
```csharp
private (PositionInfo position, int targetNum, string targetKey)? ValidateFleetTarget(
    string ocoName,
    Dictionary<string, PositionInfo> activePositions)
```

### Implementation Steps
1. Create test file: `tests/V12_Performance.Tests/UI/FleetTargetFillTests.cs`
2. Write TDD tests for ValidateFleetTarget:
   - Test null return for invalid OCO name format
   - Test null return for missing position in activePositions
   - Test valid tuple return for successful validation
   - Test edge cases (empty strings, malformed keys)
3. Extract lines 3-16 from HandleFleetTargetFill into new private method
4. Replace extracted code with single method call
5. Verify ASCII-only compliance in all string literals
6. Run complexity audit: `python scripts/complexity_audit.py`
7. Run CSharpier formatter: `dotnet csharpier format src/`
8. Run build: `powershell -File .\scripts\build_readiness.ps1`

### Acceptance Criteria
- [ ] ValidateFleetTarget method created with CYC ≤4
- [ ] Method is pure function (no side effects)
- [ ] Returns nullable tuple: `(PositionInfo, int, string)?`
- [ ] All TDD tests pass (100% coverage)
- [ ] No Unicode characters in string literals
- [ ] HandleFleetTargetFill complexity reduced by 2-3 points
- [ ] Build succeeds with zero errors
- [ ] No behavioral changes (integration test passes)

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/

# Build check
powershell -File .\scripts\build_readiness.ps1

# Test check
dotnet test tests/V12_Performance.Tests/
```

---

## TICKET-2: Extract ProcessFleetFillResult (Logging/Guard Handler)

### Scope
- **Current Method**: `HandleFleetTargetFill`
- **Current CYC**: ~13-14 (after TICKET-1)
- **Target Helper CYC**: 2-3
- **Extraction**: Duplicate guard and success logging logic

### Method Signature
```csharp
private bool ProcessFleetFillResult(
    int targetNum,
    string targetKey,
    bool alreadyProcessed,
    int applied,
    int remaining,
    double price)
```

### Implementation Steps
1. Write TDD tests for ProcessFleetFillResult:
   - Test duplicate guard path (alreadyProcessed=true, returns false)
   - Test success logging path (alreadyProcessed=false, returns true)
   - Test logging output format
   - Verify no state mutation
2. Extract lines 32-42 from HandleFleetTargetFill into new private method
3. Replace extracted code with single method call
4. Verify ASCII-only compliance in format strings
5. Run complexity audit: `python scripts/complexity_audit.py`
6. Run CSharpier formatter: `dotnet csharpier format src/`
7. Run build: `powershell -File .\scripts\build_readiness.ps1`

### Acceptance Criteria
- [ ] ProcessFleetFillResult method created with CYC ≤3
- [ ] Method returns boolean decision for next step
- [ ] No state mutation (Print is logging only)
- [ ] All TDD tests pass (100% coverage)
- [ ] No Unicode characters in format strings
- [ ] HandleFleetTargetFill complexity reduced by 2-3 points
- [ ] Build succeeds with zero errors
- [ ] No behavioral changes (integration test passes)

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/

# Build check
powershell -File .\scripts\build_readiness.ps1

# Test check
dotnet test tests/V12_Performance.Tests/
```

---

## TICKET-3: Extract CancelRelatedStopOrders (State Transition)

### Scope
- **Current Method**: `HandleFleetTargetFill`
- **Current CYC**: ~10-11 (after TICKET-2)
- **Target Helper CYC**: 3-4
- **Extraction**: Stop order cancellation loop

### Method Signature
```csharp
private void CancelRelatedStopOrders(Account ocoAcct)
```

### Implementation Steps
1. Write TDD tests for CancelRelatedStopOrders:
   - Mock Actor calls to CancelOrderOnAccount
   - Test iteration over ocoAcct.Orders
   - Verify defensive copy pattern (ToArray())
   - Test stop order filtering logic
   - Verify no new synchronization primitives
2. Extract lines 43-60 from HandleFleetTargetFill into new private method
3. Replace extracted code with single method call
4. Verify lock-free compliance (no lock() statements)
5. Run complexity audit: `python scripts/complexity_audit.py`
6. Run forensic scan: `grep -r "lock(" src/`
7. Run CSharpier formatter: `dotnet csharpier format src/`
8. Run build: `powershell -File .\scripts\build_readiness.ps1`

### Acceptance Criteria
- [ ] CancelRelatedStopOrders method created with CYC ≤4
- [ ] Uses existing Actor method (CancelOrderOnAccount)
- [ ] No new synchronization primitives
- [ ] Defensive copy pattern preserved (ToArray())
- [ ] All TDD tests pass (100% coverage)
- [ ] Zero lock() statements in grep scan
- [ ] HandleFleetTargetFill complexity reduced by 3-4 points
- [ ] Build succeeds with zero errors
- [ ] No behavioral changes (integration test passes)

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/

# Format check
dotnet csharpier check src/

# Build check
powershell -File .\scripts\build_readiness.ps1

# Test check
dotnet test tests/V12_Performance.Tests/
```

---

## TICKET-4: Refactor Main Method (Final Integration)

### Scope
- **Current Method**: `HandleFleetTargetFill`
- **Current CYC**: ~7-8 (after TICKET-3)
- **Target CYC**: 6-7
- **Refactoring**: Simplify main method to use extracted helpers

### Implementation Steps
1. Refactor HandleFleetTargetFill to sequential flow:
   - Step 1: Call ValidateFleetTarget (early return if null)
   - Step 2: Call ApplyTargetFill (existing method)
   - Step 3: Call ProcessFleetFillResult (get decision)
   - Step 4: Conditionally call CancelRelatedStopOrders
2. Remove all extracted code from main method
3. Verify linear flow (4 sequential steps)
4. Write integration test for full HandleFleetTargetFill flow
5. Run complexity audit: `python scripts/complexity_audit.py`
6. Run CSharpier formatter: `dotnet csharpier format src/`
7. Run build: `powershell -File .\scripts\build_readiness.ps1`
8. Run hard-link sync: `powershell -File .\deploy-sync.ps1`

### Acceptance Criteria
- [ ] HandleFleetTargetFill reduced to CYC ≤7
- [ ] Main method is linear (4 sequential steps)
- [ ] All helper methods integrated correctly
- [ ] Integration test passes (full flow)
- [ ] Complexity audit shows CYC ≤8 for all methods
- [ ] Build succeeds with zero errors
- [ ] Hard-link integrity verified (deploy-sync.ps1)
- [ ] No behavioral changes (all tests pass)

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first
- **TICKET-3** must be completed first

### Verification Commands
```bash
# Complexity check (final verification)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/

# Build check
powershell -File .\scripts\build_readiness.ps1

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Full test suite
dotnet test tests/V12_Performance.Tests/

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Final Verification Checklist

### Complexity Targets (Jane Street Aligned)
- [ ] HandleFleetTargetFill: CYC ≤7 (target: 6-7)
- [ ] ValidateFleetTarget: CYC ≤4 (target: 3-4)
- [ ] ProcessFleetFillResult: CYC ≤3 (target: 2-3)
- [ ] CancelRelatedStopOrders: CYC ≤4 (target: 3-4)

### DNA Compliance
- [ ] Zero lock() statements (forensic scan)
- [ ] ASCII-only compliance (all string literals)
- [ ] FSM/Actor pattern preserved
- [ ] No new synchronization primitives

### PR Hygiene
- [ ] Diff size <10k characters
- [ ] Single method scope (no scope creep)
- [ ] No whitespace mutations
- [ ] Build succeeds with zero errors

### Test Coverage
- [ ] ValidateFleetTarget: 100% coverage
- [ ] ProcessFleetFillResult: 100% coverage
- [ ] CancelRelatedStopOrders: 100% coverage
- [ ] Integration test: Full flow coverage

### Jane Street Alignment
- [ ] Cognitive simplicity (CYC ≤8)
- [ ] Pure functions (ValidateFleetTarget)
- [ ] Single responsibility (each helper)
- [ ] Linear flow (main method)
- [ ] Microsecond latency preserved

---

## Execution Timeline

### Day 1 (4-6 hours)
- **Morning**: TICKET-1 (ValidateFleetTarget extraction)
  - Create test file
  - Write TDD tests
  - Extract pure function
  - Verify complexity reduction
- **Afternoon**: TICKET-2 (ProcessFleetFillResult extraction)
  - Write TDD tests
  - Extract logging/guard logic
  - Verify complexity reduction

### Day 2 (4-6 hours)
- **Morning**: TICKET-3 (CancelRelatedStopOrders extraction)
  - Write TDD tests with mocks
  - Extract Actor integration
  - Verify lock-free compliance
- **Afternoon**: TICKET-4 (Main method refactoring)
  - Integrate all helpers
  - Write integration test
  - Run full verification suite
  - Deploy hard-link sync

---

## Risk Mitigation

### Rollback Plan
- Git revert after each ticket if complexity audit fails
- Checkpoint commits after each helper extraction
- Integration test guards against behavioral changes

### Incremental Testing
- Test each helper in isolation (TDD approach)
- Mock Actor calls for CancelRelatedStopOrders
- Integration test for full flow

### Verification Gates
- Complexity audit after each ticket
- Build check after each ticket
- Lock-free scan after TICKET-3
- Hard-link sync after TICKET-4

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC 16
- **After**: CYC 6-7
- **Reduction**: 57%
- **Target Met**: YES (≤8)

### Code Quality
- **Test Coverage**: 100% for helpers
- **Lock-Free**: VERIFIED
- **ASCII-Only**: VERIFIED
- **Jane Street Aligned**: VERIFIED

### PR Hygiene
- **Diff Size**: ~800 characters (PASS)
- **Scope Creep**: None (PASS)
- **Build Status**: Zero errors (PASS)

---

**Phase 4 Status**: COMPLETE
**Ticket Count**: 4
**Estimated Effort**: 8-12 hours (2 days)
**Next Phase**: Phase 5 (Ticket Execution)
