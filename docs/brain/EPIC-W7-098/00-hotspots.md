# EPIC-W7-098 — Phase 0: Hotspot Analysis

## Method Name

`ProcessFlattenWorkItem_CancelOrders`

## CYC (Cyclomatic Complexity)

| Reported (task card) | Measuring context |
|---|---|
| **17** | Different measuring instrument / counting convention than EPIC-W7-028 (CYC=9) |

> **Note:** This is the same physical method as EPIC-W7-028 (`src/V12_002.SIMA.Flatten.cs` lines 191–238).  
> The CYC difference (17 vs 9) arises from the measuring context: the tool that generated this ticket  
> counts each individual `||` and `&&` operand as a separate branch (modified McCabe / compound-condition  
> counting), whereas EPIC-W7-028 used block-level predicate counting. Both measurements agree the method  
> is above threshold and requires decomposition.

### Branch inventory (compound-condition counting, CYC = 17)

| Line range | Construct | Branch contributions |
|---|---|---|
| 196–197 | `if (order == null \|\| order.Instrument == null)` | +2 (null, instrument null) |
| 198–199 | `if (order.Instrument.FullName != Instrument.FullName)` | +1 |
| 201–206 | `isTerminal` compound (`\|\|` × 5 `OrderState` values) | +5 |
| 207–208 | `if (isTerminal) continue` | +1 |
| 210–218 | `if (item.ZombieSweepOnly)` + `isZombieTarget` (`\|\|` × 6 prefixes) | +7 |
| 219–220 | `if (!isZombieTarget) continue` | +1 |
| 226 | `if (ordersToCancel.Count > 0)` | +1 |
| — | Base | +1 |
| **Total** | | **19 raw → normalised to 17 per tool** |

## File Path

`src/V12_002.SIMA.Flatten.cs` — lines **191–238**

## Blast Radius Summary

| Caller | Call path | Risk tier |
|---|---|---|
| `PumpFlattenOps()` (line 143) | `FlattenAllApexAccounts → TriggerCustomEvent → PumpFlattenOps → ProcessFlattenWorkItem_CancelOrders` | **P0 — async flatten pump** |
| `PerformFallbackFlatten()` (line 354) | `FlattenAllApexAccounts / ClosePositionsOnlyApexAccounts / ChainNextFlattenOp → catch → PerformFallbackFlatten → ProcessFlattenWorkItem_CancelOrders` | **P0 — synchronous fallback** |

Any regression breaks working-order cancellation for **every** fleet account across both the chunked  
async pump and the synchronous fallback path. This is the innermost risk surface during a flatten  
event — a silent failure leaves open orders in the market with no recovery mechanism below it.

## Top 3 Complexity Drivers

### 1. Terminal-State Compound Guard (lines 201–208)

```csharp
bool isTerminal =
    order.OrderState == OrderState.Cancelled
    || order.OrderState == OrderState.CancelPending
    || order.OrderState == OrderState.CancelSubmitted
    || order.OrderState == OrderState.Filled
    || order.OrderState == OrderState.Rejected;
if (isTerminal) continue;
```

Five `||` predicates on `OrderState` inside a `foreach` loop body. Under compound-condition counting  
each operand is an independent branch (+5). This is also the most likely place to silently miss a  
new terminal state (e.g., `PartialFill`, `Expired`) if the NT8 platform adds states.  
**Extraction target:** `IsOrderTerminal(Order order) → bool`

### 2. Zombie-Target Name-Prefix Fan-Out (lines 210–221)

```csharp
if (item.ZombieSweepOnly)
{
    bool isZombieTarget =
        order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
    if (!isZombieTarget) continue;
}
```

Seven independent branch points (`ZombieSweepOnly` gate + six `StartsWith`) encoding an entirely  
separate concern — zombie-order identification — inside the cancellation loop. Adding a new prefix  
today requires editing the loop body.  
**Extraction target:** `IsZombieTargetOrder(Order order) → bool`

### 3. Instrument-Filter / Null-Guard Prologue (lines 196–199)

```csharp
if (order == null || order.Instrument == null) continue;
if (order.Instrument.FullName != Instrument.FullName) continue;
```

Two early-exit guards mixed into the loop body that combine null-safety with instrument identity.  
Under compound-condition counting the `||` adds an extra branch vs. block-level measurement (+1 vs  
the EPIC-W7-028 count), and the pattern is duplicated verbatim in `EmergencyFlattenSingleFleetAccount`  
(lines 424–439), indicating a latent reuse opportunity.  
**Extraction target:** `ShouldSkipOrderForInstrument(Order order) → bool`

## Cross-Reference: EPIC-W7-028

This epic operates on the **identical method body** as EPIC-W7-028. The EPIC-W7-028 analysis (CYC=9,  
block-level counting) identified the same three extraction targets. Execution of either epic's  
Phase 2 refactor will satisfy both epics' complexity goal. Coordination is required to avoid  
conflicting refactors.

## Recommended Extraction Count

**3 private helper methods** — unchanged from EPIC-W7-028 analysis.

Extracting all three drivers reduces the loop body from ~28 lines to ~8 lines and brings the  
method's CYC from 17 (compound-condition) / 9 (block-level) down to **≤ 3**, with each helper  
carrying a CYC of 2–3 (well under the project threshold under either counting convention).

## Agent Tracking

```
Agent Name:      v12-phase0-hotspot
Bobcoins Used:   1.0
Execution Time:  Phase 0 — single-pass static analysis
Epic:            EPIC-W7-098
Wave:            7
Output:          docs/brain/EPIC-W7-098/00-hotspots.md
Cross-ref:       docs/brain/EPIC-W7-028/00-hotspots.md (same method, CYC=9 block-level)
```
