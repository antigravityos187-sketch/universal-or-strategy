# Phase 2: Architecture Plan - EPIC-CCN-068

## Method Analysis

### Current State
- **Method**: `SymmetryGuardOnMasterFill`
- **File**: `src/V12_002.Symmetry.cs`
- **Lines**: 258-325 (68 lines total)
- **Cyclomatic Complexity**: 14
- **Target Complexity**: ≤8 (Jane Street strict standard)

## Extraction Strategy

### Three Helper Methods (Target CYC ≤8 each)

#### 1. TryResolveDispatchContext (CYC: 5)
**Responsibility**: Resolve SymmetryDispatchContext from entry name or fallback

**Signature**:
```csharp
private SymmetryDispatchContext TryResolveDispatchContext(
    string entryName,
    PositionInfo masterPos,
    DateTime fillTimeUtc
)
```

#### 2. ResolveAnchorWithCAS (CYC: 4)
**Responsibility**: Execute lock-free CAS loop to resolve anchor price

**Signature**:
```csharp
private AnchorSnapshot ResolveAnchorWithCAS(
    SymmetryDispatchContext ctx,
    double averageFillPrice,
    int fillQty
)
```

#### 3. PublishAnchorResolution (CYC: 2)
**Responsibility**: Log and trigger follower resolution

**Signature**:
```csharp
private void PublishAnchorResolution(
    SymmetryDispatchContext ctx,
    AnchorSnapshot resolvedSnap
)
```

### Refactored Main Method (CYC: 3)

```csharp
private void SymmetryGuardOnMasterFill(
    string entryName,
    PositionInfo masterPos,
    double averageFillPrice,
    int fillQty,
    DateTime fillTimeUtc
)
{
    if (masterPos == null || masterPos.IsFollower || averageFillPrice <= 0 || fillQty <= 0)
        return;

    SymmetryDispatchContext ctx = TryResolveDispatchContext(entryName, masterPos, fillTimeUtc);
    if (ctx == null)
        return;

    AnchorSnapshot resolvedSnap = ResolveAnchorWithCAS(ctx, averageFillPrice, fillQty);
    PublishAnchorResolution(ctx, resolvedSnap);
}
```

## Lock-Free Validation ✅

- ✅ No lock() statements in any helper method
- ✅ CAS loop preserved in ResolveAnchorWithCAS
- ✅ Idempotent retry via IsResolved guard
- ✅ First-writer-wins CAS semantics

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- ✅ Main method: CYC 3
- ✅ TryResolveDispatchContext: CYC 5
- ✅ ResolveAnchorWithCAS: CYC 4
- ✅ PublishAnchorResolution: CYC 2

## Implementation Checklist

### Phase 3: Implementation
- [ ] Extract TryResolveDispatchContext method
- [ ] Extract ResolveAnchorWithCAS method
- [ ] Extract PublishAnchorResolution method
- [ ] Refactor main method to call helpers
- [ ] Verify CYC reduction: 14 → 3, 5, 4, 2
- [ ] Run dotnet build (zero errors)
- [ ] Run dotnet test (100% pass)

## Approval Gate

**Status**: APPROVED FOR IMPLEMENTATION

**Next Phase**: Phase 3 (Implementation) - Switch to v12-engineer mode

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-068
**Phase**: 2 (Architecture Planning)
