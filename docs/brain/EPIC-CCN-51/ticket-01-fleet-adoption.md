---
# TICKET EPIC-CCN-51-01: Fleet Order Adoption
# Epic: EPIC-CCN-51
# Sequence: 1 of 5
# Depends on: NONE
---

## Objective
Extract fleet order adoption logic from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) into a new method `AdoptFleetOrders()` to achieve immediate 25-point CYC reduction (79 → 54).

## Scope
IN scope:
- Extract lines 313-408 from [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)
- Create new private method `AdoptFleetOrders()` in same file
- Preserve nested loop structure: `foreach Account` → `foreach Order`
- Preserve 8-way prefix classification logic (Stop_, S_, T1-T5_, Fleet_)
- Preserve try/catch per account for error isolation
- Apply FREEZE-PROOF pattern: `Account.All.ToArray()` snapshot

OUT of scope:
- Prefix classification helper (Ticket 2)
- Position reconstruction logic (Ticket 3)
- Master order adoption (Ticket 4)
- FSM hydration (Ticket 5)
- Any logic changes to classification or routing

## Context References
- Analysis: [`docs/brain/EPIC-CCN-51/01-analysis.md`](docs/brain/EPIC-CCN-51/01-analysis.md) -- Section "Phase 1: Fleet Order Adoption" (lines 34-55)
- Approach: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 2 "Sub-Methods to Create" (lines 129-148)
- Sentinel Mitigations: [`docs/brain/EPIC-CCN-51/02-approach.md`](docs/brain/EPIC-CCN-51/02-approach.md) -- Section 6.1, 6.5, 6.6, 6.7

## Implementation Instructions

### Method Signature (from Approach Section 2, lines 132-135)
```csharp
/// <summary>
/// Adopts working orders from all fleet accounts into tracking dictionaries.
/// ACTOR-SERIALIZED: Must be called on strategy thread (via EnumerateApexAccounts).
/// THREAD-SAFETY: Single-write operations to ConcurrentDictionary are safe.
/// ORDERING: Must execute BEFORE FSM hydration reads these dictionaries.
/// LATENCY: Cold path (startup/reconnect only). Target <100ms for 50 accounts.
/// Jane Street bounded-latency principle applies to hot paths (per-tick), not cold paths.
/// If production latency exceeds 500ms, create EPIC-CCN-54: Latency Optimization.
/// </summary>
/// <returns>Count of orders adopted (for logging)</returns>
private int AdoptFleetOrders()
```

### Extraction Steps

1. **Create new method** `AdoptFleetOrders()` in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) immediately after [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)

2. **Apply FREEZE-PROOF pattern** (Sentinel Gap 6.5 mitigation):
   ```csharp
   // [FREEZE-PROOF] Snapshot Account.All to prevent InvalidOperationException
   // if broker reconnects or modifies the collection during iteration.
   // Pattern verified in V12_002.SIMA.Flatten.cs line 51 and V12_002.SIMA.Fleet.cs line 489.
   Account[] accountSnapshot = Account.All.ToArray();
   int adoptedCount = 0;
   
   foreach (Account acct in accountSnapshot)  // ← Use snapshot, NOT Account.All
   {
       if (!IsFleetAccount(acct)) continue;
       
       try
       {
           foreach (Order ord in acct.Orders.ToArray())  // ← Also snapshot Orders
           {
               // ... existing classification and routing logic from lines 336-380
           }
       }
       catch (Exception ex)
       {
           Print($"[HYDRATE-ERROR] Fleet adoption failed for {acct.Name}: {ex.Message}");
       }
   }
   
   return adoptedCount;
   ```

3. **Move classification logic** from lines 336-380 into the inner loop (preserve exact logic):
   - 8-way if/else chain for prefix classification
   - State guard: `IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: false)`
   - Dictionary routing: `entryOrders[key] = ord`, `stopOrders[key] = ord`, `target1-5Orders[key] = ord`
   - Increment `adoptedCount` for each adopted order

4. **Update residual method** [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309):
   - Replace lines 313-408 with: `int adoptedCount = AdoptFleetOrders();`
   - Preserve all other phases (position rebuild, master adoption, FSM hydration)

### V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (target ~80 lines)
- [ ] Residual method CYC target: 79 → 54 (25-point reduction)
- [ ] No logic drift -- pure structural movement only
- [ ] FREEZE-PROOF pattern applied (`Account.All.ToArray()`)

## Sentinel Audit Mitigations (Reference Section 6 of Approach)

### Gap 6.1: Concurrent Dictionary Mutations (CRITICAL)
**Mitigation**: Add Actor serialization comment to method signature (see above). All dictionary writes remain single-write operations (`dict[key] = value`) which are thread-safe for `ConcurrentDictionary`. Method is called on strategy thread via [`EnumerateApexAccounts()`](src/V12_002.SIMA.Lifecycle.cs:196), preserving Actor serialization boundary.

**Verification**: 
- Grep audit: `grep -n "entryOrders\[" src/V12_002.SIMA.Lifecycle.cs` - verify all writes remain single-write
- Live session test: Trigger reconnect during active trading, verify no dictionary corruption

### Gap 6.5: Unbounded Loop Latency (HIGH-RISK)
**Mitigation**: Apply FREEZE-PROOF pattern using `Account.All.ToArray()` snapshot (exact placement specified in extraction steps above). Document latency budget in method comment.

**Verification**:
- Code review: Verify `Account.All.ToArray()` is used (not direct `Account.All`)
- Latency benchmark: Measure execution time with 10, 50, and 100 accounts (targets: <20ms, <100ms, <500ms)
- Collection modification test: Trigger broker reconnect during execution, verify no `InvalidOperationException`

### Gap 6.6: Lock-Free Guarantee (DNA VIOLATION)
**Mitigation**: Add Actor serialization comment to method signature. Method uses direct writes (`dict[key] = value`), NOT `Enqueue()` calls. This is correct - entire adoption sequence runs on strategy thread.

**Verification**:
- Lock audit: `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` - verify ZERO matches
- Actor serialization audit: Trace call chain `OnStateChange` → [`EnumerateApexAccounts()`](src/V12_002.SIMA.Lifecycle.cs:196) → [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) → `AdoptFleetOrders()` - verify all synchronous

### Gap 6.7: Bounded-Latency Principle (DNA VIOLATION)
**Mitigation**: Document latency budget in method comment (see signature above). This is a COLD PATH (startup/reconnect only), not a HOT PATH (per-tick). Jane Street bounded-latency principle applies to hot paths. Acceptable deviation for cold paths.

**Verification**:
- Latency benchmark: Measure with varying account counts (see Gap 6.5)
- Production monitoring: After deployment, monitor reconnect latency in logs

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
- [ ] `AdoptFleetOrders()` method created in [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs)
- [ ] Method uses `Account.All.ToArray()` FREEZE-PROOF pattern
- [ ] Method uses `acct.Orders.ToArray()` snapshot for inner loop
- [ ] Original method [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309) reduced to CYC 54 (from 79)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC: 79 → 54 (25-point reduction)
- [ ] lock() audit: ZERO matches
- [ ] FREEZE-PROOF comment present in method body
- [ ] Actor serialization comment present in method signature
- [ ] Latency budget comment present in method signature
- [ ] Try/catch per account preserved (error isolation)
- [ ] 8-way prefix classification logic unchanged (pure structural movement)
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible