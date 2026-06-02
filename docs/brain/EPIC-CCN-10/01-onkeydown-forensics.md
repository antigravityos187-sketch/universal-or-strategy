# EPIC-CCN-10 Stage 0: OnKeyDown Forensic Analysis

**Generated**: 2026-06-02T14:12:00Z  
**Analyst**: Advanced Mode (jCodemunch-MCP)  
**Status**: ✅ COMPLETE - Method Already Refactored

---

## Executive Summary

**CRITICAL FINDING**: The `OnKeyDown` method listed in the Master Orchestration Plan as having 49 CYC **has already been refactored** as part of Phase 7 UI work. The method currently has **CYC 9** (per complexity tool) and is documented as **CYC 3** in its docstring.

**Recommendation**: **REMOVE** `OnKeyDown` from EPIC-CCN-10 priority queue and update the Master Orchestration Plan. This symbol does not require extraction.

---

## Method Location & Current State

### Primary Method
- **File**: `src/V12_002.UI.Callbacks.cs`
- **Line**: 341-376 (36 lines)
- **Signature**: `private void OnKeyDown(object sender, KeyEventArgs e)`
- **Current CYC**: 9 (complexity tool) / 3 (docstring claim)
- **Max Nesting**: 2
- **Parameter Count**: 2
- **Assessment**: MEDIUM complexity (within Jane Street threshold of ≤15)

### Docstring
```
[Phase7-UI T-A] OnKeyDown residual dispatcher (CYC 3) - Command Pattern with O(1) lookup
```

---

## Refactoring History

### Git Provenance Analysis

**Origin Commit**:
- **SHA**: 103628c1aaf9
- **Author**: Mo
- **Date**: 2026-02-16
- **Subject**: "V12.44 Phase 5: Surgical modularization – split Orders.cs + UI.cs into 6 focused modules"
- **Intent**: "Orders.cs (2,023 lines) → Orders.Callbacks.cs + Orders.Management.cs"

**Evolution Timeline** (99 days, 3 commits):
1. **2026-02-16** (Mo): Initial extraction from monolithic UI.cs
2. **2026-05-07** (AI M. Khalid): Merge Phase 5 Part 2 (#98)
3. **2026-05-26** (mdasdispatch-hash): "[EPIC-7-QUALITY] TICKET-008: UI callbacks error handling - 8 empty catch blocks fixed"

**Dominant Pattern**: Evolution (2/3 commits)

---

## Current Architecture

### Command Pattern Implementation

The method uses a **Command Pattern** with O(1) dictionary lookup for basic hotkeys:

```csharp
private void OnKeyDown(object sender, KeyEventArgs e)
{
    // Basic hotkeys (no modifiers) - O(1) dictionary lookup
    if (_keyCommands != null && _keyCommands.TryGetValue(e.Key, out var cmd))
    {
        cmd();
        e.Handled = true;
        return;
    }

    // T1 Actions (1 + letter)
    if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1))
    {
        HandleTargetAction("T1", e.Key);
        e.Handled = true;
        return;
    }

    // T2 Actions (2 + letter)
    if (Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.NumPad2))
    {
        HandleTargetAction("T2", e.Key);
        e.Handled = true;
        return;
    }

    // Runner Actions (3 + letter)
    if (Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.NumPad3))
    {
        HandleRunnerAction(e.Key);
        e.Handled = true;
        return;
    }

    // RMA uses Shift+Click (R conflicts with NT search, Ctrl conflicts with chart drag)
}
```

### Helper Methods (Already Extracted)

#### 1. HandleTargetAction
- **Line**: 379-390 (12 lines)
- **Signature**: `private void HandleTargetAction(string target, Key key)`
- **CYC**: 6 (docstring claim)
- **Purpose**: Route T1/T2 target actions via switch statement
- **Pattern**: Switch-based dispatcher

#### 2. HandleRunnerAction
- **Line**: 393-404 (12 lines)
- **Signature**: `private void HandleRunnerAction(Key key)`
- **CYC**: 6 (docstring claim)
- **Purpose**: Route runner actions via switch statement
- **Pattern**: Switch-based dispatcher with FSM/Actor Enqueue

---

## Complexity Analysis

### Cyclomatic Complexity Discrepancy

**Master Orchestration Plan Claim**: 49 CYC  
**Actual Measurement** (jcodemunch-mcp): 9 CYC  
**Docstring Claim**: 3 CYC  

**Analysis**:
- The 49 CYC figure is **outdated** and refers to the pre-refactored version
- The current implementation has been surgically extracted into helper methods
- The docstring claim of CYC 3 is **incorrect** (should be 9)
- CYC 9 is **well within** the Jane Street threshold of ≤15

### Complexity Drivers

Current CYC 9 breakdown:
1. Base path: 1
2. `_keyCommands` null check + TryGetValue: +2
3. T1 action check (D1 || NumPad1): +2
4. T2 action check (D2 || NumPad2): +2
5. Runner action check (D3 || NumPad3): +2

**Total**: 9 decision points

---

## Dependencies

### What OnKeyDown Calls
- `_keyCommands.TryGetValue()` - Dictionary lookup (O(1))
- `HandleTargetAction()` - Helper method (CYC 6)
- `HandleRunnerAction()` - Helper method (CYC 6)

### What Calls OnKeyDown
**NONE** - Blast radius analysis shows zero importers/references.

**Explanation**: This is an event handler method, likely wired up via WPF/XAML event binding or programmatic subscription. The lack of direct code references is expected for UI event handlers.

---

## Phase 7 Refactoring Evidence

### Code Comments Found
Multiple "[Phase7-UI T-A]" comments in `src/V12_002.UI.Callbacks.cs`:

1. **Line 41**: `// [Phase7-UI T-A] Command Pattern: Pre-allocated dictionary for basic hotkeys (zero allocation on hot path)`
2. **Line 340**: `// [Phase7-UI T-A] OnKeyDown residual dispatcher (CYC 3) - Command Pattern with O(1) lookup`
3. **Line 378**: `// [Phase7-UI T-A] Helper: Route T1/T2 target actions (CYC 6)`
4. **Line 392**: `// [Phase7-UI T-A] Helper: Route runner actions (CYC 6)`

### Related File: V12_002.UI.Panel.Handlers.cs
- **Line 44**: `// [Phase7-UI T-A] Initialize command registry for basic hotkeys (CYC 3)`

**Conclusion**: This was part of a coordinated Phase 7 UI refactoring effort that extracted keyboard handling logic.

---

## Risk Assessment

### Refactoring Risk: **NONE** (Already Complete)

The method does not require further extraction because:
1. ✅ CYC 9 is below Jane Street threshold (≤15)
2. ✅ Command Pattern already implemented
3. ✅ Helper methods already extracted
4. ✅ O(1) dictionary lookup for hot path
5. ✅ Clear separation of concerns

### Architectural Quality: **HIGH**

**Strengths**:
- Early return pattern (reduces nesting)
- O(1) dictionary lookup for basic hotkeys
- Delegated complexity to helper methods
- Zero heap allocations on hot path (pre-allocated dictionary)
- Clear intent via comments

**Weaknesses**:
- Docstring CYC claim (3) doesn't match actual (9) - minor documentation issue
- No unit tests found for this method

---

## Master Orchestration Plan Discrepancy

### Root Cause Analysis

**Hypothesis**: The Master Orchestration Plan was generated from an **outdated complexity audit** that ran before the Phase 7 UI refactoring was completed.

**Evidence**:
1. Plan lists `OnKeyDown` at line 337 (actual: line 341) - 4-line drift
2. Plan claims 49 CYC (actual: 9 CYC) - 40 CYC reduction
3. Git history shows refactoring completed 2026-02-16 (99 days ago)
4. Plan generated 2026-06-02 but references pre-refactored state

**Impact**: The Master Orchestration Plan's "Next 5 Symbols" list is **stale** and needs regeneration.

---

## Recommendations

### Immediate Actions

1. **UPDATE Master Orchestration Plan**:
   - Remove `OnKeyDown` from EPIC-CCN-10 priority queue
   - Regenerate complexity audit to get current state
   - Verify all 45 symbols in the list are still valid targets

2. **FIX Docstring**:
   - Update line 340 comment from "CYC 3" to "CYC 9"
   - Or re-measure if CYC 3 is correct (complexity tool may be over-counting)

3. **ADD Unit Tests**:
   - Create `tests/V12_Performance.Tests/UI/OnKeyDownTests.cs`
   - Test Command Pattern dictionary lookup
   - Test T1/T2/Runner action routing
   - Test early return behavior

### Strategic Actions

1. **Regenerate Complexity Audit**:
   ```powershell
   python scripts/complexity_audit.py > docs/brain/complexity_audit_current.md
   ```

2. **Cross-Reference with Git History**:
   - Use `get_symbol_provenance` for all 45 symbols
   - Filter out any that were already refactored in Phase 5-7

3. **Update EPIC-CCN-10 Scope**:
   - Recalculate 4/45 progress (may be higher if other symbols were also refactored)
   - Adjust timeline estimates based on actual remaining work

---

## Appendix A: Related Methods

### ExecuteTargetAction (Downstream)
- **File**: `src/V12_002.UI.Callbacks.cs`
- **Signature**: `private void ExecuteTargetAction(string target, string action)`
- **Called By**: `HandleTargetAction`
- **Purpose**: Execute specific target action (market, 1point, 2point, etc.)

### ExecuteRunnerAction (Downstream)
- **File**: `src/V12_002.UI.Callbacks.cs`
- **Line**: 827
- **Signature**: `private void ExecuteRunnerAction(string action)`
- **Called By**: `HandleRunnerAction` (via FSM Enqueue)
- **Purpose**: Execute runner action (market, stop1pt, stop2pt, etc.)

### Command Registry Initialization
- **File**: `src/V12_002.UI.Panel.Handlers.cs`
- **Line**: 44
- **Purpose**: Initialize `_keyCommands` dictionary
- **Pattern**: Pre-allocated dictionary for zero-allocation hot path

---

## Appendix B: Complexity Tool Discrepancy

### Docstring vs Tool Measurement

**Docstring Claim**: CYC 3  
**Tool Measurement**: CYC 9  

**Possible Explanations**:
1. **Docstring is outdated** - Written during refactoring, not updated after final implementation
2. **Tool over-counts** - Counting `||` operators as separate branches (D1 || NumPad1 = +2 instead of +1)
3. **Different CYC definitions** - Docstring may use simplified CYC (ignoring short-circuit operators)

**Recommendation**: Trust the tool measurement (9) and update the docstring. The Jane Street threshold is ≤15, so both values are acceptable.

---

## Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2026-06-02 | Initial forensic analysis | Advanced Mode (jCodemunch-MCP) |

---

**[FORENSICS-COMPLETE]**

**Next Action**: Update Master Orchestration Plan to remove `OnKeyDown` from EPIC-CCN-10 queue and regenerate complexity audit for accurate remaining work.