# EPIC-W7-071 — Phase 0: Hotspot Analysis

**Epic:** EPIC-W7-071  
**Wave:** 7 | **Phase:** 0  
**Source:** `src/V12_002.SIMA.Shadow.cs`  
**Target Method:** `ShadowProcessFollowerStopUpdate`  
**Cyclomatic Complexity (CYC):** 13  
**Date:** 2025-07-15

---

## 1. Method Overview

[`ShadowProcessFollowerStopUpdate`](src/V12_002.SIMA.Shadow.cs:246) is a private method on the `V12_002` strategy partial class located inside the `#region Shadow Engine` block. It processes a stop-price update for a single named follower within the SIMA (Shadow / Symmetry Infrastructure for Multi-Account) fleet.

**Signature:**
```csharp
private bool ShadowProcessFollowerStopUpdate(
    string followerEntryName,
    double newStopPrice,
    out bool waitingOnFollower
)
```

**Responsibility:** Given a follower's entry name and the leader's new stop price, the method:
1. Looks up the follower's `FollowerBracketFSM` in `_followerBrackets`.
2. Looks up the follower's `PositionInfo` in `activePositions`.
3. Returns `false` early if neither record exists (unknown follower).
4. Sets `waitingOnFollower = true` and returns `true` if the follower position is not yet filled or its bracket has not been submitted.
5. Sets `waitingOnFollower = true` and returns `true` if the FSM is not in `Active` state or has no live `StopOrder`.
6. Short-circuits (no-op, returns `true`) if the follower stop is already within half a tick of the target price.
7. Otherwise logs and delegates to [`UpdateStopOrder`](src/V12_002.Trailing.StopUpdate.cs:84) — the two-phase cancel+resubmit replace FSM.

---

## 2. Cyclomatic Complexity Breakdown (CYC = 13)

| # | Decision Point | Location |
|---|----------------|----------|
| 1 | `hasFsm` bool expression | L255 |
| 2 | `hasFollowerPos` bool expression | L257–258 |
| 3 | `if (!hasFsm && !hasFollowerPos) return false` | L260–261 |
| 4 | `if (!hasFollowerPos …)` — outer guard | L263 |
| 5 | `!followerPos.EntryFilled` — inner check | L263 |
| 6 | `!followerPos.BracketSubmitted` — inner check | L263 |
| 7 | `waitingOnFollower = true; return true` branch (bracket not submitted) | L264–266 |
| 8 | `if (!hasFsm …)` — FSM null check | L269 |
| 9 | `fsm.State != FollowerBracketState.Active` — state check | L269 |
| 10 | `fsm.StopOrder == null` — stop order check | L269 |
| 11 | `waitingOnFollower = true; return true` branch (FSM not active) | L270–272 |
| 12 | `Math.Abs(…) < tickSize * 0.5` — half-tick no-op guard | L276–277 |
| 13 | Implicit default path leading to `UpdateStopOrder` call | L288 |

The high CYC (13) arises from three multi-predicate guard clauses (L260, L263, L269) each embedding 2–3 boolean sub-expressions that each constitute an independent branch in McCabe's graph.

---

## 3. Blast Radius

### Direct Call Chain
```
ManageTrailingStops() [V12_002.Trailing.cs:96]
  └─ ShadowEngineCheck()
       └─ ShadowPropagateStopMoves()
            └─ PropagateAndCacheStopPrice()
                 └─ ShadowMoveFollowerStops()
                      └─ ShadowProcessFollowerStopUpdate()  ← TARGET
                           └─ UpdateStopOrder() [V12_002.Trailing.StopUpdate.cs:84]

OnExecutionUpdate() [V12_002.Orders.Callbacks.Execution.cs:630]
  └─ ShadowEngineCheck()   (second call site)
```

### Shared Mutable State Touched
| Resource | Type | Access Pattern |
|----------|------|---------------|
| `_followerBrackets` | `ConcurrentDictionary<string, FollowerBracketFSM>` | Read (TryGetValue) |
| `activePositions` | `ConcurrentDictionary<string, PositionInfo>` | Read (TryGetValue) |
| `tickSize` | `double` (instance field) | Read |
| `fsm.StopOrder.StopPrice` | `Order.StopPrice` | Read |
| `followerPos.CurrentTrailLevel` | `int` | Read (passed to UpdateStopOrder) |
| `UpdateStopOrder` infrastructure | `pendingStopReplacements`, `stopOrders`, broker API | Write (via callee) |

### Downstream Risk Surface
- **`UpdateStopOrder`** — complex two-phase replace FSM touching broker API; any regression here manifests as a runaway or un-cancelled stop in a follower account.
- **`_followerBrackets`** — written by 10+ call sites across `SIMA.Dispatch`, `SIMA.Fleet`, `SIMA.Lifecycle`, `SIMA.Execution`, `Symmetry.Follower`; stale FSM state reaching this method would cause a suppressed update (`waitingOnFollower=true`) silently.
- **`ShadowMoveFollowerStops`** — aggregates `waitingOnFollower` results and gates whether the leader's stop price cache is committed; a persistent `waitingOnFollower=true` will indefinitely delay cache update and re-trigger propagation on every bar.

---

## 4. Complexity Hotspot Drivers

1. **Multi-predicate guard clauses** — Each of the three guard checks (L260, L263, L269) uses `&&`/`||` across 2–3 conditions, inflating CYC without corresponding readability benefit.
2. **Dual data-source lookup** — The method redundantly consults both `_followerBrackets` (FSM) and `activePositions` (PositionInfo) before deciding, creating a 4-path boolean grid (`hasFsm × hasFollowerPos`).
3. **Implicit `waitingOnFollower` semantics** — The `out bool` signals a distinct "found-but-not-ready" state that is threaded through two separate branches, making the three-valued return logic non-obvious (`false` = unknown; `true + waiting=true` = not ready; `true + waiting=false` = updated or no-op).
4. **In-band half-tick skip** — The `Math.Abs` no-op guard at L276 adds another path that has no side-effects but must be distinguished from a successful update for correct cache-commit semantics in the caller.

---

## 5. Risk Assessment

| Risk | Severity | Rationale |
|------|----------|-----------|
| Silent stop suppression | **High** | `waitingOnFollower=true` paths suppress cache commit indefinitely without logging |
| FSM/PositionInfo state divergence | **High** | FSM can be `Active` while PositionInfo `BracketSubmitted=false` (or vice versa) due to concurrent writes from 10+ sites |
| Missed propagation on half-tick skip | **Medium** | Caller treats this as "success" and commits cache; if `tickSize` is wrong the skip fires spuriously |
| Broker API risk via `UpdateStopOrder` | **Medium** | Delegates to a complex callee; no retry or backpressure in this path |

---

## 6. Refactoring Candidates (for Phase 1+)

- Extract `IsFollowerReady()` helper to collapse L263 + L269 into single readable predicate.
- Replace multi-predicate `&&` chains with short-circuit helper methods to reduce CYC per clause.
- Add structured log on `waitingOnFollower=true` paths (missing telemetry blind spot).
- Consider unifying FSM + PositionInfo into a single lookup so dual-source divergence cannot occur.

---

## 7. Files Implicated

| File | Role |
|------|------|
| [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs) | Target method + Shadow engine |
| [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs) | `UpdateStopOrder` callee (two-phase FSM) |
| [`src/V12_002.Symmetry.BracketFSM.cs`](src/V12_002.Symmetry.BracketFSM.cs) | `FollowerBracketFSM` / `FollowerBracketState` definitions |
| [`src/V12_002.Symmetry.cs`](src/V12_002.Symmetry.cs) | `symmetryDispatchById`, `symmetryMasterEntryToDispatch`, `symmetryFleetEntryToDispatch` |
| [`src/V12_002.Trailing.cs`](src/V12_002.Trailing.cs) | `ManageTrailingStops` → `ShadowEngineCheck` call site |
| [`src/V12_002.Orders.Callbacks.Execution.cs`](src/V12_002.Orders.Callbacks.Execution.cs) | `OnExecutionUpdate` → `ShadowEngineCheck` second call site |
| [`src/V12_002.SIMA.Dispatch.cs`](src/V12_002.SIMA.Dispatch.cs) | `_followerBrackets` write site (10+ mutations) |
| [`src/V12_002.SIMA.Fleet.cs`](src/V12_002.SIMA.Fleet.cs) | `_followerBrackets` write site |
| [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) | `_followerBrackets` recovery / rebuild |
