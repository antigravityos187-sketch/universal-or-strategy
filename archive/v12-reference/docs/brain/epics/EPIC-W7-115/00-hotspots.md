# EPIC-W7-115 Hotspot Analysis

**Method:** SweepTrackedOrders
**CYC:** 34
**File:** src/V12_002.SIMA.Lifecycle.cs

---

## Overview

`SweepTrackedOrders` is Phase 1 of the `CancelAllV12GtcOrders` cancel-sweep pipeline. It iterates
tracking dictionaries (`entryOrders`, `stopOrders`, `target1Orders`–`target5Orders`) and cancels
any live-state orders via `CancelOrderOnAccount`. Its companion `SweepBrokerOrders` (Phase 2,
same file) performs the same sweep directly against the broker order list to catch V12 orders
that fell outside tracking dicts. The CYC=34 score is the aggregate complexity for the
cancel-sweep subsystem (`CancelAllV12GtcOrders` + `SweepTrackedOrders` + `SweepBrokerOrders`),
which is the hotspot unit identified for this epic.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `CancelAllV12GtcOrders(bool force)` (line 1294, same file) |
| **Caller chain (SIMA disable)** | `ProcessShutdownSIMA` → `CancelAllV12GtcOrders` → `SweepTrackedOrders` |
| **Caller chain (strategy terminate)** | `V12_002.Lifecycle.cs:216` → `CancelAllV12GtcOrders` → `SweepTrackedOrders` |
| **Peer method in subsystem** | `SweepBrokerOrders(bool force)` (line 1360, same file) — Phase 2 broker scan |
| **Shared state written** | None — read-only dict iteration; mutations occur in `CancelOrderOnAccount` |
| **Shared state read** | `entryOrders`, `stopOrders`, `target1Orders`, `target2Orders`, `target3Orders`, `target4Orders`, `target5Orders` (7 ConcurrentDictionary<string, Order>) |
| **External dependency** | `CancelOrderOnAccount` (src/V12_002.Orders.CancelGateway.cs:46) — broker cancel gateway; referenced by 10+ callers |
| **Side-effects** | Live order cancellation — irreversible broker action; no dict mutation |
| **Threading constraint** | Must run on strategy thread (actor-serialized lifecycle path); ToArray() snapshot guards concurrent reads |
| **force=false semantic** | Cancels `entryOrders` only — bracket orders protected for live positions |
| **force=true semantic** | Cancels all 7 tracking dicts — strategy terminate path only |
| **Risk on change** | High — cancel logic is safety-critical; semantic separation (`force` flag) must be preserved exactly to avoid naked positions |

**Affected symbol count (blast radius):** 7 shared order dicts + 1 cancel gateway + 2 lifecycle callers + 1 peer sweep method = 11 directly coupled symbols.

---

## Top 3 Complexity Drivers

1. **5-way OrderState compound guard with inverted negation logic (`&&` chain of `!=`)**
   Lines 1336–1343: the "skip if not live" guard checks 5 separate `OrderState` enum values via
   chained `!=` conditions — `Working`, `Accepted`, `Submitted`, `ChangePending`, `ChangeSubmitted`.
   Each `!=` branch adds +1 CYC. The inverted guard (skip-on-mismatch rather than proceed-on-match)
   is semantically subtle: a developer adding a new broker state must know to add a sixth `&&` clause
   here. This contributes ~5 CYC points and is the single densest logical node in the method.
   The same 5-way pattern is duplicated verbatim in `SweepBrokerOrders` (lines 1395–1402),
   doubling its overall impact in the subsystem.

2. **Nested foreach-over-foreach dict iteration with ternary dict-set selection**
   Lines 1313–1351: the outer `foreach (var dict in trackedDicts)` iterates a `force`-selected
   array of 1 or 7 dicts; the inner `foreach (var kvp in dict.ToArray())` iterates each dict's
   entries. The ternary at line 1313 selects the dict array at construction time, but the guard
   `if (dict == null) continue` and `if (ord == null) continue` add two more early-exit branches
   inside the loops. The two-level nested iteration with dual null-guards contributes ~4 CYC
   points and makes stack-depth reasoning non-trivial during code review.

3. **7-way bracket-order OR chain in `SweepBrokerOrders` (peer method, same subsystem)**
   Lines 1421–1429 in `SweepBrokerOrders`: the `isBracketOrder` boolean is assembled from 7
   separate `StartsWith` prefix checks joined with `||`. Each `||` branch is a separate decision
   point (+1 CYC each). This guard implements the same protection rule as the dict-level `force`
   separation in `SweepTrackedOrders`, but as a string-matching guard rather than a structural
   selection. The duplication of the protection semantic across both methods (structural in Phase 1,
   string-match in Phase 2) creates two maintenance points for the same invariant.
   This contributes ~7 CYC points to the aggregate subsystem score.

---

## Recommended Extraction Count

**3 extractions recommended across the cancel-sweep subsystem.**

| # | Extraction | Target | CYC Reduction |
|---|---|---|---|
| 1 | `IsOrderCancellable(Order ord) → bool` | Extract the 5-way OrderState guard (lines 1336–1343) shared by both sweep methods into a named predicate | ~5 CYC removed from both callsites; eliminates duplication |
| 2 | `IsBracketOrderName(string name) → bool` | Extract the 7-way `isBracketOrder` prefix OR-chain (lines 1421–1429) into a named pure function | ~7 CYC removed from `SweepBrokerOrders`; makes the invariant a single maintenance point |
| 3 | `SelectSweepDictionaries(bool force) → IEnumerable<ConcurrentDictionary<string, Order>>` | Extract the ternary dict-array construction (lines 1313–1324) into a named builder; clarifies the force=false/true semantic contract | ~2 CYC removed from `SweepTrackedOrders`; the semantic separation becomes explicitly documented |

**Rationale:** The `force` semantic (entry-only vs all-brackets) is the load-bearing invariant of
this subsystem. Currently it is expressed in three separate places (dict-array selection, prefix
array selection, and the bracket OR-chain) with no shared abstraction. Extracting
`IsOrderCancellable` eliminates the duplicated 5-way state guard and makes future broker-state
additions a single-site change. `IsBracketOrderName` turns the 7-prefix protection rule into an
auditable pure function. Combined, these three extractions reduce aggregate CYC from ~34 to
approximately 18–20, well within the CYC ≤ 15 per-method target.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~60s
