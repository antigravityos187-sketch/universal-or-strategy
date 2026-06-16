# Extraction Tickets: EPIC-CCN-054

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2.5 hours
- **Target Method**: SymmetryGuardTryResolveFollower
- **Current CYC**: 12
- **Target CYC**: ≤8 (distributed: 5 + 3 + 2 + 2)

## TICKET-1: Extract TryGetDispatchContext Helper

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollower`
- **Current CYC**: 12
- **Target CYC**: 5 (main method after all extractions)
- **Extraction**: Dispatch context lookup and validation logic (Lines 137-157)

### Implementation
1. Create new private method `TryGetDispatchContext` with signature:
   ```csharp
   private bool TryGetDispatchContext(
       string fleetEntryName,
       PositionInfo pos,
       PendingFollowerFill pending,
       DateTime nowUtc,
       out SymmetryDispatchContext ctx
   )
   ```
2. Extract lines 137-157 from main method into new helper
3. Preserve exact logic:
   - Lookup dispatch ID from fleet entry name
   - Retrieve dispatch context by ID
   - Validate context is not null
   - Call SymmetryGuardSkipFollower if timeout exceeded
4. Add XML documentation explaining:
   - Purpose: Retrieve and validate dispatch context
   - Return semantics: false = waiting, true = processed
   - Out parameter: ctx (dispatch context or null)
5. Replace extracted section in main method with single helper call:
   ```csharp
   if (!TryGetDispatchContext(fleetEntryName, pos, pending, nowUtc, out var ctx))
       return false;
   ```
6. Run CSharpier formatting

### Acceptance Criteria
- [ ] Helper method created with CYC = 3
- [ ] Main method CYC reduced (step toward ≤5)
- [ ] Zero functional changes (logic preserved exactly)
- [ ] XML documentation added
- [ ] All tests pass (FSMActorTests.cs)
- [ ] Build succeeds
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- None (first ticket)

### Verification Commands
```bash
python scripts/complexity_audit.py
dotnet build
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj
```

---

## TICKET-2: Extract TryGetResolvedAnchor Helper

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollower`
- **Current CYC**: After TICKET-1 completion
- **Target CYC**: 5 (main method after all extractions)
- **Extraction**: Anchor resolution check logic (Lines 159-177)

### Implementation
1. Create new private method `TryGetResolvedAnchor` with signature:
   ```csharp
   private bool TryGetResolvedAnchor(
       SymmetryDispatchContext ctx,
       string fleetEntryName,
       PositionInfo pos,
       PendingFollowerFill pending,
       DateTime nowUtc,
       out double masterAnchor
   )
   ```
2. Extract lines 159-177 from main method into new helper
3. Preserve exact logic:
   - Read atomic anchor snapshot (ADR-019 pattern)
   - Check if master anchor is resolved
   - Call SymmetryGuardSkipFollower if timeout exceeded
4. Add XML documentation explaining:
   - Purpose: Validate master anchor is resolved
   - Return semantics: false = waiting, true = processed
   - Out parameter: masterAnchor (resolved price)
   - Lock-free guarantee: Atomic snapshot reads
5. Replace extracted section in main method with single helper call:
   ```csharp
   if (!TryGetResolvedAnchor(ctx, fleetEntryName, pos, pending, nowUtc, out var masterAnchor))
       return false;
   ```
6. Run CSharpier formatting

### Acceptance Criteria
- [ ] Helper method created with CYC = 2
- [ ] Main method CYC reduced (step toward ≤5)
- [ ] Zero functional changes (logic preserved exactly)
- [ ] XML documentation added
- [ ] All tests pass (FSMActorTests.cs)
- [ ] Build succeeds
- [ ] Lock-free pattern preserved (atomic reads)
- [ ] ASCII-only compliance maintained

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```bash
python scripts/complexity_audit.py
dotnet build
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj
grep -r "lock(" src/V12_002.Symmetry.Follower.cs  # Must return zero matches
```

---

## TICKET-3: Extract ValidateSlippage Helper

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollower`
- **Current CYC**: After TICKET-2 completion
- **Target CYC**: 5 (main method final state)
- **Extraction**: Slippage validation logic (Lines 179-197)

### Implementation
1. Create new private method `ValidateSlippage` with signature:
   ```csharp
   private bool ValidateSlippage(
       string fleetEntryName,
       PositionInfo pos,
       PendingFollowerFill pending,
       double masterAnchor
   )
   ```
2. Extract lines 179-197 from main method into new helper
3. Preserve exact logic:
   - Calculate slippage in points, ticks, and USD
   - Compare against SymmetryMaxSlippageTicks threshold
   - Compare against SymmetryMaxSlippageUsdPerContract threshold
   - Call SymmetryGuardSkipFollower if either threshold breached
4. Add XML documentation explaining:
   - Purpose: Validate slippage within thresholds
   - Return semantics: false = breach detected, true = validation passed
   - Pure calculation: No state mutations
5. Replace extracted section in main method with single helper call:
   ```csharp
   if (!ValidateSlippage(fleetEntryName, pos, pending, masterAnchor))
       return false;
   ```
6. Run CSharpier formatting
7. Verify main method now has CYC = 5

### Acceptance Criteria
- [ ] Helper method created with CYC = 2
- [ ] Main method CYC reduced to exactly 5
- [ ] Zero functional changes (logic preserved exactly)
- [ ] XML documentation added
- [ ] All tests pass (FSMActorTests.cs)
- [ ] Build succeeds
- [ ] Pure calculation (no allocations)
- [ ] ASCII-only compliance maintained
- [ ] **FINAL**: All methods ≤8 CYC (Jane Street compliance)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```bash
python scripts/complexity_audit.py  # Verify all methods ≤8
dotnet build
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Final Verification Checklist

After completing all 3 tickets:

### Complexity Validation
- [ ] Main method CYC = 5
- [ ] TryGetDispatchContext CYC = 3
- [ ] TryGetResolvedAnchor CYC = 2
- [ ] ValidateSlippage CYC = 2
- [ ] Total distributed CYC = 12 (same as before, but compliant)

### DNA Compliance
- [ ] Zero lock() statements
- [ ] ASCII-only compliance
- [ ] Correctness by construction (out parameters)
- [ ] Lock-free actor pattern preserved

### PR Hygiene
- [ ] Diff size <10k characters
- [ ] No scope creep (single method only)
- [ ] No whitespace mutations outside target file
- [ ] CSharpier formatting applied

### Build & Test
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes (100%)
- [ ] `powershell -File .\scripts\pre_push_validation.ps1` passes
- [ ] `powershell -File .\deploy-sync.ps1` succeeds

### Jane Street Alignment
- [ ] All methods ≤8 CYC
- [ ] Cognitive simplicity achieved
- [ ] Testability improved (isolated helpers)
- [ ] Single-responsibility principle enforced
- [ ] Zero new allocations (microsecond-latency preserved)

---

## Execution Strategy

### Sequential Execution
Execute tickets in strict order: TICKET-1 → TICKET-2 → TICKET-3

**Rationale**: Each ticket builds on the previous extraction, progressively reducing main method complexity.

### Checkpoint After Each Ticket
1. Run complexity audit
2. Run build
3. Run tests
4. Commit changes
5. Proceed to next ticket

### Rollback Plan
If any ticket fails verification:
1. Restore from checkpoint (Bob CLI `/restore`)
2. Review extraction logic
3. Re-attempt with corrected approach

---

## Success Metrics

### Quantitative
- **CYC Reduction**: 12 → 5 (main method)
- **Helper Count**: 3 new methods
- **LOC per Method**: <30 lines each
- **Test Pass Rate**: 100%
- **Build Success**: Zero errors

### Qualitative
- **Readability**: Improved (single-responsibility)
- **Testability**: Enhanced (isolated helpers)
- **Maintainability**: Better (cognitive simplicity)
- **Performance**: Preserved (zero allocations)

---

**Phase 4 Status**: COMPLETE
**Ticket Count**: 3
**Estimated Effort**: 2.5 hours
**Ready for Phase 5**: YES ✅
