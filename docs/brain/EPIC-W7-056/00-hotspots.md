# EPIC-W7-056 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field | Value |
|---|---|
| **Method Name** | `SweepBrokerOrders` |
| **Cyclomatic Complexity (CYC)** | 28 |
| **File Path** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Lines** | 1360–1454 |
| **Visibility** | `private` |
| **Caller** | `CancelAllV12GtcOrders(bool force)` → called from `ProcessShutdownSIMA()` and `V12_002.Lifecycle.cs:216` |

---

## Blast Radius Summary

`SweepBrokerOrders` is the **broker-level Phase 2 GTC cancel sweep**. It is invoked exclusively through
`CancelAllV12GtcOrders`, which is itself called from two sites:

1. **`ProcessShutdownSIMA()`** — SIMA disable path (`force=false`): soft disable, protects bracket orders on
   accounts that hold open positions.
2. **`V12_002.Lifecycle.cs:216`** — Strategy terminate path (`force=true`): hard terminate, cancels all
   V12-prefixed orders across all fleet accounts.

**Affected subsystems (blast radius):**
- `V12_002.SIMA.Lifecycle.cs` — owns `SweepTrackedOrders` + `SweepBrokerOrders` (sibling Phase 1/2 sweep pair)
- `V12_002.Lifecycle.cs` — strategy terminate caller
- `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Flatten.cs`, `V12_002.SIMA.Execution.cs` — share `IsFleetAccount` predicate
- `V12_002.REAPER.Audit.cs` — REAPER may re-audit after sweep completes; order cancellation drives FSM transitions
- `V12_002.Orders.Management.Cleanup.cs`, `V12_002.Orders.Callbacks.AccountOrders.cs` — downstream order
  state callbacks fire as broker confirms each cancellation
- Any UI / IPC command that triggers SIMA disable also indirectly invokes this sweep via the lifecycle path

**Risk level:** HIGH. Errors inside this method (missed cancels or erroneous bracket cancellations) directly
expose trader accounts to naked positions or unprotected risk.

---

## Top 3 Complexity Drivers

### 1. Dual-mode prefix table (force/soft path branch at entry)
The ternary assignment of `v12Prefixes` at lines 1365–1383 produces two completely different prefix sets
(14 prefixes vs 7) controlled by the single `force` boolean. This is the single highest-weight branch
because every subsequent loop iteration inherits its logic implicitly.

### 2. Nested triple loop with five independent guard clauses
The structure `Account.All → acct.Orders → v12Prefixes[pi]` embeds three independent `continue`/`break`
paths inside a `for` loop nested inside a `foreach` inside a `foreach`. Each guard clause (instrument
check, 5-state order-state fan-out, prefix match, bracket exclusion re-check) is an independent decision
point, collectively accounting for ~15 of the 28 CYC units.

### 3. Redundant bracket-exclusion re-guard on soft disable (`!force` block, lines 1419–1441)
After soft-disable already excludes bracket prefixes from `v12Prefixes`, the method re-evaluates the same
8 prefix conditions again inside `[FIX-FF]` to catch naming drift. This defensive duplication is correct
for safety but adds 8 additional decision points that mirror logic already expressed in the prefix table
and in `SweepTrackedOrders`. It is the clearest extraction candidate.

---

## Recommended Extraction Count

**3 extractions** are recommended to bring the residual CYC of the remaining shell to ≤10:

| # | Extracted Method | Estimated CYC Reduction |
|---|---|---|
| 1 | `BuildSweepPrefixes(bool force): string[]` | −2 (removes ternary + implicit scope) |
| 2 | `IsCancellableOrderState(Order ord): bool` | −5 (isolates the 5-state fan-out guard) |
| 3 | `IsProtectedBracketOrder(string ordName): bool` | −8 (isolates the `[FIX-FF]` re-guard block) |

Residual `SweepBrokerOrders` CYC after extractions: **≈13** (outer loop + instrument guard + prefix match
loop + isV12 flag + inner try/catch).

---

## Agent Tracking

```
epic_id   : EPIC-W7-056
wave      : 7
phase     : 0
status    : completed
agent     : Bob (analysis)
timestamp : 2025-06-13T00:00:00Z
output    : docs/brain/EPIC-W7-056/00-hotspots.md
cyc_confirmed : 28
method    : SweepBrokerOrders
source    : src/V12_002.SIMA.Lifecycle.cs:1360
```
