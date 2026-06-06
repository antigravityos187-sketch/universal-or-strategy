---
# TICKET EPIC-CCN-51-04: Master Order Adoption
# Epic: EPIC-CCN-51
# Sequence: 4 of 5
# Depends on: ticket-02-prefix-helper.md
---

## Objective
Extract master order adoption logic from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) into a new method `AdoptMasterOrders()` to reduce CYC from 40 to 25 and eliminate remaining code duplication with fleet adoption.

## Scope
IN scope:
- Extract lines 452-480 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Create new private method `AdoptMasterOrders()` in same file
- Use shared `ClassifyOrderByPrefix()` helper (created in Ticket 2)
- Preserve single account loop: `foreach (Order ord in masterBroker996h.Orders.ToArray())`
- Preserve state guard: `IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true)`

OUT of scope:
- Fleet order adoption (Ticket 1)
- Prefix classification helper (Ticket 2)
- Position reconstruction (Ticket 3)
- Master position reconstruction (already delegates to `ReconstructMasterPositionFromBrackets()`)
- FSM hydration (Ticket 5)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-51/01-analysis.md`](docs/brain/EPIC-CCN-51/01-analysis.md) -- Section "Phase 3: Master Order Adoption" (lines 72-83)
- Approach: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 2 "Sub-Methods to Create" (lines 172-191)
- Sentinel Mitigations: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 6.1, 6.6

## Implementation Instructions

### Method Signature (from Approach Section 2, lines 175-179)
```csharp
/// <summary>
/// Adopts working orders from the master account into tracking dictionaries.
/// ACTOR-SERIALIZED: Must be called on strategy thread (via EnumerateApexAccounts).
/// THREAD-SAFETY: Single-write operations to ConcurrentDictionary are safe.
/// ORDERING: Must execute AFTER fleet adoption (Phase 1) and BEFORE FSM hydration (Phase 5).
/// </summary>
/// <returns>Count of orders adopted (for logging)</returns>
private int AdoptMasterOrders()
```

### Extraction Steps

1. **Create new method** `AdoptMasterOrders()` in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) immediately after `RebuildFleetPositionFromEntry()`

2. **Extract master adoption logic** (preserve exact logic from lines 452-480):
   ```csharp
   private int AdoptMasterOrders()
   {
       int adoptedCount = 0;
       
       // Single account loop (master account only)
       foreach (Order ord in masterBroker996h.Orders.ToArray())
       {
           // State guard (includes master unknown state)
           if (!IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true))
               continue;
           
           // Use shared classification helper (eliminates duplication)
           string classification = ClassifyOrderByPrefix(ord.Name);
           if (classification == null) continue; // Skip unrecognized orders
           
           // Build dictionary key
           string key = $"{ord.Account.Name}_{ord.Instrument.FullName}";
           
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
       }
       
       return adoptedCount;
   }
   ```

3. **Update residual method** [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309):
   - Replace lines 452-480 with: `adoptedCount += AdoptMasterOrders();`
   - Preserve sequential execution order (after fleet adoption and position rebuild)

### V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (target ~30 lines)
- [ ] Residual method CYC target: 40 → 25 (15-point reduction)
- [ ] No logic drift -- pure structural movement only
- [ ] State guard logic unchanged (includeMasterUnknown: true preserved)

## Sentinel Audit Mitigations (Reference Section 6 of Approach)

### Gap 6.1: Concurrent Dictionary Mutations (CRITICAL)
**Mitigation**: Add Actor serialization comment to method signature (see above). All dictionary writes remain single-write operations (`dict[key] = value`) which are thread-safe for `ConcurrentDictionary`. Method is called on strategy thread via [`EnumerateApexAccounts()`](src/V12_002.SIMA.Lifecycle.cs:196), preserving Actor serialization boundary.

**Verification**: 
- Grep audit: `grep -n "entryOrders\[" src/V12_002.SIMA.Lifecycle.cs` - verify all writes remain single-write
- Live session test: Trigger reconnect during active trading, verify no dictionary corruption

### Gap 6.6: Lock-Free Guarantee (DNA VIOLATION)
**Mitigation**: Add Actor serialization comment to method signature. Method uses direct writes (`dict[key] = value`), NOT `Enqueue()` calls. This is correct - entire adoption sequence runs on strategy thread.

**Verification**:
- Lock audit: `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` - verify ZERO matches
- Actor serialization audit: Trace call chain `OnStateChange` → [`EnumerateApexAccounts()`](src/V12_002.SIMA.Lifecycle.cs:196) → [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) → `AdoptMasterOrders()` - verify all synchronous

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
- [ ] `AdoptMasterOrders()` method created in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs)
- [ ] Method uses `masterBroker996h.Orders.ToArray()` snapshot
- [ ] Method uses shared `ClassifyOrderByPrefix()` helper (eliminates duplication)
- [ ] Method uses `IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true)` state guard
- [ ] Original method [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) reduced to CYC 25 (from 40)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC: 40 → 25 (15-point reduction)
- [ ] lock() audit: ZERO matches
- [ ] Actor serialization comment present in method signature
- [ ] Switch statement matches fleet adoption logic (same dictionary routing)
- [ ] State guard preserves `includeMasterUnknown: true` parameter
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Critical Verification Tests

### Test Case 1: Master Account Order Adoption
```csharp
// Given: masterBroker996h has 5 orders (Stop_, T1_, T2_, T3_, Fleet_)
// Expected: All 5 orders adopted to correct dictionaries
// Verify: stopOrders.Count++, target1Orders.Count++, etc.
```

### Test Case 2: State Guard with includeMasterUnknown
```csharp
// Given: Order with OrderState.Unknown (master account)
// Expected: Order is adopted (includeMasterUnknown: true)
// Contrast: Fleet orders with Unknown state are skipped (includeMasterUnknown: false)
```

### Test Case 3: Shared Helper Usage
```csharp
// Given: Order with name "T1_AAPL_123"
// Expected: ClassifyOrderByPrefix() returns "target1"
// Verify: Order routed to target1Orders dictionary
// Verify: Same logic as fleet adoption (no duplication)
```

### Test Case 4: Unrecognized Prefix Handling
```csharp
// Given: Order with name "Unknown_AAPL_123"
// Expected: ClassifyOrderByPrefix() returns null
// Verify: Order is skipped (continue statement)
// Verify: adoptedCount NOT incremented
```

### Test Case 5: Dictionary Key Format
```csharp
// Given: Order for account "Master996h", instrument "ES 03-25"
// Expected: Dictionary key = "Master996h_ES 03-25"
// Verify: Key format matches fleet adoption logic