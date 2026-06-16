# Extraction Tickets: EPIC-CCN-009

## Overview
- **Epic ID**: EPIC-CCN-009
- **Target Method**: FindChartTraderViaChartTab
- **File**: src/V12_002.UI.Panel.Helpers.cs
- **Total Tickets**: 5
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4 → TICKET-5)
- **Estimated Effort**: 4-6 hours
- **Protocol**: V12.23 Single-Method Extraction

---

## TICKET-1: Extract Visual Tree Search Helper

### Scope
- **Current Method**: `FindChartTraderViaChartTab`
- **Current CYC**: 20
- **Target CYC**: 17 (after this extraction)
- **Extraction**: Visual tree traversal logic (lines 536-545)

### Implementation
1. Create new private method `FindChartTabInVisualTree(DependencyObject start)`
2. Extract visual tree traversal while loop (lines 536-545)
3. Return DependencyObject (ChartTab if found, null otherwise)
4. Update orchestrator to call helper method
5. Verify null handling preserved

### Code Changes
```csharp
private DependencyObject FindChartTabInVisualTree(DependencyObject start)
{
    DependencyObject current = start;
    while (current != null)
    {
        if (current.GetType().Name == "ChartTab")
        {
            return current;
        }
        current = VisualTreeHelper.GetParent(current);
    }
    return null;
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 20 to CYC 17
- [ ] Helper method has CYC ≤ 3
- [ ] All tests pass (behavior preserved)
- [ ] Build succeeds (`build_readiness.ps1`)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified
- [ ] Null handling preserved

### Dependencies
- None (first ticket)

### Verification
```powershell
# Complexity check
python3 scripts/complexity_audit.py

# Build check
powershell -File .\scripts\build_readiness.ps1

# Deploy sync
powershell -File .\deploy-sync.ps1

# Manual test
# F5 in NinjaTrader, verify panel loads
```

---

## TICKET-2: Extract Logical Tree Search Helper

### Scope
- **Current Method**: `FindChartTraderViaChartTab`
- **Current CYC**: 17 (after TICKET-1)
- **Target CYC**: 14 (after this extraction)
- **Extraction**: Logical tree traversal logic (lines 549-559)

### Implementation
1. Create new private method `FindChartTabInLogicalTree(DependencyObject start)`
2. Extract logical tree traversal while loop (lines 549-559)
3. Return DependencyObject (ChartTab if found, null otherwise)
4. Update orchestrator to call helper method as fallback
5. Verify fallback chain logic preserved

### Code Changes
```csharp
private DependencyObject FindChartTabInLogicalTree(DependencyObject start)
{
    DependencyObject current = start;
    while (current != null)
    {
        if (current.GetType().Name == "ChartTab")
        {
            return current;
        }
        current = LogicalTreeHelper.GetParent(current);
    }
    return null;
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 17 to CYC 14
- [ ] Helper method has CYC ≤ 3
- [ ] All tests pass (behavior preserved)
- [ ] Build succeeds (`build_readiness.ps1`)
- [ ] Fallback chain logic preserved
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified

### Dependencies
- TICKET-1 must be completed first

### Verification
```powershell
# Complexity check
python3 scripts/complexity_audit.py

# Build check
powershell -File .\scripts\build_readiness.ps1

# Deploy sync
powershell -File .\deploy-sync.ps1

# Manual test
# F5 in NinjaTrader, verify panel loads
```

---

## TICKET-3: Extract Reflection Search Helper

### Scope
- **Current Method**: `FindChartTraderViaChartTab`
- **Current CYC**: 14 (after TICKET-2)
- **Target CYC**: 8 (after this extraction)
- **Extraction**: Reflection-based property/field search (lines 570-597)

### Implementation
1. Create new private method `FindChartTraderViaReflection(object chartTab)`
2. Extract reflection property search (lines 570-579)
3. Extract reflection field search loop (lines 581-597)
4. Return FrameworkElement (ChartTrader if found and visible, null otherwise)
5. Update orchestrator to call helper method
6. Verify visibility check preserved

### Code Changes
```csharp
private FrameworkElement FindChartTraderViaReflection(object chartTab)
{
    // Try property first
    PropertyInfo chartTraderProp = chartTab.GetType().GetProperty("ChartTrader", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    if (chartTraderProp != null)
    {
        object chartTraderObj = chartTraderProp.GetValue(chartTab, null);
        if (chartTraderObj is FrameworkElement chartTraderElement && chartTraderElement.Visibility == Visibility.Visible)
        {
            return chartTraderElement;
        }
    }

    // Try field names
    string[] fieldNames = { "chartTrader", "_chartTrader", "m_chartTrader" };
    foreach (string fieldName in fieldNames)
    {
        FieldInfo chartTraderField = chartTab.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (chartTraderField != null)
        {
            object chartTraderObj = chartTraderField.GetValue(chartTab);
            if (chartTraderObj is FrameworkElement chartTraderElement && chartTraderElement.Visibility == Visibility.Visible)
            {
                return chartTraderElement;
            }
        }
    }

    return null;
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 14 to CYC 8
- [ ] Helper method has CYC ≤ 6
- [ ] All tests pass (behavior preserved)
- [ ] Build succeeds (`build_readiness.ps1`)
- [ ] Visibility check preserved
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified

### Dependencies
- TICKET-2 must be completed first

### Verification
```powershell
# Complexity check
python3 scripts/complexity_audit.py

# Build check
powershell -File .\scripts\build_readiness.ps1

# Deploy sync
powershell -File .\deploy-sync.ps1

# Manual test
# F5 in NinjaTrader, verify panel loads
```

---

## TICKET-4: Extract Child Search Helper

### Scope
- **Current Method**: `FindChartTraderViaChartTab`
- **Current CYC**: 8 (after TICKET-3)
- **Target CYC**: 6 (after this extraction)
- **Extraction**: Recursive child element search (lines 599-603)

### Implementation
1. Create new private method `FindChartTraderViaChildSearch(DependencyObject chartTab)`
2. Extract recursive child search call (lines 599-603)
3. Return FrameworkElement (ChartTrader if found and visible, null otherwise)
4. Update orchestrator to call helper method as final fallback
5. Verify visibility check preserved

### Code Changes
```csharp
private FrameworkElement FindChartTraderViaChildSearch(DependencyObject chartTab)
{
    FrameworkElement chartTraderElement = FindChildElementByTypeName(chartTab, "ChartTrader") as FrameworkElement;
    if (chartTraderElement != null && chartTraderElement.Visibility == Visibility.Visible)
    {
        return chartTraderElement;
    }
    return null;
}
```

### Acceptance Criteria
- [ ] Method complexity reduced from CYC 8 to CYC 6
- [ ] Helper method has CYC ≤ 2
- [ ] All tests pass (behavior preserved)
- [ ] Build succeeds (`build_readiness.ps1`)
- [ ] Visibility check preserved
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified

### Dependencies
- TICKET-3 must be completed first

### Verification
```powershell
# Complexity check
python3 scripts/complexity_audit.py

# Build check
powershell -File .\scripts\build_readiness.ps1

# Deploy sync
powershell -File .\deploy-sync.ps1

# Manual test
# F5 in NinjaTrader, verify panel loads
```

---

## TICKET-5: Refactor Orchestrator

### Scope
- **Current Method**: `FindChartTraderViaChartTab`
- **Current CYC**: 6 (after TICKET-4)
- **Target CYC**: ≤ 5 (final target)
- **Refactor**: Simplify orchestrator to coordinate helper calls

### Implementation
1. Simplify FindChartTraderViaChartTab to sequential helper calls
2. Preserve fallback chain logic (visual → logical → reflection → child)
3. Preserve null handling and early exits
4. Preserve exception handling and logging
5. Verify behavior preservation

### Code Changes
```csharp
private FrameworkElement FindChartTraderViaChartTab()
{
    try
    {
        if (ChartControl == null)
        {
            return null;
        }

        // Stage 1: Visual tree search
        DependencyObject chartTab = FindChartTabInVisualTree(ChartControl);

        // Stage 2: Logical tree search (fallback)
        if (chartTab == null)
        {
            chartTab = FindChartTabInLogicalTree(ChartControl);
        }

        // Early exit if ChartTab not found
        if (chartTab == null)
        {
            Log("FindChartTraderViaChartTab: ChartTab not found in visual or logical tree", LogLevel.Warning);
            return null;
        }

        // Stage 3: Reflection search
        FrameworkElement chartTrader = FindChartTraderViaReflection(chartTab);

        // Stage 4: Child search (fallback)
        if (chartTrader == null)
        {
            chartTrader = FindChartTraderViaChildSearch(chartTab);
        }

        // Final result
        if (chartTrader == null)
        {
            Log("FindChartTraderViaChartTab: ChartTrader not found via reflection or child search", LogLevel.Warning);
        }

        return chartTrader;
    }
    catch (Exception ex)
    {
        Log($"FindChartTraderViaChartTab: Exception - {ex.Message}", LogLevel.Error);
        return null;
    }
}
```

### Acceptance Criteria
- [ ] Method complexity reduced to CYC ≤ 5
- [ ] All 4 helper methods integrated correctly
- [ ] All tests pass (behavior preserved)
- [ ] Build succeeds (`build_readiness.ps1`)
- [ ] Fallback chain logic preserved
- [ ] Exception handling preserved
- [ ] Logging preserved
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified
- [ ] Manual F5 test passes in NinjaTrader

### Dependencies
- TICKET-4 must be completed first

### Verification
```powershell
# Final complexity check
python3 scripts/complexity_audit.py

# Build check
powershell -File .\scripts\build_readiness.ps1

# Deploy sync
powershell -File .\deploy-sync.ps1

# Pre-push validation (FULL mode)
powershell -File .\scripts\pre_push_validation.ps1

# Manual test
# F5 in NinjaTrader, verify panel loads correctly
# Test all 4 fallback strategies if possible
```

---

## Final Complexity Distribution

### Before Extraction
| Method | Lines | CYC |
|--------|-------|-----|
| FindChartTraderViaChartTab | 92 | 20 |

### After Extraction (Target)
| Method | Lines | CYC | Status |
|--------|-------|-----|--------|
| FindChartTraderViaChartTab (orchestrator) | ~30 | 5 | ✅ Target met |
| FindChartTabInVisualTree | ~10 | 3 | ✅ Target met |
| FindChartTabInLogicalTree | ~10 | 3 | ✅ Target met |
| FindChartTraderViaReflection | ~30 | 6 | ✅ Target met |
| FindChartTraderViaChildSearch | ~8 | 2 | ✅ Target met |

**Total Complexity**: 19 (distributed across 5 methods)
**Max Complexity**: 6 (well below threshold of 8)

---

## Success Criteria Summary

### Functional Requirements
- ✅ Method signature unchanged (no API surface changes)
- ✅ Behavior preserved (identical outputs for all inputs)
- ✅ All existing tests pass
- ✅ Manual F5 test in NinjaTrader succeeds

### Quality Requirements
- ✅ Complexity: CYC ≤ 8 per method (Jane Street strict standard)
- ✅ No lock() statements introduced
- ✅ No compilation errors
- ✅ Codacy quality gate passes (no new issues)

### V12 Protocol Requirements
- ✅ Scope boundary respected (single method only)
- ✅ Jane Street alignment verified
- ✅ Pre-push validation passes
- ✅ PR diff < 10k characters

---

## Execution Notes

### Sequential Execution Required
Tickets MUST be executed in order (1 → 2 → 3 → 4 → 5) because:
- Each ticket reduces complexity incrementally
- Later tickets depend on earlier extractions
- Orchestrator refactor (TICKET-5) requires all helpers to exist

### Checkpoint Strategy
After each ticket:
1. Run complexity audit to verify CYC reduction
2. Run build_readiness.ps1 to verify compilation
3. Run deploy-sync.ps1 to sync NinjaTrader hard links
4. Commit changes with descriptive message

### Rollback Plan
If any ticket fails:
1. Use Bob CLI `/restore` to revert to previous checkpoint
2. Review failure reason
3. Adjust implementation approach
4. Retry ticket

---

**Document Version**: 1.0
**Phase**: 4 (Ticket Generation)
**Status**: READY FOR PHASE 5 (EXECUTION)
**Protocol**: V12.23 Single-Method Extraction
**Next Step**: Execute TICKET-1
