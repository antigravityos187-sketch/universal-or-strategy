# Wave 3 Phase 3 Lessons Learned

**Date**: 2026-06-13
**Phase**: Phase 3 (DNA & PR Audit)
**Status**: Complete ✅
**Key Learning**: Building-Blocks Methodology Violation

---

## Critical Discovery: Architecture Bug

### The Bug

Wave 3 Phase 3 generator copied Wave 3 Phase 2 pattern (Bob Shell) instead of Wave 2 Phase 3 pattern (Claude advanced mode).

**Impact**: Wrong execution mode → Wrong output format → Missing validation

### Root Cause

**Building-Blocks Methodology Violation**: When generating phase scripts for a new wave, ALWAYS copy the SAME phase from the previous wave, NOT adjacent phases from the current wave.

---

## The Golden Rule

### ALWAYS Copy Same Phase from Previous Wave

```
✅ CORRECT:
Wave 3 Phase 3 → Copy Wave 2 Phase 3
Wave 3 Phase 4 → Copy Wave 2 Phase 4
Wave 3 Phase 5 → Copy Wave 2 Phase 5

❌ WRONG:
Wave 3 Phase 3 → Copy Wave 3 Phase 2
Wave 3 Phase 4 → Copy Wave 3 Phase 3
Wave 3 Phase 5 → Copy Wave 3 Phase 4
```

### Why This Matters

**Each phase has unique requirements**:
- Different execution modes (ask/plan/advanced/v12-engineer)
- Different command patterns
- Different output formats
- Different validation requirements

**Adjacent phases are NOT interchangeable**.

---

## Three Attempts Timeline

### First Attempt (18:04 PST) - FAILED
**Issue**: HTTP 401 authentication errors
**Root Cause**: Dummy API keys hardcoded
**Duration**: 5 minutes
**Cost**: 0 bobcoins (failed before execution)
**Fix**: Load API keys from JSON files

### Second Attempt (18:09 PST) - WRONG OUTPUT
**Issue**: Scan reports instead of audit reports
**Root Cause**: Copied Wave 3 Phase 2 (Bob Shell) instead of Wave 2 Phase 3 (Claude advanced)
**Duration**: 3 minutes execution
**Cost**: ~5.2 bobcoins (wrong output format)
**Fix**: Copy Wave 2 Phase 3 pattern exactly

### Third Attempt (18:25 PST) - SUCCESS ✅
**Result**: All 10 audit reports created (5.8K-18K files)
**Duration**: 3 minutes execution
**Cost**: ~10.93 bobcoins
**Verification**: 18:38 PST - All files confirmed

---

## Bob Shell vs Claude Advanced Mode

### Bob Shell (`/epic-scan`)
- **Purpose**: Status summary
- **Output**: Scan reports (checklist)
- **Validation**: None
- **Use Case**: "Show me current status"

### Claude Advanced Mode (`--chat-mode advanced`)
- **Purpose**: Validation gate
- **Output**: Audit reports (DNA/PR compliance)
- **Validation**: Comprehensive (8 checks)
- **Use Case**: "Is this plan safe to implement?"

### Why Phase 3 Needs Claude Advanced Mode

**Phase 3 is a GATE**: Must validate before proceeding to Phase 4.

**Required Checks**:
1. DNA compliance (lock-free, ASCII-only, Jane Street)
2. PR hygiene (diff size, whitespace, scope creep)
3. Risk assessment
4. Go/No-Go recommendation

**Bob Shell cannot perform these checks** - it only reports status.

---

## Prevention Measures

### 1. Always Reference Previous Wave

**Before generating any phase script**:
```bash
# Check what Wave 2 used for this phase
cat scripts/wave2/generate_phase{N}_scripts.py | grep "bob --"
```

### 2. Verify Against SOP

**Check these 4 things**:
- Mode (ask/plan/advanced/v12-engineer)
- Command pattern
- Output format
- Validation requirements

### 3. Test Before Full Deployment

**Always test with 1-2 epics first**:
```bash
# Generate 2 test scripts
python scripts/wave3/generate_wave3_phase{N}_scripts.py --test

# Run on VM
gcloud compute ssh v12-test-golden-v2 --command="cd universal-or-strategy && ./_p{N}_116.sh"

# Verify output format
gcloud compute ssh v12-test-golden-v2 --command="ls -lh docs/brain/EPIC-CCN-116/0{N}-*.md"

# Deploy all only after success
```

### 4. Document Deviations

**If pattern must change**:
1. Document why in `WAVE{N}_PHASE{N}_DEVIATION.md`
2. Update SOP with new pattern
3. Verify with Director before proceeding

---

## Cost Analysis

### Phase 3 Total Cost

| Attempt | Status | Bobcoins | Reason |
|---------|--------|----------|--------|
| First | Failed | 0 | HTTP 401 (dummy keys) |
| Second | Wrong | ~5.2 | Wrong output format |
| Third | Success | ~10.93 | Correct architecture |
| **Total** | **Complete** | **~16.13** | **All 10 epics** |

### Wave 3 Total (Phases 0-3)

| Phase | Bobcoins | % of Total |
|-------|----------|------------|
| Phase 0 | ~30 | 1.9% |
| Phase 1 | ~50 | 3.1% |
| Phase 2 | ~80 | 5.0% |
| Phase 3 | ~16.13 | 1.0% |
| **Total** | **~176.13** | **11.0%** |

**Remaining**: ~1,423.87 bobcoins (89%)

---

## Key Takeaways

### 1. Building-Blocks Methodology is CRITICAL

**One violation** (copying adjacent phase instead of previous wave's same phase) caused:
- 2 failed attempts
- 34 minutes debugging
- ~5.2 bobcoins wasted
- Complete architecture rewrite

**Prevention**: Always copy same phase from previous wave.

### 2. Verification Protocol Works

**Hardened protocol caught**:
- HTTP 401 immediately (first attempt)
- Wrong output format immediately (second attempt)
- Confirmed proper audit reports (third attempt)

**Lesson**: Invest in verification upfront, save debugging time later.

### 3. Phase Requirements are Unique

**Each phase has specific**:
- Execution mode
- Command pattern
- Output format
- Validation requirements

**Lesson**: Never assume adjacent phases use same pattern.

### 4. Test Before Full Deployment

**Always test with 1-2 epics first**:
- Catches issues early
- Prevents wasting bobcoins on wrong output
- Allows quick iteration

**Lesson**: 5 minutes testing saves 30 minutes debugging.

---

## Updated SOP

### Phase Script Generation (V3)

**Step 1: Copy Previous Wave's Same Phase**
```bash
cp scripts/wave{N-1}/generate_phase{X}_scripts.py scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
```

**Step 2: Update Epic Numbers Only**
```python
# Change epic numbers
API_ALLOCATION = {
    "116": "b (2).json",  # Was "107"
    "117": "b.json",      # Was "108"
    # ... etc
}
```

**Step 3: Verify Against SOP**
- Check mode matches SOP
- Check command pattern matches SOP
- Check output format matches SOP

**Step 4: Test with 2 Epics**
```bash
python scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
# Deploy 2 scripts only
# Verify output format
# Deploy all only after success
```

**Step 5: Document Any Deviations**
- If pattern must change, document why
- Update SOP with new pattern
- Verify with Director

---

## References

- **Complete Handoff**: `WAVE3_PHASE3_COMPLETE_HANDOFF.md`
- **Architecture Bug**: `WAVE3_PHASE3_ARCHITECTURE_BUG_ANALYSIS.md`
- **Scan vs Audit**: `WAVE3_PHASE3_SCAN_VS_AUDIT_EXPLANATION.md`
- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V2.md`

---

**Lesson Learned**: Building-Blocks Methodology is non-negotiable. Always copy same phase from previous wave.

**Next Phase**: Phase 4 (Ticket Generation) - Copy Wave 2 Phase 4 exactly.