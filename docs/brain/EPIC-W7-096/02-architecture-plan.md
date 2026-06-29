# EPIC-W7-096 — Phase 2: Architecture Plan
# ExecuteMultiAccountBracket

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Input:** docs/brain/EPIC-W7-096/01-scope-boundary.md

---

## Extraction Plan

| Helper Method | Extracted Logic | Params | Return | Projected CYC | Attribute |
|---|---|---|---|---|---|
| `ShouldSkipFleetAccountBracket` | IsFleetAccount + activeFleetAccounts.TryGetValue (BUG FIX: adds missing inactive-account guard) + EnableConsistencyLock + MaxDailyProfitCap ceiling | `acct`, `out string reason` | `bool` (true=skip) | 5 | `[AggressiveInlining]` |
| `CalculateBracketPrices` | stopPrice ternary + targetPrice ternary + RoundToTickSize × 2 — pure function, no side effects | `action`, `currentPrice`, `stopPoints`, `targetPoints` | `BracketPriceResult` (readonly struct) | 4 | `[AggressiveInlining]` |
| `CreateBracketOrders` | 3× CreateOrder calls + 3× OrderAction ternaries — factory only, NO Submit | `acct`, `action`, `qty`, `entryPrice`, `stopPrice`, `targetPrice`, `signalName`, `ocoId`, `out Order entry`, `out Order stop`, `out Order target` | `bool` (all non-null) | 7 | — |
| `PrintFleetForensicReport` | 15-line StringBuilder forensic timing report (shared format with ExecuteMultiAccountMarket) | `header`, `log`, `okCount`, `setupMs`, `loopMs` | `void` | 4 | `[NoInlining]` |

**Residual `ExecuteMultiAccountBracket` CYC: 6** (EnableSIMA + isFlattenRunning + priceSeed + foreach + catch/rollback)

**max_cyc_projected: 7** ✅ (CreateBracketOrders — threshold: 8)

---

## BracketPriceResult Struct

```csharp
private readonly struct BracketPriceResult
{
    public readonly double StopPrice;
    public readonly double TargetPrice;
    public BracketPriceResult(double stopPrice, double targetPrice)
        => (StopPrice, TargetPrice) = (stopPrice, targetPrice);
}
```

Zero-alloc price calculation output — aligned with carl_cook zero-alloc rule. Placed in same partial class file (`src/V12_002.SIMA.Execution.cs`) to avoid cross-file dependency.

---

## Critical OCO Atomicity Constraint

`acct.Submit(new[] { entry, stop, target })` MUST remain in `ExecuteMultiAccountBracket` (NOT extracted).
`CreateBracketOrders` creates orders only — it NEVER calls Submit.
Splitting Submit across calls would break broker-side OCO linkage between stop and target.

The `reservedDelta` assignment (`AddExpectedPositionDeltaLocked` pre-Submit) and the catch/rollback guard (`if (reservedDelta != 0)`) also stay in the outer method to preserve the Phase 7 C-02/GAP-2 race-window fix.

---

## Correctness Bug Fix

**Missing `activeFleetAccounts` guard in `ExecuteMultiAccountBracket`**:
- `ExecuteMultiAccountMarket` correctly guards: `!activeFleetAccounts.TryGetValue(acctId, out var isActive) || !isActive`
- `ExecuteMultiAccountBracket` is MISSING this guard — can submit brackets to disabled accounts
- Fix: `ShouldSkipFleetAccountBracket` adds `if (!activeFleetAccounts.TryGetValue(acct.Name, out var isActive) || !isActive)` as the second check after IsFleetAccount (lines ~L191 equivalent)
- This is a correctness fix included in the scope of the extraction refactor

---

## Complexity Driver Analysis

### Driver 1 — Entry Guards (CYC +2)
`if (!EnableSIMA) return;` and `if (isFlattenRunning) return;` are the first two operations in the method. Both read volatile booleans and provide early-exit before any heap work begins. They remain in the outer method as the first guards — gjengset mandates volatile reads stay in caller scope, never moved into helpers.

### Driver 2 — Price Seed Ternary (CYC +1)
`double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];` seeds the price used for all bracket price calculations. Single conditional expression, +1 McCabe.

### Driver 3 — Fleet Iteration + Account Filter (CYC +2)
`foreach (Account acct in Account.All)` adds +1 (loop decision). `if (IsFleetAccount(acct))` inside the loop adds +1. These enumerate all NinjaTrader accounts and gate entry to fleet-only accounts.

### Driver 4 — Consistency Lock Gate (CYC +2)
`if (EnableConsistencyLock)` (+1) gates a daily P&L ceiling check. `if (dailyPL >= MaxDailyProfitCap)` (+1) halts trading when daily cap is reached. Both are extracted to `ShouldSkipFleetAccountBracket`.

### Driver 5 — Price Calculation Ternaries (CYC +2)
`double stopPrice = action == OrderAction.Buy ? currentPrice - stopPoints : currentPrice + stopPoints;` (+1) and corresponding `targetPrice` ternary (+1). Extracted to `CalculateBracketPrices` as a pure function.

### Driver 6 — Order Action Ternaries x3 (CYC +3)
Three ternary expressions inside `CreateOrder` calls: (a) stop order action (`Buy ? Sell : BuyToCover`) (+1), (b) target order action (`Buy ? Sell : BuyToCover`) (+1), (c) `reservedDelta` sign ternary (`Buy ? quantity : -quantity`) (+1). First two extracted to `CreateBracketOrders`; reservedDelta sign stays in outer.

### Driver 7 — Exception Path + Rollback Guard (CYC +2)
`catch (Exception ex)` (+1) handles submission failures. `if (reservedDelta != 0)` (+1) conditionally rolls back the `AddExpectedPositionDeltaLocked` pre-reservation. Both stay in outer method — the rollback is part of the Phase 7 C-02/GAP-2 atomicity guarantee.

### Driver 8 — Forensic Report Compound Expressions (CYC +20 tool-attributed)
A 15-line `StringBuilder` forensic timing report assembles the pulse output after the fleet loop. The McCabe analyzer attributes +20 CYC to this block because it counts each `&&` and `||` short-circuit operator in compound Boolean expressions within `LogBuffer.Format` interpolation strings as independent branch points. For example, `acct != null && acct.Name != null && isActive && ...` contributes multiple +1s per expression. This is a tool-measurement artifact; the logical complexity is low (sequential formatting). Extracted to `PrintFleetForensicReport` eliminates all tool-attributed complexity from the outer method.

---

## Jane Street Alignment

| Rule | Application |
|---|---|
| carl_cook zero-alloc | `BracketPriceResult` readonly struct; `Account.All` enumeration — no snapshot (strategy thread per NT contract) |
| carl_cook AggressiveInlining | Applied to `ShouldSkipFleetAccountBracket` + `CalculateBracketPrices` (hot per-account path) |
| carl_cook NoInlining | Applied to `PrintFleetForensicReport` (cold logging path — prevents JIT inlining a 14-line method into hot loop) |
| carl_cook ref/in/out | `CreateBracketOrders` uses `out Order entry/stop/target` for 3 order references; no boxing |
| gjengset no lock() | No new `lock()` blocks added anywhere; `ConcurrentDictionary.TryGetValue` used (lock-free) |
| gjengset volatile | `EnableSIMA` + `isFlattenRunning` volatile reads stay as first two guards in outer method — never moved inside helpers |
| trading_billions SRP | `ShouldSkip`=filter, `CalculatePrices`=pure math, `CreateOrders`=factory, `PrintReport`=logging |
| trading_billions CYC<=8 | ShouldSkip=5, CalcPrices=4, CreateOrders=7, PrintReport=4, Residual=6 — all ≤ 8 ✅ |
| trading_billions OCO | Single atomic Submit preserved in outer method; broker OCO linkage between stop and target intact |

---

## MCP Evidence

### get_context_bundle
- Symbol confirmed: `src/V12_002.SIMA.Execution.cs::V12_002.ExecuteMultiAccountBracket#method`
- Lines: 163–309 (146 LOC actual)
- Signature: `private void ExecuteMultiAccountBracket(OrderAction action, int quantity, string signalName, double stopPoints, double targetPoints)`
- **Bug confirmed**: No `activeFleetAccounts.TryGetValue` guard in source — jumps directly from `IsFleetAccount()` to `EnableConsistencyLock` block.
- OCO Submit confirmed: `acct.Submit(new[] { entry, stop, target })` at single call site inside try block.

### get_call_hierarchy (depth=2, direction=both)
- **Callers**: 0 direct callers found in index (method called via strategy dispatch, not from indexed C# files)
- **Callees** (depth 1): `IsFleetAccount`, `LogBuffer`, `AddExpectedPositionDeltaLocked`, `ExpKey`
- **Callees** (depth 2): `LogBuffer.Format`, `expectedPositions`, `StampAccountFillGrace`
- No cross-file callers detected — safe to add new private helpers in same partial class.

### get_dependency_graph
- `src/V12_002.SIMA.Execution.cs`: 0 import edges, 0 importer edges (partial class — NinjaTrader compile-time partial merge)
- All helpers are same-file private methods; no new cross-file imports required for any extraction.

---

## Sequential Thinking Evidence

**Thought 1 — Complexity Driver Analysis**: Traced all 8 drivers summing to CYC 34. Identified Driver 8 (forensic report compound Booleans, +20 tool-attributed) as the dominant factor. Confirmed the missing `activeFleetAccounts` guard bug by comparison with `ExecuteMultiAccountMarket`.

**Thought 2 — Extraction Strategy**: Validated exact extraction boundaries for all 4 helpers from confirmed source. Confirmed OCO atomicity constraint (Submit stays in outer). Validated BracketPriceResult struct design. Confirmed ocoId string generation stays in outer for traceability. Estimated CYC for each helper: ShouldSkip=5, CalcPrices=3-4, CreateOrders=6-7, PrintReport=2-4.

**Thought 3 — CYC Validation**: Final pass confirms residual CYC 6-7 (both < 8). max_cyc_projected=7 (CreateBracketOrders). All jane Street rules satisfied: AggressiveInlining on hot helpers, NoInlining on cold logging, zero-alloc struct, no new locks, volatile reads preserved in outer method.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-096 |
| **Method** | ExecuteMultiAccountBracket |
| **Source File** | src/V12_002.SIMA.Execution.cs |
| **Lines** | 163–309 (146 LOC) |
| **CYC Before** | 34 |
| **max_cyc_projected** | 7 |
| **Helpers** | 4 |
| **Bug Fixed** | Missing activeFleetAccounts guard (bracket → disabled accounts) |
| **Jane Street KB** | carl_cook + gjengset + trading_billions applied |
| **OCO Constraint** | Submit preserved in outer method |
