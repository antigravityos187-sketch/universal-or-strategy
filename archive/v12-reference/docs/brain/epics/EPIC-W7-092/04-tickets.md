# EPIC-W7-092 — Phase 4: Implementation Tickets

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T04:00:00Z
**Input:** docs/brain/EPIC-W7-092/02-architecture-plan.md + docs/brain/EPIC-W7-092/03-audit-report.md

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-092 |

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-092 |
| **Target Method** | `SetRmaAnchorFromIpc` in `src/V12_002.SIMA.cs` |
| **CYC Baseline** | 13 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 4 |
| **CYC Reduction** | 69% (13 → 4) |
| **DNA Verdict** | PASS |
| **extraction Strategy** | Static readonly Dictionary + TryParseRmaAnchorType helper |

---

## Ticket List

### Ticket T1 — Add RmaAnchorLookup Field and TryParseRmaAnchorType Helper

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `TryParseRmaAnchorType` |
| **concern** | Add `private static readonly Dictionary<string, RmaAnchorType> RmaAnchorLookup` field and `private static bool TryParseRmaAnchorType(string key, out RmaAnchorType result)` expression-bodied helper |
| **file** | `src/V12_002.SIMA.cs` |
| **change_type** | Additive (no existing code modified) |
| **lines_to_move** | 0 (new code inserted — dictionary initializer ~8 lines + helper declaration ~2 lines) |
| **cyc_reduction** | 0 (parent method `SetRmaAnchorFromIpc` is not touched in this ticket) |
| **projected_helper_cyc** | 1 |
| **depends_on** | None (first ticket) |
| **blocked_by** | None |
| **dna_verdict** | PASS |

#### T1 Implementation Specification

Add the following two members to `src/V12_002.SIMA.cs` in the same partial class as `SetRmaAnchorFromIpc`:

```csharp
// Static readonly lookup table — allocated once at class init, zero-alloc at call time (carl_cook)
private static readonly Dictionary<string, RmaAnchorType> RmaAnchorLookup =
    new Dictionary<string, RmaAnchorType>
    {
        { "EMA30",   RmaAnchorType.Ema30   },
        { "EMA65",   RmaAnchorType.Ema65   },
        { "EMA200",  RmaAnchorType.Ema200  },
        { "OR_HIGH", RmaAnchorType.OrHigh  },
        { "OR_LOW",  RmaAnchorType.OrLow   },
        { "MANUAL",  RmaAnchorType.Manual  },
    };

// CYC=1: single expression, no branches (trading_billions: single responsibility)
private static bool TryParseRmaAnchorType(string key, out RmaAnchorType result)
    => RmaAnchorLookup.TryGetValue(key, out result);
```

#### T1 Acceptance Criteria

- [ ] `RmaAnchorLookup` field compiles without errors
- [ ] All 6 `RmaAnchorType` enum members mapped: `Ema30`, `Ema65`, `Ema200`, `OrHigh`, `OrLow`, `Manual`
- [ ] `TryParseRmaAnchorType` compiles as expression-bodied method (no block body)
- [ ] `TryParseRmaAnchorType` CYC = 1 (single expression, zero branches)
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] `SetRmaAnchorFromIpc` body is **unchanged** in this ticket

---

### Ticket T2 — Refactor SetRmaAnchorFromIpc Body (extraction)

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `SetRmaAnchorFromIpc` (refactored orchestrator) |
| **concern** | Replace the 6-branch if/else-if string dispatch chain in `SetRmaAnchorFromIpc` with a single call to `TryParseRmaAnchorType`; this is the core extraction step |
| **file** | `src/V12_002.SIMA.cs` |
| **change_type** | Surgical extraction — replaces lines 241–264 body |
| **lines_to_move** | ~18 (6-branch if/else-if block removed from parent; logic now lives in `TryParseRmaAnchorType` + `RmaAnchorLookup` from T1) |
| **cyc_reduction** | 9 (13 → 4: 6 branch conditions removed + path estimate reduced) |
| **projected_helper_cyc** | N/A (this ticket refactors the parent, not a new helper) |
| **projected_parent_cyc** | 4 (base=1 + if=1 + try=1 + catch=1) |
| **depends_on** | T1 (requires `TryParseRmaAnchorType` and `RmaAnchorLookup` to exist) |
| **blocked_by** | T1 must be completed first |
| **dna_verdict** | PASS |

#### T2 Implementation Specification

Replace the body of `SetRmaAnchorFromIpc` (lines 241–264) with the refactored orchestrator:

```csharp
// CYC=4: base(1) + if(1) + try(1) + catch(1)
private void SetRmaAnchorFromIpc(string anchorStr)
{
    try
    {
        if (TryParseRmaAnchorType(anchorStr, out RmaAnchorType anchor))
            currentRmaAnchor = anchor;

        Print("IPC SET ANCHOR: " + anchorStr);
    }
    catch (Exception ex)
    {
        Print("Error SetRmaAnchorFromIpc: " + ex.Message);
    }
}
```

The 6-branch if/else-if chain is deleted; all dispatch logic is handled via `RmaAnchorLookup.TryGetValue` inside `TryParseRmaAnchorType`. Unrecognized keys silently no-op (defense in depth — `TryGetValue` returns false, assignment is skipped).

#### T2 Acceptance Criteria

- [ ] 6-branch if/else-if chain fully removed from `SetRmaAnchorFromIpc`
- [ ] `SetRmaAnchorFromIpc` signature unchanged: `private void SetRmaAnchorFromIpc(string anchorStr)`
- [ ] Refactored body calls `TryParseRmaAnchorType(anchorStr, out RmaAnchorType anchor)`
- [ ] `try/catch` shell retained; `Print(...)` calls retained
- [ ] Projected CYC = 4 (base + if + try + catch)
- [ ] Build passes with zero errors
- [ ] xUnit test added: `[Fact]` + `Assert.Equal()` — **NEVER NUnit or MSTest**
- [ ] Caller `TryHandleRisk_SetAnchor` in `src/V12_002.UI.IPC.Commands.Mode.cs` is NOT modified
- [ ] No cross-file changes outside `src/V12_002.SIMA.cs`

---

## CYC Projection Table

| Method | CYC Baseline | CYC Projected | Threshold | Pass? |
|---|---|---|---|---|
| `SetRmaAnchorFromIpc` (original) | 13 | — | <= 8 | FAIL (baseline) |
| `RmaAnchorLookup` (static field) | N/A | N/A | N/A | N/A |
| `TryParseRmaAnchorType` (new, T1) | — | 1 | <= 8 | **PASS** |
| `SetRmaAnchorFromIpc` (after T2) | 13 | **4** | <= 8 | **PASS** |

**projected_parent_cyc_after_all = 4**
**max_cyc_projected = 4**
**cyc reduction: 69% (13 → 4)**

---

## Execution Order

```
T1 (Additive) → T2 (extraction / Surgery)
```

T1 must complete before T2 begins. T1 is purely additive and carries zero risk to existing behavior. T2 is the surgical extraction step that removes the 6-branch dispatch chain.

---

## Jane Street Compliance

| Principle | Rule | Status |
|---|---|---|
| `carl_cook` | Zero-alloc hot path | **PASS** — `static readonly Dictionary` allocated once at class init; `TryGetValue` alloc-free at call time |
| `carl_cook` | Avoid LINQ | **PASS** — Dictionary lookup only, no LINQ |
| `gjengset` | No new `lock()` blocks | **PASS** — read-only dictionary is concurrent-read safe, no locks needed |
| `trading_billions` | Single responsibility per helper | **PASS** — `TryParseRmaAnchorType` does exactly one thing |
| `trading_billions` | Each helper CYC <= 8 | **PASS** — TryParseRmaAnchorType=1, SetRmaAnchorFromIpc=4 |
| `trading_billions` | Defense in depth | **PASS** — unrecognized keys silently no-op via `TryGetValue` bool return |

---

## Boundary Constraints

| Constraint | Status |
|---|---|
| V12.23 No Scope Creep | PASS — single file, single method refactored, one helper extracted |
| Caller signature unchanged | PASS — `SetRmaAnchorFromIpc(string anchorStr)` identical |
| Cross-file changes | NONE — all changes in `src/V12_002.SIMA.cs` |
| Public API changes | NONE — all members are `private` |
| Phase 3 DNA verdict | PASS — violations: [] |
