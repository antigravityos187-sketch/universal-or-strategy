# Extraction Tickets: EPIC-CCN-056

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2 hours
- **Target Method**: SweepBrokerOrders
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street strict standard)

## TICKET-1: Extract GetTargetPrefixes Pure Function

### Scope
- **Current Method**: `SweepBrokerOrders`
- **Current CYC**: 12
- **Target CYC**: 11 (after this extraction)
- **Extraction**: Pure function to eliminate conditional prefix array initialization
- **Lines**: 1376-1398 (prefix array logic)

### Method Signature
```csharp
private static string[] GetTargetPrefixes(bool force)
```

### Implementation Steps
1. **Create new method** above SweepBrokerOrders (line ~1370)
   - Add method signature: `private static string[] GetTargetPrefixes(bool force)`
   - Add XML documentation comment explaining purpose
   
2. **Move prefix logic** from SweepBrokerOrders
   - Extract lines 1376-1398 (ternary operator with array initialization)
   - Preserve exact array contents (14-element vs 7-element arrays)
   - Return appropriate array based on force flag
   
3. **Replace inline logic** in SweepBrokerOrders
   - Replace lines 1376-1398 with: `string[] v12Prefixes = GetTargetPrefixes(force);`
   - Verify variable name matches existing usage
   
4. **Verify extraction**
   - Run: `dotnet build src/V12_002.sln`
   - Confirm zero compilation errors
   - Run: `python scripts/complexity_audit.py`
   - Confirm GetTargetPrefixes CYC = 1
   - Confirm SweepBrokerOrders CYC = 11

### Acceptance Criteria
- [ ] GetTargetPrefixes method created with correct signature
- [ ] Method is marked `private static` (pure function)
- [ ] Prefix arrays match original exactly (14 vs 7 elements)
- [ ] SweepBrokerOrders calls GetTargetPrefixes(force)
- [ ] Build succeeds with zero errors
- [ ] GetTargetPrefixes complexity = 1
- [ ] SweepBrokerOrders complexity reduced to 11
- [ ] No behavioral changes (same orders cancelled)
- [ ] CSharpier formatting applied

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.sln

# Complexity audit
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/
```

---

## TICKET-2: Extract ShouldCancelOrder Predicate

### Scope
- **Current Method**: `SweepBrokerOrders`
- **Current CYC**: 11 (after TICKET-1)
- **Target CYC**: 6-7 (final target)
- **Extraction**: Consolidate 5 filtering guards into single predicate
- **Lines**: 1405-1419 (if-continue chain)

### Method Signature
```csharp
private bool ShouldCancelOrder(Order ord, string[] v12Prefixes, bool force, string accountName)
```

### Implementation Steps
1. **Create new method** above SweepBrokerOrders (after GetTargetPrefixes)
   - Add method signature: `private bool ShouldCancelOrder(Order ord, string[] v12Prefixes, bool force, string accountName)`
   - Add XML documentation comment explaining filtering logic
   
2. **Move filtering guards** from SweepBrokerOrders
   - Extract 5 if-continue guards (lines 1405-1419):
     1. `if (!IsOrderCancellable(ord.OrderState)) continue;`
     2. `if (!IsV12OrderPrefix(ordName, v12Prefixes)) continue;`
     3. `if (ShouldProtectBracketOrder(ordName, force, acct.Name)) continue;`
     4. Instrument match validation
     5. Fleet account check (if applicable)
   - Invert logic: return `false` for conditions that would `continue`
   - Return `true` if all filters pass
   
3. **Replace if-continue chain** in SweepBrokerOrders
   - Replace lines 1405-1419 with: `if (!ShouldCancelOrder(ord, v12Prefixes, force, acct.Name)) continue;`
   - Verify predicate call is inside order iteration loop
   
4. **Verify extraction**
   - Run: `dotnet build src/V12_002.sln`
   - Confirm zero compilation errors
   - Run: `python scripts/complexity_audit.py`
   - Confirm ShouldCancelOrder CYC ≤ 4
   - Confirm SweepBrokerOrders CYC ≤ 8

### Acceptance Criteria
- [ ] ShouldCancelOrder method created with correct signature
- [ ] Method is marked `private` (instance method, accesses helpers)
- [ ] All 5 filtering guards consolidated into predicate
- [ ] Logic inverted correctly (false = skip, true = process)
- [ ] SweepBrokerOrders calls ShouldCancelOrder predicate
- [ ] Build succeeds with zero errors
- [ ] ShouldCancelOrder complexity ≤ 4
- [ ] SweepBrokerOrders complexity ≤ 8 (Jane Street compliant)
- [ ] No behavioral changes (same orders cancelled)
- [ ] CSharpier formatting applied

### Dependencies
- **TICKET-1 must be completed first**
- Requires GetTargetPrefixes to be in place
- Requires SweepBrokerOrders to be at CYC 11

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.sln

# Complexity audit (final verification)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Post-Extraction Verification

### Final Complexity Targets
- **SweepBrokerOrders**: CYC ≤ 8 ✅
- **GetTargetPrefixes**: CYC = 1 ✅
- **ShouldCancelOrder**: CYC ≤ 4 ✅
- **Total Complexity**: 11 (distributed across 3 methods)

### V12 DNA Compliance
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Hard-link integrity preserved (run deploy-sync.ps1)
- [ ] FSM/Actor pattern preserved

### Jane Street Compliance
- [ ] All methods ≤ 8 complexity (strict standard)
- [ ] Pure function testability (GetTargetPrefixes)
- [ ] Predicate clarity (ShouldCancelOrder)
- [ ] Correctness by construction

### PR Hygiene
- [ ] Diff size < 10,000 characters
- [ ] Branch: `epic/ccn-056-sweep-broker-orders-extraction`
- [ ] No whitespace mutations
- [ ] Surgical changes only

### Final Commands
```powershell
# Hard-link sync
powershell -File .\deploy-sync.ps1

# Full validation
powershell -File .\scripts\pre_push_validation.ps1

# Complexity report
python scripts/complexity_audit.py | grep -A 5 "SweepBrokerOrders"
```

---

## Implementation Notes

### Checkpointing
- Bob CLI checkpointing is enabled by default
- Restore via `/restore` if extraction fails
- Each ticket is a separate checkpoint

### Incremental Verification
- Build after each extraction
- Complexity audit after each extraction
- Never proceed to TICKET-2 if TICKET-1 fails

### Risk Mitigation
- Pure function extraction (TICKET-1) has zero side effects
- Predicate extraction (TICKET-2) consolidates existing logic
- No new logic introduced, only reorganization

---

**Phase 4 Status**: COMPLETE
**Next Phase**: Phase 5 - Implementation (Bob CLI v12-engineer mode)
**Authorization**: PROCEED TO IMPLEMENTATION
