# src/ - C# Source Code Rules

**Last Updated**: 2026-06-08T22:44:00Z
**Scope**: V12 Photon Kernel trading strategy source code

---

## Recent Major Refactors

| Date | Epic | File | Change | CYC Before | CYC After |
|------|------|------|--------|------------|-----------|
| 2026-05-11 | EPIC-CCN-1 | V12_002.SIMA.Lifecycle.cs | Extracted `LinkTargetOrderToFSM()` | 71 | 41 |

**CRITICAL**: Always check this table before targeting methods for refactoring. Stale analysis causes wasted work.

---

## V12 DNA Mandates (PLATINUM STANDARD)

### 1. Lock-Free Actor Pattern
- **BANNED**: `lock(stateLock)` blocks
- **REQUIRED**: FSM/Actor `Enqueue` model or atomic primitives
- **Audit**: `grep -r "lock(" src/` must return zero matches

### 2. ASCII-Only Compliance
- **BANNED**: Unicode, emoji, curly quotes in C# string literals
- **REQUIRED**: Plain ASCII characters only
- **Audit**: `python scripts/ascii_audit.py src/`

### 3. Cyclomatic Complexity ≤ 8 (Jane Street GODMODE)
- **THRESHOLD**: CYC ≤ 8 per method
- **RATIONALE**: Microsecond-latency reasoning, exhaustive testing, race condition auditing
- **AUDIT**: `python scripts/complexity_audit.py --threshold 8`

### 4. Correctness by Construction
- **PRINCIPLE**: "Make illegal states unrepresentable"
- **IMPLEMENTATION**: Structure types/enums so compiler prevents invalid states
- **AVOID**: Runtime if/else guards for edge cases

---

## File-Specific Rules

### V12_002.cs (Main Strategy)
- **Size**: 3,000+ lines (God-file - active refactoring target)
- **Current Hotspots**: 
  - `ProcessIpcCommands` (CYC 61) - EPIC-CCN-2 target
  - `ProcessOnStateChange` (CYC 48) - EPIC-CCN-3 target
  - `ProcessOnExecutionUpdate` (CYC 48) - EPIC-CCN-4 target
- **Pattern**: Extract to `V12_002.*.cs` partial classes

### V12_002.SIMA.Lifecycle.cs
- **Purpose**: FSM lifecycle management
- **Last Refactor**: 2026-05-11 (EPIC-CCN-1)
- **Key Method**: `LinkTargetOrderToFSM()` (CYC 3)
- **Pattern**: Single-responsibility methods, CYC ≤ 8

### V12_002.Atm.cs
- **Purpose**: ATM (Automated Trade Management) logic
- **Hotspot**: `MonitorRmaProximity` (CYC 32) - EPIC-CCN-6 target
- **Pattern**: Extract proximity checks to helper methods

---

## Coding Standards

### Naming Conventions
- **Classes**: PascalCase (e.g., `V12_002`, `SIMALifecycle`)
- **Methods**: PascalCase (e.g., `LinkTargetOrderToFSM`)
- **Private Fields**: camelCase with underscore (e.g., `_stateLock`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_RETRY_COUNT`)

### Method Structure
```csharp
// GOOD: Single responsibility, CYC ≤ 8
private void LinkTargetOrderToFSM(Order targetOrder, SIMA_FSM fsm)
{
    if (targetOrder == null || fsm == null) return;
    
    fsm.TargetOrder = targetOrder;
    fsm.TargetOrderId = targetOrder.OrderId;
    
    LogDebug($"Linked target order {targetOrder.OrderId} to FSM {fsm.Id}");
}

// BAD: Multiple responsibilities, CYC > 8
private void ProcessOrder(Order order)
{
    // 50 lines of nested if/else/switch
    // CYC 25+
}
```

### Error Handling
- **Prefer**: Early returns over nested if/else
- **Log**: All error conditions with context
- **Avoid**: Silent failures

---

## Build & Deployment

### Hard Link Synchronization
**MANDATORY**: After ANY src/ modification:
```powershell
powershell -File .\deploy-sync.ps1
```

**Purpose**: Synchronizes 83 hard-linked files to NinjaTrader directory

**Verification**: F5 in NinjaTrader IDE → Check BUILD_TAG in output

---

## Testing Requirements

### Unit Tests
- **Location**: `tests/V12_Performance.Tests/`
- **Coverage**: All extracted methods must have tests
- **Pattern**: Arrange-Act-Assert

### Integration Tests
- **Method**: F5 in NinjaTrader IDE
- **Verification**: BUILD_TAG appears in output
- **Success**: No compilation errors, strategy loads

---

## Refactoring Workflow

### Before Refactoring
1. Check "Recent Major Refactors" table above
2. Run: `python scripts/verify_index_freshness.py`
3. If stale: Run `graphify update .` and `jcodemunch index_folder`

### During Refactoring
1. Extract method with CYC ≤ 8
2. Add unit test
3. Run: `dotnet build`
4. Run: `powershell -File .\deploy-sync.ps1`

### After Refactoring
1. F5 in NinjaTrader IDE
2. Verify BUILD_TAG
3. Update "Recent Major Refactors" table in this file
4. Commit with message: `[EPIC-X] ticket-Y: description -- CYC before->after [BUILD_TAG]`

---

## Common Pitfalls

### ❌ Targeting Obsolete Code
**Problem**: Refactoring code that was already refactored
**Solution**: Always check "Recent Major Refactors" table first

### ❌ Forgetting deploy-sync.ps1
**Problem**: Changes don't appear in NinjaTrader
**Solution**: Add to muscle memory - every src/ change requires sync

### ❌ Exceeding CYC Threshold
**Problem**: Extracted method still has CYC > 8
**Solution**: Extract further until all methods ≤ 8

---

## Index

**Parent**: [`../AGENTS.md`](../AGENTS.md) (root)
**Children**: None (leaf node)
**Related**: 
- [`../docs/standards/jane-street/RULES_CATALOG.md`](../docs/standards/jane-street/RULES_CATALOG.md) - Jane Street patterns
- [`../tests/AGENTS.md`](../tests/AGENTS.md) - Testing rules