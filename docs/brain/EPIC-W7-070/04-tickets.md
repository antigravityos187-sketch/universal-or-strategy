# Phase 4: Ticket Definitions — EPIC-W7-070

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-070/02-architecture-plan.md + docs/brain/EPIC-W7-070/03-audit-report.md

---

## Epic Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-070 |
| **Method** | `HydrateFSMsFromWorkingOrders` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Lines** | 787–891 (105 LOC) |
| **Original CYC** | 13 |
| **Target max CYC** | 7 (all methods ≤ 8) |
| **Extraction Count** | 2 private helpers |
| **DNA Verdict** | PASS (Phase 3) |
| **Ticket Count** | 6 |

---

## CYC Reduction Plan

| Method | Before | After | Delta |
|---|---|---|---|
| `HydrateFSMsFromWorkingOrders` (parent) | 13 | 3 | -10 |
| `ProcessEntryOrderForFSMHydration` (helper 1) | — | 7 | new |
| `LinkStopOrderIfPresent` (helper 2) | — | 3 | new |
| **max_cyc** | **13** | **7** | **-46%** |

---

## Ticket Dependency Chain

```
T1 (baseline audit, read-only)
  └─> T2 (extract LinkStopOrderIfPresent, CYC=3)
        └─> T3 (extract ProcessEntryOrderForFSMHydration, CYC=7, calls T2 helper)
              └─> T4 (simplify parent HydrateFSMsFromWorkingOrders, CYC=3)
                    └─> T5 (build verification)
                          └─> T6 (DNA/CYC final audit)
```

---

## T1 — Baseline Audit

**Type:** Read-only audit (no code changes)
**Depends on:** None

### Description

Before any extraction begins, read and document the current state of `HydrateFSMsFromWorkingOrders` at lines 787–891 in [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:787). Record the current cyc value, enumerate all branch points, and confirm the source matches the architecture plan.

### Tasks

1. Read `src/V12_002.SIMA.Lifecycle.cs` lines 787–891.
2. Confirm method signature: `private void HydrateFSMsFromWorkingOrders()`.
3. Count and list all conditional branches (if, foreach, null checks, TryGetValue, ContainsKey, IsNullOrEmpty).
4. Confirm original cyc = 13.
5. Confirm callers: `HydrateWorkingOrdersFromBroker` (line 445) and `EnumerateApexAccounts` (line 140).
6. Document that blast radius = zero (private method, no external callers).

### Acceptance Criteria

- [ ] Source lines 787–891 confirmed readable and match architecture plan description.
- [ ] CYC = 13 recorded in ticket notes.
- [ ] Branch inventory lists all 13 conditional paths.
- [ ] Both callers identified and noted as unaffected by signature-preserving extraction.
- [ ] No code changes made in this ticket.

### Estimated CYC Reduction

**0** (read-only — establishes verified baseline for subsequent tickets)

---

## T2 — Extract `LinkStopOrderIfPresent` Helper

**Type:** Surgical extraction
**Depends on:** T1

### Description

Extract the stop-order association block from within the `HydrateFSMsFromWorkingOrders` foreach loop body into a new private helper method `LinkStopOrderIfPresent`. This is the simpler of the two extractions (target cyc = 3) and must be done first because `ProcessEntryOrderForFSMHydration` (T3) will call it.

### Extraction Target

**Signature:**
```csharp
/// <summary>
/// Links the stop order (if present and valid) to the FSM and indexes its order ID.
/// Single responsibility: stop order association only.
/// </summary>
private void LinkStopOrderIfPresent(
    FollowerBracketFSM fsm,
    string entryKey,
    ref int ordersIndexed)
```

**Logic to extract:**
1. `stopOrders.TryGetValue(entryKey, out stopOrd)` — dictionary lookup
2. Null check on `stopOrd` — early return if null
3. `fsm.StopOrder = stopOrd` — association assignment
4. `string.IsNullOrEmpty(stopOrd.OrderId)` check — guard before indexing
5. `_orderIdToFsmKey[stopOrd.OrderId] = entryKey` — index insertion
6. `ordersIndexed++` — counter increment

**Placement:** Add as new private method in `src/V12_002.SIMA.Lifecycle.cs` adjacent to `HydrateFSMsFromWorkingOrders`.

**Extraction site:** Replace the extracted block in the foreach loop body with a call to `LinkStopOrderIfPresent(fsm, kvp.Key, ref ordersIndexed)`.

### Acceptance Criteria

- [ ] `LinkStopOrderIfPresent(FollowerBracketFSM fsm, string entryKey, ref int ordersIndexed)` exists as `private void` in the file.
- [ ] Method body contains: TryGetValue lookup + null guard + StopOrder assignment + IsNullOrEmpty guard + `_orderIdToFsmKey` insertion + `ordersIndexed++`.
- [ ] CYC of new method = 3.
- [ ] Original extraction site replaced with single call to `LinkStopOrderIfPresent`.
- [ ] `dotnet build` passes with zero new warnings.
- [ ] No other files modified.

### Estimated CYC Reduction

Parent method branch count reduced by **3** (stop-order block removed from parent loop body).

---

## T3 — Extract `ProcessEntryOrderForFSMHydration` Helper

**Type:** Surgical extraction
**Depends on:** T2

### Description

Extract the full foreach loop body (lines 797–854) from `HydrateFSMsFromWorkingOrders` into a new private helper `ProcessEntryOrderForFSMHydration`. This is the primary complexity-reduction ticket. The extracted method handles the complete single-entry-order FSM hydration lifecycle including guard clauses, state mapping, contract resolution, FSM construction, stop-order linking (via `LinkStopOrderIfPresent` from T2), target-order linking, and FSM registration.

### Extraction Target

**Signature:**
```csharp
/// <summary>
/// Processes a single entry order through the FSM hydration lifecycle.
/// Applies guard clauses, resolves FSM state, builds FSM, links orders, and registers.
/// Called exclusively from HydrateFSMsFromWorkingOrders entry order pass.
/// </summary>
private void ProcessEntryOrderForFSMHydration(
    string entryKey,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated)
```

**Guard clauses (4 early returns at top of method):**
1. `if (entryOrder == null) return;`
2. `if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower) return;`
3. `if (pi.ExecutingAccount == null) return;`
4. `if (_followerBrackets.ContainsKey(entryKey)) return;` — idempotent guard

**Then (in order):**
- `MapOrderStateToFSMState(entryOrder)` → state resolution
- `FindLivePosition(...)` (conditional) → position lookup
- `ResolveRemainingContracts(...)` → contract resolution
- `BuildFSM(...)` → FSM construction
- `LinkStopOrderIfPresent(fsm, entryKey, ref ordersIndexed)` — calls T2 helper
- 5× `LinkTargetOrderToFSM(...)` (target1–5Orders)
- `RegisterFSM(...)` → registration + counter increment

**Target CYC:** 7

**Extraction site:** Replace foreach body in parent with `ProcessEntryOrderForFSMHydration(kvp.Key, kvp.Value, ref ordersIndexed, ref fsmCreated)`.

### Acceptance Criteria

- [ ] `ProcessEntryOrderForFSMHydration(string, Order, ref int, ref int)` exists as `private void` in the file.
- [ ] Method begins with exactly 4 guard-clause early returns.
- [ ] Method calls `LinkStopOrderIfPresent` internally (T2 dependency respected).
- [ ] Method calls `LinkTargetOrderToFSM` exactly 5 times (target1–5).
- [ ] CYC of new method ≤ 8 (target: 7).
- [ ] Foreach loop body in `HydrateFSMsFromWorkingOrders` replaced by single-line delegation.
- [ ] `dotnet build` passes with zero new warnings.
- [ ] No other files modified.

### Estimated CYC Reduction

Parent method cyc reduced from 13 → approximately 4 (delegates entire loop body). Combined with T2, parent is staged for final simplification in T4.

---

## T4 — Simplify Parent `HydrateFSMsFromWorkingOrders`

**Type:** Refactor (orchestration shell)
**Depends on:** T2, T3

### Description

After T2 and T3 extractions, finalize the parent `HydrateFSMsFromWorkingOrders` as a clean orchestration shell. Confirm the parent's remaining body matches the architecture plan exactly and achieves target cyc = 3.

### Expected Final Body

```csharp
private void HydrateFSMsFromWorkingOrders()
{
    int fsmCreated = 0;
    int ordersIndexed = 0;

    Print("[SIMA] Phase 5 FSM Hydration: Starting entry order pass...");

    foreach (var kvp in entryOrders.ToArray())
        ProcessEntryOrderForFSMHydration(kvp.Key, kvp.Value, ref ordersIndexed, ref fsmCreated);

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Entry Pass): {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed));

    int positionFsmCreated = HydrateFromOpenPositions(
        stopOrders, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders,
        ref ordersIndexed, ref fsmCreated);

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
        positionFsmCreated));

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed));
}
```

### Tasks

1. Review parent method after T2+T3 to confirm it matches the architecture plan body above.
2. Remove any residual inline branches that were not extracted in T2/T3.
3. Confirm final cyc = 3 (1 foreach + method entry + base = 3).
4. Confirm callers `HydrateWorkingOrdersFromBroker` and `EnumerateApexAccounts` still compile unmodified.

### Acceptance Criteria

- [ ] `HydrateFSMsFromWorkingOrders` body matches architecture plan final form.
- [ ] Parent CYC = 3.
- [ ] Callers `HydrateWorkingOrdersFromBroker` and `EnumerateApexAccounts` unaffected (same call signature).
- [ ] `dotnet build` passes with zero new warnings.
- [ ] No other files modified.

### Estimated CYC Reduction

**Final parent cyc: 3** (down from 13 = -77% reduction in parent complexity).

---

## T5 — Build Verification

**Type:** Verification gate
**Depends on:** T4

### Description

Run full build pipeline to confirm all extractions compile cleanly. Re-sync NinjaTrader hard links via `deploy-sync.ps1`.

### Tasks

1. Run `dotnet build` from repository root.
2. Confirm exit code = 0.
3. Confirm zero new CS-prefixed warnings compared to pre-extraction baseline.
4. Run `powershell -File .\deploy-sync.ps1` to re-synchronize NinjaTrader hard links.
5. Confirm deploy-sync exits cleanly.

### Acceptance Criteria

- [ ] `dotnet build` exits with code 0.
- [ ] Zero new compiler warnings (CS-prefixed) introduced by this epic.
- [ ] `deploy-sync.ps1` completes without errors.
- [ ] Build output references `src/V12_002.SIMA.Lifecycle.cs` with no error lines.

### Estimated CYC Reduction

**N/A** — verification ticket; no code changes.

---

## T6 — DNA & CYC Final Audit

**Type:** Final audit gate
**Depends on:** T5

### Description

Perform final DNA compliance audit across all three methods affected by this extraction. Confirm all cyc values are within the Jane Street ≤8 mandate, no lock() blocks were introduced, all string literals remain ASCII-only, and the diff touches only the target file.

### Audit Checks

| Check | Tool/Method | Pass Criterion |
|---|---|---|
| Parent CYC | Complexity count | `HydrateFSMsFromWorkingOrders` CYC = 3 ≤ 8 |
| Helper 1 CYC | Complexity count | `ProcessEntryOrderForFSMHydration` CYC = 7 ≤ 8 |
| Helper 2 CYC | Complexity count | `LinkStopOrderIfPresent` CYC = 3 ≤ 8 |
| max_cyc | All above | max(3, 7, 3) = 7 ≤ 8 |
| No lock() blocks | `grep "lock("` | 0 matches in `src/V12_002.SIMA.Lifecycle.cs` |
| ASCII-only literals | Manual review | No Unicode/emoji/curly quotes in new code |
| Scope containment | `git diff --name-only` | Only `src/V12_002.SIMA.Lifecycle.cs` in diff |
| No empty-catch | AST search | 0 empty-catch blocks |

### Acceptance Criteria

- [ ] `HydrateFSMsFromWorkingOrders` CYC ≤ 8 (target: 3).
- [ ] `ProcessEntryOrderForFSMHydration` CYC ≤ 8 (target: 7).
- [ ] `LinkStopOrderIfPresent` CYC ≤ 8 (target: 3).
- [ ] max_cyc across all 3 methods = 7.
- [ ] Zero `lock(` occurrences in `src/V12_002.SIMA.Lifecycle.cs`.
- [ ] All new string literals are ASCII-only.
- [ ] `git diff --name-only` shows only `src/V12_002.SIMA.Lifecycle.cs`.
- [ ] Epic marked complete in manifest.json.

### Estimated CYC Reduction

**Cumulative:** Original parent CYC 13 → max_cyc across all methods = 7 (**-46% reduction**).

---

## Sequential Thinking Summary

**4-thought chain completed. Final conclusion:**

- 6 tickets identified with clear dependency chain: T1→T2→T3→T4→T5→T6
- T2 must precede T3 because `ProcessEntryOrderForFSMHydration` calls `LinkStopOrderIfPresent`
- T4 finalizes the parent after both helpers are extracted
- T5 and T6 are verification/audit gates — no code changes
- All acceptance criteria are binary and testable
- CYC reduction: parent 13→3 (-77%), max_cyc across all = 7 (≤8 Jane Street mandate met)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **Method** | HydrateFSMsFromWorkingOrders |
| **jCodemunch tools called** | resolve_repo |
| **sequential-thinking calls** | 4 |
| **Ticket Count** | 6 |
| **Output** | docs/brain/EPIC-W7-070/04-tickets.md |
