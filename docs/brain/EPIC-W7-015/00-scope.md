# EPIC-W7-015 — Phase 1: Scope Definition

## Single Method in Scope

| Field              | Value                                                        |
|--------------------|--------------------------------------------------------------|
| **Method**         | `CancelAll_ProcessSingleFleetAccount`                        |
| **File**           | `src/V12_002.UI.IPC.Commands.Fleet.cs`                       |
| **Line**           | 300                                                          |
| **Signature**      | `private int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)` |
| **Current CYC**    | 18                                                           |
| **Target CYC**     | ≤ 8                                                          |
| **CYC Reduction**  | −10 (55 % reduction)                                         |

This epic targets a **single method** only. The scope boundary is drawn tightly
around `CancelAll_ProcessSingleFleetAccount` in
[`src/V12_002.UI.IPC.Commands.Fleet.cs`](../../src/V12_002.UI.IPC.Commands.Fleet.cs:300).
No other method in this file or any other file is modified as part of this epic.

---

## Callers Analysis

Grep across the full `src/` tree for `CancelAll_ProcessSingleFleetAccount` returns
exactly **2 hits in 1 file** — the definition at line 300 and the single call-site at
line 293, both inside `src/V12_002.UI.IPC.Commands.Fleet.cs`.

| # | Caller                        | File                                       | Line | Relationship        |
|---|-------------------------------|--------------------------------------------|------|---------------------|
| 1 | `CancelAll_ProcessFleetOrders`| `src/V12_002.UI.IPC.Commands.Fleet.cs`     | 293  | Direct (sole) caller|

**Callers count: 1 direct caller** (`CancelAll_ProcessFleetOrders`).

There are no other call-sites anywhere in the repository. The method is package-private
(`private`) to the `V12_002` partial class and is not reachable from any external
assembly or test harness outside this file.

The call chain is:

```
TryHandleFleet_CancelAll          (line 177) — IPC command entry point
  └─ CancelAll_ProcessFleetAccounts (line 268)
       └─ CancelAll_ProcessFleetOrders (line 275) ← sole direct caller
            └─ CancelAll_ProcessSingleFleetAccount (line 300) ← IN SCOPE
```

---

## Scope Boundary

The **scope boundary** for EPIC-W7-015 is defined as follows:

- **Inside scope:** The body of `CancelAll_ProcessSingleFleetAccount` (lines 300–343)
  and any new private helper methods that may be extracted from it within the same
  partial-class file (`src/V12_002.UI.IPC.Commands.Fleet.cs`).
- **Outside scope:** All other methods in `src/V12_002.UI.IPC.Commands.Fleet.cs`,
  all callers of this method, all shared-state consumers of `_followerBrackets`,
  and all other files in `src/`.

The scope boundary is intentionally narrow. Refactoring helpers (e.g.,
`IsOrderCancellable`, `IsBracketOrder`, `ShouldPreserveBracket`) must remain
pure boolean predicates with **no side effects** on `_followerBrackets`,
`activePositions`, or any order-submission gateway.

---

## Why Other Methods Are NOT in Scope

The file `src/V12_002.UI.IPC.Commands.Fleet.cs` contains **23 additional methods**,
none of which are modified in this epic. The reasons are:

1. **V12.23 constraint — feature-freeze on other IPC handlers.**
   The `V12.23` build line (referenced in `src/V12_002.UI.IPC.Commands.Mode.cs:329,333`)
   marks a feature-complete boundary. Methods such as `TryHandleFleet_LongShort` (CYC ≈ 14),
   `TryHandleFleet_CancelAll` (CYC ≈ 10), and `CancelAll_ProcessMasterAccount` (CYC ≈ 8)
   are flagged for potential future waves but are **frozen** for V12.23 stabilisation.
   Touching them now risks regressions in the IPC command dispatch chain during a
   live-trading stabilisation window.

2. **Blast-radius containment.**
   `CancelAll_ProcessSingleFleetAccount` is the only method whose CYC exceeds the
   project refactor threshold (> 15) in this file. Its callers (`CancelAll_ProcessFleetOrders`,
   `CancelAll_ProcessFleetAccounts`) are thin coordinators (CYC 2–3) and require no
   structural change to achieve the target.

3. **Single-responsibility principle for wave tasks.**
   Wave 7 is organised as one epic per hotspot method. Co-refactoring additional methods
   would violate the wave's scope-isolation contract, making blast-radius assessment and
   rollback harder.

4. **No shared complexity drivers.**
   The complexity drivers in `CancelAll_ProcessSingleFleetAccount` (compound OrderState
   guard, seven-prefix bracket filter, FSM/position interlock) are all *local* to its
   loop body. None of the other 23 methods in this file share these exact patterns, so
   extracting helpers here does not benefit them and creates no obligation to modify them.

---

## Complexity Reduction Plan (Summary)

Three targeted extractions reduce CYC 18 → ≤ 8 without altering observable behaviour:

| # | Helper to Extract                                                       | CYC saved |
|---|-------------------------------------------------------------------------|-----------|
| 1 | `IsOrderCancellable(Order order) → bool`                                | −4        |
| 2 | `IsBracketOrder(string orderName) → bool`                               | −6        |
| 3 | `ShouldPreserveBracket(bool acctHasActiveFsm, bool masterHasPosition) → bool` | −2  |

Residual CYC of `CancelAll_ProcessSingleFleetAccount` after extractions: **≈ 4–6**,
well inside the ≤ 8 target.

---

## Agent Tracking

| Field                | Value                                             |
|----------------------|---------------------------------------------------|
| **Agent Name**       | v12-phase1-scope                                  |
| **Epic**             | EPIC-W7-015                                       |
| **Wave / Phase**     | Wave 7 / Phase 1                                  |
| **Method in Scope**  | `CancelAll_ProcessSingleFleetAccount`             |
| **Source File**      | `src/V12_002.UI.IPC.Commands.Fleet.cs`            |
| **Current CYC**      | 18                                                |
| **Target CYC**       | ≤ 8                                               |
| **Callers Count**    | 1 (`CancelAll_ProcessFleetOrders`)                |
| **Scope Confirmed**  | single method                                     |
| **Timestamp (UTC)**  | 2025-07-14T00:00:00Z                              |
| **Output Artifact**  | `docs/brain/EPIC-W7-015/00-scope.md`              |
