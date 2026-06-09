# EPIC-CCN-17: Phase 0 - Hotspot Analysis

**Epic**: EPIC-CCN-17  
**Target**: [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713)  
**Date**: 2026-06-09  
**Analyst**: V12 Epic Planner

---

## Executive Summary

[`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713) is a **Rank #5 hotspot** with cyclomatic complexity **2.5x over Jane Street threshold** (37 vs 15). The method exhibits high cognitive load through deep nesting (8 levels) and a 7-case classification switch embedded within nested loops.

**Risk Profile**: LOW (zero blast radius, private method, no external callers)  
**Extraction Feasibility**: HIGH (clear logical boundaries, existing helper method pattern)

---

## Complexity Metrics

| Metric | Current | Target | Delta | Status |
|--------|---------|--------|-------|--------|
| **Cyclomatic Complexity** | 37 | ≤15 | -22 | ❌ 2.5x over |
| **Max Nesting Depth** | 8 | ≤4 | -4 | ❌ VERY HIGH |
| **Lines of Code** | 136 | <80 | -56 | ❌ Too long |
| **Parameter Count** | 0 | N/A | 0 | ✅ OK |

**Assessment**: HIGH complexity requiring immediate refactoring.

---

## Hotspot Score Calculation

```
Hotspot Score = CYC × log(1 + commits_last_90_days)
              = 37 × log(1 + 10)  # Estimated churn
              = 37 × 2.995
              = 110.84
```

**Rank**: #5 in codebase hotspots  
**Interpretation**: High-complexity code with moderate churn = elevated bug-introduction risk

---

## Method Structure Analysis

### Current Architecture

```
AdoptFleetOrders() [CYC=37, Nesting=8]
├─ Account snapshot (freeze-proof pattern)
├─ foreach Account [Nesting=1]
│  ├─ Fleet account filter
│  ├─ try-catch wrapper [Nesting=2]
│  │  ├─ foreach Order [Nesting=3]
│  │  │  ├─ Instrument filter [Nesting=4]
│  │  │  ├─ 5-way OrderState validation [Nesting=5, CYC+4]
│  │  │  ├─ ClassifyOrderByPrefix() call [Nesting=5]
│  │  │  ├─ 7-case switch (classification) [Nesting=6, CYC+6]
│  │  │  │  ├─ Dictionary routing logic [Nesting=7]
│  │  │  │  └─ Key extraction logic [Nesting=7]
│  │  │  ├─ Dictionary assignment [Nesting=6]
│  │  │  ├─ Position rebuilding (entry orders) [Nesting=7, CYC+2]
│  │  │  │  ├─ RebuildFleetPositionFromEntry() call [Nesting=8]
│  │  │  │  └─ activePositions update [Nesting=8]
│  │  │  └─ Force-sync existing positions [Nesting=7, CYC+1]
│  │  └─ catch block [Nesting=3]
└─ return adoptedCount
```

**Complexity Drivers**:
1. **5-way OR condition** (OrderState validation): +4 CYC
2. **7-case switch** (classification routing): +6 CYC
3. **Nested conditionals** (position rebuilding): +3 CYC
4. **Deep nesting** (8 levels): High cognitive load

---

## Code Smells Identified

### 1. God Method Anti-Pattern
- **136 lines** doing multiple responsibilities:
  - Account iteration
  - Order filtering
  - Classification logic
  - Dictionary routing
  - Position rebuilding
  - Error handling

### 2. Deep Nesting (8 Levels)
- **Jane Street Principle Violation**: "Make illegal states unrepresentable"
- Current structure allows many intermediate states
- Hard to reason about control flow

### 3. Switch Statement Duplication
- 7-case switch with repetitive dictionary assignment pattern
- Each case: `targetDict = X; key = substring(N); dictName = "X"`
- Opportunity for data-driven approach

### 4. Mixed Abstraction Levels
- High-level: Account/Order iteration
- Mid-level: Classification logic
- Low-level: String manipulation (`Substring`, `StartsWith`)

---

## Existing Helper Methods (Reusable)

The codebase already demonstrates the extraction pattern:

1. **[`ClassifyOrderByPrefix()`](src/V12_002.SIMA.Lifecycle.cs:993)** [CYC=8]
   - Pure function, no state mutations
   - 8-way prefix classification
   - Returns: `"stop"`, `"target1"`-`"target5"`, `"entry"`, or `null`

2. **[`RebuildFleetPositionFromEntry()`](src/V12_002.SIMA.Lifecycle.cs:858)**
   - Pure function, reads from dictionaries
   - Reconstructs `PositionInfo` struct
   - No concurrency concerns

**Pattern**: Extract complex logic into private helper methods with clear contracts.

---

## Blast Radius Assessment

**Result**: ZERO blast radius (safe extraction target)

```json
{
  "importer_count": 0,
  "direct_dependents_count": 0,
  "overall_risk_score": 0.0,
  "confirmed_count": 0,
  "potential_count": 0
}
```

**Interpretation**:
- ✅ Private method, no external callers
- ✅ No files import this symbol
- ✅ Safe to refactor without downstream impact
- ✅ Changes isolated to single file

---

## Extraction Opportunities

### High-Priority Extractions

1. **Order Validation Logic** [CYC reduction: -4]
   ```csharp
   // Current: 5-way OR condition
   if (ord.OrderState != Working && ord.OrderState != Accepted && ...)
   
   // Extract to: IsAdoptableOrderState(OrderState state)
   ```

2. **Dictionary Routing Logic** [CYC reduction: -6]
   ```csharp
   // Current: 7-case switch with repetitive pattern
   switch (classification) { case "stop": ...; case "target1": ...; }
   
   // Extract to: GetTargetDictionary(string classification, out string key)
   ```

3. **Position Rebuilding Logic** [CYC reduction: -3]
   ```csharp
   // Current: Nested if/else with activePositions checks
   if (targetDict == entryOrders && !activePositions.ContainsKey(key)) { ... }
   
   // Extract to: EnsurePositionTracking(Order ord, string key)
   ```

4. **Main Loop Simplification** [CYC reduction: -9]
   ```csharp
   // Current: Nested foreach with multiple filters
   foreach (Account acct) { foreach (Order ord) { ... } }
   
   // Extract to: ProcessAccountOrders(Account acct, ref int adoptedCount)
   ```

---

## Jane Street Alignment

### Current Violations

1. **Cognitive Complexity**: 37 CYC >> 15 threshold
   - Jane Street: "Functions should fit in working memory"
   - Current: Requires mental stack of 8 nested contexts

2. **Bounded Latency**: Cold path, but inefficient
   - Target: <100ms for 50 accounts
   - Current: Acceptable, but could be optimized

3. **Make Illegal States Unrepresentable**: Partial
   - Good: Uses `ClassifyOrderByPrefix()` for type safety
   - Bad: Deep nesting allows many intermediate states

### Post-Refactor Alignment

- ✅ CYC ≤15 per method (Jane Street threshold)
- ✅ Nesting ≤4 levels (cognitive simplicity)
- ✅ Single Responsibility Principle (each helper does one thing)
- ✅ Pure functions where possible (testability)

---

## Estimated Ticket Breakdown

Based on extraction opportunities:

1. **Ticket 1**: Extract `IsAdoptableOrderState()` helper
2. **Ticket 2**: Extract `GetTargetDictionary()` with routing logic
3. **Ticket 3**: Extract `EnsurePositionTracking()` for position rebuilding
4. **Ticket 4**: Extract `ProcessAccountOrders()` for main loop
5. **Ticket 5**: Simplify `AdoptFleetOrders()` to orchestration-only

**Total Tickets**: 4-5 (depending on Phase 2 architecture decisions)

---

## Success Criteria

### Quantitative
- ✅ CYC reduced from 37 to ≤15 (Jane Street threshold)
- ✅ Max nesting reduced from 8 to ≤4
- ✅ LOC reduced from 136 to <80 (main method)
- ✅ Zero new lock() statements (V12 DNA)
- ✅ ASCII-only compliance maintained

### Qualitative
- ✅ Each extracted method has single responsibility
- ✅ Pure functions where possible (no side effects)
- ✅ Clear method names (self-documenting)
- ✅ Preserved thread-safety (Actor model)
- ✅ Zero logic drift (surgical extraction only)

---

## Next Steps

1. **Phase 1**: Define extraction scope boundaries (this document feeds into `00-scope.md`)
2. **Phase 1.5**: Validate scope against anti-patterns (mandatory gate)
3. **Phase 2**: Design extraction architecture with method signatures
4. **Phase 3**: DNA & PR audit (verify no violations)
5. **Phase 4**: Generate executable tickets

---

## References

- **Target Method**: [`AdoptFleetOrders()`](src/V12_002.SIMA.Lifecycle.cs:713-848)
- **Helper Method**: [`ClassifyOrderByPrefix()`](src/V12_002.SIMA.Lifecycle.cs:993-1017)
- **Helper Method**: [`RebuildFleetPositionFromEntry()`](src/V12_002.SIMA.Lifecycle.cs:858)
- **Complexity Protocol**: `docs/protocol/COMPLEXITY_REDUCTION_PROTOCOL.md`
- **Jane Street Standards**: `docs/standards/JANE_STREET_DEVIATIONS.md`

---

**[INTAKE-GATE]** Hotspot analysis complete. Ready for Phase 1 (Scope Definition).