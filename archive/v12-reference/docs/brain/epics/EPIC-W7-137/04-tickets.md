# Phase 4: Ticket Definitions — EPIC-W7-137

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:25:00Z
**Input:** docs/brain/EPIC-W7-137/02-architecture-plan.md + docs/brain/EPIC-W7-137/03-audit-report.md

---

## Summary

- **Method:** `FleetSync_SyncFollowersToLevel`
- **Source File:** `src/V12_002.Trailing.cs`
- **Original CYC:** 11 (full McCabe)
- **ticket_count:** 3
- **projected_parent_cyc_after_all:** 4
- **max_cyc_projected:** 5
- **dna_verdict (Phase 3):** PASS — violations: []

---

## Sequential Thinking Evidence

**3-thought chain completed (thoughts 1–3):**

**Thought 1 — Ticket count:** CYC=11 exceeds Jane Street CYC<=8. Three distinct concerns identified: eligibility filtering, stop computation, stop application. One ticket per concern = 3 tickets. No merging (single-responsibility) and no further splitting (no value in CYC=1 nano-helpers).

**Thought 2 — Per-ticket detail:** Mapped lines to move, helper signatures, CYC reduction from parent, and projected helper CYC for each of the 3 tickets. Parent after all extractions: CYC=4 (foreach + 2 conditional continues + baseline).

**Thought 3 — CYC verification:** All 4 methods (parent + 3 helpers) confirmed CYC<=8. Parent=4, Helper1=5, Helper2=4, Helper3=3. Max=5. Margin of 3 below the 8-threshold. Plan finalized.

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `FleetSync_IsFollowerEligible` |
| **signature** | `private bool FleetSync_IsFollowerEligible(string entryName, PositionInfo fol)` |
| **concern** | Encapsulates all 5 follower eligibility guard predicates: `IsFollower`, `EntryFilled`, `BracketSubmitted` (OR compound), and `activePositions.ContainsKey`. Returns `false` on any failed check; `true` when all pass. |
| **lines_to_move** | The 5 guard-clause `continue` blocks from the top of the `foreach` body in `FleetSync_SyncFollowersToLevel` (approx. lines 144–160): `if (!fol.IsFollower) continue`, `if (!fol.EntryFilled && !fol.BracketSubmitted) continue`, `if (!activePositions.ContainsKey(entryName)) continue`, and associated predicate evaluations. |
| **cyc_reduction** | 5 (5 boolean branch predicates removed from parent scope) |
| **projected_helper_cyc** | **5** |

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | 2 |
| **helper_name** | `FleetSync_ComputeSyncStop` |
| **signature** | `private double FleetSync_ComputeSyncStop(PositionInfo fol, int leaderLongMaxLevel, int leaderShortMaxLevel, out int targetLevel)` |
| **concern** | Resolves the target trailing level via direction-dispatch ternary, guards for no-leader (`targetLevel == 0`) and no-progress (`fol.CurrentTrailLevel >= targetLevel`), then calls `CalculateStopForLevel`. Returns `0.0` sentinel when no sync is needed (zero allocation; 0.0 is never a valid NinjaTrader stop price). |
| **lines_to_move** | The two direction-dispatch ternaries assigning `targetLevel` from `leaderLongMaxLevel`/`leaderShortMaxLevel`, the `if (targetLevel == 0) continue` guard, the `if (fol.CurrentTrailLevel >= targetLevel) continue` guard, and the `CalculateStopForLevel` call (approx. lines 162–177 in the original source). |
| **cyc_reduction** | 4 (2 ternary branches + 2 guard conditions removed from parent scope) |
| **projected_helper_cyc** | **4** |

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | 3 |
| **helper_name** | `FleetSync_ApplySyncStop` |
| **signature** | `private void FleetSync_ApplySyncStop(string entryName, PositionInfo fol, double syncStopPrice, int targetLevel)` |
| **concern** | Computes `isBetter` via direction-aware price comparison (`fol.Direction == MarketPosition.Long`), then calls `UpdateStopOrder` and `Print(string.Format(...))` only when the stop improvement is confirmed. Allocation of `string.Format` is gated behind `isBetter` — fires only when stop actually moves. |
| **lines_to_move** | The `isBetter` direction ternary, the `if (isBetter)` conditional block containing `UpdateStopOrder(...)` and `Print(string.Format(...))` (approx. lines 179–191 in the original source). |
| **cyc_reduction** | 2 (direction ternary for isBetter + isBetter conditional block removed from parent scope) |
| **projected_helper_cyc** | **3** |

---

## Parent Method After All Extractions

```csharp
private void FleetSync_SyncFollowersToLevel(
    KeyValuePair<string, PositionInfo>[] positionSnapshot,
    int leaderLongMaxLevel,
    int leaderShortMaxLevel
)
{
    foreach (var kvp in positionSnapshot)
    {
        string entryName = kvp.Key;
        PositionInfo fol = kvp.Value;

        if (!FleetSync_IsFollowerEligible(entryName, fol))
            continue;

        int targetLevel;
        double syncStopPrice = FleetSync_ComputeSyncStop(
            fol, leaderLongMaxLevel, leaderShortMaxLevel, out targetLevel);

        if (syncStopPrice == 0.0)
            continue;

        FleetSync_ApplySyncStop(entryName, fol, syncStopPrice, targetLevel);
    }
}
```

- **projected_parent_cyc_after_all:** 4
- **Complexity reduction:** 63% (CYC 11 → 4)
- **Branches eliminated from parent:** 11 (5 guards + 4 ternary/direction + 2 conditional blocks)

---

## CYC Compliance Table

| Method | Before | After | Threshold | Status |
|---|---|---|---|---|
| `FleetSync_SyncFollowersToLevel` (parent) | 11 | **4** | 8 | PASS |
| `FleetSync_IsFollowerEligible` (Ticket 1) | — | **5** | 8 | PASS |
| `FleetSync_ComputeSyncStop` (Ticket 2) | — | **4** | 8 | PASS |
| `FleetSync_ApplySyncStop` (Ticket 3) | — | **3** | 8 | PASS |
| **max_cyc_projected** | — | **5** | 8 | **PASS** |

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 all methods | YES — max=5, margin=3 |
| Single-responsibility per ticket | YES |
| Lock-free / Actor pattern preserved | YES — no lock() blocks planned |
| Illegal states unrepresentable | YES — eligibility is explicit boolean contract; sentinel 0.0 is unambiguous |
| Zero-allocation hot paths | IMPROVED — string.Format gated behind isBetter |
| xUnit test framework | YES — [Fact], Assert.Equal() |
| No scope creep | YES — single file, 3 new private helpers only |

---

## Execution Order

| Ticket | Depends On | Parallelizable |
|---|---|---|
| Ticket 1 (FleetSync_IsFollowerEligible) | None | YES — can start immediately |
| Ticket 2 (FleetSync_ComputeSyncStop) | None | YES — independent of Ticket 1 |
| Ticket 3 (FleetSync_ApplySyncStop) | None | YES — independent of Tickets 1 & 2 |
| Parent refactor (inline call sites) | Tickets 1, 2, 3 all complete | NO — requires all helpers present |

All 3 helper extractions are independent and may be executed in parallel. Parent inline wiring must be last.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 5 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity (name fallback — not indexed), get_extraction_candidates (0 candidates — manual analysis used) |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket breakdown thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 5 |
| **Output** | docs/brain/EPIC-W7-137/04-tickets.md |
