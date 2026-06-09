# EPIC-CCN-13 Execution Guide

**Epic**: Extract ProcessOnStateChange (CYC 91 → ≤8)
**Target**: [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs:44)
**Status**: READY FOR EXECUTION
**Total Tickets**: 5

---

## EXECUTION ORDER

Tickets MUST be executed in this order (risk-based progression):

1. **ticket-01-extract-setdefaults.md** (LOW risk, 15 min)
2. **ticket-02-extract-terminated.md** (LOW risk, 15 min)
3. **ticket-03-extract-configure.md** (MEDIUM risk, 25 min)
4. **ticket-04-extract-dataloaded.md** (MEDIUM risk, 30 min)
5. **ticket-05-extract-realtime.md** (MEDIUM risk, 25 min)

**Total Estimated Time**: 110 minutes (~2 hours)

---

## DEPENDENCY GRAPH

```
ticket-01 (SetDefaults)
  ↓
ticket-02 (Terminated)
  ↓
ticket-03 (Configure) → creates InitializeMmioMirror
  ↓
ticket-04 (DataLoaded) → creates InitializeInstrumentSettings, InitializeTargetConfiguration
  ↓
ticket-05 (Realtime) → creates AttachUiComponents
  ↓
EPIC COMPLETE
```

**Critical Path**: All tickets are sequential (each modifies ProcessOnStateChange)

---

## TICKET SUMMARIES

### Ticket 01: Extract HandleSetDefaults (LOW)
- **Lines**: 46-127 (82 lines)
- **Target CYC**: ~15
- **Risk**: LOW (initialization only, no runtime state)
- **Duration**: 15 minutes
- **Output**: HandleSetDefaults() method

### Ticket 02: Extract HandleTerminated (LOW)
- **Lines**: 520-565 (46 lines)
- **Target CYC**: ~6
- **Risk**: LOW (cleanup only, no complex logic)
- **Duration**: 15 minutes
- **Output**: HandleTerminated() method

### Ticket 03: Extract HandleConfigure (MEDIUM)
- **Lines**: 129-267 (139 lines)
- **Target CYC**: HandleConfigure ≤8, InitializeMmioMirror ≤3
- **Risk**: MEDIUM (MMIO mirror setup, atomic operations)
- **Duration**: 25 minutes
- **Output**: HandleConfigure() + InitializeMmioMirror() methods

### Ticket 04: Extract HandleDataLoaded (MEDIUM)
- **Lines**: 269-449 (181 lines)
- **Target CYC**: HandleDataLoaded ≤8, sub-methods ≤6
- **Risk**: MEDIUM (symbol detection, target count logic)
- **Duration**: 30 minutes
- **Output**: HandleDataLoaded() + InitializeInstrumentSettings() + InitializeTargetConfiguration() methods

### Ticket 05: Extract HandleRealtime (MEDIUM)
- **Lines**: 451-518 (68 lines)
- **Target CYC**: HandleRealtime ≤8, AttachUiComponents ≤5
- **Risk**: MEDIUM (UI thread sync, dispatcher priorities)
- **Duration**: 25 minutes
- **Output**: HandleRealtime() + AttachUiComponents() methods

---

## COMPLEXITY PROGRESSION

| Ticket | ProcessOnStateChange CYC | New Methods | Total Methods |
|--------|--------------------------|-------------|---------------|
| Start  | 91                       | 0           | 1             |
| 01     | 85                       | 1           | 2             |
| 02     | 79                       | 2           | 3             |
| 03     | 64                       | 4           | 5             |
| 04     | 71                       | 7           | 8             |
| 05     | **5** ✅                 | 9           | 10            |

**Final State**: ProcessOnStateChange CYC 91 → 5 (86-point reduction)

---

## EXECUTION PROTOCOL (PER TICKET)

### Step 1: Read Ticket
- Open `docs/brain/EPIC-CCN-13/ticket-XX-*.md`
- Review objective, extraction steps, verification checklist
- Understand risk factors and critical sections

### Step 2: Execute Extraction
- Follow extraction steps exactly as documented
- Create new methods in specified order
- Replace original code with method calls
- Preserve all comments and logic

### Step 3: Verify Extraction
- Run: `python scripts/complexity_audit.py`
- Verify CYC targets met
- Check for duplicate code
- Verify indentation

### Step 4: Build Gate
- Run: `powershell -File .\scripts\build_readiness.ps1`
- Verify zero compilation errors
- Verify CSharpier formatting passes

### Step 5: V12 DNA Compliance
- Lock Audit: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
- Unicode Audit: Verify ASCII-only strings
- LOC Floor: All new methods ≥15 lines

### Step 6: F5 Gate (MANDATORY)
- Run: `powershell -File .\deploy-sync.ps1`
- Press F5 in NinjaTrader IDE
- Verify BUILD_TAG banner visible
- Verify state-specific behavior (see ticket for details)
- Type: `F5 done [BUILD_TAG]`

### Step 7: Commit
- Run: `git add src/V12_002.Lifecycle.cs`
- Run: `git commit -m "[EPIC-CCN-13] ticket-XX: [description] -- CYC [before]->[after] [BUILD_TAG]"`

### Step 8: Continue
- If more tickets remain: proceed to next ticket
- If all tickets complete: proceed to Phase 6 (PR submission)

---

## ROLLBACK PROCEDURE

If any ticket fails F5 gate:
1. Run: `git reset --hard HEAD~1`
2. Verify: ProcessOnStateChange restored to previous state
3. Document failure in `docs/brain/EPIC-CCN-13/ticket-XX-failure.md`
4. Escalate to Director
5. DO NOT proceed to next ticket

---

## CRITICAL SAFETY RULES

### V12 DNA Mandates
- ❌ **ZERO** new `lock()` statements
- ❌ **ZERO** Unicode/emoji in string literals
- ✅ **ALL** new methods ≥15 LOC (extraction floor)
- ✅ **ALL** methods CYC ≤15 (Jane Street GODMODE)

### Extraction Rules
- ✅ **PRESERVE** all comments
- ✅ **PRESERVE** all logic (no refactoring)
- ✅ **PRESERVE** all variable names
- ✅ **PRESERVE** all whitespace patterns
- ❌ **NO** "while we're here" improvements
- ❌ **NO** scope creep

### F5 Gate Rules
- ✅ **MANDATORY** for every ticket
- ✅ **BLOCKING** (cannot proceed without pass)
- ✅ **MANUAL** (Director must press F5)
- ✅ **VERIFIED** (BUILD_TAG must be visible)

---

## EPIC COMPLETION CRITERIA

### Code Metrics
- [ ] ProcessOnStateChange CYC: 91 → 5 ✅
- [ ] All 5 state handlers created
- [ ] All 4 sub-methods created
- [ ] Total methods: 10 (1 modified + 9 new)
- [ ] All methods CYC ≤15

### V12 DNA Compliance
- [ ] Zero locks in V12_002.Lifecycle.cs
- [ ] Zero Unicode in string literals
- [ ] All new methods ≥15 LOC
- [ ] deploy-sync.ps1 passes

### Build & Test
- [ ] Build passes (zero errors)
- [ ] CSharpier formatting passes
- [ ] F5 verification passes (all 5 tickets)
- [ ] All state transitions work correctly

### Git Hygiene
- [ ] 5 commits (one per ticket)
- [ ] All commits have BUILD_TAG
- [ ] Branch rebased on origin/main
- [ ] PR hygiene script passes

---

## POST-EXECUTION CHECKLIST

After all 5 tickets complete:

### Phase 6: PR Submission
- [ ] Run: `git fetch origin main && git rebase origin/main`
- [ ] Run: `gh pr create --title "[EPIC-CCN-13] Extract ProcessOnStateChange (CYC 91→5)" --body "..." --label "epic-run"`
- [ ] Extract PR number from output
- [ ] Run: `/pr-loop <PR_NUMBER>` to achieve PHS 100/100

### Final Verification
- [ ] Run: `python scripts/complexity_audit.py`
- [ ] Verify: ProcessOnStateChange CYC = 5
- [ ] Verify: All handlers CYC ≤15
- [ ] Verify: All sub-methods CYC ≤8
- [ ] Run: `powershell -File .\scripts\pre_push_validation.ps1`
- [ ] Verify: All 13 checks PASS

### Epic Completion Report
- [ ] Document final metrics in `docs/brain/EPIC-CCN-13/COMPLETION_REPORT.md`
- [ ] Update hotspot queue (remove ProcessOnStateChange)
- [ ] Archive epic documents
- [ ] Celebrate 86-point CYC reduction! 🎉

---

## EXPECTED FINAL STATE

### ProcessOnStateChange (FINAL)
```csharp
private void ProcessOnStateChange(State state)
{
    if (state == State.SetDefaults)
    {
        HandleSetDefaults();
    }
    else if (state == State.Configure)
    {
        HandleConfigure();
    }
    else if (state == State.DataLoaded)
    {
        HandleDataLoaded();
    }
    else if (state == State.Realtime)
    {
        HandleRealtime();
    }
    else if (state == State.Terminated)
    {
        HandleTerminated();
    }
}
```

**Lines**: ~15 (down from 522)
**CYC**: 5 (down from 91)
**Role**: Simple dispatcher (Jane Street GODMODE achieved)

### New Methods Created (9 total)

**State Handlers (5)**:
1. HandleSetDefaults() - CYC ~15
2. HandleConfigure() - CYC ~8
3. HandleDataLoaded() - CYC ~8
4. HandleRealtime() - CYC ~8
5. HandleTerminated() - CYC ~6

**Sub-Methods (4)**:
1. InitializeMmioMirror() - CYC ~3
2. InitializeInstrumentSettings() - CYC ~6
3. InitializeTargetConfiguration() - CYC ~6
4. AttachUiComponents() - CYC ~5

---

## TROUBLESHOOTING

### Build Failures
- Check for missing braces (CSharpier should catch)
- Check for typos in method names
- Check for missing semicolons
- Run: `dotnet build` for detailed errors

### F5 Failures
- Check NinjaTrader console for exceptions
- Verify BUILD_TAG matches commit
- Check for missing method calls
- Verify state transitions work

### Complexity Audit Failures
- Re-run: `python scripts/complexity_audit.py`
- Check if sub-extraction needed
- Verify method boundaries correct
- Escalate to Director if stuck

### Git Issues
- Check branch is clean: `git status`
- Check rebase conflicts: `git rebase --abort` then retry
- Check commit messages have BUILD_TAG
- Run: `powershell -File .\scripts\verify_pr_hygiene.ps1`

---

## CONTACT

**Epic Owner**: V12 Epic Orchestrator
**Execution Mode**: v12-engineer
**Verification Mode**: Advanced
**Director**: Human oversight at F5 gates

---

**Status**: READY FOR EXECUTION
**Next Step**: Execute ticket-01-extract-setdefaults.md
**Estimated Completion**: 2026-06-09 03:30 UTC (2 hours from now)