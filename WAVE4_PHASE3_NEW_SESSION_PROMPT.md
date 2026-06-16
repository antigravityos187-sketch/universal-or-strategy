# Wave 4 Phase 3 Execution - New Session Prompt

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Wave 4 Phase 3 (DNA & PR Audit) Execution

You are the Wave 4 Execution Lead continuing autonomous complexity reduction for the Universal OR Strategy V12 Photon Kernel. Phase 2 (Architecture Planning) completed with 100% success (84/80 files). Phase 3 scripts are generated and ready for VM deployment.

## Your Mission

Execute Phase 3 (DNA & PR Audit) for all 80 epics using the pre-generated scripts on GCP VM `v12-test-golden-v2`. Follow the 6-step execution plan in `WAVE4_PHASE3_EXECUTION_HANDOFF.md`.

## Critical Context

### Phase 2 Results (COMPLETE)
- ✅ 84/80 files created (105% success rate)
- ✅ ~199 bobcoins used (~8.3% of 2,400 budget)
- ✅ Average: 2.49 bobcoins/epic
- ✅ Completion time: 1h 43m
- ✅ Building-blocks method validated
- ✅ V2.0 polling protocol (4-min intervals, 91% cost reduction)

### Phase 3 Overview
- **Mode**: Uses `phase-3-audit` MCP tool (not a mode)
- **MCP Tool**: `execute_phase_3` from `phase-3-audit` server
- **Input**: `02-architecture-plan.md` (from Phase 2)
- **Output**: `03-audit-report.md`
- **Duration**: ~10 minutes per epic
- **Budget**: 5-10 bobcoins/epic (~400-800 total)
- **Scripts**: 80 epic scripts + 2 launchers (already generated)

### Phase 3 Scripts Generated ✅
- **Location**: `scripts/wave4/`
- **Files**: `_p3_001.sh` through `_p3_080.sh` (80 epics)
- **Launchers**: `launch_phase3_test.sh`, `launch_phase3_all.sh`
- **Method**: Building-blocks (copied from Phase 2 pattern)

## Execution Plan (6 Steps)

### Step 1: Sync Phase 2 Files to VM (CRITICAL)
Phase 3 scripts read `02-architecture-plan.md` as input. Files must exist on VM.

```bash
# Verify Phase 2 files on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-*/02-architecture-plan.md | wc -l"
# Expected: 84 files

# If missing, sync from local
gcloud compute scp --recurse docs/brain/EPIC-CCN-*/02-architecture-plan.md v12-test-golden-v2:~/universal-or-strategy/docs/brain/ --zone=us-central1-a
```

### Step 2: Upload Phase 3 Scripts
```bash
# Upload epic scripts (80 files)
gcloud compute scp scripts/wave4/_p3_*.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a

# Upload launchers (2 files)
gcloud compute scp scripts/wave4/launch_phase3_test.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a
gcloud compute scp scripts/wave4/launch_phase3_all.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a

# Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy/scripts/wave4 && chmod +x _p3_*.sh launch_phase3_*.sh"

# Verify upload
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy/scripts/wave4 && ls _p3_*.sh | wc -l && ls launch_phase3_*.sh"
# Expected: 80 + 2
```

### Step 3: Pilot Test (MANDATORY)
Test with first 2 epics before full wave launch.

```bash
# Launch pilot
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ./scripts/wave4/launch_phase3_test.sh"

# Monitor (check after 1 min, then every 4 min)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-{001,002}/03-audit-report.md"
```

**Success Criteria**:
1. ✅ Screen sessions complete
2. ✅ Files exist (>1K each)
3. ✅ No errors in logs
4. ✅ Bobcoin usage reported
5. ✅ Sequential thinking MCP used

**If pilot fails**: Debug, fix, re-test. **If pilot succeeds**: Proceed to Step 4.

### Step 4: Full Wave Launch
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && nohup ./scripts/wave4/launch_phase3_all.sh > launch_phase3.log 2>&1 &"
```

**Launch Details**:
- 80 epics, constant 12s delay
- Launch duration: 16 minutes
- Peak concurrency: ~50 agents

### Step 5: Monitoring (V2.0 Protocol)
**Initial check**: 1 min after first launch
**Subsequent checks**: Every 4 minutes

```bash
# All-in-one check
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && echo '=== SESSIONS ===' && screen -ls | grep -c 'p3-' && echo '=== FILES ===' && ls docs/brain/EPIC-CCN-*/03-audit-report.md 2>/dev/null | wc -l"

# Bobcoin sample
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase3/EPIC-CCN-{001..005}.log 2>/dev/null | head -10"
```

**Stop when**: All screen sessions complete + file count reaches 80

### Step 6: Completion Actions
1. Sync files to local
2. Extract bobcoin usage
3. Create completion report
4. Update epic roadmap

## Key Documents to Reference

**MUST READ FIRST**:
- `WAVE4_PHASE3_EXECUTION_HANDOFF.md` (500 lines) - Complete execution guide
- `WAVE4_PHASE3_PREPARATION_GUIDE.md` (450 lines) - Detailed preparation steps

**Phase 2 Reports**:
- `WAVE4_PHASE2_COMPLETION_REPORT.md` (350 lines)
- `WAVE4_PHASE2_PROGRESS_REPORT.md`

**Protocol Documentation**:
- `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md` - V2.0 polling (4-min intervals)
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md` - Building-blocks method
- `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md` - 10-phase workflow

**Wave 4 Documentation**:
- `WAVE4_HANDOFF_CORRECTED.md` - Original Wave 4 handoff
- `epic_roadmap_wave4_fresh.json` - 80 epics

## Critical Protocols

### V2.0 Polling Protocol (MANDATORY)
- **Initial check**: 1 minute after first script launch
- **Subsequent checks**: Every 4 minutes
- **Rationale**: 91% cost reduction vs 30s baseline, 26% fewer checks than V1.0

### Building-Blocks Method (MANDATORY)
- Phase 3 scripts copied from Phase 2 (not generated from scratch)
- Only phase-specific parameters changed
- Validated with 100% success in Phase 2

### Sequential Thinking MCP (MANDATORY)
- Already deployed to VM (`.bob/mcp.linux.json`)
- Required for Phase 3 compliance verification
- Validated in Phase 2 pilot tests

### Jane Street Alignment
- CYC ≤8 threshold (not 15)
- Query KB for V12 DNA rules: `python3 scripts/query_kb.py "V12 DNA"`

## Success Criteria

### Per Epic
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/03-audit-report.md`
- ✅ File size >1K
- ✅ DNA compliance checks documented
- ✅ PR hygiene validation documented
- ✅ Bobcoin usage <10 per epic

### Wave Completion
- ✅ 80/80 files created (100% success rate)
- ✅ Total bobcoin usage <800
- ✅ All APIs remain positive (>10 bobcoins)
- ✅ No wave-wide failures

## Budget Status

- **Phase 2 Used**: ~199 bobcoins (8.3%)
- **Phase 3 Budget**: 400-800 bobcoins (16.7-33.3%)
- **Remaining After Phase 3**: 58-75% (1,400-1,800 bobcoins)
- **Safety Margin**: Excellent

## Timeline Estimate

- **Prerequisites**: 15 min
- **File Sync**: 10 min
- **Script Upload**: 5 min
- **Pilot Test**: 15 min
- **Full Launch**: 16 min
- **Execution**: ~10 min/epic (parallel)
- **Monitoring**: ~30 min (7 checks × 4 min)
- **Completion**: 30 min
- **TOTAL**: ~2.5 hours

## Your First Actions

1. **Read handoff document**: `WAVE4_PHASE3_EXECUTION_HANDOFF.md`
2. **Verify VM accessible**: Test SSH connection
3. **Check Phase 2 files on VM**: Verify 84 files exist
4. **Upload Phase 3 scripts**: 82 files (80 epics + 2 launchers)
5. **Run pilot test**: First 2 epics
6. **Launch full wave**: After pilot success
7. **Monitor execution**: V2.0 protocol (1 min + 4 min intervals)
8. **Create completion report**: Document results

## Status Summary

- ✅ Phase 0: 79/80 complete
- ✅ Phase 1: 80/80 complete
- ✅ Phase 2: 84/80 complete (105% success)
- 🔄 Phase 3: Scripts generated, ready for execution
- ⏳ Phases 4-6: Pending

**Total Progress**: 243/320 files (76% of planning phases complete)

---

**Ready to execute Phase 3!** Start by reading `WAVE4_PHASE3_EXECUTION_HANDOFF.md` for complete context, then follow the 6-step execution plan. Good luck! 🚀

---

**Session Context Version**: 1.0
**Last Updated**: 2026-06-15T07:16:00Z
**Maintainer**: Wave 4 Execution Lead
**Status**: 🟢 READY FOR NEW SESSION