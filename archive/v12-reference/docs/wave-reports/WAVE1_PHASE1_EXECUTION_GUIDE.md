# Wave 1 Phase 1: 3-VM Execution Guide

**Date**: 2026-06-14
**Phase**: Phase 1 (Scope Definition)
**Epics**: 15 total (EPIC-001 through EPIC-015)
**VMs**: 3 (5 epics each)
**Method**: Building Blocks (copy Phase 0, modify phase-specific content)

---

## Quick Start

```bash
cd scripts/wave1
bash execute_phase1_3vms.sh
```

This single command will:
1. Generate all 15 Phase 1 scripts from Phase 0 template
2. Upload scripts to 3 VMs (5 epics each)
3. Launch execution in parallel
4. Display monitoring commands

**Estimated Time**: 20-30 minutes

---

## VM Distribution

| VM | Instance Name | Epics | Zone |
|----|---------------|-------|------|
| **VM1** | v12-test-golden-v2 | EPIC-001 to EPIC-005 | us-central1-a |
| **VM2** | v12-test-golden-v3 | EPIC-006 to EPIC-010 | us-central1-a |
| **VM3** | v12-test-golden-v4 | EPIC-011 to EPIC-015 | us-central1-a |

---

## Building Blocks Method

**Key Principle**: Copy working Phase 0 scripts, modify ONLY phase-specific content

### What Changes Between Phases

| Element | Phase 0 | Phase 1 |
|---------|---------|---------|
| Script name | `_p0_*.sh` | `_p1_*.sh` |
| Log directory | `logs/phase0/` | `logs/phase1/` |
| Message file | `/tmp/phase0_msg_*.txt` | `/tmp/phase1_msg_*.txt` |
| Output file | `00-hotspots.md` | `00-scope.md` |
| Task description | Hotspot Analysis | Scope Definition |
| Chat mode | `v12-phase0-hotspot` | `plan` |

### What Stays IDENTICAL

- ✅ API key loading (hardcoded)
- ✅ Directory structure
- ✅ Bob Shell invocation pattern (`bob --yolo --chat-mode`)
- ✅ Logging pattern (`2>&1 | tee`)
- ✅ Error handling (`set -e`)
- ✅ Launcher pattern (`screen -dmS ... bash -l`)

---

## Step-by-Step Execution

### Step 1: Generate Phase 1 Scripts

```bash
cd scripts/wave1
bash create_phase1_scripts.sh
```

**What it does**:
1. Downloads Phase 0 template from VM1 (`_p0_003.sh`)
2. Creates 15 Phase 1 scripts (`_p1_01.sh` through `_p1_15.sh`)
3. Uses `sed` to replace phase-specific content
4. Makes all scripts executable

**Verification**:
```bash
ls -lh _p1_*.sh | wc -l  # Should show 15
```

### Step 2: Upload to VM1 (EPIC-001-005)

```bash
gcloud compute scp _p1_0{1..5}.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp launch_phase1_vm1.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_vm1.sh"
```

### Step 3: Upload to VM2 (EPIC-006-010)

```bash
gcloud compute scp _p1_{06..10}.sh v12-test-golden-v3:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp launch_phase1_vm2.sh v12-test-golden-v3:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_vm2.sh"
```

### Step 4: Upload to VM3 (EPIC-011-015)

```bash
gcloud compute scp _p1_{11..15}.sh v12-test-golden-v4:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp launch_phase1_vm3.sh v12-test-golden-v4:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_vm3.sh"
```

### Step 5: Launch Execution

```bash
# VM1
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && bash launch_phase1_vm1.sh"

# VM2
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && bash launch_phase1_vm2.sh"

# VM3
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && bash launch_phase1_vm3.sh"
```

---

## Monitoring

### Check Screen Sessions

```bash
# VM1
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# VM2
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="screen -ls"

# VM3
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="screen -ls"
```

**Expected**: 5 sessions per VM (p1-01 through p1-05, etc.)
**When complete**: "No Sockets found" message

### Check File Creation

```bash
# VM1 (expect 5)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{001..005}/00-scope.md 2>/dev/null | wc -l"

# VM2 (expect 5)
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{006..010}/00-scope.md 2>/dev/null | wc -l"

# VM3 (expect 5)
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{011..015}/00-scope.md 2>/dev/null | wc -l"
```

**Total Expected**: 15 files

### Extract Bobcoin Usage

```bash
# VM1
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-{001..005}.log"

# VM2
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-{006..010}.log"

# VM3
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-{011..015}.log"
```

---

## Success Criteria

### Per VM

- ✅ All 5 screen sessions complete (DONE_EXIT=0)
- ✅ All 5 scope files created (`00-scope.md`)
- ✅ All 5 manifest files updated
- ✅ Bobcoin usage reported in logs
- ✅ All APIs remain positive (>10 bobcoins)

### Overall

- ✅ 15/15 epics complete
- ✅ 15 scope files created
- ✅ Total bobcoin usage <150 (budget: 75-150 bobcoins)
- ✅ No P0 blockers
- ✅ Ready for Phase 2

---

## Budget Analysis

### Phase 1 Estimates

**Per Epic**: 5-10 bobcoins
**Total**: 75-150 bobcoins (15 epics)

### Running Total (After Phase 1)

**Phase 0**: ~22.39 bobcoins
**Phase 1**: 75-150 bobcoins (estimated)
**Total Used**: ~97-172 bobcoins (6-11% of 1,600 total)
**Remaining**: ~1,428-1,503 bobcoins (89-94% of budget)

---

## Troubleshooting

### Issue: Scripts not executable

```bash
# Fix on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh"
```

### Issue: Screen sessions not starting

```bash
# Check if scripts exist
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/_p1_*.sh"

# Check logs for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -50 /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-001.log"
```

### Issue: Files not created

**Root Cause**: Missing `--yolo` flag or wrong chat mode

**Fix**: Verify script has:
```bash
bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_*.txt)"
```

### Issue: API authentication failed

**Root Cause**: Wrong API key or environment variable

**Fix**: Verify script has:
```bash
export BOBSHELL_API_KEY='bob_prod_bob-admin_...'
```

---

## Phase 1 Task Description

Phase 1 reads the Phase 0 hotspot analysis and creates a scope definition document.

**Input**: `docs/brain/EPIC-XXX/00-hotspots.md`
**Output**: `docs/brain/EPIC-XXX/00-scope.md`

**Key Sections**:
1. **Scope Summary**: What will be extracted
2. **Boundary Definition**: What will NOT be changed
3. **Complexity Targets**: CYC reduction goals
4. **Risk Assessment**: Blast radius and dependencies
5. **Extraction Strategy**: High-level approach

---

## Next Steps (After Phase 1)

1. **Sync Files**: Pull all scope files from VMs to local
2. **Verify Quality**: Review scope definitions for completeness
3. **Update Roadmap**: Mark Phase 1 complete
4. **Prepare Phase 2**: Architecture planning (10-15 bobcoins/epic)

---

## References

- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`
- **Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md`
- **Phase 0 Report**: `WAVE1_PHASE0_FINAL_COMPLETION_REPORT.md`
- **10-Phase Workflow**: `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T07:35:00Z
**Maintainer**: V12 Orchestration Team