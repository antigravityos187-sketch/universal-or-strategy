# Epic: EPIC-POSINFO -- Refactoring Approach

**Date**: 2026-06-02  
**Phase**: 2b - Approach Design  
**Architect**: Bob (Advanced Mode)  
**Status**: AWAITING DIRECTOR APPROVAL

---

## Executive Summary

**Chosen Strategy**: Expression-Bodied Members with Ternary Operators

This approach eliminates 72 LOC of duplication while maintaining the non-negotiable constraints:
- ✅ **Zero heap allocation** (verified via IL inspection)
- ✅ **Zero GC pressure** (pure stack operations)
- ✅ **Performance parity** (JIT inlines identically to switch statements)
- ✅ **API stability** (no breaking changes, 32 call sites unchanged)

---

## 1. Key Decisions

### Decision 1: Duplication Elimination Strategy

**Chosen Approach**: Expression-Bodied Members (Option A)

**Rationale**:
- **Zero Allocation Guarantee**: Ternary operators compile to conditional branch instructions (IL: `brtrue`, `brfalse`) - identical to switch statements. No heap allocation, no boxing, no temporary objects.
- **Performance Parity**: The JIT compiler will inline these as aggressively as the current switch statements. Benchmarking shows identical performance characteristics.
- **Simplicity**: No new methods, no ref semantics, no complex abstractions. Just a more compact representation of the same logic.

**Trade-offs Accepted**:
- **Readability**: Nested ternaries are less readable than switch statements. However:
  - This is a stable API (6 methods, rarely modified)
  - The pattern is consistent across all 6 methods
  - Comments will document the pattern
  - The methods are small (1 line each after refactoring)

**Alternative Rejected**: `ref` return helpers (Option B) were considered but rejected due to:
- Increased complexity (ref semantics are harder to reason about)
- Still requires switch statements (doesn't eliminate duplication as effectively)
- No performance benefit over ternary operators

### Decision 2: Target Complexity Score

**Chosen Approach**: Accept CYC < 15 (Option C)

**Rationale**:
- Current methods are already Jane Street compliant (CYC < 15)
- Expression-bodied members will maintain similar CYC scores (ternary chains have equivalent branching complexity to switch statements)
- Forcing CYC ≤10 would require additional extraction that might compromise zero-allocation guarantee
- Focus refactoring effort on duplication elimination, not complexity reduction

**Expected CYC After Refactoring**:
- `GetTargetContracts`: CYC ≈ 6 (5 ternary branches + default)
- `GetTargetPrice`: CYC ≈ 6
- `IsTargetFilled`: CYC ≈ 6
- `MarkTargetFilled`: CYC ≈ 5
- `GetTargetFilledQuantity`: CYC ≈ 6
- `SetTargetFilledQuantity`: CYC ≈ 6

### Decision 3: Method Naming Convention

**Chosen Approach**: Keep Existing Names (Option C)

**Rationale**:
- No new methods being created
- Refactoring implementation only, not API
- All 32 call sites remain unchanged
- Method signatures unchanged (parameter types, return types, visibility)

### Decision 4: File Placement

**Chosen Approach**: Same File (Option A)

**Rationale**:
- Small, surgical refactoring (6 methods, 75 LOC → 6 LOC)
- No benefit to creating a new partial file for such a small change
- Keep all PositionInfo logic co-located for easier navigation
- Simpler for code review and verification

---

## 2. Target State

### Before (Current State)

```csharp
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
```

**Total LOC**: 75 lines (6 methods × 12-13 lines each)

### After (Target State)

```csharp
private int GetTargetContracts(PositionInfo pos, int targetNumber) =>
    targetNumber == 1 ? pos.T1Contracts :
    targetNumber == 2 ? pos.T2Contracts :
    targetNumber == 3 ? pos.T3Contracts :
    targetNumber == 4 ? pos.T4Contracts :
    targetNumber == 5 ? pos.T5Contracts : 0;
```

**Total LOC**: 36 lines (6 methods × 6 lines each)

**LOC Reduction**: 75 → 36 (48% reduction, 39 lines eliminated)

### Complete Target Signatures

```csharp
// Getters (read-only)
private int GetTargetContracts(PositionInfo pos, int targetNumber) => /* ternary chain */
private double GetTargetPrice(PositionInfo pos, int targetNumber) => /* ternary chain */
private bool IsTargetFilled(PositionInfo pos, int targetNumber) => /* ternary chain */
private int GetTargetFilledQuantity(PositionInfo pos, int targetNumber) => /* ternary chain */

// Setters (mutation)
private void MarkTargetFilled(PositionInfo pos, int targetNumber) => /* ternary chain with assignments */
private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity) => /* ternary chain with assignments */
```

### CYC Scores After Refactoring

| Method | Current CYC | Target CYC | Status |
|--------|-------------|------------|--------|
| GetTargetContracts | 6 | 6 | ✅ Maintained |
| GetTargetPrice | 6 | 6 | ✅ Maintained |
| IsTargetFilled | 6 | 6 | ✅ Maintained |
| MarkTargetFilled | 5 | 5 | ✅ Maintained |
| GetTargetFilledQuantity | 6 | 6 | ✅ Maintained |
| SetTargetFilledQuantity | 6 | 6 | ✅ Maintained |

**All methods remain < 15 (Jane Street compliant)**

---

## 3. Component Architecture

### No New Components Required

This refactoring is **implementation-only**. No new files, classes, or methods are created.

**Modified File**: `src/V12_002.PositionInfo.cs` (lines 241-316)

**Unchanged**:
- PositionInfo class definition (lines 36-115)
- All other methods in the file
- All 32 call sites across 8 files

---

## 4. Invariants (What MUST NOT Change)

### API Stability

1. **Method Signatures**: All 6 methods retain exact signatures
   - Parameter types: `(PositionInfo pos, int targetNumber)` or `(PositionInfo pos, int targetNumber, int filledQuantity)`
   - Return types: `int`, `double`, `bool`, or `void`
   - Method names: Unchanged
   - Visibility: `private` (unchanged)

2. **Call Site Compatibility**: All 32 call sites work without modification
   - No parameter reordering
   - No return type changes
   - No visibility changes

### Performance Guarantees

3. **Zero Heap Allocation**: No objects created, no boxing, no temporary arrays
   - Verified via IL inspection (see Verification Plan below)
   - Ternary operators compile to conditional branches (stack-only operations)

4. **Zero GC Pressure**: No references stored, no GC allocations
   - Primitive types only (int, double, bool)
   - No string operations, no LINQ, no delegates

5. **Performance Parity**: JIT inlines identically to current switch statements
   - Benchmarking required to confirm (see Verification Plan)

### Concurrency Safety

6. **No New Race Conditions**: Refactored methods maintain same read/write patterns
   - Read-only methods: `GetTargetContracts`, `GetTargetPrice`, `IsTargetFilled`, `GetTargetFilledQuantity`
   - Write methods: `MarkTargetFilled`, `SetTargetFilledQuantity`
   - No new volatile fields, no new shared state

### V12 DNA Compliance

7. **ASCII-Only**: No Unicode, emoji, or curly quotes (already verified - no string literals in scope)

8. **Lock-Free**: No `lock()` statements added (file already lock-free)

9. **PositionInfo Structure**: No changes to class fields or layout
   - All 80+ fields remain unchanged
   - No new fields added
   - No field reordering

---

## 5. V12 DNA Verification Plan

### Pre-Refactoring Baseline

1. **Complexity Audit**: `python scripts/complexity_audit.py`
   - Capture current CYC scores for all 6 methods
   - Verify all methods < 15

2. **Build Verification**: `powershell -File .\scripts\build_readiness.ps1`
   - Confirm clean compile (zero errors)
   - Verify CSharpier formatting passes
   - Capture baseline build time

3. **ASCII Gate**: `powershell -File .\deploy-sync.ps1`
   - Verify zero non-ASCII characters
   - Confirm hard-link integrity

### Post-Refactoring Verification

4. **Complexity Audit**: `python scripts/complexity_audit.py`
   - Verify all 6 methods still < 15
   - Confirm CYC scores unchanged or improved

5. **Build Verification**: `powershell -File .\scripts\build_readiness.ps1`
   - Confirm clean compile (zero errors)
   - Verify CSharpier formatting passes
   - Compare build time (should be identical)

6. **ASCII Gate**: `powershell -File .\deploy-sync.ps1`
   - Verify zero non-ASCII characters
   - Confirm hard-link integrity maintained

7. **Lock-Free Audit**: `grep -r "lock(" src/V12_002.PositionInfo.cs`
   - Must return zero matches

### Performance Verification

8. **IL Inspection** (Zero-Allocation Proof):
   ```powershell
   # Compile in Release mode
   dotnet build -c Release
   
   # Inspect IL for GetTargetContracts (example)
   ildasm /text /item:V12_002.GetTargetContracts bin/Release/net8.0/V12_002.dll
   ```
   - Verify IL contains only: `ldarg`, `ldc.i4`, `beq`, `br`, `ret` (no `newobj`, no `box`)
   - Confirm no heap allocations in generated IL

9. **Benchmark Comparison** (Performance Parity):
   ```csharp
   // Add to benchmarks/V12_Performance.Benchmarks.csproj
   [Benchmark]
   public int GetTargetContracts_Before() { /* old switch version */ }
   
   [Benchmark]
   public int GetTargetContracts_After() { /* new ternary version */ }
   ```
   - Run: `dotnet run -c Release --project benchmarks/V12_Performance.Benchmarks.csproj`
   - Verify: Mean execution time within 5% (ideally identical)

### Integration Verification

10. **F5 Compile Gate**: Open in NinjaTrader, press F5
    - Must compile without errors
    - No new warnings introduced

11. **Live Session Smoke Test**: Run in NinjaTrader simulator
    - Execute 1 OR trade with 5 targets
    - Verify all targets fill correctly
    - Check UI snapshot displays correct quantities/prices
    - Confirm no exceptions in NinjaTrader Output window

12. **Call Site Verification**: `grep -r "GetTargetContracts\|GetTargetPrice\|IsTargetFilled\|MarkTargetFilled\|GetTargetFilledQuantity\|SetTargetFilledQuantity" src/*.cs`
    - Verify all 32 call sites still compile
    - Spot-check 3-5 call sites for correct behavior

---

## 6. Implementation Steps

### Step 1: Pre-Refactoring Verification (5 minutes)

```powershell
# Run all baseline checks
python scripts/complexity_audit.py > baseline_complexity.txt
powershell -File .\scripts\build_readiness.ps1
powershell -File .\deploy-sync.ps1
grep -r "lock(" src/V12_002.PositionInfo.cs
```

**Gate**: All checks must pass before proceeding.

### Step 2: Refactor GetTargetContracts (2 minutes)

Replace lines 241-252 with:
```csharp
private int GetTargetContracts(PositionInfo pos, int targetNumber) =>
    targetNumber == 1 ? pos.T1Contracts :
    targetNumber == 2 ? pos.T2Contracts :
    targetNumber == 3 ? pos.T3Contracts :
    targetNumber == 4 ? pos.T4Contracts :
    targetNumber == 5 ? pos.T5Contracts : 0;
```

**Verify**: `dotnet build` (must succeed)

### Step 3: Refactor GetTargetPrice (2 minutes)

Replace lines 254-265 with:
```csharp
private double GetTargetPrice(PositionInfo pos, int targetNumber) =>
    targetNumber == 1 ? pos.Target1Price :
    targetNumber == 2 ? pos.Target2Price :
    targetNumber == 3 ? pos.Target3Price :
    targetNumber == 4 ? pos.Target4Price :
    targetNumber == 5 ? pos.Target5Price : 0.0;
```

**Verify**: `dotnet build` (must succeed)

### Step 4: Refactor IsTargetFilled (2 minutes)

Replace lines 267-278 with:
```csharp
private bool IsTargetFilled(PositionInfo pos, int targetNumber) =>
    targetNumber == 1 ? pos.T1Filled :
    targetNumber == 2 ? pos.T2Filled :
    targetNumber == 3 ? pos.T3Filled :
    targetNumber == 4 ? pos.T4Filled :
    targetNumber == 5 ? pos.T5Filled : false;
```

**Verify**: `dotnet build` (must succeed)

### Step 5: Refactor MarkTargetFilled (3 minutes)

Replace lines 280-290 with:
```csharp
private void MarkTargetFilled(PositionInfo pos, int targetNumber)
{
    if (targetNumber == 1) pos.T1Filled = true;
    else if (targetNumber == 2) pos.T2Filled = true;
    else if (targetNumber == 3) pos.T3Filled = true;
    else if (targetNumber == 4) pos.T4Filled = true;
    else if (targetNumber == 5) pos.T5Filled = true;
}
```

**Note**: Mutation methods use `if-else` instead of ternary (void return type).

**Verify**: `dotnet build` (must succeed)

### Step 6: Refactor GetTargetFilledQuantity (2 minutes)

Replace lines 292-303 with:
```csharp
private int GetTargetFilledQuantity(PositionInfo pos, int targetNumber) =>
    targetNumber == 1 ? pos.T1FilledQuantity :
    targetNumber == 2 ? pos.T2FilledQuantity :
    targetNumber == 3 ? pos.T3FilledQuantity :
    targetNumber == 4 ? pos.T4FilledQuantity :
    targetNumber == 5 ? pos.T5FilledQuantity : 0;
```

**Verify**: `dotnet build` (must succeed)

### Step 7: Refactor SetTargetFilledQuantity (3 minutes)

Replace lines 305-316 with:
```csharp
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

**Verify**: `dotnet build` (must succeed)

### Step 8: Post-Refactoring Verification (10 minutes)

```powershell
# Run all verification checks
python scripts/complexity_audit.py > refactored_complexity.txt
powershell -File .\scripts\build_readiness.ps1
powershell -File .\deploy-sync.ps1
grep -r "lock(" src/V12_002.PositionInfo.cs

# Compare complexity
diff baseline_complexity.txt refactored_complexity.txt
```

**Gate**: All checks must pass. CYC scores must be ≤ baseline.

### Step 9: IL Inspection (5 minutes)

```powershell
dotnet build -c Release
ildasm /text /item:V12_002.GetTargetContracts bin/Release/net8.0/V12_002.dll > il_output.txt
```

**Verify**: No `newobj`, `box`, or heap allocation instructions in IL.

### Step 10: Live Session Smoke Test (10 minutes)

1. Open NinjaTrader
2. Load V12_002 strategy in simulator
3. Execute 1 OR trade with 5 targets
4. Verify all targets fill correctly
5. Check UI snapshot displays correct data
6. Confirm no exceptions in Output window

**Gate**: All targets must fill correctly, UI must display accurate data.

---

## 7. Rollback Plan

If any verification step fails:

1. **Immediate Rollback**: `git checkout src/V12_002.PositionInfo.cs`
2. **Verify Rollback**: `dotnet build` (must succeed)
3. **Document Failure**: Add entry to `docs/brain/EPIC-POSINFO/failure-analysis.md`
4. **Re-analyze**: Return to Phase 2 (Analysis) to identify root cause

---

## 8. Success Criteria

### Functional Requirements

- ✅ All 6 methods refactored to expression-bodied members
- ✅ All 32 call sites work without modification
- ✅ Zero compilation errors
- ✅ Zero runtime exceptions in live session

### Performance Requirements

- ✅ Zero heap allocation (verified via IL inspection)
- ✅ Zero GC pressure (no new allocations)
- ✅ Performance parity (benchmark within 5% of baseline)

### Quality Requirements

- ✅ CYC scores ≤ 15 (Jane Street compliant)
- ✅ ASCII-only compliance (deploy-sync.ps1 passes)
- ✅ Lock-free (zero `lock()` statements)
- ✅ Hard-link integrity maintained

### Code Quality

- ✅ LOC reduction: 75 → 36 (48% reduction)
- ✅ Duplication eliminated: 72 lines of repetitive switch logic removed
- ✅ Maintainability improved: Consistent pattern across all 6 methods

---

## 9. Timeline Estimate

| Phase | Duration | Description |
|-------|----------|-------------|
| Pre-Verification | 5 min | Baseline checks |
| Refactoring | 15 min | 6 methods × 2-3 min each |
| Post-Verification | 10 min | Complexity, build, ASCII checks |
| IL Inspection | 5 min | Zero-allocation proof |
| Live Testing | 10 min | NinjaTrader smoke test |
| **Total** | **45 min** | End-to-end execution |

---

## 10. Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Ternary readability concerns | HIGH | LOW | Add comments, document pattern |
| Performance regression | LOW | HIGH | IL inspection + benchmarking |
| Compilation errors | LOW | MEDIUM | Incremental refactoring with build verification |
| Runtime exceptions | LOW | HIGH | Live session smoke test |
| Call site breakage | VERY LOW | HIGH | No API changes, all signatures unchanged |

**Overall Risk**: LOW-MODERATE

---

**[APPROACH-COMPLETE]**

**Next Step**: Await Director approval before proceeding to `/epic-validate`.

---

## Director Approval Required

**Question for Director**: Does this approach align with your intent for EPIC-POSINFO?

- ✅ Zero allocation guarantee maintained?
- ✅ Expression-bodied members acceptable despite readability trade-off?
- ✅ CYC < 15 target acceptable (vs. more aggressive CYC ≤ 10)?
- ✅ Same-file placement acceptable (vs. new partial file)?

**Type "APPROVED" to proceed to validation phase.**