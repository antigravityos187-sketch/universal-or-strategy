# EPIC-W7-094 — Phase 0: Hotspot Analysis
## Method: `ExecuteMultiAccountMarket`
**Source:** `src/V12_002.SIMA.Execution.cs` · Lines 41–157  
**Wave:** 7 | **Phase:** 0  
**Generated:** 2025-07-16

---

## 1. Cyclomatic Complexity Breakdown (CYC = 17)

| # | Decision Point | Location (approx. line) | +CYC |
|---|---|---|---|
| 1 | Method base path | L41 | 1 |
| 2 | `if (!EnableSIMA)` early-return guard | L43 | +1 |
| 3 | `if (isFlattenRunning)` flatten guard | L47 | +1 |
| 4 | `foreach (Account acct in Account.All)` | L60 | +1 |
| 5 | `if (IsFleetAccount(acct))` prefix filter | L62 | +1 |
| 6 | `!activeFleetAccounts.TryGetValue(...)` (short-circuit `\|\|`) | L65 | +1 |
| 7 | `!isActive` (second condition of compound) | L65 | +1 |
| 8 | `if (EnableConsistencyLock)` | L75 | +1 |
| 9 | `if (dailyPL >= MaxDailyProfitCap)` | L78 | +1 |
| 10 | `if (order != null)` | L100 | +1 |
| 11 | `action == OrderAction.Buy \|\| action == OrderAction.BuyToCover` (1st OR operand) | L105 | +1 |
| 12 | `... ? quantity : -quantity` ternary | L105 | +1 |
| 13 | `catch (Exception ex)` exceptional path | L115 | +1 |
| 14 | `if (reservedDelta != 0)` rollback guard | L120 | +1 |
| 15 | `acct.Submit(new[] { order })` nominal vs null skip implicit | (post L107) | +1 |
| 16 | Report string build — multi-path `LogBuffer.Format` overload dispatch | L144 | +1 |
| 17 | Implicit return / fall-through of full report path | L156 | +1 |
| **Σ** | | | **17** |

---

## 2. Complexity Drivers

### 2.1 Guard Stack (pre-loop)
Two early-return guards — `EnableSIMA` and `isFlattenRunning` — gate the entire method.
Both are volatile reads from module-level fields shared with the flatten subsystem.

### 2.2 Per-Account Decision Tree (inner loop)
Each account iteration carries **5 nested decision points**:
1. Prefix filter (`IsFleetAccount`)
2. Active-fleet registry check (compound `||`)
3. Consistency-lock daily-P&L ceiling
4. Null-order guard post `CreateOrder`
5. Direction-conditional `reservedDelta` ternary

This fan-out across the `Account.All` collection is the dominant complexity contributor.

### 2.3 expectedPositions Reservation / Rollback Pattern
The pre-submit reservation (`AddExpectedPositionDeltaLocked`) with catch-block rollback introduces a two-arm exception path per iteration. The `reservedDelta != 0` guard in catch adds one extra node to the CFG.

### 2.4 Forensic Report Construction
Post-loop, a 16-line `StringBuilder` report is assembled unconditionally. While not branching in isolation, the embedded `LogBuffer.Format` calls with conditional field inclusion contribute to the path count tracked by static analyzers.

---

## 3. Blast Radius

| Dependent Symbol | File | Coupling Type |
|---|---|---|
| `TryHandleFleet_LongShort` | `src/V12_002.UI.IPC.Commands.Fleet.cs:440` | **Direct caller** (IPC dispatch path) |
| `EnableSIMA` | `src/V12_002.cs:195` | Feature flag guard |
| `isFlattenRunning` | `src/V12_002.cs:656` | Volatile flatten-guard state |
| `IsFleetAccount` | `src/V12_002.cs:864` | Account prefix filter helper |
| `activeFleetAccounts` | `src/V12_002.cs:195` | Fleet registry (`ConcurrentDictionary`) |
| `EnableConsistencyLock` / `MaxDailyProfitCap` | `src/V12_002.Properties.cs` | Risk parameter reads |
| `AddExpectedPositionDeltaLocked` | `src/V12_002.SIMA.cs:88` | Position tracker mutation |
| `ExpKey` | `src/V12_002.SIMA.cs` | Key derivation |
| `Account.All` | NinjaTrader.Cbi | Broker account enumeration (external) |
| `acct.CreateOrder` / `acct.Submit` | NinjaTrader.Cbi | Order submission (external, throws on failure) |
| `LogBuffer.Format` | `src/V12_002.StructuredLog.cs` | Logging utility |

**Cross-file span:** 4 source files + 2 external NinjaTrader APIs.

---

## 4. Risk Assessment

| Risk | Severity | Rationale |
|---|---|---|
| **Race: `reservedDelta` rollback partial coverage** | HIGH | If `CreateOrder` throws before `reservedDelta` is assigned, `reservedDelta == 0` and rollback is silently skipped. Position tracker may drift. |
| **`Account.All` live enumeration on strategy thread** | MEDIUM | NinjaTrader's `Account.All` collection can be mutated by the broker connection thread; no snapshot taken before loop. |
| **`successCount` incremented after reservation but before flush** | LOW | Count is purely cosmetic/forensic — does not affect order state, but misrepresents partial failures in the report. |
| **Forensic `StringBuilder` alloc on every call** | LOW | 512 + 1024 byte allocs on hot path; acceptable but noteworthy for latency budgets tracked in Phase 9. |
| **`EnablePathB` branch not guarded here** | INFO | `ExecuteMultiAccountMarket` is only reached when `EnablePathB == false`; the guard lives in the IPC caller, not here. Inversion of concern. |

---

## 5. Recommended Refactoring Targets (Phase 1+)

1. **Extract `ShouldSkipFleetAccount(acct, dispatchLog)` helper** — consolidates the 3-guard skip chain (inactive, consistency-lock) into a single predicate, removing 3 decision nodes from the main method.
2. **Snapshot `Account.All` before loop** — `Account.All.ToList()` (or array) eliminates cross-thread collection mutation risk during iteration.
3. **Move `reservedDelta` assignment before `CreateOrder`** — avoids the silent-skip bug: compute delta first, reserve, then create and submit; rollback is then always deterministic.
4. **Extract forensic report builder** — the 16-line `StringBuilder` block is identical in `ExecuteMultiAccountBracket` and `ExecuteMultiAccountMarket`; a shared helper reduces duplication and CYC by ~3 nodes each.
5. **Inline `successCount` check** — track fail-only; success is the complement; removes one counter variable.

---

## 6. Hotspot Summary

```
Method             : ExecuteMultiAccountMarket
File               : src/V12_002.SIMA.Execution.cs
Lines              : 41–157 (116 LOC)
CYC                : 17  ← CONFIRMED
Callers            : 1 direct (TryHandleFleet_LongShort via IPC)
Cross-file deps    : 4 source files + 2 NinjaTrader APIs
Primary risk       : reservedDelta rollback gap on CreateOrder throw
Recommended action : Extract skip-guard predicate + snapshot Account.All
```
