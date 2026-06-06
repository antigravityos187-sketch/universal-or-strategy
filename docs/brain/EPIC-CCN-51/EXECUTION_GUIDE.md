# Epic: EPIC-CCN-51 -- Execution Guide

## How to Execute Tickets (Bob Edition)

For each ticket in sequence order:
1. Open a NEW Bob session (separate from this planning session)
2. Switch to /v12-engineer mode
3. Type: `/ticket docs/brain/EPIC-CCN-51/ticket-XX-[name].md`
4. Bob will execute the PLAN-THEN-EXECUTE protocol
5. Await [TICKET-COMPLETE] report
6. Director runs manual gates (F5, complexity_audit)
7. Confirm ticket done before opening next ticket session

## Ticket Sequence (Complexity-First)

### Ticket 1: Fleet Order Adoption
**File**: [`ticket-01-fleet-adoption.md`](docs/brain/EPIC-CCN-51/ticket-01-fleet-adoption.md)  
**Depends on**: NONE  
**CYC Impact**: 79 → 54 (25-point reduction)  
**Risk**: MEDIUM (nested loops, error handling)  
**Key Features**:
- Extracts lines 313-408 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Creates `AdoptFleetOrders()` method
- Applies FREEZE-PROOF pattern (`Account.All.ToArray()`)
- Preserves 8-way prefix classification logic
- Preserves try/catch per account for error isolation

**Sentinel Mitigations**: Gaps 6.1, 6.5, 6.6, 6.7

---

### Ticket 2: Prefix Classification Helper
**File**: [`ticket-02-prefix-helper.md`](docs/brain/EPIC-CCN-51/ticket-02-prefix-helper.md)  
**Depends on**: ticket-01 (must run after fleet adoption extraction)  
**CYC Impact**: 54 → 50 (4-point reduction)  
**Risk**: LOW (pure helper, no state mutations)  
**Key Features**:
- Extracts duplicated 8-way prefix classification logic
- Creates `ClassifyOrderByPrefix(string orderName)` method
- Returns dictionary key: "stop", "target1"-"target5", "entry", or null
- Eliminates code duplication between fleet and master adoption

**Sentinel Mitigations**: None (pure function)

---

### Ticket 3: Fleet Position Reconstruction
**File**: [`ticket-03-position-rebuild.md`](docs/brain/EPIC-CCN-51/ticket-03-position-rebuild.md)  
**Depends on**: ticket-02 (can run after helper extraction)  
**CYC Impact**: 50 → 40 (10-point reduction)  
**Risk**: MEDIUM (inline business logic)  
**Key Features**:
- Extracts lines 410-450 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Creates `RebuildFleetPositionFromEntry(Order entryOrder)` method
- Preserves MarketPosition determination logic
- Preserves entry price calculation (LimitPrice fallback to StopPrice)
- Preserves stop distance calculation

**Sentinel Mitigations**: None (pure function, reads from dictionaries)

---

### Ticket 4: Master Order Adoption
**File**: [`ticket-04-master-adoption.md`](docs/brain/EPIC-CCN-51/ticket-04-master-adoption.md)  
**Depends on**: ticket-02 (requires shared helper)  
**CYC Impact**: 40 → 25 (15-point reduction)  
**Risk**: LOW (similar to Ticket 1, but simpler - single account)  
**Key Features**:
- Extracts lines 452-480 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Creates `AdoptMasterOrders()` method
- Uses shared `ClassifyOrderByPrefix()` helper (eliminates duplication)
- Preserves `IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true)` state guard

**Sentinel Mitigations**: Gaps 6.1, 6.6

---

### Ticket 5: FSM Hydration Orchestration
**File**: [`ticket-05-fsm-orchestration.md`](docs/brain/EPIC-CCN-51/ticket-05-fsm-orchestration.md)  
**Depends on**: ticket-04 (must run after all adoptions complete)  
**CYC Impact**: 25 → ≤19 (final target, acceptable range 15-19)  
**Risk**: MEDIUM (error handling added - APPROVED SAFETY REPAIR)  
**Key Features**:
- Extracts lines 523-540 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Creates `OrchestrateFSMHydration()` method
- **NEW BEHAVIOR (APPROVED)**: Wraps `HydrateFSMsFromWorkingOrders()` in try/catch
- Sets `_orderAdoptionComplete = true` even on FSM hydration failure (prevents REAPER deadlock)
- Aligns with Jane Street "fail-safe defaults" principle

**Sentinel Mitigations**: Gaps 6.2, 6.3, 6.4

---

## Dependency Graph

```
ticket-01 (fleet adoption)
    ↓
ticket-02 (prefix helper) ← FOUNDATION for tickets 3 & 4
    ↓                ↓
ticket-03        ticket-04
(position)       (master)
    ↓                ↓
    └────────┬───────┘
             ↓
        ticket-05
    (FSM orchestration)
```

**Execution Order**: 1 → 2 → 3 → 4 → 5 (sequential, no parallelization)

---

## Epic Success Criteria

### Quantitative Metrics
- [ ] [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) CYC: 79 → ≤19 (76% reduction, 60 points)
- [ ] All 5 sub-methods created with CYC ≤20
- [ ] File CYC: Reduced by ~60 points
- [ ] All new methods: LOC ≥15 (extraction floor)
- [ ] Zero new lock() statements
- [ ] Zero Unicode characters
- [ ] 81/81 hard-linked files synchronized

### Qualitative Metrics
- [ ] Code duplication eliminated (fleet vs master adoption logic)
- [ ] Position reconstruction logic testable in isolation
- [ ] Error handling preserved (try/catch per account)
- [ ] FSM hydration failure handling added (REAPER deadlock prevention)
- [ ] Logging output enhanced (error details for FSM failures)
- [ ] REAPER auditing unaffected (no false positives)

### DNA Compliance
- [ ] All 5 tickets pass F5 compile gate
- [ ] All 5 tickets pass complexity audit
- [ ] All 5 tickets pass DNA audit (lock-free, ASCII-only)
- [ ] All 5 tickets pass deploy-sync.ps1 (hard-link integrity)
- [ ] Live session test passes (manual verification)
- [ ] BUILD_TAG incremented and committed

---

## Per-Ticket Verification Checklist

After EACH ticket execution, Director must verify:

### 1. Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**: CYC reduction matches ticket target.

### 2. Hard-Link Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: ASCII gate PASS, 81/81 files synchronized.

### 3. Lock Audit
```powershell
grep -r "lock(" src/
```
**Expected**: ZERO matches (no new lock statements).

### 4. ASCII Gate
```powershell
grep -Prn "[^\x00-\x7F]" src/
```
**Expected**: ZERO matches (no Unicode introduced).

### 5. F5 Compile Gate
**Action**: Press F5 in NinjaTrader IDE.  
**Expected**: Clean compile, BUILD_TAG banner visible.

---

## Rollback Plan

### Per-Ticket Rollback
If any ticket fails verification:
1. `git reset --hard HEAD~1` (undo last commit)
2. Review failure reason (compile error, CYC regression, DNA violation)
3. Fix issue in isolation
4. Re-run verification before committing

### Epic-Level Rollback
If epic must be abandoned:
1. `git checkout main` (return to clean state)
2. `git branch -D feature/src-epic-ccn-51` (delete epic branch)
3. Document failure reason in `docs/brain/EPIC-CCN-51/failure-analysis.md`
4. Re-plan with adjusted approach

---

## Final Verification (After All 5 Tickets)

### 1. Final Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**:
- [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309): CYC ≤19 ✅
- `AdoptFleetOrders()`: CYC ≤20 ✅
- `ClassifyOrderByPrefix()`: CYC ≤5 ✅
- `RebuildFleetPositionFromEntry()`: CYC ≤8 ✅
- `AdoptMasterOrders()`: CYC ≤15 ✅
- `OrchestrateFSMHydration()`: CYC ≤5 ✅

### 2. Final DNA Audit
```powershell
# Lock-free verification
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
# Expected: ZERO matches

# ASCII-only verification
grep -Prn "[^\x00-\x7F]" src/V12_002.SIMA.Lifecycle.cs
# Expected: ZERO matches

# Hard-link integrity
powershell -File .\deploy-sync.ps1
# Expected: PASS

# BUILD_TAG bump
# Expected: BUILD_TAG incremented in src/V12_002.cs
```

### 3. Live Session Test (Manual)
1. Start strategy in NinjaTrader with existing positions
2. Trigger broker reconnect (disconnect/reconnect data feed)
3. Verify REAPER does not fire false positives
4. Verify all orders are adopted correctly (check dictionaries)
5. Verify `activePositions` dictionary matches broker positions
6. Verify `_orderAdoptionComplete` flag is set

### 4. REAPER Consistency Test
After reconnect, verify REAPER detects:
- **Ghost Position Repair**: Create position with `actualQty=0`, `expectedQty!=0` - verify REAPER detects it
- **Critical Desync Flatten**: Create position with `actualQty!=0`, `expectedQty==0` after grace - verify REAPER flattens it
- **Naked Position Detection**: Create position with no working stop - verify REAPER detects it

### 5. FSM Hydration Error Handling Test (NEW)
- Manually corrupt an order name to trigger FSM hydration failure
- Verify flag is still set to true
- Verify REAPER remains enabled
- Verify REAPER fires repair cycle (no indefinite deadlock)

### 6. Dictionary Consistency Audit
After reconnect, verify:
- `entryOrders.Count` matches number of fleet entry orders in broker
- `stopOrders.Count` matches number of stop orders in broker
- `_followerBrackets.Count` matches number of FSMs created

---

## Sentinel Audit Verification

### Gap 6.1: Concurrent Dictionary Mutations
- [ ] All extracted methods called on strategy thread (Actor-serialized)
- [ ] All dictionary writes remain single-write operations
- [ ] No compound operations (read-modify-write) introduced

### Gap 6.2: REAPER Flag Timing
- [ ] Flag set ONLY after all 5 phases complete successfully
- [ ] Reconnect during extraction resets flag correctly
- [ ] FSM hydration failure sets flag (NEW BEHAVIOR - prevents deadlock)

### Gap 6.3: FSM Hydration Failure Cascade
- [ ] Try/catch added around `HydrateFSMsFromWorkingOrders()` call
- [ ] Flag set even on FSM failure (enables REAPER repair)
- [ ] Detailed error logging for FSM failures

### Gap 6.4: REAPER Consistency
- [ ] Sequential execution order preserved (Phase 1 → 2 → 3 → 4 → 5)
- [ ] FSM consistency maintained (`_followerBrackets` populated after `entryOrders`)
- [ ] Stop order availability preserved (populated before REAPER runs)

### Gap 6.5: Unbounded Loop Latency
- [ ] FREEZE-PROOF pattern applied (`Account.All.ToArray()`)
- [ ] Latency benchmark: <100ms for 50 accounts, <500ms for 100 accounts
- [ ] Collection modification test: No `InvalidOperationException` on reconnect

### Gap 6.6: Lock-Free Guarantee
- [ ] Zero new lock() statements
- [ ] All extracted methods preserve Actor serialization
- [ ] All dictionary writes remain single-threaded

### Gap 6.7: Bounded-Latency Principle
- [ ] Latency budget documented (cold path, not hot path)
- [ ] Acceptable deviation for startup/reconnect operations
- [ ] Production monitoring plan for latency >500ms

---

## Notes for Director

### Behavior Change (APPROVED 2026-06-06)
**Ticket 5** introduces an intentional behavior change:
- **Previous**: FSM hydration failure left `_orderAdoptionComplete = false`, REAPER disabled indefinitely
- **New**: FSM hydration failure sets `_orderAdoptionComplete = true`, REAPER enabled, REAPER repairs desync
- **Rationale**: Jane Street "fail-safe defaults" principle - systems should degrade gracefully, not deadlock
- **Classification**: VALID SAFETY REPAIR discovered during Sentinel Audit (Gap 6.3)

### Complexity Target Adjustment
**Residual method CYC target**: ≤19 (acceptable range 15-19)
- Original target was ≤15, adjusted during Phase 3 validation
- Residual method includes a foreach loop (lines 225-228 in approach) which adds CYC
- If residual exceeds 19, the position rebuild loop will be extracted to a separate method

### Execution Time Estimate
- **Total**: 6-8 hours (1-1.5 hours per ticket including verification)
- **Ticket 1**: 1.5 hours (highest complexity)
- **Ticket 2**: 1 hour (simple helper)
- **Ticket 3**: 1.5 hours (business logic extraction)
- **Ticket 4**: 1 hour (similar to Ticket 1, simpler)
- **Ticket 5**: 1.5 hours (error handling + testing)

### Critical Success Factors
1. **Sequential Execution**: Tickets MUST be executed in order (dependencies)
2. **Verification After Each**: F5 + complexity audit after EVERY ticket
3. **No Scope Creep**: Stick to extraction only, no logic improvements
4. **FREEZE-PROOF Pattern**: Ticket 1 MUST use `Account.All.ToArray()`
5. **Error Handling**: Ticket 5 MUST wrap FSM hydration in try/catch