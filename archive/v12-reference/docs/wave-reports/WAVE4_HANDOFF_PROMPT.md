# Wave 4 Phase 0 - Autonomous Execution Start

Execute Wave 4 Phase 0 for 80 epics autonomously. Install Firebase, generate scripts, launch, monitor, report. YOLO mode - no approval gates.

## Context Summary

**Wave**: 4 (renamed from Wave 3 to avoid confusion)
**Phase**: 0 (Hotspot Analysis)
**Epics**: 80 (EPIC-CCN-001 through EPIC-CCN-080)
**Workflow**: 10-phase manifest-based (V12.25)
**APIs**: 15 existing (round-robin rotation, ~150 bobcoins each)
**VM**: `v12-test-golden-v2` (zone: us-central1-a)
**Merge Status**: NOT required (Firebase bypasses git)

## Critical Files

**Firebase**:
- Local: `firebase-credentials.json` (gitignored, must copy to VM)
- Scripts: `scripts/query_kb.py`, `scripts/phase_4_5_ticket_review_mcp.py`
- 5 phases use Firebase: 1, 2, 3, 4.5, 5, 5.V (manual + automated)

**Documentation**:
- 10-Phase SOP: `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`
- Script Generation SOP: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- API Rotation: `WAVE3_API_ROTATION_STRATEGY.md` (content is Wave 4)
- Execution Plan: `WAVE3_FINAL_EXECUTION_PLAN.md` (content is Wave 4)
- Firebase Integration: `FIREBASE_INTEGRATION_CORRECTION.md`
- Merge Analysis: `WAVE3_MERGE_ANALYSIS.md`

**Wave 2 Reference**:
- Complete Report: `building-blocks/autonomous-refactoring/WAVE2_COMPLETE_REPORT.md`
- Lessons Learned: `building-blocks/autonomous-refactoring/WAVE2_LESSONS_LEARNED.md`
- Phase 0 Scripts: `scripts/wave2/generate_phase0_scripts.py` (copy this!)

## Building-Blocks Method (MANDATORY)

**Golden Rule**: ALWAYS copy SAME phase from PREVIOUS wave, NEVER generate from scratch

**For Phase 0**:
1. Copy `scripts/wave2/generate_phase0_scripts.py` → `scripts/wave4/generate_phase0_scripts.py`
2. Update epic numbers: 107-115 → 001-080
3. Implement API rotation: `api_index = (epic_num - 1) % 15`
4. Load 15 API keys from `docs/API/*.json` files
5. Hardcode keys in scripts (no jq extraction)

**Script Template** (from Wave 2):
```bash
#!/bin/bash
export BOBSHELL_API_KEY='bob_prod_bob-admin_...'  # Hardcoded
cd /home/malhitticrypto/universal-or-strategy
screen -dmS p0-{NUM} bash -l -c "bob --yolo --chat-mode v12-phase0-hotspot \"$(cat /tmp/phase0_msg_{NUM}.txt)\" 2>&1 | tee logs/phase0/EPIC-CCN-{NUM}.log"
```

**Critical Requirements**:
- ✅ `--yolo` flag (file persistence in SSH mode)
- ✅ `bash -l` (login shell)
- ✅ Hardcoded API keys (not jq extraction)
- ✅ Message file approach (`/tmp/phase0_msg_*.txt`)

## Execution Steps (YOLO Mode)

### Step 1: Install Firebase on VM (5 min)

```bash
# Install package
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="pip3 install firebase-admin"

# Copy credentials
gcloud compute scp firebase-credentials.json v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Test (expect 10 documents)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd ~/universal-or-strategy && python3 scripts/query_kb.py test"
```

**Success**: No errors, 10 Jane Street documents returned

### Step 2: Run Complexity Audit (2 min)

```bash
# Run audit
python scripts/complexity_audit.py > complexity_audit_wave4.txt

# Parse top 80 (if parser exists)
python scripts/parse_complexity_audit.py complexity_audit_wave4.txt --top 80 --output epic_roadmap_wave4.json
```

**Success**: `complexity_audit_wave4.txt` created with 80+ methods CYC >8

**Note**: If parser doesn't exist, manually extract top 80 methods from audit output

### Step 3: Generate Phase 0 Scripts (10 min)

```bash
# Copy Wave 2 generator (building-blocks method)
cp scripts/wave2/generate_phase0_scripts.py scripts/wave4/generate_phase0_scripts.py

# Edit for Wave 4:
# - Epic range: 001-080 (was 107-115)
# - API rotation: api_index = (epic_num - 1) % 15
# - Load 15 API keys from docs/API/*.json
# - Hardcode keys in scripts

# Generate
python scripts/wave4/generate_phase0_scripts.py

# Validate
ls scripts/wave4/_p0_*.sh | wc -l  # Expect: 80
ls scripts/wave4/launch_phase0_all.sh  # Expect: exists
```

**Success**: 80 scripts + 1 launcher created, all executable

### Step 4: Launch Wave 4 Phase 0 (5 min)

```bash
# Upload scripts
gcloud compute scp scripts/wave4/_p0_*.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave4/launch_phase0_all.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Make executable
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x ~/universal-or-strategy/_p0_*.sh ~/universal-or-strategy/launch_phase0_all.sh"

# Launch
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd ~/universal-or-strategy && ./launch_phase0_all.sh"
```

**Success**: 80 screen sessions created, logs directory exists

## Monitoring (4-Minute Polling)

```bash
# Count running sessions (80 → 0)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c 'p0-' || echo 0"

# Count files created (0 → 160)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls ~/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l"

# Check errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i 'error\|failed' ~/universal-or-strategy/logs/phase0/*.log | head -20"
```

**Complete When**: All sessions done, 160 files created, no errors

**Estimated Time**: 30-60 minutes

## Success Criteria

**Per Epic (80 total)**:
- ✅ Screen session completes
- ✅ Files exist: `docs/brain/EPIC-CCN-{ID}/00-hotspots.md`
- ✅ Files exist: `docs/brain/EPIC-CCN-{ID}/manifest.json`
- ✅ Bobcoin usage logged
- ✅ API balance positive

**Phase 0 Complete**:
- ✅ 80/80 epics done
- ✅ 160 files created
- ✅ All APIs >10 bobcoins
- ✅ No P0 blockers

## Post-Completion Actions

```bash
# Sync to local
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/ ./docs/brain/ --zone=us-central1-a
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/logs/phase0/ ./logs/phase0/ --zone=us-central1-a

# Extract bobcoin usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' ~/universal-or-strategy/logs/phase0/*.log" > wave4_phase0_usage.txt

# Validate
ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l  # Expect: 80
ls docs/brain/EPIC-CCN-*/manifest.json | wc -l  # Expect: 80

# Generate report
# Create: WAVE4_PHASE0_COMPLETION_REPORT.md
# Include: Success rate, bobcoin usage, failures, next steps
```

## Failure Recovery

**Firebase fails**: Retry with `pip3 install --user firebase-admin`
**Test fails**: Check file permissions `chmod 600 firebase-credentials.json`
**Scripts fail**: Follow building-blocks method - copy Wave 2, don't generate from scratch
**Files don't persist**: Verify `--yolo` flag in all scripts
**API negative**: STOP immediately, check allocation, contact IBM

## Key Reminders

- **10-Phase Workflow**: Phase 4.5 (Ticket Review) is NEW in Wave 4
- **Firebase**: 5 phases use it (1, 2, 3, 4.5, 5, 5.V)
- **API Rotation**: 15 APIs, round-robin, 5-6 epics each
- **Building-Blocks**: ALWAYS copy previous wave, NEVER generate from scratch
- **Polling**: 4-minute intervals (cache optimization)
- **No Merge**: Firebase bypasses git (pip + gcloud scp)

## API Keys Location

15 API keys in: `docs/API/*.json`

Files:
- `bob.json`, `bob (1).json`, `bob (2).json`, `bob (3).json`, `bob (4).json`, `bob (5).json`, `bob (6).json`
- `b.json`, `b (2).json`
- `jessica.json`, `mikethelife.json`, `sammy96.json`, `sean.carter.jr@atomicmail.io.json`, `tory.json`

**Total**: 15 files (some have spaces in names, handle carefully)

## Execution Command

**Paste this to start**:

```
Execute Wave 4 Phase 0 autonomously. Follow WAVE4_HANDOFF_PROMPT.md. Complete Steps 1-4 in YOLO mode, monitor until done, generate completion report. I'm standing by.
```

---

**Version**: 1.0
**Created**: 2026-06-14T21:54:00Z
**Wave**: 4
**Phase**: 0
**Epics**: 80
**Status**: Ready for autonomous execution