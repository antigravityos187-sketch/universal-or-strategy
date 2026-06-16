# Extraction Tickets: EPIC-CCN-018

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 2 hours
- **Target File**: src/V12_002.UI.IPC.cs
- **Target Method**: IsSymbolMatch (Current CYC: 18)

## TICKET-1: Extract IsGlobalKeyword Helper

### Scope
- **Current Method**: `IsSymbolMatch`
- **Current CYC**: 18
- **Target CYC**: N/A (new method with CYC 2)
- **Extraction**: Global keyword matching logic (7 keywords)

### Implementation
1. Add new static method `IsGlobalKeyword(string target)` above `IsSymbolMatch`
2. Implement switch expression for keywords: GLOBAL, ALL, ON, OFF, RMA, ORB, OR, MOMO
3. Return true for matches, false otherwise
4. Use pattern: `return target switch { "GLOBAL" => true, ... _ => false };`

### Code Template
```csharp
private static bool IsGlobalKeyword(string target)
{
    return target switch
    {
        "GLOBAL" => true,
        "ALL" => true,
        "ON" => true,
        "OFF" => true,
        "RMA" => true,
        "ORB" => true,
        "OR" => true,
        "MOMO" => true,
        _ => false
    };
}
```

### Acceptance Criteria
- [ ] Method added with correct signature
- [ ] Switch expression covers all 7 keywords
- [ ] Method is static and private
- [ ] Complexity audit shows CYC ≤ 2
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (method not yet called)

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract IsDirectSymbolMatch Helper

### Scope
- **Current Method**: `IsSymbolMatch`
- **Current CYC**: 18
- **Target CYC**: N/A (new method with CYC 5)
- **Extraction**: Direct symbol matching logic (4 conditions)

### Implementation
1. Add new static method `IsDirectSymbolMatch(string mySym, string myFull, string target)` above `IsSymbolMatch`
2. Implement 4 OR conditions:
   - Exact match: `mySym == target`
   - Prefix match: `mySym.StartsWith(target)`
   - Contains: `myFull.Contains(target)`
   - Reverse prefix: `target.StartsWith(mySym)`
3. Return true if any condition matches

### Code Template
```csharp
private static bool IsDirectSymbolMatch(string mySym, string myFull, string target)
{
    return mySym == target
        || mySym.StartsWith(target)
        || myFull.Contains(target)
        || target.StartsWith(mySym);
}
```

### Acceptance Criteria
- [ ] Method added with correct signature
- [ ] All 4 matching conditions implemented
- [ ] Method is static and private
- [ ] Complexity audit shows CYC ≤ 5
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (method not yet called)

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Extract IsMicroFuturesAlias Helper

### Scope
- **Current Method**: `IsSymbolMatch`
- **Current CYC**: 18
- **Target CYC**: N/A (new method with CYC 4)
- **Extraction**: Micro futures alias matching logic (3 conditions)

### Implementation
1. Add new static method `IsMicroFuturesAlias(string mySym, string target)` above `IsSymbolMatch`
2. Implement 3 OR conditions for micro futures mappings:
   - MES → ES: `(mySym == "MES" && target == "ES")`
   - MYM → YM: `(mySym == "MYM" && target == "YM")`
   - MGC → GC: `(mySym == "MGC" && target == "GC")`
3. Return true if any mapping matches

### Code Template
```csharp
private static bool IsMicroFuturesAlias(string mySym, string target)
{
    return (mySym == "MES" && target == "ES")
        || (mySym == "MYM" && target == "YM")
        || (mySym == "MGC" && target == "GC");
}
```

### Acceptance Criteria
- [ ] Method added with correct signature
- [ ] All 3 micro futures mappings implemented
- [ ] Method is static and private
- [ ] Complexity audit shows CYC ≤ 4
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (method not yet called)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

---

## TICKET-4: Refactor IsSymbolMatch to Use Helpers

### Scope
- **Current Method**: `IsSymbolMatch`
- **Current CYC**: 18
- **Target CYC**: ≤ 4
- **Extraction**: Replace OR chain with helper method calls

### Implementation
1. Keep normalization logic (ToUpperInvariant, Trim)
2. Replace 17-condition OR chain with 3 helper calls:
   - `IsGlobalKeyword(target)`
   - `IsDirectSymbolMatch(mySym, myFull, target)`
   - `IsMicroFuturesAlias(mySym, target)`
3. Return OR of all three helper results

### Code Template
```csharp
private bool IsSymbolMatch(string targetSymbol)
{
    string mySym = Instrument.MasterInstrument.Name.ToUpperInvariant();
    string myFull = Instrument.FullName.ToUpperInvariant();
    string target = targetSymbol.Trim().ToUpperInvariant();

    return IsGlobalKeyword(target)
        || IsDirectSymbolMatch(mySym, myFull, target)
        || IsMicroFuturesAlias(mySym, target);
}
```

### Acceptance Criteria
- [ ] Method refactored to call 3 helpers
- [ ] Normalization logic preserved
- [ ] Complexity audit shows CYC ≤ 4
- [ ] Build succeeds (`dotnet build`)
- [ ] All tests pass (`dotnet test`)
- [ ] Pre-push validation passes (`powershell -File .\scripts\pre_push_validation.ps1 -Fast`)
- [ ] Manual F5 test in NinjaTrader confirms symbol matching works
- [ ] No behavioral changes (logic equivalence verified)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

---

## Complexity Reduction Summary

| Metric | Before | After | Delta | Status |
|--------|--------|-------|-------|--------|
| IsSymbolMatch CYC | 18 | 4 | -14 (-78%) | ✅ TARGET |
| IsGlobalKeyword CYC | N/A | 2 | +2 | ✅ PASS |
| IsDirectSymbolMatch CYC | N/A | 5 | +5 | ✅ PASS |
| IsMicroFuturesAlias CYC | N/A | 4 | +4 | ✅ PASS |
| Max Method CYC | 18 | 5 | -13 (-72%) | ✅ PASS |
| Total Methods | 1 | 4 | +3 | ✅ PASS |

## Post-Implementation Checklist

### Build & Test
- [ ] Run `dotnet build` (zero errors)
- [ ] Run `dotnet test` (100% pass)
- [ ] Run `python scripts/complexity_audit.py` (verify CYC ≤ 15)
- [ ] Run `dotnet csharpier format src/` (enforce formatting)

### Quality Gates
- [ ] Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
- [ ] Verify no new Codacy issues
- [ ] Confirm diff size < 10k characters

### Hard-Link Sync
- [ ] Run `powershell -File .\deploy-sync.ps1` (sync to NinjaTrader)
- [ ] Manual F5 test in NinjaTrader
- [ ] Verify symbol matching behavior unchanged

### PR Hygiene
- [ ] Single file changed (V12_002.UI.IPC.cs)
- [ ] No whitespace mutations outside target method
- [ ] Clear commit message: "EPIC-CCN-018: Extract IsSymbolMatch helpers (CYC 18→4)"

---

**Ticket Generation Date**: 2026-06-15
**Phase 4 Status**: COMPLETED
**Next Phase**: Phase 5 (Ticket Execution)
