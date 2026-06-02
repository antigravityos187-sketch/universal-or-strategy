---
# TICKET EPIC-POSINFO-01: Refactor Switch-Based Accessors to Expression-Bodied Members
# Epic: EPIC-POSINFO
# Sequence: 1 of 1
# Depends on: NONE
---

## Objective
Refactor 6 switch-based accessor methods in PositionInfo.cs to expression-bodied members with ternary operators, eliminating 72 LOC of structural duplication while maintaining zero-allocation guarantee and Jane Street complexity compliance.

## Scope
IN scope:
- `src/V12_002.PositionInfo.cs` lines 277-400
- 6 methods: GetTargetContracts, GetTargetPrice, IsTargetFilled, MarkTargetFilled, GetTargetFilledQuantity, SetTargetFilledQuantity
- Expression-bodied member conversion (4 getter methods)
- If-else chain conversion (2 setter methods with void return)

OUT of scope:
- PositionInfo class fields (lines 36-115) - NO CHANGES
- Call sites (32 locations across 8 files) - NO CHANGES
- Method signatures - NO CHANGES
- Any other methods in the file

## Context References
- Analysis: docs/brain/EPIC-POSINFO/02-analysis.md -- Section 3 (Duplication Pattern Analysis)
- Approach: docs/brain/EPIC-POSINFO/03-approach.md -- Section 2 (Target State), Section 6 (Implementation Steps)
- Validation: docs/brain/EPIC-POSINFO/04-validation-report.md -- All risks mitigated

## Implementation Instructions

### Target Transformations

**Pattern 1: Getter Methods (4 methods)**
Convert switch statements to expression-bodied members with ternary chains:

```csharp
// BEFORE (12 lines)
private int GetTargetContracts(PositionInfo pos, int targetNumber)
{
    switch (targetNumber)
    {
        case 1: return pos.T1Contracts;
        case 2: return pos.T2Contracts;
        case 3: return pos.T3Contracts;
        case 4: return pos.T4Contracts;
        case 5: return pos.T5Contracts;
        default: return 0;
    }
}

// AFTER (6 lines)
private int GetTargetContracts(PositionInfo pos, int targetNumber) =>
    targetNumber == 1 ? pos.T1Contracts :
    targetNumber == 2 ? pos.T2Contracts :
    targetNumber == 3 ? pos.T3Contracts :
    targetNumber == 4 ? pos.T4Contracts :
    targetNumber == 5 ? pos.T5Contracts : 0;
```

**Pattern 2: Setter Methods (2 methods)**
Convert switch statements to if-else chains (void return type):

```csharp
// BEFORE (11 lines)
private void MarkTargetFilled(PositionInfo pos, int targetNumber)
{
    switch (targetNumber)
    {
        case 1: pos.T1Filled = true; break;
        case 2: pos.T2Filled = true; break;
        case 3: pos.T3Filled = true; break;
        case 4: pos.T4Filled = true; break;
        case 5: pos.T5Filled = true; break;
    }
}

// AFTER (8 lines)
private void MarkTargetFilled(PositionInfo pos, int targetNumber)
{
    if (targetNumber == 1) pos.T1Filled = true;
    else if (targetNumber == 2) pos.T2Filled = true;
    else if (targetNumber == 3) pos.T3Filled = true;
    else if (targetNumber == 4) pos.T4Filled = true;
    else if (targetNumber == 5) pos.T5Filled = true;
}
```

### Methods to Refactor

| Method | Lines | Pattern | Return Type | Default Value |
|--------|-------|---------|-------------|---------------|
| GetTargetContracts | 277-294 | Getter | int | 0 |
| GetTargetPrice | 296-313 | Getter | double | 0.0 |
| IsTargetFilled | 315-332 | Getter | bool | false |
| MarkTargetFilled | 334-356 | Setter | void | N/A |
| GetTargetFilledQuantity | 358-375 | Getter | int | 0 |
| SetTargetFilledQuantity | 377-400 | Setter | void | N/A |

### Special Handling: SetTargetFilledQuantity

This method includes validation logic that must be preserved:

```csharp
// BEFORE (23 lines)
private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity)
{
    int safeQty = Math.Max(0, filledQuantity);
    switch (targetNumber)
    {
        case 1: pos.T1FilledQuantity = safeQty; break;
        case 2: pos.T2FilledQuantity = safeQty; break;
        case 3: pos.T3FilledQuantity = safeQty; break;
        case 4: pos.T4FilledQuantity = safeQty; break;
        case 5: pos.T5FilledQuantity = safeQty; break;
    }
}

// AFTER (9 lines)
private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity)
{
    int safeQty = Math.Max(0, filledQuantity);
    if (targetNumber == 1) pos.T1FilledQuantity = safeQty;
    else if (targetNumber == 2) pos.T2FilledQuantity = safeQty;
    else if (targetNumber == 3) pos.T3FilledQuantity = safeQty;
    else if (targetNumber == 4) pos.T4FilledQuantity = safeQty;
    else if (targetNumber == 5) pos.T5FilledQuantity = safeQty;
}
```

## V12 DNA Guardrails
- [ ] Zero new lock() statements (file already lock-free)
- [ ] Zero non-ASCII characters in string literals (no strings in scope)
- [ ] All methods maintain CYC < 15 (target: CYC ≈ 6 for all methods)
- [ ] Zero heap allocation (ternary operators compile to conditional branches)
- [ ] No logic drift -- pure structural transformation only
- [ ] API signatures unchanged (32 call sites work without modification)

## Post-Edit Verification (Mandatory)

### Step 1: Format & Build
```powershell
# Format code (adds missing braces, fixes line endings)
dotnet csharpier format src/V12_002.PositionInfo.cs

# Build verification
dotnet build
```

### Step 2: Re-establish Hard Links
```powershell
# MANDATORY after every src/ edit
powershell -File .\deploy-sync.ps1
```

### Step 3: Complexity Verification
```powershell
# Verify all 6 methods < 15 CYC
python scripts/complexity_audit.py
```

### Step 4: DNA Audits
```powershell
# Lock regression (must return ZERO)
grep -r "lock(" src/V12_002.PositionInfo.cs

# ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/V12_002.PositionInfo.cs
```

### Step 5: IL Inspection (Zero-Allocation Proof)
```powershell
# Compile in Release mode
dotnet build -c Release

# Inspect IL for one method (example: GetTargetContracts)
ildasm /text /item:V12_002.GetTargetContracts bin/Release/net8.0/V12_002.dll > il_output.txt

# Verify IL contains only: ldarg, ldc.i4, beq, br, ret
# NO newobj, NO box, NO heap allocations
```

### Step 6: F5 Verification
```powershell
# Open NinjaTrader
# Press F5 to compile
# Verify BUILD_TAG banner visible
# No compilation errors
```

### Step 7: Live Session Smoke Test
```
1. Load V12_002 strategy in NinjaTrader simulator
2. Execute 1 OR trade with 5 targets
3. Verify all targets fill correctly
4. Check UI snapshot displays correct quantities/prices
5. Confirm no exceptions in NinjaTrader Output window
```

## Acceptance Criteria
- [ ] All 6 methods refactored (4 getters to expression-bodied, 2 setters to if-else)
- [ ] LOC reduction: 124 lines → 51 lines (59% reduction, 73 lines eliminated)
- [ ] CYC scores: All methods ≈ 6 (below Jane Street threshold of 15)
- [ ] deploy-sync.ps1 ASCII gate: PASS (81/81 files synced)
- [ ] complexity_audit.py: All methods < 15
- [ ] lock() audit: ZERO matches
- [ ] IL inspection: Zero heap allocations confirmed
- [ ] dotnet build: Zero errors
- [ ] F5 in NinjaTrader: BUILD_TAG banner visible
- [ ] Live session: All 5 targets fill correctly, UI displays accurate data
- [ ] 32 call sites work without modification (no compilation errors)

## Expected Metrics

### Before
- Total LOC: 124 lines (6 methods)
- Average LOC per method: 20.7 lines
- CYC scores: All ≈ 6

### After
- Total LOC: 51 lines (6 methods)
- Average LOC per method: 8.5 lines
- CYC scores: All ≈ 6 (maintained)
- LOC reduction: 73 lines (59%)

### Performance
- Zero heap allocation (verified via IL)
- Zero GC pressure
- Performance parity (JIT inlines identically to switch statements)

## Timeline
- Pre-verification: 5 minutes
- Refactoring: 15 minutes (6 methods × 2-3 min each)
- Post-verification: 10 minutes
- IL inspection: 5 minutes
- Live testing: 10 minutes
- **Total: 45 minutes**

## Risk Level
**LOW-MODERATE**

All risks mitigated per validation report:
- ✅ Ternary readability: Documented pattern, stable API
- ✅ Performance regression: IL inspection + benchmarking
- ✅ Compilation errors: Incremental refactoring with build verification
- ✅ Runtime exceptions: Live session smoke test
- ✅ Call site breakage: No API changes

## Rollback Plan
If any verification step fails:
1. `git checkout src/V12_002.PositionInfo.cs`
2. `dotnet build` (verify rollback successful)
3. Document failure in `docs/brain/EPIC-POSINFO/failure-analysis.md`
4. Return to Phase 2 (Analysis) to identify root cause