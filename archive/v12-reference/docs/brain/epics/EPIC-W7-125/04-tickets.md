# EPIC-W7-125 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:25:00Z
**Input:** docs/brain/EPIC-W7-125/02-architecture-plan.md + docs/brain/EPIC-W7-125/03-audit-report.md

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 6 |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic ID** | EPIC-W7-125 |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity (x2), search_symbols, sequentialthinking (3 thoughts) |
| **dna_verdict_input** | PASS (from Phase 3) |
| **ticket_count** | 2 |

---

## Summary

| Property | Value |
|---|---|
| **Method** | `ShadowPropagateStopMoves` |
| **File** | `src/V12_002.SIMA.Shadow.cs` |
| **CYC Baseline (historical)** | 20 |
| **CYC Current (parent)** | 4 |
| **Gap** | `ValidateCachedEntry` CYC=9 (MCP-confirmed live) — 1 over V12 ≤8 threshold |
| **extraction_count** | 1 |
| **max_cyc_projected** | 8 (`ValidateLeaderPosition`, unchanged) |
| **Total Tickets** | 2 |

---

## MCP Complexity Evidence

| Symbol | CYC (MCP Live) | Assessment | Source |
|---|---|---|---|
| `ValidateCachedEntry` | **9** | medium | `get_symbol_complexity` — `src/V12_002.SIMA.Shadow.cs:158`, param_count=5, lines=25 |
| `ShadowPropagateStopMoves` | 4 | low | Phase 2 confirmed (symbol not in current index segment; value authoritative from Phase 2 MCP run) |

---

## Sequential Thinking Evidence

| Thought | Conclusion |
|---|---|
| **Thought 1** | Scoped 2-ticket plan: T1 (extraction impl) + T2 (verification). Single-file, single-extraction. Blast radius confirmed zero. |
| **Thought 2** | T1 details locked: insert ValidateCachedPosition at ~line 182 (after ValidateCachedEntry). Refactor ValidateCachedEntry body to delegate position-side guard. CYC targets: both methods → 5. |
| **Thought 3** | All inputs validated live. ValidateCachedEntry CYC=9 confirmed via `get_symbol_complexity`. 2-ticket structure is minimal and sufficient. Ready to write 04-tickets.md. |

---

## Ticket T1 — Extract `ValidateCachedPosition` from `ValidateCachedEntry`

| Field | Value |
|---|---|
| **ID** | `EPIC-W7-125-T1` |
| **Type** | `extraction` |
| **File** | `src/V12_002.SIMA.Shadow.cs` |
| **Target Method** | `ValidateCachedEntry` (line 158) |
| **New Method** | `ValidateCachedPosition` |
| **CYC Before** | `ValidateCachedEntry` = 9 (violation) |
| **CYC Target** | `ValidateCachedEntry` = 5, `ValidateCachedPosition` = 5 |
| **Priority** | P0 — blocking (cyc violation active) |

### Description

`ValidateCachedEntry` (CYC=9) contains a single compound `if` with 8 chained `||` conditions across two logical groups:
- **Group A (5 conditions):** Position liveness — `TryGetValue`, null check, `IsFollower`, `EntryFilled`, `RemainingContracts`
- **Group B (3 conditions):** Stop order validity — `TryGetValue`, null check, `StopPrice > 0`

This ticket extracts **Group A** into a new private static helper `ValidateCachedPosition`, reducing `ValidateCachedEntry` from CYC=9 to CYC=5 and making both methods independently unit-testable.

### New Method to Add

Insert the following method into `src/V12_002.SIMA.Shadow.cs`, immediately after `ValidateCachedEntry` (approximately line 183):

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool ValidateCachedPosition(
    string entryKey,
    ConcurrentDictionary<string, PositionInfo> activePositions,
    out PositionInfo livePos
)
{
    return activePositions.TryGetValue(entryKey, out livePos)
        && livePos != null
        && !livePos.IsFollower
        && livePos.EntryFilled
        && livePos.RemainingContracts > 0;
}
```

**Projected CYC:** 1 (base) + 4 (&&/|| short-circuit branches) = **5** ≤ 8 ✓

### Refactored `ValidateCachedEntry`

Replace the body of `ValidateCachedEntry` (lines 158–182) with:

```csharp
private static bool ValidateCachedEntry(
    string entryKey,
    ConcurrentDictionary<string, PositionInfo> activePositions,
    ConcurrentDictionary<string, Order> stopOrders
)
{
    PositionInfo livePos;
    Order liveStop;

    if (!ValidateCachedPosition(entryKey, activePositions, out livePos))
    {
        return false;
    }
    if (!stopOrders.TryGetValue(entryKey, out liveStop)
        || liveStop == null
        || liveStop.StopPrice <= 0)
    {
        return false;
    }

    return true;
}
```

**Projected CYC:** 1 (base) + 1 (if #1) + 1 (if #2) + 1 (|| null) + 1 (|| StopPrice) = **5** ≤ 8 ✓

### Jane Street Constraints

| Pattern | Requirement |
|---|---|
| **carl_cook — AggressiveInlining** | Apply `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to `ValidateCachedPosition` — pure predicate, zero branches on hot path |
| **trading_billions — Single Responsibility** | `ValidateCachedPosition`: concern = "is the position side of this cache key alive?". `ValidateCachedEntry`: concern = "are BOTH position AND stop alive?" |
| **gjengset — Lock-Free** | No `lock()` blocks. Use existing `ConcurrentDictionary` + `TryGetValue` atomics only |
| **ASCII-Only** | No Unicode, emoji, or curly quotes in any string literal or comment |

### Acceptance Criteria

- [ ] `ValidateCachedPosition` method added to `src/V12_002.SIMA.Shadow.cs` with `[AggressiveInlining]` attribute
- [ ] `ValidateCachedEntry` body refactored to call `ValidateCachedPosition` for Group A guard
- [ ] No other methods in the file modified
- [ ] No `lock()` blocks introduced
- [ ] All identifiers and string literals are ASCII-only
- [ ] `dotnet build` passes with zero errors and zero new warnings
- [ ] `cyc(ValidateCachedEntry)` ≤ 8 (target: 5)
- [ ] `cyc(ValidateCachedPosition)` ≤ 8 (target: 5)

---

## Ticket T2 — Verify CYC Compliance and Build Health

| Field | Value |
|---|---|
| **ID** | `EPIC-W7-125-T2` |
| **Type** | `verification` |
| **File** | `src/V12_002.SIMA.Shadow.cs` |
| **Depends On** | `EPIC-W7-125-T1` (extraction must be applied first) |
| **CYC Target** | All 6 in-scope methods ≤ 8; `max_cyc_in_scope` = 8 |
| **Priority** | P0 — gate for epic completion |

### Description

Post-extraction verification pass. Confirms the extraction in T1 has resolved the cyc violation and the file remains build-clean. No code changes are made in this ticket — it is a measurement and sign-off step.

### Verification Steps

1. Run `dotnet build` — must exit 0 with zero errors and zero new warnings
2. Run `python scripts/complexity_audit.py` — confirm `ValidateCachedEntry` ≤ 8 and `ValidateCachedPosition` ≤ 8
3. Run MCP `get_symbol_complexity` for `ValidateCachedEntry` — expected CYC ≤ 8
4. Confirm all 6 in-scope methods comply:

| Method | Projected CYC | Verified CYC | Status |
|---|---|---|---|
| `ShadowPropagateStopMoves` | 4 | _measure_ | ≤ 8 required |
| `ValidateLeaderPosition` | 8 | _measure_ | ≤ 8 required |
| `DetectStopPriceChange` | 2 | _measure_ | ≤ 8 required |
| `PropagateAndCacheStopPrice` | 2 | _measure_ | ≤ 8 required |
| `ValidateCachedEntry` (refactored) | **5** | _measure_ | ≤ 8 required — was 9 |
| `ValidateCachedPosition` (new) | **5** | _measure_ | ≤ 8 required — new |

### Acceptance Criteria

- [ ] `dotnet build` exits 0 with zero errors and zero new warnings
- [ ] `cyc(ValidateCachedEntry)` confirmed ≤ 8 by measurement (target 5)
- [ ] `cyc(ValidateCachedPosition)` confirmed ≤ 8 by measurement (target 5)
- [ ] `max_cyc_in_scope` = 8 (`ValidateLeaderPosition`, unchanged)
- [ ] No regressions in any other method in `src/V12_002.SIMA.Shadow.cs`
- [ ] `deploy-sync.ps1` executed successfully (NinjaTrader hard-link sync)
- [ ] `ticket-1-completion.md` and `ticket-2-completion.md` written to `docs/brain/EPIC-W7-125/`

---

## Ticket Index

| ID | Type | CYC Target | File | Depends On |
|---|---|---|---|---|
| `EPIC-W7-125-T1` | extraction | ValidateCachedEntry → 5, ValidateCachedPosition → 5 | `src/V12_002.SIMA.Shadow.cs` | — |
| `EPIC-W7-125-T2` | verification | max_cyc_in_scope = 8 | `src/V12_002.SIMA.Shadow.cs` | T1 |

**Total tickets: 2**
