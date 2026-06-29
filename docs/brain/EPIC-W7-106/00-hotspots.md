# EPIC-W7-106 — Phase 0: Hotspot Analysis

## Method Name
`LogHealthCheckResult`

## CYC Score
**0** (tool-reported) — **⚠ REQUIRES MANUAL REVIEW**
Static branch-count analysis yields an estimated CYC of **5** (see notes below).

## File Path
`src/V12_002.SIMA.Fleet.cs` — lines 581–610

---

## CYC=0 Finding Notes

The jCodemunch `get_symbol_complexity` tool returned CYC=0 for this method. This is a known
artefact that occurs when the tool cannot resolve a `private void` helper with a multi-line
parameter list in a `partial class` context. The method **does exist** and has been confirmed
by direct source inspection.

**Manual static branch-count (McCabe):**

| # | Construct | +CYC |
|---|---|---|
| — | Method entry (base) | 1 |
| 1 | `if (brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending)` — 4 `&&`-joined predicates | +4 |
| 2 | `else if (brokerFlat && (hasActiveFsm \|\| hasActivePosition \|\| hasDispatchPending))` — compound predicate | +3 |
| 3 | Inner ternary: `hasActiveFsm ? … : (hasDispatchPending ? … : …)` — 2 `?:` branches | +2 |
| **Total** | | **~10** |

> Note: McCabe's strict definition counts each boolean operator in a compound condition as +1.
> Under the simpler "decision-point only" variant (one per `if`/`else if`/ternary), CYC ≈ 5.
> The tool-reported 0 is treated as a parse failure, not a true score.

---

## Blast Radius Summary

`LogHealthCheckResult` is a **diagnostic-only leaf** in the SIMA Fleet health-check call chain:

```
PumpFleetDispatch (SIMA.Fleet.cs)
  └─ ShouldSkipFleetAccount           (line 450)
       └─ ShouldSkipFleet_RunHealthCheck  (line 478)   ← direct caller
            └─ LogHealthCheckResult        (line 581)   ← TARGET
```

**Callers:** 1 direct caller — `ShouldSkipFleet_RunHealthCheck` (line 495).
`ShouldSkipFleetAccount` is itself called from the fleet dispatch pump for every account
processed per dispatch cycle.

**Callees:** None. The method is a pure `void` sink — it only mutates the `StringBuilder`
`dispatchLog` parameter via `AppendLine`.

**Shared state touched:**
- `StringBuilder dispatchLog` (write-only, passed by reference) — no heap allocation of
  mutable shared objects.
- No `ConcurrentDictionary`, no field reads, no position/FSM state mutations.

**Blast score: LOW** — the method is write-isolated (diagnostic log only). A defect here
produces incorrect forensic log output but cannot affect order routing, FSM state, or broker
position. Zero external callers beyond the single direct parent.

---

## Top 3 Complexity Drivers

### 1 — Compound boolean guard with four negated sub-conditions (`if` branch, line 590)
```csharp
if (brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending)
```
Four `&&`-linked predicates, three of which are negated, form the "all-clear" branch. A
reader must mentally evaluate the full truth table to understand which state combination
triggers the no-op path. This is the dominant CYC contributor.

### 2 — Asymmetric `else if` with mixed `||` and `&&` operators (line 600)
```csharp
else if (brokerFlat && (hasActiveFsm || hasActivePosition || hasDispatchPending))
```
The second branch re-checks `brokerFlat` (already confirmed by the first branch failing)
and ORs three sub-conditions — two of which are the logical negations from branch 1. The
redundant `brokerFlat` re-check and the OR fan-out add cognitive load disproportionate to
the actual logic delta.

### 3 — Nested ternary inside `string.Format` call (line 606)
```csharp
hasActiveFsm ? "FSM active" : (hasDispatchPending ? "dispatch pending" : "activePos present")
```
A two-level nested ternary embedded inside a `string.Format` argument makes the three
diagnostic string outputs invisible at a glance and prevents IDE tooling from flagging
incomplete coverage of the `(!hasActiveFsm && !hasDispatchPending && hasActivePosition)`
state.

---

## Recommended Extraction Count

**0 extractions** — the method is already the result of a previous extraction pass
(`T-W1-Perf` tag, per comment on line 483: *"T-W1-Perf: Extracted helpers reduce CYC from
31 to <=15"*). Further extraction would push the diagnostic logic into a fragment too small
to stand alone.

**Recommended refactor instead:** Replace the two compound-condition branches with a named
local `bool flat = brokerFlat` guard and extract the nested ternary into a helper string
`string ActiveComponent(...)`. This is a **simplification-in-place** (CYC neutral, readability
improvement) rather than a structural extraction.

| # | Proposed Change | Type | CYC Delta |
|---|---|---|---|
| 1 | Extract nested ternary to `DescribeActiveComponent(hasActiveFsm, hasDispatchPending)` | Inline rename | 0 |
| 2 | Drop redundant `brokerFlat &&` re-check in `else if` (already guaranteed true at that point) | Simplification | −1 |

Expected post-refactor CYC: **~4** (decision-point variant) or **~8** (McCabe strict).

---

## Agent Tracking

```
epic:           EPIC-W7-106
wave:           7
phase:          0 — Hotspot Analysis
status:         completed (manual review flagged — CYC=0 from tool, estimated ~5 from static analysis)
output:         docs/brain/EPIC-W7-106/00-hotspots.md
cyc_tool:       0
cyc_estimated:  5–10 (decision-point vs McCabe strict)
method:         LogHealthCheckResult
source_file:    src/V12_002.SIMA.Fleet.cs (lines 581–610)
agent_name:     v12-phase0-hotspot
bobcoins_used:  12
execution_time: ~95s
```
