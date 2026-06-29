# EPIC-W7-016 — Phase 1: Scope Definition

## Single Method in Scope

| Field               | Value                                         |
|---------------------|-----------------------------------------------|
| **Method**          | `TryHandleFleet_CancelAll`                    |
| **File**            | `src/V12_002.UI.IPC.Commands.Fleet.cs`        |
| **Lines**           | 177–232                                       |
| **Current CYC**     | 21                                            |
| **Target CYC**      | ≤ 8                                           |
| **CYC Reduction**   | −13 (required)                                |
| **Callers Count**   | 1 — `TryHandleFleetCommand` at line 52        |

This phase concerns a **single method**: `TryHandleFleet_CancelAll`. No other methods are scheduled for modification in this wave phase.

---

## Scope Boundary

The **scope boundary** is drawn precisely around `TryHandleFleet_CancelAll` (lines 177–232 of `src/V12_002.UI.IPC.Commands.Fleet.cs`). Everything outside that method body is out of scope for Phase 1.

### What is inside the scope boundary

- The body of `TryHandleFleet_CancelAll` — all 55 lines (177–232)
- New private helper methods extracted from within that body during refactoring phases, if added to the same file as pure extraction artefacts
- Documentation produced specifically for this epic phase

### What is outside the scope boundary

- All other fleet handler methods in the same file (`TryHandleFleet_Trim`, `TryHandleFleet_Lock50`, `TryHandleFleet_FlattenOnly`, `TryHandleFleet_Flatten`, `TryHandleFleet_ResetMemory`, `TryHandleFleet_LongShort`, `TryHandleFleet_OrLong`, `TryHandleFleet_OrShort`, etc.)
- The caller `TryHandleFleetCommand` (line 52) — its signature and call site are not touched
- The downstream callees `CancelAll_ProcessMasterAccount` (line 234) and `CancelAll_ProcessFleetAccounts` (line 268) — these are separate, already-extracted helpers with their own CYC profiles
- Shared infrastructure helpers (`MetadataGuardDuplicate`, `CancelOrderOnAccount`) in other partial-class files
- Any other file in the `src/` tree

---

## Why Other Methods Are NOT in Scope (V12.23)

Version rule **V12.23** mandates the minimum-blast-radius principle: each refactor epic targets exactly the hotspot method identified in Phase 0, and does not bundle adjacent methods into the same change set. The reasons this rule applies here are:

1. **Isolation of risk.** `TryHandleFleet_CancelAll` directly governs live order cancellation across SIMA and single-account fleet modes. Widening the scope boundary to co-hotspots (`TryHandleFleetCommand` CYC 19, `CancelAll_ProcessMasterAccount` CYC 12, `CancelAll_ProcessFleetAccounts` CYC 9) in the same commit would multiply the verification surface and risk introducing regressions across multiple command dispatch paths simultaneously.

2. **Independent epic eligibility.** Each of the adjacent high-CYC methods (`TryHandleFleetCommand`, `CancelAll_ProcessMasterAccount`, `CancelAll_ProcessFleetAccounts`) qualifies for its own epic under Wave 7 hotspot triage. Combining them into EPIC-W7-016 would violate the single-method-per-epic constraint and confound complexity attribution.

3. **Caller immutability.** The single caller `TryHandleFleetCommand` (1 caller, confirmed by grep across all `src/` files) must not be modified during this epic. Its call site at line 52 (`if (TryHandleFleet_CancelAll(action, cmdId))`) must remain untouched so that the refactored method remains a drop-in replacement with identical signature and return semantics.

4. **V12.23 project rule.** The V12.23 governance rule explicitly prohibits scope creep into methods not named in the Phase 0 hotspot output. `00-hotspots.md` names only `TryHandleFleet_CancelAll`; therefore no other method may be modified under this epic ID.

---

## Caller Analysis

Caller discovery was performed via `grep` across the entire `src/` directory. Results:

| Caller Method          | File                                        | Line | Relationship     |
|------------------------|---------------------------------------------|------|------------------|
| `TryHandleFleetCommand`| `src/V12_002.UI.IPC.Commands.Fleet.cs`      | 52   | Direct caller    |

**Callers count: 1.** The method is called in exactly one place in the whole codebase. This minimal caller footprint confirms the scope boundary is safe to honour without any call-site updates during refactoring.

---

## Complexity Reduction Plan (Summary)

Detailed extraction plan is produced in Phase 2. The following extractions (identified in Phase 0) are the expected drivers of CYC reduction:

| # | Proposed Extraction            | Complexity Captured                        | CYC Δ |
|---|--------------------------------|--------------------------------------------|-------|
| 1 | `IsOrderCancellable(Order)`    | 5-state `OrderState` compound predicate    | −4    |
| 2 | `IsBracketOrStopOrder(string)` | 7-prefix `StartsWith` name-guard           | −6    |
| 3 | `CancelAll_SingleAccount()`    | Entire non-SIMA inline loop body           | −2    |
|   | **Total reduction**            |                                            | **−12** |

Residual CYC in `TryHandleFleet_CancelAll` after extraction: ≈ 4 (action guard + dedup guard + SIMA branch + return). Target ≤ 8 is satisfied.

---

## Agent Tracking

| Field              | Value                                           |
|--------------------|-------------------------------------------------|
| **Agent Name**     | v12-phase1-scope                                |
| **Epic**           | EPIC-W7-016                                     |
| **Wave**           | 7                                               |
| **Phase**          | 1 — Scope Definition                            |
| **Method**         | `TryHandleFleet_CancelAll`                      |
| **Source File**    | `src/V12_002.UI.IPC.Commands.Fleet.cs`          |
| **Current CYC**    | 21                                              |
| **Target CYC**     | ≤ 8                                             |
| **Callers Count**  | 1                                               |
| **Output**         | `docs/brain/EPIC-W7-016/00-scope.md`            |
| **Status**         | completed                                       |
