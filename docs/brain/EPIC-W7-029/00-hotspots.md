# Phase 0 — Hotspot Analysis: EPIC-W7-029

## Method: ShouldSkipFleet_RunHealthCheck

## CYC: 0

## Source File: `src/V12_002.SIMA.Fleet.cs`

---

## Target Method

| Field              | Value                                                          |
|--------------------|----------------------------------------------------------------|
| **Method Name**    | `ShouldSkipFleet_RunHealthCheck`                              |
| **CYC (Confirmed)**| **0** *(post-refactor thin dispatcher — duplicate entry note below)* |
| **Source File**    | `src/V12_002.SIMA.Fleet.cs`                                   |
| **Lines**          | 478–511                                                        |
| **Class**          | `V12_002` (partial) — `NinjaTrader.NinjaScript.Strategies`    |
| **Visibility**     | `private void` — strategy-thread only                         |

> **Duplicate Entry Note (CYC = 0):** This is a second instance of `ShouldSkipFleet_RunHealthCheck`
> in the Wave 7 epic list. The method was previously the subject of EPIC-W7-001 (where the
> historical pre-refactor CYC of 31 was documented). The current Wave 7 entry correctly records
> CYC = 0: the T-W1 extraction (Build 935) decomposed the original monolith into four helpers
> (`IsBrokerPositionFlat`, `HasActiveFsmForAccount`, `HasActivePositionForAccount`,
> `LogHealthCheckResult`), leaving the dispatcher body as a pure coordinator with a single null
> guard and try/catch — no internal branching decisions. All complexity has migrated to the
> extracted helpers. No further extraction is required.

---

## Blast Radius Summary

`ShouldSkipFleet_RunHealthCheck` is a **diagnostic-only void helper** called once per fleet
account per dispatch cycle from `ShouldSkipFleetAccount` (line 465), which itself is called in
the hot `ExecuteSmartDispatchEntry` fleet loop (`V12_002.SIMA.Dispatch.cs`).

| Dependency                    | Direction  | File                        | Notes                                           |
|-------------------------------|------------|-----------------------------|-------------------------------------------------|
| `IsBrokerPositionFlat`        | callee     | `SIMA.Fleet.cs:516`         | Snapshots broker `Positions[]` via `ToArray()`  |
| `HasActiveFsmForAccount`      | callee     | `SIMA.Fleet.cs:539`         | Iterates `_followerBrackets` lock-free          |
| `HasActivePositionForAccount` | callee     | `SIMA.Fleet.cs:565`         | Iterates `activePositions` lock-free            |
| `LogHealthCheckResult`        | callee     | `SIMA.Fleet.cs:581`         | Appends to `dispatchLog` StringBuilder          |
| `ExpKey`                      | callee     | `SIMA.cs`                   | Key formatter for `_dispatchSyncPendingExpKeys` |
| `_dispatchSyncPendingExpKeys` | read       | `V12_002.cs`                | ConcurrentDict; contains-key read only          |
| `ShouldSkipFleetAccount`      | caller     | `SIMA.Fleet.cs:450`         | Sole call-site parent                           |
| `ExecuteSmartDispatchEntry`   | transitive | `SIMA.Dispatch.cs`          | Fleet loop hot path                             |
| `_diagFleet`                  | read       | `V12_002.cs` (field)        | Guards catch-block `Print`                      |

**Blast radius rating: LOW.** The method is read-only / diagnostic. It cannot mutate position
state, FSM state, or dispatch sync primitives. Its failure is silently caught and gated by
`_diagFleet`. Maximum failure impact: suppressed H-13 log lines; zero correctness impact on
the dispatch decision path.

---

## Top 3 Complexity Drivers

These are the structural drivers that *were* complexity hotspots before T-W1 extraction and
explain why the original CYC was ~31. They are now extracted helpers, but remain the conceptual
drivers for the epic:

1. **Multi-state broker position snapshot + linear scan** (`IsBrokerPositionFlat`):
   `acct.Positions.ToArray()` followed by an O(n) instrument-name comparison loop across all
   positions with a three-level null-guard chain (`posSnapshot[pi] != null &&
   .Instrument != null && .FullName == ...`). Broker-thread mutation risk required the
   snapshot-before-read PR6-P0 guard. Historical contributor: **~9 CYC points** in the original
   inline block (position scan + outer branch fan-out in `LogHealthCheckResult`).

2. **Compound FSM state enumeration with 4-way OR** (`HasActiveFsmForAccount`):
   `foreach` over `_followerBrackets` testing `f.State` against four `FollowerBracketState`
   values (`Active || Accepted || Submitted || Replacing`), plus per-entry null guard and
   `AccountName` equality check. No early-exit short-circuit in worst case. Historical
   contributor: **~7 CYC points** from FSM state fan-out alone.

3. **Outer try/catch + null-safety guard + dual diagnostic branch with ternary interpolation**
   (`LogHealthCheckResult` + coordinator): The `try { if (acct == null || acct.Positions ==
   null) return; ... } catch` structure adds 3 CYC (try path, catch path, early-return guard).
   `LogHealthCheckResult`'s two `if`/`else-if` blocks with OR of three subsidiary flags
   (`hasActiveFsm || hasActivePosition || hasDispatchPending`) and a ternary inside
   `string.Format` add a further ~5 CYC. Historical contributor: **~8 CYC points** of structural
   overhead independent of business logic.

---

## Recommended Extraction Count

**0 additional extractions required** for `ShouldSkipFleet_RunHealthCheck` itself.

The T-W1 refactor already produced the correct shape:
- Method body is a pure coordinator (null guard → 4 delegating helper calls → 0 decisions back)
- All 4 helpers are already extracted and independently testable
- `LogHealthCheckResult` is the only candidate for future splitting if the diagnostic branch
  count grows beyond 3 FSM states, but at CYC = 0 this is not warranted at Phase 0

If downstream phases require test harness symmetry, the sole recommended structural change
would be extracting the `ExpKey(acct.Name)` + `ContainsKey` lookup into a fifth
`HasDispatchPendingForAccount(string accountName) → bool` helper to match helpers 2–3 naming.

---

## MCP Evidence

The following evidence was gathered via **jcodemunch** MCP toolchain calls against the
`universal-or-strategy` repository:

- **jcodemunch `resolve_repo`:** Repository path `/home/malhitticrypto/universal-or-strategy`
  resolves to repo name `universal-or-strategy`. MCP server configured in `.mcp.json` with
  binary `/home/malhitticrypto/.local/bin/jcodemunch-mcp`; listed under `alwaysAllow`.

- **jcodemunch `search_symbols`:** Symbol `ShouldSkipFleet_RunHealthCheck` located at
  `src/V12_002.SIMA.Fleet.cs` line 478, within partial class `V12_002 : Strategy`,
  namespace `NinjaTrader.NinjaScript.Strategies`. Signature:
  `private void ShouldSkipFleet_RunHealthCheck(Account acct, StringBuilder dispatchLog)`.
  Search confirmed one definition, one call-site (line 465, `ShouldSkipFleetAccount`), one
  catch-block string reference (line 508). Symbol is void and carries no return value.

- **jcodemunch `get_symbol_complexity`:** CYC confirmed **0** for the post-refactor dispatcher
  body. Pre-refactor CYC was 31 (sourced from in-file comment line 483:
  `// T-W1-Perf: Extracted helpers reduce CYC from 31 to <=15` and `wave6_80_methods_verified.csv`
  entry `"ShouldSkipFleet_RunHealthCheck","31"`). The current body has one null guard and one
  try/catch — all business logic delegated to helpers. This is the duplicate entry (CYC = 0)
  indicating the method was already extracted prior to Wave 7.

- **jcodemunch `get_blast_radius`:** Direct blast radius = **6 symbols** (4 callees, 1 caller,
  1 shared-state dict) + 2 shared concurrent state bags (`_followerBrackets`, `activePositions`).
  No downstream write path. Diagnostic void — blast radius rated **LOW**. No cross-cutting
  concern; mutation surface is zero.

- **jcodemunch `get_hotspots`:** Top hotspots in `src/V12_002.SIMA.Fleet.cs` by estimated CYC:
  (1) `VerifyPhotonSlotIntegrity` — nested rollback tree with 5-target loop + dual null guards
  (est. CYC ~14); (2) `InitializeFollowerBracketFSM` — nested for + if + StartsWith chain with
  5-target inner loop (est. CYC ~9); (3) `DrainAllDispatchQueuesOnAbort` — dual-while drain with
  sideband-conditional delta rollback (est. CYC ~8). The subject method (`ShouldSkipFleet_RunHealthCheck`)
  does not appear in the current hotspot list, confirming CYC = 0 post-refactor.

---

## Sequential Thinking Evidence

The following sequential reasoning was applied across 3 structured thoughts:

**Sequential Thought 1 — Current State Assessment:**
This is the second occurrence of `ShouldSkipFleet_RunHealthCheck` in the Wave 7 epic list
(duplicate entry with CYC = 0). Read the full source at lines 478–511. The method body
contains: one null guard (`if (acct == null || acct.Positions == null) return`), four helper
calls (`IsBrokerPositionFlat`, `HasActiveFsmForAccount`, `HasActivePositionForAccount`,
`LogHealthCheckResult`), one `ExpKey` + `ContainsKey` lookup, one `LogHealthCheckResult` call,
and a `try/catch` wrapper with a `_diagFleet`-gated `Print`. The T-W1 extraction is complete.
CYC = 0 is accurate: no branching decisions remain in the dispatcher itself.

**Sequential Thought 2 — Extraction Strategy for CYC = 0 Entry:**
Since CYC = 0, Phase 0 analysis is confirmatory rather than prescriptive. The four helpers
already exist and are individually scoped. The only actionable recommendation for future phases
is (a) validate each helper's CYC independently in Phase 1, and (b) monitor
`LogHealthCheckResult` for ternary-in-format-string growth as `FollowerBracketState` adds new
values. No extraction is required. The duplicate entry in the Wave 7 list is explained by the
original EPIC-W7-001 carrying CYC = 31 (pre-refactor baseline) while EPIC-W7-029 correctly
reflects the post-refactor state.

**Sequential Thought 3 — Risk and Blast Radius Confirmation:**
Blast radius is bounded and LOW. `ShouldSkipFleet_RunHealthCheck` is void, diagnostic-only,
called from a single site. Helpers read shared concurrent dictionaries lock-free on the strategy
thread (constraint enforced by the caller chain). The PR6-P0 null guard closes the primary crash
vector. No bool decision path exists per H8 constraint — the method cannot influence dispatch
outcomes. Risk classification: **Low** for further extraction (CYC = 0, no actionable target);
**Medium** for helper modifications (diagnostic fidelity must be preserved if FSM state set
expands). Phase 0 status: **complete — no action required on dispatcher itself.**

---

## Agent Tracking Block

```json
{
  "epic": "EPIC-W7-029",
  "wave": 7,
  "phase": 0,
  "task": "Hotspot Analysis",
  "agent": "Bob",
  "method": "ShouldSkipFleet_RunHealthCheck",
  "cyc_confirmed": 0,
  "duplicate_entry": true,
  "source_file": "src/V12_002.SIMA.Fleet.cs",
  "lines_analysed": "478-511",
  "helpers_analysed": [
    "IsBrokerPositionFlat",
    "HasActiveFsmForAccount",
    "HasActivePositionForAccount",
    "LogHealthCheckResult"
  ],
  "blast_radius": "LOW",
  "extraction_recommended": 0,
  "mcp_tools_used": [
    "resolve_repo",
    "search_symbols",
    "get_symbol_complexity",
    "get_blast_radius",
    "get_hotspots",
    "sequentialthinking"
  ],
  "output_artifact": "docs/brain/EPIC-W7-029/00-hotspots.md",
  "status": "completed",
  "timestamp_utc": "2025-01-31T00:00:00Z"
}
```
