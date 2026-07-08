# Phase 2: Architecture Plan -- EPIC-W7-116

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-116/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `AuditFleet_CalculateExpectedActual`
- **Source File:** `src/V12_002.REAPER.Audit.cs`
- **Lines:** 382-451
- **Class:** `V12_002` (partial -- REAPER Audit Module)
- **Original CYC:** 13

### jcodemunch get_context_bundle result

Symbol `AuditFleet_CalculateExpectedActual` resolved via `search_symbols` fallback (get_context_bundle returned symbol-not-found on first attempt). Full source obtained via `get_symbol_source`. The method signature is:

```csharp
private void AuditFleet_CalculateExpectedActual(
    Account acct,
    bool shouldLog,
    out int actualQty,
    out int expectedQty,
    out string expectedKey,
    out bool syncPending,
    out bool inFillGrace,
    out bool hasState,
    out List<FollowerBracketFSM> accountFsms,
    out Position pos
)
```

Key findings: 10 `out` parameters, 4 logical sections, FSM mutation side-effect via `TryTerminateFollowerBracket`, called exclusively from `AuditSingleFleetAccount`.

### jcodemunch get_call_hierarchy result

- **Direct callers (depth 1):** `AuditSingleFleetAccount` (src/V12_002.REAPER.Audit.cs, line 121)
- **Indirect callers (depth 2):** `AuditApexPositions` (src/V12_002.REAPER.Audit.cs, line 16)
- **Direct callees (depth 1):** `GetFsmExpectedPosition`, `TryTerminateFollowerBracket`, `ExpKey`, `IsReaperFillGraceActive`, `_positionPassFailedFirstSeen.TryRemove`, `_dispatchSyncPendingExpKeys.ContainsKey`
- **Resolution:** ast_resolved for callers, ast_inferred for callees

### jcodemunch get_dependency_graph result

- **Direction:** both (imports + importers)
- **Node count:** 1 (leaf node)
- **Edge count:** 0
- **Finding:** `src/V12_002.REAPER.Audit.cs` has no tracked cross-file import edges in index. All callees resolve via partial-class member resolution within the same `V12_002` partial class spread across multiple files. No cross-file dependency blast radius.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0 (empty -- index lacks per-symbol complexity metadata; requires re-indexing with jcodemunch-mcp >= 1.16)
- **Fallback:** Complexity analysis performed via source inspection and hotspot data from `00-hotspots.md`
- **Confirmed target:** `AuditFleet_CalculateExpectedActual` CYC=13 per manual branch count in Phase 0

---

## Sequential Thinking Summary

**5-thought chain executed via `sequentialthinking` MCP tool.**

**Final Thought (5/5):** Extraction plan for `AuditFleet_CalculateExpectedActual` is sound and complete. Three private helpers extracted -- `GetSignedActualQty` (CYC=2), `RepairHydratedActiveFsms` (CYC=5), `LogAuditStateIfNeeded` (CYC=3). Parent residual CYC=3. Max CYC across all resulting methods = 5 (`RepairHydratedActiveFsms`). All under the Jane Street threshold of 8. `extraction_count=3`, `max_cyc_projected=5`. The approach preserves the 10 `out`-parameter contract of the parent method, maintains all 1 direct call site (`AuditSingleFleetAccount`), and does not modify any other methods per V12.23 No Scope Creep Protocol. Jane Street alignment: FULL PASS on CYC<=8, single-responsibility, lock-free, ASCII-only, private scope, xUnit testable.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC | Signature |
|---|---|---|---|
| `GetSignedActualQty` | Pure function: reads broker `Position`, returns signed int quantity. Returns 0 if position is null or flat. | 2 | `private int GetSignedActualQty(Position pos)` |
| `RepairHydratedActiveFsms` | Iterates `accountFsms`, repairs hydrated-active FSMs with no entry order. Calls `TryTerminateFollowerBracket` as FSM side-effect for stale FSMs when broker is flat. | 5 | `private void RepairHydratedActiveFsms(List<FollowerBracketFSM> accountFsms, ref int fsmExpectedQty, int actualQty)` |
| `LogAuditStateIfNeeded` | Computes `hasState = (expectedQty != 0 \|\| actualQty != 0)` and conditionally prints audit log line. Returns `hasState` bool. | 3 | `private bool LogAuditStateIfNeeded(Account acct, bool shouldLog, int expectedQty, int actualQty)` |

---

## Parent Method After Extraction

**Remaining logic in `AuditFleet_CalculateExpectedActual` after extraction:**

1. Lookup broker `pos` via `FirstOrDefault` on `acct.Positions`
2. Call `GetSignedActualQty(pos)` to assign `actualQty`
3. Filter `_followerBrackets` to build `accountFsms` list
4. Call `GetFsmExpectedPosition(acct.Name)` to get `fsmExpectedQty`
5. Call `RepairHydratedActiveFsms(accountFsms, ref fsmExpectedQty, actualQty)`
6. Conditional `TryRemove` from `_positionPassFailedFirstSeen` if `fsmExpectedQty != 0`
7. Assign `expectedKey`, `expectedQty`, `syncPending`, `inFillGrace` (all simple assignments/lookups)
8. Call `LogAuditStateIfNeeded(acct, shouldLog, expectedQty, actualQty)` to get `hasState`

**Projected CYC:** 3 (base=1 + fsmExpectedQty check=1 + delegate calls add 0 each)

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved (all methods) | YES -- max=5, parent=3 |
| Single-responsibility per helper | YES -- each helper does exactly one thing |
| Lock-free/Actor pattern preserved | YES -- `TryTerminateFollowerBracket` delegates to existing FSM/Actor; no new `lock()` blocks introduced |
| Illegal states unrepresentable | YES -- `GetSignedActualQty` makes the null/flat guard explicit; out-parameters remain authoritative in parent |
| ASCII-only string literals | YES -- all Print format strings use ASCII only |
| xUnit [Fact] tests required | YES -- 3 tests: `GetSignedActualQty_ReturnsZeroWhenNull`, `RepairHydratedActiveFsms_TerminatesStaleFsm`, `LogAuditStateIfNeeded_ReturnsTrueWhenHasState` |
| Private scope, same partial class | YES -- all helpers added as `private` methods in `V12_002` partial class in same file |
| No callers modified | YES -- `AuditSingleFleetAccount` call site unchanged; method signature preserved |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Wave** | 7 |
| **Phase** | 2 -- Architecture Planning |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Method** | AuditFleet_CalculateExpectedActual |
| **Original CYC** | 13 |
| **max_cyc_projected** | 5 |
| **extraction_count** | 3 |
| **Output** | docs/brain/EPIC-W7-116/02-architecture-plan.md |
