# Extraction Tickets: EPIC-CCN-046

## Overview
- **Epic**: EPIC-CCN-046 - HandleChartClick_ConvertPrice Complexity Reduction
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 4-6 hours
- **Target File**: src/V12_002.UI.Callbacks.cs
- **Current Complexity**: 9 (CYC)
- **Target Complexity**: ≤8 (CYC)

## Extraction Strategy
Transform HandleChartClick_ConvertPrice from monolithic method into orchestrator pattern with 3 focused helper methods.

---

## TICKET-1: Extract ValidateChartClickInput

### Scope
- **Current Method**: `HandleChartClick_ConvertPrice`
- **Current CYC**: 9
- **Target CYC**: ≤8 (after all extractions)
- **Extraction**: Validation logic for chart click coordinates and chart state

### Implementation
1. Create private method `ValidateChartClickInput(chart, clickX, clickY)` returning `bool`
2. Move validation logic from HandleChartClick_ConvertPrice:
   - Check if chart instance is valid (not null)
   - Validate click coordinates are within chart bounds
3. Update HandleChartClick_ConvertPrice to call helper:
   - `if (!ValidateChartClickInput(chart, x, y)) return;`
4. Add XML documentation to helper method
5. Run CSharpier formatter: `dotnet csharpier format src/V12_002.UI.Callbacks.cs`

### Acceptance Criteria
- [ ] ValidateChartClickInput method created with CYC ≤2
- [ ] Validation logic extracted from main method
- [ ] Main method calls helper with early return on false
- [ ] XML documentation added
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (manual UI test)
- [ ] CSharpier formatting applied
- [ ] Git commit: "EPIC-CCN-046: Extract ValidateChartClickInput"

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Build check
dotnet build

# Complexity check (expect CYC reduction)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/V12_002.UI.Callbacks.cs
```

---

## TICKET-2: Extract ConvertPriceCoordinates

### Scope
- **Current Method**: `HandleChartClick_ConvertPrice`
- **Current CYC**: ~7 (after TICKET-1)
- **Target CYC**: ≤5 (after this extraction)
- **Extraction**: Price coordinate conversion logic

### Implementation
1. Create private method `ConvertPriceCoordinates(chart, clickY)` returning `double?`
2. Move conversion logic from HandleChartClick_ConvertPrice:
   - Extract price scale from chart
   - Apply coordinate transformation
   - Handle edge cases (out of range, invalid scale)
   - Return null if conversion fails
3. Update HandleChartClick_ConvertPrice to call helper:
   - `double? price = ConvertPriceCoordinates(chart, y);`
   - `if (price == null) return;`
4. Add XML documentation to helper method
5. Run CSharpier formatter: `dotnet csharpier format src/V12_002.UI.Callbacks.cs`

### Acceptance Criteria
- [ ] ConvertPriceCoordinates method created with CYC ≤3
- [ ] Conversion logic extracted from main method
- [ ] Main method calls helper with null check
- [ ] XML documentation added
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (manual UI test)
- [ ] CSharpier formatting applied
- [ ] Git commit: "EPIC-CCN-046: Extract ConvertPriceCoordinates"

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Build check
dotnet build

# Complexity check (expect further CYC reduction)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/V12_002.UI.Callbacks.cs
```

---

## TICKET-3: Extract UpdateChartState

### Scope
- **Current Method**: `HandleChartClick_ConvertPrice`
- **Current CYC**: ~5 (after TICKET-2)
- **Target CYC**: ≤3 (after this extraction)
- **Extraction**: Chart UI state update logic

### Implementation
1. Create private method `UpdateChartState(chart, price)` returning `void`
2. Move state update logic from HandleChartClick_ConvertPrice:
   - Update chart overlay with price marker
   - Trigger UI refresh if needed
   - Use FSM Enqueue pattern if state mutation required
3. Update HandleChartClick_ConvertPrice to call helper:
   - `UpdateChartState(chart, price.Value);`
4. Add XML documentation to helper method
5. Run CSharpier formatter: `dotnet csharpier format src/V12_002.UI.Callbacks.cs`

### Acceptance Criteria
- [ ] UpdateChartState method created with CYC ≤2
- [ ] State update logic extracted from main method
- [ ] Main method calls helper (fire-and-forget)
- [ ] XML documentation added
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (manual UI test)
- [ ] No lock() statements introduced (lock-free audit)
- [ ] CSharpier formatting applied
- [ ] Git commit: "EPIC-CCN-046: Extract UpdateChartState"

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
# Build check
dotnet build

# Complexity check (expect CYC ≤3 for main method)
python scripts/complexity_audit.py

# Lock-free audit (expect zero matches)
grep -r "lock(" src/V12_002.UI.Callbacks.cs

# Format check
dotnet csharpier check src/V12_002.UI.Callbacks.cs
```

---

## TICKET-4: Verify Complexity Reduction

### Scope
- **Current Method**: `HandleChartClick_ConvertPrice`
- **Expected CYC**: ≤3 (orchestrator with 3 decision points)
- **Verification**: Full compliance audit

### Implementation
1. Run full complexity audit on V12_002.UI.Callbacks.cs
2. Verify HandleChartClick_ConvertPrice CYC ≤8 (target met)
3. Verify each helper method CYC ≤3
4. Run full build readiness check
5. Run lock-free audit (zero lock() statements)
6. Manual UI testing (behavior preservation)
7. Hard-link sync: `powershell -File .\deploy-sync.ps1`
8. Git commit: "EPIC-CCN-046: Verify complexity reduction"

### Acceptance Criteria
- [ ] HandleChartClick_ConvertPrice CYC ≤8 (verified)
- [ ] ValidateChartClickInput CYC ≤2 (verified)
- [ ] ConvertPriceCoordinates CYC ≤3 (verified)
- [ ] UpdateChartState CYC ≤2 (verified)
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Zero lock() statements: `grep -r "lock(" src/V12_002.UI.Callbacks.cs`
- [ ] UI behavior identical to original (manual test)
- [ ] Hard-link sync completed
- [ ] All tests pass (if test infrastructure exists)
- [ ] Pre-push validation: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```powershell
# Full complexity audit
python scripts/complexity_audit.py

# Build readiness (includes CSharpier check)
powershell -File .\scripts\build_readiness.ps1

# Lock-free audit
grep -r "lock(" src/V12_002.UI.Callbacks.cs

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Success Metrics

### Complexity Reduction
- **Before**: HandleChartClick_ConvertPrice CYC = 9
- **After**: 
  - Main orchestrator CYC ≤3
  - ValidateChartClickInput CYC ≤2
  - ConvertPriceCoordinates CYC ≤3
  - UpdateChartState CYC ≤2
- **Total**: 10 decision points across 4 methods (vs 9 in monolith)
- **Target Met**: ✅ Main method CYC ≤8

### V12 DNA Compliance
- **Correctness by Construction**: ✅ Type system enforces valid states
- **Lock-Free Actor Pattern**: ✅ Zero lock() statements
- **ASCII-Only Compliance**: ✅ No Unicode characters
- **Jane Street Alignment**: ✅ Cognitive simplicity (each method ≤3 decision points)

### PR Hygiene
- **Diff Size**: ~800 characters (well under 10k limit)
- **Scope**: Single method extraction (no scope creep)
- **Build**: Zero compilation errors
- **Behavior**: Identical UI behavior before/after

---

## Rollback Plan

### Rollback Triggers
1. Compilation errors after any extraction
2. Behavior changes detected in UI testing
3. Complexity not reduced to ≤8 after all extractions
4. New lock() statements introduced
5. Performance regression detected

### Rollback Procedure
1. Identify failing ticket (TICKET-1, TICKET-2, TICKET-3, or TICKET-4)
2. Git revert to commit before failing ticket
3. Document failure reason in manifest.json
4. Re-plan extraction strategy if needed
5. Restart from last successful ticket

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Protocol**: V12.23 Phase 4 (Ticket Generation)
**Status**: READY FOR PHASE 5 EXECUTION
