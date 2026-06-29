# EPIC-W7-084 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:05:00Z
**Input:** docs/brain/EPIC-W7-084/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `AuditFleet_CalculateExpectedActual` |
| **File** | `src/V12_002.REAPER.Audit.cs` |
| **CYC Baseline** | 382 (confirmed in 00-scope.md; precomputed.json shows stale 0) |
| **CYC Target** | <= 8 |
| **Extraction Count** | 5 helpers |
| **max_cyc_projected** | 6 (parent after extraction) |
| **Helper Max CYC** | 4 (ReconcileStaleFsms) |

---

## jCodemunch Evidence

### get_context_bundle

Symbol `src/V12_002.REAPER.Audit.cs::V12_002.AuditFleet_CalculateExpectedActual#method`
confirmed at line 382, end_line 451. Signature:

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

Imports: `System`, `System.Collections.Concurrent`, `System.Collections.Generic`, `System.Linq`,
`System.Threading`, `NinjaTrader.Cbi`, `NinjaTrader.NinjaScript.Strategies`.

### get_dependency_graph

`src/V12_002.REAPER.Audit.cs`: 1 node, 0 cross-file import edges. The file is self-contained
with no external file dependencies. All helpers will remain in the same file (V12.23 compliant).

### get_call_hierarchy

- **Callers:** `AuditSingleFleetAccount` (depth=1, same file), `AuditApexPositions` (depth=2, same file)
- **Callees (depth=1):** `GetFsmExpectedPosition`, `TryTerminateFollowerBracket`, `ExpKey`,
  `IsReaperFillGraceActive`, `_positionPassFailedFirstSeen.TryRemove`, `_dispatchSyncPendingExpKeys.ContainsKey`
- **Callee (depth=2):** `RemoveFsmOrderIdMappings`, `LogBuffer.Format`, `LogBuffer.ValidateThreadAffinity`,
  `_accountFillGraceTicks`

The method signature is preserved unchanged. `AuditSingleFleetAccount` (direct caller, 1 call site)
is not modified.

---

## Sequential Thinking Decisions (5 thoughts)

**Thought 1 (probe):** Identified target method location, confirmed CYC=382 vs stale precomputed=0.
Authoritative source is 00-scope.md.

**Thought 2 (decomposition):** Identified 5 cohesive logical sections in the method body:
position resolution, FSM collection, stale FSM reconciliation, position-pass state cleanup,
and output assembly + logging.

**Thought 3 (Jane Street alignment):** Applied carl_cook zero-alloc + cold-logging extraction,
trading_billions single-responsibility per helper. ReconcileStaleFsms uses `ref int fsmExpectedQty`
to preserve mutation semantics without allocating an extra wrapper.

**Thought 4 (CYC projection):** Parent CYC after extraction = ~6. Each helper CYC <= 8.
Max helper CYC = 4 (ReconcileStaleFsms: foreach + 2x if = 4 paths).

**Thought 5 (verification):** Architecture confirmed. 5 extractions, max_cyc_projected=6,
all helpers private same-file, V12.23 compliant.

---

## Extraction Plan

### Helper 1: `AuditFleet_ResolvePosition`

**Purpose:** Resolve actual broker position quantity from account.

```csharp
private void AuditFleet_ResolvePosition(
    Account acct,
    out int actualQty,
    out Position pos
)
```

**Extracted logic:**
- `pos = acct.Positions.FirstOrDefault(...)` 
- `actualQty = 0;`
- `if (pos != null && pos.MarketPosition != MarketPosition.Flat)` => `actualQty = Long ? qty : -qty`

**Projected CYC:** 3

---

### Helper 2: `AuditFleet_CollectFsmState`

**Purpose:** Collect FSM list and expected quantity from FSM authority.

```csharp
private void AuditFleet_CollectFsmState(
    Account acct,
    out List<FollowerBracketFSM> accountFsms,
    out int fsmExpectedQty
)
```

**Extracted logic:**
- `accountFsms = _followerBrackets.Values.Where(f => f.AccountName == acct.Name).ToList()`
- `fsmExpectedQty = GetFsmExpectedPosition(acct.Name)`

**Projected CYC:** 2

---

### Helper 3: `AuditFleet_ReconcileStaleFsms`

**Purpose:** Resolve hydrated Active FSMs with no order reference (restart edge case).
Side effect: may call `TryTerminateFollowerBracket` and adjust `fsmExpectedQty`.

```csharp
private void AuditFleet_ReconcileStaleFsms(
    List<FollowerBracketFSM> accountFsms,
    string accountName,
    int actualQty,
    ref int fsmExpectedQty
)
```

**Extracted logic:**
- `foreach (var f in accountFsms)` + `if (f.State == Active && f.EntryOrder == null)`
- Branch: `if (actualQty != 0)` => `fsmExpectedQty += actualQty`
- Branch: `else` => `TryTerminateFollowerBracket(...)` + `Print(...)`

**Projected CYC:** 4 (foreach=1, outer-if=1, inner-if=1, else=1)

**Jane Street:** `[MethodImpl(MethodImplOptions.NoInlining)]` — cold path (error recovery)

---

### Helper 4: `AuditFleet_ClearPositionPassState`

**Purpose:** Clear per-account position-pass failure state when FSM has recovered.

```csharp
private void AuditFleet_ClearPositionPassState(
    string accountName,
    int fsmExpectedQty
)
```

**Extracted logic:**
- `if (fsmExpectedQty != 0)` => `_positionPassFailedFirstSeen.TryRemove(acct.Name, out _)`

**Projected CYC:** 2

---

### Helper 5: `AuditFleet_AssembleOutputs`

**Purpose:** Assemble all out-parameter outputs from resolved state.

```csharp
private void AuditFleet_AssembleOutputs(
    string accountName,
    int actualQty,
    int fsmExpectedQty,
    out string expectedKey,
    out int expectedQty,
    out bool syncPending,
    out bool inFillGrace,
    out bool hasState
)
```

**Extracted logic:**
- `expectedKey = ExpKey(acct.Name)`
- `expectedQty = fsmExpectedQty`
- `syncPending = _dispatchSyncPendingExpKeys.ContainsKey(expectedKey)`
- `inFillGrace = IsReaperFillGraceActive(expectedKey)`
- `hasState = expectedQty != 0 || actualQty != 0`

**Projected CYC:** 3

---

### Parent After Extraction: `AuditFleet_CalculateExpectedActual`

**Remaining logic (orchestrator only):**
```csharp
AuditFleet_ResolvePosition(acct, out actualQty, out pos);
AuditFleet_CollectFsmState(acct, out accountFsms, out int fsmExpectedQty);
AuditFleet_ReconcileStaleFsms(accountFsms, acct.Name, actualQty, ref fsmExpectedQty);
AuditFleet_ClearPositionPassState(acct.Name, fsmExpectedQty);
AuditFleet_AssembleOutputs(acct.Name, actualQty, fsmExpectedQty,
    out expectedKey, out expectedQty, out syncPending, out inFillGrace, out hasState);
if (shouldLog && hasState)
{
    Print($"[REAPER] {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
}
```

**Projected CYC:** 6 (5 calls + if-shouldLog + if-hasState = 3 branch paths in parent)

---

## CYC Summary

| Method | CYC Before | CYC After |
|---|---|---|
| `AuditFleet_CalculateExpectedActual` | 382 | 6 |
| `AuditFleet_ResolvePosition` | N/A | 3 |
| `AuditFleet_CollectFsmState` | N/A | 2 |
| `AuditFleet_ReconcileStaleFsms` | N/A | 4 |
| `AuditFleet_ClearPositionPassState` | N/A | 2 |
| `AuditFleet_AssembleOutputs` | N/A | 3 |
| **max_cyc_projected** | — | **6** |

All helpers and the parent are <= 8. Jane Street strict threshold satisfied.

---

## Jane Street Alignment

| KB Source | Principle Applied |
|---|---|
| **carl_cook** | Cold path (`ReconcileStaleFsms`) uses `[NoInlining]`; hot output assembly uses minimal allocations |
| **trading_billions** | Single responsibility per helper — each helper does exactly ONE logical thing |
| **gjengset** | No shared mutable state crossing helper boundaries; `ref int fsmExpectedQty` is explicit, no hidden mutation |

---

## V12.23 Compliance

| Check | Status |
|---|---|
| ONE EPIC = ONE CONCERN | PASS — only `AuditFleet_CalculateExpectedActual` modified |
| Helpers are private, same file | PASS |
| No cross-file changes | PASS |
| Caller signature unchanged | PASS |
| No sibling method modifications | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-084 |
| **Method** | AuditFleet_CalculateExpectedActual |
| **CYC Baseline** | 382 |
| **Extraction Count** | 5 |
| **max_cyc_projected** | 6 |
| **jCodemunch Tools Used** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **Sequential Thoughts** | 5 |
| **Output** | docs/brain/EPIC-W7-084/02-architecture-plan.md |
