# Phase 1: Scope Definition - EPIC-W7-094

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-23T21:48:09Z

---

## Method Under Refactoring

| Attribute       | Value                                        |
|-----------------|----------------------------------------------|
| **Method**      | `ExecuteMultiAccountMarket`                  |
| **File**        | `src/V12_002.SIMA.Execution.cs`              |
| **Lines**       | 41–157 (117 lines)                           |
| **Signature**   | `private void ExecuteMultiAccountMarket(OrderAction action, int quantity, string signalName)` |
| **CYC (before)**| 17                                           |
| **Target CYC**  | ≤ 8 per method                               |

### CYC Contributor Inventory

The 17 decision points that drive the current CYC score:

| # | Decision Point | Source Line(s) | CYC Δ |
|---|----------------|---------------|-------|
| 1 | Method entry (base) | 41 | +1 |
| 2 | `if (!EnableSIMA)` | 43 | +1 |
| 3 | `if (isFlattenRunning)` | 46 | +1 |
| 4 | `foreach (Account acct in Account.All)` | 60 | +1 |
| 5 | `if (IsFleetAccount(acct))` | 62 | +1 |
| 6 | `!activeFleetAccounts.TryGetValue(…)` | 65 | +1 |
| 7 | `\|\| !isActive` (short-circuit) | 65 | +1 |
| 8 | `try` block | 72 | +1 |
| 9 | `if (EnableConsistencyLock)` | 75 | +1 |
| 10 | `if (dailyPL >= MaxDailyProfitCap)` | 78 | +1 |
| 11 | `if (order != null)` | 100 | +1 |
| 12 | Ternary: `action == OrderAction.Buy` | 104–105 | +1 |
| 13 | `\|\| action == OrderAction.BuyToCover` (short-circuit) | 105 | +1 |
| 14 | `catch (Exception ex)` | 115 | +1 |
| 15 | `if (reservedDelta != 0)` | 120 | +1 |
| 16–17 | Implicit loop-exit + fall-through paths | — | +2 |

**Total: 17**

---

## IN SCOPE — Extractions to Bring CYC to ≤ 8

Three private helper methods will be extracted from the body of `ExecuteMultiAccountMarket`.  
No public or internal API surface changes.

### Helper 1 — `IsAccountEligible`

**Responsibility**: Encapsulates the two skip-guard checks applied to each account before an order
is attempted (active-fleet-accounts registration check and consistency-lock PnL check).

**Extracted from lines**: 65–85

**Proposed signature**:
```csharp
private bool IsAccountEligible(Account acct, StringBuilder dispatchLog)
```

**Returns**: `true` if the account should proceed to order submission; `false` to skip.

**Decision points absorbed**: 4 (active-flag TryGetValue, `||!isActive`, ConsistencyLock guard,
dailyPL cap check)

**Residual CYC of this helper**: ≤ 5

---

### Helper 2 — `DispatchOrderToAccount`

**Responsibility**: Creates a market order for a single account, reserves the expected-position
delta before submit (Phase 7 GAP-3 protocol), submits, and rolls back the reservation on any
exception.  Appends a per-account OK/FAIL line to the dispatch log and increments the relevant
counter via `ref` parameters.

**Extracted from lines**: 87–124

**Proposed signature**:
```csharp
private void DispatchOrderToAccount(
    Account acct,
    OrderAction action,
    int quantity,
    string signalName,
    StringBuilder dispatchLog,
    ref int successCount,
    ref int failCount)
```

**Decision points absorbed**: 6 (`order != null`, ternary Buy/BuyToCover, `||` short-circuit,
`try`, `catch`, `reservedDelta != 0` rollback)

**Residual CYC of this helper**: ≤ 7

---

### Helper 3 — `BuildForensicReport`

**Responsibility**: Assembles the ASCII forensic-pulse report string from timing measurements and
the per-account dispatch log.  Pure data formatting — zero branches.

**Extracted from lines**: 135–156

**Proposed signature**:
```csharp
private static string BuildForensicReport(
    StringBuilder dispatchLog,
    OrderAction action,
    int quantity,
    int successCount,
    int failCount,
    double setupMs,
    double loopMs,
    double totalMs)
```

**Decision points absorbed**: 0 (string assembly only)

**Residual CYC of this helper**: 1

---

### Orchestrator After Extraction — `ExecuteMultiAccountMarket`

After the three extractions the orchestrator retains only:
- Two early-return guards (`EnableSIMA`, `isFlattenRunning`)
- Stopwatch setup
- `foreach` loop with `IsFleetAccount` check → `IsAccountEligible` → `DispatchOrderToAccount`
- Timing math
- `BuildForensicReport` call → `Print`

**Estimated residual CYC of orchestrator**: 5  
*(base +1, two guards +2, foreach +1, IsFleetAccount if +1)*

---

## OUT OF SCOPE

The following are explicitly **not changed** by this refactoring:

| Item | Rationale |
|------|-----------|
| Public/internal method signatures | `ExecuteMultiAccountMarket` signature is unchanged; IPC command handler at `src/V12_002.UI.IPC.Commands.Fleet.cs:440` will continue to work without modification |
| Observable behaviour | Order submission logic, skip semantics, Phase 7 GAP-3 rollback, and log output are preserved exactly |
| Log output format | The forensic pulse report ASCII layout is unchanged |
| Timing measurements | `Stopwatch` placement, `t0Ticks`, `tLoopStartTicks`, `tFinalTicks` collection points are unchanged |
| Other methods in the file | Only `ExecuteMultiAccountMarket` and its three new private helpers are touched |
| Other files | No file outside `src/V12_002.SIMA.Execution.cs` is modified |
| Unit tests | Test authorship is out of scope for Phase 1–2; noted as a follow-on action |
| `IsFleetAccount` call site | The outer `if (IsFleetAccount(acct))` gate remains in the orchestrator loop |

---

## Extraction Plan (Ordered)

```
Step 1 — Extract BuildForensicReport (zero-risk, zero branches, pure formatting)
Step 2 — Extract IsAccountEligible   (skip-guard logic, ref-free, easy to verify)
Step 3 — Extract DispatchOrderToAccount (core order submission + GAP-3 rollback)
Step 4 — Verify orchestrator CYC ≤ 8 with static analysis tool
```

Extracting in this order keeps the orchestrator compilable and testable after each step.

---

## Risk Assessment

| Risk | Likelihood | Severity | Mitigation |
|------|-----------|----------|------------|
| Phase 7 GAP-3 rollback broken by extraction | Low | High | `reservedDelta` and rollback logic stay together inside `DispatchOrderToAccount`; not split across methods |
| IPC call site broken | Very Low | High | Orchestrator signature is unchanged; IPC handler at `Fleet.cs:440` is untouched |
| Log output diverges | Low | Medium | `BuildForensicReport` receives the same `dispatchLog` StringBuilder assembled by the same helper calls |
| Thread-safety regression | Low | Medium | `AddExpectedPositionDeltaLocked` call remains inside `DispatchOrderToAccount`; locking semantics unchanged |
| `continue` semantics lost | Low | Low | `IsAccountEligible` returns `bool`; orchestrator loop uses `if (!IsAccountEligible(…)) continue` — equivalent to original `continue` statements |
| Static analysis CYC miscalculation | Low | Low | Each helper independently verifiable; orchestrator CYC ≤ 5 leaves headroom below threshold of 8 |

**Overall refactoring risk: LOW** — blast radius is zero (private method, one indirect caller via IPC), code is stable (not in top-50 churn hotspots), and all extracted helpers remain private within the same class.

---

## Success Criteria

| Criterion | Measurement |
|-----------|-------------|
| `ExecuteMultiAccountMarket` CYC ≤ 8 | Static analysis after extraction |
| Each extracted helper CYC ≤ 8 | Static analysis after extraction |
| Max nesting depth ≤ 3 in orchestrator | Manual inspection / static tool |
| Zero behaviour change | Diff of log output strings identical; order submission path identical |
| IPC command handler compiles unchanged | `src/V12_002.UI.IPC.Commands.Fleet.cs` not modified; build passes |
| No new public API surface | All three helpers are `private`; none appear in any interface or public class |
| Phase 7 GAP-3 rollback preserved | `reservedDelta` rollback path exists inside `DispatchOrderToAccount` catch block |

---

## Summary

Extract **3 private helpers** (`IsAccountEligible`, `DispatchOrderToAccount`, `BuildForensicReport`)
from `ExecuteMultiAccountMarket` to reduce its CYC from **17 → 5** while each helper stays within
CYC ≤ 7, achieving full Jane Street compliance (threshold: 8) across all produced methods.
