# Extraction Tickets: EPIC-CCN-062

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2 hours
- **Target Method**: ProcessFleetSlot
- **Current CYC**: 11
- **Target CYC**: 6 (≤8 Jane Street standard)

## TICKET-1: Extract HandleFleetDispatchError

### Scope
- **Current Method**: `ProcessFleetSlot`
- **Current CYC**: 11
- **Target CYC**: 8 (after this extraction)
- **Extraction**: Consolidate error handling logic from catch block into focused helper method

### Implementation
1. Create new private method `HandleFleetDispatchError` with signature:
   ```csharp
   private void HandleFleetDispatchError(Exception ex, string fleetEntryName, string accountName, string expectedKey, int reservedDelta, bool syncCleared)
   ```
2. Move catch block logic (lines containing conditional branches for syncCleared and reservedDelta) into new method
3. Replace catch block body in ProcessFleetSlot with single call to HandleFleetDispatchError
4. Preserve all error logging and rollback behavior
5. Apply CSharpier formatting: `dotnet csharpier format src/`

### Acceptance Criteria
- [ ] Method complexity reduced from 11 to 8 (verified via `python3 scripts/complexity_audit.py`)
- [ ] HandleFleetDispatchError contains 2 conditional branches (syncCleared, reservedDelta checks)
- [ ] All existing tests pass: `dotnet test`
- [ ] No behavioral changes (error handling identical to original)
- [ ] Build succeeds: `dotnet build`
- [ ] CSharpier formatting applied with zero issues
- [ ] No lock() statements introduced (lock-free validation)
- [ ] ASCII-only compliance maintained

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Complexity check
python3 scripts/complexity_audit.py

# Build verification
dotnet build

# Test verification
dotnet test

# Format check
dotnet csharpier check src/
```

---

## TICKET-2: Extract CleanupFleetDispatch

### Scope
- **Current Method**: `ProcessFleetSlot`
- **Current CYC**: 8 (after TICKET-1)
- **Target CYC**: 6 (final target)
- **Extraction**: Consolidate cleanup logic from finally block into focused helper method

### Implementation
1. Create new private method `CleanupFleetDispatch` with signature:
   ```csharp
   private void CleanupFleetDispatch(int poolSlotIndex)
   ```
2. Move finally block logic (conditional poolSlotIndex check and Interlocked.Decrement) into new method
3. Replace finally block body in ProcessFleetSlot with single call to CleanupFleetDispatch
4. Preserve atomic operation (Interlocked.Decrement on _pendingFleetDispatchCount)
5. Apply CSharpier formatting: `dotnet csharpier format src/`

### Acceptance Criteria
- [ ] Method complexity reduced from 8 to 6 (verified via `python3 scripts/complexity_audit.py`)
- [ ] CleanupFleetDispatch contains 1 conditional branch (poolSlotIndex >= 0 check)
- [ ] All existing tests pass: `dotnet test`
- [ ] No behavioral changes (cleanup identical to original)
- [ ] Build succeeds: `dotnet build`
- [ ] CSharpier formatting applied with zero issues
- [ ] Atomic operation preserved (Interlocked.Decrement)
- [ ] No lock() statements introduced (lock-free validation)
- [ ] ASCII-only compliance maintained

### Dependencies
- **TICKET-1 must be completed first** (sequential extraction to avoid merge conflicts)

### Verification Commands
```bash
# Complexity check (should show CYC=6 for ProcessFleetSlot)
python3 scripts/complexity_audit.py

# Build verification
dotnet build

# Test verification
dotnet test

# Format check
dotnet csharpier check src/

# Hard-link sync (MANDATORY after all extractions)
powershell -File .\deploy-sync.ps1
```

---

## Post-Extraction Validation

### After Both Tickets Complete
1. **Pre-Push Validation** (MANDATORY):
   ```bash
   powershell -File .\scripts\pre_push_validation.ps1
   ```
   - All 13 checks must pass
   - Focus on: Build, Tests, Formatting, Complexity, PR Hygiene

2. **Hard-Link Sync** (MANDATORY):
   ```bash
   powershell -File .\deploy-sync.ps1
   ```
   - Synchronizes NinjaTrader hard links
   - Verifies BUILD_TAG integrity

3. **Complexity Verification**:
   - ProcessFleetSlot: CYC ≤ 6 ✅
   - HandleFleetDispatchError: CYC ≤ 3 ✅
   - CleanupFleetDispatch: CYC ≤ 2 ✅

4. **Codacy Dashboard Check**:
   - Verify "Up to quality standards" status
   - Confirm complexity reduction reflected

### Success Metrics
- **Complexity Reduction**: 11 → 6 (45% reduction)
- **Jane Street Compliance**: CYC ≤ 8 ✅ (target: 6)
- **Lock-Free Validation**: PASS (no locks introduced)
- **Test Coverage**: 100% pass rate maintained
- **Build Status**: Zero errors
- **Formatting**: Zero CSharpier issues

---

## Risk Mitigation

### Low-Risk Extraction
- **Pure refactoring**: No logic changes
- **Clear boundaries**: Error handling and cleanup are isolated concerns
- **Existing tests**: Provide safety net for regression detection
- **Single file**: Minimal blast radius (src/V12_002.SIMA.Fleet.cs only)

### Rollback Plan
If issues arise:
1. Use Bob CLI `/restore` command to revert to checkpoint
2. Re-run pre-push validation to confirm clean state
3. Review extraction logic for edge cases
4. Consult architecture plan (02-architecture-plan.md) for guidance

---

## V12 DNA Compliance Checklist

### Lock-Free Actor Pattern
- [ ] No lock() statements in extracted methods
- [ ] Atomic operations preserved (Interlocked.Decrement)
- [ ] FSM pattern maintained (InitializeFollowerBracketFSM)

### ASCII-Only Compliance
- [ ] No Unicode characters in string literals
- [ ] No emoji or curly quotes

### Correctness by Construction
- [ ] Type-safe parameter passing
- [ ] Compiler-enforced contracts
- [ ] No runtime type checks needed

### Hard-Link Integrity
- [ ] deploy-sync.ps1 executed after all changes
- [ ] BUILD_TAG verified in NinjaTrader

---

**Phase 4 Ticket Generation**: COMPLETE
**Ready for Phase 5**: YES (execute tickets sequentially via Bob CLI)
