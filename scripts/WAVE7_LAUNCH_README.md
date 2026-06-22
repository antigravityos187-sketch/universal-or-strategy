# Wave 7 Launch Scripts

**Version**: 1.0  
**Created**: 2026-06-21  
**Purpose**: Master launch and monitoring scripts for Wave 7 autonomous execution

---

## Overview

Wave 7 executes **161 epics** to reduce all methods from CYC > 8 to CYC ≤ 8 (Jane Street strict standard). These scripts orchestrate the full wave execution with cost-optimized polling and comprehensive monitoring.

---

## Scripts

### 1. `launch_wave7.sh` - Master Launch Script

**Purpose**: Launch and monitor all 161 Wave 7 epics with two-phase polling strategy.

**Features**:
- ✅ Sequential epic launch (161 epics)
- ✅ Two-phase polling (1-min → 4-min after epic 10)
- ✅ Lamport event monitoring
- ✅ Progress reporting (X/161 complete)
- ✅ Recovery loop for failed epics
- ✅ Cost tracking (per API)
- ✅ Error detection (UTF-8, xUnit, build failures)
- ✅ Graceful shutdown (Ctrl+C handling)

**Usage**:
```bash
# From local machine (launches on VM)
bash scripts/launch_wave7.sh
```

**Polling Strategy**:
- **Phase 1** (First 10 epics): 1-minute intervals (launch verification)
- **Phase 2** (Remaining 151 epics): 4-minute intervals (cost-optimized)

**Cost Optimization**:
- 4-minute polling = 88% cost reduction (per COST_OPTIMIZED_POLLING_PROTOCOL.md)
- Stays within 5-minute cache window for all providers
- Parallel execution during polling (zero overhead)

### 2. `check_wave7_status.sh` - Status Check Script

**Purpose**: Detailed status monitoring for Wave 7 execution.

**Features**:
- ✅ Overall progress (completed/running/failed/pending)
- ✅ VM status (active sessions, disk usage)
- ✅ Phase distribution (epics per phase)
- ✅ Error analysis (Lamport log failures)
- ✅ Performance metrics (average phase durations)
- ✅ Recent activity (last 10 Lamport events)
- ✅ Recommendations (recovery actions)

**Usage**:
```bash
# From local machine (queries VM)
bash scripts/check_wave7_status.sh
```

**Output Example**:
```
========================================
Wave 7 Status Report - 2026-06-21 16:00:00
========================================

--- Overall Progress ---
  Completed: 42/161 (26%)
  Running:   15
  Failed:    2
  Pending:   102
  [=============-------------------------------------] 26%

--- VM Status ---
  Active screen sessions: 15
  Disk Usage: 12G / 50G (24% used)

--- Phase Distribution ---
  Phase 0: 5 epics
  Phase 1: 8 epics
  Phase 2: 2 epics

--- Error Analysis ---
Found 2 phase failures
Last 5 failures:
  - EPIC-W7-023 (Phase 1.5)
  - EPIC-W7-045 (Phase 2)

--- Performance Metrics ---
Average Phase Durations:
  Phase 0: 2.3 min (n=42)
  Phase 1: 3.1 min (n=37)
  Phase 2: 5.2 min (n=35)

--- Recent Activity (Last 10 Events) ---
  ✓ EPIC-W7-042 Phase 0 completed
  ▶ EPIC-W7-043 Phase 0 started
  ✗ EPIC-W7-023 Phase 1.5 failed
  ✓ EPIC-W7-041 Phase 1 completed

--- Recommendations ---
  ⚠  Run recovery loop for 2 failed epics

========================================
End of Status Report
========================================
```

---

## Prerequisites

### Local Machine
- ✅ Python 3.8+
- ✅ gcloud CLI configured
- ✅ SSH access to VM (v12-test-golden-v2)
- ✅ Wave 7 roadmap: `epic_roadmap_wave7.json`
- ✅ Templates: `building-blocks/wave7/phase*_template_wave7.sh`
- ✅ Lamport infrastructure: `.lamport/wave7/`

### VM (v12-test-golden-v2)
- ✅ IP: 34.69.114.138
- ✅ Zone: us-central1-a
- ✅ User: malhitticrypto
- ✅ Branch: main
- ✅ Build: Passing
- ✅ Bob CLI installed
- ✅ Screen utility installed

---

## Execution Workflow

### Step 1: Pre-Flight Checks

```bash
# Verify VM connectivity
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="echo 'VM accessible'"

# Verify roadmap exists
ls -la epic_roadmap_wave7.json

# Verify templates exist
ls -la building-blocks/wave7/phase*_template_wave7.sh

# Verify Lamport infrastructure
ls -la .lamport/wave7/
```

### Step 2: Launch Wave 7

```bash
# Launch all 161 epics
bash scripts/launch_wave7.sh
```

**What Happens**:
1. Pre-flight checks (VM, roadmap, templates, Lamport)
2. Record `wave_start` event
3. Launch first 10 epics (1-minute polling)
4. Verify launch success (no failures)
5. Launch remaining 151 epics (4-minute polling)
6. Monitor progress until 161/161 complete
7. Run recovery loop if failures detected
8. Record `wave_complete` event

### Step 3: Monitor Progress

```bash
# Check status anytime during execution
bash scripts/check_wave7_status.sh

# Or watch continuously (every 4 minutes)
watch -n 240 bash scripts/check_wave7_status.sh
```

### Step 4: Handle Failures (If Any)

If failures detected:

1. **Check failure analysis**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
       --command="cat /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-W7-XXX/failure-analysis.md"
   ```

2. **Review Lamport events**:
   ```bash
   grep "EPIC-W7-XXX" .lamport/wave7/event_log.jsonl | grep "phase_fail"
   ```

3. **Fix issues** (manual intervention required)

4. **Re-run failed epic**:
   ```bash
   # The recovery loop will handle this automatically
   # Or manually re-launch specific epic
   ```

### Step 5: Post-Wave Validation

After 161/161 completion:

```bash
# Sync from VM to local
powershell -File .\deploy-sync.ps1

# Run pre-push validation (all 13 checks)
powershell -File .\scripts\pre_push_validation.ps1

# Verify complexity reduction
python scripts/complexity_audit.py

# Test in NinjaTrader
# 1. Open NinjaTrader
# 2. Press F5 (compile)
# 3. Verify zero errors
```

---

## Monitoring & Observability

### Lamport Event Log

**Location**: `.lamport/wave7/event_log.jsonl`

**Event Types**:
- `wave_start` - Wave 7 execution begins
- `epic_start` - Epic begins (Phase 0 start)
- `phase_start` - Phase execution begins
- `phase_complete` - Phase execution succeeds
- `phase_fail` - Phase execution fails
- `epic_complete` - Epic completes (Phase 6 complete)
- `wave_complete` - All 161 epics complete

**Query Examples**:
```bash
# Count completed epics
grep '"event_type": "epic_complete"' .lamport/wave7/event_log.jsonl | wc -l

# Find failed phases
grep '"event_type": "phase_fail"' .lamport/wave7/event_log.jsonl

# Get epic timeline
grep "EPIC-W7-042" .lamport/wave7/event_log.jsonl | jq -r '[.timestamp, .event_type, .phase] | @tsv'
```

### VM Logs

**Location**: `/home/malhitticrypto/universal-or-strategy/logs/`

**Log Files**:
- `EPIC-W7-XXX-phase0.log` - Phase 0 execution log
- `EPIC-W7-XXX-phase1.log` - Phase 1 execution log
- etc.

**Access**:
```bash
# View specific log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="tail -100 /home/malhitticrypto/universal-or-strategy/logs/EPIC-W7-042-phase0.log"

# Search for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="grep -i 'error\|failed\|exception' /home/malhitticrypto/universal-or-strategy/logs/EPIC-W7-*-phase*.log"
```

---

## Cost Tracking

### Bobcoin Usage

**Per API**:
- Anthropic Claude (Bob Shell)
- OpenAI GPT
- Google Gemini
- Groq

**Tracking**:
- Logged in Lamport events (metadata field)
- Aggregated in wave completion report
- Per-phase breakdown available

**Expected Cost** (161 epics):
- Phase 0: ~$50 (hotspot analysis)
- Phase 1: ~$75 (scope definition)
- Phase 1.5: ~$40 (boundary validation)
- Phase 2: ~$125 (architecture planning)
- Phase 3: ~$60 (DNA audit)
- Phase 4: ~$40 (ticket generation)
- Phase 5: ~$400 (ticket execution - varies by complexity)
- Phase 5.V: ~$120 (verification)
- Phase 6: ~$80 (final review)
- **Total**: ~$990 (estimated)

**Cost Optimization**:
- 4-minute polling = 88% reduction vs 30-second polling
- Cache hits = 90% of polls after first
- Parallel execution = zero polling overhead

---

## Recovery Protocol

### Automatic Recovery

The `launch_wave7.sh` script includes automatic recovery:

1. **Detection**: Polls detect failed epics via manifest status
2. **Analysis**: Creates `failure-analysis.md` if missing
3. **Notification**: Logs warning and pauses wave
4. **Manual Fix**: User fixes issues
5. **Re-run**: User re-launches wave (resumes from failures)

### Manual Recovery

If automatic recovery insufficient:

```bash
# 1. Identify failed epic
bash scripts/check_wave7_status.sh | grep "Failed:"

# 2. Analyze failure
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cat /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-W7-XXX/failure-analysis.md"

# 3. Fix issue (depends on failure type)
# - UTF-8 encoding: Fix file encoding
# - xUnit violation: Regenerate tests
# - Build failure: Fix compilation errors
# - Scope creep: Revert out-of-scope changes

# 4. Re-run epic
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto/universal-or-strategy && docs/brain/EPIC-W7-XXX/_phase0.sh"

# 5. Verify completion
bash scripts/check_wave7_status.sh
```

---

## Success Criteria

### Wave Completion
- ✅ 161/161 epics complete (100%)
- ✅ All phases pass for all epics
- ✅ Build passes on VM and local
- ✅ F5 in NinjaTrader succeeds
- ✅ All methods CYC ≤8
- ✅ Zero UTF-8 violations
- ✅ Zero xUnit framework violations
- ✅ Lamport events logged for all phases
- ✅ Cost within budget (~$990)

### Quality Gates
- ✅ Pre-push validation passes (13/13 checks)
- ✅ CodeScene score ≤8 for all files
- ✅ Codacy grade maintained or improved
- ✅ Zero compilation errors
- ✅ Zero test failures
- ✅ Zero scope creep violations

---

## Troubleshooting

### Issue: Wave won't start

**Symptoms**: Pre-flight checks fail

**Solutions**:
```bash
# Check VM connectivity
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="echo 'test'"

# Verify roadmap exists
ls -la epic_roadmap_wave7.json

# Verify templates exist
ls -la building-blocks/wave7/

# Verify Lamport infrastructure
ls -la .lamport/wave7/
```

### Issue: Epics stuck in "running"

**Symptoms**: No progress for >1 hour

**Solutions**:
```bash
# Check active screen sessions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="screen -ls"

# Check for frozen sessions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="ps aux | grep bob"

# Kill stuck session
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="screen -X -S EPIC-W7-XXX quit"

# Re-launch epic
# (handled by recovery loop)
```

### Issue: High failure rate

**Symptoms**: >10% epics failing

**Solutions**:
1. Check common failure patterns in Lamport log
2. Review failure-analysis.md files
3. Fix systemic issues (e.g., template bugs)
4. Update templates if needed
5. Re-run failed epics

### Issue: VM disk full

**Symptoms**: Disk usage >90%

**Solutions**:
```bash
# Check disk usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="df -h"

# Clean up logs
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto/universal-or-strategy && rm -f logs/*.log"

# Clean up temp files
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto/universal-or-strategy && rm -rf temp_*"
```

---

## References

- **Execution Plan**: `docs/workflow/WAVE7_EXECUTION_PLAN.md`
- **Polling Protocol**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`
- **Building-Blocks Method**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Lamport Events**: `.lamport/wave7/README.md`
- **Epic Roadmap**: `epic_roadmap_wave7.json`
- **Templates**: `building-blocks/wave7/`

---

## Next Steps

1. **Review this README**
2. **Run pre-flight checks**
3. **Launch Wave 7**: `bash scripts/launch_wave7.sh`
4. **Monitor progress**: `bash scripts/check_wave7_status.sh`
5. **Handle failures** (if any)
6. **Post-wave validation**
7. **Create PR for Wave 7**

---

**Last Updated**: 2026-06-21  
**Version**: 1.0  
**Status**: Ready for Execution