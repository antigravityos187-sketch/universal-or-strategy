---
# TICKET EPIC-CCN-51-02: Prefix Classification Helper
# Epic: EPIC-CCN-51
# Sequence: 2 of 5
# Depends on: ticket-01-fleet-adoption.md
---

## Objective
Extract duplicated prefix classification logic into a shared helper method `ClassifyOrderByPrefix()` to eliminate code duplication between fleet and master adoption paths, reducing CYC from 54 to 50.

## Scope
IN scope:
- Extract 8-way prefix classification logic from `AdoptFleetOrders()` (created in Ticket 1)
- Extract identical logic from lines 460-475 (master adoption section)
- Create new private method `ClassifyOrderByPrefix(string orderName)` in same file
- Return dictionary key string: "stop", "target1"-"target5", "entry", or null

OUT of scope:
- Enum-based classification (deferred to future epic EPIC-CCN-53)
- Changes to dictionary routing logic
- Changes to state guard logic
- Position reconstruction (Ticket 3)
- FSM hydration (Ticket 5)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-51/01-analysis.md`](docs/brain/EPIC-CCN-51/01-analysis.md) -- Section "Phase 3: Master Order Adoption" (lines 72-83) - documents duplication
- Approach: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 2 "Sub-Methods to Create" (lines 113-126)
- Sentinel Mitigations: None (pure function, no concurrency or state)

## Implementation Instructions

### Method Signature (from Approach Section 2, lines 116-119)
```csharp
/// <summary>
/// Classifies an order by its name prefix to determine target dictionary.
/// Pure function - no state mutations, no concurrency concerns.
/// </summary>
/// <param name="orderName">Order name (e.g., "Stop_AAPL_123", "T1_AAPL_456")</param>
/// <returns>Dictionary key: "stop", "target1"-"target5", "entry", or null if unrecognized</returns>
private string ClassifyOrderByPrefix(string orderName)
```

### Extraction Steps

1. **Create new method** `ClassifyOrderByPrefix()` in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) immediately after `AdoptFleetOrders()`

2. **Extract 8-way classification logic** (preserve exact string matching):
   ```csharp
   private string ClassifyOrderByPrefix(string orderName)
   {
       if (string.IsNullOrEmpty(orderName))
           return null;
       
       // 8-way prefix classification (preserve exact logic from lines 336-380)
       if (orderName.StartsWith("Stop_") || orderName.StartsWith("S_"))
           return "stop";
       else if (orderName.StartsWith("T1_"))
           return "target1";
       else if (orderName.StartsWith("T2_"))
           return "target2";
       else if (orderName.StartsWith("T3_"))
           return "target3";
       else if (orderName.StartsWith("T4_"))
           return "target4";
       else if (orderName.StartsWith("T5_"))
           return "target5";
       else if (orderName.StartsWith("Fleet_"))
           return "entry";
       else
           return null; // Unrecognized prefix
   }
   ```

3. **Update `AdoptFleetOrders()`** (from Ticket 1):
   - Replace 8-way if/else chain with:
     ```csharp
     string classification = ClassifyOrderByPrefix(ord.Name);
     if (classification == null) continue; // Skip unrecognized orders
     
     // Route to appropriate dictionary based on classification
     switch (classification)
     {
         case "stop":
             stopOrders[key] = ord;
             break;
         case "target1":
             target1Orders[key] = ord;
             break;
         case "target2":
             target2Orders[key] = ord;
             break;
         case "target3":
             target3Orders[key] = ord;
             break;
         case "target4":
             target4Orders[key] = ord;
             break;
         case "target5":
             target5Orders[key] = ord;
             break;
         case "entry":
             entryOrders[key] = ord;
             break;
     }
     adoptedCount++;
     ```

4. **Update master adoption section** (lines 460-475 in original method):
   - Replace duplicated 8-way if/else chain with same `ClassifyOrderByPrefix()` call
   - Use identical switch statement for dictionary routing
   - This change will be applied in Ticket 4 (Master Order Adoption)

### V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (target ~25 lines)
- [ ] Residual method CYC target: 54 → 50 (4-point reduction)
- [ ] No logic drift -- pure structural movement only
- [ ] String matching logic unchanged (exact preservation)

## Sentinel Audit Mitigations (Reference Section 6 of Approach)

**No Sentinel mitigations required** - this is a pure function with:
- No state mutations
- No dictionary access
- No concurrency concerns
- No Actor serialization requirements
- No latency constraints (simple string comparison)

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
- [ ] `ClassifyOrderByPrefix()` method created in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs)
- [ ] Method handles all 8 prefixes: Stop_, S_, T1_, T2_, T3_, T4_, T5_, Fleet_
- [ ] Method returns correct dictionary keys: "stop", "target1"-"target5", "entry", or null
- [ ] Method handles edge cases: null, empty string, no prefix, malformed prefix
- [ ] `AdoptFleetOrders()` updated to use helper (8-way if/else replaced with switch)
- [ ] Original method [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) reduced to CYC 50 (from 54)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC: 54 → 50 (4-point reduction)
- [ ] lock() audit: ZERO matches
- [ ] Test all 8 prefixes produce correct classification
- [ ] Test edge cases: null → null, empty → null, "Unknown_" → null
- [ ] Verify output matches original if/else logic for each case
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Critical Verification Tests

### Test Case 1: All Valid Prefixes
```csharp
// Expected outputs:
ClassifyOrderByPrefix("Stop_AAPL_123")  → "stop"
ClassifyOrderByPrefix("S_AAPL_123")     → "stop"
ClassifyOrderByPrefix("T1_AAPL_123")    → "target1"
ClassifyOrderByPrefix("T2_AAPL_123")    → "target2"
ClassifyOrderByPrefix("T3_AAPL_123")    → "target3"
ClassifyOrderByPrefix("T4_AAPL_123")    → "target4"
ClassifyOrderByPrefix("T5_AAPL_123")    → "target5"
ClassifyOrderByPrefix("Fleet_AAPL_123") → "entry"
```

### Test Case 2: Edge Cases
```csharp
// Expected outputs:
ClassifyOrderByPrefix(null)           → null
ClassifyOrderByPrefix("")             → null
ClassifyOrderByPrefix("Unknown_123")  → null
ClassifyOrderByPrefix("AAPL_123")     → null (no prefix)
```

### Test Case 3: Case Sensitivity
```csharp
// Expected outputs (verify case-sensitive matching):
ClassifyOrderByPrefix("stop_AAPL_123") → null (lowercase 's' not recognized)
ClassifyOrderByPrefix("STOP_AAPL_123") → "stop" (uppercase 'STOP' recognized)