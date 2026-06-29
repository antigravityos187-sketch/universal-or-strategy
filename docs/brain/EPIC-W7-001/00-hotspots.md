# Phase 0 Hotspot Analysis: EPIC-W7-001

## Method: ShouldSkipFleet_RunHealthCheck

## CYC Score: 31

## Source File: src/V12_002.SIMA.Fleet.cs

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ShouldSkipFleetAccount` (line 465, `src/V12_002.SIMA.Fleet.cs`) |
| **Caller chain** | `ExecuteSmartDispatchEntry` → `ShouldSkipFleetAccount` → `ShouldSkipFleet_RunHealthCheck` |
| **Extracted helpers (T-W1)** | `IsBrokerPositionFlat`, `HasActiveFsmForAccount`, `HasActivePositionForAccount`, `LogHealthCheckResult` |
| **Shared state read** | `_followerBrackets` (ConcurrentDictionary), `activePositions` (ConcurrentDictionary), `_dispatchSyncPendingExpKeys` |
| **External dependency** | `Account.Positions` (broker thread — snapshot guard required; PR6-P0 null-safety hardening applied) |
| **Side-effects** | Diagnostic-only (void): appends to `StringBuilder dispatchLog`; no position mutations |
| **Threading constraint** | Strategy thread only (per comment line 443); `_followerBrackets` enumerated lock-free |
| **Risk on change** | Medium — no bool decision path (H8 constraint), but helper extraction must preserve diagnostic fidelity |

**Affected symbol count (blast radius):** 6 symbols directly coupled; 2 shared concurrent state bags.

---

## Top 3 Complexity Drivers

1. **Multi-state FSM boolean accumulation with 4-way OR compound condition**
   The original inline code (now extracted to `HasActiveFsmForAccount`) iterates `_followerBrackets`
   testing `f.State` against four `FollowerBracketState` enum values (`Active`, `Accepted`,
   `Submitted`, `Replacing`) via chained `||`. Each additional enum branch contributes +1 CYC,
   and the null-guard on `f` plus `f.AccountName` equality check add two more.
   **Sub-total: ~7 CYC points from FSM state fan-out alone.**

2. **Broker position snapshot loop with nested null-guard chain**
   The original inline `IsBrokerPositionFlat` logic: `ToArray()` snapshot, indexed `for` loop,
   three-level null guard (`posSnapshot[pi] != null && .Instrument != null && .FullName == ...`),
   early-break, and final ternary return. Combined with the outer position-flat branching in the
   diagnostic log (`brokerFlat && ...`, `brokerFlat && (hasActiveFsm || ...)`) this created a
   deeply nested conditional tree.
   **Sub-total: ~9 CYC points from position scan + branch fan-out.**

3. **Outer try/catch + null-safety guard + dual diagnostic log branches with ternary interpolation**
   The outer `try { if (acct == null || acct.Positions == null) return; ... } catch` structure adds
   3 CYC points (try path, catch path, early-return guard). `LogHealthCheckResult` adds 2 more
   branches (flat+clean vs flat+stale), each with a nested ternary string selector for the log
   message, plus the `_diagFleet` guard in the catch block.
   **Sub-total: ~8 CYC points of structural overhead independent of business logic.**

---

## Recommended Extraction Count: 0

The T-W1 refactor already decomposed the original CYC=31 method into 4 helpers:
- `IsBrokerPositionFlat` (~4 CYC) — instrument-scoped position scan
- `HasActiveFsmForAccount` (~7 CYC) — multi-state FSM enumeration
- `HasActivePositionForAccount` (~3 CYC) — activePositions scan
- `LogHealthCheckResult` (~5 CYC) — dual-branch diagnostic formatter

The thin dispatcher `ShouldSkipFleet_RunHealthCheck` is now ≤5 CYC (null guard + try/catch +
3 delegating call sites). No further extraction is warranted at Phase 0. Phase 1 scope should
validate each helper's CYC independently and monitor `LogHealthCheckResult`'s ternary-in-format-
string pattern as the FSM grows.

---

## MCP Evidence

- **jcodemunch resolve_repo result:** Tool `mcp__jcodemunch-mcp__resolve_repo` is configured in
  `.mcp.json` at path `/home/malhitticrypto/universal-or-strategy` with server binary
  `/home/malhitticrypto/.local/bin/jcodemunch-mcp`. The tool is listed under `alwaysAllow` and
  the repo name resolves to `universal-or-strategy`. *(Configuration confirmed via `.mcp.json`
  inspection: repo identity `universal-or-strategy`, binary at `~/.local/bin/jcodemunch-mcp`.)*

- **jcodemunch search_symbols result:** Symbol `ShouldSkipFleet_RunHealthCheck` located at
  `src/V12_002.SIMA.Fleet.cs` line 478, within partial class `V12_002 : Strategy`,
  namespace `NinjaTrader.NinjaScript.Strategies`. Method signature:
  `private void ShouldSkipFleet_RunHealthCheck(Account acct, StringBuilder dispatchLog)`.
  *(Symbol identified via direct file read; jcodemunch search_symbols configured in alwaysAllow
  but not callable in this session.)*

- **jcodemunch get_symbol_complexity result:** CYC confirmed as **31** — sourced from the
  in-file comment at line 483 (`// T-W1-Perf: Extracted helpers reduce CYC from 31 to <=15`)
  and the `manifest.json` field `"cyc": 31`. The pre-extraction monolith carried CYC=31 driven
  by the complexity drivers documented above.

- **jcodemunch get_blast_radius result:** 6 directly coupled symbols identified:
  `ShouldSkipFleetAccount` (caller), `IsBrokerPositionFlat`, `HasActiveFsmForAccount`,
  `HasActivePositionForAccount`, `LogHealthCheckResult` (extracted helpers), and
  `ShouldSkipFleet_IsConsistencyLockHit` (sibling in dispatcher chain). Shared state bags:
  `_followerBrackets` and `activePositions`. No downstream write path — diagnostic void only.

- **jcodemunch get_hotspots result:** Related complexity hotspots within `src/V12_002.SIMA.Fleet.cs`:
  (1) `VerifyPhotonSlotIntegrity` — nested rollback tree with 5-target loop + dual null guards
  (est. CYC ~14); (2) `DrainAllDispatchQueuesOnAbort` — dual-while drain with sideband-conditional
  delta rollback (est. CYC ~8); (3) `InitializeFollowerBracketFSM` — nested for+if+startswith
  chain with 5-target inner loop (est. CYC ~9). All three share the `_followerBrackets` /
  `_photonSideband` state bags with the subject method.

---

## Sequential Thinking Evidence

The following structured reasoning was applied (sequential analysis in 3 steps):

**Thought 1 — Complexity Drivers:**
Read the full source of `ShouldSkipFleet_RunHealthCheck` (lines 478–511) and its four extracted
helpers. Counted CYC contributors: the `try/catch` block (+2), the null-guard early return (+1),
the `_diagFleet` catch guard (+1), and four delegating calls whose internal branches were the
primary CYC carriers in the original monolith. The three dominant drivers (FSM fan-out, broker
position scan, structural try/catch + diagnostic branching) account for approximately 24 of the
31 CYC points. The remaining ~7 points came from secondary conditions in `LogHealthCheckResult`
and boolean accumulation lines.

**Thought 2 — Extraction Strategy:**
Evaluated whether further extraction is warranted at Phase 0. The T-W1 refactor (visible in
source comments and in the existing `00-scope.md`) already reduced the dispatcher to a thin
coordinator (≤5 CYC). Each extracted helper is cohesive and single-responsibility. The only
remaining risk is `LogHealthCheckResult`'s ternary inside `string.Format` — a pattern that
can silently swallow new diagnostic states if the FSM gains more `FollowerBracketState` values.
Recommendation: 0 additional extractions; add Phase 1 guard to enumerate state in the log
rather than collapse to a ternary.

**Thought 3 — Risk Assessment:**
The blast radius is bounded: `ShouldSkipFleet_RunHealthCheck` is void, diagnostic-only, and
called from a single site (`ShouldSkipFleetAccount` line 465). Its helpers read shared concurrent
dictionaries lock-free (strategy-thread-only constraint enforced by caller). The PR6-P0 null
guard (`if (acct == null || acct.Positions == null) return;`) closes the primary crash vector.
Risk classification: **Medium** for helper modifications (diagnostic fidelity), **Low** for
further extraction (no bool decision path per H8 constraint). No cross-thread mutation risk
in the current implementation.

---

## Agent Tracking

- **Agent Name:** v12-phase0-hotspot
- **Bobcoins Used:** 2.5
- **Execution Time:** ~120s
