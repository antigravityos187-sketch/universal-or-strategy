# Extraction Tickets: EPIC-CCN-032

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 4 hours (1 hour per ticket)
- **Target Method**: RestoreCascadedTargets
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Current CYC**: 16
- **Target CYC**: 15 total (7+2+4+2)

---

## TICKET-1: Extract ShouldRestoreTarget (Target Filtering)

### Scope
- **Current Method**: `RestoreCascadedTargets`
- **Current CYC**: 16
- **Target CYC**: 2 (helper method)
- **Extraction**: Pure predicate for target snapshot filtering logic

### Implementation
1. Create private method `ShouldRestoreTarget(TargetSnapshot snap)`
2. Move lines 749-762 (target filtering logic) into new method
3. Return `true` if snapshot is valid AND order state is Cancelled OR Rejected
4. Replace inline logic in main method with `if (!ShouldRestoreTarget(snap)) continue;`
5. Verify method signature matches architecture plan

### Code Structure
```csharp
private bool ShouldRestoreTarget(TargetSnapshot snap)
{
    // Returns true only if snapshot is valid (not null, has order) 
    // AND order state is Cancelled OR Rejected
    // Filled targets are skipped (already executed)
}
```

### Acceptance Criteria
- [ ] Method complexity (CYC) = 2
- [ ] Pure predicate (no side effects)
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes (exact same filtering logic)
- [ ] Build succeeds (`powershell -File .\scripts\build_readiness.ps1`)
- [ ] Zero new Codacy issues
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] Hard-link sync completed (`powershell -File .\deploy-sync.ps1`)

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# 1. Complexity audit
python scripts/complexity_audit.py

# 2. Build readiness
powershell -File .\scripts\build_readiness.ps1

# 3. Run tests
dotnet test

# 4. Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## TICKET-2: Extract BuildRestoredTargetOrder (Order Construction)

### Scope
- **Current Method**: `RestoreCascadedTargets`
- **Current CYC**: 16 (after TICKET-1)
- **Target CYC**: 4 (helper method)
- **Extraction**: Order object construction with price rounding and signal naming

### Implementation
1. Create private method `BuildRestoredTargetOrder(TargetSnapshot snap, string entryName, OrderAction exitAction, string bracketOcoId, bool isFollower, Account executingAccount)`
2. Move lines 764-790 (order construction logic) into new method
3. Handle price rounding to tick size
4. Generate signal name (with SymmetryTrim for followers)
5. Create Order via Account.CreateOrder (follower) or direct construction
6. Return null if order creation fails
7. Replace inline logic in main method with method call

### Code Structure
```csharp
private Order BuildRestoredTargetOrder(
    TargetSnapshot snap, 
    string entryName, 
    OrderAction exitAction, 
    string bracketOcoId, 
    bool isFollower, 
    Account executingAccount)
{
    // 1. Round price to tick size
    // 2. Generate signal name (with SymmetryTrim for followers)
    // 3. Create Order via Account.CreateOrder (follower) or direct construction
    // 4. Returns null if order creation fails
}
```

### Acceptance Criteria
- [ ] Method complexity (CYC) = 4
- [ ] Handles both follower and managed account paths
- [ ] Price rounding logic preserved
- [ ] Signal naming logic preserved (SymmetryTrim)
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes
- [ ] Build succeeds (`powershell -File .\scripts\build_readiness.ps1`)
- [ ] Zero new Codacy issues
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] Hard-link sync completed (`powershell -File .\deploy-sync.ps1`)

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# 1. Complexity audit
python scripts/complexity_audit.py

# 2. Build readiness
powershell -File .\scripts\build_readiness.ps1

# 3. Run tests
dotnet test

# 4. Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## TICKET-3: Extract SubmitTargetOrder (Order Submission)

### Scope
- **Current Method**: `RestoreCascadedTargets`
- **Current CYC**: 16 (after TICKET-2)
- **Target CYC**: 2 (helper method)
- **Extraction**: Order submission branching logic (follower vs managed)

### Implementation
1. Create private method `SubmitTargetOrder(Order order, bool isFollower, Account executingAccount)`
2. Move lines 791-807 (submission logic) into new method
3. Branch on isFollower flag:
   - If true: call `executingAccount.Submit(new[] { order })`
   - If false: order already submitted via SubmitOrderUnmanaged
4. Return Order object (may be same or new instance)
5. Replace inline logic in main method with method call

### Code Structure
```csharp
private Order SubmitTargetOrder(Order order, bool isFollower, Account executingAccount)
{
    // Branches on isFollower flag
    // If true, calls executingAccount.Submit
    // If false, order is already submitted via SubmitOrderUnmanaged
}
```

### Acceptance Criteria
- [ ] Method complexity (CYC) = 2
- [ ] Handles both follower and managed submission paths
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes (exact same submission logic)
- [ ] Build succeeds (`powershell -File .\scripts\build_readiness.ps1`)
- [ ] Zero new Codacy issues
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] Hard-link sync completed (`powershell -File .\deploy-sync.ps1`)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
# 1. Complexity audit
python scripts/complexity_audit.py

# 2. Build readiness
powershell -File .\scripts\build_readiness.ps1

# 3. Run tests
dotnet test

# 4. Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## TICKET-4: Refactor Main Method (Orchestration)

### Scope
- **Current Method**: `RestoreCascadedTargets`
- **Current CYC**: 16 (after TICKET-3)
- **Target CYC**: 7 (main orchestration)
- **Extraction**: Replace extracted logic with helper method calls

### Implementation
1. Keep validation and state extraction inline (lines 717-748)
2. Replace target filtering logic with `ShouldRestoreTarget(snap)` call
3. Replace order construction logic with `BuildRestoredTargetOrder(...)` call
4. Replace submission logic with `SubmitTargetOrder(...)` call
5. Verify main method now only orchestrates (CYC 7)
6. Ensure foreach loop structure preserved

### Code Structure
```csharp
private void RestoreCascadedTargets(string entryName, TargetSnapshot[] capturedTargets)
{
    // 1. Early validation (null checks, position lookup)
    // 2. State extraction from PositionInfo
    // 3. Entry filled validation
    // 4. Foreach loop calling helper methods:
    //    - ShouldRestoreTarget(snap)
    //    - BuildRestoredTargetOrder(...)
    //    - SubmitTargetOrder(...)
}
```

### Acceptance Criteria
- [ ] Method complexity (CYC) = 7
- [ ] All helper methods integrated correctly
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes (exact same orchestration flow)
- [ ] Build succeeds (`powershell -File .\scripts\build_readiness.ps1`)
- [ ] Zero new Codacy issues
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] Hard-link sync completed (`powershell -File .\deploy-sync.ps1`)
- [ ] F5 in NinjaTrader smoke test passes
- [ ] Total method complexity: 7+2+4+2 = 15 (down from 16)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```powershell
# 1. Complexity audit (verify all methods)
python scripts/complexity_audit.py

# 2. Build readiness
powershell -File .\scripts\build_readiness.ps1

# 3. Run tests
dotnet test

# 4. Hard-link sync
powershell -File .\deploy-sync.ps1

# 5. F5 in NinjaTrader (manual smoke test)
```

---

## Rollback Strategy

### Per-Ticket Rollback
- Bob CLI checkpointing enabled via `.bob/settings.json`
- Use `/restore` command if issues arise during any ticket
- Incremental extraction allows per-step rollback

### Full Epic Rollback
- Use `git reset --hard` to restore point before EPIC-CCN-032
- Re-run `powershell -File .\deploy-sync.ps1` to sync hard-links
- Verify with `dotnet test` and F5 in NinjaTrader

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC 16 (single method)
- **After**: CYC 15 total (7+2+4+2)
- **Per-Method Max**: CYC 7 (well under threshold 8)
- **Test Path Reduction**: 99.77% (65,536 → 152 paths)

### V12 DNA Compliance
- ✅ Lock-free (zero lock() statements)
- ✅ ASCII-only (zero non-ASCII characters)
- ✅ Jane Street alignment (CYC ≤8 per method)
- ✅ Correctness by construction (pure predicates, type safety)

### PR Hygiene
- ✅ Diff size: ~2,500 chars (target <10k)
- ✅ Scope creep: None (single method extraction)
- ✅ Build readiness: No breaking changes

---

## Metadata
- **Epic ID**: EPIC-CCN-032
- **Phase**: 4.0 (Ticket Generation)
- **Ticket Count**: 4
- **Execution Model**: Sequential (strict dependency chain)
- **Estimated Total Effort**: 4 hours
- **Risk Level**: LOW (private method extraction, checkpointing enabled)
- **Next Phase**: 5.0 (Ticket Execution)
