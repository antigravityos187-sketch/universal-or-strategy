# Phase 2: Architecture Planning - EPIC-CCN-045

## Target Method Analysis

### Current State
- **Method**: `OnKeyDown`
- **File**: `src/V12_002.UI.Callbacks.cs`
- **Line Range**: 391-427 (37 lines)
- **Complexity**: 9 (CYC)
- **LOC**: 17 (executable lines)
- **Tier**: 2 (Medium complexity)

### Complexity Breakdown
```
Base complexity: 1
+ if (_keyCommands != null && _keyCommands.TryGetValue(...)) : +2 (if + AND)
+ if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1)) : +2 (if + OR)
+ if (Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.NumPad2)) : +2 (if + OR)
+ if (Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.NumPad3)) : +2 (if + OR)
= Total: 9
```

## Extraction Strategy

### Goal
Reduce complexity from **CYC 9** to **CYC ≤8** (Jane Street strict standard)

### Approach: Extract Modifier Key Routing
Extract the three modifier key blocks (T1, T2, Runner) into a single helper method that:
1. Checks which modifier key is pressed
2. Routes to the appropriate handler
3. Returns bool indicating if key was handled

### Complexity Impact
**Before**:
- `OnKeyDown`: CYC 9

**After**:
- `OnKeyDown`: CYC 4 (1 base + 1 if for _keyCommands + 1 if for TryHandleModifierAction + 1 AND)
- `TryHandleModifierAction`: CYC 7 (1 base + 3 if statements + 3 OR conditions)

**Result**: Both methods ≤8 ✅

## Method Signatures

### Original Method
```csharp
// [Phase7-UI T-A] OnKeyDown residual dispatcher (CYC 3) - Command Pattern with O(1) lookup
private void OnKeyDown(object sender, KeyEventArgs e)
```

**Parameters**:
- `sender`: object (event sender, typically ChartControl.OwnerChart)
- `e`: KeyEventArgs (contains Key property and Handled flag)

**Return**: void

**Access**: private (event handler callback)

### Proposed Helper Method

```csharp
// [EPIC-CCN-045] Modifier key routing (CYC 7) - Extracts T1/T2/Runner hotkey logic
private bool TryHandleModifierAction(Key key)
```

**Parameters**:
- `key`: Key (the pressed key from KeyEventArgs)

**Return**: bool (true if key was handled, false otherwise)

**Access**: private (internal helper)

**Rationale**:
- Returns bool to indicate handling status (cleaner than setting Handled in multiple places)
- Takes only Key parameter (no need for full KeyEventArgs)
- Private access (not part of public API)

## Call Graph

```
OnKeyDown (CYC 4)
├─> _keyCommands.TryGetValue() [Dictionary lookup]
│   └─> cmd() [Execute command delegate]
│
└─> TryHandleModifierAction(key) [NEW METHOD]
    ├─> Keyboard.IsKeyDown(Key.D1/NumPad1)
    │   └─> HandleTargetAction("T1", key)
    │
    ├─> Keyboard.IsKeyDown(Key.D2/NumPad2)
    │   └─> HandleTargetAction("T2", key)
    │
    └─> Keyboard.IsKeyDown(Key.D3/NumPad3)
        └─> HandleRunnerAction(key)
```

### Data Flow
1. **OnKeyDown** receives KeyEventArgs from NinjaTrader event system
2. First checks dictionary for basic hotkeys (no extraction needed - already optimal)
3. If not found, calls **TryHandleModifierAction** with key parameter
4. **TryHandleModifierAction** checks modifier keys and routes to existing handlers
5. Returns true if handled, false otherwise
6. **OnKeyDown** sets Handled flag based on return value

### Shared State
- **None**: Method is stateless
- Uses existing instance fields:
  - `_keyCommands` (Dictionary<Key, Action>)
  - Existing helper methods: `HandleTargetAction`, `HandleRunnerAction`

## Implementation Plan

### Step 1: Create Helper Method
Insert new method after OnKeyDown (before HandleTargetAction at line ~439):

```csharp
// [EPIC-CCN-045] Modifier key routing (CYC 7) - Extracts T1/T2/Runner hotkey logic
private bool TryHandleModifierAction(Key key)
{
    // T1 Actions (1 + letter)
    if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1))
    {
        HandleTargetAction("T1", key);
        return true;
    }

    // T2 Actions (2 + letter)
    if (Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.NumPad2))
    {
        HandleTargetAction("T2", key);
        return true;
    }

    // Runner Actions (3 + letter)
    if (Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.NumPad3))
    {
        HandleRunnerAction(key);
        return true;
    }

    return false;
}
```

### Step 2: Refactor OnKeyDown
Replace lines 402-433 with single call:

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

    // Modifier key actions (T1/T2/Runner)
    if (TryHandleModifierAction(e.Key))
    {
        e.Handled = true;
        return;
    }

    // RMA uses Shift+Click (R conflicts with NT search, Ctrl conflicts with chart drag)
}
```

### Step 3: Verify Complexity
- Run `python3 scripts/complexity_audit.py` to confirm CYC ≤8 for both methods
- Expected: OnKeyDown CYC 4, TryHandleModifierAction CYC 7

## Lock-Free Validation

### Analysis
✅ **No lock() statements**: Method is pure event handler dispatch
✅ **No shared mutable state**: Only reads from _keyCommands dictionary
✅ **No race conditions**: Event handlers run on UI thread (single-threaded)
✅ **Atomic operations**: Not required (UI thread serialization)

### FSM/Actor Pattern Compliance
- **Not applicable**: This is a UI event handler, not FSM state transition
- **Thread safety**: Guaranteed by WPF event dispatcher (UI thread only)
- **Correctness**: Early returns prevent state inconsistency

## Jane Street Compliance

### Cognitive Simplicity ✅
- **Before**: 9 decision points in single method
- **After**: 4 decision points in OnKeyDown, 7 in TryHandleModifierAction
- **Benefit**: Each method has single, clear responsibility
  - OnKeyDown: Route to basic commands OR modifier actions
  - TryHandleModifierAction: Determine which modifier is pressed

### HFT Microsecond-Latency Requirements ✅
- **Hot Path**: Dictionary lookup remains O(1) (unchanged)
- **Cold Path**: Modifier key checks (not performance-critical)
- **No Allocations**: No new objects created (bool return is stack-allocated)
- **No Virtual Calls**: All methods are private (devirtualized by JIT)

### Testability ✅
- **Before**: 9 paths to test (exponential growth)
- **After**: 4 paths in OnKeyDown + 7 paths in TryHandleModifierAction = 11 total
- **Benefit**: Paths are isolated and can be tested independently
- **Mock Strategy**: Can mock Keyboard.IsKeyDown for unit tests

### Make Illegal States Unrepresentable ✅
- **No state changes**: Pure dispatch logic
- **Type safety**: Key enum prevents invalid keys
- **Early returns**: Prevent fall-through bugs

## Risk Assessment

### Low Risk Factors
- Pure Extract Method refactoring (no logic changes)
- No caller/callee modifications
- No state structure changes
- Existing helper methods remain unchanged

### Mitigation
- Verify with F5 in NinjaTrader after extraction
- Run `powershell -File .\scripts\build_readiness.ps1`
- Check for compilation errors

## Success Criteria

### Functional
- [ ] OnKeyDown complexity reduced to ≤8
- [ ] TryHandleModifierAction complexity ≤8
- [ ] All hotkeys work identically (T1, T2, Runner, basic)
- [ ] No compilation errors
- [ ] No runtime exceptions

### Non-Functional
- [ ] No performance regression (dictionary lookup still O(1))
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Code readability improved

### Verification
- [ ] `python3 scripts/complexity_audit.py` shows CYC ≤8
- [ ] `dotnet build` succeeds
- [ ] `powershell -File .\deploy-sync.ps1` succeeds
- [ ] F5 in NinjaTrader loads without errors

## Next Phase

**Phase 3: Implementation** (Bob CLI `v12-engineer` mode)
- Execute extraction using apply_diff or search_and_replace
- Run complexity audit
- Verify build
- Test in NinjaTrader

---

**Architecture Plan Status**: READY FOR REVIEW
**Estimated Complexity Reduction**: CYC 9 → CYC 4 (OnKeyDown)
**Jane Street Alignment**: PASS (Cognitive simplicity, testability, no illegal states)
**Lock-Free Compliance**: PASS (UI thread serialization, no locks)
