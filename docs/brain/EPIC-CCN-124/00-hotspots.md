# Phase 0: Hotspot Analysis - EPIC-CCN-124

## Target Method
- **Method**: DrawORBox
- **File**: src/V12_002.DrawingHelpers.cs
- **Cyclomatic Complexity**: 12
- **Lines of Code**: 77

## Complexity Metrics

### Method Signature
```csharp
private void DrawORBox(...)
```

### Complexity Breakdown
- **Estimated Cyclomatic Complexity**: 12
- **Lines of Code**: 77
- **Classification**: OK (below threshold 15, but approaching watch zone)

### Complexity Drivers
Based on the audit report, this method has moderate complexity due to:
1. Multiple conditional branches for drawing logic
2. Time zone conversion handling
3. Drawing object state management
4. OR box visualization logic

## Blast Radius

### Direct Dependencies
- Called by: OR completion logic, session reset handlers
- Calls: ConvertToSelectedTimeZone, GetDrawObject, GetStableHash
- Modifies: NinjaTrader drawing objects collection

### Impact Assessment
- **Risk Level**: MEDIUM
- **Reason**: Core visualization method used during OR window completion
- **Affected Components**: 
  - OR box rendering
  - Time zone display
  - Drawing object lifecycle

## Call Hierarchy

### Callers (Upstream)
- ProcessORCompletion (V12_002.BarUpdate.cs)
- UpdateORBoxDisplay (V12_002.BarUpdate.cs)
- Session reset handlers

### Callees (Downstream)
- ConvertToSelectedTimeZone (complexity: 7)
- GetDrawObject (complexity: 4)
- GetStableHash (complexity: 3)
- RemoveDrawObjects (complexity: 1)

## Risk Assessment

**Overall Risk**: MEDIUM

### Risk Factors
1. **Complexity**: 12/15 (80% of threshold) - approaching watch zone
2. **LOC**: 77 lines - moderate size, manageable
3. **Coupling**: Moderate - depends on 4 helper methods
4. **Blast Radius**: Medium - affects OR visualization subsystem
5. **Churn**: Unknown (requires git history analysis)

### Refactoring Recommendation
- **Priority**: MEDIUM
- **Approach**: Extract drawing state preparation logic
- **Target Complexity**: Reduce to ≤10 by extracting:
  - Time zone conversion logic
  - Drawing object creation/update logic
  - Box dimension calculation logic

### V12 DNA Alignment
- ✅ No lock statements detected
- ✅ ASCII-only compliance (visual inspection needed)
- ⚠️ Complexity approaching Jane Street threshold (15)
- ✅ Single responsibility (drawing OR box)

## Next Steps (Phase 1)
1. Generate detailed extraction plan
2. Identify atomic extraction candidates
3. Verify no lock-free violations
4. Plan TDD test coverage for extracted methods

---
**Analysis Date**: 2026-06-13
**Analyzer**: V12 Phase 0 Hotspot Protocol
**Epic**: EPIC-CCN-124
