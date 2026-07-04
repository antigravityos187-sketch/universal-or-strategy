# src/ - C# Source Code Rules

**Last Updated**: 2026-07-02
**Scope**: V12 Photon Kernel trading strategy source code

---

## Recent Major Refactors (Wave 7)

| Epic | File | Method | CYC Before | CYC After | Date |
|------|------|--------|------------|-----------|------|
| EPIC-W7-018 | V12_002.Orders.Callbacks.cs | (see completion report) | >8 | <=8 | 2026-07 |
| EPIC-W7-019 | V12_002.Orders.Callbacks.Execution.cs | (see completion report) | >8 | <=8 | 2026-07 |
| EPIC-W7-028 | V12_002.Orders.Management.cs | (see completion report) | >8 | <=8 | 2026-07 |
| EPIC-W7-031 | V12_002.BarUpdate.cs | (see completion report) | >8 | <=8 | 2026-07 |
| EPIC-W7-035 | V12_002.Entries.OR.cs | (see completion report) | >8 | <=8 | 2026-07 |

> Full wave 7 history: `docs/brain/EPIC-W7-*/05-completion-report.md`
> Full epic roadmap: `epic_roadmap.json`

**CRITICAL**: Always check this table and `epic_roadmap.json` before targeting methods for
refactoring. Stale analysis causes duplicated or conflicting work.

---

## V12 DNA Mandates (PLATINUM STANDARD)

### 1. Lock-Free Actor Pattern
- **BANNED**: `lock(stateLock)` blocks in any form
- **REQUIRED**: FSM/Actor `Enqueue` model or atomic primitives
- **Scan**: `grep -r "lock(" src/` must return zero matches at all times

### 2. ASCII-Only Compliance
- **BANNED**: Unicode, emoji, curly quotes in C# string literals
- **Scan**: `python scripts/ascii_audit.py src/`

### 3. Cyclomatic Complexity <= 8 (Jane Street GODMODE)
- **Threshold**: CYC <= 8 per method (Jane Street strict standard)
- **Why**: Microsecond-latency reasoning, exhaustive testing, race condition auditing,
  and DSB micro-op cache fit (methods >8 overflow the CPU instruction cache)
- **Scan**: `python scripts/complexity_audit.py --threshold 8`

### 4. Correctness by Construction
- "Make illegal states unrepresentable"
- Structure types/enums so the compiler prevents invalid states
- Avoid runtime if/else guards for edge cases

---

## File-Specific Notes

### V12_002.cs (Main Strategy)
- God-file  -- active Wave 7 refactoring target
- All extracted methods go to `V12_002.*.cs` partial classes
- Pattern: one partial class per concern (`BarUpdate`, `Entries.*`, `Orders.*`, etc.)

### Partial Class Naming
- `V12_002.BarUpdate.cs`  -- bar processing
- `V12_002.Entries.*.cs`  -- entry signal families (OR, FFMA, MOMO, Retest, Trend)
- `V12_002.Orders.*.cs`  -- order lifecycle (Callbacks, Management, etc.)
- `V12_002.UI.*.cs`  -- panel, IPC, sizing, snapshot
- `V12_002.SIMA.*.cs`  -- SIMA FSM lifecycle and fleet
- `V12_002.REAPER.*.cs`  -- audit and repair
- `V12_002.Safety.*.cs`  -- watchdog and compliance

---

## Coding Standards

### Naming Conventions
- **Classes**: PascalCase (`V12_002`, `SIMALifecycle`)
- **Methods**: PascalCase (`LinkTargetOrderToFSM`)
- **Private fields**: camelCase with underscore prefix (`_stateLock`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_RETRY_COUNT`)
- **Build Tags**: Increment in `V12_002.Properties.cs` for every production delivery
- **Prefixes**: `V12_001` (Panel), `V12_002` (Strategy)

### Method Structure
```csharp
// GOOD: single responsibility, CYC <= 8, early returns
private void LinkTargetOrderToFSM(Order targetOrder, SIMA_FSM fsm)
{
    if (targetOrder == null || fsm == null) return;
    fsm.TargetOrder = targetOrder;
    fsm.TargetOrderId = targetOrder.OrderId;
    LogDebug($"Linked target order {targetOrder.OrderId} to FSM {fsm.Id}");
}

// BAD: multiple responsibilities, CYC > 8, nested conditions
```

### Error Handling
- Prefer early returns over nested if/else
- Log all error conditions with ASCII-only context strings
- No silent failures

---

## Build & Deployment

### Hard Link Synchronization (MANDATORY after ANY src/ edit)
```powershell
powershell -File .\deploy-sync.ps1
```
Synchronizes 83 hard-linked files to NinjaTrader Strategies directory.
Verification: F5 in NinjaTrader IDE -> check BUILD_TAG in output.

---

## Testing Requirements

- **Framework**: xUnit ONLY  -- never NUnit or MSTest
- **Location**: `tests/V12_Performance.Tests/`
- **Pattern**: Arrange-Act-Assert with `[Fact]` attribute
- **Coverage**: All Wave 7 extracted methods must have xUnit tests
- **Reference**: `docs/intel/jane-street/testing-strategies.md`

---

## Refactoring Workflow (Wave 7)

### Before Refactoring
1. Check "Recent Major Refactors" table above
2. Check `epic_roadmap.json` for the target method's epic assignment
3. Verify jCodemunch index is current: `mcp__jcodemunch-mcp__resolve_repo`

### During Refactoring
1. Extract method with CYC <= 8
2. Write xUnit `[Fact]` test for the extracted method
3. Run: `dotnet build`
4. Run: `powershell -File .\deploy-sync.ps1`

### After Refactoring
1. F5 in NinjaTrader IDE  -- verify BUILD_TAG
2. Update "Recent Major Refactors" table in this file
3. Commit: `[EPIC-W7-NNN] ticket-N: extract MethodName CYC before->after [BUILD_TAG]`

---

## Common Pitfalls

### Targeting already-refactored code
Check `epic_roadmap.json` and completion reports first.

### Forgetting deploy-sync.ps1
File-edit tools create new inodes, silently breaking hard links.
Every `src/` change requires sync  -- no exceptions.

### Exceeding CYC threshold after extraction
If the extracted method still exceeds CYC 8, extract further.
One method, one responsibility.

---

## Index

**Parent**: [`../AGENTS.md`](../AGENTS.md) (root)
**Children**: None (leaf node)
**Related**:
- [`../docs/intel/jane-street/complexity-reduction.md`](../docs/intel/jane-street/complexity-reduction.md)
- [`../tests/AGENTS.md`](../tests/AGENTS.md)  -- Testing rules
