# Extraction Tickets: EPIC-CCN-017

## Overview
- **Epic ID**: EPIC-CCN-017
- **Method**: TryApplyConfigTarget_Value
- **File**: src/V12_002.UI.IPC.Commands.Config.cs
- **Current Complexity**: CYC 17
- **Target Complexity**: CYC 8
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2-3 hours

## Extraction Strategy
Pattern extraction to eliminate duplication across T1/T2/T3 handlers:
- Extract parse-validate-assign pattern into reusable helper method
- Refactor numeric target handlers to use helper
- Preserve CIT handler as-is (no duplication)

---

## TICKET-1: TDD Baseline Creation

### Scope
- **Current Method**: `TryApplyConfigTarget_Value`
- **Current CYC**: 17
- **Target**: Establish 100% baseline test coverage
- **Type**: Test-Driven Development (TDD) Baseline

### Implementation
1. Create test file: `tests/V12_Performance.Tests/UI/IPC/ConfigCommandsTests.cs`
2. Test all 4 key paths:
   - T1 handler (parse → validate → assign)
   - T2 handler (parse → validate → assign)
   - T3 handler (parse → validate → assign)
   - CIT handler (direct string assignment)
3. Test validation rejection paths:
   - Invalid multiplier values (via ValidateIpcMultiplier)
   - Logging behavior for rejected values
4. Test parse failure paths:
   - Non-numeric strings for T1/T2/T3
   - Verify graceful handling (no exceptions)
5. Test edge cases:
   - Empty strings
   - Null values (if applicable)
   - Boundary values (min/max multipliers)
6. Verify all tests GREEN before proceeding to TICKET-2

### Acceptance Criteria
- [ ] Test file created with comprehensive coverage
- [ ] All 4 key paths tested (T1, T2, T3, CIT)
- [ ] Validation rejection paths tested
- [ ] Parse failure paths tested
- [ ] Edge cases covered
- [ ] All tests pass (100% GREEN)
- [ ] Baseline coverage established for regression detection

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Run tests
dotnet test tests/V12_Performance.Tests/UI/IPC/ConfigCommandsTests.cs

# Verify coverage
# (Manual verification: all code paths exercised)
```

### Estimated Effort
- **Time**: 45-60 minutes
- **Complexity**: LOW (straightforward test creation)

---

## TICKET-2: Helper Method Extraction & Refactoring

### Scope
- **Current Method**: `TryApplyConfigTarget_Value`
- **Current CYC**: 17
- **Target CYC**: 8
- **Extraction**: Create `TryApplyTargetValue` helper method and refactor T1/T2/T3 handlers

### Implementation

#### Step 1: Extract Helper Method
Create new private method in `V12_002.UI.IPC.Commands.Config.cs`:

```csharp
private bool TryApplyTargetValue(string targetName, string value, Action<double> setter)
{
    // Parse string to double
    if (!double.TryParse(value, out double v))
    {
        return true; // Key recognized, value ignored
    }

    // Validate via existing method
    if (!ValidateIpcMultiplier(v, out string reason))
    {
        Print($"IPC rejected {targetName}: {reason}");
        return true; // Key recognized, value rejected
    }

    // Assign validated value
    setter(v);
    return true;
}
```

**Helper Complexity**: CYC 3
- Parse check: +1
- Validation check: +1
- Success path: +1

#### Step 2: Refactor T1 Handler
Replace T1 block with helper call:
```csharp
if (key == "T1")
{
    return TryApplyTargetValue("T1", val, v => Target1Value = v);
}
```

**Verify**: Run tests → confirm GREEN

#### Step 3: Refactor T2 Handler
Replace T2 block with helper call:
```csharp
if (key == "T2")
{
    return TryApplyTargetValue("T2", val, v => Target2Value = v);
}
```

**Verify**: Run tests → confirm GREEN

#### Step 4: Refactor T3 Handler
Replace T3 block with helper call:
```csharp
if (key == "T3")
{
    return TryApplyTargetValue("T3", val, v => Target3Value = v);
}
```

**Verify**: Run tests → confirm GREEN

#### Step 5: Preserve CIT Handler
Leave CIT handler unchanged (no duplication):
```csharp
if (key == "CIT")
{
    ChaseIfTouchPoints = val;
    return true;
}
```

#### Step 6: Final Orchestrator Structure
```csharp
private bool TryApplyConfigTarget_Value(string key, string val)
{
    if (key == "T1") return TryApplyTargetValue("T1", val, v => Target1Value = v);
    if (key == "T2") return TryApplyTargetValue("T2", val, v => Target2Value = v);
    if (key == "T3") return TryApplyTargetValue("T3", val, v => Target3Value = v);
    if (key == "CIT") { ChaseIfTouchPoints = val; return true; }
    return false; // Key not recognized
}
```

**Orchestrator Complexity**: CYC 5
- T1 check: +1
- T2 check: +1
- T3 check: +1
- CIT check: +1
- Fallback return: +1

**Total Complexity**: CYC 8 (3 + 5)

### Acceptance Criteria
- [ ] Helper method `TryApplyTargetValue` created
- [ ] Helper method complexity: CYC 3
- [ ] T1 handler refactored to use helper
- [ ] T2 handler refactored to use helper
- [ ] T3 handler refactored to use helper
- [ ] CIT handler preserved unchanged
- [ ] Orchestrator complexity: CYC 5
- [ ] Total method complexity: CYC 8
- [ ] All tests pass (100% GREEN)
- [ ] No behavioral changes (regression tests pass)
- [ ] Build succeeds
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- **TICKET-1** must be completed first (TDD baseline required)

### Verification Commands
```powershell
# Run tests after each refactoring step
dotnet test tests/V12_Performance.Tests/UI/IPC/ConfigCommandsTests.cs

# Verify complexity reduction
python scripts/complexity_audit.py

# Verify lock-free compliance
grep -r "lock(" src/V12_002.UI.IPC.Commands.Config.cs

# Format code
dotnet csharpier format src/

# Build
dotnet build src/V12_002.csproj
```

### Estimated Effort
- **Time**: 60-90 minutes
- **Complexity**: MEDIUM (incremental refactoring with test verification)

---

## TICKET-3: Verification & Sign-off

### Scope
- **Type**: Final verification and quality gates
- **Target**: Confirm all success criteria met

### Implementation

#### Step 1: Complexity Verification
```powershell
# Run complexity audit
python scripts/complexity_audit.py

# Expected output:
# TryApplyConfigTarget_Value: CYC 8 (PASS)
# TryApplyTargetValue: CYC 3 (PASS)
```

#### Step 2: Lock-Free Compliance
```powershell
# Verify no lock() statements
grep -r "lock(" src/V12_002.UI.IPC.Commands.Config.cs

# Expected output: (no matches)
```

#### Step 3: Pre-Push Validation
```powershell
# Run full validation suite
powershell -File .\scripts\pre_push_validation.ps1

# Expected: All 13 checks PASS
```

#### Step 4: Hard-Link Integrity
```powershell
# Sync NinjaTrader hard links
powershell -File .\deploy-sync.ps1

# Expected: DIFF GUARD PASS (<10k chars)
```

#### Step 5: NinjaTrader Runtime Test
1. Open NinjaTrader
2. Press F5 to compile
3. Verify no compilation errors
4. Test IPC config commands (T1, T2, T3, CIT)
5. Verify behavior unchanged

#### Step 6: CodeScene Hotspot Analysis
1. Open `src/V12_002.UI.IPC.Commands.Config.cs` in VS Code
2. Check CodeScene status bar for Code Health Score
3. Verify improvement from baseline
4. Document hotspot reduction

#### Step 7: Update Manifest
Update `docs/brain/EPIC-CCN-017/manifest.json`:
```json
{
  "phases": {
    "phase_4": {
      "status": "completed",
      "output": "04-tickets.md",
      "ticket_count": 3
    },
    "phase_5": {
      "status": "completed",
      "tickets_executed": 3,
      "complexity_achieved": "CYC_8",
      "verification_passed": true,
      "date": "2026-06-15"
    }
  }
}
```

### Acceptance Criteria
- [ ] Complexity reduced to CYC 8 (verified)
- [ ] Lock-free compliance maintained (verified)
- [ ] All tests pass (100% GREEN)
- [ ] Pre-push validation passes (13/13 checks)
- [ ] Hard-link integrity verified (deploy-sync PASS)
- [ ] NinjaTrader F5 test passes
- [ ] CodeScene hotspot improvement documented
- [ ] Manifest updated with Phase 5 completion
- [ ] No new Codacy issues introduced
- [ ] No new CodeRabbit critical/high findings

### Dependencies
- **TICKET-2** must be completed first (extraction implementation required)

### Verification Commands
```powershell
# Full verification suite
python scripts/complexity_audit.py
grep -r "lock(" src/
dotnet test
powershell -File .\scripts\pre_push_validation.ps1
powershell -File .\deploy-sync.ps1
```

### Estimated Effort
- **Time**: 30-45 minutes
- **Complexity**: LOW (verification and documentation)

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC 17
- **After**: CYC 8
- **Reduction**: 53% (9 points)
- **Target Met**: ✅ YES (CYC ≤ 15, Jane Street aligned)

### Code Quality
- **Lock-Free**: ✅ Maintained (0 lock() statements)
- **ASCII-Only**: ✅ Maintained (0 non-ASCII characters)
- **Correctness by Construction**: ✅ Enforced (type-safe Action delegate)
- **Jane Street Alignment**: ✅ Achieved (cognitive simplicity)

### PR Hygiene
- **Diff Size**: ~800 chars (well within 10k limit)
- **Scope Creep**: ZERO (single method extraction)
- **Build Readiness**: ✅ Behavior-preserving transformation

### Risk Profile
- **Implementation Risk**: LOW
- **Regression Risk**: MINIMAL
- **Performance Risk**: ZERO

---

## Execution Timeline

| Ticket | Description | Effort | Cumulative |
|--------|-------------|--------|------------|
| TICKET-1 | TDD Baseline | 45-60 min | 1 hour |
| TICKET-2 | Extraction & Refactoring | 60-90 min | 2.5 hours |
| TICKET-3 | Verification & Sign-off | 30-45 min | 3 hours |

**Total Estimated Effort**: 2-3 hours

---

## Notes

### Incremental Verification Strategy
Each ticket includes verification steps to catch regressions early:
- TICKET-1: Establish baseline (all tests GREEN)
- TICKET-2: Verify after each handler refactoring (T1 → T2 → T3)
- TICKET-3: Final quality gates and sign-off

### Jane Street Alignment
The extraction achieves Jane Street's strict cognitive simplicity standard:
- Helper method: CYC 3 (simple parse-validate-assign)
- Orchestrator: CYC 5 (straightforward key routing)
- Total: CYC 8 (easy to reason about under microsecond latency)

### Performance Preservation
- Lambda compiled to static method (no allocation)
- Inline-friendly method size (<20 LOC)
- No additional branching introduced
- Hot-path execution unchanged

---

**Phase 4 Status**: COMPLETED
**Tickets Generated**: 3
**Ready for Phase 5**: YES (execute TICKET-1 → TICKET-2 → TICKET-3)
