# Phase 4 Tickets -- EPIC-IPC-MODE-CYC11

**Agent**: v12-phase4-tickets
**Phase**: 4 -- Ticket Generation
**Input**: `docs/brain/EPIC-IPC-MODE-CYC11/02-architecture-plan.md`
**Output**: `docs/brain/EPIC-IPC-MODE-CYC11/04-tickets.md`
**MCP Evidence**: Sequential Thinking (3 thoughts -- scope decomposition, verbatim code verification, PR gate compliance)

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Source file verified | `src/V12_002.UI.IPC.Commands.Mode.cs` lines 138-177 (read live) |
| Architecture decision | Option A: Replace OR-chain with HashSet.Contains |
| CYC before | 11 |
| CYC after | 7 |
| Ticket count | **1** (3 changes are inseparable -- same file, same method context) |
| Sequential Thinking | 3 thoughts -- decomposition check, verbatim match, PR gate compliance |
| PR gate | COMPLIANT -- last step is `git push --no-verify`, no `gh pr create` |

---

## Ticket Count Rationale

All 3 changes must be in a single ticket:
- Adding `_knownModes` without the `Contains` call = unused dead code (compile warning)
- Using `_knownModes.Contains` without the field = compile error
- Comment update is cosmetic, belongs in the same commit
- Splitting creates intermediate broken states -- forbidden

**Verdict: 1 ticket is the minimum correct decomposition.**

---

## T1: HashSet Replace + Method Body Edit

| Field | Value |
|-------|-------|
| Ticket ID | T1 |
| Epic | EPIC-IPC-MODE-CYC11 |
| File | `src/V12_002.UI.IPC.Commands.Mode.cs` |
| Method | `SetMode_ActivateModeFlags` (line 139) |
| Branch | `wave7/epic-ipc-mode-cyc11` (create off `main` at `63a1e76d`) |
| Mode | `v12-engineer` |
| CYC change | 11 -> 7 |
| Risk | VERY LOW -- 0 external callers, pure internal refactor |

### Summary

Replace the 4-operator OR-chain in `SetMode_ActivateModeFlags` with a `static readonly HashSet<string>` field lookup. Add the `_knownModes` field before the method, replace the `bool isKnownMode` OR-chain with `if (!_knownModes.Contains(newMode))`, and update the stale CYC comment.

---

### BEFORE -- Exact Current Code (verbatim from file, lines 138-177)

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

---

### AFTER -- Full Method Post-Refactor

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

### Surgical Edit Plan (3 apply_diff operations)

**Edit 1 -- Insert `_knownModes` field before line 138**

Insert the following 6 lines immediately before line 138 (the `[EPIC-W7-OVERRUN]` comment):

```csharp
        // OKF lock-free-patterns.md: static readonly = immutable after init, zero coordination needed
        private static readonly HashSet<string> _knownModes = new HashSet<string>(StringComparer.Ordinal)
        {
            "RMA", "RETEST", "TREND", "MOMO", "FFMA"
        };

```

**Edit 2 -- Update stale CYC comment on line 138**

Search:
```
        // [EPIC-W7-OVERRUN] Extracted: ATOMIC clear-all + set the incoming mode flag (CYC=7)
```

Replace:
```
        // [EPIC-W7-OVERRUN] Extracted: ATOMIC clear-all + set the incoming mode flag (CYC=7 post-EPIC-IPC-MODE-CYC11)
```

**Edit 3 -- Replace OR-chain guard (lines 141-148 in original numbering)**

Search (exact block including surrounding comment):
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

Replace:
```csharp
            // OKF sidecar_lifecycle: reject unknown modes BEFORE any state mutation
            if (!_knownModes.Contains(newMode))
            {
                Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'");
                return false;
            }
```

**INVARIANT**: The `switch` statement body (cases RMA/RETEST/TREND/MOMO/FFMA) and `return true;` at the end of the method MUST remain bit-for-bit identical.

---

### Execution Steps for v12-engineer

```
Step 1:  git fetch origin main
Step 2:  git checkout -b wave7/epic-ipc-mode-cyc11 63a1e76d
Step 3:  Apply Edit 1 (insert _knownModes field before line 138)
Step 4:  Apply Edit 2 (update CYC comment)
Step 5:  Apply Edit 3 (replace OR-chain with HashSet.Contains)
Step 6:  python scripts/complexity_audit.py
         -- GATE: SetMode_ActivateModeFlags must show CYC <= 8 (expected: 7)
Step 7:  grep -r "lock(" src/
         -- GATE: must return 0 results
Step 8:  dotnet csharpier check src/
         -- If FAIL: dotnet csharpier format src/ then re-check
Step 9:  powershell -File .\scripts\build_readiness.ps1
         -- GATE: zero compilation errors (pre-existing test errors are out of scope)
Step 10: powershell -File .\deploy-sync.ps1
         -- MANDATORY: sync hard links to NT8 (per AGENTS.md Section 2)
Step 11: git add src/V12_002.UI.IPC.Commands.Mode.cs
Step 12: git commit -m "refactor(ipc-mode): replace OR-chain with HashSet lookup, CYC 11->7 [EPIC-IPC-MODE-CYC11]"
Step 13: git push --no-verify origin wave7/epic-ipc-mode-cyc11
```

> **PR GATE (V12.35)**: Steps end at push. There is NO `gh pr create` step.
> PR creation is exclusively owned by v12-phase6-review after final sign-off.

---

### Verification Checklist

- [ ] `_knownModes` field inserted immediately before the `SetMode_ActivateModeFlags` method comment
- [ ] Field is `private static readonly HashSet<string>` with `StringComparer.Ordinal`
- [ ] Field contains exactly 5 entries: "RMA", "RETEST", "TREND", "MOMO", "FFMA" (ASCII, uppercase)
- [ ] `bool isKnownMode` variable eliminated from method body
- [ ] `if (!isKnownMode)` replaced by `if (!_knownModes.Contains(newMode))`
- [ ] `Print($"[V12 IPC REJECT] SET_MODE rejected: unknown mode '{newMode}'")` preserved verbatim
- [ ] `return false;` after rejected guard preserved
- [ ] Switch statement body UNCHANGED -- all 5 cases identical to before
- [ ] `ActivateMOMOMode()` call in MOMO case preserved
- [ ] RMA dual-flag (`isRMAModeActive = true; isRMAButtonClicked = true;`) preserved
- [ ] Comment updated: "(CYC=7)" -> "(CYC=7 post-EPIC-IPC-MODE-CYC11)"
- [ ] `grep -r "_knownModes" src/` returns exactly 2 matches (field declaration + Contains call)
- [ ] `grep -r "lock(" src/` returns 0 matches
- [ ] `python scripts/complexity_audit.py` reports CYC <= 8 for `SetMode_ActivateModeFlags`
- [ ] `dotnet csharpier check src/` passes
- [ ] `powershell -File .\scripts\build_readiness.ps1` passes
- [ ] `powershell -File .\deploy-sync.ps1` executes without error
- [ ] Branch `wave7/epic-ipc-mode-cyc11` pushed with `--no-verify`

---

### Acceptance Criteria

| Criterion | Target | Measurement |
|-----------|--------|-------------|
| CYC of `SetMode_ActivateModeFlags` | <= 7 | `python scripts/complexity_audit.py` |
| `lock(` in src/ | 0 matches | `grep -r "lock(" src/` |
| Build | Zero errors | `build_readiness.ps1` |
| Formatting | PASS | `dotnet csharpier check src/` |
| Behavioral regression | None | Switch body unchanged; Print+return false preserved |
| `_knownModes` field references | Exactly 2 | `grep -r "_knownModes" src/` |

**Epic done when**: `complexity_audit.py` confirms CYC <= 7 AND build passes.

---

## OKF Compliance

| Rule | Status |
|------|--------|
| `lock()` BANNED | COMPLIANT -- `static readonly` is CLR-guaranteed lock-free init |
| CYC <= 8 | COMPLIANT -- CYC = 7 after change |
| ASCII-only | COMPLIANT -- all identifiers and string literals are ASCII |
| `_camelCase` private fields | COMPLIANT -- `_knownModes` follows `_modeProfiles` convention in same file |
| `sidecar_lifecycle`: allowlist check before state mutation | COMPLIANT -- guard remains first statement |
| Behavior-preserving refactor | COMPLIANT -- switch body and all return values unchanged |
| `lock-free-patterns.md`: "static readonly collections are safe" | COMPLIANT -- exact match |
| No `gh pr create` in ticket (V12.35) | COMPLIANT -- last step is `git push --no-verify` |
