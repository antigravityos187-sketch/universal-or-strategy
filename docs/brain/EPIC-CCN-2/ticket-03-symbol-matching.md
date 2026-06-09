---
# TICKET EPIC-CCN-2-03: Extract IsCommandForThisChart()
# Epic: EPIC-CCN-2
# Sequence: 3 of 5
# Depends on: ticket-02-global-command.md
---

## Objective
Extract the symbol matching logic (lines 332-350) from [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) into a dedicated `IsCommandForThisChart()` method using guard clauses and local functions to achieve CYC 6.

## Scope
IN scope:
- **File**: [`src/V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- **Target Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260)
- **Extraction Range**: Lines 332-350 (symbol matching boolean expression)
- **New Method**: `IsCommandForThisChart()` (to be created below `IsGlobalCommand()`)

OUT of scope:
- Previously extracted methods (unchanged)
- Logging logic (handled in ticket-04)
- FSM enqueue logic (unchanged)

## Context References
- **Analysis**: [`docs/brain/EPIC-CCN-2/01-analysis.md`](01-analysis.md) -- Section "Risk Hotspots" (lines 79-88)
- **Approach**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Extraction 3: IsCommandForThisChart()" (lines 336-437)
- **Incremental Plan**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Step 3: Extract IsCommandForThisChart()" (lines 603-624)

## Implementation Instructions

### 1. Create New Method with Guard Clauses and Local Functions
Add this method immediately after `IsGlobalCommand()`:

```csharp
private bool IsCommandForThisChart(
    string action,
    string targetSymbol,
    bool isGlobalCommand,
    out string mySym
)
{
    // CYC 6 (guard clauses + symbol matching)
    mySym = Instrument.MasterInstrument.Name.ToUpperInvariant();
    string target = targetSymbol.Trim().ToUpperInvariant();
    
    // Early exit for global commands (CYC 1)
    if (isGlobalCommand) return true;
    
    // Early exit for broadcast targets (CYC 3)
    if (target == "GLOBAL" || target == "ALL") return true;
    
    // Mode toggle keywords (CYC 1)
    if (IsModeKeyword(target)) return true;
    
    // Symbol matching (CYC 1)
    return MatchesSymbol(mySym, target);
    
    // Local functions (not extracted - avoid 15-LOC floor)
    bool IsModeKeyword(string t) => // CYC 6
        t == "ON" || t == "OFF" || t == "RMA"
        || t == "ORB" || t == "OR" || t == "MOMO";
    
    bool MatchesSymbol(string my, string tgt) // CYC 8
    {
        string myFull = Instrument.FullName.ToUpperInvariant();
        return my == tgt
            || my.StartsWith(tgt)
            || tgt.StartsWith(my)
            || myFull.Contains(tgt)
            || (tgt == "MES" && my.Contains("ES"))
            || (tgt == "MYM" && my.Contains("YM"))
            || (tgt == "MGC" && my.Contains("GC"));
    }
}
```

### 2. Symbol Matching Rules (Preserve Exactly)
**Exact Match**: `my == tgt` (e.g., "ES" == "ES")
**Prefix Match**: `my.StartsWith(tgt)` (e.g., "ES 03-25" starts with "ES")
**Reverse Prefix**: `tgt.StartsWith(my)` (e.g., target "ES 03-25" starts with "ES")
**Full Name Contains**: `myFull.Contains(tgt)` (e.g., "E-mini S&P 500" contains "ES")
**Micro Contracts**: Special mappings for micro futures
- MES → ES (E-mini S&P 500)
- MYM → YM (E-mini Dow)
- MGC → GC (Gold)

### 3. Update Call Site in ProcessIpcCommands()
Replace the original boolean expression (lines 332-350) with:

```csharp
bool isForMe = IsCommandForThisChart(action, targetSymbol, isGlobalCommand, out string mySym);
```

**Note**: The `out string mySym` parameter is required because `mySym` is used in the logging call downstream.

### 4. Preserve Exact Behavior
**CRITICAL**: This is a pure structural refactoring. Do NOT change:
- Symbol normalization (ToUpperInvariant, Trim)
- Matching rule order or logic
- Global command bypass semantics
- Mode keyword list (ON, OFF, RMA, ORB, OR, MOMO)
- Micro contract mappings (MES, MYM, MGC)

### 5. Guard Clauses Strategy (Jane Street Pattern)
**Why Guard Clauses?**
- Early exits reduce nesting depth (from 6 to 2)
- Each condition is independently testable
- Cognitive load reduced: "if not this, then not that, else check this"
- Aligns with Jane Street GODMODE principle: simple, verifiable logic

**Why Local Functions?**
- Avoids 15-LOC extraction floor violation
- Groups related logic (mode keywords, symbol matching)
- `complexity_audit.py` will report CYC 6 for parent method (local functions inlined)

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Method >= 15 LOC (including local functions: ~35 lines)
- [ ] Residual `ProcessIpcCommands()` CYC target: ~25 (down from ~45)
- [ ] New `IsCommandForThisChart()` CYC target: 6 (verified by complexity_audit.py)
- [ ] No logic drift -- pure structural movement only

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] `IsCommandForThisChart()` method created in [`V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- [ ] Method uses guard clauses for early exits (global, broadcast, mode keywords)
- [ ] Method uses local functions (IsModeKeyword, MatchesSymbol)
- [ ] All 10+ symbol matching rules preserved (exact, prefix, reverse, contains, micro mappings)
- [ ] `out string mySym` parameter correctly populated for downstream logging
- [ ] Call site in `ProcessIpcCommands()` updated to single-line method call
- [ ] `deploy-sync.ps1` ASCII gate: PASS
- [ ] `complexity_audit.py` shows:
  - `ProcessIpcCommands()`: CYC reduced (target ~25)
  - `IsCommandForThisChart()`: CYC = 6
- [ ] `lock()` audit: ZERO matches
- [ ] Compile: `dotnet build` succeeds
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible
- [ ] Test: Multi-chart session (ES, GC, YM), send symbol-specific commands
- [ ] Test: Verify MES→ES, MYM→YM, MGC→GC micro contract routing works
- [ ] Test: Send command with target "GLOBAL", verify all charts receive it