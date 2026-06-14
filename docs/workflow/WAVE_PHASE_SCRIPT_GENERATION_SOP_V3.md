# Wave Phase Script Generation SOP V3

**Version**: 3.0
**Date**: 2026-06-13
**Status**: MANDATORY
**Supersedes**: V2.0

---

## Critical Update (V3)

**New Rule**: ALWAYS copy the SAME phase from the PREVIOUS wave, NOT adjacent phases from the current wave.

**Violation Discovered**: Wave 3 Phase 3 copied Wave 3 Phase 2 (wrong) instead of Wave 2 Phase 3 (correct).

**Impact**: Wrong execution mode → Wrong output format → 2 failed attempts → 34 minutes debugging → ~5.2 bobcoins wasted.

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

## Standard Operating Procedure

### Step 1: Copy Previous Wave's Same Phase

**Command**:
```bash
cp scripts/wave{N-1}/generate_phase{X}_scripts.py scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
```

**Example** (Wave 3 Phase 4):
```bash
cp scripts/wave2/generate_phase4_scripts.py scripts/wave3/generate_wave3_phase4_scripts.py
```

**DO NOT**:
- Copy adjacent phase from current wave
- Generate from scratch
- Assume patterns are similar

### Step 2: Update Epic Numbers ONLY

**Change ONLY these lines**:
```python
# Epic to API key mapping (Wave 3: CCN-116 through CCN-125)
API_ALLOCATION = {
    "116": "b (2).json",  # Was "107"
    "117": "b.json",      # Was "108"
    "118": "bob (1).json", # Was "109"
    # ... etc
}
```

**DO NOT change**:
- Mode (ask/plan/advanced/v12-engineer)
- Command pattern
- Output format
- Validation requirements

### Step 3: Verify Against SOP

**Check these 4 things**:

1. **Mode matches SOP**:
   ```bash
   grep "chat-mode" scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
   ```

2. **Command pattern matches SOP**:
   ```bash
   grep "bob --" scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
   ```

3. **Output format matches SOP**:
   ```bash
   grep "0{X}-" scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
   ```

4. **Validation requirements match SOP**:
   - Check prompt includes required checks
   - Check manifest update logic
   - Check bobcoin reporting

### Step 4: Test with 2 Epics

**Generate test scripts**:
```bash
python scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
```

**Deploy 2 scripts only**:
```bash
gcloud compute scp _p{X}_116.sh _p{X}_117.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
```

**Run on VM**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ./_p{X}_116.sh"
```

**Verify output format**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh docs/brain/EPIC-CCN-116/0{X}-*.md"
```

**Deploy all only after success**.

### Step 5: Document Any Deviations

**If pattern must change**:
1. Create `WAVE{N}_PHASE{X}_DEVIATION.md`
2. Document why pattern changed
3. Update this SOP with new pattern
4. Verify with Director before proceeding

---

## Phase-Specific Requirements

### Phase 0 (Hotspot Analysis)
- **Mode**: `ask`
- **Command**: `bob --yolo --chat-mode ask "$(cat /tmp/phase0_msg_X.txt)"`
- **Output**: `00-hotspots.md`, `manifest.json`
- **Validation**: jCodemunch hotspot data

### Phase 1 (Scope Definition)
- **Mode**: `plan`
- **Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_X.txt)"`
- **Output**: `00-scope.md`
- **Validation**: Single-method boundary

### Phase 2 (Architecture Planning)
- **Mode**: `plan`
- **Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_X.txt)"`
- **Output**: `02-architecture-plan.md`, `02-diagrams.mmd`
- **Validation**: Jane Street alignment

### Phase 3 (DNA & PR Audit)
- **Mode**: `advanced`
- **Command**: `bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_X.txt)"`
- **Output**: `03-audit-report.md`
- **Validation**: DNA compliance, PR hygiene

### Phase 4 (Ticket Generation)
- **Mode**: `plan`
- **Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase4_msg_X.txt)"`
- **Output**: `04-tickets.md`
- **Validation**: Ticket breakdown, execution order

### Phase 5 (Ticket Execution)
- **Mode**: `v12-engineer`
- **Command**: `bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_X.txt)"`
- **Output**: `ticket-X-completion.md`
- **Validation**: Build passes, tests pass

### Phase 6 (Final Review)
- **Mode**: `advanced`
- **Command**: `bob --yolo --chat-mode advanced "$(cat /tmp/phase6_msg_X.txt)"`
- **Output**: `05-completion-report.md`
- **Validation**: All tickets verified, roadmap updated

---

## Common Mistakes

### Mistake 1: Copying Adjacent Phase

**Wrong**:
```bash
# Copying Wave 3 Phase 2 for Wave 3 Phase 3
cp scripts/wave3/generate_wave3_phase2_scripts.py scripts/wave3/generate_wave3_phase3_scripts.py
```

**Right**:
```bash
# Copying Wave 2 Phase 3 for Wave 3 Phase 3
cp scripts/wave2/generate_phase3_scripts.py scripts/wave3/generate_wave3_phase3_scripts.py
```

### Mistake 2: Changing Mode

**Wrong**:
```python
# Changing mode from 'advanced' to 'plan'
bob --yolo --chat-mode plan "$(cat /tmp/phase3_msg_X.txt)"
```

**Right**:
```python
# Keeping mode as 'advanced' (from Wave 2 Phase 3)
bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_X.txt)"
```

### Mistake 3: Skipping Test

**Wrong**:
```bash
# Deploying all 10 scripts without testing
gcloud compute scp _p3_*.sh v12-test-golden-v2:~/universal-or-strategy/
```

**Right**:
```bash
# Testing 2 scripts first
gcloud compute scp _p3_116.sh _p3_117.sh v12-test-golden-v2:~/universal-or-strategy/
# Verify output format
# Deploy all only after success
```

---

## Verification Checklist

Before deploying any phase scripts, verify:

- [ ] Copied from previous wave's SAME phase (not adjacent phase)
- [ ] Updated epic numbers only (107-115 → 116-125)
- [ ] Mode matches SOP (ask/plan/advanced/v12-engineer)
- [ ] Command pattern matches SOP
- [ ] Output format matches SOP (0X-*.md)
- [ ] Validation requirements match SOP
- [ ] Tested with 2 epics first
- [ ] Output format verified
- [ ] Ready to deploy all

---

## Recovery Procedure

If wrong output format detected:

1. **STOP immediately** - Do not deploy remaining scripts
2. **Identify root cause** - Check which phase was copied
3. **Create corrected generator** - Copy correct phase from previous wave
4. **Test with 2 epics** - Verify output format
5. **Deploy all** - Only after success
6. **Document failure** - Update lessons learned

---

## Success Metrics

### Per Phase
- ✅ All scripts generated without errors
- ✅ All scripts use correct mode
- ✅ All scripts produce correct output format
- ✅ All scripts complete within budget
- ✅ All APIs remain positive

### Per Wave
- ✅ All phases follow SOP
- ✅ No architecture bugs
- ✅ No wrong output formats
- ✅ Budget maintained (>80% remaining)

---

## Version History

### V3.0 (2026-06-13)
- **Added**: Golden Rule (always copy same phase from previous wave)
- **Added**: Common mistakes section
- **Added**: Recovery procedure
- **Reason**: Wave 3 Phase 3 architecture bug (copied adjacent phase)

### V2.0 (2026-06-12)
- **Added**: Test with 2 epics before full deployment
- **Added**: Verification checklist
- **Reason**: Wave 3 Phase 1 failures (3 attempts)

### V1.0 (2026-06-11)
- **Initial**: Basic script generation procedure
- **Reason**: Wave 2 Phase 0 success

---

## References

- **Wave 3 Phase 3 Bug**: `WAVE3_PHASE3_ARCHITECTURE_BUG_ANALYSIS.md`
- **Lessons Learned**: `building-blocks/autonomous-refactoring/WAVE3_PHASE3_LESSONS_LEARNED.md`
- **Complete Handoff**: `WAVE3_PHASE3_COMPLETE_HANDOFF.md`

---

**MANDATORY COMPLIANCE**: All agents MUST follow this SOP for all phase script generation.

**Violation Consequences**: Wrong output format, wasted bobcoins, debugging time, architecture rewrites.

**Next Update**: After Wave 3 Phase 4 completion.