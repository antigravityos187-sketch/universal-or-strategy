# EPIC-W7-041 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-041/02-architecture-plan.md, docs/brain/EPIC-W7-041/03-audit-report.md

---

## Header

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-041 |
| **Method** | `AuditStopQuantityAndPrint` |
| **Source File** | `src/V12_002.Orders.Management.cs` |
| **Lines** | 90–174 |
| **Original CYC** | 8 |
| **jCodemunch CYC** | 13 (raw AST; audit baseline = 8) |
| **Wave** | 7 |
| **Extraction Count** | 2 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 1 |

---

## Ticket Summary

| Ticket | Helper | Concern | CYC Removed | Helper CYC |
|---|---|---|---|---|
| T-1 | `AuditStopQuantityAndLog` | integrity-audit | −3 | 4 |
| T-2 | `BuildAndPrintBracketSummary` | print-format | −4 | 5 |

**max_cyc_projected:** 5  
**projected_parent_cyc_after_all:** 1 ≤ 8 ✅

---

## Ticket T-1: Extract `AuditStopQuantityAndLog`

### Metadata

| Field | Value |
|---|---|
| **ticket_id** | T-1 |
| **epic_id** | EPIC-W7-041 |
| **helper_name** | `AuditStopQuantityAndLog` |
| **concern** | integrity-audit |
| **source_file** | `src/V12_002.Orders.Management.cs` |
| **parent_method** | `AuditStopQuantityAndPrint` |
| **lines_to_move** | ~95–112 (Segment A) + ~150–157 (Segment D) ≈ 26 lines |
| **cyc_reduction** | −3 (null guard, mismatch check, target sum check) |
| **projected_helper_cyc** | 4 |
| **decorator** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **execution_order** | 1 |

### Signature

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

### Segments Moved

| Segment | Original Lines | Content |
|---|---|---|
| A | ~95–112 | `stopOrder != null` null guard + `stopOrder.Quantity != pos.TotalContracts` mismatch check + `Print("[STOP_AUDIT] MISMATCH ...")` / `Print("[STOP_AUDIT] OK ...")` |
| D | ~150–157 | `if (_targetSum != pos.TotalContracts)` check + `Print("[BRACKET_WARN] ...")` |

### CYC Breakdown for `AuditStopQuantityAndLog`

| # | Condition | CYC Contribution |
|---|---|---|
| 1 | Base | +1 |
| 2 | `stopOrder != null` (null guard) | +1 |
| 3 | `stopOrder.Quantity != pos.TotalContracts` (mismatch) | +1 |
| 4 | `if (_targetSum != pos.TotalContracts)` | +1 |

**Projected CYC: 4** ✅ (≤ 8)

### CYC Removed from Parent

Removing conditions 2, 3, 4 above from parent: **−3**

### Acceptance Criteria

- [ ] `AuditStopQuantityAndLog` exists as a private method in `src/V12_002.Orders.Management.cs`
- [ ] Method is decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Segments A and D logic is fully contained in helper; removed from parent
- [ ] All `Print()` literals remain ASCII-only
- [ ] No `lock()` blocks introduced
- [ ] Parent `AuditStopQuantityAndPrint` calls `AuditStopQuantityAndLog(entryName, pos, stopOrder, nonRunnerLimitQty, runnerQty)` at original Segment A position
- [ ] Build passes: `dotnet build`
- [ ] xUnit test covers: null stopOrder path, mismatch path, OK path, sum mismatch path

---

## Ticket T-2: Extract `BuildAndPrintBracketSummary`

### Metadata

| Field | Value |
|---|---|
| **ticket_id** | T-2 |
| **epic_id** | EPIC-W7-041 |
| **helper_name** | `BuildAndPrintBracketSummary` |
| **concern** | print-format |
| **source_file** | `src/V12_002.Orders.Management.cs` |
| **parent_method** | `AuditStopQuantityAndPrint` |
| **lines_to_move** | ~119–145 (Segments B+C) ≈ 27 lines |
| **cyc_reduction** | −4 (isFollowerSubmit, for-loop, targetQty continue, isRunnerSlot) |
| **projected_helper_cyc** | 5 |
| **decorator** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **execution_order** | 2 |

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void BuildAndPrintBracketSummary(
    string entryName,
    PositionInfo pos,
    double validatedStopPrice,
    bool isFollowerSubmit
)
```

### Segments Moved

| Segment | Original Lines | Content |
|---|---|---|
| B | ~119–123 | `if (isFollowerSubmit)` + `Print("[938-BRACKET] ...")` |
| C | ~125–145 | `StringBuilder` construction + `for (int targetNum = 1; targetNum <= 5; targetNum++)` loop + `if (targetQty <= 0) continue` + `if (isRunnerSlot) ... else ...` + calls to `GetTargetContracts`, `IsRunnerTarget`, `GetTargetPrice` + `Print("BRACKET V12.1101E ...")` |

### CYC Breakdown for `BuildAndPrintBracketSummary`

| # | Condition | CYC Contribution |
|---|---|---|
| 1 | Base | +1 |
| 2 | `if (isFollowerSubmit)` | +1 |
| 3 | `for (int targetNum = 1; targetNum <= 5; targetNum++)` | +1 |
| 4 | `if (targetQty <= 0) continue` | +1 |
| 5 | `if (isRunnerSlot)` | +1 |

**Projected CYC: 5** ✅ (≤ 8)

### CYC Removed from Parent

Removing conditions 2, 3, 4, 5 above from parent: **−4**

### Acceptance Criteria

- [ ] `BuildAndPrintBracketSummary` exists as a private method in `src/V12_002.Orders.Management.cs`
- [ ] Method is decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Segments B and C logic is fully contained in helper; removed from parent
- [ ] All `Print()` literals remain ASCII-only
- [ ] No `lock()` blocks introduced
- [ ] Parent `AuditStopQuantityAndPrint` calls `BuildAndPrintBracketSummary(entryName, pos, validatedStopPrice, isFollowerSubmit)` after `AuditStopQuantityAndLog` call
- [ ] Build passes: `dotnet build`
- [ ] xUnit test covers: isFollowerSubmit=true path, loop with runner slot, loop with non-runner slot, targetQty=0 skip

---

## Parent Method After All Extractions

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

**Parent projected CYC: 1** ✅ (assignment + 2 calls, 0 branches)

---

## CYC Summary

| Method | Role | Projected CYC | Threshold | Status |
|---|---|---|---|---|
| `AuditStopQuantityAndPrint` (parent) | Orchestrator | 1 | ≤ 8 | ✅ PASS |
| `AuditStopQuantityAndLog` | Audit helper (T-1) | 4 | ≤ 8 | ✅ PASS |
| `BuildAndPrintBracketSummary` | Print helper (T-2) | 5 | ≤ 8 | ✅ PASS |

**max_cyc_projected:** 5  
**projected_parent_cyc_after_all:** 1

---

## Sequential Thinking Evidence

| Thought | Summary |
|---|---|
| 1 | Mapped CYC drivers to 2 extraction groups; confirmed parent reduces to CYC=1 after both tickets |
| 2 | Detailed T-1: AuditStopQuantityAndLog — Segments A+D, ~26 lines, CYC-3, helper CYC=4 |
| 3 | Detailed T-2: BuildAndPrintBracketSummary — Segments B+C, ~27 lines, CYC-4, helper CYC=5 |
| 4 | Final verification: all CYC constraints satisfied, max=5, parent=1, ticket_count=2 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-041 |
| **Phase** | 4 — Ticket Generation |
| **Lane** | P4-L3 |
| **Method** | `AuditStopQuantityAndPrint` |
| **Source File** | `src/V12_002.Orders.Management.cs` |
| **Original CYC** | 8 |
| **jCodemunch CYC** | 13 |
| **ticket_count** | 2 |
| **max_cyc_projected** | 5 |
| **projected_parent_cyc_after_all** | 1 |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking (4 thoughts) |
| **Output** | `docs/brain/EPIC-W7-041/04-tickets.md` |
