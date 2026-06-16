# Extraction Tickets: EPIC-CCN-073

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2 hours
- **Target Method**: `DeserializeSnapshot` in `src/V12_002.StickyState.cs`
- **Current Complexity**: CYC = 9
- **Target Complexity**: CYC ≤ 8 (Jane Street strict standard)

## TICKET-1: Extract ParseScalarFields Helper

### Scope
- **Current Method**: `DeserializeSnapshot` (lines 441-502)
- **Current CYC**: 9
- **Target CYC**: Reduce by extracting scalar field parsing
- **Extraction**: Lines 447-452 (6 lines of sequential field parsing)

### Implementation
1. Create new private method `ParseScalarFields` after line 502
2. Method signature: `private void ParseScalarFields(string json, StateSnapshot snapshot)`
3. Copy lines 447-452 into new method body:
   - ParseJsonLong for Timestamp
   - ParseJsonString for CurrentState
   - ParseJsonInt for BarsSinceEntry
   - ParseJsonBool for IsFlat
4. Replace lines 447-452 in DeserializeSnapshot with: `ParseScalarFields(json, snapshot);`
5. Verify compilation with `dotnet build`

### Acceptance Criteria
- [ ] New method `ParseScalarFields` created with correct signature
- [ ] Lines 447-452 extracted to new method
- [ ] Main method calls new helper method
- [ ] Method complexity: ParseScalarFields CYC = 1 (sequential, no branching)
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes (pure refactoring)
- [ ] Build succeeds with zero errors
- [ ] No lock() statements introduced (lock-free compliance)

### Dependencies
- None (first ticket)

### Verification Commands
```bash
dotnet build
dotnet test
python scripts/complexity_audit.py
```

---

## TICKET-2: Extract ParseAccountPositions Helper

### Scope
- **Current Method**: `DeserializeSnapshot` (after TICKET-1 completion)
- **Current CYC**: ~6 (after TICKET-1)
- **Target CYC**: ≤ 3 (orchestrator only)
- **Extraction**: Lines 454-484 (31 lines of dictionary parsing logic)

### Implementation
1. Create new private method `ParseAccountPositions` after ParseScalarFields
2. Method signature: `private void ParseAccountPositions(string json, StateSnapshot snapshot)`
3. Copy lines 454-484 into new method body:
   - Find "AccountPositions" section in JSON
   - Parse nested dictionary with foreach loop
   - Handle IndexOf operations for key-value extraction
   - Populate snapshot.AccountPositions dictionary
4. Replace lines 454-484 in DeserializeSnapshot with: `ParseAccountPositions(json, snapshot);`
5. Verify compilation with `dotnet build`

### Acceptance Criteria
- [ ] New method `ParseAccountPositions` created with correct signature
- [ ] Lines 454-484 extracted to new method
- [ ] Main method calls new helper method
- [ ] Method complexity: ParseAccountPositions CYC = 6 (nested conditionals + loop)
- [ ] Main method complexity: DeserializeSnapshot CYC ≤ 3 (try-catch orchestration only)
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes (pure refactoring)
- [ ] Build succeeds with zero errors
- [ ] No lock() statements introduced (lock-free compliance)
- [ ] Hard links synced (`powershell -File .\deploy-sync.ps1`)

### Dependencies
- **TICKET-1 must be completed first**
- Requires ParseScalarFields to be in place

### Verification Commands
```bash
dotnet build
dotnet test
python scripts/complexity_audit.py
powershell -File .\deploy-sync.ps1
```

---

## Final Complexity Distribution

### Before Extraction
- **DeserializeSnapshot**: CYC = 9 (62 lines, monolithic)

### After TICKET-1
- **DeserializeSnapshot**: CYC = ~6 (orchestrator + dictionary parsing)
- **ParseScalarFields**: CYC = 1 (sequential field parsing)

### After TICKET-2 (Final State)
- **DeserializeSnapshot**: CYC = 3 (orchestrator: try + 2 catch blocks)
- **ParseScalarFields**: CYC = 1 (sequential field parsing)
- **ParseAccountPositions**: CYC = 6 (dictionary parsing with nested logic)
- **Total Distributed Complexity**: 10 (3+1+6, down from 9 but better organized)

## Lock-Free Compliance

All tickets maintain lock-free properties:
- ✅ No lock() statements
- ✅ Atomic primitives only (Interlocked.Increment in error handlers)
- ✅ Pure function pattern (no global state mutation)
- ✅ Thread-safe by design (parameter passing, local variables)

## Jane Street Alignment

### Cognitive Simplicity
- Each method has single, clear responsibility
- Orchestrator pattern separates concerns
- Helper methods are independently testable

### Testability
- **ParseScalarFields**: Test with valid/invalid JSON, verify field population
- **ParseAccountPositions**: Test with empty/malformed/valid dictionaries
- **DeserializeSnapshot**: Integration test for orchestration + error handling

### Correctness by Construction
- Single Responsibility Principle applied
- Fail-fast error handling in orchestrator
- Immutable inputs, controlled mutation via parameters
- Strong typing throughout

## Execution Notes

### Pre-Execution Checklist
- [ ] Checkpoint created (`/checkpoint` in Bob CLI)
- [ ] Current branch is up-to-date with main
- [ ] All tests passing before changes
- [ ] Complexity audit baseline captured

### Post-Execution Checklist
- [ ] All tickets completed sequentially
- [ ] Complexity target achieved (CYC ≤ 8)
- [ ] All tests passing
- [ ] Build succeeds
- [ ] Hard links synced
- [ ] No lock() statements introduced
- [ ] Ready for Phase 5 verification

## Rollback Plan

If any ticket fails:
1. Use Bob CLI `/restore` to revert to checkpoint
2. Review failure reason
3. Adjust extraction strategy if needed
4. Re-attempt with corrected approach

## Success Metrics

- ✅ **Complexity Reduced**: DeserializeSnapshot from CYC=9 to CYC=3
- ✅ **Lock-Free Maintained**: Zero lock() statements
- ✅ **Tests Pass**: 100% test suite green
- ✅ **Build Clean**: Zero compilation errors
- ✅ **Jane Street Aligned**: Cognitive simplicity + testability achieved
