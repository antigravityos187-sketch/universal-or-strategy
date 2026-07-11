# Wave 1 Phase 2 Execution Guide: 80+ Epics at Scale

**Date**: 2026-06-14
**Phase**: Architecture Planning
**Strategy**: Rolling launch with 30-second delays
**VM**: v12-test-golden-v2 (n2-standard-8)

## Quick Start

### Step 1: Generate Phase 2 Scripts (Local)

```bash
# Generate all Phase 2 scripts from epic roadmap
python scripts/wave1/generate_phase2_all_epics.py
```

**Expected Output**:
```
Found 80+ pending epics
Loaded 10 API keys
Generated: _p2_001.sh (API key 1)
Generated: _p2_002.sh (API key 2)
...
Generated 80+ Phase 2 scripts
```

### Step 2: Upload to VM

```bash
# Upload Phase 2 scripts
gcloud compute scp scripts/wave1/_p2_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Upload launcher
gcloud compute scp scripts/wave1/launch_phase2_rolling.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Upload monitor
gcloud compute scp scripts/wave1/monitor_phase2_progress.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

### Step 3: Launch Rolling Execution

```bash
# Make scripts executable and launch
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && chmod +x launch_phase2_rolling.sh monitor_phase2_progress.sh _p2_*.sh && ./launch_phase2_rolling.sh"
```

**Expected Timeline**:
- Launch phase: ~40 minutes (80 epics × 30 sec)
- Execution phase: ~2 minutes per epic (overlapping)
- Total duration: ~45-50 minutes

### Step 4: Monitor Progress

**Option A: Real-time Dashboard (Recommended)**
```bash
# SSH to VM and run monitor
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a

# Once connected:
cd ~/universal-or-strategy
./monitor_phase2_progress.sh
```

**Option B: Quick Status Checks**
```bash
# Running agents
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c 'p2-'"

# Completed epics
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l"

# VM load
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="uptime && free -h"
```

## Monitoring Commands

### Check Running Agents
```bash
# List all screen sessions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# Count running agents
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c 'p2-'"
```

### Check Completion Status
```bash
# Count completed epics
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l"

# List recent completions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lt /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | head -10"
```

### Check VM Health
```bash
# CPU load (expect <2.0)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="uptime"

# Memory usage (expect <50%)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="free -h"

# Disk usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="df -h /home"
```

### View Logs
```bash
# View specific log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -100 /home/malhitticrypto/universal-or-strategy/logs/phase2/EPIC-001.log"

# Check for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i 'error\|failed\|exception' /home/malhitticrypto/universal-or-strategy/logs/phase2/*.log | head -20"

# Attach to running agent (Ctrl+A, D to detach)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -r p2-001"
```

## Expected Behavior

### Launch Phase (0-40 minutes)
- New agent launches every 30 seconds
- Screen sessions appear: `p2-001`, `p2-002`, etc.
- VM load gradually increases to ~1.0-1.5
- Memory usage increases to ~10-15 GB

### Execution Phase (40-50 minutes)
- Agents complete and exit (~2 min each)
- Screen sessions disappear as agents finish
- Architecture plan files appear in `docs/brain/EPIC-*/`
- VM load stabilizes around peak concurrency (~15-20 agents)

### Completion (50+ minutes)
- All screen sessions gone (`screen -ls` shows "No Sockets found")
- 80+ architecture plan files created
- VM load returns to idle (~0.1)
- Memory usage drops to baseline (~4 GB)

## Success Criteria

### Per Epic
- ✅ File created: `docs/brain/EPIC-XXX/02-architecture-plan.md`
- ✅ Manifest updated: `manifest.json` phase = "2"
- ✅ Bobcoin usage reported in log
- ✅ No errors in log file

### Wave Completion
- ✅ 80/80 epics complete (100% success rate)
- ✅ Total bobcoins <500 (31% of budget)
- ✅ VM load remained <2.0 throughout
- ✅ No API rate limit errors
- ✅ All files verified on disk

## Troubleshooting

### Issue: Agent Fails to Start
**Symptom**: Screen session doesn't appear after launch
**Check**: `screen -ls` shows no session for that epic
**Solution**: Relaunch manually:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && screen -dmS p2-001 bash -l -c './_p2_001.sh 2>&1 | tee logs/phase2/EPIC-001.log'"
```

### Issue: VM Load Too High
**Symptom**: `uptime` shows load >3.0
**Check**: `screen -ls | grep -c 'p2-'` shows >30 agents
**Solution**: Pause launches, wait for agents to complete
```bash
# Kill launcher if still running
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="pkill -f launch_phase2_rolling"

# Wait for agents to complete
# Resume launches after load drops <1.5
```

### Issue: File Not Created
**Symptom**: Agent completes but no architecture plan file
**Check**: Log shows "files created" but `ls` shows nothing
**Root Cause**: Missing `--yolo` flag (should not happen - already in scripts)
**Solution**: Relaunch with explicit verification:
```bash
# Check if file exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-001/02-architecture-plan.md"

# If missing, relaunch
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && ./_p2_001.sh"
```

### Issue: API Rate Limit
**Symptom**: Logs show "429 Too Many Requests" or "Rate limit exceeded"
**Check**: Multiple agents hitting jCodemunch simultaneously
**Solution**: Increase launch delay to 60 seconds
```bash
# Edit launcher on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a

# Once connected:
cd ~/universal-or-strategy
sed -i 's/DELAY=30/DELAY=60/' launch_phase2_rolling.sh

# Relaunch remaining epics
./launch_phase2_rolling.sh
```

## Post-Completion Actions

### Step 1: Verify All Files Created
```bash
# Count architecture plans
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l"

# Expected: 80+ files
```

### Step 2: Extract Bobcoin Usage
```bash
# Download logs
gcloud compute scp v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/logs/phase2/*.log ./logs/phase2/ --zone=us-central1-a

# Extract usage locally
grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase2/*.log > phase2_bobcoin_usage.txt

# Analyze
python scripts/wave1/analyze_bobcoin_usage.py phase2_bobcoin_usage.txt
```

### Step 3: Update Roadmap
```bash
# Mark Phase 2 complete for all epics
python scripts/wave1/update_roadmap_phase2.py
```

### Step 4: Document Results
Create `WAVE1_PHASE2_COMPLETION_REPORT.md` with:
- Total epics completed
- Bobcoin usage (total, average, per-API)
- Execution time (actual vs estimated)
- VM performance (peak load, memory)
- Lessons learned
- Recommendations for Phase 3

## Next Steps

### Phase 3: DNA & PR Audit
- **Same Pattern**: Rolling launch with 30-second delays
- **Estimated**: 5-10 bobcoins/epic
- **Total**: 400-800 bobcoins (25-50% of budget)
- **Mode**: `advanced` (requires MCP tools)

### Phase 4: Ticket Generation
- **Same Pattern**: Rolling launch with 30-second delays
- **Estimated**: 5-10 bobcoins/epic
- **Total**: 400-800 bobcoins (25-50% of budget)
- **Mode**: `plan`

## Budget Tracking

### Phase 0-2 Projection
| Phase | Epics | Bobcoins/Epic | Total | % of 1,600 |
|-------|-------|---------------|-------|------------|
| Phase 0 | 80 | 1.49 | 120 | 7.5% |
| Phase 1 | 80 | 1.17 | 95 | 6.0% |
| Phase 2 | 80 | 3.00 | 240 | 15.0% |
| **Total** | **240** | **1.89** | **455** | **28.4%** |

**Remaining**: 1,145 bobcoins (71.6%)

### Phase 0-4 Projection
| Phase | Epics | Bobcoins/Epic | Total | % of 1,600 |
|-------|-------|---------------|-------|------------|
| Phase 0-2 | 240 | 1.89 | 455 | 28.4% |
| Phase 3 | 80 | 7.50 | 600 | 37.5% |
| Phase 4 | 80 | 7.50 | 600 | 37.5% |
| **Total** | **400** | **4.14** | **1,655** | **103.4%** |

**Note**: Phase 3-4 may exceed budget. Monitor closely and adjust strategy if needed.

## Emergency Procedures

### Stop All Agents
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="killall screen"
```

### Relaunch Failed Epics
```bash
# Get list of incomplete epics
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="comm -23 <(ls _p2_*.sh | sed 's/_p2_\([0-9]*\)\.sh/\1/' | sort) <(ls docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | sed 's/.*EPIC-\([0-9]*\)\/.*/\1/' | sort)"

# Relaunch individually
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && screen -dmS p2-001 bash -l -c './_p2_001.sh 2>&1 | tee logs/phase2/EPIC-001.log'"
```

## Contact

**Questions?** Check:
- `WAVE1_SCALING_STRATEGY.md` - Overall strategy
- `WAVE1_PHASE1_FINAL_REPORT.md` - Phase 1 results
- `.bob/skills/gcp-vm-wave-execution/skill.md` - GCP VM skill documentation

**Status**: Ready for execution