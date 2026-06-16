# Phase 2: Architecture Plan - EPIC-CCN-018

## Executive Summary

**Target**: IsSymbolMatch method in V12_002.UI.IPC.cs
**Current Complexity**: 18 (CYC)
**Target Complexity**: ≤8 per method (Jane Street strict standard)
**Strategy**: Extract conditional logic into 3 focused helper methods

## Current Method Analysis

### Original Signature
```csharp
private bool IsSymbolMatch(string targetSymbol)
```

### Current Implementation
- **Lines of Code**: 19
- **Cyclomatic Complexity**: 18
- **Branches**: 17 OR conditions + 1 method entry
- **Responsibility**: Determines if a target symbol matches the current instrument

### Complexity Breakdown
The method has 3 distinct matching strategies:
1. **Global Keywords** (7 branches): GLOBAL, ALL, ON, OFF, RMA, ORB, OR, MOMO
2. **Direct Symbol Matching** (4 branches): Exact match, prefix match, contains, reverse prefix
3. **Micro Futures Aliases** (3 branches): MES→ES, MYM→YM, MGC→GC

## Extraction Strategy

### Approach: Strategy Pattern with Helper Methods
Split the monolithic OR chain into focused, testable methods that each handle one matching strategy.

### Target Architecture
```
IsSymbolMatch (CYC: 4)
├── IsGlobalKeyword (CYC: 2)
├── IsDirectSymbolMatch (CYC: 5)
└── IsMicroFuturesAlias (CYC: 4)
```

**Total Complexity**: 4 + 2 + 5 + 4 = 15 (distributed across 4 methods)
**Max Method Complexity**: 5 (well under threshold of 8)

## Proposed Method Signatures

### 1. IsGlobalKeyword (New Helper)
```csharp
private static bool IsGlobalKeyword(string target)
```

**Complexity**: 2 (1 entry + 1 switch expression)
**Rationale**: Static method using switch expression for O(1) lookup

### 2. IsDirectSymbolMatch (New Helper)
```csharp
private static bool IsDirectSymbolMatch(string mySym, string myFull, string target)
```

**Complexity**: 5 (1 entry + 4 OR conditions)
**Rationale**: Encapsulates string matching logic

### 3. IsMicroFuturesAlias (New Helper)
```csharp
private static bool IsMicroFuturesAlias(string mySym, string target)
```

**Complexity**: 4 (1 entry + 3 OR conditions)
**Rationale**: Encapsulates futures market knowledge

### 4. IsSymbolMatch (Refactored)
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

**Complexity**: 4 (1 entry + 3 OR conditions calling helpers)

## Call Graph

```
IsSymbolMatch (instance method)
│
├─→ IsGlobalKeyword(target) [static]
├─→ IsDirectSymbolMatch(mySym, myFull, target) [static]
└─→ IsMicroFuturesAlias(mySym, target) [static]
```

**Data Flow**: IsSymbolMatch normalizes input, passes to static helpers, returns OR of all strategies

**Shared State**: None (all helpers are static and pure)

## Lock-Free Validation

### ✅ Compliance Checklist
- [x] No lock() statements
- [x] No shared mutable state
- [x] Atomic operations (string comparisons)
- [x] Thread-safe by design

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Before**: 1 method with CYC 18
- **After**: 4 methods with max CYC 5

### Correctness by Construction
- Global Keywords: Switch expression makes invalid keywords unrepresentable
- Symbol Matching: Clear separation of matching strategies
- Micro Futures: Explicit mapping

## Implementation Plan

### Step 1: Add Helper Methods
1. Add IsGlobalKeyword static method
2. Add IsDirectSymbolMatch static method
3. Add IsMicroFuturesAlias static method

### Step 2: Refactor IsSymbolMatch
1. Replace OR chain with helper calls
2. Preserve normalization logic

### Step 3: Verification
1. Run dotnet build
2. Run complexity_audit.py
3. Run pre_push_validation.ps1 -Fast
4. Manual F5 test in NinjaTrader

## Complexity Reduction Summary

| Metric | Before | After | Delta |
|--------|--------|-------|-------|
| IsSymbolMatch CYC | 18 | 4 | -14 (-78%) |
| Max Method CYC | 18 | 5 | -13 (-72%) |
| Total Methods | 1 | 4 | +3 |

## V12 DNA Alignment

- [x] Correctness by Construction
- [x] Lock-Free Actor Pattern (N/A - pure function)
- [x] ASCII-Only Compliance
- [x] Jane Street Alignment (CYC ≤8)

---

**Plan Date**: 2026-06-15
**Architect**: Bob Shell (v12-engineer mode)
**Status**: Ready for Phase 3 (Adjudicator Review)
