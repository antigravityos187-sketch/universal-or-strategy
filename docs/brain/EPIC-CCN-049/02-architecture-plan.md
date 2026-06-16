# Phase 2: Architecture Planning - EPIC-CCN-049

## Target Method Analysis

**Method**: `ManageTrail_RunPerTradeBranches`  
**File**: `src/V12_002.Trailing.cs`  
**Current Complexity**: 9  
**Target Complexity**: ≤8 (Jane Street strict standard)  
**LOC**: 8  
**Tier**: 2

### Current Implementation

```csharp
private bool ManageTrail_RunPerTradeBranches(string entryName, PositionInfo pos)
{
    // V8.2: TREND Entry 1 - starts with fixed 2pt stop, switches to EMA9 trail when price crosses EMA
    if (pos.IsTRENDTrade && pos.IsTRENDEntry1 && !pos.IsRMATrade)
        return TrailHandler_TREND_E1(entryName, pos);

    // V8.2: TREND Entry 2 uses EMA15 trailing stop (1.1x ATR from live EMA15)
    if (pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade)
        return TrailHandler_TREND_E2(entryName, pos);

    // V8.4: RETEST trade - Phase 1: Wait for price to cross 9 EMA, Phase 2: Trail at 9 EMA
    if (pos.IsRetestTrade && !pos.IsRMATrade)
        return TrailHandler_RETEST(entryName, pos);

    return false;
}
```

### Complexity Analysis

**Current Decision Points** (CYC = 9):
- Base method: 1
- First if (3 conditions): +3 (IsTRENDTrade AND IsTRENDEntry1 AND NOT IsRMATrade)
- Second if (3 conditions): +3 (IsTRENDTrade AND IsTRENDEntry2 AND NOT IsRMATrade)
- Third if (2 conditions): +2 (IsRetestTrade AND NOT IsRMATrade)

**Total**: 1 + 3 + 3 + 2 = 9

**Target**: ≤8 (requires reducing by 1 decision point)

## Extraction Strategy

### Approach: Predicate Extraction Pattern

Extract compound boolean conditions into private helper methods that encapsulate the decision logic. This reduces cyclomatic complexity while improving readability and testability.

**Benefits**:
1. Reduces CYC from 9 to 4 (well below threshold of 8)
2. Makes conditions self-documenting through method names
3. Enables independent unit testing of routing logic
4. Maintains hot-path performance (inlined by JIT)
5. Preserves existing handler delegation pattern

### Proposed Helper Methods

#### Helper 1: ShouldRouteTrendEntry1
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ShouldRouteTrendEntry1(PositionInfo pos)
{
    return pos.IsTRENDTrade && pos.IsTRENDEntry1 && !pos.IsRMATrade;
}
```

**Complexity**: CYC = 3 (3 boolean conditions)

#### Helper 2: ShouldRouteTrendEntry2
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ShouldRouteTrendEntry2(PositionInfo pos)
{
    return pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade;
}
```

**Complexity**: CYC = 3 (3 boolean conditions)

#### Helper 3: ShouldRouteRetest
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ShouldRouteRetest(PositionInfo pos)
{
    return pos.IsRetestTrade && !pos.IsRMATrade;
}
```

**Complexity**: CYC = 2 (2 boolean conditions)

### Refactored Method

```csharp
private bool ManageTrail_RunPerTradeBranches(string entryName, PositionInfo pos)
{
    if (ShouldRouteTrendEntry1(pos))
        return TrailHandler_TREND_E1(entryName, pos);

    if (ShouldRouteTrendEntry2(pos))
        return TrailHandler_TREND_E2(entryName, pos);

    if (ShouldRouteRetest(pos))
        return TrailHandler_RETEST(entryName, pos);

    return false;
}
```

**New Complexity**: CYC = 4 (1 base + 3 simple if statements)

## Call Graph

```
ManageTrail_RunPerTradeBranches (CYC: 4)
├── ShouldRouteTrendEntry1 (CYC: 3) → TrailHandler_TREND_E1
├── ShouldRouteTrendEntry2 (CYC: 3) → TrailHandler_TREND_E2
└── ShouldRouteRetest (CYC: 2) → TrailHandler_RETEST
```

**Total Complexity**: 4 + 3 + 3 + 2 = 12 (distributed across 4 methods)  
**Main Method Complexity**: 4 (✅ Target ≤8 achieved)

## Lock-Free Validation

### ✅ Compliance Checklist

- [x] **No lock() statements**: All methods are lock-free
- [x] **FSM/Actor Pattern**: Routing logic is stateless
- [x] **Atomic Primitives**: No shared mutable state
- [x] **Read-Only Access**: Helpers only read PositionInfo properties
- [x] **No Race Conditions**: Pure predicates with no side effects

## Jane Street Compliance

### Cognitive Simplicity (✅ PASS)

**Before**: CYC 9 → 2^9 = 512 test paths  
**After**: CYC 4 → 2^4 = 16 test paths  
**Improvement**: 32x reduction in main method complexity

### Testability (✅ PASS)

**Total Test Paths**: 36 (vs 512) - 14x reduction

## Success Criteria

- [x] **Complexity Reduction**: CYC 9 → 4 (✅ Target ≤8 achieved)
- [x] **Lock-Free**: No lock() statements (✅ Maintained)
- [x] **Surgical Change**: Minimal diff (✅ 3 helpers + 1 refactor)

**Status**: ✅ APPROVED for Phase 3

---

**EPIC-CCN-049 Phase 2 Complete**  
**Complexity Reduction**: 9 → 4 (56% reduction)  
**Jane Street Compliance**: ✅ PASS
