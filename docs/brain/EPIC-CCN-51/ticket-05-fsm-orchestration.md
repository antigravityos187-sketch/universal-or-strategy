---
# TICKET EPIC-CCN-51-05: FSM Hydration Orchestration
# Epic: EPIC-CCN-51
# Sequence: 5 of 5
# Depends on: ticket-04-master-adoption.md
---

## Objective
Extract FSM hydration orchestration from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) into a new method `OrchestrateFSMHydration()` with error handling to prevent REAPER deadlock, reducing CYC from 25 to ≤19 (final target).

## Scope
IN scope:
- Extract lines 523-540 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Create new private method `OrchestrateFSMHydration()` in same file
- Add try/catch around `HydrateFSMsFromWorkingOrders()` call (APPROVED SAFETY REPAIR)
- Set `_orderAdoptionComplete = true` flag even on FSM hydration failure (prevents REAPER deadlock)
- Add detailed error logging for FSM hydration failures

OUT of scope:
- Refactoring `HydrateFSMsFromWorkingOrders()` itself (CYC 72 - separate epic EPIC-CCN-52)
- Changes to FSM creation logic
- Changes to REAPER audit logic
- Fleet/master adoption (Tickets 1, 4)
- Position reconstruction (Ticket 3)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-51/01-analysis.md`](docs/brain/EPIC-CCN-51/01-analysis.md) -- Section "Phase 5: FSM Hydration Orchestration" (lines 94-103)
- Approach: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 2 "Sub-Methods to Create" (lines 193-213)
- Sentinel Mitigations: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 6.2, 6.3, 6.4

## Implementation Instructions

### Method Signature (from Approach Section 2, lines 196-200)
```csharp
/// <summary>
/// Orchestrates FSM hydration from adopted orders and sets completion flag.
/// Wraps FSM hydration in try/catch to prevent REAPER deadlock on failure.
/// BEHAVIOR CHANGE (APPROVED 2026-06-06): Sets flag even on FSM failure to enable REAPER repair.
/// Aligns with Jane Street "fail-safe defaults" principle - systems should degrade gracefully, not deadlock.
/// </summary>
private void OrchestrateFSMHydration()
```

### Extraction Steps

1. **Create new method** `OrchestrateFSMHydration()` in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) immediately after `AdoptMasterOrders()`

2. **Extract FSM hydration logic with error handling** (NEW BEHAVIOR - APPROVED):
   ```csharp
   private void OrchestrateFSMHydration()
   {
       try
       {
           // Delegate to existing FSM hydration method (CYC 72 - separate epic EPIC-CCN-52)
           HydrateFSMsFromWorkingOrders();
           
           // Success path: Set flag to enable REAPER auditing
           _orderAdoptionComplete = true;
           
           Print("[HYDRATE] FSM hydration complete, REAPER enabled");
       }
       catch (Exception ex)
       {
           // CRITICAL ERROR HANDLING (NEW BEHAVIOR - APPROVED 2026-06-06)
           Print($"[HYDRATE-ERROR] FSM hydration failed: {ex.Message}");
           Print($"[HYDRATE-ERROR] Stack trace: {ex.StackTrace}");
           
           // BEHAVIOR CHANGE: Set flag anyway to prevent REAPER deadlock
           // Rationale: REAPER will detect desync and repair (better than being disabled forever)
           // This aligns with Jane Street "fail-safe defaults" principle
           _orderAdoptionComplete = true;
           
           Print("[HYDRATE-RECOVERY] REAPER enabled despite FSM failure (will repair desync)");
           Print("[HYDRATE-RECOVERY] Adopted orders remain in dictionaries for REAPER audit");
       }
   }
   ```

3. **Update residual method** [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309):
   - Replace lines 523-540 with: `OrchestrateFSMHydration();`
   - Remove direct `_orderAdoptionComplete = true` assignment (now in orchestrator)
   - Preserve sequential execution order (after all adoptions and position rebuilds)

### V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (target ~30 lines with error handling)
- [ ] Residual method CYC target: 25 → ≤19 (acceptable range 15-19)
- [ ] APPROVED BEHAVIOR CHANGE: Error handling added (prevents REAPER deadlock)
- [ ] Flag setting logic moved to orchestrator (centralized control)

## Sentinel Audit Mitigations (Reference Section 6 of Approach)

### Gap 6.2: REAPER Flag Timing (CRITICAL)
**Mitigation**: 
1. **Reconnect Race Handling**: Extraction preserves existing behavior - if broker disconnects DURING extraction, flag reset at [`V12_002.Lifecycle.cs`](src/V12_002.Lifecycle.cs:767) line 767 will occur, and next reconnect will re-run full adoption sequence. This is CORRECT behavior (don't set flag on partial adoption).
2. **All-or-Nothing Semantics**: Extraction maintains sequential execution - if ANY phase fails (throws exception), flag is NOT set. This prevents REAPER from auditing against incomplete data.
3. **FSM Hydration Failure Handling**: NEW - wrap [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) in try/catch, set flag even on failure to prevent REAPER deadlock.

**Verification**:
- Unit test (if harness exists): Mock [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) to throw exception, verify flag is still set
- Live session test: Manually corrupt an order name to trigger FSM hydration failure, verify REAPER remains enabled and fires repair cycle
- Reconnect test: Trigger broker disconnect during `AdoptFleetOrders()`, verify flag remains false and re-adoption occurs on reconnect

### Gap 6.3: FSM Hydration Failure Cascade (CRITICAL)
**Mitigation**: 
1. **Error Handling Wrapper**: Add try/catch in `OrchestrateFSMHydration()` (see extraction steps above)
2. **Partial Success Handling**: On FSM hydration failure:
   - Adopted orders remain in dictionaries (partial success)
   - Flag is set to true (enables REAPER)
   - REAPER will detect desync between dictionaries and FSM state
   - REAPER will fire repair cycle to fix the inconsistency
3. **Failure Isolation**: Try/catch is ONLY around [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902), not entire method. If order adoption fails (Phases 1-4), exception propagates normally (correct behavior - don't enable REAPER on adoption failure).
4. **Logging**: Add detailed error logging to distinguish between adoption failure and FSM hydration failure.

**Verification**:
- Code review: Verify try/catch is ONLY around [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) call, not entire `OrchestrateFSMHydration()` method
- Exception test: Manually trigger FSM hydration failure (corrupt order name), verify:
  - Flag is set to true
  - REAPER remains enabled
  - REAPER fires repair cycle
  - No indefinite REAPER deadlock

### Gap 6.4: REAPER Consistency (HIGH-RISK)
**Mitigation**: 
1. **Sequential Execution Preservation**: Extraction maintains exact same execution order:
   - Phase 1: Populate `entryOrders`, `stopOrders`, `target1-5Orders`
   - Phase 2: Populate `activePositions` (reads `entryOrders`)
   - Phase 3: Populate master orders (same dictionaries)
   - Phase 4: Populate master position
   - Phase 5: Populate `_followerBrackets` via [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) (reads `entryOrders`)
2. **FSM Consistency**: `_followerBrackets` is populated AFTER `entryOrders` (Phase 5 reads Phase 1 output). This ordering is preserved by extraction.
3. **Stop Order Availability**: `stopOrders` is populated in Phase 1 (fleet) and Phase 3 (master), BEFORE REAPER runs.

**Verification**:
- REAPER consistency test: After epic completion, trigger reconnect and verify REAPER does not fire false positives:
  - Ghost position repair (actualQty=0, expectedQty!=0)
  - Critical desync flatten (actualQty!=0, expectedQty==0 after grace)
  - Naked position detection (position exists, no working stop)
- Dictionary consistency audit: After reconnect, verify:
  - `entryOrders.Count` matches number of fleet entry orders in broker
  - `stopOrders.Count` matches number of stop orders in broker
  - `_followerBrackets.Count` matches number of FSMs created

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] `OrchestrateFSMHydration()` method created in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs)
- [ ] Method wraps `HydrateFSMsFromWorkingOrkers()` in try/catch
- [ ] Flag `_orderAdoptionComplete` set to true in both success and failure paths
- [ ] Detailed error logging added for FSM hydration failures
- [ ] Original method [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) reduced to CYC ≤19 (from 25)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC: 25 → ≤19 (acceptable range 15-19)
- [ ] lock() audit: ZERO matches
- [ ] Behavior change documented in method comment (APPROVED 2026-06-06)
- [ ] Jane Street "fail-safe defaults" principle referenced in comment
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Critical Verification Tests

### Test Case 1: FSM Hydration Success Path
```csharp
// Given: All orders adopted successfully, FSM hydration succeeds
// Expected: _orderAdoptionComplete = true, REAPER enabled
// Verify: Print "[HYDRATE] FSM hydration complete, REAPER enabled"
```

### Test Case 2: FSM Hydration Failure Path (NEW BEHAVIOR)
```csharp
// Given: Orders adopted successfully, FSM hydration throws exception
// Expected: _orderAdoptionComplete = true (STILL SET), REAPER enabled
// Verify: Print "[HYDRATE-ERROR] FSM hydration failed: ..."
// Verify: Print "[HYDRATE-RECOVERY] REAPER enabled despite FSM failure"
// Verify: REAPER fires repair cycle (not disabled indefinitely)
```

### Test Case 3: Adoption Failure (Existing Behavior Preserved)
```csharp
// Given: AdoptFleetOrders() throws exception
// Expected: Exception propagates, _orderAdoptionComplete remains false
// Verify: OrchestrateFSMHydration() is NEVER called
// Verify: REAPER remains disabled (correct - incomplete adoption)
```

### Test Case 4: Reconnect During Adoption
```csharp
// Given: Broker disconnects during AdoptFleetOrders()
// Expected: Flag reset to false at V12_002.Lifecycle.cs line 767
// Verify: Next reconnect re-runs full adoption sequence
// Verify: OrchestrateFSMHydration() called again on reconnect
```

### Test Case 5: REAPER Repair After FSM Failure
```csharp
// Given: FSM hydration failed, flag set to true, REAPER enabled
// Expected: REAPER detects desync (dictionaries populated, FSMs missing)
// Verify: REAPER fires repair cycle
// Verify: Desync is corrected (FSMs created or positions flattened)
// Verify: No indefinite REAPER deadlock
```

### Test Case 6: Error Logging Completeness
```csharp
// Given: FSM hydration throws exception with message and stack trace
// Expected: Both message and stack trace logged
// Verify: Print "[HYDRATE-ERROR] FSM hydration failed: {ex.Message}"
// Verify: Print "[HYDRATE-ERROR] Stack trace: {ex.StackTrace}"
// Verify: Sufficient detail for debugging in production logs
```

## Behavior Change Documentation (APPROVED)

**Change**: Set `_orderAdoptionComplete = true` even when FSM hydration fails.

**Rationale**: 
- **Previous Behavior**: FSM hydration failure left flag false, REAPER disabled indefinitely (liveness violation)
- **New Behavior**: FSM hydration failure sets flag true, REAPER enabled, REAPER repairs desync automatically
- **Alignment**: Jane Street "fail-safe defaults" principle - systems should degrade gracefully, not deadlock
- **Discovery**: Sentinel Audit Gap 6.3 (Phase 2.3)
- **Approval**: Director approved 2026-06-06 under "fix all issues when touching a file" directive
- **Classification**: VALID SAFETY REPAIR, not scope creep

**Impact**:
- ✅ **Gain**: REAPER remains operational even on FSM hydration failure
- ✅ **Gain**: Desync is automatically repaired (REAPER detects and fixes)
- ✅ **Gain**: No indefinite REAPER deadlock (liveness guarantee)
- ⚠️ **Trade-off**: REAPER may fire false positives immediately after FSM failure (acceptable - will self-correct)