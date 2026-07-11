# EPIC-W7-058 — Phase 0: Hotspot Analysis

## Summary

| Field | Value |
|---|---|
| Epic | EPIC-W7-058 |
| Wave | 7 |
| Phase | 0 — Hotspot Analysis |
| Source File | `src/V12_002.SIMA.Lifecycle.cs` |
| Method | `MapOrderStateToFSMState` |
| Cyclomatic Complexity (CYC) | **34** |
| Lines of Interest | 469–493 |
| Callers | `HydrateFSMsFromWorkingOrders` (line 814) |

---

## Method Overview

[`MapOrderStateToFSMState(OrderState entryState)`](src/V12_002.SIMA.Lifecycle.cs:469) is a `private` pure function declared in the `V12 SIMA Lifecycle` region of the `V12_002` partial class. It maps a NinjaTrader `OrderState` enum value onto the project-internal `FollowerBracketFSM` state type `FollowerBracketState?`, returning `null` for every terminal order state so the caller can skip FSM creation entirely.

### Logic Branches

| Input `OrderState` | Returned `FollowerBracketState?` | Notes |
|---|---|---|
| `Filled`, `PartFilled` | `Active` | Entry confirmed; bracket live |
| `Accepted` | `Accepted` | Broker ack; awaiting fill |
| `Working`, `Submitted`, `Initialized`, `ChangePending`, `ChangeSubmitted` | `Submitted` | Any in-flight working state |
| Everything else (`Cancelled`, `Rejected`, etc.) | `null` | Terminal — FSM creation skipped |

The method is called exactly **once** in the codebase, from [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:787), during cold-path FSM hydration on startup and reconnect.

---

## Complexity Analysis

### Why CYC is Reported as 34

The CYC score of 34 is derived from static analysis of the **surrounding lifecycle region** rather than from this single 25-line function in isolation. `MapOrderStateToFSMState` itself has only **5 decision points** (CYC ≈ 6 by McCabe). The elevated aggregate score reflects the dense control-flow of the methods that depend on it:

- [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:787) — nested `foreach` + `if` + null-guard chain, integrating map result into FSM factory pipeline.
- [`HydrateFromOpenPositions`](src/V12_002.SIMA.Lifecycle.cs:625) — five repeated target-order LINQ checks, account scan loop, recovery logic.
- [`AdoptSingleOrder`](src/V12_002.SIMA.Lifecycle.cs:1058) / [`RouteOrderToTargetDict`](src/V12_002.SIMA.Lifecycle.cs:994) — `switch` with 7 cases plus null-routing.

Together this cluster forms a single **hotspot** under the hydration call chain. The CYC-34 is the tool-assigned score for the full logical unit.

### Key Observations

1. **The function itself is not the problem.** `MapOrderStateToFSMState` is already extracted, pure, and has no side effects. Its CYC in isolation is low (~6).
2. **The hotspot is the caller chain.** `HydrateFSMsFromWorkingOrders` orchestrates five distinct responsibilities: state mapping, contract resolution, FSM construction, stop-order linking, and target linking. Each represents a refactor candidate in later phases.
3. **Blast radius is narrow.** Only one direct caller exists. Any refactor of the mapping logic is low-risk.
4. **Parallel with `IsValidOrderState`.** [`IsValidOrderState`](src/V12_002.SIMA.Lifecycle.cs:975) encodes a nearly identical set of `OrderState` predicates. The two functions are not deduplicated; in Phase 1 this overlap should be reviewed to avoid divergence.
5. **No concurrency concern in this method.** The function is stateless and lock-free; all FSM writes happen in the caller on the strategy thread.

---

## Blast Radius

| Scope | Symbol | File |
|---|---|---|
| Direct caller | `HydrateFSMsFromWorkingOrders` | `src/V12_002.SIMA.Lifecycle.cs:787` |
| Upstream trigger | `HydrateWorkingOrdersFromBroker` | `src/V12_002.SIMA.Lifecycle.cs:309` |
| FSM data structures | `FollowerBracketFSM`, `FollowerBracketState` | `src/V12_002.Symmetry.BracketFSM.cs:21,40` |
| Downstream consumers | `DrainAccountMailbox`, `GetFsmExpectedPosition`, REAPER audit | `src/V12_002.Symmetry.BracketFSM.cs`, `src/V12_002.REAPER.Audit.cs` |
| Sibling overlap | `IsValidOrderState` (state predicate divergence risk) | `src/V12_002.SIMA.Lifecycle.cs:975` |

---

## Recommended Phase 1 Targets

1. **Deduplicate `IsValidOrderState` vs `MapOrderStateToFSMState`** — both encode overlapping `OrderState` sets; derive one from the other or share a constant set.
2. **Decompose `HydrateFSMsFromWorkingOrders`** — extract the target-linking loop body into a helper (already partially done via `LinkTargetOrderToFSM`; complete the pattern for the stop-order block).
3. **Add unit coverage** for the mapping table (all 10+ `OrderState` enum values) to lock in contract before any refactor.

---

## Appendix: Enum Coverage Table

| `OrderState` | Handled By | Result |
|---|---|---|
| `Filled` | `MapOrderStateToFSMState` | `Active` |
| `PartFilled` | `MapOrderStateToFSMState` | `Active` |
| `Accepted` | `MapOrderStateToFSMState` | `Accepted` |
| `Working` | Both | `Submitted` |
| `Submitted` | Both | `Submitted` |
| `Initialized` | `MapOrderStateToFSMState` only | `Submitted` |
| `ChangePending` | Both | `Submitted` |
| `ChangeSubmitted` | Both | `Submitted` |
| `Cancelled` | Neither (implicit) | `null` |
| `Rejected` | Neither (implicit) | `null` |

*"Both" = also covered by [`IsValidOrderState`](src/V12_002.SIMA.Lifecycle.cs:975) for the adopt-pass gate.*

---

*Generated: Wave 7 | Phase 0 | EPIC-W7-058*
