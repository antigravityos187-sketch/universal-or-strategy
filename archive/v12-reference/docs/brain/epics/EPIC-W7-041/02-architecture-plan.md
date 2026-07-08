# EPIC-W7-041 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:35:02Z
**Input:** docs/brain/EPIC-W7-041/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `AuditStopQuantityAndPrint` |
| **Source File** | `src/V12_002.Orders.Management.cs` |
| **Original CYC** | 8 |
| **Extraction Count** | 2 |
| **max_cyc_projected** | 5 |
| **Parent CYC After Extraction** | 1 |
| **Boundary Verdict** | PASS (from Phase 1.5) |

---

## Original Method Analysis

```
private void AuditStopQuantityAndPrint(
    string entryName,
    PositionInfo pos,
    Order stopOrder,
    double validatedStopPrice,
    int nonRunnerLimitQty,
    int runnerQty,
    bool isFollowerSubmit
)
```

**Located at:** `src/V12_002.Orders.Management.cs` line 90
**Called by:** `SubmitBracketOrders` (1 caller — signature unchanged by this epic)

### Complexity Drivers (CYC = 8)

| # | Condition | CYC Contribution |
|---|---|---|
| 1 | Base | +1 |
| 2 | `stopOrder != null` (null guard) | +1 |
| 3 | `stopOrder.Quantity != pos.TotalContracts` (short-circuit AND) | +1 |
| 4 | `if (isFollowerSubmit)` | +1 |
| 5 | `for (int targetNum = 1; targetNum <= 5; targetNum++)` | +1 |
| 6 | `if (targetQty <= 0) continue` | +1 |
| 7 | `if (isRunnerSlot)` ... else | +1 |
| 8 | `if (_targetSum != pos.TotalContracts)` | +1 |

**Total CYC: 8**

### Logical Segments

- **Segment A** (lines ~95–112): Stop quantity audit — null/mismatch check on `stopOrder`, prints `[STOP_AUDIT] MISMATCH` or `[STOP_AUDIT] OK`
- **Segment B** (lines ~119–123): Follower bracket confirmation — conditional `Print("[938-BRACKET]...")` when `isFollowerSubmit`
- **Segment C** (lines ~125–145): Bracket message builder — `StringBuilder` loop over targets 1–5, runner vs non-runner format branching, calls `GetTargetContracts`, `IsRunnerTarget`, `GetTargetPrice`
- **Segment D** (lines ~150–157): Target sum audit — verifies `nonRunnerLimitQty + runnerQty == pos.TotalContracts`, prints `[BRACKET_WARN]` on mismatch

---

## Extraction Plan

### Design Decision

Scope boundary mandates exactly **2 new helper methods**. Segments are combined by logical affinity:
- **Audit helpers** (A + D) → `AuditStopQuantityAndLog` — all integrity checks
- **Print helpers** (B + C) → `BuildAndPrintBracketSummary` — all output formatting

---

### Helper 1: `AuditStopQuantityAndLog`

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void AuditStopQuantityAndLog(
    string entryName,
    PositionInfo pos,
    Order stopOrder,
    int nonRunnerLimitQty,
    int runnerQty
)
```

**Responsibility:** Zero-trust stop quantity audit (V12.Audit [S-003]) and target sum audit (V12.Audit [D-007]). Prints `[STOP_AUDIT] MISMATCH`, `[STOP_AUDIT] OK`, or `[BRACKET_WARN]` as appropriate.

**Contains Segments:** A (stop qty check) + D (target sum check)

**Projected CYC Breakdown:**

| # | Condition | CYC Contribution |
|---|---|---|
| 1 | Base | +1 |
| 2 | `stopOrder != null` | +1 |
| 3 | `stopOrder.Quantity != pos.TotalContracts` | +1 |
| 4 | `if (_targetSum != pos.TotalContracts)` | +1 |

**Projected CYC: 4** ✅ (≤ 8)

---

### Helper 2: `BuildAndPrintBracketSummary`

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void BuildAndPrintBracketSummary(
    string entryName,
    PositionInfo pos,
    double validatedStopPrice,
    bool isFollowerSubmit
)
```

**Responsibility:** Prints follower bracket confirmation if applicable, then builds and prints the full bracket summary (`BRACKET V12.1101E`) with per-target quantities and prices using `StringBuilder`.

**Contains Segments:** B (follower print) + C (bracket builder loop)

**Projected CYC Breakdown:**

| # | Condition | CYC Contribution |
|---|---|---|
| 1 | Base | +1 |
| 2 | `if (isFollowerSubmit)` | +1 |
| 3 | `for (int targetNum = 1; targetNum <= 5; targetNum++)` | +1 |
| 4 | `if (targetQty <= 0) continue` | +1 |
| 5 | `if (isRunnerSlot)` | +1 |

**Projected CYC: 5** ✅ (≤ 8)

---

### Parent After Extraction: `AuditStopQuantityAndPrint`

```csharp
private void AuditStopQuantityAndPrint(
    string entryName,
    PositionInfo pos,
    Order stopOrder,
    double validatedStopPrice,
    int nonRunnerLimitQty,
    int runnerQty,
    bool isFollowerSubmit
)
{
    pos.CurrentStopPrice = validatedStopPrice;
    AuditStopQuantityAndLog(entryName, pos, stopOrder, nonRunnerLimitQty, runnerQty);
    BuildAndPrintBracketSummary(entryName, pos, validatedStopPrice, isFollowerSubmit);
}
```

**Contains:** Single assignment + 2 method calls. No branches.

**Projected CYC: 1** ✅ (≤ 8)

---

## CYC Summary Table

| Method | Role | Projected CYC | Status |
|---|---|---|---|
| `AuditStopQuantityAndPrint` (parent) | Orchestrator | 1 | ✅ PASS |
| `AuditStopQuantityAndLog` | Audit helper | 4 | ✅ PASS |
| `BuildAndPrintBracketSummary` | Print helper | 5 | ✅ PASS |

**max_cyc_projected: 5** ✅ (≤ 8 threshold)

---

## Jane Street Alignment

### gjengset — Cache Line / False Sharing

- Both helpers receive `pos` and `stopOrder` parameters by reference (existing pattern in codebase)
- No shared mutable state crosses helper boundaries
- The single state mutation (`pos.CurrentStopPrice = validatedStopPrice`) stays in the parent method, executed once before helper delegation
- No `volatile` or `MemoryBarrier` required — helpers are read-only on all inputs

### carl_cook — Hot Path Zero-Alloc / NoInlining Cold Paths

- **Both helpers tagged `[MethodImpl(MethodImplOptions.NoInlining)]`**: These are cold audit/log paths containing `string.Format` and `Print()` calls — they must not pollute the instruction cache of the `SubmitBracketOrders` hot path
- **Parent `AuditStopQuantityAndPrint`**: After extraction, the parent is a thin 3-line coordinator; its cold nature (it ultimately calls Print) means it too benefits from not being inlined
- **`StringBuilder` allocation** in `BuildAndPrintBracketSummary`: Acceptable cold-path allocation — logging by definition is not zero-alloc
- **Parent method**: No new heap allocations after extraction (assignment + 2 calls)

### trading_billions — Single Responsibility / Defense in Depth

- `AuditStopQuantityAndLog`: **Single responsibility** = "verify stop order quantity and target distribution integrity, log any violations"
- `BuildAndPrintBracketSummary`: **Single responsibility** = "format and emit the complete bracket submission confirmation"
- **Defense in depth**: Audit ordering preserved — `AuditStopQuantityAndLog` is called BEFORE `BuildAndPrintBracketSummary`, maintaining the V12.Audit [S-003] and [D-007] semantics from the original code comments
- Each helper is independently unit-testable with xUnit

---

## MCP Evidence

### jcodemunch: resolve_repo
- **Repo:** `antigravityos187-sketch/universal-or-strategy`
- **Indexed:** true | **Symbol count:** 5120 | **Backend:** sqlite

### jcodemunch: get_context_bundle
- Symbol `src/V12_002.Orders.Management.cs::V12_002.AuditStopQuantityAndPrint#method` retrieved successfully
- Method at line 90, 7 parameters, calls: `GetTargetContracts`, `IsRunnerTarget`, `GetTargetPrice`, `Print`, `StringBuilder`
- Full method body confirmed (see Original Method Analysis above)

### jcodemunch: get_call_hierarchy
- **Callers (depth 1):** `SubmitBracketOrders` (ast_resolved) — 1 caller confirmed
- **Callees (depth 1):** `Format`, `GetTargetContracts`, `IsRunnerTarget`, `GetTargetPrice` (ast_inferred)
- **Callee depth 2:** `ValidateThreadAffinity`, `FormatInternal`, `GetTargetMode` (logging infrastructure)
- Caller `SubmitBracketOrders` signature unchanged by this epic

### jcodemunch: get_dependency_graph
- `src/V12_002.Orders.Management.cs`: 0 import edges, 0 importer edges at file level
- Self-contained partial class — no cross-file import rewriting needed

### jcodemunch: get_extraction_candidates
- No auto-detected candidates (requires min_callers=2 for typical extraction heuristic)
- Manual extraction design applied via Sequential Thinking analysis

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Driver Identification
- Enumerated all 8 CYC-contributing branches: null guard, quantity mismatch, follower submit, for-loop, targetQty skip, isRunnerSlot, targetSum check
- Identified 4 logical segments (A: stop audit, B: follower print, C: bracket builder, D: target sum audit)
- Established parent can become CYC 1 after 2-helper extraction

### Thought 2 — Extraction Design
- Evaluated 2-helper combination: (A+D) → `AuditStopQuantityAndLog` (CYC 4) + (B+C) → `BuildAndPrintBracketSummary` (CYC 5)
- Verified all projected CYCs ≤ 8; max_cyc_projected = 5
- Confirmed scope boundary compliance (exactly 2 helpers, same-file private methods)
- Defined complete method signatures with parameter lists

### Thought 3 — Jane Street Alignment
- gjengset: No shared mutable state across helpers; single state mutation in parent
- carl_cook: Both helpers get `[MethodImpl(MethodImplOptions.NoInlining)]`; cold log path extraction complete
- trading_billions: Single responsibility per helper; audit ordering preserved (V12.Audit [S-003] / [D-007])

### Thought 4 — Verification
- Confirmed all 3 methods (parent + 2 helpers) have projected CYC ≤ 8
- max_cyc_projected = 5; all constraints satisfied
- Design ready for Phase 3 (DNA audit) and Phase 4 (ticket generation)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-041 |
| **MCP Tools Used** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, sequentialthinking (4 thoughts) |
| **max_cyc_projected** | 5 |
| **extraction_count** | 2 |
