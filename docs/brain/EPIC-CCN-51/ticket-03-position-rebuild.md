---
# TICKET EPIC-CCN-51-03: Fleet Position Reconstruction
# Epic: EPIC-CCN-51
# Sequence: 3 of 5
# Depends on: ticket-02-prefix-helper.md
---

## Objective
Extract fleet position reconstruction logic from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) into a new method `RebuildFleetPositionFromEntry()` to reduce CYC from 50 to 40 and make position logic testable in isolation.

## Scope
IN scope:
- Extract lines 410-450 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Create new private method `RebuildFleetPositionFromEntry(Order entryOrder)` in same file
- Preserve inline business logic: MarketPosition determination, entry price calculation, stop distance calculation
- Preserve PositionInfo struct field population (10+ fields)

OUT of scope:
- Master position reconstruction (already delegates to `ReconstructMasterPositionFromBrackets()`)
- Changes to PositionInfo struct definition
- Changes to position calculation logic
- Master order adoption (Ticket 4)
- FSM hydration (Ticket 5)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-51/01-analysis.md`](docs/brain/EPIC-CCN-51/01-analysis.md) -- Section "Phase 2: Fleet Position Reconstruction" (lines 56-70)
- Approach: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 2 "Sub-Methods to Create" (lines 150-170)
- Sentinel Mitigations: None (pure function, reads from dictionaries populated in Ticket 1)

## Implementation Instructions

### Method Signature (from Approach Section 2, lines 153-157)
```csharp
/// <summary>
/// Reconstructs a PositionInfo struct from a fleet entry order.
/// Pure function - reads from entryOrders and stopOrders dictionaries.
/// No state mutations, no concurrency concerns.
/// </summary>
/// <param name="entryOrder">Fleet entry order (prefix "Fleet_")</param>
/// <returns>PositionInfo struct with position details</returns>
private PositionInfo RebuildFleetPositionFromEntry(Order entryOrder)
```

### Extraction Steps

1. **Create new method** `RebuildFleetPositionFromEntry()` in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) immediately after `ClassifyOrderByPrefix()`

2. **Extract position reconstruction logic** (preserve exact calculations from lines 410-450):
   ```csharp
   private PositionInfo RebuildFleetPositionFromEntry(Order entryOrder)
   {
       // Determine MarketPosition (preserve exact logic)
       MarketPosition marketPos = (entryOrder.OrderAction == OrderAction.Buy || 
                                   entryOrder.OrderAction == OrderAction.BuyToCover)
           ? MarketPosition.Long
           : MarketPosition.Short;
       
       // Calculate entry price (LimitPrice fallback to StopPrice)
       double entryPrice = entryOrder.LimitPrice > 0 
           ? entryOrder.LimitPrice 
           : entryOrder.StopPrice;
       
       // Find corresponding stop order for stop distance calculation
       string stopKey = $"{entryOrder.Account.Name}_{entryOrder.Instrument.FullName}";
       double stopDistance = 0;
       
       if (stopOrders.TryGetValue(stopKey, out Order stopOrder))
       {
           double stopPrice = stopOrder.StopPrice > 0 
               ? stopOrder.StopPrice 
               : stopOrder.LimitPrice;
           stopDistance = Math.Abs(entryPrice - stopPrice);
       }
       
       // Populate PositionInfo struct (preserve all fields from original)
       return new PositionInfo
       {
           MarketPosition = marketPos,
           EntryPrice = entryPrice,
           StopDistance = stopDistance,
           Quantity = entryOrder.Quantity,
           Instrument = entryOrder.Instrument,
           Account = entryOrder.Account,
           EntryTime = entryOrder.Time,
           // ... (include all other fields from original struct initialization)
       };
   }
   ```

3. **Update residual method** [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309):
   - Replace lines 410-450 with:
     ```csharp
     // Phase 2: Rebuild fleet positions (inline loop calling helper)
     foreach (var entry in entryOrders.Values)
     {
         string key = $"{entry.Account.Name}_{entry.Instrument.FullName}";
         activePositions[key] = RebuildFleetPositionFromEntry(entry);
     }
     ```

### V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (target ~40 lines)
- [ ] Residual method CYC target: 50 → 40 (10-point reduction)
- [ ] No logic drift -- pure structural movement only
- [ ] Position calculation logic unchanged (exact preservation)

## Sentinel Audit Mitigations (Reference Section 6 of Approach)

**No Sentinel mitigations required** - this is a pure function with:
- No state mutations (returns new PositionInfo struct)
- Reads from dictionaries populated in Ticket 1 (entryOrders, stopOrders)
- No concurrency concerns (called on strategy thread)
- No Actor serialization requirements (synchronous call)
- No latency constraints (simple struct creation)

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] `RebuildFleetPositionFromEntry()` method created in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs)
- [ ] Method returns PositionInfo struct with all fields populated
- [ ] Original method [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) reduced to CYC 40 (from 50)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC: 50 → 40 (10-point reduction)
- [ ] lock() audit: ZERO matches
- [ ] Residual method contains foreach loop calling helper (lines 225-228 in approach)
- [ ] All PositionInfo fields match original struct initialization
- [ ] MarketPosition logic preserved: Buy/BuyToCover → Long, else Short
- [ ] Entry price logic preserved: LimitPrice fallback to StopPrice
- [ ] Stop distance logic preserved: Math.Abs(entryPrice - stopPrice)
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Critical Verification Tests

### Test Case 1: Long Position (Buy Order)
```csharp
// Given: entryOrder with OrderAction.Buy, LimitPrice=100, Quantity=10
// Expected: PositionInfo with MarketPosition.Long, EntryPrice=100
```

### Test Case 2: Short Position (Sell Order)
```csharp
// Given: entryOrder with OrderAction.Sell, LimitPrice=100, Quantity=10
// Expected: PositionInfo with MarketPosition.Short, EntryPrice=100
```

### Test Case 3: LimitPrice Fallback to StopPrice
```csharp
// Given: entryOrder with LimitPrice=0, StopPrice=100
// Expected: PositionInfo with EntryPrice=100 (fallback to StopPrice)
```

### Test Case 4: Stop Distance Calculation
```csharp
// Given: entryOrder with EntryPrice=100, stopOrder with StopPrice=95
// Expected: PositionInfo with StopDistance=5 (Math.Abs(100-95))
```

### Test Case 5: Missing Stop Order
```csharp
// Given: entryOrder with no corresponding stop order in stopOrders dictionary
// Expected: PositionInfo with StopDistance=0 (default when stop not found)
```

### Test Case 6: Field-by-Field Comparison
**CRITICAL**: Compare extracted method output to current inline logic for ALL PositionInfo fields:
- [ ] MarketPosition (Long/Short)
- [ ] EntryPrice (LimitPrice or StopPrice)
- [ ] StopDistance (calculated from stop order)
- [ ] Quantity (from entry order)
- [ ] Instrument (from entry order)
- [ ] Account (from entry order)
- [ ] EntryTime (from entry order)
- [ ] [List all other fields from PositionInfo struct]

**Verification Method**: 
1. Read original lines 410-450 to identify ALL PositionInfo fields being set
2. Ensure extracted method populates EVERY field identically
3. Document any fields that are conditionally set (e.g., only set if stop order exists)