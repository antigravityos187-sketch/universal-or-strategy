# Extraction Tickets: EPIC-CCN-077

## Overview
- **Total Tickets**: 1
- **Execution Order**: Sequential (TICKET-1)
- **Estimated Effort**: 1 hour
- **Current Complexity**: 9
- **Target Complexity**: ≤8

## Analysis Summary

The `ProcessClientStream` method has already been partially refactored with helper methods:
- `ProcessClientStream_ReadChunk` - Handles network stream reading
- `ProcessClientStream_DecodeUtf8` - Handles UTF-8 decoding with error handling
- `ProcessClientStream_ExtractLines` - Handles line buffering and extraction
- `ProcessClientStream_DispatchLine` - Handles line dispatching

**Current Complexity Source**: The remaining complexity (CYC=9) comes from:
1. Main while loop condition (2 conditions: `isIpcRunning && client.Connected`)
2. Early continue/break logic after ReadChunk
3. Early break after DecodeUtf8
4. Conditional logic in ExtractLines result handling
5. Foreach loop for line processing

**Extraction Strategy**: Extract the "process lines batch" logic into a final helper method to reduce the orchestration complexity in the main method.

---

## TICKET-1: Extract Line Batch Processing Logic

### Scope
- **Current Method**: `ProcessClientStream`
- **Current CYC**: 9
- **Target CYC**: ≤8
- **Extraction**: Extract the lines array processing logic into `ProcessClientStream_ProcessLinesBatch`

### Implementation

1. **Create new helper method** `ProcessClientStream_ProcessLinesBatch`:
   ```csharp
   private void ProcessClientStream_ProcessLinesBatch(IpcClientSession session, string[] lines)
   {
       foreach (string line in lines)
       {
           ProcessClientStream_DispatchLine(session, line);
       }
   }
   ```

2. **Refactor ProcessClientStream** to use the new helper:
   ```csharp
   // Replace this block:
   foreach (string line in lines)
   {
       ProcessClientStream_DispatchLine(session, line);
   }
   
   // With this single call:
   ProcessClientStream_ProcessLinesBatch(session, lines);
   ```

3. **Verify complexity reduction**:
   - Run `python scripts/complexity_audit.py` to confirm CYC ≤8
   - The extraction removes the foreach loop from the main method
   - Expected final CYC: 7-8

### Acceptance Criteria
- [ ] New helper method `ProcessClientStream_ProcessLinesBatch` created
- [ ] Method signature: `private void ProcessClientStream_ProcessLinesBatch(IpcClientSession session, string[] lines)`
- [ ] ProcessClientStream refactored to call the new helper
- [ ] Method complexity reduced to ≤8 (verified with complexity_audit.py)
- [ ] All tests pass (`dotnet test`)
- [ ] No behavioral changes (identical input/output behavior)
- [ ] Build succeeds (`build_readiness.ps1`)
- [ ] Hard-link integrity maintained (`deploy-sync.ps1`)
- [ ] Pre-push validation passes (`pre_push_validation.ps1 -Fast`)
- [ ] No lock() statements introduced (verified with `grep -r "lock(" src/V12_002.UI.IPC.Server.cs`)
- [ ] ASCII-only compliance maintained (no Unicode/emoji)

### Dependencies
- None (single ticket)

### Verification Commands
```powershell
# 1. Complexity audit
python scripts/complexity_audit.py

# 2. Lock-free verification
grep -r "lock(" src/V12_002.UI.IPC.Server.cs

# 3. Build readiness
powershell -File .\scripts\build_readiness.ps1

# 4. Hard-link sync
powershell -File .\deploy-sync.ps1

# 5. Pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

### Risk Assessment
- **Risk Level**: LOW
- **Rationale**: 
  - Simple extraction of foreach loop into helper method
  - No changes to method signature or behavior
  - Minimal code movement (~3 lines)
  - Already has 4 helper methods - adding 5th follows established pattern

### Rollback Plan
- Use Bob CLI `/restore` command if issues arise
- Checkpointing enabled in `.bob/settings.json`

---

## Post-Implementation Notes

### Jane Street Alignment
This extraction aligns with Jane Street's cognitive simplicity principle:
- **Before**: CYC=9 - orchestration logic mixed with loop iteration
- **After**: CYC≤8 - pure orchestration, all details delegated to helpers
- **Benefit**: Easier to reason about under microsecond latency constraints

### Testing Strategy
- Existing tests should pass without modification
- No new tests required (behavioral equivalence)
- Consider adding TDD tests for the new helper method (optional)

### Code Review Checklist
- [ ] Verify no scope creep (only ProcessClientStream + new helper modified)
- [ ] Confirm CYC ≤8 achieved
- [ ] Validate Jane Street alignment (cognitive simplicity)
- [ ] Check diff size <10k characters
- [ ] Verify no whitespace mutations
- [ ] Confirm ASCII-only compliance
- [ ] Validate lock-free pattern preserved
