# Extraction Tickets: EPIC-CCN-068

## Overview
- **Epic**: EPIC-CCN-068
- **Method**: `SymmetryGuardOnMasterFill`
- **File**: `src/V12_002.Symmetry.cs`
- **Current CYC**: 14
- **Target CYC**: 3 (main) + 5 + 4 + 2 (helpers) = ≤8 per method
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours

---

## TICKET-1: Extract TryResolveDispatchContext

### Scope
- **Current Method**: `SymmetryGuardOnMasterFill` (lines 258-325)
- **Current CYC**: 14
- **Target CYC**: 5 (extracted helper)
- **Extraction**: Context resolution logic with fallback handling

### Responsibility
Resolve `SymmetryDispatchContext` from entry name or fallback to default context.

### Signature
```csharp
private SymmetryDispatchContext TryResolveDispatchContext(
    string entryName,
    PositionInfo masterPos,
    DateTime fillTimeUtc
)
```

### Implementation Steps
1. Extract context resolution logic from lines ~262-275
2. Create new private method with signature above
3. Handle null/empty entryName cases
4. Implement fallback to default context
5. Return null if resolution fails
6. Update main method to call helper
7. Verify CYC: main reduces by ~5 points

### Acceptance Criteria
- [ ] Helper method created with CYC ≤ 5
- [ ] Main method CYC reduced (14 → ~9)
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (logic preserved)
- [ ] Lock-free validation (no `lock()` statements)
- [ ] ASCII-only compliance verified

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.Symmetry.cs

# Complexity audit
python scripts/complexity_audit.py

# Test suite
dotnet test
```

---

## TICKET-2: Extract ResolveAnchorWithCAS

### Scope
- **Current Method**: `SymmetryGuardOnMasterFill` (after TICKET-1)
- **Current CYC**: ~9 (after TICKET-1)
- **Target CYC**: 4 (extracted helper)
- **Extraction**: Lock-free CAS loop for anchor price resolution

### Responsibility
Execute lock-free Compare-And-Swap loop to resolve anchor price atomically.

### Signature
```csharp
private AnchorSnapshot ResolveAnchorWithCAS(
    SymmetryDispatchContext ctx,
    double averageFillPrice,
    int fillQty
)
```

### Implementation Steps
1. Extract CAS loop logic from lines ~276-305
2. Create new private method with signature above
3. Preserve idempotent retry via `IsResolved` guard
4. Maintain first-writer-wins CAS semantics
5. Return resolved `AnchorSnapshot`
6. Update main method to call helper
7. Verify CYC: main reduces by ~4 points

### Acceptance Criteria
- [ ] Helper method created with CYC ≤ 4
- [ ] Main method CYC reduced (~9 → ~5)
- [ ] CAS loop semantics preserved
- [ ] Idempotent retry logic intact
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes
- [ ] Lock-free validation (CAS-only, no locks)

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.Symmetry.cs

# Complexity audit
python scripts/complexity_audit.py

# Test suite
dotnet test

# Lock-free scan
grep -n "lock(" src/V12_002.Symmetry.cs
```

---

## TICKET-3: Extract PublishAnchorResolution

### Scope
- **Current Method**: `SymmetryGuardOnMasterFill` (after TICKET-2)
- **Current CYC**: ~5 (after TICKET-2)
- **Target CYC**: 2 (extracted helper)
- **Extraction**: Logging and follower resolution trigger

### Responsibility
Log anchor resolution and trigger follower resolution workflow.

### Signature
```csharp
private void PublishAnchorResolution(
    SymmetryDispatchContext ctx,
    AnchorSnapshot resolvedSnap
)
```

### Implementation Steps
1. Extract logging/publish logic from lines ~306-320
2. Create new private method with signature above
3. Preserve log message format
4. Maintain follower resolution trigger
5. Update main method to call helper
6. Verify CYC: main reduces to ≤3

### Acceptance Criteria
- [ ] Helper method created with CYC ≤ 2
- [ ] Main method CYC reduced (~5 → 3)
- [ ] Final main method CYC ≤ 3 (Jane Street compliant)
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes
- [ ] Log output format preserved
- [ ] Follower resolution triggered correctly

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.Symmetry.cs

# Complexity audit (final verification)
python scripts/complexity_audit.py

# Test suite
dotnet test

# Pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Final Refactored Method (Target)

```csharp
private void SymmetryGuardOnMasterFill(
    string entryName,
    PositionInfo masterPos,
    double averageFillPrice,
    int fillQty,
    DateTime fillTimeUtc
)
{
    // Guard clause (CYC: 1)
    if (masterPos == null || masterPos.IsFollower || averageFillPrice <= 0 || fillQty <= 0)
        return;

    // Context resolution (CYC: 1)
    SymmetryDispatchContext ctx = TryResolveDispatchContext(entryName, masterPos, fillTimeUtc);
    if (ctx == null)
        return;

    // Anchor resolution (CYC: 1)
    AnchorSnapshot resolvedSnap = ResolveAnchorWithCAS(ctx, averageFillPrice, fillQty);
    
    // Publish resolution (CYC: 0)
    PublishAnchorResolution(ctx, resolvedSnap);
}
```

**Final CYC**: 3 (Jane Street compliant ✅)

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC 14 (single method)
- **After**: CYC 3 + 5 + 4 + 2 = 14 total (distributed across 4 methods)
- **Per-Method Max**: 5 (well below Jane Street threshold of 8)

### Lock-Free Validation
- ✅ No `lock()` statements in any method
- ✅ CAS loop preserved in `ResolveAnchorWithCAS`
- ✅ Idempotent retry semantics maintained

### Jane Street Compliance
- ✅ All methods ≤8 CYC (strict standard)
- ✅ Cognitive simplicity achieved
- ✅ Single-responsibility principle enforced

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-068
**Phase**: 4 (Ticket Generation)
**Status**: READY FOR EXECUTION
