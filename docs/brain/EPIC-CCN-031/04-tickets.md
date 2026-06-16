# Extraction Tickets: EPIC-CCN-031

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4 hours
- **Target Method**: `AuditMaster_HandleNakedPosition`
- **Current CYC**: 15
- **Target CYC**: ≤ 8 (Jane Street strict standard)
- **Final CYC**: 4 (main method after all extractions)

## TICKET-1: Extract HasWorkingStopOrder (Pure Function)

### Scope
- **Current Method**: `AuditMaster_HandleNakedPosition`
- **Current CYC**: 15
- **Target CYC**: 14 (after this extraction)
- **Extraction**: Pure function to check if any order in snapshot is a working stop order

### Implementation
1. Create new private method `HasWorkingStopOrder(Order[] orders, string instrumentFullName)`
2. Extract lines 629-638 (order snapshot & stop detection logic)
3. Return boolean: true if working stop exists, false otherwise
4. Replace extracted code in main method with single call: `bool masterHasWorkingStop = HasWorkingStopOrder(orders, masterPos.Instrument.FullName);`
5. Run CSharpier: `dotnet csharpier format src/V12_002.REAPER.Audit.cs`
6. Verify complexity: `python scripts/complexity_audit.py src/V12_002.REAPER.Audit.cs`

### Method Signature
```csharp
/// <summary>
/// Pure function: Checks if any order in the snapshot is a working stop order.
/// </summary>
/// <param name="orders">Order snapshot array</param>
/// <param name="instrumentFullName">Instrument to filter by</param>
/// <returns>True if working stop order exists, false otherwise</returns>
private bool HasWorkingStopOrder(Order[] orders, string instrumentFullName)
{
    // Extract lines 629-638 here
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 15 to CYC 14
- [ ] New method has CYC 1 (single return statement)
- [ ] All tests pass: `dotnet test`
- [ ] No behavioral changes (logic identical)
- [ ] Build succeeds: `dotnet build`
- [ ] CSharpier formatting applied
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced

### Dependencies
- None (first ticket)

### Testing Focus
- Unit test: Empty order array returns false
- Unit test: Orders with no stops return false
- Unit test: Orders with working stop return true
- Unit test: Orders with non-working stop return false

---

## TICKET-2: Extract TryStartGraceWindow (State Tracking)

### Scope
- **Current Method**: `AuditMaster_HandleNakedPosition`
- **Current CYC**: 14 (after TICKET-1)
- **Target CYC**: 12 (after this extraction)
- **Extraction**: Grace window tracking logic using ConcurrentDictionary

### Implementation
1. Create new private method `TryStartGraceWindow(string accountName, int actualQty, int graceSeconds)`
2. Extract lines 640-656 (grace window tracking logic)
3. Return boolean: true if grace window just started, false if already exists
4. Replace extracted code in main method with: `if (TryStartGraceWindow(masterPos.Account.Name, masterActualQty, graceSeconds))`
5. Run CSharpier: `dotnet csharpier format src/V12_002.REAPER.Audit.cs`
6. Verify complexity: `python scripts/complexity_audit.py src/V12_002.REAPER.Audit.cs`

### Method Signature
```csharp
/// <summary>
/// Lock-free state tracking: Checks if grace window already started, initializes if not.
/// Uses ConcurrentDictionary.TryGetValue for atomic operation.
/// </summary>
/// <param name="accountName">Account identifier</param>
/// <param name="actualQty">Current position quantity</param>
/// <param name="graceSeconds">Grace period duration</param>
/// <returns>True if grace window just started, false if already exists</returns>
private bool TryStartGraceWindow(string accountName, int actualQty, int graceSeconds)
{
    // Extract lines 640-656 here
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 14 to CYC 12
- [ ] New method has CYC 2 (if-else branch)
- [ ] All tests pass: `dotnet test`
- [ ] No behavioral changes (logic identical)
- [ ] Build succeeds: `dotnet build`
- [ ] CSharpier formatting applied
- [ ] Lock-free guarantee maintained (ConcurrentDictionary only)
- [ ] No lock() statements introduced

### Dependencies
- TICKET-1 must be completed first

### Testing Focus
- Unit test: First call returns true (grace window started)
- Unit test: Second call returns false (grace window exists)
- Integration test: Concurrent calls (race condition safety)
- Unit test: ConcurrentDictionary state correctly updated

---

## TICKET-3: Extract EnqueueNakedStopWithTrigger (Async Dispatch)

### Scope
- **Current Method**: `AuditMaster_HandleNakedPosition`
- **Current CYC**: 12 (after TICKET-2)
- **Target CYC**: 4 (final target achieved)
- **Extraction**: Enqueue + trigger logic with error recovery

### Implementation
1. Create new private method `EnqueueNakedStopWithTrigger(Position masterPos, int masterActualQty, string masterExpectedKey, DateTime firstSeen)`
2. Extract lines 657-673 (enqueue + trigger logic)
3. Handle exceptions and cleanup (TryRemove from grace window dictionary)
4. Replace extracted code in main method with single call: `EnqueueNakedStopWithTrigger(masterPos, masterActualQty, masterExpectedKey, firstSeen);`
5. Add else block for cleanup: `else { _nakedStopGraceWindows.TryRemove(masterPos.Account.Name, out _); }`
6. Run CSharpier: `dotnet csharpier format src/V12_002.REAPER.Audit.cs`
7. Verify complexity: `python scripts/complexity_audit.py src/V12_002.REAPER.Audit.cs`
8. **FINAL VERIFICATION**: Main method CYC must be ≤ 8 (target: 4)

### Method Signature
```csharp
/// <summary>
/// Async dispatch: Enqueues naked stop order and triggers processing queue.
/// Handles exceptions and performs cleanup on failure.
/// Uses FSM/Actor Enqueue pattern with ConcurrentDictionary.TryRemove for cleanup.
/// </summary>
/// <param name="masterPos">Master position</param>
/// <param name="masterActualQty">Current quantity</param>
/// <param name="masterExpectedKey">Expected key for tracking</param>
/// <param name="firstSeen">Timestamp when naked position first detected</param>
private void EnqueueNakedStopWithTrigger(Position masterPos, int masterActualQty, string masterExpectedKey, DateTime firstSeen)
{
    // Extract lines 657-673 here
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 12 to CYC 4 (FINAL TARGET)
- [ ] New method has CYC 3 (if + try-catch)
- [ ] All tests pass: `dotnet test`
- [ ] No behavioral changes (logic identical)
- [ ] Build succeeds: `dotnet build`
- [ ] CSharpier formatting applied
- [ ] Lock-free guarantee maintained (Enqueue + TryRemove)
- [ ] No lock() statements introduced
- [ ] **FINAL**: Main method CYC ≤ 8 verified

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Testing Focus
- Unit test: Successful enqueue + trigger
- Unit test: Exception handling (TriggerCustomEvent fails)
- Unit test: Cleanup on exception (TryRemove called)
- Integration test: Full flow with all 3 helpers

---

## Post-Extraction Validation

### Mandatory Checks (After TICKET-3)
1. **Complexity Audit**: `python scripts/complexity_audit.py src/V12_002.REAPER.Audit.cs`
   - Main method: CYC ≤ 8 (target: 4)
   - Helper 1: CYC 1
   - Helper 2: CYC 2
   - Helper 3: CYC 3

2. **Build Readiness**: `powershell -File .\scripts\build_readiness.ps1`
   - Zero compilation errors
   - CSharpier formatting verified

3. **Lock-Free Scan**: `grep -r "lock(" src/V12_002.REAPER.Audit.cs`
   - Zero matches (no lock() statements)

4. **ASCII-Only Scan**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
   - Check #1 must pass (ASCII-only)

5. **Test Suite**: `dotnet test`
   - All existing tests pass
   - 7 new tests added (per architecture plan)

### Success Metrics
- **Complexity Reduction**: 15 → 4 (73% reduction)
- **Cognitive Load**: 1 method → 4 methods (single responsibility)
- **Testability**: Pure functions + mockable dependencies
- **Performance**: Zero additional allocations
- **Lock-Free**: 100% (no lock contention)

### PR Preparation
1. Run full validation: `powershell -File .\scripts\pre_push_validation.ps1`
2. Verify diff size: `git diff --stat` (target: <10k chars)
3. Update manifest: Set phase_4.status = "completed"
4. Create PR with title: "EPIC-CCN-031: Extract AuditMaster_HandleNakedPosition (CYC 15→4)"

---

## Execution Notes

### Sequential Execution Required
- Each ticket builds on the previous extraction
- Do NOT parallelize (dependencies exist)
- Verify complexity after each ticket

### Rollback Strategy
- Each ticket is atomic (single method extraction)
- If any ticket fails validation, rollback that ticket only
- Use git stash or restore points

### Time Estimates
- TICKET-1: 1 hour (pure function, simple)
- TICKET-2: 1.5 hours (state tracking, testing)
- TICKET-3: 1.5 hours (async dispatch, error handling)
- **Total**: 4 hours

### Risk Mitigation
- Run tests after each ticket (catch regressions early)
- Verify complexity after each ticket (ensure progress)
- Use CSharpier after each ticket (maintain formatting)
- Check lock-free guarantee after each ticket (DNA compliance)
