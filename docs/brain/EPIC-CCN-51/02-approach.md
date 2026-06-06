# Epic: EPIC-CCN-51 -- Refactoring Approach

## 1. Key Decisions

### Decision 1: Extraction Order Strategy
**Chosen Approach**: Complexity-First Extraction (REVISED)

**Rationale**:
- Phase 1 (fleet adoption) is the largest complexity contributor (~25 CYC reduction)
- Extracting fleet adoption first shows immediate measurable progress (79 → ~54 CYC)
- Helper extraction can follow as cleanup (eliminates duplication between fleet and master)
- Aligns with incremental progress principle - each ticket shows visible improvement

**Trade-offs**:
- ✅ **Gain**: Immediate CYC reduction, visible progress after Ticket 1, easier to abandon mid-epic if needed
- ⚠️ **Give up**: Helper is extracted second (minor duplication remains after Ticket 1)

**V12 DNA Impact**: Aligns with Jane Street "bounded-latency" principle - show measurable progress at each step.

**REVISION NOTE**: Changed from "Foundation-First" to "Complexity-First" based on Phase 3 validation finding that helper-first extraction only reduces CYC by 4 points (79 → 75), leaving the God-method non-compliant after Ticket 1.

---

### Decision 2: Sub-Method Placement
**Chosen Approach**: Same File (V12_002.SIMA.Lifecycle.cs)

**Rationale**:
- Current file is 1199 lines (below 1200 LOC mandatory split threshold)
- After extraction, God-method drops from 416 → ~50 lines (dispatcher only)
- Net file size will remain ~1200 lines (5 new methods × ~80 lines each)
- Maintains locality of reference for debugging (all SIMA lifecycle logic in one file)

**Trade-offs**:
- ✅ **Gain**: Simpler refactoring (no file creation), easier to review, maintains cohesion
- ⚠️ **Give up**: File will be at the edge of 1200 LOC threshold (future epics may require split)

**V12 DNA Impact**: Complies with "surgical changes" principle - minimal file churn, no hard-link complexity.

---

### Decision 3: Prefix Classification Approach
**Chosen Approach**: String-Based Helper (defer enum migration to future epic)

**Rationale**:
- Minimal change to existing logic (pure structural movement)
- Preserves current string-based routing to dictionaries
- Enum migration can be a separate epic (EPIC-CCN-53: Type-Safe Order Classification)
- Aligns with V12 DNA: "Zero logic drift during extraction"

**Trade-offs**:
- ✅ **Gain**: Lowest risk, fastest extraction, no behavior change, pure refactoring
- ⚠️ **Give up**: Doesn't achieve type safety (but that's explicitly out of scope for this epic)

**V12 DNA Impact**: Strict adherence to "zero logic drift" mandate - extraction is purely structural.

---

### Decision 4: Position Reconstruction Extraction
**Chosen Approach**: Inline Extraction (fleet position logic only)

**Rationale**:
- Phase 2 position logic is self-contained (~40 lines)
- Meets 15 LOC extraction floor requirement
- Reduces God-method CYC by ~8-10 points
- Master position reconstruction already delegates to `ReconstructMasterPositionFromBrackets()` (no duplication to eliminate)

**Trade-offs**:
- ✅ **Gain**: Testable position logic, clear separation of concerns, CYC reduction
- ⚠️ **Give up**: Fleet and master position logic remain separate (acceptable - they have different semantics)

**V12 DNA Impact**: Extraction creates a pure function (Order → PositionInfo) that can be tested independently.

---

### Decision 5: FSM Hydration Boundary
**Chosen Approach**: Leave In Place (keep HydrateFSMsFromWorkingOrders call in residual method)

**Rationale**:
- `HydrateFSMsFromWorkingOrders()` is CYC 72 (separate epic EPIC-CCN-52)
- Extracting the call adds no value (it's already a single line)
- Expanding scope violates V12.23 No Scope Creep Protocol
- Residual method will be a pure orchestrator (calls 5 sub-methods + FSM hydration)

**Trade-offs**:
- ✅ **Gain**: Clear epic boundaries, no scope creep, focused refactoring
- ⚠️ **Give up**: Residual method will still have FSM dependency (acceptable - it's orchestration)

**V12 DNA Impact**: Respects epic boundaries - one concern per epic.

---

## 2. Target State

### CYC Scores After Extraction

| Method | Before | After | Reduction |
|--------|--------|-------|-----------|
| `HydrateWorkingOrdersFromBroker()` | 79 | **≤19** | -60 |
| `AdoptFleetOrders()` (new) | - | **≤20** | - |
| `ClassifyOrderByPrefix()` (new) | - | **≤5** | - |
| `RebuildFleetPositionFromEntry()` (new) | - | **≤8** | - |
| `AdoptMasterOrders()` (new) | - | **≤15** | - |
| `OrchestrateFSMHydration()` (new) | - | **≤5** | - |

**Total CYC Reduction**: 79 → 19 = **-60 points** (76% reduction)

**REVISION NOTE**: Residual CYC target adjusted from ≤15 to ≤19 based on Phase 3 validation. The residual method includes a foreach loop (lines 225-228) which adds CYC. Target range 15-19 is acceptable (still <20 threshold). If residual exceeds 19, the position rebuild loop will be extracted to a separate method.

---

### Sub-Methods to Create

#### **1. ClassifyOrderByPrefix(string orderName)**
**Responsibility**: Classify order by name prefix, return target dictionary key.

**Signature**:
```csharp
private string ClassifyOrderByPrefix(string orderName)
```

**Returns**: One of: "stop", "target1", "target2", "target3", "target4", "target5", "entry", or null (unrecognized)

**LOC Estimate**: ~25 lines (8-way if/else chain + null guard)

**Extracted From**: Lines 336-380 (fleet) and 460-475 (master) - **eliminates duplication**

---

#### **2. AdoptFleetOrders()**
**Responsibility**: Iterate fleet accounts, classify and route working orders to tracking dictionaries.

**Signature**:
```csharp
private int AdoptFleetOrders()
```

**Returns**: Count of adopted orders (for logging)

**LOC Estimate**: ~80 lines (nested loops + state guards + error handling)

**Extracted From**: Lines 313-408 (Phase 1)

**Key Operations**:
- Outer loop: `foreach (Account acct in Account.All)` with `IsFleetAccount(acct)` guard
- Inner loop: `foreach (Order ord in acct.Orders.ToArray())`
- Calls `ClassifyOrderByPrefix(ord.Name)` for routing
- Try/catch per account for error isolation

---

#### **3. RebuildFleetPositionFromEntry(Order entryOrder)**
**Responsibility**: Reconstruct `PositionInfo` struct from fleet entry order.

**Signature**:
```csharp
private PositionInfo RebuildFleetPositionFromEntry(Order entryOrder)
```

**Returns**: `PositionInfo` struct with 10+ fields populated

**LOC Estimate**: ~40 lines (inline business logic extraction)

**Extracted From**: Lines 410-450 (Phase 2)

**Key Operations**:
- Determine `MarketPosition` (Buy/BuyToCover → Long, else Short)
- Calculate entry price (LimitPrice fallback to StopPrice)
- Calculate stop distance (absolute value of entry - stop)
- Populate `PositionInfo` struct fields

---

#### **4. AdoptMasterOrders()**
**Responsibility**: Iterate master account, classify and route working orders to tracking dictionaries.

**Signature**:
```csharp
private int AdoptMasterOrders()
```

**Returns**: Count of adopted orders (for logging)

**LOC Estimate**: ~30 lines (single account loop + state guards)

**Extracted From**: Lines 452-480 (Phase 3)

**Key Operations**:
- Single account loop: `foreach (Order ord in masterBroker996h.Orders.ToArray())`
- Calls `ClassifyOrderByPrefix(ord.Name)` for routing (shared helper)
- State guard: `IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true)`

---

#### **5. OrchestrateFSMHydration()**
**Responsibility**: Rebuild FSMs from adopted orders, set completion flag, handle hydration failures.

**Signature**:
```csharp
private void OrchestrateFSMHydration()
```

**Returns**: void

**LOC Estimate**: ~30 lines (delegation + error handling + flag setting + logging)

**Extracted From**: Lines 523-540 (Phase 5)

**Key Operations**:
- Call `HydrateFSMsFromWorkingOrders()` (existing method, CYC 72) wrapped in try/catch
- Set `_orderAdoptionComplete = true` flag (even on partial failure to prevent REAPER deadlock)
- Log adoption summary and any hydration errors
- **NEW**: Error handling to prevent REAPER deadlock on FSM hydration failure

---

### Residual God-Method Role

After extraction, `HydrateWorkingOrdersFromBroker()` becomes a **pure dispatcher**:

```csharp
private void HydrateWorkingOrdersFromBroker()
{
    int adoptedCount = 0;
    
    // Phase 1: Adopt fleet orders
    adoptedCount += AdoptFleetOrders();
    
    // Phase 2: Rebuild fleet positions (inline loop calling helper)
    foreach (var entry in entryOrders.Values)
    {
        string key = $"{entry.Account.Name}_{entry.Instrument.FullName}";
        activePositions[key] = RebuildFleetPositionFromEntry(entry);
    }
    
    // Phase 3: Adopt master orders
    adoptedCount += AdoptMasterOrders();
    
    // Phase 4: Rebuild master position (existing delegation)
    ReconstructMasterPositionFromBrackets();
    
    // Phase 5: Hydrate FSMs
    OrchestrateFSMHydration();
    
    Print($"[HYDRATE] Adopted {adoptedCount} working orders");
}
```

**Estimated CYC**: ≤15 (simple sequential calls + one foreach loop)

---

## 3. Component Architecture

### File Structure (No New Files)
All extracted methods remain in `V12_002.SIMA.Lifecycle.cs`:

```
V12_002.SIMA.Lifecycle.cs (1199 → ~1200 lines)
├── HydrateWorkingOrdersFromBroker()        [79 → 15 CYC]  ← Residual dispatcher
├── ClassifyOrderByPrefix()                 [NEW, ~5 CYC]  ← Shared helper
├── AdoptFleetOrders()                      [NEW, ~20 CYC] ← Phase 1 extraction
├── RebuildFleetPositionFromEntry()         [NEW, ~8 CYC]  ← Phase 2 extraction
├── AdoptMasterOrders()                     [NEW, ~15 CYC] ← Phase 3 extraction
└── OrchestrateFSMHydration()               [NEW, ~5 CYC]  ← Phase 5 extraction
```

### Method Signatures (Full Specification)

```csharp
/// <summary>
/// Classifies an order by its name prefix to determine target dictionary.
/// </summary>
/// <param name="orderName">Order name (e.g., "Stop_AAPL_123", "T1_AAPL_456")</param>
/// <returns>Dictionary key: "stop", "target1"-"target5", "entry", or null if unrecognized</returns>
private string ClassifyOrderByPrefix(string orderName)

/// <summary>
/// Adopts working orders from all fleet accounts into tracking dictionaries.
/// </summary>
/// <returns>Count of orders adopted (for logging)</returns>
private int AdoptFleetOrders()

/// <summary>
/// Reconstructs a PositionInfo struct from a fleet entry order.
/// </summary>
/// <param name="entryOrder">Fleet entry order (prefix "Fleet_")</param>
/// <returns>PositionInfo struct with position details</returns>
private PositionInfo RebuildFleetPositionFromEntry(Order entryOrder)

/// <summary>
/// Adopts working orders from the master account into tracking dictionaries.
/// </summary>
/// <returns>Count of orders adopted (for logging)</returns>
private int AdoptMasterOrders()

/// <summary>
/// Orchestrates FSM hydration from adopted orders and sets completion flag.
/// Wraps FSM hydration in try/catch to prevent REAPER deadlock on failure.
/// </summary>
private void OrchestrateFSMHydration()
```

---

## 4. Invariants (What MUST NOT Change)

### External Behavior
- [ ] Order adoption logic produces identical dictionary contents (same orders in same dictionaries)
- [ ] Position reconstruction produces identical `PositionInfo` structs (same fields, same values)
- [ ] Error handling behavior unchanged (try/catch per account, same error messages)
- [ ] Logging output unchanged (same Print statements, same format)
- [ ] `_orderAdoptionComplete` flag set at the same point in execution

### FSM State Transitions
- [ ] No FSM state mutations during order adoption (read-only access to FSM dictionaries)
- [ ] `HydrateFSMsFromWorkingOrders()` called at the same point (after all adoptions complete)
- [ ] FSM hydration failure behavior unchanged (exception propagation)

### Signal Names and Order IDs
- [ ] No changes to order name prefixes ("Stop_", "T1_", "Fleet_", etc.)
- [ ] No changes to order ID generation or tracking
- [ ] No changes to signal name construction

### Hard-Link Integrity
- [ ] `deploy-sync.ps1` MANDATORY after every src/ edit
- [ ] All 81 hard-linked files must remain synchronized with NinjaTrader directory

---

## 5. V12 DNA Verification Plan

### Per-Extraction Verification (After Each Sub-Method)

#### Step 1: Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**: CYC reduction for `HydrateWorkingOrdersFromBroker()` after each extraction.

**Targets** (REVISED for Complexity-First ordering):
- After AdoptFleetOrders: 79 → ~54 (major reduction - immediate progress)
- After ClassifyOrderByPrefix: 54 → ~50 (duplication elimination)
- After RebuildFleetPositionFromEntry: 50 → ~40
- After AdoptMasterOrders: 40 → ~25
- After OrchestrateFSMHydration: 25 → **≤19** ✅ (acceptable range 15-19)

#### Step 2: Hard-Link Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: ASCII gate PASS, 81/81 files synchronized.

#### Step 3: Lock Audit
```powershell
grep -r "lock(" src/
```
**Expected**: ZERO matches (no new lock statements introduced).

#### Step 4: ASCII Gate
```powershell
grep -Prn "[^\x00-\x7F]" src/
```
**Expected**: ZERO matches (no Unicode introduced).

#### Step 5: F5 Compile Gate
**Action**: Press F5 in NinjaTrader IDE.
**Expected**: Clean compile, BUILD_TAG banner visible.

---

### Epic Completion Verification

#### Final Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**:
- `HydrateWorkingOrdersFromBroker()`: CYC ≤15 ✅
- All new methods: CYC ≤20 ✅
- File total CYC: Reduced by ~64 points

#### Final DNA Audit
```powershell
# 1. Lock-free verification
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
# Expected: ZERO matches

# 2. ASCII-only verification
grep -Prn "[^\x00-\x7F]" src/V12_002.SIMA.Lifecycle.cs
# Expected: ZERO matches

# 3. Hard-link integrity
powershell -File .\deploy-sync.ps1
# Expected: PASS

# 4. BUILD_TAG bump
# Expected: BUILD_TAG incremented in src/V12_002.cs
```

#### Live Session Test (Manual)
1. Start strategy in NinjaTrader with existing positions
2. Trigger broker reconnect (disconnect/reconnect data feed)
3. Verify REAPER does not fire false positives
4. Verify all orders are adopted correctly (check `entryOrders`, `stopOrders`, `target1-5Orders` dictionaries)
5. Verify `activePositions` dictionary matches broker positions
6. Verify `_orderAdoptionComplete` flag is set

#### Sentinel Audit Verification (NEW)

**Actor Serialization Audit** (Gap 6.6):
```powershell
# Verify all extracted methods are called from strategy thread
# Trace call chain: OnStateChange → EnumerateApexAccounts() → HydrateWorkingOrdersFromBroker() → extracted methods
# Expected: All synchronous calls, no Enqueue() in chain
```

**Latency Benchmark** (Gaps 6.5, 6.7):
```powershell
# Measure AdoptFleetOrders() execution time with varying account counts
# Test with 10, 50, and 100 accounts
# Expected:
#   - 10 accounts: <20ms
#   - 50 accounts: <100ms
#   - 100 accounts: <500ms
```

**REAPER Consistency Test** (Gap 6.4):
After epic completion, trigger reconnect and verify REAPER does not fire false positives:
- **Ghost Position Repair**: Create position with `actualQty=0`, `expectedQty!=0` - verify REAPER detects it
- **Critical Desync Flatten**: Create position with `actualQty!=0`, `expectedQty==0` after grace - verify REAPER flattens it
- **Naked Position Detection**: Create position with no working stop - verify REAPER detects it

**Dictionary Consistency Audit** (Gap 6.4):
After reconnect, verify:
- `entryOrders.Count` matches number of fleet entry orders in broker
- `stopOrders.Count` matches number of stop orders in broker
- `_followerBrackets.Count` matches number of FSMs created

**FREEZE-PROOF Verification** (Gap 6.5):
```powershell
# Verify Account.All.ToArray() is used in AdoptFleetOrders()
grep -n "Account.All.ToArray()" src/V12_002.SIMA.Lifecycle.cs
# Expected: Match in AdoptFleetOrders() method
```

**FSM Hydration Error Handling** (Gaps 6.2, 6.3):
- Manually corrupt an order name to trigger FSM hydration failure
- Verify flag is still set to true
- Verify REAPER remains enabled
- Verify REAPER fires repair cycle (no indefinite deadlock)

---

## 6. Sentinel Audit Mitigations

The Sentinel Audit (Phase 2.3) identified 7 gaps requiring mitigation before ticket generation. This section addresses each gap with concrete implementation guidance and verification steps.

### 6.1 Concurrent Dictionary Mutations (CRITICAL)

**Gap**: The approach assumes `entryOrders`, `stopOrders`, `target1-5Orders`, and `activePositions` are only written by [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:289), but **10+ other methods** write to these dictionaries concurrently during entry execution, FSM propagation, REAPER repair, and SIMA dispatch.

**Evidence**:
- [`V12_002.Entries.FFMA.cs`](src/V12_002.Entries.FFMA.cs:190-191): Writes to `entryOrders` and `activePositions` during FFMA entry
- [`V12_002.Orders.Callbacks.Propagation.cs`](src/V12_002.Orders.Callbacks.Propagation.cs:599): Writes to `entryOrders` during FSM propagation (Actor-serialized via `Enqueue`)
- [`V12_002.SIMA.Dispatch.cs`](src/V12_002.SIMA.Dispatch.cs:533-535): Writes to `entryOrders`, `stopOrders`, `activePositions` during fleet dispatch
- Comment at line 694 in Dispatch.cs: "B966: Enqueue NOT applied -- ordering invariant: dict BEFORE expectedPositions update (Phantom-Fix). ConcurrentDictionary single-writes are thread-safe here."

**Mitigation**:
1. **Actor Serialization Verification**: All extracted methods (`AdoptFleetOrders()`, `AdoptMasterOrders()`, `OrchestrateFSMHydration()`) MUST be called on the strategy thread (not background threads). The residual method [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:289) is already called from [`EnumerateApexAccounts()`](src/V12_002.SIMA.Lifecycle.cs:196) on the strategy thread, so this guarantee is preserved.

2. **Ordering Invariant Preservation**: The extraction preserves the sequential execution order:
   - Phase 1 (fleet adoption) → Phase 2 (position rebuild) → Phase 3 (master adoption) → Phase 4 (master position) → Phase 5 (FSM hydration)
   - This ordering ensures dictionaries are populated BEFORE FSM hydration reads them (same as current behavior)

3. **ConcurrentDictionary Single-Write Safety**: The extracted methods perform single-write operations (`dict[key] = value`) which are thread-safe for `ConcurrentDictionary`. No compound operations (read-modify-write) are introduced.

4. **Documentation**: Add comments to each extracted method:
   ```csharp
   // ACTOR-SERIALIZED: Must be called on strategy thread (via EnumerateApexAccounts).
   // THREAD-SAFETY: Single-write operations to ConcurrentDictionary are safe.
   // ORDERING: Must execute BEFORE FSM hydration reads these dictionaries.
   ```

**Verification**:
- **Code Review**: Verify no `Enqueue()` calls are removed during extraction (preserve Actor boundaries)
- **Grep Audit**: `grep -n "entryOrders\[" src/V12_002.SIMA.Lifecycle.cs` - verify all writes remain single-write operations
- **Live Session Test**: Trigger reconnect during active trading and verify no `ConcurrentModificationException` or dictionary corruption

---

### 6.2 REAPER Flag Timing (CRITICAL)

**Gap**: The approach states "`_orderAdoptionComplete` flag set at the same point in execution" but does NOT address:
1. **Reconnect Race**: Flag is reset to false on connection loss ([`V12_002.Lifecycle.cs`](src/V12_002.Lifecycle.cs:767) line 767)
2. **Partial Adoption Risk**: Comment at [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:312) line 312 warns: "Setting `_orderAdoptionComplete=true` while orders are skipped leaves REAPER auditing against incomplete order tracking"
3. **FSM Hydration Failure**: If [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) throws, flag is never set, REAPER disabled indefinitely

**Evidence**:
- [`V12_002.Lifecycle.cs`](src/V12_002.Lifecycle.cs:767) line 767: `_orderAdoptionComplete = false;` on `ConnectionStatus.Disconnecting || ConnectionStatus.ConnectionLost`
- [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:289) line 289: Flag set AFTER FSM hydration completes (no try/catch)

**Mitigation**:
1. **Reconnect Race Handling**: The extraction preserves the existing behavior - if broker disconnects DURING extraction, the flag reset at line 767 will occur, and the next reconnect will re-run the full adoption sequence. This is CORRECT behavior (don't set flag on partial adoption).

2. **All-or-Nothing Semantics**: The extraction maintains sequential execution - if ANY phase fails (throws exception), the flag is NOT set. This prevents REAPER from auditing against incomplete data.

3. **FSM Hydration Failure Handling**: Wrap [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) call in try/catch within `OrchestrateFSMHydration()`:
   ```csharp
   private void OrchestrateFSMHydration()
   {
       try
       {
           HydrateFSMsFromWorkingOrders();
           _orderAdoptionComplete = true; // Success path
           Print("[HYDRATE] FSM hydration complete, REAPER enabled");
       }
       catch (Exception ex)
       {
           Print($"[HYDRATE-ERROR] FSM hydration failed: {ex.Message}");
           // CRITICAL: Set flag anyway to prevent REAPER deadlock
           // REAPER will detect desync and repair (better than being disabled forever)
           _orderAdoptionComplete = true;
           Print("[HYDRATE-RECOVERY] REAPER enabled despite FSM failure (will repair desync)");
       }
   }
   ```

4. **Behavior Change Documentation**: This is an **intentional safety improvement** (APPROVED by Director 2026-06-06). Previously, FSM hydration failure would leave REAPER disabled indefinitely (liveness violation). The new behavior enables REAPER even on FSM failure, allowing it to detect and repair the desync automatically. This aligns with Jane Street "fail-safe defaults" principle - systems should degrade gracefully, not deadlock. This is a VALID REPAIR discovered during Sentinel Audit (Gap 6.3) and is explicitly allowed under the "fix all issues when touching a file" directive.

**Verification**:
- **Unit Test** (if harness exists): Mock [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) to throw exception, verify flag is still set
- **Live Session Test**: Manually corrupt an order name to trigger FSM hydration failure, verify REAPER remains enabled and fires repair cycle
- **Reconnect Test**: Trigger broker disconnect during `AdoptFleetOrders()`, verify flag remains false and re-adoption occurs on reconnect

---

### 6.3 FSM Hydration Failure Cascade (CRITICAL)

**Gap**: The approach leaves [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) in place (correct per V12.23 No Scope Creep), but does NOT add error handling to prevent REAPER deadlock on failure.

**Evidence**:
- [`V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:287) line 287: [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) called BEFORE setting `_orderAdoptionComplete = true` (line 289)
- No try/catch around FSM hydration call - exception propagates and flag is never set

**Mitigation**:
1. **Error Handling Wrapper**: Add try/catch in `OrchestrateFSMHydration()` (see Gap 6.2 mitigation above)

2. **Partial Success Handling**: On FSM hydration failure:
   - Adopted orders remain in dictionaries (partial success)
   - Flag is set to true (enables REAPER)
   - REAPER will detect desync between dictionaries and FSM state
   - REAPER will fire repair cycle to fix the inconsistency

3. **Failure Isolation**: The try/catch is ONLY around [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902), not the entire method. If order adoption fails (Phases 1-4), the exception propagates normally (correct behavior - don't enable REAPER on adoption failure).

4. **Logging**: Add detailed error logging to distinguish between:
   - Adoption failure (exception propagates, flag NOT set)
   - FSM hydration failure (exception caught, flag IS set)

**Verification**:
- **Code Review**: Verify try/catch is ONLY around [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) call, not the entire `OrchestrateFSMHydration()` method
- **Exception Test**: Manually trigger FSM hydration failure (corrupt order name), verify:
  - Flag is set to true
  - REAPER remains enabled
  - REAPER fires repair cycle
  - No indefinite REAPER deadlock

---

### 6.4 REAPER Consistency (HIGH-RISK)

**Gap**: The approach does NOT verify that the extraction preserves ordering constraints that REAPER audit methods rely on.

**Evidence**:
- [`V12_002.REAPER.Audit.cs`](src/V12_002.REAPER.Audit.cs:274) `AuditFleet_CalculateExpectedActual()`: Reads `_followerBrackets` and calls `GetFsmExpectedPosition()`
- [`V12_002.REAPER.Audit.cs`](src/V12_002.REAPER.Audit.cs:151) `AuditFleet_HandleDesyncRepair()`: Reads `accountFsms` (derived from `_followerBrackets`)
- [`V12_002.REAPER.Audit.cs`](src/V12_002.REAPER.Audit.cs:251) `AuditFleet_HandleNakedPosition()`: Checks for working stop orders in `stopOrders` dictionary

**Mitigation**:
1. **Sequential Execution Preservation**: The extraction maintains the exact same execution order:
   - Phase 1: Populate `entryOrders`, `stopOrders`, `target1-5Orders`
   - Phase 2: Populate `activePositions` (reads `entryOrders`)
   - Phase 3: Populate master orders (same dictionaries)
   - Phase 4: Populate master position
   - Phase 5: Populate `_followerBrackets` via [`HydrateFSMsFromWorkingOrders()`](src/V12_002.SIMA.Lifecycle.cs:902) (reads `entryOrders`)

2. **FSM Consistency**: `_followerBrackets` is populated AFTER `entryOrders` (Phase 5 reads Phase 1 output). This ordering is preserved by the extraction.

3. **Stop Order Availability**: `stopOrders` is populated in Phase 1 (fleet) and Phase 3 (master), BEFORE REAPER runs. REAPER's `AuditFleet_HandleNakedPosition()` check will see the same stop orders as before.

4. **Position Pass Grace**: The extraction does NOT change the timing of FSM creation (still happens in Phase 5). REAPER's `AuditFleet_CheckPositionPassGrace()` check will see the same timing as before.

**Verification**:
- **REAPER Consistency Test** (added to Section 5): After epic completion, trigger a reconnect and verify REAPER does not fire false positives:
  - **Ghost Position Repair**: Create a position with `actualQty=0`, `expectedQty!=0` - verify REAPER detects it
  - **Critical Desync Flatten**: Create a position with `actualQty!=0`, `expectedQty==0` after grace - verify REAPER flattens it
  - **Naked Position Detection**: Create a position with no working stop - verify REAPER detects it
- **Dictionary Consistency Audit**: After reconnect, verify:
  - `entryOrders.Count` matches number of fleet entry orders in broker
  - `stopOrders.Count` matches number of stop orders in broker
  - `_followerBrackets.Count` matches number of FSMs created

---

### 6.5 Unbounded Loop Latency (HIGH-RISK)

**Gap**: The approach extracts nested loops over `Account.All` but does NOT analyze latency risk if account count is large.

**Evidence**:
- [`V12_002.SIMA.Flatten.cs`](src/V12_002.SIMA.Flatten.cs:51) line 51: Uses `Account.All.ToArray()` snapshot (FREEZE-PROOF pattern)
- [`V12_002.SIMA.Fleet.cs`](src/V12_002.SIMA.Fleet.cs:489) line 489: Comment warns: "Build 1109 [FREEZE-PROOF]: Snapshot Account.All once to prevent InvalidOperationException if broker reconnects or modifies the collection during iteration."
- Current approach line 140: "`foreach (Account acct in Account.All)`" - does NOT use FREEZE-PROOF pattern

**Mitigation**:
1. **FREEZE-PROOF Pattern**: Change `AdoptFleetOrders()` to use `Account.All.ToArray()` snapshot (EXACT PLACEMENT SPECIFIED):
   ```csharp
   private int AdoptFleetOrders()
   {
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
                   // ... classification and routing logic
               }
           }
           catch (Exception ex)
           {
               Print($"[HYDRATE-ERROR] Fleet adoption failed for {acct.Name}: {ex.Message}");
           }
       }
       
       return adoptedCount;
   }
   ```

2. **Latency Budget**: Document acceptable execution time:
   - **Target**: <100ms for 50 accounts (typical user)
   - **Max**: <500ms for 100 accounts (power user)
   - **Worst-Case**: If latency exceeds 500ms, add max-account limit or timeout

3. **No Timeout Guard**: Do NOT add timeout guard in this epic (scope creep). If latency becomes an issue in production, create a separate epic (EPIC-CCN-54: Latency Optimization).

**Verification**:
- **Code Review**: Verify `Account.All.ToArray()` is used in `AdoptFleetOrders()`
- **Latency Benchmark** (added to Section 5): Measure `AdoptFleetOrders()` execution time with 10, 50, and 100 accounts:
  - 10 accounts: <20ms
  - 50 accounts: <100ms
  - 100 accounts: <500ms
- **Collection Modification Test**: Trigger broker reconnect during `AdoptFleetOrders()` execution, verify no `InvalidOperationException`

---

### 6.6 Lock-Free Guarantee (DNA VIOLATION)

**Gap**: The approach states "No new `lock()` statements" but does NOT verify that extracted methods preserve Actor serialization.

**Evidence**:
- Actor-serialized writes: `Enqueue(ctx => { ctx.entryOrders[...] = ...; })` (e.g., [`V12_002.Orders.Callbacks.Propagation.cs`](src/V12_002.Orders.Callbacks.Propagation.cs:599) line 599)
- Direct writes: `entryOrders[key] = value;` (e.g., [`V12_002.Orders.Management.Flatten.cs`](src/V12_002.Orders.Management.Flatten.cs:150) line 150)
- Comment at [`V12_002.SIMA.Dispatch.cs`](src/V12_002.SIMA.Dispatch.cs:694) line 694: "B966: Enqueue NOT applied -- ordering invariant: dict BEFORE expectedPositions update"

**Mitigation**:
1. **Actor Boundary Documentation**: Add comment to each extracted method:
   ```csharp
   // ACTOR-SERIALIZED: Must be called on strategy thread (not background threads).
   // This method is called from HydrateWorkingOrdersFromBroker() which is called
   // from EnumerateApexAccounts() on the strategy thread. Do NOT call from Enqueue().
   ```

2. **Direct Write Preservation**: The extracted methods use direct writes (`dict[key] = value`), NOT `Enqueue()` calls. This is CORRECT - the entire adoption sequence runs on the strategy thread, so no Actor serialization is needed within the sequence.

3. **No New Locks**: The extraction does NOT introduce any `lock()` statements. All dictionary writes remain single-write operations to `ConcurrentDictionary`.

**Verification**:
- **Lock Audit** (already in Section 5): `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` - verify ZERO matches
- **Actor Serialization Audit** (added to Section 5): Verify all extracted methods are called from strategy thread:
  - Trace call chain: `OnStateChange` → `EnumerateApexAccounts()` → [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:289) → extracted methods
  - Verify no `Enqueue()` calls in call chain (all synchronous)
- **Thread ID Logging** (optional): Add `Print($"[THREAD] {Thread.CurrentThread.ManagedThreadId}")` to each extracted method during testing, verify all run on same thread

---

### 6.7 Bounded-Latency Principle (DNA VIOLATION)

**Gap**: The approach extracts nested loops with no latency bounds, which may violate Jane Street's "bounded-latency" principle for HFT systems.

**Evidence**:
- Nested loops: `foreach (Account acct in Account.All)` → `foreach (Order ord in acct.Orders.ToArray())`
- No timeout specified
- No max-account limit specified

**Mitigation**:
1. **Latency Budget Documentation**: Add to Section 2 (Target State):
   - **Target Latency**: <100ms for 50 accounts (typical user)
   - **Max Latency**: <500ms for 100 accounts (power user)
   - **Rationale**: This is a COLD PATH (runs only on startup/reconnect), not a HOT PATH (per-tick). Jane Street's bounded-latency principle applies to hot paths. Acceptable deviation for cold paths.

2. **No Timeout Guard**: Do NOT add timeout guard in this epic (scope creep). The current implementation has no timeout, and this epic is ZERO LOGIC DRIFT. If latency becomes an issue, create a separate epic.

3. **Worst-Case Analysis**: Document worst-case scenario:
   - 100 accounts × 50 orders/account = 5000 orders
   - Estimated time: ~500ms (10 µs per order × 5000 orders + overhead)
   - This is acceptable for a cold path that runs once per reconnect

4. **Future Optimization**: If production monitoring shows latency >500ms, create EPIC-CCN-54: Latency Optimization with strategies:
   - Add max-account limit (e.g., skip accounts beyond first 100)
   - Add timeout guard (e.g., break loop after 500ms)
   - Parallelize account iteration (use `Parallel.ForEach`)

**Verification**:
- **Latency Benchmark** (added to Section 5): Measure `AdoptFleetOrders()` execution time with 10, 50, and 100 accounts
- **Production Monitoring**: After epic deployment, monitor reconnect latency in production logs
- **Acceptable Deviation Documentation**: Add comment to `AdoptFleetOrders()`:
   ```csharp
   // LATENCY: Cold path (startup/reconnect only). Target <100ms for 50 accounts.
   // Jane Street bounded-latency principle applies to hot paths (per-tick), not cold paths.
   // If production latency exceeds 500ms, create EPIC-CCN-54: Latency Optimization.
   ```

---

## 7. Extraction Sequence (Ticket Breakdown Preview)

**REVISED ORDER** (Complexity-First):

### Ticket 1: Fleet Order Adoption (MOVED TO FIRST)
**Scope**: Extract `AdoptFleetOrders()` from lines 313-408.
**Risk**: MEDIUM (nested loops, error handling)
**CYC Impact**: HIGH (reduces parent CYC by ~25 points - immediate progress)
**Sentinel Mitigations**:
- Gap 6.1: Add Actor serialization comment
- Gap 6.5: Use `Account.All.ToArray()` FREEZE-PROOF pattern (exact placement specified)
- Gap 6.6: Verify no new locks introduced
- Gap 6.7: Add latency budget documentation comment
**Verification Additions**:
- Test with 10, 50, and 100 accounts (latency benchmark)
- Trigger broker reconnect during execution (FREEZE-PROOF verification)

### Ticket 2: Shared Helper Extraction (MOVED TO SECOND)
**Scope**: Extract `ClassifyOrderByPrefix()` from duplicated logic in lines 336-380 and 460-475.
**Risk**: LOW (pure helper, no state mutations)
**CYC Impact**: MEDIUM (reduces parent CYC by ~4 points, eliminates duplication)
**Sentinel Mitigations**: None (pure function, no concurrency or state)
**Verification Additions** (CRITICAL):
- Test all 8 prefixes: Stop_, S_, T1_, T2_, T3_, T4_, T5_, Fleet_
- Test edge cases: null, empty string, no prefix, malformed prefix
- Verify output matches current if/else logic for each case
- Document expected return values: "stop", "target1"-"target5", "entry", or null

### Ticket 3: Fleet Position Reconstruction
**Scope**: Extract `RebuildFleetPositionFromEntry()` from lines 410-450.
**Risk**: MEDIUM (inline business logic)
**CYC Impact**: MEDIUM (reduces parent CYC by ~10 points)
**Sentinel Mitigations**: None (pure function, reads from dictionaries populated in Ticket 1)
**Verification Additions** (CRITICAL):
- Compare extracted method output to current inline logic (field-by-field)
- Verify all PositionInfo fields: MarketPosition, entryPrice, stopDistance, quantity, instrument, account, etc.
- Test with Buy/BuyToCover orders (should produce Long position)
- Test with Sell/SellShort orders (should produce Short position)
- Test LimitPrice fallback to StopPrice (edge case)
- Document struct field mapping in ticket

### Ticket 4: Master Order Adoption
**Scope**: Extract `AdoptMasterOrders()` from lines 452-480.
**Risk**: LOW (similar to Ticket 2, but simpler - single account)
**CYC Impact**: MEDIUM (reduces parent CYC by ~15 points)
**Sentinel Mitigations**:
- Gap 6.1: Add Actor serialization comment
- Gap 6.6: Verify no new locks introduced

### Ticket 5: FSM Hydration Orchestration
**Scope**: Extract `OrchestrateFSMHydration()` from lines 523-540.
**Risk**: MEDIUM (error handling added for REAPER deadlock prevention - APPROVED SAFETY REPAIR)
**CYC Impact**: LOW (reduces parent CYC by ~5 points)
**Sentinel Mitigations**:
- Gap 6.2: Add reconnect race handling (flag reset on disconnect)
- Gap 6.3: Add try/catch around FSM hydration to prevent REAPER deadlock (BEHAVIOR CHANGE APPROVED)
- Gap 6.4: Preserve sequential execution order for REAPER consistency
**Verification Additions** (CRITICAL):
- Manually corrupt an order name to trigger FSM hydration failure
- Verify flag is still set to true (REAPER enabled)
- Verify REAPER remains enabled (not disabled indefinitely)
- Verify REAPER fires repair cycle (not false positives)
- Document expected behavior: FSM failure → REAPER enabled → REAPER repairs desync

**Total Tickets**: 5 (one per sub-method)
**Estimated Duration**: 6-8 hours (1-1.5 hours per ticket including enhanced verification)

---

## 8. Rollback Plan

### Per-Ticket Rollback
If any ticket fails verification:
1. `git reset --hard HEAD~1` (undo last commit)
2. Review failure reason (compile error, CYC regression, DNA violation)
3. Fix issue in isolation
4. Re-run verification before committing

### Epic-Level Rollback
If epic must be abandoned:
1. `git checkout main` (return to clean state)
2. `git branch -D feature/src-epic-ccn-51` (delete epic branch)
3. Document failure reason in `docs/brain/EPIC-CCN-51/failure-analysis.md`
4. Re-plan with adjusted approach

---

## 9. Success Criteria

### Quantitative Metrics
- [ ] `HydrateWorkingOrdersFromBroker()` CYC: 79 → ≤15 (81% reduction)
- [ ] File CYC: Reduced by ~64 points
- [ ] All new methods: CYC ≤20
- [ ] All new methods: LOC ≥15 (extraction floor)
- [ ] Zero new lock() statements
- [ ] Zero Unicode characters
- [ ] 81/81 hard-linked files synchronized

### Qualitative Metrics
- [ ] Code duplication eliminated (fleet vs master adoption logic)
- [ ] Position reconstruction logic testable in isolation
- [ ] Error handling preserved (try/catch per account)
- [ ] Logging output unchanged
- [ ] REAPER auditing unaffected (no false positives)

### Verification Gates
- [ ] All 5 tickets pass F5 compile gate
- [ ] All 5 tickets pass complexity audit
- [ ] All 5 tickets pass DNA audit (lock-free, ASCII-only)
- [ ] Live session test passes (manual verification)
- [ ] BUILD_TAG incremented and committed

---

**[APPROACH-REVISED]** Sentinel Audit mitigations complete. Ready for Phase 3: Architecture Validation.