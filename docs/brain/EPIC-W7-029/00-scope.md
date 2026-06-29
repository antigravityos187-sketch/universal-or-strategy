# EPIC-W7-029 — Phase 1: Scope Definition

## Single Method in Scope

| Field            | Value                                    |
|------------------|------------------------------------------|
| **Method**       | `ShouldSkipFleet_RunHealthCheck`         |
| **Source File**  | `src/V12_002.SIMA.Fleet.cs`             |
| **Lines**        | 478–511                                  |
| **Visibility**   | `private void`                           |
| **Class**        | `V12_002` (partial) — `NinjaTrader.NinjaScript.Strategies` |
| **CYC (Current)**| **0** (post-refactor thin dispatcher)   |
| **CYC (Target)** | **≤ 8**                                  |

This scope document governs a **single method**: `ShouldSkipFleet_RunHealthCheck`.
No other method is included within this scope boundary.

---

## CYC Assessment

- **Current CYC: 0** — confirmed by Phase 0 hotspot analysis (`00-hotspots.md`).
  The T-W1 refactor (Build 935) decomposed the original monolithic body (historical CYC = 31)
  into four extracted helpers. The dispatcher now contains only a null guard, four delegating
  helper calls, and a `try/catch` wrapper — no internal branching decisions remain.
- **Target CYC: ≤ 8** — the standard Wave 7 ceiling for methods in this epic series.
  The method already satisfies the target; Phase 1 confirms compliance rather than prescribing
  reduction work.

---

## Callers

Grep of `src/` for `ShouldSkipFleet_RunHealthCheck` produced **3 matches** across
**1 file** (`src/V12_002.SIMA.Fleet.cs`):

| Line | Role          | Context                                            |
|------|---------------|----------------------------------------------------|
| 465  | **Call site** | `ShouldSkipFleet_RunHealthCheck(acct, dispatchLog)` — called from `ShouldSkipFleetAccount` (Step 2 of fleet gate) |
| 478  | Definition    | Method declaration                                 |
| 508  | String ref    | Catch-block `Print` message only — not a call      |

**Caller count: 1** — `ShouldSkipFleetAccount` at `src/V12_002.SIMA.Fleet.cs:465` is the
sole direct caller. `ShouldSkipFleetAccount` is itself invoked from the hot fleet loop in
`ExecuteSmartDispatchEntry` (`V12_002.SIMA.Dispatch.cs`), making this a transitive hot-path
participant, but the subject method's direct call surface is exactly **one call site**.

---

## Scope Boundary

The **scope boundary** for EPIC-W7-029 Phase 1 is drawn precisely and exclusively around the
**single method** `ShouldSkipFleet_RunHealthCheck` (lines 478–511,
`src/V12_002.SIMA.Fleet.cs`). Everything outside this boundary — including the four extracted
helpers (`IsBrokerPositionFlat`, `HasActiveFsmForAccount`, `HasActivePositionForAccount`,
`LogHealthCheckResult`), the caller (`ShouldSkipFleetAccount`), and all other fleet-file
methods — is **out of scope** for this phase.

---

## Why Other Methods Are NOT in Scope (V12.23)

Per **V12.23** (Wave 7 single-method scoping constraint), each epic in Wave 7 targets exactly
one method per phase boundary. The following classes of methods are explicitly excluded:

1. **Extracted helpers** (`IsBrokerPositionFlat`, `HasActiveFsmForAccount`,
   `HasActivePositionForAccount`, `LogHealthCheckResult`): These were produced by the T-W1
   refactor as downstream recipients of complexity migrated out of the subject method. They carry
   their own CYC values and are candidates for independent epics if their own CYC exceeds the
   Wave 7 ceiling. Including them here would violate the single-method scope contract and
   conflate separate complexity domains.

2. **Caller** (`ShouldSkipFleetAccount`): The caller establishes the invocation context but is
   not the subject of this epic. Its CYC is governed by a separate entry in the Wave 7 backlog.
   Changes to the caller's decision tree are out of scope.

3. **Other fleet-file hotspots** (`VerifyPhotonSlotIntegrity` est. CYC ~14,
   `InitializeFollowerBracketFSM` est. CYC ~9, `DrainAllDispatchQueuesOnAbort` est. CYC ~8):
   These are higher-priority CYC reduction targets in their own right and must be addressed
   under separate epics. Folding them into EPIC-W7-029 would exceed the V12.23 scope boundary
   and corrupt blast-radius isolation for this phase.

4. **Any method in other files** (`V12_002.SIMA.Dispatch.cs`, `V12_002.cs`, etc.): The
   source file constraint (`src/V12_002.SIMA.Fleet.cs`) is a hard scope boundary under V12.23.
   Cross-file inclusions require a separate epic with its own Phase 0 hotspot analysis.

---

## Structural Summary

```
ShouldSkipFleet_RunHealthCheck          ← SINGLE METHOD IN SCOPE
│
├── [PR6-P0] null guard (acct / acct.Positions)
├── IsBrokerPositionFlat(acct)          ← callee, OUT OF SCOPE
├── HasActiveFsmForAccount(acct.Name)   ← callee, OUT OF SCOPE
├── HasActivePositionForAccount(acct.Name) ← callee, OUT OF SCOPE
├── _dispatchSyncPendingExpKeys.ContainsKey(ExpKey(acct.Name))
├── LogHealthCheckResult(...)           ← callee, OUT OF SCOPE
└── catch (Exception ex) → _diagFleet-gated Print
```

**CYC = 0** — dispatcher is a pure coordinator; all branching logic lives in extracted helpers.
Target CYC ≤ 8 is already satisfied. Phase 1 status: **compliant, no extraction required**.

---

## Agent Tracking Block

```json
{
  "epic": "EPIC-W7-029",
  "wave": 7,
  "phase": 1,
  "task": "Scope Definition",
  "agent_name": "v12-phase1-scope",
  "method": "ShouldSkipFleet_RunHealthCheck",
  "source_file": "src/V12_002.SIMA.Fleet.cs",
  "cyc_current": 0,
  "cyc_target": 8,
  "callers_count": 1,
  "caller_sites": [
    "src/V12_002.SIMA.Fleet.cs:465 — ShouldSkipFleetAccount"
  ],
  "scope_confirmed_single_method": true,
  "scope_boundary": "lines 478-511, src/V12_002.SIMA.Fleet.cs only",
  "out_of_scope_reason": "V12.23 single-method scoping constraint",
  "extraction_required": false,
  "output_artifact": "docs/brain/EPIC-W7-029/00-scope.md",
  "status": "completed"
}
```
