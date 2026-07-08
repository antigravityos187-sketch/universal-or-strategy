# EPIC-W7-025 Architecture Plan — Phase 2

**Epic**: EPIC-W7-025
**Method**: `CheckFFMAConditions`
**File**: [`src/V12_002.Entries.FFMA.cs`](src/V12_002.Entries.FFMA.cs:43)
**Wave**: 7
**Phase**: 2 — Architecture Planning
**Agent**: v12-phase2-architecture

---

## 1. Original Method (MCP-Confirmed)

| Property        | Value                                  |
|-----------------|----------------------------------------|
| Symbol ID       | `src/V12_002.Entries.FFMA.cs::V12_002.CheckFFMAConditions#method` |
| File            | `src/V12_002.Entries.FFMA.cs`          |
| Line            | 43                                     |
| End Line        | 108                                    |
| Signature       | `private void CheckFFMAConditions()`   |
| CYC (MCP)       | **16** (task list reported 2 — MCP override) |
| Assessment      | HIGH                                   |
| max_nesting     | 6                                      |
| Lines           | 66                                     |
| Params          | 0                                      |

> **Note**: Task list CYC=2 was incorrect. MCP `get_symbol_complexity` confirmed CYC=16. Extraction IS required.

### Branch Enumeration

| # | Branch Source | Branches |
|---|---------------|----------|
| 1 | `!isFFMAModeArmed \|\| !FFMAEnabled` | 2 |
| 2 | `ema9==null \|\| rsiIndicator==null \|\| currentATR<=0` | 3 |
| 3 | `CurrentBar < 20` | 1 |
| 4 | `try/catch` block | 1 |
| 5 | `rsiValue > FFMARSIOverbought && distanceFromEMA >= FFMAEMADistance && isRedCandle` | 3 |
| 6 | `stopDistance < tickSize * 2` (SHORT block) | 1 |
| 7 | `rsiValue < FFMARSIOversold && distanceFromEMA <= -FFMAEMADistance && isGreenCandle` | 3 |
| 8 | `stopDistance < tickSize * 2` (LONG block) | 1 |
| **Total** | | **CYC = 1 + 15 = 16** |

---

## 2. Extraction Plan

| Helper Method | Responsibility | Signature | Projected CYC |
|---------------|---------------|-----------|---------------|
| `CheckFFMAGuards` | Validate armed/enabled state and null/bar guards | `private bool CheckFFMAGuards()` | 7 |
| `ComputeFFMAStopDistance` | Clamp stop distance with MaximumStop and minimum tick floor | `private double ComputeFFMAStopDistance(double currentPrice, double candleExtreme)` | 2 |
| `TryExecuteFFMAShort` | SHORT setup: evaluate overbought + EMA distance + red candle, log, execute | `private bool TryExecuteFFMAShort(double rsiValue, double distanceFromEMA, double currentPrice)` | 4 |
| `TryExecuteFFMALong` | LONG setup: evaluate oversold + EMA distance + green candle, log, execute | `private bool TryExecuteFFMALong(double rsiValue, double distanceFromEMA, double currentPrice)` | 4 |

### Helper Signatures

```csharp
// HELPER 1 — Guard validation (CYC 7)
private bool CheckFFMAGuards()
{
    if (!isFFMAModeArmed || !FFMAEnabled)
        return false;
    if (ema9 == null || rsiIndicator == null || currentATR <= 0)
        return false;
    if (CurrentBar < 20)
        return false;
    return true;
}

// HELPER 2 — Stop distance computation (CYC 2)
private double ComputeFFMAStopDistance(double currentPrice, double candleExtreme)
{
    double stopDistance = Math.Min(Math.Abs(currentPrice - candleExtreme), MaximumStop);
    if (stopDistance < tickSize * 2)
        stopDistance = tickSize * 2;
    return stopDistance;
}

// HELPER 3 — SHORT entry (CYC 4)
private bool TryExecuteFFMAShort(double rsiValue, double distanceFromEMA, double currentPrice)
{
    if (rsiValue > FFMARSIOverbought && distanceFromEMA >= FFMAEMADistance && Close[0] < Open[0])
    {
        Print(string.Format(
            "FFMA SHORT TRIGGERED: RSI={0:F1} > {1} | Distance={2:F2}pts > {3}pts | RED candle",
            rsiValue, FFMARSIOverbought, distanceFromEMA, FFMAEMADistance));
        double stopDistance = ComputeFFMAStopDistance(currentPrice, High[0]);
        int contracts = CalculatePositionSize(stopDistance);
        ExecuteFFMAEntry(MarketPosition.Short, contracts);
        return true;
    }
    return false;
}

// HELPER 4 — LONG entry (CYC 4)
private bool TryExecuteFFMALong(double rsiValue, double distanceFromEMA, double currentPrice)
{
    if (rsiValue < FFMARSIOversold && distanceFromEMA <= -FFMAEMADistance && Close[0] > Open[0])
    {
        Print(string.Format(
            "FFMA LONG TRIGGERED: RSI={0:F1} < {1} | Distance={2:F2}pts (below by {3}pts) | GREEN candle",
            rsiValue, FFMARSIOversold, distanceFromEMA, FFMAEMADistance));
        double stopDistance = ComputeFFMAStopDistance(currentPrice, Low[0]);
        int contracts = CalculatePositionSize(stopDistance);
        ExecuteFFMAEntry(MarketPosition.Long, contracts);
        return true;
    }
    return false;
}
```

---

## 3. Parent After Extraction

```csharp
private void CheckFFMAConditions()
{
    if (!CheckFFMAGuards())
        return;

    try
    {
        double ema9Value = ema9[0];
        double rsiValue = rsiIndicator[0];
        double currentPrice = Close[0];
        double distanceFromEMA = currentPrice - ema9Value;

        if (TryExecuteFFMAShort(rsiValue, distanceFromEMA, currentPrice))
            return;

        TryExecuteFFMALong(rsiValue, distanceFromEMA, currentPrice);
    }
    catch (Exception ex)
    {
        Print("ERROR CheckFFMAConditions: " + ex.Message);
    }
}
```

**Parent CYC after extraction**: 3 (if/guard + try/catch + if/return)

---

## 4. max_cyc_projected

```
max_cyc_projected: 7
```

Projected CYC across all resulting methods:
| Method | CYC |
|--------|-----|
| `CheckFFMAConditions` (parent) | 3 |
| `CheckFFMAGuards` | 7 |
| `ComputeFFMAStopDistance` | 2 |
| `TryExecuteFFMAShort` | 4 |
| `TryExecuteFFMALong` | 4 |
| **max** | **7** ✓ <= 8 |

---

## 5. Jane Street Alignment Notes

| Rule | Source | Compliance |
|------|--------|-----------|
| CYC <= 8 per helper | trading_billions | max_cyc=7 ✓ |
| Single responsibility per helper | trading_billions | Each helper owns one concern ✓ |
| Zero-alloc hot path | carl_cook | Helpers receive `double` value params — no heap allocs ✓ |
| No LINQ | carl_cook | No LINQ used ✓ |
| Cold logging extracted out-of-line | carl_cook | `Print` calls stay inside TryExecuteFFMA* — these are cold-path signal triggers ✓ |
| No new `lock()` blocks | gjengset | No locks added ✓ |
| No volatile/barrier changes | gjengset | No threading primitives modified ✓ |
| Defense in depth | trading_billions | Guards isolated in `CheckFFMAGuards` — clear single-check point ✓ |

---

## 6. MCP Evidence

| Tool | Key Finding |
|------|-------------|
| `resolve_repo` | Repo indexed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols |
| `search_symbols` | Symbol confirmed at `src/V12_002.Entries.FFMA.cs` line 43 |
| `get_symbol_complexity` | CYC=16, max_nesting=6, lines=66, assessment=HIGH — **overrides task list CYC=2** |
| `get_symbol_source` | Full source reviewed: 3 guard blocks + try/catch + SHORT block + LONG block |
| `get_call_hierarchy` | 0 callers (private method within partial class), 60 callees (depth 3) |
| `get_dependency_graph` | `src/V12_002.Entries.FFMA.cs` has 0 external import edges (self-contained partial class) |

---

## 7. Sequential Thinking Evidence

**Thought 1** — Branch enumeration and CYC verification:
- Mapped all 8 branch points across the method body
- Confirmed CYC=16 matching MCP output (task list value of 2 is wrong)
- Identified 3 logical concerns: guards, SHORT entry, LONG entry

**Thought 2** — Extraction design:
- Designed 4 helpers with projected CYC of 7, 2, 4, 4
- Shared `ComputeFFMAStopDistance` eliminates duplicated stop-clamping logic
- Validated zero-alloc compliance (all double/int value params)
- Confirmed no lock() or LINQ introduced

**Thought 3** — Final validation:
- max_cyc_projected=7 <= 8 ✓
- 0 callers means no external signature impact
- Print calls remain in cold-path branches (carl_cook compliant)
- All 5 resulting methods (parent + 4 helpers) satisfy CYC <= 8

---

## 8. Agent Tracking

| Field | Value |
|-------|-------|
| Agent | v12-phase2-architecture |
| Wave | 7 |
| Epic | EPIC-W7-025 |
| Phase | 2 |
| Method | `CheckFFMAConditions` |
| File | `src/V12_002.Entries.FFMA.cs` |
| CYC (task list) | 2 |
| CYC (MCP confirmed) | **16** |
| max_cyc_projected | **7** |
| Extractions planned | 4 |
| Status | COMPLETE |
| Timestamp | 2025-07-11 |
