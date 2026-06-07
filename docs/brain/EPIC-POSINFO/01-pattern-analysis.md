# Pattern Analysis: Switch-Based Field Accessor Pattern

**Epic**: EPIC-POSINFO  
**File**: `src/V12_002.PositionInfo.cs` (lines 277-400)  
**Date**: 2026-06-02

---

## Pattern Name

**"Switch-Based Field Accessor Pattern"** - Structural duplication across 6 accessor methods operating on the 5-target ladder system (T1-T5).

---

## Duplication Metrics

### Quantitative Analysis
- **Methods affected**: 6 (GetTargetContracts, GetTargetPrice, IsTargetFilled, MarkTargetFilled, GetTargetFilledQuantity, SetTargetFilledQuantity)
- **Lines of duplication**: 124 total lines (6 methods × ~18-23 lines each)
- **Structural duplication**: 72 lines of pure switch logic (6 methods × 12 lines each)
- **Cyclomatic complexity**: CYC=11 per method (below Jane Street threshold ≤15, above ideal ≤10)
- **CodeScene score**: Unknown (needs measurement post-refactor)

### Pattern Breakdown
Each method contains:
- 1 method signature (1 line)
- 1 switch statement header (1 line)
- 5 case blocks (10 lines: 5 × 2 lines each)
- 1 default case (2 lines)
- 1 closing brace (1 line)
- **Total**: 16-23 lines per method depending on validation logic

---

## Pattern Structure

### Example 1: Getter Pattern (GetTargetContracts)
```csharp
private int GetTargetContracts(PositionInfo pos, int targetNumber)
{
    switch (targetNumber)
    {
        case 1:
            return pos.T1Contracts;
        case 2:
            return pos.T2Contracts;
        case 3:
            return pos.T3Contracts;
        case 4:
            return pos.T4Contracts;
        case 5:
            return pos.T5Contracts;
        default:
            return 0;
    }
}
```

### Example 2: Setter Pattern (MarkTargetFilled)
```csharp
private void MarkTargetFilled(PositionInfo pos, int targetNumber)
{
    if (targetNumber < 1 || targetNumber > 5)
        return;
    switch (targetNumber)
    {
        case 1:
            pos.T1Filled = true;
            break;
        case 2:
            pos.T2Filled = true;
            break;
        case 3:
            pos.T3Filled = true;
            break;
        case 4:
            pos.T4Filled = true;
            break;
        case 5:
            pos.T5Filled = true;
            break;
    }
}
```

### Example 3: Setter with Validation (SetTargetFilledQuantity)
```csharp
private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity)
{
    if (targetNumber < 1 || targetNumber > 5)
        return;
    int safeQty = Math.Max(0, filledQuantity);
    switch (targetNumber)
    {
        case 1:
            pos.T1FilledQuantity = safeQty;
            break;
        case 2:
            pos.T2FilledQuantity = safeQty;
            break;
        case 3:
            pos.T3FilledQuantity = safeQty;
            break;
        case 4:
            pos.T4FilledQuantity = safeQty;
            break;
        case 5:
            pos.T5FilledQuantity = safeQty;
            break;
    }
}
```

---

## Structural Variations

### Variation 1: Return Type
- **int**: GetTargetContracts, GetTargetFilledQuantity (default: 0)
- **double**: GetTargetPrice (default: 0.0)
- **bool**: IsTargetFilled (default: false)
- **void**: MarkTargetFilled, SetTargetFilledQuantity (no default)

### Variation 2: Field Names
- **Contracts**: T1Contracts, T2Contracts, T3Contracts, T4Contracts, T5Contracts
- **Price**: Target1Price, Target2Price, Target3Price, Target4Price, Target5Price
- **Filled**: T1Filled, T2Filled, T3Filled, T4Filled, T5Filled
- **FilledQuantity**: T1FilledQuantity, T2FilledQuantity, T3FilledQuantity, T4FilledQuantity, T5FilledQuantity

### Variation 3: Validation Logic
- **None**: GetTargetContracts, GetTargetPrice, IsTargetFilled, GetTargetFilledQuantity
- **Range check**: MarkTargetFilled, SetTargetFilledQuantity (if targetNumber < 1 || targetNumber > 5)
- **Value sanitization**: SetTargetFilledQuantity (Math.Max(0, filledQuantity))

---

## Anti-Patterns to Avoid

### 1. Arrays/Dictionaries (Heap Allocation)
**Why banned**: Violates Jane Street zero-allocation principle for hot paths.

**Example of what NOT to do**:
```csharp
// ❌ BANNED - Causes heap allocation
private int[] _targetContracts = new int[5];
private int GetTargetContracts(PositionInfo pos, int targetNumber)
{
    return _targetContracts[targetNumber - 1];
}
```

**Rationale**: 
- Array indexing requires bounds checking (allocation)
- Dictionary lookups allocate on miss
- PR #14 specifically reverted from array-based accessors for this reason

### 2. Reflection (Performance + Allocation)
**Why banned**: Slow + allocates on every call.

**Example of what NOT to do**:
```csharp
// ❌ BANNED - Reflection allocates and is slow
private int GetTargetContracts(PositionInfo pos, int targetNumber)
{
    var propName = $"T{targetNumber}Contracts";
    var prop = typeof(PositionInfo).GetProperty(propName);
    return (int)prop.GetValue(pos);
}
```

**Rationale**:
- `GetProperty()` allocates
- `GetValue()` boxes value types
- 100x slower than direct field access

### 3. Struct Modification (Scope Creep)
**Why out of scope**: 100+ call sites affected.

**Example of what NOT to do**:
```csharp
// ❌ OUT OF SCOPE - Requires changing PositionInfo struct
public struct PositionInfo
{
    // Replacing 15 individual fields with an array
    public int[] TargetContracts; // ❌ Breaks 100+ call sites
}
```

**Rationale**:
- Changing `PositionInfo` field declarations requires updating 100+ call sites
- Violates V12.23 No Scope Creep Protocol
- Would require separate epic (EPIC-POSINFO-STRUCT)

### 4. "While We're Here" Fixes (Scope Creep)
**Why banned**: Violates V12.23 No Scope Creep Protocol.

**Example of what NOT to do**:
```csharp
// ❌ SCOPE CREEP - Fixing unrelated methods in same file
private int GetTargetContracts(PositionInfo pos, int targetNumber)
{
    // ... refactored logic ...
}

// ❌ Don't also refactor these unrelated methods:
private TargetMode GetTargetMode(int targetNumber) { ... }
private bool IsRunnerTarget(int targetNumber) { ... }
```

**Rationale**:
- ONE EPIC = ONE CONCERN
- Mixing concerns causes PR bloat and review complexity
- Separate epics for separate concerns

---

## Recommended Approach (To Be Determined in Phase 2)

**Deferred to `/epic-plan` phase** after Director confirmation of scope boundaries.

### Constraints for Planning Phase
1. **MUST preserve**: Zero-allocation hot paths
2. **MUST maintain**: Existing API surface (method signatures unchanged)
3. **MUST achieve**: CYC ≤ 10 per method
4. **MUST target**: CodeScene 10/10 score
5. **MUST NOT**: Introduce arrays, dictionaries, reflection, or struct changes

### Evaluation Criteria
- **Duplication reduction**: Eliminate 72 lines of structural duplication
- **Complexity reduction**: Lower CYC from 11 to ≤10
- **Performance**: Zero regression (maintain zero-allocation)
- **Maintainability**: Improve CodeScene score to 10/10

---

## Historical Context

### PR #14 (2026-05-22): Array Reversion
- **Before**: Array-based accessors (`_targetContracts[targetNumber - 1]`)
- **After**: Switch-based accessors (current implementation)
- **Reason**: AMAL harness detected heap allocation in hot paths
- **Result**: Zero-allocation compliance achieved ✅

### Current State (2026-06-02)
- **Allocation**: Zero ✅
- **Complexity**: CYC=11 (acceptable but improvable)
- **Duplication**: 72 lines of structural duplication (primary issue)
- **CodeScene**: Unknown (needs measurement)

---

## Next Steps

1. **Director Confirmation**: Verify scope boundaries in `00-scope.md`
2. **Phase 2 Planning**: Evaluate refactoring approaches in `/epic-plan`
3. **CodeScene Baseline**: Measure current score before refactoring
4. **Approach Selection**: Choose solution that meets all constraints
5. **Implementation**: Execute in Phase 4 with mandatory checkpointing

---

## References

- **Scope Document**: [`docs/brain/EPIC-POSINFO/00-scope.md`](./00-scope.md)
- **Source File**: [`src/V12_002.PositionInfo.cs`](../../src/V12_002.PositionInfo.cs) (lines 277-400)
- **V12.23 Protocol**: [`docs/brain/EPIC-13/09-pr12-failure-analysis.md`](../EPIC-13/09-pr12-failure-analysis.md)
- **Jane Street Intel**: [`docs/intel/jane-street/`](../../intel/jane-street/)