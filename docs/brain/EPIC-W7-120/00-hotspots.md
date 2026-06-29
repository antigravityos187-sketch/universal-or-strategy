# EPIC-W7-120 Hotspot Analysis

**Method:** HandleFsmFilled
**CYC:** 14
**File:** src/V12_002.Symmetry.BracketFSM.cs

---

## Overview

`HandleFsmFilled` (lines 349–375) is the FSM fill-event handler inside the
Follower Bracket FSM subsystem (`BracketFSM` Actor Consumer region). It is
called exclusively from [`ProcessBracketEvent`](src/V12_002.Symmetry.BracketFSM.cs:381)
on the `OrderState.Filled` / `OrderState.PartFilled` case arm.

Its job is to classify the filled order as a **stop**, **one of five named
targets**, or an **entry**, then update `fsm.RemainingContracts` and mutate
`fsm.State` (`Filled` vs. `Active`). The Cyclomatic Complexity of 14 is driven
entirely by short-circuit boolean chains used to classify `evt.SignalName`
prefix patterns; no loops are present.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ProcessBracketEvent` (line 397, `src/V12_002.Symmetry.BracketFSM.cs`) |
| **Caller chain** | `OnBarUpdate → DrainAccountMailbox → ProcessBracketEvent → HandleFsmFilled` |
| **Mailbox enqueue site** | `src/V12_002.Orders.Callbacks.AccountOrders.cs:51` (account thread) |
| **Teardown on drain** | `src/V12_002.Lifecycle.cs:261` — flushes `_accountMailbox` on shutdown |
| **FSM state written** | `fsm.State` → `FollowerBracketState.Filled` or `FollowerBracketState.Active` |
| **Shared state written** | `fsm.RemainingContracts` — volatile int read downstream by REAPER, Trailing, UI, Shadow auditor |
| **Downstream readers of `RemainingContracts`** | `REAPER.Audit`, `Trailing.StopUpdate`, `SIMA.Shadow`, `Orders.Callbacks.Execution`, `UI.Panel.StateSync`, `UI.Compliance`, `Orders.Management.Flatten` (7 files) |
| **Downstream readers of `fsm.State == Active`** | `SIMA.Lifecycle`, `SIMA.Fleet`, `Orders.Callbacks.AccountOrders`, `SIMA.Shadow`, `REAPER.Audit` (5 files) |
| **Threading constraint** | Strategy thread only (consumed via `DrainAccountMailbox` on `OnBarUpdate`); no locking needed within handler |
| **Risk on change** | **High** — `fsm.RemainingContracts` is the authoritative contract counter for all downstream bracket lifecycle decisions; incorrect decrement logic silently mis-routes to `Active` instead of `Filled`, leaving phantom positions |

**Affected symbol count (blast radius):** 1 direct caller + 12 downstream readers across 8 files;
`_accountMailbox` (`ConcurrentQueue<AccountEvent>`) is the only shared concurrent structure involved.

---

## Top 3 Complexity Drivers

### 1. Five-way target prefix fan-out (`T1_` … `T5_`) — **+6 CYC**

```csharp
bool isTarget =
    !string.IsNullOrEmpty(evt.SignalName)
    && (
        evt.SignalName.StartsWith("T1_")
        || evt.SignalName.StartsWith("T2_")
        || evt.SignalName.StartsWith("T3_")
        || evt.SignalName.StartsWith("T4_")
        || evt.SignalName.StartsWith("T5_")
    );
```

Each `||` short-circuit arm is a separate decision point (+1 CYC each: the null
guard + 5 `StartsWith` checks = **6 CYC**). The pattern is purely structural —
the five prefixes encode slot index — making it a prime candidate for a single
`IsTargetSignal(string)` helper that uses a char-range check
(`s[1] >= '1' && s[1] <= '5' && s[2] == '_'`) or a small lookup.

### 2. Stop prefix two-way OR with null guard — **+3 CYC**

```csharp
bool isStop =
    !string.IsNullOrEmpty(evt.SignalName)
    && (evt.SignalName.StartsWith("Stop_") || evt.SignalName.StartsWith("S_"));
```

Null guard (+1), `StartsWith("Stop_")` (+1), `||` arm `StartsWith("S_")` (+1).
The dual prefix exists because of a historical signal-name abbreviation (`S_`
vs. the full `Stop_`). A `IsStopSignal(string)` helper would encapsulate both
forms and reduce the inline decision count.

### 3. Compound `else if` for entry-fill detection and ternary state selector — **+3 CYC**

```csharp
if (isStop || isTarget)   // +1 for ||
{
    fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, evt.FilledQty));
    fsm.State = fsm.RemainingContracts <= 0   // +1 ternary
        ? FollowerBracketState.Filled
        : FollowerBracketState.Active;
}
else if (fsm.State == FollowerBracketState.Accepted
         || fsm.State == FollowerBracketState.Submitted)   // +1 for ||
{
    fsm.State = FollowerBracketState.Active;
}
```

The `||` in the outer `if` gate (+1), the ternary contract-to-state map (+1),
and the `||` in the `else if` guard (+1) add 3 CYC. The ternary is the
critical business-logic decision; the `else if` state guard is low-risk but
could be pushed into a `TransitionToActive(fsm)` helper (already the pattern
used by `TransitionToAccepted` at line 286).

---

## Recommended Extraction Count

**3 extractions recommended.**

| # | Proposed Helper | Removes from `HandleFsmFilled` | Estimated CYC Delta |
|---|---|---|---|
| 1 | `IsStopSignal(string signalName) → bool` | Null guard + 2 `StartsWith` arms | −3 CYC |
| 2 | `IsTargetSignal(string signalName) → bool` | Null guard + 5 `StartsWith` arms | −6 CYC |
| 3 | `ApplyFillContracts(FollowerBracketFSM fsm, int filledQty)` | Decrement + ternary state assignment | −2 CYC |

Post-extraction `HandleFsmFilled` would consist of:
1. `bool isStop = IsStopSignal(evt.SignalName);`
2. `bool isTarget = IsTargetSignal(evt.SignalName);`
3. `if (isStop || isTarget) ApplyFillContracts(fsm, evt.FilledQty);`
4. `else if (...) fsm.State = Active;`

Projected residual CYC: **≤ 4** (base + outer if + `||` gate + `else if`).

`IsStopSignal` and `IsTargetSignal` can be `private static` pure functions
(no FSM state access), making them trivially unit-testable without strategy
infrastructure.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~55s |
