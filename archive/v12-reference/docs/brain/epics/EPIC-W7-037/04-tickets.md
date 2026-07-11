# Phase 4: Implementation Tickets — EPIC-W7-037

**Epic:** EPIC-W7-037 | **Method:** `SymmetryNormalizeTradeType` | **Source:** `src/V12_002.Symmetry.Replace.cs` | **Original CYC:** 10 (jCodemunch confirmed; 9 project-canonical) | **Wave:** 7

---

## ticket_count: 2

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** `IsOrTradeType`
- **concern:** Encapsulate the three-predicate OR trade-type classification (single boolean responsibility)
- **lines_to_move:** The compound boolean expression `t.StartsWith("OR", StringComparison.Ordinal) || t.Contains("ORLONG") || t.Contains("ORSHORT")` currently inline in `SymmetryNormalizeTradeType` (source lines ~338–340). Extract as a new `private static bool IsOrTradeType(string t)` method in the same partial class file `src/V12_002.Symmetry.Replace.cs`.
- **implementation:**
  ```csharp
  private static bool IsOrTradeType(string t)
  {
      return t.StartsWith("OR", StringComparison.Ordinal)
          || t.Contains("ORLONG")
          || t.Contains("ORSHORT");
  }
  ```
- **cyc_reduction:** Removes 2 branch-points from the parent (the compound OR with 3 predicates contributes 2 extra branches to the parent's count)
- **projected_helper_cyc:** 3 (base=1 + 2 OR-predicate branches)
- **dependency:** None — this ticket must be completed first; Ticket 2 calls `IsOrTradeType`
- **file_scope:** `src/V12_002.Symmetry.Replace.cs` only — no cross-file changes
- **caller_changes_required:** None — parent signature unchanged

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** `NormalizeTradeTypeKernel`
- **concern:** Encapsulate the sequential prefix-match classification chain (TREND / RETEST / FFMA / MOMO / RMA / OR / GENERIC) — separates null-guard+uppercasing concern (parent) from classification concern (kernel)
- **lines_to_move:** The 6 `if-return` prefix-match chains plus the `IsOrTradeType` call and `"GENERIC"` fallback from `SymmetryNormalizeTradeType` body (approximately lines 323–341 post-uppercasing). Extract as a new `private static string NormalizeTradeTypeKernel(string t)` method in the same partial class file. Parent is reduced to null-guard + `ToUpperInvariant` + tail call to `NormalizeTradeTypeKernel`.
- **implementation:**
  ```csharp
  private static string NormalizeTradeTypeKernel(string t)
  {
      if (t.StartsWith("TREND",  StringComparison.Ordinal)) return "TREND";
      if (t.StartsWith("RETEST", StringComparison.Ordinal)) return "RETEST";
      if (t.StartsWith("FFMA",   StringComparison.Ordinal)) return "FFMA";
      if (t.StartsWith("MOMO",   StringComparison.Ordinal)) return "MOMO";
      if (t.StartsWith("RMA",    StringComparison.Ordinal)) return "RMA";
      if (IsOrTradeType(t))                                  return "OR";
      return "GENERIC";
  }
  ```
- **resulting_parent_body:**
  ```csharp
  private string SymmetryNormalizeTradeType(string raw)
  {
      if (string.IsNullOrEmpty(raw))
          return "GENERIC";

      string t = raw.ToUpperInvariant();
      return NormalizeTradeTypeKernel(t);
  }
  ```
- **cyc_reduction:** Removes 7 branch-points from parent (6 `StartsWith` checks + 1 `IsOrTradeType` branch); parent retains only 1 null-guard branch
- **projected_helper_cyc:** 7 (base=1 + 6 if-branch checks)
- **dependency:** Ticket 1 must be completed first (`IsOrTradeType` must exist before `NormalizeTradeTypeKernel` is written)
- **file_scope:** `src/V12_002.Symmetry.Replace.cs` only — no cross-file changes
- **caller_changes_required:** None — parent signature `private string SymmetryNormalizeTradeType(string raw)` unchanged; callers at `src/V12_002.Symmetry.cs:146` and `src/V12_002.Symmetry.cs:332` not modified

---

## projected_parent_cyc_after_all: 2

---

## CYC Summary Table

| Method | Projected CYC | ≤ 8? |
|---|---|---|
| `IsOrTradeType` | 3 | ✅ |
| `NormalizeTradeTypeKernel` | 7 | ✅ |
| `SymmetryNormalizeTradeType` (parent, post-extraction) | 2 | ✅ |
| **max_cyc_projected** | **7** | ✅ |

---

## Execution Order

1. **Ticket 1 first** — add `IsOrTradeType` as `private static bool`
2. **Ticket 2 second** — add `NormalizeTradeTypeKernel` as `private static string` (calls `IsOrTradeType`), then refactor parent body

**Invariant to preserve:** Prefix-priority ordering `TREND > RETEST > FFMA > MOMO > RMA > IsOrTradeType > GENERIC` must be maintained exactly in `NormalizeTradeTypeKernel`.

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC ≤ 8 achieved | ✅ YES — max_cyc_projected = 7 |
| Single-responsibility per helper | ✅ YES — `IsOrTradeType` = OR predicate only; `NormalizeTradeTypeKernel` = classification chain only; parent = null-guard + delegation |
| Lock-free / Actor pattern | ✅ YES — pure functional; no state mutations; no lock blocks |
| Illegal states unrepresentable | ✅ YES — returns one of `{"GENERIC","TREND","RETEST","FFMA","MOMO","RMA","OR"}` |
| Zero-allocation hot path | ✅ YES — all helpers `private static`; no LINQ, closures, or new heap allocations |
| V12.23 No Scope Creep | ✅ YES — single file only; no caller modifications |

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase4-tickets |
| Epic | EPIC-W7-037 |
| Wave | 7 |
| Phase | 4 — Ticket Generation |
| Bobcoins Used | 6 |
| Execution Time | 2026-06-29T01:20:00Z |
| jcodemunch tools called | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_extraction_candidates` |
| sequential-thinking calls | 4 (1 probe + 3 ticket-breakdown thoughts) |
| ticket_count | 2 |
| projected_parent_cyc_after_all | 2 |
| Original CYC | 10 (jCodemunch confirmed; 9 project-canonical; 0 task-prompt default) |
