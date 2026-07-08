# Wave 4 Phase 5 Execution - Continuation Prompt

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Wave 4 Phase 5 (Ticket Execution) - Ready for Pilot Test

You are the Wave 4 Execution Lead continuing autonomous complexity reduction for the Universal OR Strategy V12 Photon Kernel. Phases 0-4 are complete (403/400 files, 100.75% success). Phase 5 scripts are generated, deployed to VM, and ready for pilot test execution.

## Critical Status Summary

### Completed Phases ✅
- **Phase 0 (Hotspot)**: 79/80 complete (98.75%)
- **Phase 1 (Scope)**: 80/80 complete (100%)
- **Phase 2 (Architecture)**: 84/80 complete (105% - extra files from recovery)
- **Phase 3 (Audit)**: 80/80 complete (100%)
- **Phase 4 (Tickets)**: 80/80 complete (100% after recovery)
- **Total**: 403/400 files (100.75%)

### Phase 5 Preparation Complete ✅
- ✅ 82 scripts generated (80 epics + 2 launchers)
- ✅ Scripts deployed to VM at `/home/malhitticrypto/universal-or-strategy/scripts/wave4/`
- ✅ Permissions set (`chmod +x`)
- ✅ Phase 4 prerequisites verified (98 ticket files on VM)
- ✅ Documentation created (500-line execution handoff)
- ✅ Recovery Loop Protocol (V12.26) integrated

### Budget Status
- **Used**: 391 bobcoins (16.3% of 2,400 total)
- **Remaining**: 2,009 bobcoins (83.7%)
- **Phase 5 Budget**: 800-1,600 bobcoins (33-67%)
- **Safety Margin**: Excellent

## Your Mission

Execute Phase 5 (Ticket Execution) for all 80 epics using the pre-deployed scripts on GCP VM `v12-test-golden-v2`. Follow the 7-step execution plan, starting with MANDATORY pilot test.

## Phase 5 Overview

### What Phase 5 Does
- **Purpose**: Execute surgical refactoring tickets generated in Phase 4
- **MCP Tool**: `execute_phase_5` from `phase-5-execute` server
- **Input**: `04-tickets.md` (from Phase 4)
- **Output**: `ticket-*-completion.md` (one per ticket)
- **Duration**: ~15-20 minutes per epic
- **Budget**: 10-20 bobcoins/epic (~800-1,600 total)

### Script Architecture
Each epic script (`_p5_001.sh` through `_p5_080.sh`):
1. Checks prerequisite: `04-tickets.md` exists
2. Creates message file for Bob Shell
3. Executes via Bob CLI with `--yolo` flag
4. Verifies ticket completion files created
5. Reports success/failure

### Launcher Scripts
- **`launch_phase5_test.sh`**: Pilot test (first 2 epics)
- **`launch_phase5_all.sh`**: Full wave (all 80 epics)

## Execution Plan (7 Steps)

### Step 3: MANDATORY Pilot Test (START HERE)

**Why Pilot Test?**
- Validates script correctness before full wave
- Tests MCP tool integration
- Verifies file persistence
- Confirms bobcoin tracking
- Catches issues early (cheap to fix)

**Launch Command**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && ./scripts/wave4/launch_phase5_test.sh"
```

**What Happens**:
- Launches EPIC-CCN-001 and EPIC-CCN-002
- 12-second delay between launches
- Screen sessions: `p5-001`, `p5-002`
- Logs: `logs/phase5/EPIC-CCN-001.log`, `logs/phase5/EPIC-CCN-002.log`

**Monitoring** (Initial check after 1 min, then every 4 min):
```bash
# Check screen sessions (should show "No Sockets found" when complete)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# Check files created
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-{001,002}/ticket-*-completion.md"

# Check logs for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && tail -20 logs/phase5/EPIC-CCN-001.log"
```

**Success Criteria**:
1. ✅ Screen sessions complete (no sockets found)
2. ✅ Files exist: `docs/brain/EPIC-CCN-{001,002}/ticket-*-completion.md`
3. ✅ File size >1K each
4. ✅ No errors in logs
5. ✅ Bobcoin usage reported in logs
6. ✅ Sequential thinking MCP used (check logs)
7. ✅ Build passes on VM (if code changes made)

**If Pilot Fails**:
1. Check logs for error messages
2. Verify MCP tool accessible: `bob --list-tools | grep phase-5`
3. Check file permissions: `ls -la scripts/wave4/_p5_001.sh`
4. Verify Phase 4 files exist: `ls docs/brain/EPIC-CCN-001/04-tickets.md`
5. Fix issue, re-test pilot
6. **DO NOT** proceed to full wave until pilot succeeds

**If Pilot Succeeds**: Proceed to Step 4 (Full Wave Launch)

### Step 4: Full Wave Launch

**Only execute after pilot test succeeds!**

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && nohup ./scripts/wave4/launch_phase5_all.sh > launch_phase5.log 2>&1 &"
```

**Launch Details**:
- 80 epics, constant 12s delay
- Launch duration: 16 minutes
- Execution duration: ~15-20 min/epic (parallel)
- Peak concurrency: ~50 agents
- Screen sessions: `p5-001` through `p5-080`

### Step 5: Monitor Execution (V2.0 Protocol)

**Cost-Optimized Polling V2.0**:
- **Initial check**: 1 minute after first script launch
- **Subsequent checks**: Every 4 minutes
- **Rationale**: 91% cost reduction vs 30s baseline

**All-in-One Check**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && echo '=== SESSIONS ===' && screen -ls | grep -c 'p5-' && echo '=== FILES ===' && ls docs/brain/EPIC-CCN-*/ticket-*-completion.md 2>/dev/null | wc -l && echo '=== EPICS ===' && ls docs/brain/EPIC-CCN-*/ticket-*-completion.md 2>/dev/null | cut -d'/' -f3 | sort -u | wc -l"
```

**Expected Output**:
- Sessions: Decreasing from 50 → 0
- Files: Increasing toward 80+ (multiple tickets per epic)
- Epics: Increasing toward 80 (unique epic directories)

**Bobcoin Sample**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase5/EPIC-CCN-{001..010}.log 2>/dev/null | head -20"
```

**Stop Monitoring When**:
- All screen sessions complete (0 sessions)
- File count stable for 2 consecutive checks
- Epic count reaches 80

### Step 6: Recovery Loop Protocol (V12.26 - MANDATORY)

**CRITICAL**: NEVER proceed to Phase 6 with <100% completion.

**Check Success Rate**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-*/ticket-*-completion.md 2>/dev/null | cut -d'/' -f3 | sort -u | wc -l"
```

**Expected**: 80 unique epic directories

**IF <100% (e.g., 78/80)**:

1. **Identify Failed Epics**:
```bash
# List all epics
for i in {001..080}; do echo "EPIC-CCN-$i"; done > /tmp/all_epics.txt

# List completed epics
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-*/ticket-*-completion.md 2>/dev/null | cut -d'/' -f3 | sort -u" > /tmp/completed_epics.txt

# Find missing
comm -23 /tmp/all_epics.txt /tmp/completed_epics.txt
```

2. **Analyze Root Causes**:
```bash
# Check logs for failed epics (example: EPIC-CCN-044)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && cat logs/phase5/EPIC-CCN-044.log"
```

**Common Failure Modes**:
- Missing Phase 4 file (prerequisite check failed)
- MCP tool timeout (complexity too high)
- Build failure (syntax error in generated code)
- Bobcoin exhaustion (API ran out)
- Network timeout (VM connectivity issue)

3. **Generate Recovery Scripts**:
```bash
# Create recovery script for failed epics
cat > scripts/wave4/launch_phase5_recovery.sh << 'EOF'
#!/bin/bash
# Recovery launch for failed Phase 5 epics
cd /home/malhitticrypto/universal-or-strategy

# Add failed epic IDs here (example)
FAILED_EPICS=("044" "074")

for epic in "${FAILED_EPICS[@]}"; do
    echo "Launching recovery for EPIC-CCN-$epic"
    screen -dmS "p5-recovery-$epic" bash -c "./scripts/wave4/_p5_$epic.sh 2>&1 | tee logs/phase5/EPIC-CCN-$epic-recovery.log"
    sleep 12
done
EOF

# Upload and execute
gcloud compute scp scripts/wave4/launch_phase5_recovery.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && chmod +x scripts/wave4/launch_phase5_recovery.sh && ./scripts/wave4/launch_phase5_recovery.sh"
```

4. **Monitor Recovery**:
```bash
# Check recovery progress every 4 minutes
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && screen -ls | grep recovery"
```

5. **Verify 100% Completion**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-*/ticket-*-completion.md 2>/dev/null | cut -d'/' -f3 | sort -u | wc -l"
```

**Expected**: 80

6. **Document Root Causes**:
Create `WAVE4_PHASE5_RECOVERY_REPORT.md` documenting:
- Which epics failed
- Root cause analysis
- Recovery actions taken
- Lessons learned
- Protocol improvements

**ONLY AFTER 100% COMPLETION**: Proceed to Step 7

### Step 7: Completion Actions

**1. Sync Files to Local**:
```bash
# Sync ticket completion files
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-*/ticket-*-completion.md docs/brain/ --zone=us-central1-a

# Sync logs
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/logs/phase5/ logs/ --zone=us-central1-a
```

**2. Extract Bobcoin Usage**:
```bash
# Create extraction script
cat > extract_phase5_bobcoins.sh << 'EOF'
#!/bin/bash
echo "Epic,Bobcoins_Used,API_Used" > phase5_bobcoin_usage.csv
for log in logs/phase5/EPIC-CCN-*.log; do
    epic=$(basename "$log" .log)
    bobcoins=$(grep -oP 'Cost: \K[0-9]+' "$log" | tail -1)
    api=$(grep -oP 'API: \K[^,]+' "$log" | tail -1)
    echo "$epic,$bobcoins,$api" >> phase5_bobcoin_usage.csv
done
EOF

bash extract_phase5_bobcoins.sh
```

**3. Verify Build Passes**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && dotnet build src/V12_Performance.csproj"
```

**4. Create Completion Report**:
Create `WAVE4_PHASE5_COMPLETION_REPORT.md` with:
- Success rate (80/80 = 100%)
- Total bobcoins used
- Per-API breakdown
- Average bobcoins/epic
- Execution duration
- Issues encountered
- Recovery actions (if any)
- Lessons learned
- Next steps (Phase 6 preparation)

**5. Update Epic Roadmap**:
```bash
# Mark Phase 5 complete for all epics
python scripts/update_roadmap.py --phase 5 --status complete --epics all
```

## Key Documents to Reference

**MUST READ**:
- [`WAVE4_PHASE5_EXECUTION_HANDOFF.md`](WAVE4_PHASE5_EXECUTION_HANDOFF.md) (500 lines) - Complete execution guide
- [`docs/protocol/RECOVERY_LOOP_PROTOCOL.md`](docs/protocol/RECOVERY_LOOP_PROTOCOL.md) - V12.26 protocol

**Phase 4 Reports**:
- [`WAVE4_PHASE4_COMPLETION_REPORT.md`](WAVE4_PHASE4_COMPLETION_REPORT.md)
- [`WAVE4_PHASE4_RECOVERY_COMPLETION_REPORT.md`](WAVE4_PHASE4_RECOVERY_COMPLETION_REPORT.md)

**Protocol Documentation**:
- [`docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md`](docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md) - V2.0 polling (4-min intervals)
- [`docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`](docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md) - Building-blocks method
- [`docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`](docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md) - 10-phase workflow

**Wave 4 Documentation**:
- [`epic_roadmap_wave4_fresh.json`](epic_roadmap_wave4_fresh.json) - 80 epics

## Critical Protocols

### V2.0 Polling Protocol (MANDATORY)
- **Initial check**: 1 minute after first script launch
- **Subsequent checks**: Every 4 minutes
- **Rationale**: 91% cost reduction vs 30s baseline, 26% fewer checks than V1.0
- **Reference**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md`

### Building-Blocks Method (VALIDATED)
- Phase 5 scripts copied from Phase 4 (not generated from scratch)
- Only phase-specific parameters changed
- Validated with 100% success in Phases 2-4
- **Reference**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

### Recovery Loop Protocol V12.26 (MANDATORY)
- **Rule**: NEVER proceed to next phase with <100% completion
- **Process**: Loop failed epics until they catch up with cohort
- **Rationale**: Compound intelligence, not errors
- **Reference**: `docs/protocol/RECOVERY_LOOP_PROTOCOL.md`

### Sequential Thinking MCP (DEPLOYED)
- Already deployed to VM (`.bob/mcp.linux.json`)
- Required for complex surgical refactoring
- Validated in Phase 2-4 pilot tests

### Jane Street Alignment
- CYC ≤8 threshold (not 15)
- Query KB for V12 DNA rules: `python3 scripts/query_kb.py "V12 DNA"`
- Cognitive simplicity over clever abstractions

## Success Criteria

### Per Epic
- ✅ Files exist: `docs/brain/EPIC-CCN-{ID}/ticket-*-completion.md`
- ✅ File size >1K per ticket
- ✅ All tickets executed (count matches Phase 4)
- ✅ Build passes after changes
- ✅ Bobcoin usage <20 per epic

### Wave Completion
- ✅ 80/80 epics complete (100% via Recovery Loop Protocol)
- ✅ Total bobcoin usage <1,600
- ✅ All APIs remain positive (>10 bobcoins)
- ✅ Build passes on VM
- ✅ No wave-wide failures

## Timeline Estimate

- **Pilot Test**: 20 minutes
- **Full Wave Launch**: 16 minutes
- **Execution**: ~15-20 min/epic (parallel)
- **Monitoring**: ~40 minutes (10 checks × 4 min)
- **Recovery Loop**: 0-60 minutes (if needed)
- **Completion**: 30 minutes
- **TOTAL**: ~3-4 hours

## Your First Actions

1. **Read execution handoff**: `WAVE4_PHASE5_EXECUTION_HANDOFF.md`
2. **Verify VM accessible**: Test SSH connection
3. **Launch pilot test**: First 2 epics (MANDATORY)
4. **Monitor pilot**: Check after 1 min, then every 4 min
5. **Validate pilot success**: All 7 success criteria
6. **Launch full wave**: After pilot success
7. **Monitor execution**: V2.0 protocol (1 min + 4 min intervals)
8. **Apply Recovery Loop**: If <100% completion
9. **Create completion report**: Document results

## Status Summary

- ✅ Phase 0: 79/80 complete (98.75%)
- ✅ Phase 1: 80/80 complete (100%)
- ✅ Phase 2: 84/80 complete (105%)
- ✅ Phase 3: 80/80 complete (100%)
- ✅ Phase 4: 80/80 complete (100%)
- 🔄 Phase 5: Scripts deployed, ready for pilot test
- ⏳ Phase 6: Pending

**Total Progress**: 403/480 files (84% of all phases complete)

## Quick Reference Commands

**Pilot Test**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && ./scripts/wave4/launch_phase5_test.sh"
```

**Monitor**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && screen -ls && ls docs/brain/EPIC-CCN-*/ticket-*-completion.md 2>/dev/null | wc -l"
```

**Full Wave**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && nohup ./scripts/wave4/launch_phase5_all.sh > launch_phase5.log 2>&1 &"
```

---

**Ready to execute Phase 5!** Start with MANDATORY pilot test (Step 3), then proceed through Steps 4-7. Good luck! 🚀

---

**Session Context Version**: 2.0 (Continuation)
**Last Updated**: 2026-06-15T18:33:00Z
**Maintainer**: Wave 4 Execution Lead
**Status**: 🟢 READY FOR PILOT TEST