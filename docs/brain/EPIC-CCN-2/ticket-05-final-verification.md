---
# TICKET EPIC-CCN-2-05: Final Verification and BUILD_TAG Bump
# Epic: EPIC-CCN-2
# Sequence: 5 of 5
# Depends on: ticket-04-logging.md
---

## Objective
Verify that all 4 extractions have successfully reduced [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) to CYC ≤ 8, all V12 DNA constraints are satisfied, and bump BUILD_TAG to mark epic completion.

## Scope
IN scope:
- **Verification**: Run full complexity audit on all 5 methods
- **Testing**: Execute comprehensive IPC command test suite
- **BUILD_TAG**: Bump version in [`src/V12_002.cs`](../../../src/V12_002.cs)
- **Hard-Link Sync**: Final `deploy-sync.ps1` execution

OUT of scope:
- No code changes (verification only)
- No new extractions (epic complete)

## Context References
- **Analysis**: [`docs/brain/EPIC-CCN-2/01-analysis.md`](01-analysis.md) -- Section "Success Criteria" (lines 163-189)
- **Approach**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Step 5: Final Verification" (lines 646-672)
- **Approach**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Success Criteria" (lines 859-898)

## Verification Checklist

### 1. Complexity Audit (Primary Gate)
Run full complexity audit and verify all methods meet targets:

```powershell
python scripts/complexity_audit.py
```

**Expected Results**:
| Method | Target CYC | Status |
|--------|------------|--------|
| `ProcessIpcCommands()` | ≤ 8 | ✅ PASS |
| `ValidateIpcCommandSyntax()` | ≤ 8 | ✅ PASS |
| `IsGlobalCommand()` | 4 | ✅ PASS |
| `IsCommandForThisChart()` | 6 | ✅ PASS |
| `LogIpcCommandReceived()` | ≤ 3 | ✅ PASS |

**Acceptance**: ALL methods must meet or beat target CYC. If any method exceeds target, STOP and report to Director.

### 2. Extraction Floor Verification
Verify all extracted methods meet 15-LOC minimum:

```powershell
# Manual inspection of each method in V12_002.UI.IPC.cs
# Count lines from opening brace to closing brace (inclusive)
```

**Expected Results**:
| Method | Min LOC | Actual LOC | Status |
|--------|---------|------------|--------|
| `ValidateIpcCommandSyntax()` | 15 | ~35 | ✅ PASS |
| `IsGlobalCommand()` | 15 | ~25 | ✅ PASS |
| `IsCommandForThisChart()` | 15 | ~35 | ✅ PASS |
| `LogIpcCommandReceived()` | 15 | ~15 | ✅ PASS |

**Note**: Local functions count toward parent method LOC.

### 3. Lock-Free Verification (Zero Tolerance)
Verify no new `lock()` statements introduced:

```powershell
grep -r "lock(" src/
```

**Expected Result**: ZERO matches in [`V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)

**Acceptance**: If ANY `lock()` found, STOP and report to Director.

### 4. ASCII-Only Compliance (Zero Tolerance)
Verify no non-ASCII characters in string literals:

```powershell
grep -Prn "[^\x00-\x7F]" src/
```

**Expected Result**: ZERO matches in [`V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)

**Acceptance**: If ANY non-ASCII found, STOP and report to Director.

### 5. Compilation Gate (Blocking)
Verify clean build:

```powershell
dotnet build
```

**Expected Result**: 0 errors, 0 warnings

**Acceptance**: If build fails, STOP and report to Director.

### 6. Runtime Gate (Blocking)
Verify strategy loads and runs:

1. Press F5 in NinjaTrader
2. Verify BUILD_TAG banner appears in output window
3. Verify strategy loads without errors
4. Verify no runtime exceptions in first 60 seconds

**Acceptance**: If strategy fails to load or throws exceptions, STOP and report to Director.

### 7. IPC Command Test Suite (Blocking)
Execute comprehensive IPC command tests via Remote App UI:

**Test 1: Validation**
- Send malformed command (missing parts)
- **Expected**: Rejection message in log
- **Verify**: `ValidateIpcCommandSyntax()` working

**Test 2: Global Command Classification**
- Send `TOGGLE_ACCOUNT` command
- **Expected**: Command reaches all chart instances
- **Verify**: `IsGlobalCommand()` working

**Test 3: Symbol Matching**
- Multi-chart session (ES, GC, YM)
- Send `FLATTEN|ES` command
- **Expected**: Only ES chart responds
- **Verify**: `IsCommandForThisChart()` working

**Test 4: Micro Contract Routing**
- Load MES chart (E-mini S&P 500 micro)
- Send `FLATTEN|MES` command
- **Expected**: MES chart responds (MES→ES mapping)
- **Verify**: Micro contract logic preserved

**Test 5: Logging Format**
- Send any command
- **Expected**: Log format unchanged, `[GLOBAL CMD]` suffix for global commands
- **Verify**: `LogIpcCommandReceived()` working

**Acceptance**: ALL 5 tests must pass. If any test fails, STOP and report to Director.

### 8. BUILD_TAG Bump (Mandatory)
Update BUILD_TAG in [`src/V12_002.cs`](../../../src/V12_002.cs):

1. Locate `BUILD_TAG` constant (around line 50)
2. Increment version (e.g., "V12.002.123" → "V12.002.124")
3. Add comment: `// EPIC-CCN-2: ProcessIpcCommands complexity reduction (CYC 76 → 8)`

**Example**:
```csharp
private const string BUILD_TAG = "V12.002.124"; // EPIC-CCN-2: ProcessIpcCommands complexity reduction (CYC 76 → 8)
```

### 9. Hard-Link Sync (Final)
Execute final hard-link synchronization:

```powershell
powershell -File .\deploy-sync.ps1
```

**Expected Result**: ASCII gate PASS, hard links synchronized

**Acceptance**: If deploy-sync fails, STOP and report to Director.

## Success Criteria Summary

### Functional Correctness
- [ ] All IPC commands work (TOGGLE_ACCOUNT, SET_SIMA, FLATTEN, etc.)
- [ ] Symbol matching logic unchanged (MGC→GC, MES→ES, etc.)
- [ ] Validation pipeline unchanged (syntax, timestamp, hardening, allowlist)
- [ ] Diagnostic logging output identical
- [ ] Queue drain behavior unchanged

### Complexity Reduction
- [ ] `ProcessIpcCommands()`: CYC ≤ 8 (down from 76)
- [ ] `ValidateIpcCommandSyntax()`: CYC ≤ 8
- [ ] `IsGlobalCommand()`: CYC = 4
- [ ] `IsCommandForThisChart()`: CYC = 6
- [ ] `LogIpcCommandReceived()`: CYC ≤ 3
- [ ] All extracted methods ≥ 15 LOC

### V12 DNA Compliance
- [ ] Zero new `lock()` statements
- [ ] ASCII-only in all string literals
- [ ] `deploy-sync.ps1` executed successfully
- [ ] BUILD_TAG bumped and verified in NinjaTrader output

### Testing
- [ ] F5 in NinjaTrader compiles without errors
- [ ] Strategy loads and runs
- [ ] All 5 IPC command tests pass
- [ ] `complexity_audit.py` confirms all CYC targets met

## Rollback Plan
If ANY verification step fails:

1. **Identify Failure Point**: Document which verification step failed
2. **Assess Impact**: Determine if issue is in current ticket or previous ticket
3. **Revert Strategy**:
   - If issue in ticket-04: Revert ticket-04 commit only
   - If issue in ticket-03: Revert tickets 03-04
   - If issue in ticket-02: Revert tickets 02-04
   - If issue in ticket-01: Revert entire epic (all 4 tickets)
4. **Report to Director**: Provide failure analysis and recommended fix

## Post-Verification Actions
Once ALL verification steps pass:

1. **Commit Changes**: Commit BUILD_TAG bump with message:
   ```
   EPIC-CCN-2 COMPLETE: ProcessIpcCommands complexity reduction (CYC 76 → 8)
   
   - Extracted ValidateIpcCommandSyntax() (CYC ≤8)
   - Extracted IsGlobalCommand() (CYC 4)
   - Extracted IsCommandForThisChart() (CYC 6)
   - Extracted LogIpcCommandReceived() (CYC ≤3)
   - Residual ProcessIpcCommands() (CYC ≤8)
   
   All V12 DNA constraints satisfied.
   ```

2. **Update Epic Status**: Mark EPIC-CCN-2 as COMPLETE in epic tracking

3. **Report to Director**: Present final complexity audit results and test outcomes

## Acceptance Criteria
- [ ] All 9 verification steps completed successfully
- [ ] BUILD_TAG bumped in [`V12_002.cs`](../../../src/V12_002.cs)
- [ ] Final `deploy-sync.ps1` executed successfully
- [ ] All 5 methods verified via `complexity_audit.py`
- [ ] All 5 IPC command tests passed
- [ ] Epic marked as COMPLETE
- [ ] Director approval received