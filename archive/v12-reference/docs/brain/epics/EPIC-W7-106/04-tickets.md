# EPIC-W7-106 — Phase 4 Tickets
**Method**: LogHealthCheckResult
**Source**: src/V12_002.SIMA.Fleet.cs
**CYC**: 0 (tool artefact; manual analysis: ~10 McCabe strict)
**Lane**: P4-L7
**DNA Verdict**: PASS
**max_cyc_projected**: 4
**extraction_count**: 3

---

## Ticket Summary

| # | Ticket | Type | CYC Target | Method |
|---|--------|------|-----------|--------|
| 1 | Extract IsFleetAllClear | extraction | ≤4 | `IsFleetAllClear` |
| 2 | Extract IsFleetPendingReconciliation | extraction | ≤4 | `IsFleetPendingReconciliation` |
| 3 | Extract DescribeActiveComponent | extraction | ≤3 | `DescribeActiveComponent` |
| 4 | Refactor parent + integration test | refactor | ≤3 | `LogHealthCheckResult` |

---

## Ticket 1 — Extract IsFleetAllClear

**Type**: extraction
**Target CYC**: ≤4
**File**: `src/V12_002.SIMA.Fleet.cs`
**Complexity Driver**: Driver 1 — 4-predicate AND-chain guard

**Description**:
Extract the all-clear boolean predicate from the first if-branch of `LogHealthCheckResult` into a dedicated private static helper `IsFleetAllClear`. This predicate returns `true` when the broker is flat AND no FSM, position, or dispatch is active. Removing this 4-predicate AND-chain from the parent eliminates 3 decision points, reducing parent CYC by 3.

**Signature**:
```csharp
private static bool IsFleetAllClear(
    bool brokerFlat,
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending)
```

**Logic**:
```csharp
return brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending;
```

**Acceptance Criteria**:
- [ ] `IsFleetAllClear` added as `private static bool` in `src/V12_002.SIMA.Fleet.cs` within the same partial class
- [ ] Method body contains exactly one return statement with the 4-predicate AND expression
- [ ] CYC of `IsFleetAllClear` ≤ 4
- [ ] No lock() blocks introduced
- [ ] All string literals in method remain ASCII-only
- [ ] xUnit [Fact] test: `IsFleetAllClear(true, false, false, false)` returns `true`
- [ ] xUnit [Fact] test: `IsFleetAllClear(false, false, false, false)` returns `false` (brokerFlat=false)
- [ ] xUnit [Fact] test: `IsFleetAllClear(true, true, false, false)` returns `false` (hasActiveFsm=true)
- [ ] xUnit [Fact] test: `IsFleetAllClear(true, false, true, false)` returns `false` (hasActivePosition=true)
- [ ] Build passes: `dotnet build src/`
- [ ] CSharpier check passes: `dotnet csharpier check src/`

---

## Ticket 2 — Extract IsFleetPendingReconciliation

**Type**: extraction
**Target CYC**: ≤4
**File**: `src/V12_002.SIMA.Fleet.cs`
**Complexity Driver**: Driver 2 — asymmetric else-if branch with OR fan-out

**Description**:
Extract the pending-reconciliation boolean predicate from the else-if branch of `LogHealthCheckResult` into a dedicated private static helper `IsFleetPendingReconciliation`. This predicate returns `true` when the broker is flat but at least one active component (FSM, position, or pending dispatch) is present. The redundant `brokerFlat &&` re-check in the original else-if is dropped — the helper is only called after `IsFleetAllClear` returns false. Removing the 3-predicate OR fan-out from the parent eliminates 2 decision points.

**Signature**:
```csharp
private static bool IsFleetPendingReconciliation(
    bool brokerFlat,
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending)
```

**Logic**:
```csharp
return brokerFlat && (hasActiveFsm || hasActivePosition || hasDispatchPending);
```

**Acceptance Criteria**:
- [ ] `IsFleetPendingReconciliation` added as `private static bool` in `src/V12_002.SIMA.Fleet.cs` within the same partial class
- [ ] Method body contains exactly one return statement with the compound boolean expression
- [ ] CYC of `IsFleetPendingReconciliation` ≤ 4
- [ ] No lock() blocks introduced
- [ ] All string literals remain ASCII-only
- [ ] xUnit [Fact] test: `IsFleetPendingReconciliation(true, true, false, false)` returns `true` (FSM active)
- [ ] xUnit [Fact] test: `IsFleetPendingReconciliation(true, false, true, false)` returns `true` (position active)
- [ ] xUnit [Fact] test: `IsFleetPendingReconciliation(true, false, false, true)` returns `true` (dispatch pending)
- [ ] xUnit [Fact] test: `IsFleetPendingReconciliation(false, true, false, false)` returns `false` (not flat)
- [ ] Build passes: `dotnet build src/`
- [ ] CSharpier check passes: `dotnet csharpier check src/`

---

## Ticket 3 — Extract DescribeActiveComponent

**Type**: extraction
**Target CYC**: ≤3
**File**: `src/V12_002.SIMA.Fleet.cs`
**Complexity Driver**: Driver 3 — nested ternary in string.Format call

**Description**:
Extract the active-component naming logic from the nested ternary inside `string.Format` in `LogHealthCheckResult` into a dedicated private static helper `DescribeActiveComponent`. Replace the ternary with explicit `if/return` branches. This produces a readable, independently testable method and eliminates the nesting complexity from the parent's format call. Returns one of three ASCII diagnostic strings: `"FSM"`, `"DISPATCH"`, or `"ACTIVE_POSITION"`.

**Signature**:
```csharp
private static string DescribeActiveComponent(
    bool hasActiveFsm,
    bool hasDispatchPending)
```

**Logic**:
```csharp
if (hasActiveFsm)
{
    return "FSM";
}
if (hasDispatchPending)
{
    return "DISPATCH";
}
return "ACTIVE_POSITION";
```

**Acceptance Criteria**:
- [ ] `DescribeActiveComponent` added as `private static string` in `src/V12_002.SIMA.Fleet.cs` within the same partial class
- [ ] No ternary operators in the method body — uses explicit if/return branches only
- [ ] Returns only ASCII-only string literals: `"FSM"`, `"DISPATCH"`, `"ACTIVE_POSITION"`
- [ ] CYC of `DescribeActiveComponent` ≤ 3
- [ ] No lock() blocks introduced
- [ ] xUnit [Fact] test: `DescribeActiveComponent(true, false)` returns `"FSM"`
- [ ] xUnit [Fact] test: `DescribeActiveComponent(false, true)` returns `"DISPATCH"`
- [ ] xUnit [Fact] test: `DescribeActiveComponent(false, false)` returns `"ACTIVE_POSITION"`
- [ ] Build passes: `dotnet build src/`
- [ ] CSharpier check passes: `dotnet csharpier check src/`

---

## Ticket 4 — Refactor LogHealthCheckResult Parent + Integration Test

**Type**: refactor
**Target CYC**: ≤3 (parent method)
**File**: `src/V12_002.SIMA.Fleet.cs`
**Depends On**: Tickets 1, 2, 3 (all three helpers must be present)

**Description**:
Replace the inline boolean logic in `LogHealthCheckResult` with calls to the three helpers extracted in Tickets 1–3. The parent body is reduced to two guarded early-return branches (delegating all boolean evaluation to named predicates) plus a single fallthrough `AppendLine` for the NOT_FLAT case. Add an xUnit integration test that exercises all three health state paths using a `StringBuilder` and verifies the appended log line content for each state.

**Pseudocode after refactor**:
```csharp
private void LogHealthCheckResult(
    string accountName,
    bool brokerFlat,
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending,
    StringBuilder dispatchLog)
{
    if (IsFleetAllClear(brokerFlat, hasActiveFsm, hasActivePosition, hasDispatchPending))
    {
        dispatchLog.AppendLine(string.Format("[{0}] HealthCheck: FLAT+CLEAR", accountName));
        return;
    }

    if (IsFleetPendingReconciliation(brokerFlat, hasActiveFsm, hasActivePosition, hasDispatchPending))
    {
        string component = DescribeActiveComponent(hasActiveFsm, hasDispatchPending);
        dispatchLog.AppendLine(string.Format("[{0}] HealthCheck: FLAT+PENDING component={1}", accountName, component));
        return;
    }

    dispatchLog.AppendLine(string.Format("[{0}] HealthCheck: NOT_FLAT", accountName));
}
```

**Acceptance Criteria**:
- [ ] `LogHealthCheckResult` body uses `IsFleetAllClear`, `IsFleetPendingReconciliation`, and `DescribeActiveComponent` — no inline boolean operators remain in the parent body
- [ ] Method signature unchanged (caller `ShouldSkipFleet_RunHealthCheck` at line 478 requires no modification)
- [ ] CYC of `LogHealthCheckResult` after refactor ≤ 3
- [ ] All format strings remain ASCII-only: `"[{0}] HealthCheck: FLAT+CLEAR"`, `"[{0}] HealthCheck: FLAT+PENDING component={1}"`, `"[{0}] HealthCheck: NOT_FLAT"`
- [ ] No lock() blocks introduced
- [ ] xUnit integration [Fact] test: ALL_CLEAR path — `IsFleetAllClear(true, false, false, false)` → log contains `"FLAT+CLEAR"`
- [ ] xUnit integration [Fact] test: PENDING_RECONCILIATION path — `IsFleetPendingReconciliation(true, true, false, false)` → log contains `"FLAT+PENDING component=FSM"`
- [ ] xUnit integration [Fact] test: NOT_FLAT path — `brokerFlat=false` → log contains `"NOT_FLAT"`
- [ ] Build passes: `dotnet build src/`
- [ ] CSharpier check passes: `dotnet csharpier check src/`
- [ ] `deploy-sync.ps1` executed after all changes to re-synchronize NinjaTrader hard links

---

## Agent Tracking

- **Agent Name**: v12-phase4-tickets
- **Wave**: 7
- **Epic**: EPIC-W7-106
- **Method**: LogHealthCheckResult
- **Source**: src/V12_002.SIMA.Fleet.cs
- **CYC (tool-reported)**: 0 (parse artefact)
- **CYC (manual McCabe strict)**: ~10
- **max_cyc_projected**: 4
- **extraction_count**: 3
- **ticket_count**: 4
- **Sequential Thinking calls**: 4 (1 probe + 3 planning thoughts)
- **jCodemunch tools called**: resolve_repo, get_symbol_complexity (not-found — confirmed parse artefact)
- **DNA verdict (Phase 3)**: PASS
- **MCP resolve_repo**: SUCCESS (antigravityos187-sketch/universal-or-strategy, 5147 symbols)
- **Phase**: 4 complete
