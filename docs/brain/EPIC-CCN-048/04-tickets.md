# Extraction Tickets: EPIC-CCN-048

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 3-4 hours
- **Target Method**: UpdateExistingPendingReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current Complexity**: 9
- **Target Complexity**: 5 (main method) + 1+2+1 (helpers) = 9 redistributed

---

## TICKET-1: Extract CreatePendingReplacement Helper

### Scope
- **Current Method**: `UpdateExistingPendingReplacement`
- **Current CYC**: 9
- **Target CYC**: N/A (pure function extraction)
- **Extraction**: Object construction logic into pure helper method

### Implementation
1. Create new private method `CreatePendingReplacement` with signature:
   ```csharp
   private PendingStopReplacement CreatePendingReplacement(
       string entryName,
       PositionInfo pos,
       Order currentStop,
       double validatedStopPrice,
       TargetInfo[] capturedTargets
   )
   ```

2. Move object construction logic from lines ~185-192 into helper:
   ```csharp
   return new PendingStopReplacement
   {
       EntryName = entryName,
       Quantity = pos.Quantity,
       StopPrice = validatedStopPrice,
       Direction = pos.MarketPosition,
       OldOrder = currentStop,
       CreatedTime = DateTime.UtcNow,
       CapturedTargets = capturedTargets,
       BracketRestorationNeeded = capturedTargets != null && capturedTargets.Length > 0
   };
   ```

3. Replace inline construction in main method with helper call:
   ```csharp
   var newPending = CreatePendingReplacement(entryName, pos, currentStop, validatedStopPrice, capturedTargets);
   ```

4. Run `dotnet build` to verify compilation
5. Run `dotnet test` to verify existing tests pass
6. Run `python scripts/complexity_audit.py` to verify helper CYC=1

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Helper complexity CYC=1 (no branching)
- [ ] Main method calls helper instead of inline construction
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] No behavioral changes (black-box identical)

### Dependencies
- None (first ticket)

### Test Coverage
- **New Unit Test**: `CreatePendingReplacement_ValidInputs_ReturnsCorrectStruct`
  - Verify all fields populated correctly
  - Verify BracketRestorationNeeded flag logic
  - Verify CreatedTime is set

---

## TICKET-2: Extract HandleCircuitBreakerCheck Helper

### Scope
- **Current Method**: `UpdateExistingPendingReplacement`
- **Current CYC**: 9 (after TICKET-1)
- **Target CYC**: N/A (side effect extraction)
- **Extraction**: Circuit breaker activation logic into isolated helper

### Implementation
1. Create new private method `HandleCircuitBreakerCheck` with signature:
   ```csharp
   private void HandleCircuitBreakerCheck(int currentCount)
   ```

2. Move circuit breaker logic from lines ~195-207 into helper:
   ```csharp
   if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
   {
       circuitBreakerActive = true;
       circuitBreakerActivatedTime = DateTime.UtcNow;
       Print($"[V12] Circuit breaker activated: {currentCount} pending replacements exceed threshold {CIRCUIT_BREAKER_THRESHOLD}");
   }
   ```

3. Replace inline logic in main method with helper call:
   ```csharp
   if (pendingStopReplacements.TryAdd(entryName, newPending))
   {
       int currentCount = Interlocked.Increment(ref pendingReplacementCount);
       HandleCircuitBreakerCheck(currentCount);
       // ... rest of logic
   }
   ```

4. Run `dotnet build` to verify compilation
5. Run `dotnet test` to verify existing tests pass
6. Run `python scripts/complexity_audit.py` to verify helper CYC=2

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Helper complexity CYC=2 (single compound conditional)
- [ ] Main method calls helper after Interlocked.Increment
- [ ] Circuit breaker activation logic preserved
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] No behavioral changes (black-box identical)

### Dependencies
- TICKET-1 must be completed first

### Test Coverage
- **New Unit Test 1**: `HandleCircuitBreakerCheck_BelowThreshold_NoActivation`
  - Verify circuit breaker not activated when count < threshold
  - Verify circuitBreakerActive remains false
  
- **New Unit Test 2**: `HandleCircuitBreakerCheck_AboveThreshold_ActivatesBreaker`
  - Verify circuit breaker activated when count >= threshold
  - Verify circuitBreakerActive set to true
  - Verify circuitBreakerActivatedTime set
  - Verify Print message called

---

## TICKET-3: Extract RefreshBracketTargetsIfNeeded Helper

### Scope
- **Current Method**: `UpdateExistingPendingReplacement`
- **Current CYC**: 9 (after TICKET-1 and TICKET-2)
- **Target CYC**: N/A (conditional update extraction)
- **Extraction**: Bracket restoration logic into conditional helper

### Implementation
1. Create new private method `RefreshBracketTargetsIfNeeded` with signature:
   ```csharp
   private void RefreshBracketTargetsIfNeeded(
       string entryName,
       PendingStopReplacement pending
   )
   ```

2. Move bracket refresh logic from lines ~211-217 into helper:
   ```csharp
   if (!pending.BracketRestorationNeeded)
   {
       var refreshedTargets = RefreshTargetSnapshot(entryName);
       pending.CapturedTargets = refreshedTargets;
       pending.BracketRestorationNeeded = refreshedTargets != null && refreshedTargets.Length > 0;
   }
   ```

3. Replace inline logic in main method with helper call:
   ```csharp
   else if (pendingStopReplacements.TryGetValue(entryName, out var pending))
   {
       RefreshBracketTargetsIfNeeded(entryName, pending);
       pending.StopPrice = validatedStopPrice;
   }
   ```

4. Run `dotnet build` to verify compilation
5. Run `dotnet test` to verify existing tests pass
6. Run `python scripts/complexity_audit.py` to verify helper CYC=1

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Helper complexity CYC=1 (single conditional)
- [ ] Main method calls helper in TryGetValue branch
- [ ] Bracket restoration logic preserved
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] No behavioral changes (black-box identical)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Test Coverage
- **New Unit Test 1**: `RefreshBracketTargetsIfNeeded_AlreadyPopulated_NoRefresh`
  - Verify RefreshTargetSnapshot NOT called when BracketRestorationNeeded=true
  - Verify CapturedTargets unchanged
  
- **New Unit Test 2**: `RefreshBracketTargetsIfNeeded_NotPopulated_RefreshesTargets`
  - Verify RefreshTargetSnapshot called when BracketRestorationNeeded=false
  - Verify CapturedTargets updated
  - Verify BracketRestorationNeeded flag updated

---

## TICKET-4: Refactor Main Method Orchestration

### Scope
- **Current Method**: `UpdateExistingPendingReplacement`
- **Current CYC**: 9
- **Target CYC**: 5
- **Extraction**: Final orchestration refactor using all three helpers

### Implementation
1. Refactor main method to use all three helpers:
   ```csharp
   private void UpdateExistingPendingReplacement(
       string entryName,
       PositionInfo pos,
       Order currentStop,
       double validatedStopPrice,
       int newTrailLevel
   )
   {
       // Step 1: Capture targets (existing logic)
       var capturedTargets = CaptureTargetSnapshot(entryName);
       
       // Step 2: Create pending replacement (TICKET-1 helper)
       var newPending = CreatePendingReplacement(entryName, pos, currentStop, validatedStopPrice, capturedTargets);
       
       // Step 3: Try add or update
       if (pendingStopReplacements.TryAdd(entryName, newPending))
       {
           int currentCount = Interlocked.Increment(ref pendingReplacementCount);
           HandleCircuitBreakerCheck(currentCount); // TICKET-2 helper
       }
       else if (pendingStopReplacements.TryGetValue(entryName, out var pending))
       {
           RefreshBracketTargetsIfNeeded(entryName, pending); // TICKET-3 helper
           pending.StopPrice = validatedStopPrice;
       }
       
       // Step 4: Update position (existing logic)
       pos.PendingStopReplacement = newPending;
       pos.PendingStopReplacementLevel = newTrailLevel;
   }
   ```

2. Run `python scripts/complexity_audit.py` to verify main method CYC=5
3. Run `dotnet build` to verify compilation
4. Run `dotnet test` to verify all tests pass
5. Run `dotnet csharpier format src/` to format code
6. Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast` for validation

### Acceptance Criteria
- [ ] Main method complexity reduced to CYC=5
- [ ] All three helpers integrated correctly
- [ ] Method orchestrates logic clearly (sequential steps)
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] Code formatted (CSharpier)
- [ ] Pre-push validation passes (Fast mode)
- [ ] Diff size <10,000 characters
- [ ] No scope creep (single method only)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Test Coverage
- **Integration Test**: Existing tests for UpdateExistingPendingReplacement should pass unchanged
- **Complexity Regression Test**: Verify main method CYC ≤ 8 (strict Jane Street standard)

---

## Execution Strategy

### Sequential Order (TDD Approach)
1. **TICKET-1**: Extract pure function (lowest risk, highest testability)
2. **TICKET-2**: Extract side effects (isolated, mockable)
3. **TICKET-3**: Extract conditional logic (simple, single branch)
4. **TICKET-4**: Refactor orchestration (integration, final verification)

### Verification Checkpoints
After each ticket:
- ✅ Run `dotnet build` (zero errors)
- ✅ Run `dotnet test` (100% pass)
- ✅ Run `python scripts/complexity_audit.py` (verify CYC targets)
- ✅ Verify no scope creep (single method only)

After TICKET-4:
- ✅ Run `dotnet csharpier format src/` (formatting)
- ✅ Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast` (13 checks)
- ✅ Verify diff size <10,000 characters
- ✅ Verify lock-free compliance: `grep -r "lock(" src/V12_002.Trailing.StopUpdate.cs` → zero matches

### Rollback Strategy
- Each ticket is independently revertible
- Checkpoint after each ticket completion
- If any ticket fails, revert to previous checkpoint
- Maximum blast radius: single method body

---

## Phase 5 Verification Checklist

After all tickets completed:
- [ ] Compare implementation against 02-architecture-plan.md
- [ ] Verify complexity: main ≤5, helpers ≤2
- [ ] Verify lock-free: `grep -r "lock(" src/` → zero matches
- [ ] Verify ASCII-only: no Unicode characters
- [ ] Verify diff size: <10,000 characters
- [ ] Verify scope: single method only
- [ ] Run full pre-push validation (13 checks)
- [ ] Run `powershell -File .\deploy-sync.ps1` (hard-link sync)
- [ ] Verify NinjaTrader F5 test (BUILD_TAG validation)

---

## Risk Mitigation

### Low-Risk Extraction
- **Pure function first** (TICKET-1): Zero side effects, highest testability
- **Isolated side effects second** (TICKET-2): Mockable, single responsibility
- **Simple conditional third** (TICKET-3): Single branch, clear logic
- **Integration last** (TICKET-4): All helpers tested independently

### Performance Validation (Optional)
- Verify JIT inlining with BenchmarkDotNet
- Confirm zero allocations with dotMemory
- Validate hot path unchanged with profiler

### Test-Driven Development
- Write unit tests BEFORE extraction
- Verify existing integration tests pass AFTER each ticket
- Add complexity regression test in TICKET-4

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC = 9 (single method)
- **After**: CYC = 5 (main) + 1+2+1 (helpers) = 9 redistributed
- **Jane Street Compliance**: Main method CYC = 5 ≤ 8 ✅

### Code Quality
- **Cognitive Load**: Reduced (single responsibility per method)
- **Testability**: Improved (pure functions, isolated side effects)
- **Maintainability**: Improved (clear orchestration, explicit state)

### V12 DNA Compliance
- ✅ Correctness by Construction (type-safe state transitions)
- ✅ Lock-Free Actor Pattern (zero lock() blocks)
- ✅ ASCII-Only Compliance (no Unicode)
- ✅ Jane Street Alignment (cognitive simplicity)

---

**END OF TICKET BREAKDOWN**
