# Phase 0 Hotspot Analysis -- EPIC-IPC-MODE-CYC11

**Agent Name**: v12-phase0-hotspot
**Execution Time**: Phase 0 cold-start
**MCP Evidence**: jcodemunch (resolve_repo + search_symbols + get_file_content) + complexity_audit.py

---

## Target Method

| Field | Value |
|-------|-------|
| Method | `SetMode_ActivateModeFlags` |
| File | [`src/V12_002.UI.IPC.Commands.Mode.cs`](../../src/V12_002.UI.IPC.Commands.Mode.cs:139) |
| Line | 139 |
| LOC | 30 |
| CYC Confirmed | **11** |
| Audit Tool | `scripts/complexity_audit.py` |
| Status | REFACTOR |

---

## MCP Evidence

### jcodemunch resolve_repo
- Repo: `local/universal-or-strategy-17657650`
- Symbol count: 2435 | File count: 280
- Index status: indexed

### get_file_content (lines 139-177)
Method body retrieved verbatim -- full source confirmed at line 139.

### complexity_audit.py output
```
| SetMode_ActivateModeFlags | 30 | 11 | | REFACTOR |
  - V12_002.UI.IPC.Commands.Mode.cs::SetMode_ActivateModeFlags (CYC=11, LOC=30)
```

---

## Sequential Thinking Evidence

### Thought 1 -- CYC Manual Calculation
McCabe CYC computed from source (lines 139-177):

| Decision Point | Count | +CYC |
|----------------|-------|------|
| Base | 1 | +1 |
| `||` operators in OR-chain (4 binary ops) | 4 | +4 |
| `if (!isKnownMode)` | 1 | +1 |
| `switch` cases ("RMA", "RETEST", "TREND", "MOMO", "FFMA") | 5 | +5 |
| **Total** | | **11** |

### Thought 2 -- Fix Strategy
- HashSet replaces OR-chain: eliminates all 4 `||` branch points (-4 CYC)
- switch expression replaces switch statement: +1 instead of +5 (-4 CYC)
- Projected new CYC = 1 (base) + 1 (if) + 1 (switch expr) = **3**

### Thought 3 -- Readiness
All findings confirmed. Artifact ready to write.

---

## Root Cause Breakdown

### Source (lines 139-177)

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

### Note on Stale Comment
The comment says `CYC=7` -- this is stale from the original extraction estimate.
The actual implementation has CYC=11 due to:
1. The OR-chain contributing +4 (4 binary `||` operators)
2. The switch statement contributing +5 (5 cases)

---

## Fix Strategy Summary

### Step 1 -- Replace OR-chain with HashSet (-4 CYC)

```csharp
private static readonly HashSet<string> _knownModes = new HashSet<string>(StringComparer.Ordinal)
{
    "RMA", "RETEST", "TREND", "MOMO", "FFMA"
};
```

Replace:
```csharp
bool isKnownMode = newMode == "RMA" || newMode == "RETEST" || newMode == "TREND" || newMode == "MOMO" || newMode == "FFMA";
if (!isKnownMode) { ... }
```

With:
```csharp
if (!_knownModes.Contains(newMode)) { ... }
```

CYC impact: -4 (eliminates all 4 `||` branch points)

### Step 2 -- Replace switch statement with switch expression (-4 CYC)

Per OKF `complexity-reduction.md`: switch expression adds CYC+1 (exhaustive coverage) vs switch statement CYC+N.

```csharp
_ = newMode switch
{
    "RMA"    => ActivateRmaMode(),
    "RETEST" => SetRetestMode(),
    "TREND"  => SetTrendMode(),
    "MOMO"   => (object)ActivateMOMOMode(),
    "FFMA"   => SetFfmaMode(),
    _        => (object)null
};
```

Or simpler -- extract to an `ApplyModeFlag(string newMode)` helper with 5 minimal operations.

### Projected CYC After Fix
| Signal | Before | After |
|--------|--------|-------|
| Base | 1 | 1 |
| OR-chain (HashSet) | +4 | +0 |
| if guard | +1 | +1 |
| switch/expr | +5 | +1 |
| **Total** | **11** | **3** |

Target CYC <= 8 will be met. Target CYC = 3.

---

## Jane Street OKF Alignment

| Rule | Application |
|------|-------------|
| `complexity-reduction.md` CYC<=8 | Current CYC=11 violates; fix brings to 3 |
| Lookup table / HashSet dispatch | Replace OR-chain with `_knownModes.Contains()` |
| switch expression over statement | Replace switch statement for CYC+1 savings |
| OKF sidecar_lifecycle | Allowlist check BEFORE state mutation (already correct; must preserve) |
