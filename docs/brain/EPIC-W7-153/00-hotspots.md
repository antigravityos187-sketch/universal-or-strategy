# EPIC-W7-153 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field        | Value                                            |
|--------------|--------------------------------------------------|
| Method Name  | `HandleTrimCommand`                              |
| CYC Score    | **20**                                           |
| File Path    | `src/V12_002.UI.IPC.Commands.Config.cs`          |
| Lines        | 37–146 (110 lines)                               |
| Signature    | `private void HandleTrimCommand(string action, string[] parts)` |

---

## Blast Radius Summary

`HandleTrimCommand` is invoked from **one call site**: [`TryHandleFleet_Trim`](src/V12_002.UI.IPC.Commands.Fleet.cs:83) in `V12_002.UI.IPC.Commands.Fleet.cs`, which routes `TRIM_25` / `TRIM_50` IPC commands.

| Dimension             | Detail                                                                                      |
|-----------------------|---------------------------------------------------------------------------------------------|
| **Direct callers**    | `TryHandleFleet_Trim` (Fleet.cs:87) — single call site, gated by `action == "TRIM_25"` or `"TRIM_50"` |
| **Shared state**      | `activePositions` (`ConcurrentDictionary`) — read by 41 source files; writes `rawQty` via `SubmitOrderUnmanaged` / `Account.Submit` |
| **SIMA integration**  | Mutates live broker orders on `pos.ExecutingAccount` when `EnableSIMA && pos.IsFollower`    |
| **Thread risk**       | `pos.RemainingContracts` is `volatile` (written from `OnOrderUpdate`, `OnExecutionUpdate`, `OnBarUpdate` threads); any extracted helper must not re-read `RemainingContracts` across yield points |
| **Persistence side-effect** | None — does not call `MarkStickyDirty()` (contrast with `HandleConfigCommand`)       |
| **Extraction risk**   | **Low** — logic is self-contained; extracted helpers can be pure functions receiving already-computed values |

---

## Top 3 Complexity Drivers

### Driver 1 — Outer `foreach` with deep two-branch `if/else` (nesting depth 4)

```
foreach (pos in activePositions.Values.ToArray())          // loop → +1
  if (pos.RemainingContracts > 1)                          // branch → +1
    if (remainingAfterTrim < 1)                            // safety guard → +1
    if (rawQty >= 1 && (pos.RemainingContracts-rawQty)>=1) // compound guard → +2
      if (EnableSIMA && pos.IsFollower && …)               // fleet branch → +3 (&&)
        if (trimSig.Length > 50)                           // signal truncation → +1
      else
        if (pos.Direction == MarketPosition.Long)          // direction split → +1
    else { skip-log }
  else { skip-log }
```

This single loop accounts for **~11 CYC points** through nesting and compound boolean expressions. Extracting the per-position trim logic into `TrimSinglePosition(PositionInfo pos, double percent)` eliminates all inner branches from the top-level method.

---

### Driver 2 — Duplicate SIMA vs. unmanaged order submission paths

Lines 60–122 implement two completely separate order-submission paths behind the `if (EnableSIMA && pos.IsFollower && pos.ExecutingAccount != null)` guard:

- **SIMA path**: `Account.CreateOrder` + `Account.Submit` + Print (fleet log format)
- **Unmanaged path**: `SubmitOrderUnmanaged` × 2 (Long / Short) + Print (IPC log format)

The direction-based `if/else` on line 100 duplicates the `SubmitOrderUnmanaged` call, adding **+2 CYC** that can be eliminated by computing `trimAction` once (already done at line 56) and passing it into a single `SubmitTrimOrder(OrderAction, int, string)` helper.

---

### Driver 3 — Inline quantity safety calculations (lines 47–54)

Two consecutive guarded mutations of `rawQty`:

```csharp
int rawQty = Math.Max(1, (int)Math.Floor(pos.RemainingContracts * percent)); // +0 (Math.Max)
int remainingAfterTrim = pos.RemainingContracts - rawQty;
if (remainingAfterTrim < 1)          // → +1
    rawQty = pos.RemainingContracts - 1;
if (rawQty >= 1 && (...) >= 1)       // → +2 (compound)
```

This "safe-trim" arithmetic is a pure function of `(remainingContracts, percent)` and can be extracted as `int ComputeSafeTrimQty(int remaining, double percent)` returning `-1` (or `0`) when trim is impossible, collapsing **3 CYC points** into a single guard at the call site.

---

## Recommended Extraction Count

| Extract | Name (suggested)                                 | CYC reduction |
|---------|--------------------------------------------------|---------------|
| 1       | `ComputeSafeTrimQty(int remaining, double percent) → int` | −3           |
| 2       | `SubmitTrimOrderUnmanaged(OrderAction, int, string)`       | −2           |
| 3       | `TrimSinglePosition(PositionInfo pos, double percent)`     | −7 (moves loop body out; consolidates drivers 1+2+3) |

**Total recommended extractions: 3**
Projected post-refactor CYC for `HandleTrimCommand`: **≤ 5**
(loop entry + outer `if` + `foreach` iteration = 3 residual; method call overhead not counted)

---

## Agent Tracking

| Field             | Value                       |
|-------------------|-----------------------------|
| Agent Name        | v12-phase0-hotspot          |
| Bobcoins Used     | 6                           |
| Execution Time    | ~95 seconds                 |
| Wave              | 7                           |
| Phase             | 0 — Hotspot Analysis        |
| Epic              | EPIC-W7-153                 |
| Output            | docs/brain/EPIC-W7-153/00-hotspots.md |
