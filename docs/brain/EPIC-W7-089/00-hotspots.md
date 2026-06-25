# Phase 0: Hotspot Analysis - EPIC-W7-089

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: ~20 seconds

## Target Method
- **Method**: CancelWatchdogWorkingOrders
- **File**: src/V12_002.Safety.Watchdog.cs
- **Line**: 138
- **Cyclomatic Complexity**: 10 (actual, not 12 as initially stated)
- **Max Nesting Depth**: 3
- **Parameter Count**: 2
- **Lines of Code**: 28

## Complexity Metrics

### Symbol Complexity Analysis
```json
{
  "cyclomatic": 10,
  "max_nesting": 3,
  "param_count": 2,
  "lines": 28,
  "assessment": "medium"
}
```

**Assessment**: MEDIUM complexity
- CYC=10 exceeds Jane Street threshold of 8
- Moderate nesting depth (3 levels)
- Reasonable parameter count (2)
- Compact method size (28 lines)

### Hotspot Ranking Context
The target method `CancelWatchdogWorkingOrders` does NOT appear in the top 50 hotspots list, indicating:
- Lower churn rate compared to high-risk methods
- Not a frequent change target
- Relatively stable code

Top 3 actual hotspots for reference:
1. **HydrateFromOpenPositions** (CYC=34, hotspot_score=120.88)
2. **IsCommandForThisInstrument** (CYC=38, hotspot_score=109.83)
3. **HandleTerminated** (CYC=30, hotspot_score=102.04)

## Blast Radius Analysis

### Direct Impact
```json
{
  "importer_count": 0,
  "direct_dependents_count": 0,
  "overall_risk_score": 0.0,
  "confirmed_count": 0,
  "potential_count": 0
}
```

**Risk Level**: **VERY LOW**
- Zero external importers
- Zero direct dependents
- No confirmed blast radius
- No potential blast radius

This method is internally scoped and has minimal external coupling.

## Call Hierarchy

### Callers (Who calls this method)
**1 caller identified:**
- `ExecuteWatchdogLeadAccountFlatten` (src/V12_002.Safety.Watchdog.cs:211)
  - Resolution: ast_resolved
  - Depth: 1

### Callees (What this method calls)
**4 callees identified:**

1. **CancelOrderOnAccount** (src/V12_002.Orders.CancelGateway.cs:46)
   - Resolution: ast_inferred
   - Depth: 1

2. **CancelOrderOnAccount** (src-vm-backup/V12_002.Orders.CancelGateway.cs:46)
   - Resolution: ast_inferred
   - Depth: 1
   - Note: Backup copy detected

3. **IsOrderTerminal** (src/V12_002.Orders.Management.Flatten.cs:698)
   - Resolution: ast_inferred
   - Depth: 2

4. **IsOrderTerminal** (src-vm-backup/V12_002.Orders.Management.Flatten.cs:574)
   - Resolution: ast_inferred
   - Depth: 2
   - Note: Backup copy detected

### Call Graph Summary
```
ExecuteWatchdogLeadAccountFlatten (caller)
  └─> CancelWatchdogWorkingOrders (TARGET)
        ├─> CancelOrderOnAccount (depth 1)
        └─> IsOrderTerminal (depth 2)
```

**Depth Reached**: 2 (out of requested 3)

## Risk Assessment

### Overall Risk: **LOW**

**Justification:**
1. **Blast Radius**: Zero external dependencies (risk_score=0.0)
2. **Complexity**: CYC=10 exceeds threshold of 8 (medium risk)
3. **Churn**: Not in top 50 hotspots (low churn)
4. **Coupling**: Single caller, minimal callees (low coupling)
5. **Scope**: Internal watchdog safety logic (contained domain)

### Refactoring Priority
**Priority**: MEDIUM-LOW
- Complexity warrants reduction (CYC 10→8)
- Low blast radius makes refactoring safe
- Not a high-churn hotspot
- Good candidate for surgical extraction

### Recommended Approach
1. Extract conditional branches into helper methods
2. Reduce nesting depth from 3 to 2
3. Target CYC reduction from 10 to ≤8
4. Maintain single-caller relationship
5. Preserve watchdog safety semantics

## Context Notes
- Method is part of V12 Safety Watchdog subsystem
- Handles cancellation of working orders during watchdog operations
- Called exclusively by `ExecuteWatchdogLeadAccountFlatten`
- Uses order cancellation gateway and terminal state checks
- Backup copies detected in src-vm-backup/ (not active code)

## Next Steps (Phase 1)
1. Define precise scope boundary
2. Identify extraction candidates within the method
3. Plan helper method signatures
4. Validate no hidden dependencies
5. Prepare ticket breakdown for Phase 4
