# EPIC-W7-134 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field           | Value                                           |
|-----------------|------------------------------------------------ |
| **Method Name** | `MoveSpecificTarget`                            |
| **File Path**   | `src/V12_002.Trailing.Breakeven.cs`             |
| **Lines**       | 335 – 410 (76 lines)                            |
| **CYC (current)** | **11**                                        |
| **CYC (task input)** | 0 (pre-analysis placeholder; see note)    |
| **Prior CYC**   | 37 (pre-Phase7-S5-T05) → 8 (post-refactor note in XML doc) |

> **Note on CYC: 0 input value.** The task spec listed CYC: 0 as the starting value.
> Direct static analysis of the current source at lines 335–410 yields **CYC = 11**:
> base(1) + guard-if(1) + foreach(1) + containsKey-if(1) + null-check(1) +
> notFoundReason-if(1) + calc-validate-if(1) + rejectionReason-if(1) +
> isFollower-if(1) + try/catch-branch(1) + movedCount-if(1) = **11**.
> The inline doc comment at line 332 claims CYC→8 after the prior extraction sprint;
> the additional defensive guards added post-refactor account for the delta.
> **This analysis uses CYC = 11 as the confirmed live figure.**
> If tooling returns 0, it indicates the symbol index did not resolve — see Manual Review flag below.

---

## Method Located

- **Status:** ✅ LOCATED — symbol is present and readable at line 335.
- **Signature:** `private void MoveSpecificTarget(int targetNum, double profitPoints)`
- **Class:** `V12_002` (partial class, `NinjaTrader.NinjaScript.Strategies` namespace)
- **Region:** `#region Stop Management Helpers (V11)`

---

## Blast Radius Summary

### Direct Callers

| Caller File                              | Line | Context                                           |
|------------------------------------------|------|---------------------------------------------------|
| `src/V12_002.UI.IPC.Commands.Fleet.cs`   | 687  | IPC fleet command handler — parses `distance` string, converts to `profitPoints`, dispatches |

### Extracted Helper Methods (Phase7-S5-T05 — same file)

All 5 helpers live in the same partial-class file and are called exclusively by `MoveSpecificTarget`:

| Helper                              | Lines     | Role                                         |
|-------------------------------------|-----------|----------------------------------------------|
| `ValidateMoveTargetRequest`         | 166–183   | Gate: targetNum range + activePositions null check |
| `FindTargetOrderForPosition`        | 186–222   | Account-aware order lookup (master/follower routing) |
| `CalculateAndValidateNewTargetPrice`| 225–272   | Entry±profitPoints, tick-round, direction safety |
| `ExecuteFollowerTargetMove`         | 275–309   | Two-phase FSM cancel+spec for follower accounts |
| `ExecuteMasterTargetMove`           | 312–327   | `ChangeOrder` for NinjaScript-managed master orders |

### Downstream State Mutations

| Target                              | Via                          |
|-------------------------------------|------------------------------|
| `_followerTargetReplaceSpecs`       | `ExecuteFollowerTargetMove`  |
| `StampReaperMoveGrace()`            | `ExecuteFollowerTargetMove`  |
| `pos.ExecutingAccount.Cancel()`     | `ExecuteFollowerTargetMove`  |
| `ChangeOrder()`                     | `ExecuteMasterTargetMove`    |

### Cross-file Mentions (non-call)

| File                          | Line | Nature                                         |
|-------------------------------|------|------------------------------------------------|
| `src/V12_002.cs`              | 790  | Comment referencing follower path pattern      |
| `src/V12_002.Trailing.cs`     | 6    | Module header comment listing this method      |

**Blast radius is NARROW.** Single call-site (fleet IPC handler). No virtual dispatch, no event wiring, no interface implementation. Refactoring the orchestrator body is safe in isolation.

---

## Top 3 Complexity Drivers

### Driver 1 — Foreach body with 3 stacked early-exit guards (lines 347–397)
```
foreach (var kvp in activePositions.ToArray())          // loop node
{
    if (!activePositions.ContainsKey(kvp.Key)) continue // +1 (redundant TOCTOU guard)
    …
    if (targetOrder == null) { … continue; }            // +1
    if (notFoundReason != null) Print(…)                // +1 (nested inside null check)
    if (!CalculateAndValidate…) { … continue; }         // +1
    if (rejectionReason != null) Print(…)               // +1
    if (pos.IsFollower && pos.ExecutingAccount != null)  // +1 (compound predicate)
}
```
**Impact:** 6 of the 10 non-base decision points live inside this single loop body.
The `ContainsKey` re-check on a `ToArray()` snapshot is a dead branch — it can never
fire because the snapshot is immutable; removing it reduces CYC by 1 for free.

### Driver 2 — try/catch wrapping the execute dispatch (lines 381–396)
```csharp
try
{
    if (pos.IsFollower && pos.ExecutingAccount != null)   // +1
    { ExecuteFollowerTargetMove(…); }
    else
    { ExecuteMasterTargetMove(…); }
    movedCount++;
}
catch (Exception ex)                                      // +1 (catch = branch)
{
    Print(…);
}
```
**Impact:** The `try/catch` adds a hidden branch. The underlying helpers already have
their own `try/catch` internals (e.g., `ExecuteTargetAbsoluteMove` at line 505).
The outer catch degrades observability — exceptions are swallowed as `Print` noise.
Removing the outer try/catch and relying on the helper-level guards would reduce CYC
by 1 and improve error propagation.

### Driver 3 — Dual-branch null-check log emission pattern (lines 358–377)
```csharp
if (targetOrder == null)
{
    if (notFoundReason != null)    // +1 — always true by contract, yet checked anyway
        Print(notFoundReason);
    continue;
}
…
if (!CalculateAndValidate…)
{
    if (rejectionReason != null)   // +1 — same pattern
        Print(rejectionReason);
    continue;
}
```
**Impact:** Both helpers (`FindTargetOrderForPosition`, `CalculateAndValidateNewTargetPrice`)
already set their `out` reason strings unconditionally when they return false/null.
The inner null guards are therefore always true and add phantom complexity. Removing them
(call `Print` unconditionally on the `out` value) removes 2 CYC points.

---

## CYC Reduction Roadmap

| Action                                                    | CYC Saved | Risk   |
|-----------------------------------------------------------|-----------|--------|
| Remove redundant `ContainsKey` re-check on snapshot       | −1        | None   |
| Remove inner `if (notFoundReason != null)` null guard     | −1        | None   |
| Remove inner `if (rejectionReason != null)` null guard    | −1        | None   |
| Remove outer `try/catch` (rely on helper-level guards)    | −1        | Low    |
| **Total achievable reduction**                            | **−4**    |        |
| **Projected CYC after changes**                           | **7**     |        |

### Recommended Extraction Count: **0**

The 5 helpers have already been extracted in Phase7-S5-T05. No further extraction is
warranted. The remaining CYC is reducible purely by **dead-branch removal and guard
consolidation** within the existing orchestrator body — no new helper methods needed.

---

## Manual Review Flag

> ⚠️ `mcp__jcodemunch-mcp__*` tooling calls were not available in this execution
> environment. CYC, blast radius, and hotspot data above were derived by direct static
> analysis of the source file using native file-read and grep tools.
> Tooling-based CYC confirmation should be run when the MCP server is available to
> cross-check the manual figure of **11**.

---

## Agent Tracking

| Field             | Value                                    |
|-------------------|------------------------------------------|
| **Agent Name**    | v12-phase0-hotspot                       |
| **Bobcoins Used** | 7                                        |
| **Execution Time**| ~55s                                     |
| **Wave**          | 7                                        |
| **Phase**         | 0 — Hotspot Analysis                     |
| **Completed At**  | 2025-06-14                               |
