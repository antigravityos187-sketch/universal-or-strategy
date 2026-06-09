# EPIC-CCN-17: Phase 1 - Scope Definition

**Epic**: EPIC-CCN-17  
**Target**: [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713)  
**Date**: 2026-06-09  
**Phase**: 1 (Scope Definition)  
**Status**: COMPLETE

---

## Mission Brief

Reduce cyclomatic complexity of [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713) from **37 to ≤15** through surgical extraction of helper methods. This epic targets a **Rank #5 hotspot** with **2.5x Jane Street threshold violation**.

**Risk Level**: LOW (zero blast radius, private method)  
**Extraction Strategy**: Surgical (preserve all logic, zero drift)

---

## Scope Boundaries

### IN SCOPE ✅

#### 1. Order State Validation Logic
**Current Location**: Lines 733-740  
**Complexity**: 5-way OR condition (+4 CYC)

```csharp
// EXTRACT THIS
if (
    ord.OrderState != OrderState.Working
    && ord.OrderState != OrderState.Accepted
    && ord.OrderState != OrderState.Submitted
    && ord.OrderState != OrderState.ChangePending
    && ord.OrderState != OrderState.ChangeSubmitted
)
    continue;
```

**Target Helper**: `IsAdoptableOrderState(OrderState state)`  
**Signature**: `private bool IsAdoptableOrderState(OrderState state)`  
**Returns**: `true` if order should be adopted, `false` otherwise  
**CYC Reduction**: -4

---

#### 2. Dictionary Routing Logic
**Current Location**: Lines 748-791  
**Complexity**: 7-case switch (+6 CYC)

```csharp
// EXTRACT THIS
switch (classification)
{
    case "stop":
        targetDict = stopOrders;
        key = name.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
            ? name.Substring(5)
            : name.Substring(2);
        dictName = "stopOrders";
        break;
    case "target1":
        targetDict = target1Orders;
        key = name.Substring(3);
        dictName = "target1Orders";
        break;
    // ... 5 more cases
}
```

**Target Helper**: `GetTargetDictionary(string classification, string orderName, out string key, out string dictName)`  
**Signature**: `private ConcurrentDictionary<string, Order> GetTargetDictionary(string classification, string orderName, out string key, out string dictName)`  
**Returns**: Target dictionary reference, or `null` if classification invalid  
**CYC Reduction**: -6

---

#### 3. Position Rebuilding Logic
**Current Location**: Lines 795-829  
**Complexity**: Nested if/else with activePositions checks (+3 CYC)

```csharp
// EXTRACT THIS
if (targetDict == entryOrders && !activePositions.ContainsKey(key))
{
    PositionInfo pos = RebuildFleetPositionFromEntry(ord, key);
    activePositions[key] = pos;
    Print(string.Format("[SIMA HYDRATE] Rebuilt activePositions struct for {0} | DNA: IsMOMO={1} IsRMA={2} IsTREND={3} IsRetest={4}",
        key, pos.IsMOMOTrade, pos.IsRMATrade, pos.IsTRENDTrade, pos.IsRetestTrade));
}
else
{
    PositionInfo existingPos;
    if (activePositions.TryGetValue(key, out existingPos))
    {
        existingPos.TotalContracts = ord.Quantity;
        existingPos.ExecutingAccount = acct;
        Print(string.Format("[SIMA HYDRATE] Force-synced TotalContracts={0} ExecutingAccount={1} for {2}",
            ord.Quantity, acct.Name, key));
    }
}
```

**Target Helper**: `EnsurePositionTracking(Order ord, string key, Account acct, ConcurrentDictionary<string, Order> targetDict)`  
**Signature**: `private void EnsurePositionTracking(Order ord, string key, Account acct, ConcurrentDictionary<string, Order> targetDict)`  
**Side Effects**: Updates `activePositions` dictionary, calls `Print()`  
**CYC Reduction**: -3

---

#### 4. Account Order Processing Loop
**Current Location**: Lines 722-838  
**Complexity**: Nested foreach with multiple filters (+9 CYC)

```csharp
// EXTRACT THIS
foreach (Order ord in acct.Orders.ToArray())
{
    if (ord.Instrument?.FullName != Instrument?.FullName)
        continue;
    
    // [5-way validation]
    // [Classification]
    // [Dictionary routing]
    // [Position rebuilding]
    // [Logging]
}
```

**Target Helper**: `ProcessAccountOrders(Account acct, ref int adoptedCount)`  
**Signature**: `private void ProcessAccountOrders(Account acct, ref int adoptedCount)`  
**Side Effects**: Updates tracking dictionaries, increments `adoptedCount`  
**CYC Reduction**: -9

---

#### 5. Main Method Simplification
**Current Location**: Lines 713-848  
**Target State**: Orchestration-only (≤15 CYC)

```csharp
// TARGET STATE
private int AdoptFleetOrders()
{
    Account[] accountSnapshot = Account.All.ToArray();
    int adoptedCount = 0;

    foreach (Account acct in accountSnapshot)
    {
        if (!IsFleetAccount(acct))
            continue;
        
        try
        {
            ProcessAccountOrders(acct, ref adoptedCount);
        }
        catch (Exception ex)
        {
            Print(string.Format("[HYDRATE-ERROR] Fleet adoption failed for {0}: {1}", acct.Name, ex.Message));
        }
    }

    return adoptedCount;
}
```

**Final CYC**: ~8 (well under 15 threshold)

---

### OUT OF SCOPE ❌

#### 1. Existing Helper Methods
**DO NOT MODIFY**:
- [`ClassifyOrderByPrefix()`](src/V12_002.SIMA.Lifecycle.cs:993) - Already extracted, working correctly
- [`RebuildFleetPositionFromEntry()`](src/V12_002.SIMA.Lifecycle.cs:858) - Pure function, no changes needed
- [`IsFleetAccount()`](src/V12_002.SIMA.Fleet.cs) - External helper, preserve calls

**Rationale**: These methods already follow V12 DNA (pure functions, single responsibility). No refactoring needed.

---

#### 2. Account/Order Iteration Structure
**PRESERVE**:
```csharp
Account[] accountSnapshot = Account.All.ToArray();  // [FREEZE-PROOF] pattern
foreach (Account acct in accountSnapshot) { ... }
foreach (Order ord in acct.Orders.ToArray()) { ... }
```

**Rationale**: 
- Freeze-proof pattern verified in other files (V12_002.SIMA.Flatten.cs:51, V12_002.SIMA.Fleet.cs:489)
- Prevents `InvalidOperationException` during broker reconnects
- Thread-safe snapshot approach is V12 DNA

---

#### 3. Exception Handling Wrapper
**PRESERVE**:
```csharp
try
{
    // Order processing logic
}
catch (Exception ex)
{
    Print(string.Format("[HYDRATE-ERROR] Fleet adoption failed for {0}: {1}", acct.Name, ex.Message));
}
```

**Rationale**: 
- Prevents single account failure from cascading
- Logging pattern consistent with codebase
- No complexity reduction benefit from extraction

---

#### 4. Instrument Filter
**PRESERVE**:
```csharp
if (ord.Instrument?.FullName != Instrument?.FullName)
    continue;
```

**Rationale**: 
- Simple null-safe check (CYC +1 only)
- Extraction would add more complexity than it removes
- Clear intent as-is

---

#### 5. Logging Statements
**PRESERVE**:
```csharp
Print(string.Format("[SIMA HYDRATE] Adopted working order {0} into {1}", name, dictName));
Print(string.Format("[SIMA HYDRATE] Rebuilt activePositions struct for {0} | DNA: ...", key, ...));
```

**Rationale**: 
- Diagnostic logging is essential for production debugging
- No complexity contribution (simple statements)
- Consistent with V12 logging patterns

---

## Boundary Conditions

### Thread-Safety Requirements

**MANDATORY**: All extracted methods MUST maintain Actor model thread-safety:

1. **No new lock() statements** (V12 DNA violation)
2. **ConcurrentDictionary operations** are inherently thread-safe for single-write
3. **Actor-serialized execution** via `EnumerateApexAccounts()` ensures single-thread access
4. **No shared mutable state** between helper methods

**Verification**: Each ticket MUST include thread-safety audit in acceptance criteria.

---

### Signature Preservation

**MANDATORY**: All existing method signatures MUST remain unchanged:

- ✅ `private int AdoptFleetOrders()` - Preserve return type and parameters
- ✅ `private string ClassifyOrderByPrefix(string orderName)` - Do not modify
- ✅ `private PositionInfo RebuildFleetPositionFromEntry(Order entryOrder, string key)` - Do not modify

**Rationale**: Zero blast radius confirmed by jCodemunch. No external callers to update.

---

### Logic Drift Prevention

**MANDATORY**: Surgical extraction only - zero logic changes:

1. **Preserve all conditional logic** (no "while we're here" improvements)
2. **Preserve all string operations** (`Substring`, `StartsWith`, case-insensitive comparisons)
3. **Preserve all dictionary operations** (exact key extraction logic)
4. **Preserve all logging statements** (format strings, variable names)

**Verification**: Use `git diff` to confirm only structural changes (method boundaries), not logic changes.

---

### ASCII-Only Compliance

**MANDATORY**: All extracted methods MUST maintain ASCII-only strings:

- ✅ No Unicode characters in string literals
- ✅ No emoji in comments or logging
- ✅ No curly quotes (use straight quotes)

**Verification**: Run `scripts/ascii_audit.ps1` before each commit.

---

### Complexity Targets

**MANDATORY**: Each extracted method MUST meet Jane Street threshold:

| Method | Target CYC | Max Nesting | Max LOC |
|--------|-----------|-------------|---------|
| `IsAdoptableOrderState()` | ≤5 | ≤2 | <20 |
| `GetTargetDictionary()` | ≤8 | ≤3 | <40 |
| `EnsurePositionTracking()` | ≤6 | ≤3 | <30 |
| `ProcessAccountOrders()` | ≤12 | ≤4 | <60 |
| `AdoptFleetOrders()` (main) | ≤8 | ≤3 | <40 |

**Verification**: Run `python scripts/complexity_audit.py` after each extraction.

---

## Dependencies

### Internal Dependencies (Same File)

**Required for Extraction**:
- [`ClassifyOrderByPrefix()`](src/V12_002.SIMA.Lifecycle.cs:993) - Called by `ProcessAccountOrders()`
- [`RebuildFleetPositionFromEntry()`](src/V12_002.SIMA.Lifecycle.cs:858) - Called by `EnsurePositionTracking()`
- [`IsFleetAccount()`](src/V12_002.SIMA.Fleet.cs) - Called by `AdoptFleetOrders()` (main)

**State Dependencies**:
- `stopOrders`, `target1Orders`-`target5Orders`, `entryOrders` - ConcurrentDictionary fields
- `activePositions` - ConcurrentDictionary field
- `Instrument` - Strategy property (null-safe access)

---

### External Dependencies (Other Files)

**No External Callers**: Blast radius analysis confirms zero importers.

**NinjaTrader API Dependencies**:
- `Account.All` - Broker account collection
- `Order` - NinjaTrader order object
- `OrderState` - Enum (Working, Accepted, Submitted, ChangePending, ChangeSubmitted)
- `PositionInfo` - V12 struct (defined in same file)

---

## Estimated Ticket Breakdown

Based on extraction opportunities and complexity reduction targets:

### Ticket 1: Extract Order State Validation
**Target**: `IsAdoptableOrderState(OrderState state)`  
**CYC Reduction**: -4  
**Estimated Effort**: 1 hour  
**Risk**: LOW (pure function, no side effects)

---

### Ticket 2: Extract Dictionary Routing Logic
**Target**: `GetTargetDictionary(string classification, string orderName, out string key, out string dictName)`  
**CYC Reduction**: -6  
**Estimated Effort**: 2 hours  
**Risk**: MEDIUM (complex switch logic, multiple out parameters)

---

### Ticket 3: Extract Position Rebuilding Logic
**Target**: `EnsurePositionTracking(Order ord, string key, Account acct, ConcurrentDictionary<string, Order> targetDict)`  
**CYC Reduction**: -3  
**Estimated Effort**: 1.5 hours  
**Risk**: MEDIUM (side effects on activePositions, logging)

---

### Ticket 4: Extract Account Order Processing
**Target**: `ProcessAccountOrders(Account acct, ref int adoptedCount)`  
**CYC Reduction**: -9  
**Estimated Effort**: 2 hours  
**Risk**: MEDIUM (orchestrates other helpers, ref parameter)

---

### Ticket 5: Simplify Main Method
**Target**: `AdoptFleetOrders()` orchestration-only  
**CYC Reduction**: -9 (net result)  
**Estimated Effort**: 1 hour  
**Risk**: LOW (simple orchestration)

---

**Total Tickets**: 5  
**Total Effort**: 7.5 hours  
**Total CYC Reduction**: 37 → 8 (29-point reduction)

---

## Success Criteria

### Quantitative Metrics

- ✅ **CYC**: Reduced from 37 to ≤8 (main method)
- ✅ **Max Nesting**: Reduced from 8 to ≤3 (main method)
- ✅ **LOC**: Reduced from 136 to <40 (main method)
- ✅ **Helper Methods**: 4 new methods, each ≤15 CYC
- ✅ **Build**: Zero compilation errors
- ✅ **Tests**: All existing tests pass (no new tests required for refactor)

---

### Qualitative Criteria

- ✅ **Single Responsibility**: Each helper method does one thing
- ✅ **Pure Functions**: `IsAdoptableOrderState()` and `GetTargetDictionary()` have no side effects
- ✅ **Clear Names**: Method names self-document purpose
- ✅ **Thread-Safety**: Actor model preserved, no new locks
- ✅ **Logic Preservation**: Zero drift, surgical extraction only
- ✅ **ASCII Compliance**: No Unicode in string literals
- ✅ **Documentation**: XML comments for all new methods

---

### V12 DNA Compliance

- ✅ **Zero new lock() statements** (Actor model only)
- ✅ **ASCII-only string literals** (no Unicode)
- ✅ **≥15 LOC extraction floor** (all helpers meet threshold)
- ✅ **deploy-sync.ps1 after every src/ edit** (hard-link integrity)
- ✅ **complexity_audit.py before/after** (verify CYC reduction)

---

## Risk Assessment

### Overall Risk: LOW

**Justification**:
- ✅ Zero blast radius (no external callers)
- ✅ Private method (no API surface changes)
- ✅ Existing helper method pattern (proven approach)
- ✅ Clear extraction boundaries (no ambiguity)
- ✅ Surgical extraction (zero logic drift)

---

### Mitigation Strategies

1. **Logic Drift Prevention**:
   - Use `git diff` to verify only structural changes
   - Run `complexity_audit.py` before/after each ticket
   - Manual code review of each extraction

2. **Thread-Safety Verification**:
   - Audit each helper for lock-free compliance
   - Verify ConcurrentDictionary usage patterns
   - Test under concurrent load (if applicable)

3. **Build Integrity**:
   - Run `deploy-sync.ps1` after each src/ edit
   - Verify F5 in NinjaTrader after each ticket
   - Run full test suite after each ticket

---

## Next Steps

1. **Phase 1.5**: Scope Boundary Validation (MANDATORY)
   - Anti-pattern gate: Verify no scope creep
   - Confirm extraction boundaries are clear
   - Director approval required before Phase 2

2. **Phase 2**: Architecture Planning
   - Design method signatures for all 4 helpers
   - Document parameter passing strategy
   - Create sequence diagrams for orchestration

3. **Phase 3**: DNA & PR Audit
   - Verify no V12 DNA violations
   - Check PR hygiene (diff size, commit structure)
   - Adversarial review of extraction plan

4. **Phase 4**: Ticket Generation
   - Generate 5 executable ticket files
   - Each ticket self-contained (no cross-dependencies)
   - Include acceptance criteria and verification steps

---

## References

- **Target Method**: [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713-848)
- **Hotspot Analysis**: `docs/brain/EPIC-CCN-17/00-hotspots.md`
- **Complexity Protocol**: `docs/protocol/COMPLEXITY_REDUCTION_PROTOCOL.md`
- **Jane Street Standards**: `docs/standards/JANE_STREET_DEVIATIONS.md`
- **V12 DNA**: `AGENTS.md` (Section 2: Architectural Mandates)

---

**[SCOPE-GATE]** Scope definition complete. Awaiting Director confirmation before Phase 1.5 (Scope Boundary Validation).