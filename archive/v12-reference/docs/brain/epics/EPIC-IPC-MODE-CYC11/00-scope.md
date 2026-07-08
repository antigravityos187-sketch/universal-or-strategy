# Phase 1 Scope Definition -- EPIC-IPC-MODE-CYC11

**Agent**: v12-phase1-scope
**Phase**: 1 -- Scope Definition
**Input**: `docs/brain/EPIC-IPC-MODE-CYC11/00-hotspots.md`
**MCP Evidence**: jcodemunch (list_repos, get_file_outline, search_symbols, find_references, get_file_content) + Sequential Thinking (4 thoughts)

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Repo | `local/universal-or-strategy-17657650` |
| jCodemunch calls | `list_repos`, `get_file_outline`, `search_symbols` x2, `find_references`, `get_file_content` x2 |
| Sequential Thinking | 4 thoughts -- scope boundary validation complete |
| Source confirmation | Line 139 definition confirmed via `grep` + `read_file` |
| Caller analysis | `find_references` -> 0 cross-file callers; 1 same-file caller at line 132 |

---

## Files IN Scope

| File | Change Type | Reason |
|------|-------------|--------|
| [`src/V12_002.UI.IPC.Commands.Mode.cs`](../../src/V12_002.UI.IPC.Commands.Mode.cs) | MODIFY | Contains `SetMode_ActivateModeFlags` (target method) + will receive new `_knownModes` field |

**Total files in scope: 1**

---

## Methods IN Scope

### Primary Target -- MODIFY

| Field | Value |
|-------|-------|
| Method | [`SetMode_ActivateModeFlags`](../../src/V12_002.UI.IPC.Commands.Mode.cs:139) |
| Line range | 139 -- 177 |
| LOC | 39 (definition to closing brace) |
| CYC before | **11** |
| CYC after (projected) | **3** |
| Caller | `TryHandleMode_SetMode` at line 132 (same file, same class) |
| Cross-file callers | **0** (confirmed by `find_references`) |

### New Field -- ADD to same file

| Field | Value |
|-------|-------|
| Symbol | `_knownModes` |
| Type | `private static readonly HashSet<string>` |
| Location | Class-level field in `src/V12_002.UI.IPC.Commands.Mode.cs` |
| Initialization | `new HashSet<string>(StringComparer.Ordinal) { "RMA", "RETEST", "TREND", "MOMO", "FFMA" }` |
| OKF basis | `lock-free-patterns.md`: static readonly collections are safe -- immutable after init, zero coordination needed |

### Optional Helper -- ADD to same file (if switch expression requires)

| Field | Value |
|-------|-------|
| Condition | Only if the switch expression pattern requires a statement-returning helper |
| Location | Private method in `src/V12_002.UI.IPC.Commands.Mode.cs` |
| Max CYC | 1 (single switch expression, CYC+1 per OKF) |
| Naming | `ApplyModeFlag(string newMode)` or similar -- PascalCase, no underscores in name |
| Status | Optional -- not required if switch expression is self-contained |

---

## Changes Planned

### Change 1 -- Add `_knownModes` static readonly field

```csharp
// OKF lock-free-patterns.md: static readonly = safe, immutable after init, no lock() needed
private static readonly HashSet<string> _knownModes = new HashSet<string>(StringComparer.Ordinal)
{
    "RMA", "RETEST", "TREND", "MOMO", "FFMA"
};
```

CYC impact: 0 (field declaration, no branching)

### Change 2 -- Replace OR-chain with HashSet lookup (-4 CYC)

**Before** (CYC contributes +4 from 4x `||`):
```csharp
bool isKnownMode =
    newMode == "RMA" || newMode == "RETEST" || newMode == "TREND" || newMode == "MOMO" || newMode == "FFMA";
if (!isKnownMode) { ... }
```

**After** (CYC contributes +1 from single `if`):
```csharp
if (!_knownModes.Contains(newMode))
{
    Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'");
    return false;
}
```

CYC delta: -4 (eliminates 4 binary `||` branch points)

### Change 3 -- Replace switch statement with switch expression (-4 CYC)

**Before** (switch statement: CYC +5, one per case):
```csharp
switch (newMode)
{
    case "RMA":
        isRMAModeActive = true;
        isRMAButtonClicked = true;
        break;
    case "RETEST":
        isRetestModeActive = true;
        break;
    case "TREND":
        isTRENDModeActive = true;
        break;
    case "MOMO":
        ActivateMOMOMode();
        break;
    case "FFMA":
        isFFMAModeArmed = true;
        break;
}
```

**After** (switch expression: CYC +1 per OKF `complexity-reduction.md`):
Implementation approach will be confirmed in Phase 2 Architecture Planning.
Two valid patterns:
- Pattern A: Inline switch expression with `Action` dispatch via lambda
- Pattern B: Extract `ApplyModeFlag(string)` helper carrying the switch expression body

Both patterns remain within the SAME FILE. Final pattern selected in Phase 2.

CYC delta: -4 (switch statement +5 cases replaced by switch expression +1)

---

## CYC Reduction Summary

| Signal | Before | After |
|--------|--------|-------|
| Base | +1 | +1 |
| OR-chain (4x `\|\|`) | +4 | +0 (HashSet.Contains) |
| `if (!isKnownMode)` guard | +1 | +1 (preserved -- sidecar_lifecycle) |
| switch statement (5 cases) | +5 | +1 (switch expression) |
| **Total** | **11** | **3** |

Target CYC <= 8: **MET** (projected 3, confirmed well under limit)

---

## Files EXPLICITLY Out of Scope

| File | Reason Excluded |
|------|----------------|
| `src/V12_002.UI.IPC.Commands.Mode.cs` line 121 -- `TryHandleMode_SetMode` | CALLER is not modified; signature of callee unchanged |
| `src/V12_002.Entries.MOMO.cs` -- `ActivateMOMOMode()` | Existing method called by switch case; not modified |
| `src/V12_002.UI.IPC.cs` -- `AllowedIpcActions` | Separate allowlist (action-level); not mode-level; unrelated |
| `src/V12_002.IPC.Hardening.cs` -- `ValidIpcActions` | Separate hardening list; unrelated to mode flag logic |
| `src/V12_002.UI.IPC.Commands.Misc.cs` -- `ToggleStrategyMode_SetFlags` | Different command handler; unrelated |
| All other `src/` files | No dependency on `SetMode_ActivateModeFlags` (0 cross-file references confirmed) |
| `tests/` directory | Test creation is Phase 4/5 ticket scope; not Phase 1 scope |
| `docs/` directory (other than this file) | Documentation only; no code changes |
| `scripts/` directory | Tooling only; not modified |

---

## OKF Constraints Applied

| Rule | Source Document | Application |
|------|----------------|-------------|
| `static readonly` collections are safe | `lock-free-patterns.md` | `_knownModes` is `static readonly` -- immutable after init, zero locking needed |
| `lock()` BANNED | `lock-free-patterns.md` | No `lock()` introduced anywhere. The HashSet field requires none. |
| CYC <= 8 per method | `complexity-reduction.md` | Current CYC=11 violates; fix targets CYC=3 |
| HashSet / lookup table dispatch | `complexity-reduction.md` | OR-chain replaced by `_knownModes.Contains(newMode)` |
| switch expression over statement | `complexity-reduction.md` | switch statement (CYC+5) replaced by switch expression (CYC+1) |
| sidecar_lifecycle: allowlist BEFORE state mutation | `how-to-build-an-exchange.md` | Guard clause preserved -- unknown mode rejected before ANY flag is cleared |
| ASCII-only | ASCII rule | All strings, comments, identifiers must remain ASCII; no em-dashes, curly quotes, Unicode |
| xUnit [Fact] only | `testing-strategies.md` | Any tests added in Phase 5 must use xUnit; NUnit/MSTest banned |
| No `DateTime.Now` | `how-to-build-an-exchange.md` | Not applicable to this method; no time comparison |
| Method naming PascalCase | Naming convention | Any new helpers use PascalCase, no underscores in method names |

---

## Behavioral Invariants (Must Preserve)

These behaviors are NOT changed by the refactor:

1. **Unknown mode rejection**: Any mode string not in `{ "RMA", "RETEST", "TREND", "MOMO", "FFMA" }` must return `false` with a `Print` rejection message.
2. **Guard-before-mutation**: The allowlist check fires BEFORE any flag is cleared (OKF sidecar_lifecycle).
3. **ATOMIC clear-all**: All 6 mode flags (`isRMAModeActive`, `isRMAButtonClicked`, `isRetestModeActive`, `isTRENDModeActive`, `isMOMOModeActive`, `isFFMAModeArmed`) are reset to `false` before the new mode is activated.
4. **Return values**: Returns `false` on rejection, `true` on success -- callee `TryHandleMode_SetMode` at line 132 depends on this.
5. **ActivateMOMOMode() call preserved**: The "MOMO" case must still call `ActivateMOMOMode()` -- do not inline or bypass.
6. **RMA dual-flag**: The "RMA" case must set BOTH `isRMAModeActive = true` AND `isRMAButtonClicked = true`.

---

## Scope Boundary Diagram

```
src/V12_002.UI.IPC.Commands.Mode.cs  [1 FILE -- IN SCOPE]
  |
  +-- class V12_002 (partial)
        |
        +-- _knownModes [NEW FIELD -- HashSet<string>, static readonly]
        |
        +-- TryHandleMode_SetMode() [line 121 -- NOT MODIFIED, calls target at line 132]
        |     |
        |     +---> SetMode_ActivateModeFlags(newMode)  [TARGET -- MODIFIED]
        |
        +-- SetMode_ActivateModeFlags() [line 139 -- PRIMARY TARGET]
        |     Changes:
        |       - OR-chain replaced by _knownModes.Contains()
        |       - switch statement replaced by switch expression
        |       - Behavioral invariants preserved
        |
        +-- ApplyModeFlag() [OPTIONAL NEW HELPER -- same file, if needed]

External files: ALL OUT OF SCOPE (0 cross-file callers confirmed)
```

---

## Scope Validation Checklist

| Check | Status |
|-------|--------|
| Target method confirmed at line 139 via `grep` | PASS |
| CYC=11 confirmed via `complexity_audit.py` (Phase 0) | PASS |
| Cross-file caller count = 0 (via `find_references`) | PASS |
| Method signature unchanged (callers unaffected) | PASS |
| OKF `lock-free-patterns.md` -- static readonly HashSet is safe | PASS |
| OKF `complexity-reduction.md` -- CYC target 3 <= 8 | PASS |
| sidecar_lifecycle guard preserved in planned fix | PASS |
| Scope limited to 1 file | PASS |
| No scope creep to other files | PASS |
| ASCII-only constraint documented | PASS |
