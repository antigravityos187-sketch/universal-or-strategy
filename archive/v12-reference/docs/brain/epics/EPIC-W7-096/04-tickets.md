# EPIC-W7-096 — Phase 4: Implementation Tickets
# ExecuteMultiAccountBracket — Surgical Extraction

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Epic:** EPIC-W7-096
**Method:** `ExecuteMultiAccountBracket`
**Source File:** [`src/V12_002.SIMA.Execution.cs`](src/V12_002.SIMA.Execution.cs)
**CYC Before:** 34
**Ticket Count:** 4
**Projected Parent CYC After All:** 6

---

## Summary

`ExecuteMultiAccountBracket` has CYC=34, exceeding the Jane Street mandatory threshold of 8. This plan defines 4 surgical extraction tickets that reduce the outer method to CYC=6 while preserving OCO atomicity (Submit stays in outer), fixing a correctness bug (missing `activeFleetAccounts` guard), and satisfying all V12 DNA rules. Each ticket targets a single concern with zero cross-file impact.

---

## Ticket Overview

| Ticket | Helper | Concern | Lines to Move | Projected Helper CYC | CYC Reduction from Outer |
|---|---|---|---|---|---|
| TICKET-1 | `ShouldSkipFleetAccountBracket` | Account eligibility filter + bug fix | ~14 | 5 | ~6 |
| TICKET-2 | `CalculateBracketPrices` | Pure price math + `BracketPriceResult` struct | ~12 | 4 | ~2 |
| TICKET-3 | `CreateBracketOrders` | Order factory (NO Submit — OCO atomicity) | ~22 | 7 | ~5 |
| TICKET-4 | `PrintFleetForensicReport` | Forensic timing report (15-line StringBuilder) | ~15 | 4 | ~21 |

**projected_parent_cyc_after_all: 6**
**max_cyc_projected: 7** (TICKET-3: `CreateBracketOrders`) ✅ ≤ 8

---

## TICKET-1: Extract `ShouldSkipFleetAccountBracket`

**ticket_id:** TICKET-1
**helper_name:** `ShouldSkipFleetAccountBracket`
**concern:** Account eligibility filter — IsFleetAccount check, `activeFleetAccounts.TryGetValue` guard (BUG FIX), `EnableConsistencyLock` block, `MaxDailyProfitCap` ceiling
**lines_to_move:** ~14
**cyc_reduction:** ~6 (removes Drivers 3+4 from outer method)
**projected_helper_cyc:** 5
**attribute:** `[AggressiveInlining]`
**return_type:** `bool` (`true` = skip this account)
**signature:** `private bool ShouldSkipFleetAccountBracket(Account acct, out string reason)`

### What to Extract
All lines inside the per-account loop that determine whether to skip the account:
1. `if (!IsFleetAccount(acct))` early-exit guard
2. **BUG FIX**: `if (!activeFleetAccounts.TryGetValue(acct.Name, out var isActive) || !isActive)` — this guard is **missing** in the current source (present in `ExecuteMultiAccountMarket`, absent here). Add it as the second check.
3. `if (EnableConsistencyLock)` + `if (dailyPL >= MaxDailyProfitCap)` — daily profit cap ceiling block

### Critical Constraint
`ConcurrentDictionary.TryGetValue` is used (lock-free). No `lock()` blocks may be introduced. The `activeFleetAccounts` dictionary is a `ConcurrentDictionary<string, bool>` — `TryGetValue` is thread-safe without locks.

### Verification
- Helper returns `true` (skip) for: non-fleet account, inactive account, daily cap reached
- `out reason` string carries the skip reason for the forensic log
- After extraction: outer loop body becomes `if (ShouldSkipFleetAccountBracket(acct, out reason)) continue;`

---

## TICKET-2: Extract `CalculateBracketPrices` + Define `BracketPriceResult`

**ticket_id:** TICKET-2
**helper_name:** `CalculateBracketPrices`
**concern:** Pure price math — `stopPrice`/`targetPrice` ternaries + `RoundToTickSize × 2`; introduces `BracketPriceResult` readonly struct (zero-alloc)
**lines_to_move:** ~12 (2 ternaries + 2 `RoundToTickSize` calls + 7-line struct definition)
**cyc_reduction:** ~2 (removes Driver 5 from outer method)
**projected_helper_cyc:** 4
**attribute:** `[AggressiveInlining]`
**return_type:** `BracketPriceResult` (new readonly struct)
**signature:** `private BracketPriceResult CalculateBracketPrices(OrderAction action, double currentPrice, double stopPoints, double targetPoints)`

### What to Extract
```
stopPrice = action == OrderAction.Buy ? currentPrice - stopPoints : currentPrice + stopPoints;
targetPrice = action == OrderAction.Buy ? currentPrice + targetPoints : currentPrice - targetPoints;
stopPrice = RoundToTickSize(stopPrice);
targetPrice = RoundToTickSize(targetPrice);
```

### New Type: `BracketPriceResult` Readonly Struct
```csharp
private readonly struct BracketPriceResult
{
    public readonly double StopPrice;
    public readonly double TargetPrice;
    public BracketPriceResult(double stopPrice, double targetPrice)
        => (StopPrice, TargetPrice) = (stopPrice, targetPrice);
}
```
Place in same file (`src/V12_002.SIMA.Execution.cs`) — zero cross-file dependency. Zero-alloc per carl_cook rule.

### Verification
- Pure function: no side effects, no field reads, no logging
- After extraction: `var prices = CalculateBracketPrices(action, currentPrice, stopPoints, targetPoints);`
- Caller accesses: `prices.StopPrice`, `prices.TargetPrice`

---

## TICKET-3: Extract `CreateBracketOrders`

**ticket_id:** TICKET-3
**helper_name:** `CreateBracketOrders`
**concern:** Order factory — 3× `CreateOrder` calls + 3× `OrderAction` ternaries. **NEVER calls Submit** (OCO atomicity preserved in outer method).
**lines_to_move:** ~22
**cyc_reduction:** ~5 (removes partial Driver 3 + Driver 6 from outer method)
**projected_helper_cyc:** 7
**attribute:** *(none)*
**return_type:** `bool` (`true` = all three orders non-null)
**signature:** `private bool CreateBracketOrders(Account acct, OrderAction action, int qty, double entryPrice, double stopPrice, double targetPrice, string signalName, string ocoId, out Order entry, out Order stop, out Order target)`

### What to Extract
The 3× `CreateOrder` factory calls with their `OrderAction` ternary arguments:
- Entry order creation
- Stop order creation (ternary: `Buy ? Sell : BuyToCover`)
- Target order creation (ternary: `Buy ? Sell : BuyToCover`)

### CRITICAL: OCO Atomicity Constraint
**`acct.Submit(new[] { entry, stop, target })` MUST remain in `ExecuteMultiAccountBracket`.**

`CreateBracketOrders` creates the three `Order` objects via the factory. The caller (outer method) then calls `Submit` with all three in a single array — this is the broker-side OCO linkage that atomically links stop and target. Splitting `Submit` into the helper would break the OCO relationship.

The `reservedDelta` assignment (`AddExpectedPositionDeltaLocked` pre-Submit) and the `catch (Exception ex) { if (reservedDelta != 0) ... }` rollback guard **also stay in the outer method** — they are part of the Phase 7 C-02/GAP-2 race-window atomicity fix.

### Verification
- Helper returns `false` if any order is null; outer skips `Submit` on `false`
- After extraction outer code pattern:
  ```
  if (!CreateBracketOrders(acct, action, qty, entryPrice, prices.StopPrice, prices.TargetPrice, signalName, ocoId, out var entry, out var stop, out var target))
      continue;
  reservedDelta = AddExpectedPositionDeltaLocked(...);
  acct.Submit(new[] { entry, stop, target });
  ```

---

## TICKET-4: Extract `PrintFleetForensicReport`

**ticket_id:** TICKET-4
**helper_name:** `PrintFleetForensicReport`
**concern:** Forensic timing report — 15-line `StringBuilder` block assembling pulse output with compound Boolean expressions (tool-attributed +20 CYC in outer)
**lines_to_move:** ~15
**cyc_reduction:** ~21 (removes Driver 8 entirely from outer method)
**projected_helper_cyc:** 4
**attribute:** `[NoInlining]`
**return_type:** `void`
**signature:** `private void PrintFleetForensicReport(string header, LogBuffer log, int okCount, double setupMs, double loopMs)`

### What to Extract
The 15-line `StringBuilder` forensic timing report block that assembles the pulse output after the fleet loop. The McCabe complexity analyzer attributes ~+20 CYC to this block due to compound Boolean operators (`&&`, `||`) inside interpolation expressions (e.g., `acct != null && acct.Name != null && isActive && ...`). This is a tool-measurement artifact — the logical complexity is low (sequential formatting). Extraction eliminates all tool-attributed CYC from the outer method.

### [NoInlining] Rationale
Cold logging path. Marking `[NoInlining]` prevents the JIT from inlining a 15-line method body into the hot account iteration loop, keeping the hot path lean.

### Shared Format Note
`PrintFleetForensicReport` shares the forensic report format with `ExecuteMultiAccountMarket`. If a shared private helper already exists for that method, consider consolidating — but do not introduce cross-method coupling unless the format is byte-identical.

### Verification
- After extraction: outer calls `PrintFleetForensicReport(header, log, okCount, setupMs, loopMs);`
- No field mutations inside helper — read-only access to counts and timing values

---

## CYC Projection After All Extractions

| Method | CYC Before | CYC After | Delta |
|---|---|---|---|
| `ExecuteMultiAccountBracket` (outer) | 34 | **6** | -28 |
| `ShouldSkipFleetAccountBracket` | — | 5 | new |
| `CalculateBracketPrices` | — | 4 | new |
| `CreateBracketOrders` | — | 7 | new |
| `PrintFleetForensicReport` | — | 4 | new |
| **max_cyc_projected** | — | **7** | ✅ ≤ 8 |

**Total CYC reduction: 34 → 6 (outer) = 82.4% reduction in parent method**
**Jane Street threshold (CYC ≤ 8): ALL methods pass ✅**

---

## DNA Compliance (from Phase 3 Audit)

| Check | Status |
|---|---|
| Zero `lock()` blocks | ✅ PASS |
| ASCII-only literals | ✅ PASS |
| No scope creep (single file) | ✅ PASS |
| xUnit tests only (no NUnit/MSTest) | ✅ PASS |
| max_cyc_projected ≤ 8 | ✅ PASS (7) |
| Zero dependency cycles | ✅ PASS |
| OCO atomicity preserved | ✅ PASS (Submit in outer) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-096 |
| **Method** | `ExecuteMultiAccountBracket` |
| **Source File** | `src/V12_002.SIMA.Execution.cs` |
| **CYC Before** | 34 |
| **ticket_count** | 4 |
| **projected_parent_cyc_after_all** | 6 |
| **max_cyc_projected** | 7 (`CreateBracketOrders`) |
| **Bug Fix Included** | Yes — TICKET-1 adds missing `activeFleetAccounts` guard |
| **OCO Constraint** | `Submit` preserved in outer method (not extracted) |
| **MCP Tools Used** | resolve_repo, sequentialthinking (×3) |
| **DNA Verdict** | PASS (from Phase 3) |
| **Sequential Thinking** | 3 thoughts — driver analysis, line estimation, final validation |
