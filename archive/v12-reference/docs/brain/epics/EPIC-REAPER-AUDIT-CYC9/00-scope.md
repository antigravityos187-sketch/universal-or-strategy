# EPIC-REAPER-AUDIT-CYC9 -- Phase 1: Scope Definition

## Agent Tracking

- **Agent Name**: v12-phase1-scope
- **Mode**: v12-phase1-scope (subtask, plan)
- **Execution Time**: 2026-06-15
- **Input**: `docs/brain/EPIC-REAPER-AUDIT-CYC9/00-hotspots.md`
- **Output**: `docs/brain/EPIC-REAPER-AUDIT-CYC9/00-scope.md`
- **MCP Evidence**: jCodemunch grep confirmed method at line 753; source body read
  verbatim and verified identical to Phase 0 capture.
- **Sequential Thinking**: 3 thoughts -- CYC math re-verified, blast radius confirmed
  zero beyond single file, scope boundary finalized.

---

## 1. Problem Statement

`AuditMaster_IsWorkingStopOrder` in [`src/V12_002.REAPER.Audit.cs`](../../src/V12_002.REAPER.Audit.cs)
at line 753 has CYC=9. The Jane Street strict standard is CYC<=8 (OKF rule 6).
Delta = +1. One branch must be moved out of the parent.

---

## 2. In-Scope Changes

### 2.1 Target File

**ONE file only**: `src/V12_002.REAPER.Audit.cs`

### 2.2 Target Method (body rewrite -- signature UNCHANGED)

```csharp
// BEFORE (CYC=9)
private bool AuditMaster_IsWorkingStopOrder(Order o, string instrName)
{
    if (o == null || o.Instrument?.FullName != instrName)
    {
        return false;
    }
    bool isActive = o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted;
    bool isStop = o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit;
    bool isProtective = o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover;
    return isActive && isStop && isProtective;
}

// AFTER (CYC=6)
private bool AuditMaster_IsWorkingStopOrder(Order o, string instrName)
{
    if (o == null || o.Instrument?.FullName != instrName)
    {
        return false;
    }
    return IsActiveOrderState(o) && IsStopOrderType(o) && IsProtectiveAction(o);
}
```

### 2.3 New Private Expression-Body Helpers (3)

All helpers are: **private**, **same class**, **no state mutation**, **no allocations**.

| Helper | Expression | CYC |
|--------|-----------|-----|
| `IsActiveOrderState(Order o)` | `o.OrderState == OrderState.Working \|\| o.OrderState == OrderState.Accepted` | 2 |
| `IsStopOrderType(Order o)` | `o.OrderType == OrderType.StopMarket \|\| o.OrderType == OrderType.StopLimit` | 2 |
| `IsProtectiveAction(Order o)` | `o.OrderAction == OrderAction.Sell \|\| o.OrderAction == OrderAction.BuyToCover` | 2 |

**Implementation (expression-body syntax)**:

```csharp
private bool IsActiveOrderState(Order o) =>
    o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted;

private bool IsStopOrderType(Order o) =>
    o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit;

private bool IsProtectiveAction(Order o) =>
    o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover;
```

---

## 3. CYC Verification (Post-Extraction)

### Parent: `AuditMaster_IsWorkingStopOrder`

| Branch | +CYC | Running Total |
|--------|------|---------------|
| base | -- | 1 |
| `if (o == null ...)` | +1 | 2 |
| `o == null \|\|` (short-circuit OR) | +1 | 3 |
| `o.Instrument?.FullName` (null-conditional) | +1 | 4 |
| `IsActiveOrderState(o) &&` (return) | +1 | 5 |
| `IsStopOrderType(o) &&` (return) | +1 | 6 |

**Parent CYC after extraction = 6. Compliant (<=8).**

### Each Helper

Each helper has exactly: base(1) + one `||`(+1) = **CYC=2. Compliant (<=8).**

---

## 4. Out-of-Scope (Explicit Exclusions)

| Item | Reason |
|------|--------|
| `src/V12_002.UI.Compliance.cs::EnsureDailySummaryCsv` | CYC=8 -- AT threshold (not over). `complexity_audit.py` mislabels it BLOCKING; manual count confirms compliance. |
| All other `src/` files | Zero blast radius -- helpers are private, parent signature unchanged. |
| All test files | The 3 helpers are `private` -- not directly testable from outside the class. No xUnit tests required for this epic. |
| `QueuedAccountOrderUpdate` usages | STRUCT member access pattern (`.` not `?.`) is unaffected; this epic does not touch that type. |
| Caller at line 749 | `return orders.Any(o => AuditMaster_IsWorkingStopOrder(o, instrName));` stays UNCHANGED. |

---

## 5. Scope Boundary Statement

> **Exactly ONE file changes: `src/V12_002.REAPER.Audit.cs`.**
> Exactly ONE method body is rewritten: `AuditMaster_IsWorkingStopOrder`.
> Exactly THREE new private expression-body helpers are added to the same class.
> The method signature of `AuditMaster_IsWorkingStopOrder` is NOT changed.
> No public API surface is added or modified.
> No test files are created or modified.

---

## 6. Blast Radius Analysis

- **Direct caller**: `AuditMaster_HasWorkingStop` at line 749 -- lambda unchanged.
- **Callers of caller**: Out of scope for this epic.
- **New helpers**: Called only from `AuditMaster_IsWorkingStopOrder`. No other code references them.
- **Name collision check**: `IsActiveOrderState`, `IsStopOrderType`, `IsProtectiveAction` are new symbols with no prior usage in the file or codebase (confirmed by jCodemunch Phase 0 MCP evidence).

---

## 7. Branch Strategy

- **Branch**: `wave7/epic-reaper-audit-cyc9` off `main`
- **GitButler virtual branch** (V12.24 mandate -- no `git checkout -b`)
- **Single commit** touching exactly 1 file: `src/V12_002.REAPER.Audit.cs`
- **Post-commit**: Run `powershell -File .\deploy-sync.ps1` to sync NT8 hard links

---

## 8. OKF Constraint Checklist

| OKF Rule | Check |
|----------|-------|
| `lock()` banned | PASS -- expression-body bool helpers have no locks, no state mutation |
| `DateTime.Now` banned | PASS -- no time usage in scope |
| xUnit only (no NUnit/MSTest) | PASS -- no test files in scope |
| ASCII only (no Unicode > U+007F) | PASS -- all identifiers and comments ASCII |
| CYC<=8 (Jane Street strict) | PASS -- parent=6, each helper=2 |
| No new allocations on hot path | PASS -- bool returns, zero heap allocation |
| `QueuedAccountOrderUpdate` is STRUCT | N/A -- not touched by this extraction |
| `[MethodImpl(AggressiveInlining)]` | OPTIONAL -- helpers are small enough JIT inlines automatically |

---

## 9. Success Criteria

All criteria must pass before marking EPIC complete:

| # | Criterion | Tool | Pass Condition |
|---|-----------|------|---------------|
| 1 | Complexity audit | `python scripts/complexity_audit.py` | 0 violations CYC>8 |
| 2 | Build | `dotnet build` | 0 errors, 0 new warnings |
| 3 | ASCII gate | `powershell -File .\scripts\pre_push_validation.ps1 -Fast` | ASCII check passes |
| 4 | Lock-free audit | `grep -r "lock(" src/` | 0 matches |
| 5 | NT8 hard-link sync | `powershell -File .\deploy-sync.ps1` | exits 0 |

---

## 10. Phase Handoff

- **Next phase**: Phase 1.5 (Scope Boundary Validation) via `v12-phase1-5-boundary` mode
- **Input for Phase 1.5**: this file (`00-scope.md`)
- **Input for Phase 2**: `01-scope-boundary.md` (output of Phase 1.5)
