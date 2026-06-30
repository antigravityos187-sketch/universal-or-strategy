# Phase 4: Ticket Generation — EPIC-W7-064

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:25:00Z
**Inputs:**
- `docs/brain/EPIC-W7-064/02-architecture-plan.md`
- `docs/brain/EPIC-W7-064/03-audit-report.md`

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `ResolveFsm_ByScan` |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Lines** | 209–246 |
| **Original CYC** | 11 |
| **Signature** | `private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)` |
| **DNA Verdict** | PASS |

---

## Sequential Thinking Summary

**Thought 1 — Ticket count decision:**
`ResolveFsm_ByScan` contains exactly one separable concern beyond the parent's orchestration logic: the per-FSM slot scan (Stop → Targets[0-4] → Entry) combined with `_orderIdToFsmKey` backfill. One concern = one ticket. `ticket_count = 1`.

**Thought 2 — Ticket detail breakdown:**
Ticket 1 extracts the inner scan/match body from the foreach loop into `MatchOrderInFsm(FollowerBracketFSM f, string orderId)`. Lines moved: the 3-slot scan branches previously inline in the loop body (approx. lines 220–240), plus removal of dead-code `bool foundT` (lines 225, 234–235). Parent retains null-guard, foreach shell, account filter, and delegating call. CYC delta for parent: 11 → 5. Helper CYC: 5.

**Thought 3 — CYC verification:**
Parent `ResolveFsm_ByScan` post-extraction: CYC = 5 (1 base + 1 IsNullOrEmpty guard + 1 foreach + 1 AccountName filter + 1 match != null). Helper `MatchOrderInFsm`: CYC = 5 (1 base + 1 StopOrder check + 1 for loop + 1 Targets check + 1 EntryOrder check). Both ≤ 8. `max_cyc_projected = 5`. Jane Street mandate satisfied. All callers unaffected (private signature unchanged).

---

## ticket_count: 1

---

## Ticket Definitions

---

### TICKET-1

| Field | Value |
|---|---|
| **ticket_id** | TICKET-1 |
| **epic_id** | EPIC-W7-064 |
| **helper_name** | `MatchOrderInFsm` |
| **concern** | Per-FSM slot scan (StopOrder → Targets[0-4] → EntryOrder) with `_orderIdToFsmKey` backfill on match; dead-code `bool foundT` removal |
| **source_file** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **lines_to_move** | ~220–240 (inner loop body of `ResolveFsm_ByScan`; 3-slot match branches + backfill + dead-code removal) |
| **cyc_reduction** | 6 (parent: 11 → 5) |
| **projected_helper_cyc** | 5 |
| **projected_parent_cyc** | 5 |
| **insertion_point** | Immediately after `ResolveFsm_ByScan` in same partial class |
| **access_modifier** | `private` |
| **return_type** | `FollowerBracketFSM` (returns `f` on match, `null` on no match) |
| **parameters** | `FollowerBracketFSM f`, `string orderId` |
| **side_effects** | Writes to `_orderIdToFsmKey[orderId]` (ConcurrentDictionary, lock-free) on match |
| **dead_code_removed** | `bool foundT` declaration + `foundT = true` assignment + `if (foundT) break` guard |

#### Helper Signature

```csharp
private FollowerBracketFSM MatchOrderInFsm(FollowerBracketFSM f, string orderId)
```

#### Helper Reference Implementation

```csharp
private FollowerBracketFSM MatchOrderInFsm(FollowerBracketFSM f, string orderId)
{
    if (f.StopOrder != null && f.StopOrder.OrderId == orderId)
    {
        _orderIdToFsmKey[orderId] = f.EntryName;
        return f;
    }

    for (int i = 0; i < 5; i++)
    {
        if (f.Targets[i] != null && f.Targets[i].OrderId == orderId)
        {
            _orderIdToFsmKey[orderId] = f.EntryName;
            return f;
        }
    }

    if (f.EntryOrder != null && f.EntryOrder.OrderId == orderId)
    {
        _orderIdToFsmKey[orderId] = f.EntryName;
        return f;
    }

    return null;
}
```

#### Parent After Extraction

```csharp
private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)
{
    if (string.IsNullOrEmpty(orderId))
        return null;

    foreach (var f in _followerBrackets.Values)
    {
        if (f.AccountName != accountAlias)
            continue;

        var match = MatchOrderInFsm(f, orderId);
        if (match != null)
            return match;
    }

    return null;
}
```

#### CYC Breakdown — Parent Post-Extraction

| Branch | +CYC |
|---|---|
| Base | +1 |
| `IsNullOrEmpty(orderId)` guard | +1 |
| `foreach` loop | +1 |
| `AccountName != accountAlias` filter | +1 |
| `match != null` return check | +1 |
| **Total** | **5** |

#### CYC Breakdown — Helper `MatchOrderInFsm`

| Branch | +CYC |
|---|---|
| Base | +1 |
| `f.StopOrder != null && ... == orderId` | +1 |
| `for (int i = 0; i < 5; i++)` | +1 |
| `f.Targets[i] != null && ... == orderId` | +1 |
| `f.EntryOrder != null && ... == orderId` | +1 |
| **Total** | **5** |

#### Acceptance Criteria

- [ ] `MatchOrderInFsm` compiles with no errors in `src/V12_002.Symmetry.BracketFSM.cs`
- [ ] `ResolveFsm_ByScan` delegates inner scan to `MatchOrderInFsm`; parent signature unchanged
- [ ] `bool foundT` dead code removed from rewritten region
- [ ] `_orderIdToFsmKey` backfill preserved in `MatchOrderInFsm` on every match branch
- [ ] xUnit test: `MatchOrderInFsm` returns correct FSM on StopOrder match
- [ ] xUnit test: `MatchOrderInFsm` returns correct FSM on Targets[i] match
- [ ] xUnit test: `MatchOrderInFsm` returns correct FSM on EntryOrder match
- [ ] xUnit test: `MatchOrderInFsm` returns null when no slots match
- [ ] xUnit test: `ResolveFsm_ByScan` returns null when `orderId` is null/empty
- [ ] Build passes (`dotnet build` zero errors)
- [ ] CSharpier check passes (`dotnet csharpier check src/`)
- [ ] `grep -r "lock(" src/V12_002.Symmetry.BracketFSM.cs` returns zero matches

---

## projected_parent_cyc_after_all: 5

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC ≤ 8 — parent post-extraction | ✅ PASS — CYC = 5 |
| CYC ≤ 8 — helper `MatchOrderInFsm` | ✅ PASS — CYC = 5 |
| Single-responsibility per helper | ✅ PASS — helper does exactly one thing: scan FSM slots and backfill cache |
| Lock-free / Actor pattern preserved | ✅ PASS — `ConcurrentDictionary` retained; zero `lock()` blocks |
| Dead-code removal | ✅ PASS — `bool foundT` and `if (foundT) break` provably unreachable and removed |
| No scope creep (V12.23) | ✅ PASS — single-file, single-method extraction; callers unaffected |
| xUnit tests only (V12.32) | ✅ PASS — acceptance criteria use `[Fact]` / `Assert.*`; no NUnit/MSTest |
| ASCII-only identifiers | ✅ PASS — all new identifiers use ASCII characters only |
| Zero-allocation hot path | ✅ PASS — no heap allocations; helper passes and returns existing references |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity (index miss — symbol ambiguous), get_extraction_candidates (no cross-file callers — expected) |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-validation thoughts) |
| **Wave** | 7 |
| **Epic** | EPIC-W7-064 |
| **Phase** | 4 — Ticket Generation |
| **ticket_count** | 1 |
| **projected_parent_cyc_after_all** | 5 |
