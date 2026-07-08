# Phase 2 Architecture Plan -- EPIC-IPC-MODE-CYC11

**Agent**: v12-phase2-architecture
**Phase**: 2 -- Architecture Planning
**Input**: `docs/brain/EPIC-IPC-MODE-CYC11/01-scope-boundary.md`
**Output**: `docs/brain/EPIC-IPC-MODE-CYC11/02-architecture-plan.md`
**MCP Evidence**: jcodemunch (get_file_content), Sequential Thinking (4 thoughts)

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Repo | `local/universal-or-strategy-17657650` (jCodemunch) |
| Target file | `src/V12_002.UI.IPC.Commands.Mode.cs` |
| Target method | `SetMode_ActivateModeFlags` (line 139) |
| Source lines read | 1-185 (full method context + file header) |
| Sequential Thinking | 4 thoughts -- Option A vs B analysis, field placement, OKF compliance, final decision |
| Decision | **Option A: Replace OR-chain only** |
| OKF docs consulted | `lock-free-patterns.md`, `complexity-reduction.md` |

---

## Architecture Decision: Option A (OR-chain replacement only)

### Selected: Option A -- Replace OR-chain with HashSet.Contains

**Rejected: Option B** (convert switch statement to switch expression)

### Rationale for Rejecting Option B

The switch statement cases contain **imperative side effects** (not expressions that return values):
- `"RMA"` case: two sequential assignments (`isRMAModeActive = true; isRMAButtonClicked = true;`)
- `"MOMO"` case: void method call (`ActivateMOMOMode();`)

C# switch expression arms require a single expression, not statements. Converting to a switch
expression would require extracting 5 new private helper methods (`ApplyRmaFlags()`,
`ApplyRetestFlag()`, etc.) -- this increases method count and LOC with no architectural benefit
beyond the CYC reduction already achieved by Option A.

OKF `complexity-reduction.md`: "Extract guard clauses (early returns, flat structure)" and
"Named private helper methods -- single concern" -- but only when needed. Option A achieves
the CYC<=8 target WITHOUT requiring new helpers.

**OKF rule** ("Always prefer switch expression") applies to enum dispatch where arms
naturally return values. For pure side-effect dispatch, the switch statement is the correct form.

### Option A CYC Result: 7 (within <=8 target)

```
CYC Before: 1 (base) + 4 (OR-chain ||) + 1 (if !isKnownMode) + 5 (switch cases) = 11
CYC After:  1 (base) + 0 (HashSet.Contains) + 1 (if !Contains) + 5 (switch cases) = 7
```

---

## Changes Required

### Change 1 -- Add `_knownModes` Static Readonly HashSet Field

**Location**: Insert immediately before line 138 (the comment preceding `SetMode_ActivateModeFlags`)

**Code to insert** (lines to add before line 138):

```csharp
        // OKF lock-free-patterns.md: static readonly = immutable after init, zero coordination needed
        private static readonly HashSet<string> _knownModes = new HashSet<string>(StringComparer.Ordinal)
        {
            "RMA", "RETEST", "TREND", "MOMO", "FFMA"
        };

```

**Notes**:
- `StringComparer.Ordinal`: case-sensitive O(1) lookup. `newMode` is already
  normalized to uppercase at line 128 (`parts[1].Trim().ToUpperInvariant()`), so
  `Ordinal` is equivalent to `OrdinalIgnoreCase` on this input but is more explicit.
- `static readonly`: CLR guarantees thread-safe type initialization. Read-only after init.
  No `lock()` needed. Follows `AllowedIpcActions` idiom in `src/V12_002.UI.IPC.cs`.
- `private`: invisible outside the declaring class. No cross-file blast radius.

---

### Change 2 -- Replace OR-chain with HashSet.Contains

**Location**: Lines 141-144 in `SetMode_ActivateModeFlags`

**BEFORE** (lines 141-148, exact verbatim):

```csharp
            // OKF sidecar_lifecycle: reject unknown modes BEFORE any state mutation
            bool isKnownMode =
                newMode == "RMA" || newMode == "RETEST" || newMode == "TREND" || newMode == "MOMO" || newMode == "FFMA";
            if (!isKnownMode)
            {
                Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'");
                return false;
            }
```

**AFTER** (exact replacement):

```csharp
            // OKF sidecar_lifecycle: reject unknown modes BEFORE any state mutation
            if (!_knownModes.Contains(newMode))
            {
                Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'");
                return false;
            }
```

**CYC delta**: -4 (removes 4 binary `||` branch points; `HashSet.Contains` is O(1), no branch)

**Behavioral equivalence**: 100% identical. The guard rejects unknown modes before any state
mutation. The `bool isKnownMode` intermediate variable is eliminated; the if condition directly
inverts the Contains result. The `Print + return false` body is unchanged.

---

### Change 3 -- Update Stale CYC Comment

**Location**: Line 138 (the comment above `SetMode_ActivateModeFlags`)

**BEFORE**:
```csharp
        // [EPIC-W7-OVERRUN] Extracted: ATOMIC clear-all + set the incoming mode flag (CYC=7)
```

**AFTER**:
```csharp
        // [EPIC-W7-OVERRUN] Extracted: ATOMIC clear-all + set the incoming mode flag (CYC=7 post-EPIC-IPC-MODE-CYC11)
```

**Rationale**: The comment originally said CYC=7 (the extraction target) but the actual
implementation was CYC=11 due to the OR-chain not being extracted. After this fix, CYC=7
becomes accurate. The annotation documents when it was corrected.

---

## Full Method: Before and After

### BEFORE (lines 138-177, verbatim from source)

```csharp
        // [EPIC-W7-OVERRUN] Extracted: ATOMIC clear-all + set the incoming mode flag (CYC=7)
        private bool SetMode_ActivateModeFlags(string newMode)
        {
            // OKF sidecar_lifecycle: reject unknown modes BEFORE any state mutation
            bool isKnownMode =
                newMode == "RMA" || newMode == "RETEST" || newMode == "TREND" || newMode == "MOMO" || newMode == "FFMA";
            if (!isKnownMode)
            {
                Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'");
                return false;
            }

            // ATOMIC mode transition: clear all flags, then set the new mode
            isRMAModeActive = false;
            isRMAButtonClicked = false;
            isRetestModeActive = false;
            isTRENDModeActive = false;
            isMOMOModeActive = false;
            isFFMAModeArmed = false;
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
            return true;
        }
```

### AFTER (full method, post-refactor)

```csharp
        // OKF lock-free-patterns.md: static readonly = immutable after init, zero coordination needed
        private static readonly HashSet<string> _knownModes = new HashSet<string>(StringComparer.Ordinal)
        {
            "RMA", "RETEST", "TREND", "MOMO", "FFMA"
        };

        // [EPIC-W7-OVERRUN] Extracted: ATOMIC clear-all + set the incoming mode flag (CYC=7 post-EPIC-IPC-MODE-CYC11)
        private bool SetMode_ActivateModeFlags(string newMode)
        {
            // OKF sidecar_lifecycle: reject unknown modes BEFORE any state mutation
            if (!_knownModes.Contains(newMode))
            {
                Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'");
                return false;
            }

            // ATOMIC mode transition: clear all flags, then set the new mode
            isRMAModeActive = false;
            isRMAButtonClicked = false;
            isRetestModeActive = false;
            isTRENDModeActive = false;
            isMOMOModeActive = false;
            isFFMAModeArmed = false;
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
            return true;
        }
```

---

## CYC Calculation (Before and After)

| Decision Point | Before | After |
|----------------|--------|-------|
| Base | +1 | +1 |
| `||` operators in OR-chain | +4 | +0 (HashSet.Contains -- no branch) |
| `if (!isKnownMode)` / `if (!Contains)` | +1 | +1 |
| `switch` case "RMA" | +1 | +1 |
| `switch` case "RETEST" | +1 | +1 |
| `switch` case "TREND" | +1 | +1 |
| `switch` case "MOMO" | +1 | +1 |
| `switch` case "FFMA" | +1 | +1 |
| **Total CYC** | **11** | **7** |

**Target**: CYC <= 8. **Result**: 7. **Delta**: -4. Target met with 1 unit margin.

---

## OKF Compliance Notes

| Rule | Status | Evidence |
|------|--------|---------|
| `lock()` BANNED | COMPLIANT | No lock anywhere. `_knownModes` is static readonly -- CLR type-init is lock-free. |
| `lock-free-patterns.md`: "static readonly collections are safe (immutable after init)" | COMPLIANT | `_knownModes` matches this exact pattern. |
| CYC <= 8 | COMPLIANT | CYC = 7 after change (was 11). |
| ASCII only | COMPLIANT | All string literals ("RMA", "RETEST", "TREND", "MOMO", "FFMA") and comments are ASCII. |
| `_camelCase` for private fields | COMPLIANT | `_knownModes` follows the file's convention (`_modeProfiles` pattern in same file). |
| `complexity-reduction.md`: "Lookup table / HashSet dispatch (replaces switch+N cases)" | COMPLIANT | OR-chain replaced by HashSet.Contains. |
| `sidecar_lifecycle`: "Allowlist check BEFORE rate limiter / state mutation" | COMPLIANT | Guard (`if (!_knownModes.Contains)`) remains first statement, before all flag mutations. |
| `how-to-build-an-exchange.md`: behavior-preserving refactor | COMPLIANT | Switch statement unchanged. ActivateMOMOMode() call preserved. Return values unchanged. |
| xUnit only (if tests written) | N/A for plan | Phase 4 tickets will specify xUnit [Fact] test requirements. |

---

## Risk Assessment

| Risk | Likelihood | Severity | Mitigation |
|------|-----------|----------|-----------|
| Behavioral regression in `SetMode_ActivateModeFlags` | VERY LOW | HIGH | OR-chain and HashSet.Contains are logically identical for the 5 known modes + all unknowns |
| `_knownModes` field name collision | NONE | N/A | grep confirmed no existing `_knownModes` in codebase |
| CLR static init race condition | NONE | N/A | CLR guarantees thread-safe type initialization for static readonly fields |
| Partial class visibility issue | NONE | N/A | `_knownModes` is `private` -- visible only within V12_002 class, consistent across partial files |
| `newMode` encoding edge case | NONE | N/A | `newMode = parts[1].Trim().ToUpperInvariant()` at line 128 -- always uppercase ASCII before reaching guard |
| CYC tool measurement discrepancy | LOW | LOW | McCabe calculation verified manually. Switch statement accounts for all 5 cases. Result is 7. |

**Overall risk**: VERY LOW. This is a pure internal refactor with 0 external callers confirmed.

---

## Files in Scope

| File | Change |
|------|--------|
| [`src/V12_002.UI.IPC.Commands.Mode.cs`](../../src/V12_002.UI.IPC.Commands.Mode.cs:138) | Add field (6 lines), replace OR-chain (4 lines -> 1 line), update comment |

**No other files modified.**

---

## Validation Checklist (for Phase 5 Executor)

- [ ] `_knownModes` field inserted before `SetMode_ActivateModeFlags` method
- [ ] OR-chain (lines 142-143) replaced by `if (!_knownModes.Contains(newMode))`
- [ ] `bool isKnownMode` intermediate variable eliminated
- [ ] `if (!isKnownMode)` guard replaced by `if (!_knownModes.Contains(newMode))`
- [ ] Switch statement body UNCHANGED (lines 157-175)
- [ ] `ActivateMOMOMode()` call preserved in MOMO case
- [ ] RMA dual-flag (`isRMAModeActive`, `isRMAButtonClicked`) preserved in RMA case
- [ ] Comment on line 138 updated to reference EPIC-IPC-MODE-CYC11
- [ ] `dotnet build` passes (zero errors)
- [ ] `scripts/complexity_audit.py` reports CYC=7 for `SetMode_ActivateModeFlags`
- [ ] `grep -r "lock(" src/` returns 0 matches
- [ ] `grep -r "_knownModes" src/` returns exactly 2 matches (field decl + Contains call)

---

## Phase 4 Ticket Sketch

**Ticket 1: HashSet field + OR-chain replacement**
- File: `src/V12_002.UI.IPC.Commands.Mode.cs`
- Insert `_knownModes` HashSet field before line 138
- Replace OR-chain at lines 141-144 with `if (!_knownModes.Contains(newMode))`
- Update stale comment at line 138
- Verify: build passes, CYC audit shows 7

**No additional tickets needed.** Single self-contained change, single file, single method.
