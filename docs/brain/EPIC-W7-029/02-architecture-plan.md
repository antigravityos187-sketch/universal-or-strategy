# EPIC-W7-029 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-029
**Method:** `ShouldSkipFleet_RunHealthCheck`
**Source:** `src/V12_002.SIMA.Fleet.cs` (lines 478–511)
**CYC Baseline:** ~5 (already compliant) | **Target:** <= 8

---

## Status: ALREADY COMPLIANT — No Extractions Required

The method `ShouldSkipFleet_RunHealthCheck` was previously refactored (T-W1, CYC reduced from 31 to current ~5). The docstring confirms: "Extracted helpers reduce CYC from 31 to <=15." The method now delegates to 4 focused helpers.

---

## Extraction Plan

| # | New Helper | Extracted Logic | Projected CYC | Status |
|---|---|---|---|---|
| — | None required | Method already at CYC ~5 | ~5 | ALREADY COMPLIANT |

**Current CYC breakdown:**
- try/catch wrapper: +1
- `if (acct == null || acct.Positions == null)`: +2 (binary OR)
- `if (_diagFleet)` in catch block: +1
- base: +1 = CYC **~5**

**max_cyc_projected: 5** — within <= 8 threshold. No further extraction needed.

---

## Existing Delegate Architecture (Already Correct)

The current structure already satisfies Jane Street single-responsibility:

```
ShouldSkipFleet_RunHealthCheck(acct, dispatchLog):
  [guard] null check for acct + acct.Positions
  brokerFlat     = IsBrokerPositionFlat(acct)          // focused delegate
  hasFsm         = HasActiveFsmForAccount(acct.Name)   // focused delegate
  hasPosition    = HasActivePositionForAccount(acct.Name) // focused delegate
  hasDispatch    = _dispatchSyncPendingExpKeys.ContainsKey(ExpKey(acct.Name))
  LogHealthCheckResult(...)                             // focused log delegate
  catch: if(_diagFleet) Print(...)
```

Each delegate has single responsibility. The parent is a pure orchestrator (CYC ~5).

---

## Jane Street KB Compliance

| Rule | Application |
|---|---|
| carl_cook: zero-alloc hot path | No allocations in orchestrator; diagnostics in cold catch path |
| carl_cook: AggressiveInlining hot | N/A — orchestrator calls delegates; no inlining change needed |
| carl_cook: avoid LINQ | No LINQ present |
| gjengset: no new lock() blocks | Zero locks; ContainsKey on ConcurrentDictionary is lock-free |
| trading_billions: single responsibility | Each delegate has one concern (flat check, FSM check, position check, log) |
| trading_billions: CYC <= 8 | Parent ~5, all <= 8 — PASS |
| trading_billions: defense in depth | try/catch wraps entire body; null guard is first check |

---

## MCP Evidence

- **resolve_repo:** `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols
- **get_context_bundle:** Full source lines 478–511 (34 lines); method already delegates to 4 helpers; CYC ~5
- **get_call_hierarchy:** 1 direct caller (ShouldSkipFleetAccount); 4 direct callees all in same file; blast radius fully contained
- **dependency_graph:** Zero cross-file edges

---

## Sequential Thinking Evidence

- **Thought 1 (complexity drivers):** Actual CYC ~5 from try/catch (+1), null OR guard (+2), catch if(_diagFleet) (+1). Method was previously reduced from CYC=31 via T-W1 extractions.
- **Thought 2 (extraction strategy):** No new extractions required — method is already at target. Existing delegation pattern is correct architecture.
- **Thought 3 (CYC validation):** CYC ~5 <= 8 PASS. Zero new extractions. Document verify-only action.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-029 |
| **Extractions Planned** | 0 (already compliant) |
| **max_cyc_projected** | 5 |
