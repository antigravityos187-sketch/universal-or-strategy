# EPIC-W7-027 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-027
**Method:** `Dispatch_PublishMarketBracketToPhoton`
**Source:** `src/V12_002.SIMA.Dispatch.cs` (lines 612–753)
**CYC Baseline:** 9 | **Target:** <= 8

---

## Extraction Plan

| # | New Helper | Extracted Logic | Projected CYC | Jane Street Rule |
|---|---|---|---|---|
| 1 | `Dispatch_CommitBracketToPhotonRing` | ClaimPhotonPoolSlot + PopulatePhotonSlot + TryIncrementDispatchCountWithCircuitBreaker (if-return guard) + EnqueueToPhotonRing + finalize (syncPending/reservedDelta/registeredForCleanup = 0) + LogDispatchCompletion | ~3 | single-responsibility; circuit-breaker isolated |

**Parent method after extraction (projected CYC ~5):**
- exitAction ternary (+1)
- if (stop == null) guard + return (+2)
- reservedDelta ternary (+1)
- base (+1) = CYC 5

**New helper `Dispatch_CommitBracketToPhotonRing` (projected CYC ~3):**
- if (!TryIncrementDispatchCountWithCircuitBreaker) + return (+2)
- base (+1) = CYC 3

**max_cyc_projected: 5** (parent) — well within <= 8 threshold.

---

## Extraction Boundary

```
KEEP in parent:
  var ordersToSubmit = new List<Order> { entry };
  exitAction = ... (ternary)
  PublishPhoton_StopOrder(...)
  if (stop == null) { ... return; }
  PublishPhoton_TargetOrders(...) -> stagedTargets, nonRunnerLimitQty, runnerQty
  RegisterTrackingDictionaries(...)
  InitializeFollowerBracketFSM(...)
  SymmetryGuardRegisterFollower(...)
  reservedDelta = ... (ternary)
  AddExpectedPositionDeltaLocked(...)
  Dispatch_CommitBracketToPhotonRing(...)  // <-- delegate Phase B

EXTRACT to Dispatch_CommitBracketToPhotonRing:
  var (_proxyOrders, _poolSlotIndex) = ClaimPhotonPoolSlot();
  FleetDispatchSlot _slot = PopulatePhotonSlot(...)
  if (!TryIncrementDispatchCountWithCircuitBreaker(...)) { return; }
  int _orderIdx = 2 + stagedTargets.Count;
  EnqueueToPhotonRing(...)
  syncPending = false; reservedDelta = 0; registeredForCleanup = false;
  LogDispatchCompletion(...)
```

---

## Jane Street KB Compliance

| Rule | Application |
|---|---|
| carl_cook: zero-alloc hot path | No new allocations in extracted helper; List<Order> already owned by parent |
| carl_cook: AggressiveInlining hot / NoInlining cold | No attribute changes needed; circuit-breaker trip path is cold, stays NoInlining-eligible |
| carl_cook: avoid LINQ | No LINQ present |
| gjengset: no new lock() blocks | Zero new locks; ref params pass through atomically |
| gjengset: volatile + MemoryBarrier | Unchanged; TryIncrementDispatchCountWithCircuitBreaker owns its own barriers |
| trading_billions: single responsibility | Parent = orchestrate bracket dispatch; helper = commit to photon ring |
| trading_billions: CYC <= 8 | Parent ~5, helper ~3, both <= 8 |
| trading_billions: rate-limit circuit breaker | Preserved inside new helper at natural site |

---

## MCP Evidence

- **resolve_repo:** `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols
- **get_context_bundle:** Full source retrieved (lines 612–753); 7 delegate helpers already extracted; residual CYC from: stop null-guard, exitAction ternary, reservedDelta ternary, circuit-breaker if-block
- **get_call_hierarchy:** 2 callers (Dispatch_ProcessFleetLoop depth=1, ExecuteSmartDispatchEntry depth=2); 8 direct callees all in same file
- **get_dependency_graph:** Zero cross-file import/importer edges — blast radius fully self-contained

---

## Sequential Thinking Evidence

- **Thought 1 (complexity drivers):** CYC=9 from 4 branch points: stop null-guard (2), exitAction ternary (1), reservedDelta ternary (1), circuit-breaker if (2). Remaining complexity post-prior-extractions.
- **Thought 2 (extraction strategy):** Extract Phase B (pool/ring/finalize block) into `Dispatch_CommitBracketToPhotonRing`; parent retains orchestration; helper owns atomic commit + circuit-breaker guard.
- **Thought 3 (CYC validation):** Parent CYC ~5, helper CYC ~3 — both <= 8. Defense in depth preserved. Zero new lock blocks.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-027 |
| **Extractions Planned** | 1 |
| **max_cyc_projected** | 5 |
