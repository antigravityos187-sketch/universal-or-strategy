# Phase 1: Scope Definition - EPIC-CCN-110

## Epic Metadata
- **Epic ID**: EPIC-CCN-110
- **Target Method**: `AdoptMasterOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Line**: 1193
- **Current CYC**: 19
- **Target CYC**: ≤8 (58% reduction)
- **Risk Level**: MEDIUM

## Method Purpose
Adopts working orders from the master account into tracking dictionaries during SIMA lifecycle hydration. This is a critical cold-path operation that runs during strategy startup and reconnection events.

## Current Structure Analysis

### Method Signature
```csharp
private int AdoptMasterOrders()
```

### Complexity Breakdown
The method contains:
1. **Single account loop** - Iterates over `Account.Orders.ToArray()`
2. **Instrument filter** - Checks `ord.Instrument?.FullName != Instrument?.FullName`
3. **State guard** - 7-way OrderState validation (Working, Accepted, Submitted, ChangePending, ChangeSubmitted, Unknown)
4. **Classification logic** - Calls `ClassifyOrderByPrefix(name)` helper
5. **Entry filter** - Skips unrecognized orders and Fleet_ entries
6. **Key extraction** - Builds dictionary key from order name
7. **6-way switch** - Routes to appropriate dictionary (stop, target1-5)
8. **Counter increment** - Tracks adopted orders

### Dependencies
- **Reads from**: `Account.Orders`, `Instrument.FullName`
- **Writes to**: `stopOrders`, `target1Orders`, `target2Orders`, `target3Orders`, `target4Orders`, `target5Orders`
- **Calls**: `ClassifyOrderByPrefix()` (already extracted, CYC 1)
- **Returns**: `int adoptedCount`

### Caller Context
- **Called by**: `HydrateWorkingOrdersFromBroker()` (line 1108.003 comment reference)
- **Execution path**: SIMA lifecycle → EnumerateApexAccounts → HydrateWorkingOrdersFromBroker → AdoptMasterOrders
- **Thread safety**: ACTOR-SERIALIZED (must run on strategy thread)
- **Frequency**: Cold path (startup/reconnect only)

## Scope Boundary (V12.23 Protocol)

### IN SCOPE
1. **Extract state validation** - 7-way OrderState check (CYC contribution: ~7)
2. **Extract dictionary routing** - 6-way switch statement (CYC contribution: ~6)
3. **Extract key extraction logic** - Conditional substring logic (CYC contribution: ~2)

### OUT OF SCOPE
1. ❌ `ClassifyOrderByPrefix()` - Already extracted (separate method)
2. ❌ `AdoptFleetOrders()` - Separate method, different account scope
3. ❌ `HydrateWorkingOrdersFromBroker()` - Parent orchestrator (CYC 72, separate epic EPIC-CCN-52)
4. ❌ Tracking dictionary definitions - Core data structures
5. ❌ Exception handling patterns - Minimal in this method

### BOUNDARY JUSTIFICATION
- **Single-method extraction**: All complexity is contained within `AdoptMasterOrders()`
- **No cross-file changes**: Extraction targets are within same file
- **No logic drift**: Pure structural movement only
- **Caller stability**: Single caller (`HydrateWorkingOrdersFromBroker()`)

## Extraction Strategy (Preliminary)

### Target 1: State Validation Helper
**Proposed signature**:
```csharp
private bool IsValidMasterOrderState(Order ord)
```
**Purpose**: Consolidate 7-way OrderState validation
**CYC reduction**: ~7 → 1 (6-point reduction)

### Target 2: Dictionary Routing Helper
**Proposed signature**:
```csharp
private void RouteOrderToMasterDict(Order ord, string classification, string key, ref int adoptedCount)
```
**Purpose**: Consolidate 6-way switch statement
**CYC reduction**: ~6 → 1 (5-point reduction)

### Target 3: Key Extraction Helper
**Proposed signature**:
```csharp
private string ExtractMasterOrderKey(string orderName)
```
**Purpose**: Consolidate conditional substring logic
**CYC reduction**: ~2 → 1 (1-point reduction)

### Expected Final CYC
- **Current**: 19
- **After extraction**: 19 - 6 - 5 - 1 = **7** ✅ (target ≤8 achieved)

## Risk Assessment

### Technical Risks
- **LOW**: Method is pure data transformation (no FSM state, no locks)
- **LOW**: Single caller reduces integration risk
- **LOW**: Cold path (no hot-path latency concerns)

### Blast Radius
- **Direct callers**: 1 (`HydrateWorkingOrdersFromBroker()`)
- **Indirect impact**: SIMA lifecycle hydration workflow
- **Test coverage**: Unknown (likely zero - cold path)

### Mitigation Strategy
1. **Preserve exact behavior**: No logic changes during extraction
2. **Maintain thread safety**: All helpers remain ACTOR-SERIALIZED
3. **Add unit tests**: Test state validation, routing, key extraction in isolation
4. **Integration test**: F5 in NinjaTrader after extraction

## Jane Street Alignment

### Cognitive Simplicity
- **Before**: 19 CYC = difficult to reason about under restart scenarios
- **After**: 7 CYC = single-pass loop with helper calls (easy to audit)

### Correctness by Construction
- **State validation**: Explicit helper makes valid states obvious
- **Dictionary routing**: Switch statement → helper eliminates routing errors
- **Key extraction**: Centralized logic prevents substring bugs

### Testing Strategy
- **Unit tests**: Test each helper in isolation (state validation, routing, key extraction)
- **Integration test**: Verify adoption count matches before/after extraction
- **Regression test**: F5 in NinjaTrader with live broker connection

## Success Criteria

### Functional Requirements
- ✅ Method behavior unchanged (same orders adopted)
- ✅ Return value unchanged (same `adoptedCount`)
- ✅ Dictionary state unchanged (same orders in same dicts)
- ✅ Thread safety preserved (ACTOR-SERIALIZED)

### Quality Requirements
- ✅ Final CYC ≤8 (target: 7)
- ✅ All extracted methods CYC ≤8
- ✅ No logic drift (pure structural movement)
- ✅ ASCII-only compliance maintained

### Verification Requirements
- ✅ `complexity_audit.py` confirms CYC ≤8
- ✅ `deploy-sync.ps1` passes (ASCII gate)
- ✅ `dotnet build` succeeds (zero errors)
- ✅ F5 in NinjaTrader succeeds (strategy loads)
- ✅ Unit tests pass (if added)

## Next Steps (Phase 2)

1. **Architecture Planning**: Design helper method signatures
2. **DNA Audit**: Verify no lock usage, ASCII compliance
3. **PR Hygiene**: Estimate diff size (<10k chars target)
4. **Ticket Generation**: Create surgical extraction tickets

## Status
[OK] Phase 1 Complete - Ready for Phase 2 (Architecture Planning)

---

**Document Version**: 1.0  
**Created**: 2026-06-11  
**Author**: Bob Shell (v12-engineer)  
**Protocol**: V12.23 (No Scope Creep)
