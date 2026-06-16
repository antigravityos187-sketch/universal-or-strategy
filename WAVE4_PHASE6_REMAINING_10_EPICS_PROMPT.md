# Wave 4 Phase 6 - Complete Remaining 10 Epics

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Execute Phase 6 for Final 10 Epics to Reach 79/80

You are completing Wave 4 by executing Phase 6 (Verification) for the final 10 epics that have Phase 5 complete but are missing Phase 6. Use the building-blocks method to generate scripts and execute on GCP VM with 12-second staggered launch.

## Critical Status Summary

### Wave 4 Current Status
- **Phase 5 Complete**: 80/80 (100%)
- **Phase 6 Complete**: 69/80 (86.25%)
- **Both Phases**: 69/80 (86.25%)
- **Goal**: Execute Phase 6 for 10 epics to reach 79/80 (98.75%)

### The 10 Target Epics
All have Phase 5 completion files, need Phase 6 verification:
1. EPIC-CCN-003
2. EPIC-CCN-015
3. EPIC-CCN-030
4. EPIC-CCN-031
5. EPIC-CCN-033
6. EPIC-CCN-042
7. EPIC-CCN-045
8. EPIC-CCN-055
9. EPIC-CCN-060
10. EPIC-CCN-075

### Why Skip EPIC-CCN-027?
- **Target method doesn't exist**: `Dispatch_PublishMarketBracketToPhoton` not found in codebase
- **Root cause**: Stale jCodemunch index (method was removed/renamed)
- **Status**: INVALID - cannot execute Phase 5 or Phase 6
- **Decision**: Skip and accept 79/80 (98.75%) as Wave 4 completion

### Commits Already Done
- ✅ `dff1d78b`: Wave 4 Phase 5+6 for 68 epics (2026-06-15)
- ✅ `a14b32d2`: EPIC-CCN-016 Phase 5+6 (2026-06-16)
- ✅ Total: 69/80 epics committed

## Your Mission

Generate Phase 6 scripts using building-blocks method, upload to VM, execute with 12-second staggered launch, monitor completion, sync files, and commit.

## Execution Plan (8 Steps)

### Step 1: Generate Phase 6 Scripts (Building-Blocks Method)

**MANDATORY**: Copy from Wave 4 Phase 6 scripts, modify only epic-specific parameters.

**Reference Script** (from `scripts/wave4/_p6_001.sh`):
```bash
#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-001
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-001"
API_KEY="bob_prod_bob-admin_..."

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check
if ! ls docs/brain/EPIC-CCN-001/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "ERROR: Missing Phase 5 completion files for EPIC-CCN-001"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_001.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-001.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-001".

The tool will verify:
1. All tickets executed successfully
2. Complexity targets met
3. Build passes
4. No behavioral changes
5. All acceptance criteria satisfied

**Verification**: Confirm verification report exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase6_msg_001.txt)"

# Verify verification report created
if [ -f "docs/brain/EPIC-CCN-001/06-verification-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-001"
    echo "File: docs/brain/EPIC-CCN-001/06-verification-report.md"
    ls -lh docs/brain/EPIC-CCN-001/06-verification-report.md
else
    echo "ERROR: No verification report created for EPIC-CCN-001"
    exit 1
fi
```

**Generation Script** (`scripts/generate_phase6_remaining.py`):
```python
#!/usr/bin/env python3
"""Generate Phase 6 scripts for remaining 10 epics using building-blocks method."""

import json
from pathlib import Path

# Load API keys
api_keys = []
for api_file in Path('docs/API').glob('*.json'):
    with open(api_file) as f:
        data = json.load(f)
        api_keys.append(data['api_key'])

# Target epics (10 remaining)
target_epics = [3, 15, 30, 31, 33, 42, 45, 55, 60, 75]

for i, epic_num in enumerate(target_epics):
    epic_id = f"EPIC-CCN-{epic_num:03d}"
    api_key = api_keys[i % len(api_keys)]  # Round-robin
    
    script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id}
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check
if ! ls docs/brain/{epic_id}/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_{epic_num:03d}.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for {epic_id}.

Call the execute_phase_6 tool with epic_id="{epic_id}".

The tool will verify:
1. All tickets executed successfully
2. Complexity targets met
3. Build passes
4. No behavioral changes
5. All acceptance criteria satisfied

**Verification**: Confirm verification report exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase6_msg_{epic_num:03d}.txt)"

# Verify verification report created
if [ -f "docs/brain/{epic_id}/06-verification-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for {epic_id}"
    echo "File: docs/brain/{epic_id}/06-verification-report.md"
    ls -lh docs/brain/{epic_id}/06-verification-report.md
else
    echo "ERROR: No verification report created for {epic_id}"
    exit 1
fi
"""
    
    output_path = Path(f'scripts/wave4_remaining/_p6r_{epic_num:03d}.sh')
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(script)
    print(f"Generated: {output_path}")

print(f"\nTotal scripts generated: {len(target_epics)}")
```

**Execute Generation**:
```bash
python scripts/generate_phase6_remaining.py
```

### Step 2: Upload Scripts to VM

```bash
# Create directory on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="mkdir -p /home/malhitticrypto/universal-or-strategy/scripts/wave4_remaining"

# Upload all Phase 6 scripts
gcloud compute scp scripts/wave4_remaining/_p6r_*.sh \
  v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave4_remaining/ \
  --zone=us-central1-a

# Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="chmod +x /home/malhitticrypto/universal-or-strategy/scripts/wave4_remaining/_p6r_*.sh"

# Verify upload
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -la /home/malhitticrypto/universal-or-strategy/scripts/wave4_remaining/_p6r_*.sh | wc -l"
# Expected: 10
```

### Step 3: MANDATORY Pilot Test

**Launch Command**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && screen -dmS p6r-003 bash -l -c './scripts/wave4_remaining/_p6r_003.sh 2>&1 | tee logs/phase6_remaining/EPIC-CCN-003.log'"
```

**Wait 1 minute, then check**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && screen -ls && ls docs/brain/EPIC-CCN-003/06-verification-report.md"
```

**Success Criteria**:
1. ✅ Screen session complete
2. ✅ File exists: `docs/brain/EPIC-CCN-003/06-verification-report.md`
3. ✅ File size >1K
4. ✅ No errors in log
5. ✅ Bobcoin usage reported

**If Pilot Fails**: Fix issue, re-test. DO NOT proceed to full launch.

### Step 4: Generate Launcher Script

**Full Launcher** (`scripts/wave4_remaining/launch_phase6_remaining.sh`):
```bash
#!/bin/bash
# Phase 6 Remaining - 10 Epics
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Starting Phase 6 for remaining 10 epics"
mkdir -p logs/phase6_remaining

# Target epics
EPICS=(003 015 030 031 033 042 045 055 060 075)

for epic_num in "${EPICS[@]}"; do
    EPIC="EPIC-CCN-${epic_num}"
    echo "[$(date)] Launching ${EPIC}"
    
    screen -dmS p6r-${epic_num} bash -l -c \
        "./scripts/wave4_remaining/_p6r_${epic_num}.sh 2>&1 | tee logs/phase6_remaining/${EPIC}.log"
    
    sleep 12
done

echo "[$(date)] All 10 epics launched for Phase 6"
echo "Monitor with: screen -ls | grep -c 'p6r-'"
echo "Check files: ls docs/brain/EPIC-CCN-{003,015,030,031,033,042,045,055,060,075}/06-verification-report.md | wc -l"
```

**Upload Launcher**:
```bash
gcloud compute scp scripts/wave4_remaining/launch_phase6_remaining.sh \
  v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave4_remaining/ \
  --zone=us-central1-a

gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="chmod +x /home/malhitticrypto/universal-or-strategy/scripts/wave4_remaining/launch_phase6_remaining.sh"
```

### Step 5: Launch Full Wave

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && ./scripts/wave4_remaining/launch_phase6_remaining.sh"
```

### Step 6: Monitor Execution (V2.0 Protocol)

**Initial Check** (1 min after first script):
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && echo '=== CHECK 1 ===' && screen -ls | grep -c 'p6r-' && ls docs/brain/EPIC-CCN-{003,015,030,031,033,042,045,055,060,075}/06-verification-report.md 2>/dev/null | wc -l"
```

**Subsequent Checks** (every 4 minutes):
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && echo '=== CHECK N ===' && date && screen -ls | grep -c 'p6r-' && ls docs/brain/EPIC-CCN-{003,015,030,031,033,042,045,055,060,075}/06-verification-report.md 2>/dev/null | wc -l"
```

**Stop When**:
- All screen sessions complete (0 sessions)
- File count reaches 10
- Count stable for 2 consecutive checks

### Step 7: Sync Files from VM

```bash
# Sync Phase 6 files for 10 epics
gcloud compute scp --recurse \
  "v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-{003,015,030,031,033,042,045,055,060,075}/06-verification-report.md" \
  ./docs/brain/ \
  --zone=us-central1-a
```

### Step 8: Commit and Celebrate

**Verify Files Synced**:
```bash
ls docs/brain/EPIC-CCN-{003,015,030,031,033,042,045,055,060,075}/06-verification-report.md | wc -l
# Expected: 10
```

**Commit**:
```bash
git add docs/brain/EPIC-CCN-{003,015,030,031,033,042,045,055,060,075}/06-verification-report.md
git commit -m "docs: Wave 4 Phase 6 completion - remaining 10 epics (79/80 total)"
```

**Update Roadmap**:
```bash
# Mark 10 epics as complete in epic_roadmap.json
# Update Wave 4 status to 79/80
```

**Celebrate**: Wave 4 is 79/80 (98.75%) complete! 🎉

## Key Documents

**MUST READ**:
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md` - Building-blocks method
- `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md` - 4-minute polling
- `docs/protocol/RECOVERY_LOOP_PROTOCOL.md` - If <100% success

**Reference**:
- `scripts/wave4/_p6_001.sh` - Template script
- `WAVE4_PHASE6_COMPLETION_REPORT.md` - Previous Phase 6 execution
- `.bob/skills/gcp-vm-wave-execution/skill.md` - VM execution protocol

## VM Configuration

- **Instance**: v12-test-golden-v2
- **Zone**: us-central1-a
- **Type**: n2-standard-8 (8 vCPU, 32 GB RAM)
- **Status**: Check with `gcloud compute instances list --filter="name=v12-test-golden-v2"`
- **Start if TERMINATED**: `gcloud compute instances start v12-test-golden-v2 --zone=us-central1-a`

## API Keys & Bobcoins

- **Total APIs**: 15 (docs/API/*.json)
- **Bobcoins per API**: 160
- **Total Budget**: 2,400 bobcoins
- **Used (Phases 0-6)**: ~1,200 bobcoins
- **Remaining**: ~1,200 bobcoins
- **Phase 6 Budget**: 50-100 bobcoins (5-10 per epic)

## Success Criteria

### Per Epic
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/06-verification-report.md`
- ✅ File size >1K
- ✅ All verification checks passed
- ✅ Bobcoin usage <10 per epic

### Wave Completion
- ✅ 10/10 epics complete (100% of remaining)
- ✅ Total: 79/80 epics (98.75%)
- ✅ Total bobcoin usage <100
- ✅ All APIs remain positive
- ✅ Files synced and committed

## Timeline Estimate

- **Script Generation**: 5 minutes
- **Upload & Permissions**: 5 minutes
- **Pilot Test**: 10 minutes
- **Full Wave Launch**: 2 minutes (10 × 12s)
- **Execution**: ~10-15 min/epic (parallel)
- **Monitoring**: ~20 minutes (5 checks × 4 min)
- **Sync & Commit**: 10 minutes
- **TOTAL**: ~60-75 minutes

## Common Pitfalls (AVOID)

1. ❌ **Generating scripts from scratch** - Use building-blocks method
2. ❌ **Skipping pilot test** - Always test one epic first
3. ❌ **Wrong delay** - Use 12 seconds (not 13, 14, etc.)
4. ❌ **Polling too frequently** - Use 4-minute intervals
5. ❌ **Forgetting to sync files** - Files on VM don't auto-sync
6. ❌ **Not verifying upload** - Check script count matches
7. ❌ **Background execution** - Use screen for visibility
8. ❌ **Skipping EPIC-CCN-027** - It's invalid, don't waste time

## Your First Actions

1. Check VM status (start if terminated)
2. Generate Phase 6 scripts using building-blocks method
3. Upload scripts to VM and set permissions
4. Verify upload (10 scripts)
5. Run pilot test (EPIC-CCN-003)
6. Validate pilot success
7. Generate and upload launcher script
8. Launch full wave
9. Monitor execution (4-minute intervals)
10. Sync files and commit

---

**Ready to complete Wave 4!** Start with VM status check and script generation. Good luck! 🚀

---

**Session Context Version**: 1.0 (Wave 4 Phase 6 Remaining)
**Last Updated**: 2026-06-16T17:10:00Z
**Maintainer**: Wave 4 Completion Lead
**Status**: 🟢 READY FOR PHASE 6 REMAINING EXECUTION