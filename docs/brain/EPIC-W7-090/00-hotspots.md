# EPIC-W7-090 Hotspot Analysis

**Method:** `OnWatchdogTimer`
**CYC:** 0 (confirmed — linear timer-callback structure; all branches are flat guard returns)
**File:** `src/V12_002.Safety.Watchdog.cs`

---

## Overview

[`OnWatchdogTimer`](src/V12_002.Safety.Watchdog.cs:36) is the heartbeat-violation detector for the
V12_002 safety subsystem. It fires every 2 000 ms on a `System.Threading.Timer` thread (not the
NinjaTrader strategy thread). Its sole job is: read `_strategyHeartbeatTicks`, compare against
`WatchdogTimeoutTicks` (5 s), and escalate through a two-stage CAS state machine when the strategy
thread appears frozen while a working order is open.

Stage 0 → 1: Enqueues [`ExecuteWatchdogLeadAccountFlatten`](src/V12_002.Safety.Watchdog.cs:211)
onto the actor queue (strategy-thread execution path — safe for NinjaTrader APIs).

Stage 1 → 2: Falls through to [`ExecuteWatchdogDirectFallback`](src/V12_002.Safety.Watchdog.cs:244)
which calls `masterAccount.Cancel()` and `masterAccount.CreateOrder()` directly (off-thread, last
resort when the actor queue itself is frozen).

The method is confirmed **CYC = 0** by static analysis: every `if`/`else` is a flat early-return
guard with no nested conditionals; the two `Interlocked.CompareExchange` calls are not counted as
branches by McCabe.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Timer owner** | `_watchdogTimer` (`System.Threading.Timer`) — field declared [`src/V12_002.cs:654`](src/V12_002.cs:654) |
| **Started by** | [`StartWatchdog()`](src/V12_002.Safety.Watchdog.cs:16) ← [`HandleRealtime`](src/V12_002.Lifecycle.cs:647) |
| **Stopped by** | [`StopWatchdog()`](src/V12_002.Safety.Watchdog.cs:25) ← [`HandleTerminated`](src/V12_002.Lifecycle.cs:195) |
| **Heartbeat source** | [`TouchStrategyHeartbeat()`](src/V12_002.UI.Snapshot.cs:20) — called from `BarUpdate`, `OnStateChange Realtime`, and `HandleRealtime` |
| **Stage field** | `_watchdogStage` (`int`, `src/V12_002.cs:655`) — 0 idle / 1 enqueued / 2 direct-fallback |
| **Stage-1 action** | [`Enqueue(ctx => ctx.ExecuteWatchdogLeadAccountFlatten())`](src/V12_002.Safety.Watchdog.cs:69) — actor queue, strategy thread |
| **Stage-2 action** | [`ExecuteWatchdogDirectFallback()`](src/V12_002.Safety.Watchdog.cs:244) — off-thread, `Account.Cancel` + `Account.CreateOrder` |
| **Guard: working order** | [`HasWatchdogLeadAccountWorkingOrder()`](src/V12_002.Safety.Watchdog.cs:112) — `ToArray()` snapshot over `masterAccount.Orders` |
| **Flatten helpers** | `CancelWatchdogWorkingOrders`, `FlattenWatchdogPositions`, `CancelDirectFallbackOrders`, `FlattenDirectFallbackPositions` |
| **Shared flatten scope** | `EnterFlattenScope()` / `ExitFlattenScope()` (`src/V12_002.cs:695,701`) — depth counter, also used by SIMA flatten |
| **Shared position state** | `SetExpectedPositionLocked(ExpKey(masterAccount.Name), 0)` — written in stage-1 path |
| **Threading risk** | Timer callback runs on a CLR thread pool thread; `Enqueue` is thread-safe but `ExecuteWatchdogDirectFallback` bypasses the actor queue entirely |
| **Termination guard** | `_isTerminating` (`volatile bool`, `src/V12_002.cs:135`) — checked first in `OnWatchdogTimer` and `ExecuteWatchdogLeadAccountFlatten` |

**Affected symbol count (blast radius):** 12 symbols directly coupled; 3 shared volatile/interlocked state fields.

---

## Top 3 Complexity Drivers

1. **Two-level CAS escalation state machine across a timer thread boundary**
   The `_watchdogStage` field is mutated by `Interlocked.CompareExchange` from a CLR thread pool
   thread (timer) and reset by `ExecuteWatchdogLeadAccountFlatten` on the strategy thread. The
   CompareExchange at line 64 (`0→1`) and line 84 (`1→2`) form an implicit state machine with
   three observable states (idle / enqueued / direct-fallback). A race between the timer firing a
   second tick and the strategy thread resetting stage to 0 is possible — the CAS makes this safe,
   but any future addition of a stage-3 must reason about all interleaving paths. This is the
   primary complexity driver even though it registers CYC = 0 under McCabe.

2. **Off-thread broker API calls in the direct fallback path**
   [`ExecuteWatchdogDirectFallback`](src/V12_002.Safety.Watchdog.cs:244) is invoked directly from
   the timer thread (no `Enqueue`). It calls `masterAccount.Cancel()` and
   `masterAccount.CreateOrder()` / `masterAccount.Submit()` — NinjaTrader broker APIs not
   documented as thread-safe from arbitrary CLR threads. This departs from the pattern used
   everywhere else in V12_002 (strategy-thread-only via actor queue) and represents a latent
   thread-safety risk that is invisible to CYC metrics.

3. **`HasWatchdogLeadAccountWorkingOrder` called three times across the escalation chain**
   The guard is checked in `OnWatchdogTimer` (line 55), in `ExecuteWatchdogLeadAccountFlatten`
   (line 216), and in `ExecuteWatchdogDirectFallback` (line 249). Each call snapshots
   `masterAccount.Orders.ToArray()` — a heap allocation on a hot timer path. Between the first
   and second check, orders can change state (TOCTOU). The redundant checks reflect defensive
   coding but add cognitive overhead and risk subtle divergence if the guard logic is ever patched
   in only one location.

---

## Recommended Extraction Count

**0 extractions recommended for Phase 0.**

**Rationale:**

`OnWatchdogTimer` itself is already thin (CYC = 0, 53 LOC including braces). The complexity
described above is **architectural**, not structural: the two-stage CAS pattern and the off-thread
broker call are design decisions that cannot be decomposed by method extraction. Phase 1 work
items should focus on:

- Documenting the threading contract of `ExecuteWatchdogDirectFallback` (is `Account.Cancel` safe
  from a CLR thread pool thread in NinjaTrader 8 Realtime state?) and adding an explicit comment.
- Evaluating whether the triple `HasWatchdogLeadAccountWorkingOrder` call should be collapsed to a
  single snapshot passed as a parameter, to eliminate TOCTOU divergence and reduce allocations.
- Confirming that `EnterFlattenScope` / `ExitFlattenScope` are re-entrant-safe when called from a
  stage-1 path on the strategy thread while a SIMA flatten may be in progress on the same thread.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~60s
