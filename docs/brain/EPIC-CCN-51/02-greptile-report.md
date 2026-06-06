# Epic: EPIC-CCN-51 -- Sentinel Audit (Semantic Scan)

## Executive Summary

**Verdict**: ⚠️ **REVISION REQUIRED**

The approach document (02-approach.md) is structurally sound but contains **3 CRITICAL gaps** and **2 HIGH-RISK omissions** that must be addressed before ticket generation. The semantic scan revealed hidden integration points, concurrent dictionary mutations, and REAPER dependencies not mentioned in the analysis.

---

## Semantic Gap Analysis

### Gap 1: 🔴 CRITICAL - Concurrent Dictionary Mutations (NOT MENTIONED)

**Finding**: The approach assumes `entryOrders`, `stopOrders`, `target1-5Orders`, and `activePositions` are only written by `HydrateWorkingOrdersFromBroker()`, but **10+ other methods** write to these dictionaries concurrently.

**Evidence** (Query 2 results):
- **`V12_002.Entries.FFMA.cs`** (lines 190-191, 327-328, 471-472): Writes to `entryOrders` and `activePositions` during FFMA entry execution
- **`V12_002.Entries.MOMO.cs`** (lines 161-162): Writes to `entryOrders` and `activePositions` during MOMO entry
- **`V12_002.Entries.OR.cs`** (lines 226-227): Writes to `entryOrders` and `activePositions` during OR entry
- **`V12_002.Entries.RMA.cs`** (line 145, 176): Writes to `entryOrders` and `activePositions` during RMA entry
- **`V12_002.Entries.Retest.cs`** (lines 176, 195, 316, 334): Writes to `entryOrders` and `activePositions` during Retest entry
- **`V12_002.Entries.Trend.cs`** (lines 328, 361, 669-670): Writes to `entryOrders` and `activePositions` during Trend entry
- **`V12_002.Orders.Callbacks.Propagation.cs`** (line 599): Writes to `entryOrders` during FSM propagation
- **`V12_002.Orders.Management.Flatten.cs`** (line 150): Writes to `entryOrders` during CIT management
- **`V12_002.Orders.Management.StopSync.cs`** (line 322): Writes to `stopOrders` during stop sync
- **`V12_002.Orders.Management.cs`** (lines 218, 246): Writes to `stopOrders` during stop creation
- **`V12_002.REAPER.Repair.cs`** (line 217): Writes to `entryOrders` during REAPER repair
- **`V12_002.SIMA.Dispatch.cs`** (lines 533-535, 694): Writes to `entryOrders`, `stopOrders`, and `activePositions` during fleet dispatch

**Risk**: The extracted methods will read/write these dictionaries while other methods are also mutating them. The approach document does NOT address:
1. **Race Condition Risk**: What happens if `AdoptFleetOrders()` writes to `entryOrders` while `PumpFleetDispatch()` is also writing?
2. **Ordering Invariants**: Several comments mention "dict BEFORE expectedPositions update" (line 694) - are there ordering constraints the extraction must preserve?
3. **Actor Serialization**: Some writes use `Enqueue(ctx => { ctx.entryOrders[...] = ...; })` (Actor model), others are direct writes. Does the extraction preserve this distinction?

**Approach Document Gap**: Section 4 (Invariants) mentions "Order adoption logic produces identical dictionary contents" but does NOT verify that the extraction preserves thread-safety or ordering constraints.

**Recommendation**: Add a new section to the approach document:
- **"Dictionary Mutation Safety"**: Document which methods write to these dictionaries, verify that the extraction does not introduce new race conditions, and confirm that all writes remain Actor-serialized (via `Enqueue`) or use `ConcurrentDictionary` single-write guarantees.

---

### Gap 2: 🔴 CRITICAL - REAPER Dependency on `_orderAdoptionComplete` Timing (INCOMPLETE)

**Finding**: The approach assumes `_orderAdoptionComplete` is only checked by REAPER, but the semantic scan reveals **2 additional integration points** not mentioned in the analysis.

**Evidence** (Query 1 results):
1. **`V12_002.REAPER.cs` line 143**: REAPER checks `!_orderAdoptionComplete` to skip auditing (DOCUMENTED in approach)
2. **`V12_002.Lifecycle.cs` line 767**: Flag is **RESET to false** on connection loss (`ConnectionStatus.Disconnecting || ConnectionStatus.ConnectionLost`) (NOT MENTIONED in approach)
3. **`V12_002.SIMA.Lifecycle.cs` line 312**: Comment warns: "Setting _orderAdoptionComplete=true while these are skipped leaves REAPER auditing against incomplete order tracking and can fire false repair cycles." (NOT MENTIONED in approach)

**Risk**: The approach document states (line 206): "Sets `_orderAdoptionComplete = true` flag" but does NOT address:
1. **Reconnect Race**: If the broker disconnects DURING extraction (between `AdoptFleetOrders()` and `OrchestrateFSMHydration()`), the flag is reset to false (line 767). Does the residual method handle this correctly?
2. **Partial Adoption Risk**: The comment at line 312 warns that setting the flag while orders are skipped causes false REAPER repairs. Does the extraction preserve the "all-or-nothing" semantics?
3. **Flag Timing**: The flag is set AFTER `HydrateFSMsFromWorkingOrders()` completes (line 289). If FSM hydration fails, the flag is never set. Does the extraction preserve this failure mode?

**Approach Document Gap**: Section 4 (Invariants) states "`_orderAdoptionComplete` flag set at the same point in execution" but does NOT verify that the extraction handles reconnect races or FSM hydration failures correctly.

**Recommendation**: Add to Section 4 (Invariants):
- **"Flag Timing Invariant"**: Verify that `_orderAdoptionComplete` is set ONLY after all 5 phases complete successfully, and that reconnect during extraction resets the flag correctly.
- Add a test case: "Trigger broker disconnect during `AdoptFleetOrders()` and verify flag remains false."

---

### Gap 3: 🔴 CRITICAL - FSM Hydration Failure Cascade (NOT ANALYZED)

**Finding**: The approach states (line 80): "Expanding scope violates V12.23 No Scope Creep Protocol" and leaves `HydrateFSMsFromWorkingOrders()` in place. However, the semantic scan reveals that **FSM hydration failure blocks the `_orderAdoptionComplete` flag**, leaving REAPER disabled indefinitely.

**Evidence** (Query 4 results):
- **`V12_002.SIMA.Lifecycle.cs` line 287**: `HydrateFSMsFromWorkingOrders()` is called BEFORE setting `_orderAdoptionComplete = true` (line 289)
- **`V12_002.SIMA.Lifecycle.cs` line 902**: `HydrateFSMsFromWorkingOrders()` is CYC 72 (separate epic EPIC-CCN-52)
- **No try/catch around FSM hydration call**: If `HydrateFSMsFromWorkingOrders()` throws an exception, the flag is never set, and REAPER remains disabled.

**Risk**: The approach document does NOT address:
1. **Failure Isolation**: If FSM hydration fails (e.g., due to a malformed order name), does the extraction ensure that adopted orders remain in dictionaries (partial success) or are they rolled back (all-or-nothing)?
2. **REAPER Deadlock**: If FSM hydration fails and the flag is never set, REAPER auditing is disabled indefinitely. Does the extraction add error handling to prevent this?
3. **Residual Method Responsibility**: The residual method (line 237 in approach) calls `OrchestrateFSMHydration()` without a try/catch. Should the extraction add one?

**Approach Document Gap**: Section 2 (Target State) shows the residual method calling `OrchestrateFSMHydration()` (line 237) but does NOT specify error handling. Section 4 (Invariants) states "FSM hydration failure behavior unchanged (exception propagation)" but does NOT verify that this is safe.

**Recommendation**: Add to Section 2 (Target State):
- **"Error Handling for FSM Hydration"**: Wrap `OrchestrateFSMHydration()` in a try/catch that logs the error and sets `_orderAdoptionComplete = true` anyway (to prevent REAPER deadlock). Document this as a behavior change (acceptable because it prevents a worse failure mode).
- Alternatively, document that FSM hydration failure is a fatal error and should propagate (current behavior), but add a comment explaining the REAPER deadlock risk.

---

## Integration Risks

### Risk 1: 🟡 HIGH - REAPER Audit Methods Depend on Dictionary Consistency

**Finding**: The semantic scan reveals that **8 REAPER audit methods** read from the dictionaries that `HydrateWorkingOrdersFromBroker()` writes to. The approach does NOT verify that the extraction preserves the ordering constraints these methods rely on.

**Evidence** (Query 5 results):
- **`V12_002.REAPER.Audit.cs`**:
  - `AuditFleet_CalculateExpectedActual()` (line 274): Reads `_followerBrackets` and calls `GetFsmExpectedPosition()`
  - `AuditFleet_HandleDesyncRepair()` (line 151): Reads `accountFsms` (derived from `_followerBrackets`)
  - `AuditFleet_CheckPositionPassGrace()` (line 192): Checks if FSMs were created during reconnect
  - `AuditFleet_HandleCriticalDesyncFlatten()` (line 223): Reads expected position from FSMs
  - `AuditFleet_HandleNakedPosition()` (line 251): Checks for working stop orders in `stopOrders` dictionary
  - `AuditFleet_CheckWorkingStop()` (line 388): Iterates `acct.Orders.ToArray()` to find working stops

**Risk**: The approach document does NOT address:
1. **FSM Consistency**: REAPER methods assume that `_followerBrackets` is consistent with `entryOrders`. Does the extraction preserve this invariant?
2. **Position Pass Grace**: `AuditFleet_CheckPositionPassGrace()` checks if FSMs were created during reconnect. Does the extraction affect this timing?
3. **Naked Position Detection**: `AuditFleet_HandleNakedPosition()` checks for working stops in `stopOrders`. Does the extraction ensure stops are adopted before REAPER runs?

**Approach Document Gap**: Section 2 (Target State) shows the residual method calling phases sequentially (lines 220-237) but does NOT verify that this ordering satisfies REAPER's consistency requirements.

**Recommendation**: Add to Section 5 (V12 DNA Verification Plan):
- **"REAPER Consistency Test"**: After epic completion, trigger a reconnect and verify that REAPER does not fire false positives. Specifically test:
  - Ghost position repair (actualQty=0, expectedQty!=0)
  - Critical desync flatten (actualQty!=0, expectedQty==0 after grace)
  - Naked position detection (position exists, no working stop)

---

### Risk 2: 🟡 HIGH - Account.All Iteration is Unbounded (LATENCY RISK)

**Finding**: The approach extracts nested loops over `Account.All` (line 140 in approach: "Outer loop: `foreach (Account acct in Account.All)`") but does NOT analyze the latency risk if the account count is large.

**Evidence** (Query 6 results):
- **20 matches** for `Account.All` iteration across 9 files
- **`V12_002.SIMA.Flatten.cs` line 51**: Uses `Account.All.ToArray()` snapshot to prevent collection modification during iteration (FREEZE-PROOF pattern)
- **`V12_002.SIMA.Fleet.cs` line 489**: Comment warns: "Build 1109 [FREEZE-PROOF]: Snapshot Account.All once to prevent InvalidOperationException if broker reconnects or modifies the collection during iteration."

**Risk**: The approach document does NOT address:
1. **Unbounded Loop**: If a user has 50+ accounts, the nested loop (`Account.All` → `acct.Orders`) could iterate 1000+ orders. Does this violate Jane Street's bounded-latency principle?
2. **Collection Modification**: The approach extracts the loop as-is (line 140: "`foreach (Account acct in Account.All)`") but does NOT use the FREEZE-PROOF pattern (`Account.All.ToArray()`). Is this safe?
3. **Latency Budget**: The approach does NOT specify a latency budget for the extraction. Should there be a timeout or max-account limit?

**Approach Document Gap**: Section 2 (Target State) shows `AdoptFleetOrders()` iterating `Account.All` (line 140) but does NOT mention the FREEZE-PROOF pattern or latency constraints.

**Recommendation**: Add to Section 2 (Target State):
- **"FREEZE-PROOF Pattern"**: Change `AdoptFleetOrders()` signature to use `Account.All.ToArray()` snapshot (line 140 should read: "`Account[] snapshot = Account.All.ToArray(); foreach (Account acct in snapshot)`").
- Add to Section 5 (V12 DNA Verification Plan): "Latency Test: Measure `AdoptFleetOrders()` execution time with 50 accounts and verify it completes in <100ms."

---

## DNA Violation Detection

### Violation 1: ⚠️ POTENTIAL - Lock-Free Guarantee Not Verified

**Finding**: The approach states (line 88): "No new `lock()` statements (method is already lock-free via Actor model)" but does NOT verify that the extracted methods preserve Actor serialization.

**Evidence** (Query 2 results):
- **Actor-serialized writes**: `Enqueue(ctx => { ctx.entryOrders[...] = ...; })` (e.g., line 599 in Propagation.cs)
- **Direct writes**: `entryOrders[key] = value;` (e.g., line 150 in Flatten.cs, line 217 in Repair.cs)
- **Comment at line 694 in Dispatch.cs**: "B966: Enqueue NOT applied -- ordering invariant: dict BEFORE expectedPositions update (Phantom-Fix). ConcurrentDictionary single-writes are thread-safe here."

**Risk**: The approach document does NOT specify:
1. **Actor Boundary**: Are the extracted methods called via `Enqueue()` or directly on the strategy thread?
2. **Dictionary Thread-Safety**: The approach assumes `ConcurrentDictionary` single-writes are safe, but does NOT verify that the extraction preserves this guarantee.
3. **Ordering Constraints**: Some writes have explicit ordering requirements (e.g., "dict BEFORE expectedPositions"). Does the extraction preserve these?

**Approach Document Gap**: Section 4 (Invariants) states "No FSM state mutations during order adoption (read-only access to FSM dictionaries)" but does NOT verify that the extracted methods are Actor-serialized.

**Recommendation**: Add to Section 5 (V12 DNA Verification Plan):
- **"Actor Serialization Audit"**: Verify that all extracted methods are called from the strategy thread (not background threads) and that all dictionary writes remain single-threaded.
- Add a comment to each extracted method: "// ACTOR-SERIALIZED: Must be called via Enqueue() or on strategy thread."

---

### Violation 2: ⚠️ POTENTIAL - Bounded-Latency Principle Not Enforced

**Finding**: The approach extracts nested loops with no latency bounds, which may violate Jane Street's "bounded-latency" principle for HFT systems.

**Evidence** (Query 6 results):
- **Nested loops**: `foreach (Account acct in Account.All)` → `foreach (Order ord in acct.Orders.ToArray())`
- **No timeout**: The approach does NOT specify a max execution time for the extracted methods
- **No max-account limit**: The approach does NOT specify a limit on the number of accounts to iterate

**Risk**: The approach document does NOT address:
1. **Latency Budget**: What is the acceptable execution time for `AdoptFleetOrders()`? 10ms? 100ms? 1s?
2. **Worst-Case Analysis**: If a user has 100 accounts with 50 orders each (5000 orders), how long does the extraction take?
3. **Timeout Handling**: Should the extraction add a timeout to prevent indefinite blocking?

**Approach Document Gap**: Section 2 (Target State) estimates LOC (line 136: "~80 lines") but does NOT estimate latency. Section 5 (V12 DNA Verification Plan) does NOT include a latency test.

**Recommendation**: Add to Section 5 (V12 DNA Verification Plan):
- **"Latency Benchmark"**: Measure `AdoptFleetOrders()` execution time with 10, 50, and 100 accounts. Verify it completes in <100ms for 50 accounts.
- If latency exceeds 100ms, add a max-account limit or timeout.

---

## Sentinel Verdict

**Status**: ⚠️ **REVISION REQUIRED**

The approach document is **structurally sound** but contains **3 CRITICAL gaps** that must be addressed before ticket generation:

1. **Gap 1 (CRITICAL)**: Add "Dictionary Mutation Safety" section to verify thread-safety and ordering constraints.
2. **Gap 2 (CRITICAL)**: Add "Flag Timing Invariant" to Section 4 to handle reconnect races and FSM hydration failures.
3. **Gap 3 (CRITICAL)**: Add error handling for FSM hydration failure to prevent REAPER deadlock.

**Additional Recommendations**:
- **Risk 1 (HIGH)**: Add REAPER consistency test to Section 5.
- **Risk 2 (HIGH)**: Use FREEZE-PROOF pattern (`Account.All.ToArray()`) in `AdoptFleetOrders()`.
- **Violation 1 (POTENTIAL)**: Add Actor serialization audit to Section 5.
- **Violation 2 (POTENTIAL)**: Add latency benchmark to Section 5.

**Next Steps**:
1. Director reviews this sentinel report
2. Planner updates 02-approach.md to address the 3 CRITICAL gaps
3. Sentinel re-reviews updated approach (optional)
4. Proceed to Phase 3 (Validation) if gaps are resolved

---

**[SENTINEL-GATE]** Semantic Scan complete. Awaiting Sentinel-Adversary approval.