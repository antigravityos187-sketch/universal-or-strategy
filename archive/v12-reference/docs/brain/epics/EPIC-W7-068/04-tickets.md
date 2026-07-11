# Phase 4: Ticket Definitions — EPIC-W7-068

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:**
- `docs/brain/EPIC-W7-068/02-architecture-plan.md`
- `docs/brain/EPIC-W7-068/03-audit-report.md`

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-068 |
| **Method** | `TryParseTargetMode` |
| **Source File** | `src/V12_002.UI.IPC.cs` |
| **Method Lines** | 97-128 |
| **Original CYC** | 7 (actual McCabe; index returns 0 due to partial-class analyser gap) |
| **ticket_count** | 1 |
| **projected_parent_cyc_after_all** | 7 |
| **Extraction Count** | 0 (CYC=7 already <= 8; no structural extraction required) |

---

## Sequential Thinking Validation

**3-thought chain completed.**

- **Thought 1:** CYC=7 is already compliant. Architecture plan declares `extraction_count=0`. `get_extraction_candidates` returned empty. Single in-place observability ticket required. `ticket_count = 1`.
- **Thought 2:** Ticket T1 scoped to adding a `Print` diagnostic statement in the `default:` arm of the switch (line ~126). Single-statement additive change. `cyc_reduction = 0` (Print is straight-line; no branches). No helper method extracted.
- **Thought 3:** Post-change CYC = 7 + 0 = **7 <= 8**. PASS. Caller `TryApplyConfigTarget_Type` unaffected — signature unchanged. `projected_parent_cyc_after_all = 7`.

---

## Ticket Definitions

---

### Ticket T1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **type** | observability-in-place |
| **helper_name** | *(none — no helper extracted; change is in-place in parent method)* |
| **concern** | Add `Print` diagnostic to the `default:` arm of the switch in `TryParseTargetMode` to surface unrecognized `raw` input values at runtime |
| **file** | `src/V12_002.UI.IPC.cs` |
| **method** | `TryParseTargetMode` (lines 97-128) |
| **lines_to_modify** | Line ~126: insert `Print("TryParseTargetMode: unrecognized target mode value '" + raw + "'");` before `return false;` in the `default:` arm |
| **lines_to_move** | None (additive only; no lines relocated) |
| **cyc_reduction** | 0 (Print statement adds no branch points; CYC unchanged at 7) |
| **projected_helper_cyc** | N/A (no helper method created) |
| **projected_parent_cyc_after** | 7 |
| **dna_verdict** | PASS (confirmed in Phase 3 audit — all 9 DNA checks passed, zero violations) |

#### Change Detail

**Before (default arm):**
```csharp
default:
    return false;
```

**After (default arm):**
```csharp
default:
    Print("TryParseTargetMode: unrecognized target mode value '" + raw + "'");
    return false;
```

#### Rationale

- `TryParseTargetMode` is a pure, static parser with CYC=7 — already within the Jane Street <= 8 mandate.
- The only gap identified in Phase 0 (`00-hotspots.md`) was missing observability: when an unrecognized string is passed, the method returned `false` silently with no diagnostic output.
- Adding `Print(...)` in the `default:` arm eliminates the silent-failure gap without altering any control-flow branch.
- The `Print` literal `"TryParseTargetMode: unrecognized target mode value '"` is ASCII-only (0x20–0x7E) — compliant with the V12 ASCII-Only mandate.
- The method signature `private static bool TryParseTargetMode(string raw, out TargetMode mode)` is **not changed** — all 5 call sites in `TryApplyConfigTarget_Type` (`src/V12_002.UI.IPC.Commands.Config.cs` lines 303, 311, 319, 327, 335) are unaffected.

#### Acceptance Criteria

1. `Print` statement present in `default:` arm before `return false;`.
2. `dotnet build` passes with zero errors.
3. `dotnet csharpier check src/` passes with zero formatting issues.
4. `grep -n "lock(" src/V12_002.UI.IPC.cs` returns zero matches.
5. `Print` literal is ASCII-only — no Unicode, emoji, or curly quotes.
6. Method signature unchanged — caller `TryApplyConfigTarget_Type` compiles without modification.
7. CYC remains 7 (no new `if`/`else`/`while`/`for`/`&&`/`||`/`?:` added).

---

## Projected CYC Summary

| Scope | CYC Before | CYC After | Delta | Threshold | Status |
|---|---|---|---|---|---|
| `TryParseTargetMode` (parent) | 7 | 7 | 0 | <= 8 | PASS |
| Helpers extracted | N/A | N/A | N/A | N/A | N/A |
| **projected_parent_cyc_after_all** | — | **7** | — | <= 8 | **PASS** |

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 after ticket | YES — CYC=7, unchanged |
| ASCII-only string literals in change | YES — `Print` literal is ASCII-only |
| Lock-free pattern preserved | YES — no `lock()` blocks; method is pure computation |
| Illegal states unrepresentable | YES — `out mode` always assigned before any `return`; `default:` arm now logs before `return false;` |
| No scope creep | YES — bounded to lines 97-128 of `src/V12_002.UI.IPC.cs`; caller unmodified |
| xUnit test framework | YES — no new test framework introduced; existing xUnit standard applies |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Phase** | 4 — Ticket Generation |
| **Wave** | 7 |
| **Epic** | EPIC-W7-068 |
| **jcodemunch tools called** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates` |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-breakdown thoughts) |
| **ticket_count** | 1 |
| **projected_parent_cyc_after_all** | 7 |

---

*Wave 7 | Phase 4 | EPIC-W7-068*
