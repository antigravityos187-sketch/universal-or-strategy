# EPIC-REAPER-AUDIT-CYC9 -- Phase 0: Hotspot Analysis

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Execution Time**: 2026-06-15 (single-session)

---

## Target Method

| Field        | Value                                      |
|--------------|--------------------------------------------|
| Method       | `AuditMaster_IsWorkingStopOrder`           |
| File         | `src/V12_002.REAPER.Audit.cs`              |
| Line         | 753                                        |
| LOC          | 7 (body, excluding signature and braces)   |
| CYC (audit)  | 9 (REFACTOR -- exceeds Jane Street CYC<=8) |
| CYC (manual) | 9 (confirmed by branch count below)        |

---

## Current Method Body (verbatim)

```csharp
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
```

---

## CYC=9 Branch-Count Proof

Starting from base complexity = 1:

| Branch                                           | +CYC | Running Total |
|--------------------------------------------------|------|---------------|
| `if (o == null ...)`                             | +1   | 2             |
| `o == null \|\|` (short-circuit OR)              | +1   | 3             |
| `o.Instrument?.FullName` (null-conditional `?.`) | +1   | 4             |
| `OrderState.Working \|\|` (isActive)             | +1   | 5             |
| `OrderType.StopMarket \|\|` (isStop)             | +1   | 6             |
| `OrderAction.Sell \|\|` (isProtective)           | +1   | 7             |
| `isActive &&` (return)                           | +1   | 8             |
| `isStop &&` (return)                             | +1   | **9**         |

**CYC = 9. Threshold = 8. Delta = +1. REFACTOR required.**

---

## Proposed Extractions (3 Boolean Helpers)

### Helper 1: `IsActiveOrderState(Order o)`

**Captures**: `o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted`

**Result CYC**: 2 (base + 1 for `||`)

**Removes from parent**: 1 branch (`||` in isActive line)

---

### Helper 2: `IsStopOrderType(Order o)`

**Captures**: `o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit`

**Result CYC**: 2 (base + 1 for `||`)

**Removes from parent**: 1 branch (`||` in isStop line)

---

### Helper 3: `IsProtectiveAction(Order o)`

**Captures**: `o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover`

**Result CYC**: 2 (base + 1 for `||`)

**Removes from parent**: 1 branch (`||` in isProtective line)

---

## Post-Extraction Projection

After extracting all 3 helpers, the parent method retains:

| Branch                                          | +CYC | Running Total |
|-------------------------------------------------|------|---------------|
| `if (o == null ...)`                            | +1   | 2             |
| `o == null \|\|` (short-circuit OR)             | +1   | 3             |
| `o.Instrument?.FullName` (null-conditional)     | +1   | 4             |
| `IsActiveOrderState(...) &&` in return          | +1   | 5             |
| `IsStopOrderType(...) &&` in return             | +1   | 6             |

**Parent CYC after extraction = 6. Compliant (<=8).**

All 3 helpers have CYC=2. Compliant.

---

## Scope: Only CYC>8 Violation

`complexity_audit.py` output (2026-06-15 run):

```
CYC > 8 (BLOCKING): 2
  - V12_002.REAPER.Audit.cs::AuditMaster_IsWorkingStopOrder (CYC=9, LOC=7)
  - V12_002.UI.Compliance.cs::EnsureDailySummaryCsv (CYC=8, LOC=30)
```

**Note**: `EnsureDailySummaryCsv` reports CYC=8 -- exactly AT the Jane Street threshold
(CYC<=8). This is a script labeling error; CYC=8 is WATCH, not BLOCKING. Manual branch
count confirms CYC=8 (7 conditional branches + base=1).

**`AuditMaster_IsWorkingStopOrder` is the ONLY true CYC>8 violation remaining.**

---

## Key Constraints for Ticket Execution

- `QueuedAccountOrderUpdate` is a **STRUCT** -- use `.` (not `?.`) for member access
- No `lock()` anywhere -- Actor/Enqueue pattern only (OKF: lock-free-patterns.md)
- xUnit only -- no NUnit or MSTest ([Fact], Assert.Equal)
- ASCII only -- no em-dashes, curly quotes, or Unicode > U+007F
- Run `powershell -File .\deploy-sync.ps1` after any `src/` edits to sync NT8 hard links

---

## MCP Evidence

- **jCodemunch resolve_repo**: local/universal-or-strategy-17657650, 2435 symbols, 101 C# files
- **search_symbols**: `AuditMaster_IsWorkingStopOrder` located at
  `src/V12_002.REAPER.Audit.cs` line 753
- **get_symbol_source**: Method body read verbatim (lines 753-763)
- **complexity_audit.py**: CYC=9 confirmed as the single true BLOCKING violation

## Sequential Thinking Evidence

- Thought 1 (CYC count): Manually walked all 8 conditional branches, confirmed CYC=9
- Thought 2 (EnsureDailySummaryCsv): Verified CYC=8 is AT threshold, not over it --
  script labels it BLOCKING erroneously; only AuditMaster_IsWorkingStopOrder is a true violation
