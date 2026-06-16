# Extraction Tickets: EPIC-CCN-041

## Overview
- **Epic**: EPIC-CCN-041
- **Target Method**: SymmetryGuardPruneDispatches
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: CYC=10
- **Target Complexity**: CYC≤8 (Jane Street standard)
- **Total Tickets**: 4 (3 extractions + 1 verification)
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 2-3 hours
- **Risk Level**: Low (incremental extraction with test verification)

---

## TICKET-1: Extract IsDispatchExpired Helper

### Scope
- **Current Method**: `SymmetryGuardPruneDispatches`
- **Current CYC**: 10
- **Target CYC After Extraction**: 8
- **Extraction**: TTL expiration logic into pure function

### Implementation
1. Create private method `IsDispatchExpired(SymmetryDispatchContext ctx, DateTime nowUtc)`
2. Move TTL expiration check logic: `(nowUtc - ctx.CreatedUtc).TotalSeconds > SymmetryDispatchTtlSec`
3. Replace inline check with method call in main method
4. Verify method signature:
   ```csharp
   private bool IsDispatchExpired(SymmetryDispatchContext ctx, DateTime nowUtc)
   {
       return (nowUtc - ctx.CreatedUtc).TotalSeconds > SymmetryDispatchTtlSec;
   }
   ```

### Acceptance Criteria
- [ ] Method `IsDispatchExpired` created with correct signature
- [ ] Method is private and pure (no side effects)
- [ ] Main method calls `IsDispatchExpired(ctx, nowUtc)`
- [ ] Complexity reduced: Main method CYC=8, Helper CYC=1
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (output identical)
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`

### Dependencies
- None (first ticket)

### Verification Command
```bash
python3 scripts/complexity_audit.py | grep -A 5 "SymmetryGuardPruneDispatches"
```

---

## TICKET-2: Extract HasActiveFollowers Helper

### Scope
- **Current Method**: `SymmetryGuardPruneDispatches`
- **Current CYC**: 8 (after TICKET-1)
- **Target CYC After Extraction**: 4
- **Extraction**: Active followers detection logic with early exit pattern

### Implementation
1. Create private method `HasActiveFollowers(string[] followers)`
2. Move nested loop logic that checks `activePositions.ContainsKey(followerId)`
3. Implement early return pattern (Jane Street preference):
   ```csharp
   private bool HasActiveFollowers(string[] followers)
   {
       foreach (var followerId in followers)
       {
           if (activePositions.ContainsKey(followerId))
           {
               return true; // Early exit on first match
           }
       }
       return false;
   }
   ```
4. Replace nested loop in main method with: `HasActiveFollowers(ctx.Followers)`
5. Ensure immutable snapshot: `ctx.Followers` is already string[] (immutable)

### Acceptance Criteria
- [ ] Method `HasActiveFollowers` created with correct signature
- [ ] Method is private and uses early exit pattern
- [ ] Lock-free: Uses `ConcurrentDictionary.ContainsKey()` (thread-safe read)
- [ ] Main method calls `HasActiveFollowers(ctx.Followers)`
- [ ] Complexity reduced: Main method CYC=4, Helper CYC=2
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (output identical)
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`

### Dependencies
- TICKET-1 must be completed first

### Verification Command
```bash
python3 scripts/complexity_audit.py | grep -A 5 "SymmetryGuardPruneDispatches"
grep -n "lock(" src/V12_002.Symmetry.Replace.cs  # Should return no matches
```

---

## TICKET-3: Extract ShouldRemoveDispatch Orchestrator

### Scope
- **Current Method**: `SymmetryGuardPruneDispatches`
- **Current CYC**: 4 (after TICKET-2)
- **Target CYC After Extraction**: 3
- **Extraction**: Removal decision logic that orchestrates the two helper methods

### Implementation
1. Create private method `ShouldRemoveDispatch(SymmetryDispatchContext ctx, DateTime nowUtc)`
2. Implement guard clause pattern with null check:
   ```csharp
   private bool ShouldRemoveDispatch(SymmetryDispatchContext ctx, DateTime nowUtc)
   {
       if (ctx == null)
       {
           return false; // Guard clause
       }
       
       if (IsDispatchExpired(ctx, nowUtc))
       {
           return true; // Remove if expired
       }
       
       if (ctx.AnchorId == null)
       {
           return true; // Remove if anchor unresolved
       }
       
       return !HasActiveFollowers(ctx.Followers); // Remove if no active followers
   }
   ```
3. Simplify main method to single if-statement:
   ```csharp
   foreach (var kvp in symmetryDispatchById.ToArray())
   {
       if (ShouldRemoveDispatch(kvp.Value, nowUtc))
       {
           symmetryDispatchById.TryRemove(kvp.Key, out _);
       }
   }
   ```

### Acceptance Criteria
- [ ] Method `ShouldRemoveDispatch` created with correct signature
- [ ] Method is private and orchestrates helper methods
- [ ] Guard clause prevents null dereference
- [ ] Main method simplified to: foreach + if + TryRemove
- [ ] Complexity reduced: Main method CYC=3, Orchestrator CYC=5
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes (output identical)
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Command
```bash
python3 scripts/complexity_audit.py | grep -A 5 "SymmetryGuardPruneDispatches"
```

---

## TICKET-4: Final Verification & Hard-Link Sync

### Scope
- **Verification**: Confirm all extractions meet V12 DNA and Jane Street standards
- **Sync**: Update NinjaTrader hard links
- **Documentation**: Update manifest.json

### Implementation
1. Run full complexity audit:
   ```bash
   python3 scripts/complexity_audit.py
   ```
2. Verify target complexity achieved:
   - Main method: CYC=3 ✓
   - IsDispatchExpired: CYC=1 ✓
   - HasActiveFollowers: CYC=2 ✓
   - ShouldRemoveDispatch: CYC=5 ✓
3. Run full test suite:
   ```bash
   dotnet test
   ```
4. Run build readiness check:
   ```bash
   powershell -File .\scripts\build_readiness.ps1
   ```
5. Sync NinjaTrader hard links:
   ```bash
   powershell -File .\deploy-sync.ps1
   ```
6. Run pre-push validation:
   ```bash
   powershell -File .\scripts\pre_push_validation.ps1
   ```
7. Update manifest.json:
   ```json
   {
     "phases": {
       "phase_4": {
         "status": "completed",
         "output": "04-tickets.md",
         "ticket_count": 4
       },
       "phase_5": {
         "status": "ready",
         "tickets_completed": 0,
         "tickets_total": 4
       }
     }
   }
   ```

### Acceptance Criteria
- [ ] Complexity audit shows CYC≤8 for all methods
- [ ] Main method CYC=3 (70% reduction from CYC=10)
- [ ] All helper methods CYC≤5
- [ ] All tests pass (100% pass rate)
- [ ] Build succeeds with zero errors
- [ ] Hard links synced successfully
- [ ] Pre-push validation passes all checks
- [ ] Manifest.json updated with Phase 4 completion
- [ ] No lock() statements in extracted code
- [ ] ASCII-only compliance verified

### Dependencies
- TICKET-1 must be completed
- TICKET-2 must be completed
- TICKET-3 must be completed

### Verification Commands
```bash
# Complexity verification
python3 scripts/complexity_audit.py | grep -A 10 "SymmetryGuardPruneDispatches"

# Lock-free verification
grep -r "lock(" src/V12_002.Symmetry.Replace.cs  # Should return no matches

# ASCII-only verification
grep -P "[^\x00-\x7F]" src/V12_002.Symmetry.Replace.cs  # Should return no matches

# Test verification
dotnet test --verbosity normal

# Build verification
dotnet build --no-incremental
```

---

## Execution Strategy

### Sequential Execution
1. **TICKET-1**: Extract IsDispatchExpired (reduces CYC 10→8)
2. **TICKET-2**: Extract HasActiveFollowers (reduces CYC 8→4)
3. **TICKET-3**: Extract ShouldRemoveDispatch (reduces CYC 4→3)
4. **TICKET-4**: Final verification and sync

### Rollback Plan
- Each ticket is independently revertable via git
- Bob CLI auto-checkpointing enabled
- Use `/restore` command if needed
- Git revert if tests fail after any ticket

### Risk Mitigation
- **Low Risk**: Incremental extraction with test verification after each step
- **Test Coverage**: Existing tests verify behavioral equivalence
- **Atomic Operations**: All mutations use ConcurrentDictionary atomic primitives
- **Immutable Snapshots**: No shared mutable state between helpers

---

## V12 DNA Compliance Checklist

### Correctness by Construction
- [x] Null safety enforced via guard clause
- [x] Immutable snapshots prevent race conditions
- [x] Pure functions have no side effects
- [x] Type safety maintained throughout

### Lock-Free Actor Pattern
- [x] Zero lock() blocks
- [x] Uses immutable snapshots (ToArray(), ctx.Followers)
- [x] Atomic operations only (ContainsKey, TryRemove)
- [x] FSM/Actor Enqueue pattern maintained

### ASCII-Only Compliance
- [x] No Unicode characters
- [x] No emoji or curly quotes
- [x] ASCII method names and comments

### Jane Street Alignment
- [x] Cognitive simplicity (CYC≤8)
- [x] Early exit pattern (HasActiveFollowers)
- [x] Single-pass iteration (microsecond-latency optimized)
- [x] Pure functions enable exhaustive testing

---

**Generated**: 2026-06-15T16:54:00Z
**Phase**: Phase 4 (Ticket Generation)
**Epic**: EPIC-CCN-041
**Status**: READY FOR PHASE 5 (Ticket Execution)
**Total Tickets**: 4
**Estimated Effort**: 2-3 hours
**Risk Level**: Low
