# EPIC-CCN-13 Ticket 01: Extract HandleSetDefaults

**Epic**: EPIC-CCN-13 - Extract ProcessOnStateChange (CYC 91 → ≤8)
**Ticket**: 01 of 5
**Risk Level**: LOW
**Estimated Duration**: 15 minutes
**Target CYC**: ~15 (acceptable for pure initialization)

---

## OBJECTIVE

Extract the `State.SetDefaults` branch from ProcessOnStateChange into a dedicated `HandleSetDefaults()` method. This is pure property initialization with no branching logic, making it the lowest-risk extraction in the epic.

---

## CURRENT STATE

**File**: [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs:44)
**Method**: `ProcessOnStateChange(State state)`
**Lines**: 46-127 (82 lines)
**Current CYC**: 91 (entire method)

**Target Section**:
```csharp
if (state == State.SetDefaults)
{
    _configureComplete = false;
    _dataLoadedComplete = false;
    Interlocked.Exchange(ref _startupReadinessLogEmitted, 0);
    ResetTelemetry();
    Description = "Universal OR Strategy V12.12 - Build " + BUILD_TAG;
    Name = "V12_002";
    Calculate = Calculate.OnPriceChange;
    // ... 60+ property assignments ...
}
```

---

## TARGET STATE

**New Method Signature**:
```csharp
private void HandleSetDefaults()
```

**Modified ProcessOnStateChange**:
```csharp
private void ProcessOnStateChange(State state)
{
    if (state == State.SetDefaults)
    {
        HandleSetDefaults();
    }
    else if (state == State.Configure)
    {
        // ... existing code ...
    }
    // ... other branches unchanged ...
}
```

---

## EXTRACTION STEPS

### Step 1: Create HandleSetDefaults Method
1. Position: Insert new method immediately after `ProcessOnStateChange` (after line 565)
2. Copy lines 46-127 (SetDefaults branch body) into new method
3. Remove the opening `if (state == State.SetDefaults) {` and closing `}`
4. Verify method signature: `private void HandleSetDefaults()`

### Step 2: Replace Original Branch
1. In `ProcessOnStateChange`, replace lines 46-127 with:
```csharp
if (state == State.SetDefaults)
{
    HandleSetDefaults();
}
```

### Step 3: Verify Extraction
1. **Line Count**: HandleSetDefaults should be ~82 lines
2. **No Logic Changes**: Exact copy of original code
3. **Indentation**: Adjust for method body (remove one level)
4. **Braces**: Verify all braces match

---

## COMPLEXITY ESTIMATE

**Before**:
- ProcessOnStateChange: CYC 91

**After**:
- ProcessOnStateChange: CYC 86 (one branch extracted)
- HandleSetDefaults: CYC ~15 (pure initialization, acceptable)

---

## VERIFICATION CHECKLIST

### Pre-Extraction
- [ ] Read current ProcessOnStateChange source
- [ ] Identify lines 46-127 (SetDefaults branch)
- [ ] Verify no nested conditionals in target section

### Post-Extraction
- [ ] HandleSetDefaults exists at correct location
- [ ] HandleSetDefaults contains exact copy of lines 46-127
- [ ] ProcessOnStateChange calls HandleSetDefaults()
- [ ] No duplicate code remains
- [ ] Indentation is correct

### Complexity Audit
- [ ] Run: `python scripts/complexity_audit.py`
- [ ] Verify: ProcessOnStateChange CYC reduced
- [ ] Verify: HandleSetDefaults CYC ~15

### Build Gate
- [ ] Run: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Verify: Zero compilation errors
- [ ] Verify: CSharpier formatting passes

### V12 DNA Compliance
- [ ] Lock Audit: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
- [ ] Unicode Audit: Verify ASCII-only in all string literals
- [ ] LOC Floor: HandleSetDefaults ≥15 lines ✅ (82 lines)

### F5 Gate (MANDATORY)
- [ ] Run: `powershell -File .\deploy-sync.ps1`
- [ ] Press F5 in NinjaTrader IDE
- [ ] Verify: BUILD_TAG banner visible
- [ ] Type: `F5 done [BUILD_TAG]`

### Commit
- [ ] Run: `git add src/V12_002.Lifecycle.cs`
- [ ] Run: `git commit -m "[EPIC-CCN-13] ticket-01: extract HandleSetDefaults -- CYC 91->86 [BUILD_TAG]"`

---

## RISK ASSESSMENT

**Risk Level**: LOW

**Why Low Risk**:
- Pure property initialization (no branching logic)
- No conditionals or loops
- No shared state mutations (except flags)
- Straightforward copy-paste extraction

**Potential Issues**:
- None identified

---

## ROLLBACK PROCEDURE

If F5 gate fails:
1. Run: `git reset --hard HEAD~1`
2. Verify: ProcessOnStateChange restored to original state
3. Report failure in `docs/brain/EPIC-CCN-13/ticket-01-failure.md`
4. Escalate to Director

---

## SUCCESS CRITERIA

- [ ] HandleSetDefaults method created
- [ ] ProcessOnStateChange calls HandleSetDefaults()
- [ ] CYC reduced: 91 → 86
- [ ] HandleSetDefaults CYC ~15 (acceptable)
- [ ] Build passes
- [ ] F5 verification passes
- [ ] Zero locks, zero Unicode
- [ ] Commit created with BUILD_TAG

---

## DEPENDENCIES

**Upstream**: None (first ticket)
**Downstream**: None (independent extraction)

---

## NOTES

- This is the warm-up ticket (lowest risk)
- Sets the pattern for remaining tickets
- HandleSetDefaults CYC ~15 is acceptable per Jane Street for pure initialization
- No sub-method extraction needed

---

**Status**: READY FOR EXECUTION
**Assigned To**: v12-engineer mode
**Estimated Time**: 15 minutes