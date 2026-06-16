# Wave 3 Final Execution Plan

**Version**: 1.0
**Date**: 2026-06-14
**Status**: APPROVED
**Strategy**: Adaptive API Rotation with Real-Time Monitoring

---

## Executive Summary

**Total Epics**: 80 (two-tier system: complexity-based selection)
**API Strategy**: Rotate 15 existing APIs, monitor usage, re-evaluate dynamically
**Budget Approach**: Adaptive - track actual usage vs. estimates, adjust rotation as needed
**Execution Model**: Sequential phases, parallel epics, staggered launches

---

## Two-Tier Epic Selection System

**User Clarification**: "the two tier system is how created the 80 epics"

### Interpretation: Complexity-Based Tiers

**Tier 1**: High-complexity methods (CYC 15-30)
- Highest priority for refactoring
- Larger bobcoin budget per epic
- More complex extraction patterns

**Tier 2**: Medium-complexity methods (CYC 9-14)
- Secondary priority
- Smaller bobcoin budget per epic
- Simpler extraction patterns

**Total**: 80 epics selected across both tiers

---

## API Rotation Strategy (Revised)

### Available APIs: 15 Existing

**API Pool** (from Wave 2):
```
api-01.json through api-15.json
```

**Starting Balance**: ~150 bobcoins each (after Wave 2 usage)
**Total Available**: 15 × 150 = 2,250 bobcoins

### Rotation Pattern: Round-Robin

**Formula**: `epic_num % 15 = api_index`

**Distribution**:
- Each API handles: 80 ÷ 15 = 5.33 epics (5-6 epics each)
- API 1-5: 6 epics each (30 epics)
- API 6-15: 5 epics each (50 epics)

**Example**:
```
EPIC-001 → API-01
EPIC-002 → API-02
...
EPIC-015 → API-15
EPIC-016 → API-01 (rotation)
EPIC-017 → API-02
...
EPIC-080 → API-05
```

---

## Budget Analysis: Adaptive Monitoring

### Estimated Usage (Conservative)

**Per Epic Per Phase**: 5 bobcoins (planning estimate)
**Per Epic Total**: 5 × 10 phases = 50 bobcoins
**Total Needed**: 80 × 50 = 4,000 bobcoins

### Available Budget

**Current**: 2,250 bobcoins (15 APIs × 150)
**Shortfall**: 1,750 bobcoins (44%)

### Adaptive Strategy: Monitor & Adjust

**Phase 0 Baseline**:
1. Launch Phase 0 for all 80 epics
2. Track actual bobcoin usage per epic
3. Calculate real cost: `actual_cost = total_used ÷ 80`
4. Project total: `projected_total = actual_cost × 10 phases × 80 epics`
5. Compare to available budget

**Decision Points**:
- If `projected_total < 2,250`: Continue with all 80 epics
- If `projected_total > 2,250`: Pause and re-evaluate
  - Option 1: Reduce epic count to fit budget
  - Option 2: Acquire additional APIs
  - Option 3: Defer execution phases (5-6)

**Monitoring Frequency**: After each phase completion

---

## Execution Workflow

### Phase-by-Phase Pattern

**For Each Phase (0 through 6)**:

1. **Generate Scripts** (10 min):
   ```bash
   python scripts/wave3/generate_wave3_phase{X}_scripts.py
   ```

2. **Upload to VM** (5 min):
   ```bash
   gcloud compute scp scripts/wave3/_p{X}_*.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
   gcloud compute scp scripts/wave3/launch_phase{X}_all.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
   ```

3. **Launch Staggered** (40 min):
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ./launch_phase{X}_all.sh"
   ```
   - 80 epics × 30s avg delay = 40 minutes launch time

4. **Monitor Execution** (60 min):
   ```bash
   # Poll every 4 minutes (cache optimization)
   watch -n 240 'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c p{X}-"'
   ```

5. **Verify Completion** (10 min):
   ```bash
   # Check files created
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls docs/brain/EPIC-CCN-*/0{X}-*.md 2>/dev/null | wc -l"
   
   # Extract bobcoin usage
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase{X}/*.log"
   ```

6. **Budget Analysis** (15 min):
   - Calculate actual cost per epic
   - Project remaining budget
   - Decide: continue, pause, or adjust

**Total Per Phase**: ~140 minutes (2.3 hours)

---

## Bobcoin Tracking Protocol

### Real-Time Monitoring

**After Each Phase**:

1. **Extract Usage Data**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="grep -E 'Cost:.*Balance:' logs/phase{X}/*.log" >  bobcoin_phase{X}.txt
   ```

2. **Parse and Aggregate**:
   ```python
   import re
   
   # Parse bobcoin_phase{X}.txt
   costs = []
   balances = {}
   
   for line in open('bobcoin_phase{X}.txt'):
       match = re.search(r'Cost: ([\d.]+).*Balance: ([\d.]+)', line)
       if match:
           cost = float(match.group(1))
           balance = float(match.group(2))
           costs.append(cost)
           # Track per-API balance
   
   avg_cost = sum(costs) / len(costs)
   print(f"Phase {X} Average Cost: {avg_cost:.2f} bobcoins/epic")
   print(f"Projected Total: {avg_cost * 10 * 80:.2f} bobcoins")
   ```

3. **Update Tracking Document**:
   ```markdown
   # Wave 3 Bobcoin Usage Tracking
   
   ## Phase {X} Summary
   - Epics Completed: 80/80
   - Average Cost: {avg_cost} bobcoins/epic
   - Total Phase Cost: {total_cost} bobcoins
   - Remaining Budget: {remaining} bobcoins
   - Projected Total: {projected} bobcoins
   - Status: {CONTINUE|PAUSE|ADJUST}
   ```

4. **Decision Gate**:
   - **CONTINUE**: If projected < available
   - **PAUSE**: If projected > available (re-evaluate)
   - **ADJUST**: If close to limit (reduce epic count)

---

## Staggered Launch Implementation

### Master Launch Script Pattern

```bash
#!/bin/bash
# launch_phase{X}_all.sh

PHASE={X}
EPICS=($(seq -f "%03g" 1 80))
BASE_DELAY=12
MAX_DELAY=54

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    # Calculate staggered delay (12-54 seconds)
    DELAY=$((BASE_DELAY + (i % (MAX_DELAY - BASE_DELAY + 1))))
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s)"
    
    # Launch in screen session
    screen -dmS p${PHASE}-${EPIC} bash -l -c \
        "./_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch
    sleep ${DELAY}
done

echo "[$(date)] All 80 epics launched for Phase ${PHASE}"
```

**Launch Time**: 80 epics × 30s avg = 40 minutes

---

## Firebase Integration

### VM Setup (One-Time)

```bash
# Install firebase-admin
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="pip3 install firebase-admin"

# Copy credentials
gcloud compute scp firebase-credentials.json \
  v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ \
  --zone=us-central1-a

# Test connectivity
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && python3 scripts/query_kb.py test"
```

**Expected Output**: "Successfully connected to Firebase. Found 10 Jane Street documents."

### Phase 4.5 Integration

**Phase 4.5** (Ticket Review) is the ONLY phase requiring Firebase:
- Validates tickets against Jane Street KB
- Checks for P0/P1 violations
- Ensures HFT pattern compliance

**No Firebase Impact** on Phases 0-4, 5-6.

---

## Success Criteria

### Per Phase
- ✅ All 80 scripts generated with correct API rotation
- ✅ All scripts follow SOP V3 (copy same phase from previous wave)
- ✅ All scripts use correct mode (ask/plan/advanced/v12-engineer)
- ✅ All 80 epics complete successfully
- ✅ All output files created (0{X}-*.md)
- ✅ Bobcoin usage tracked and documented
- ✅ All APIs remain positive (>10 bobcoins)
- ✅ Budget projection updated

### Wave 3 Completion
- ✅ All 10 phases executed (0 through 6)
- ✅ 80 epics complete (CCN-001 through CCN-080)
- ✅ Complexity targets met (CYC ≤8)
- ✅ No P0 blockers
- ✅ Build passes
- ✅ Tests pass
- ✅ Roadmap updated
- ✅ Budget analysis documented

---

## Risk Mitigation

### Risk 1: Budget Exhaustion

**Trigger**: Projected total > available budget after Phase 0
**Response**:
1. Pause execution
2. Calculate actual cost per epic
3. Determine sustainable epic count
4. Options:
   - Reduce to 50 epics (fits budget)
   - Acquire 6 more APIs (960 bobcoins)
   - Defer execution phases (5-6)

### Risk 2: API Rate Limiting

**Trigger**: jCodemunch timeouts (like Wave 2 EPIC-112)
**Response**:
1. Increase staggered delays (30s → 60s)
2. Reduce parallel execution (80 → 40)
3. Retry failed epics individually

### Risk 3: File Persistence Failure

**Trigger**: Files not created on disk
**Response**:
1. Verify `--yolo` flag in all scripts
2. Check Bob Shell authentication
3. Verify VM disk space
4. Relaunch failed epics

### Risk 4: Phase Script Architecture Bug

**Trigger**: Wrong output format (like Wave 3 Phase 3)
**Response**:
1. STOP immediately
2. Verify copied from correct phase (same phase, previous wave)
3. Regenerate scripts following SOP V3
4. Test with 2 epics before full deployment

---

## Next Steps (Immediate)

### Step 1: Firebase Installation (5 min)
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="pip3 install firebase-admin"

gcloud compute scp firebase-credentials.json \
  v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ \
  --zone=us-central1-a

gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && python3 scripts/query_kb.py test"
```

### Step 2: Complexity Audit (2 min)
```bash
python scripts/complexity_audit.py > complexity_audit_wave3.txt
```

### Step 3: Epic Selection (10 min)
- Parse audit output
- Select 80 epics (two-tier system)
- Document in `WAVE3_EPIC_ROADMAP.md`

### Step 4: Generate Phase 0 Scripts (10 min)
```bash
# Copy Wave 2 Phase 0 generator
cp scripts/wave2/generate_phase0_scripts.py scripts/wave3/generate_wave3_phase0_scripts.py

# Update epic numbers (001-080)
# Update API rotation (15 APIs, round-robin)

# Generate scripts
python scripts/wave3/generate_wave3_phase0_scripts.py
```

### Step 5: Test Launch (15 min)
```bash
# Upload 2 test scripts
gcloud compute scp _p0_001.sh _p0_002.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Execute
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ./_p0_001.sh"

# Verify output
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -lh docs/brain/EPIC-CCN-001/00-hotspots.md"
```

### Step 6: Full Launch (After Test Success)
```bash
# Upload all scripts
gcloud compute scp _p0_*.sh launch_phase0_all.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Execute
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ./launch_phase0_all.sh"

# Monitor
watch -n 240 'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c p0-"'
```

---

## Timeline Estimate

### Per Phase
- Script generation: 10 min
- Upload: 5 min
- Launch: 40 min
- Execution: 60 min
- Verification: 10 min
- Budget analysis: 15 min
- **Total**: 140 min (2.3 hours)

### Wave 3 Total
- 10 phases × 2.3 hours = 23 hours
- With breaks and monitoring: ~30 hours
- **Estimated Duration**: 3-4 days (8 hours/day)

---

## References

- **SOP V3**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md`
- **API Rotation**: `WAVE3_API_ROTATION_STRATEGY.md`
- **Wave 2 Config**: `docs/workflow/WAVE_2_CONFIGURATION.md`

---

**APPROVED**: Ready for execution pending Firebase installation and complexity audit.

**MONITORING**: Real-time bobcoin tracking after each phase with adaptive decision gates.

**FLEXIBILITY**: Adjust epic count dynamically based on actual usage vs. projections.