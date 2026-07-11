# EPIC-W7-113 — Phase 0: Hotspot Analysis

**Wave:** 7 | **Phase:** 0  
**Method:** `HydrateFSMsFromWorkingOrders`  
**Source:** `src/V12_002.SIMA.Lifecycle.cs` (line 787)  
**Cyclomatic Complexity (CYC):** 0 (tool-reported) / **12** (manual count — see § 2)  
**Status:** ⚠️ Requires Manual Review — tool score unreliable; manual count used for all downstream decisions

---

## 1. Symbol Location

| Item | Detail |
|---|---|
| Class | `V12_002` (partial) |
| Namespace | `NinjaTrader.NinjaScript.Strategies` |
| File | `src/V12_002.SIMA.Lifecycle.cs` |
| Definition line | 787 |
| Method span | lines 787–891 (104 lines) |
| Only call-site | `HydrateWorkingOrdersFromBroker` → line 445 |

> **Note on CYC = 0:** `mcp__jcodemunch-mcp__search_symbols` and `get_symbol_complexity` returned a score of 0 for this symbol.
> This is a known indexer gap when a partial-class method has no public visibility marker exposed to the static-analysis provider.
> Manual branch-count below confirms **CYC ≥ 12**. All phase recommendations are based on the manual count.

---

## 2. CYC Breakdown (Manual Count)

Method body: `src/V12_002.SIMA.Lifecycle.cs` lines 787–891.

| # | Construct | Line | Notes |
|---|---|---|---|
| 1 | Base path | — | +1 (entry) |
| 2 | `foreach (var kvp in entryOrders.ToArray())` | 795 | loop back-edge |
| 3 | `if (entryOrder == null)` | 799 | null guard |
| 4 | `!activePositions.TryGetValue(...) \|\| !pi.IsFollower` | 804 | `\|\|` left operand |
| 5 | `!pi.IsFollower` | 804 | `\|\|` right operand |
| 6 | `if (pi.ExecutingAccount == null)` | 806 | null guard |
| 7 | `if (_followerBrackets.ContainsKey(entryKey))` | 810 | idempotent guard |
| 8 | `if (state == null)` | 815 | terminal-state skip |
| 9 | `if (state.Value == FollowerBracketState.Active)` | 820 | conditional position lookup |
| 10 | `if (stopOrders.TryGetValue(...) && stopOrd != null)` | 836 | `&&` left operand |
| 11 | `stopOrd != null` | 836 | `&&` right operand |
| 12 | `if (!string.IsNullOrEmpty(stopOrd.OrderId))` | 839 | nested order-ID guard |

**Manual CYC total: 12**

> The five `LinkTargetOrderToFSM` calls (lines 847–851) each delegate to a helper that contributes
> their own internal branching; they are **not** counted here (they are separate methods with their own CYC budgets).

---

## 3. Blast Radius

### Direct callers

| Caller | File | Line |
|---|---|---|
| `HydrateWorkingOrdersFromBroker` | `src/V12_002.SIMA.Lifecycle.cs` | 445 |

### Transitive downward calls (within `HydrateFSMsFromWorkingOrders`)

```
HydrateFSMsFromWorkingOrders
  ├─ MapOrderStateToFSMState(entryOrder.OrderState)        → line 814  (CYC 4)
  ├─ FindLivePosition(pi)                                  → line 822  (CYC 3)
  ├─ ResolveRemainingContracts(state, qty, posQty)         → line 825  (CYC 2)
  ├─ BuildFSM(entryKey, acct, order, state, qty)           → line 832  (CYC 1)
  ├─ LinkTargetOrderToFSM × 5  (target1–5)                 → lines 847–851  (CYC 2 each)
  ├─ RegisterFSM(entryKey, fsm, order, ref idx, ref cnt)   → line 854  (CYC 2)
  └─ HydrateFromOpenPositions(stopOrders, t1–t5, ...)      → line 866  (CYC ~10, largest delegate)
```

### State mutated by this method

| Field | Type | Risk if corrupted |
|---|---|---|
| `_followerBrackets` | `ConcurrentDictionary<string, FollowerBracketFSM>` | FSM duplication / missing FSM — REAPER acts on wrong position set |
| `_orderIdToFsmKey` | `ConcurrentDictionary<string, string>` | Event routing failure — bracket fills/cancels silently lost |

### Cross-file consumers of mutated state

| File | Access pattern |
|---|---|
| `src/V12_002.Symmetry.BracketFSM.cs` | `ResolveFsmFromEvent`, `TryTerminateFollowerBracket`, `RemoveFsmOrderIdMappings` |
| `src/V12_002.Symmetry.Follower.cs` | `_followerBrackets[fleetEntryName] = fsm` (write peer) |
| `src/V12_002.SIMA.Fleet.cs` | `InitializeFollowerBracketFSM`, flatten sweep |
| `src/V12_002.SIMA.Dispatch.cs` | `InitializeFollowerBracketFSM` (x2), `TryRemove` |
| `src/V12_002.SIMA.Shadow.cs` | FSM state read for shadow-stop logic |
| `src/V12_002.UI.IPC.Commands.Fleet.cs` | Active FSM query for fleet UI |

### Upstream trigger chain

```
OnStateChange → ApplySimaState → HydrateWorkingOrdersFromBroker (reconnect/startup cold path)
                                    └─ HydrateFSMsFromWorkingOrders   ← HOTSPOT
```

---

## 4. Top 3 Complexity Drivers

### Driver 1 — Multi-guard `foreach` body with five sequential continue-guards (lines 795–816)

The loop body carries **five independent early-return guards** before it reaches the first meaningful operation at line 820. Each guard tests a different invariant (null order, non-follower account, null executing account, idempotent key, terminal state). The number of branches needed to enter the productive path is 5, making the happy path feel deeply nested even though it is structurally flat. A Specification Object or a `ShouldProcessEntry(key, order, out PositionInfo pi)` helper would collapse these to a single boolean precondition.

### Driver 2 — Inline stop-order linking with nested null-guard (lines 835–844)

```csharp
if (stopOrders.TryGetValue(entryKey, out stopOrd) && stopOrd != null)
{
    fsm.StopOrder = stopOrd;
    if (!string.IsNullOrEmpty(stopOrd.OrderId))   // nested
    {
        _orderIdToFsmKey[stopOrd.OrderId] = entryKey;
        ordersIndexed++;
    }
}
```
This two-level conditional pattern is repeated verbatim in `HydrateFromOpenPositions` (lines 707–715 and five more times for target slots). The stop-order linking logic was already factored out into `LinkTargetOrderToFSM` for targets but **not** for the stop order. Extracting a `LinkStopOrderToFSM(ref fsm, entryKey, stopOrders, ref ordersIndexed)` mirror would remove this branch and its duplicate in the position-pass method.

### Driver 3 — Dual-pass orchestration in a single method body (lines 794–875)

`HydrateFSMsFromWorkingOrders` coordinates two conceptually separate passes — an **entry-order pass** and a **position pass** — within one body, with the second pass delegated to `HydrateFromOpenPositions` which itself has CYC ≈ 10. This means a reader must track two execution contexts, two `fsmCreated`/`ordersIndexed` counters (shared by ref), and two `Print` telemetry blocks, all in one method. Extracting `RunEntryOrderPass(ref int ordersIndexed, ref int fsmCreated)` as the counterpart to the already-extracted `HydrateFromOpenPositions` would give both passes symmetric structure and independent testability.

---

## 5. Recommended Extraction Count

| Priority | Extraction | Notes |
|---|---|---|
| P0 | Extract `TryGetEntryPassCandidate(entryKey, out PositionInfo pi)` | Collapses 5 continue-guards into a single boolean; pure function |
| P0 | Extract `LinkStopOrderToFSM(ref fsm, entryKey, stopOrders, ref ordersIndexed)` | Mirrors existing `LinkTargetOrderToFSM`; eliminates duplicate in position pass |
| P1 | Extract `RunEntryOrderPass(ref int ordersIndexed, ref int fsmCreated)` | Symmetric peer to `HydrateFromOpenPositions`; isolates entry-pass logic |

**Total recommended extractions: 3**

After extraction, `HydrateFSMsFromWorkingOrders` reduces to a ~20-line orchestrator with CYC ≤ 3 (two sequential calls + telemetry prints).

---

## 6. Risk Assessment

| Dimension | Rating | Rationale |
|---|---|---|
| Correctness risk | 🔴 HIGH | Corrupting `_followerBrackets` at startup causes REAPER to act on wrong position set — live account risk |
| Thread-safety risk | 🟡 MEDIUM | `ConcurrentDictionary` ops are individually atomic; but multi-step read-modify-write across two dictionaries is not transactional |
| Testability | 🟡 MEDIUM | CYC 12 requires ≥12 test paths; method is side-effectful (mutates two global dictionaries + calls `Print`) — needs seam injection |
| Change blast radius | 🔴 HIGH | `_followerBrackets` is consumed by 6 files across Symmetry, Fleet, Dispatch, Shadow, and UI layers |

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase0-hotspot |
| Bobcoins Used | 18 |
| Execution Time | ~4 min |
| Tool Note | `mcp__jcodemunch-mcp__search_symbols` and `get_symbol_complexity` returned CYC = 0 (indexer gap on private partial-class method). Manual branch-count performed directly against source. All recommendations based on manual CYC = 12. |

---

*Generated: Phase 0 — Hotspot Analysis | EPIC-W7-113 | Wave 7*
