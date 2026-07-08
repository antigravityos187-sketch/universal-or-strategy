# Wave 4 Phase 3 Preparation Guide

**Date**: 2026-06-15
**Phase**: Phase 3 (DNA & PR Audit)
**Status**: 🔄 PREPARATION
**Prerequisites**: Phase 2 complete (84/80 files, 100% success)

---

## Phase 3 Overview

**Purpose**: V12 DNA compliance checks and PR hygiene validation

**Mode**: `advanced` (requires MCP tools: jCodemunch, Graphify)

**Command**: `epic-scan`

**Input**: `02-architecture-plan.md` (from Phase 2)

**Output**: `03-audit-report.md`

**Duration**: ~10 minutes per epic

**Budget**: 5-10 bobcoins per epic (~400-800 total for 80 epics)

**Jane Street KB**: ⚠️ RECOMMENDED (query for V12 DNA rules)

**Sequential Thinking**: ✅ MANDATORY (compliance verification requires systematic checks)

---

## Pre-Phase 3 Checklist

### 1. File Sync (CRITICAL)

**Sync Phase 2 Files to Local**:
```bash
# Sync architecture plans
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-*/02-architecture-plan.md ./docs/brain/ --zone=us-central1-a

# Verify sync
ls docs/brain/EPIC-CCN-*/02-architecture-plan.md | wc -l
# Expected: 84 files
```

**Why Critical**: Phase 3 scripts will read `02-architecture-plan.md` as input. Files must exist on VM.

### 2. Bobcoin Usage Extraction

**Extract Complete Usage**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase2/*.log > phase2_bobcoin_usage.txt"

# Download report
gcloud compute scp v12-test-golden-v2:~/universal-or-strategy/phase2_bobcoin_usage.txt ./ --zone=us-central1-a
```

**Analysis**:
- Calculate total cost (sum all costs)
- Verify all APIs remain positive (>10 bobcoins)
- Update tracking document

### 3. Epic Roadmap Update

**Update Status**:
```json
{
  "epic_id": "EPIC-CCN-001",
  "phase_0": "complete",
  "phase_1": "complete",
  "phase_2": "complete",  // ← UPDATE THIS
  "phase_3": "pending",
  "status": "in_progress"
}
```

**File**: `epic_roadmap_wave4_fresh.json`

### 4. Environment Validation

**VM Health Check**:
```bash
# 1. VM running?
gcloud compute instances list --filter="name=v12-test-golden-v2"

# 2. Build passes?
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && dotnet build"

# 3. jCodemunch index current?
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && python3 -c 'import sys; sys.path.insert(0, \"scripts\"); from query_kb import query_kb; print(query_kb(\"test\"))'"

# 4. Jane Street KB accessible?
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && python3 scripts/query_kb.py 'V12 DNA'"
```

### 5. MCP Configuration Validation

**Verify Sequential Thinking**:
```bash
# Check Linux-compatible config exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cat universal-or-strategy/.bob/mcp.linux.json | grep sequential-thinking"

# Test MCP server
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="npx -y @modelcontextprotocol/server-sequential-thinking --version"
```

---

## Phase 3 Script Generation (Building-Blocks Method)

### Step 1: Copy Phase 2 Scripts

**Local Machine**:
```bash
# Navigate to scripts directory
cd scripts/wave4/

# Copy Phase 2 scripts to Phase 3
for i in {001..080}; do
  cp _p2_${i}.sh _p3_${i}.sh
done

# Copy launcher
cp launch_phase2_all.sh launch_phase3_all.sh
cp launch_phase2_test.sh launch_phase3_test.sh
```

### Step 2: Find-and-Replace (Phase-Specific Changes)

**Changes Required**:

1. **Mode**: `plan` → `advanced`
   ```bash
   sed -i 's/--chat-mode plan/--chat-mode advanced/g' _p3_*.sh
   ```

2. **Phase Number**: `2` → `3`
   ```bash
   sed -i 's/phase2/phase3/g' _p3_*.sh
   sed -i 's/Phase 2/Phase 3/g' _p3_*.sh
   sed -i 's/p2-/p3-/g' _p3_*.sh
   ```

3. **Input File**: `02-architecture-plan.md`
   ```bash
   # No change needed - Phase 3 reads Phase 2 output
   ```

4. **Output File**: `02-architecture-plan.md` → `03-audit-report.md`
   ```bash
   sed -i 's/02-architecture-plan.md/03-audit-report.md/g' _p3_*.sh
   ```

5. **Command**: `epic-plan` → `epic-scan`
   ```bash
   sed -i 's/epic-plan/epic-scan/g' _p3_*.sh
   ```

6. **Purpose**: Update description
   ```bash
   sed -i 's/Architecture Planning/DNA \& PR Audit/g' _p3_*.sh
   ```

### Step 3: Update Launcher Scripts

**launch_phase3_all.sh**:
```bash
sed -i 's/phase2/phase3/g' launch_phase3_all.sh
sed -i 's/Phase 2/Phase 3/g' launch_phase3_all.sh
sed -i 's/p2-/p3-/g' launch_phase3_all.sh
```

**launch_phase3_test.sh**:
```bash
sed -i 's/phase2/phase3/g' launch_phase3_test.sh
sed -i 's/Phase 2/Phase 3/g' launch_phase3_test.sh
sed -i 's/p2-/p3-/g' launch_phase3_test.sh
```

### Step 4: Verify Changes with Diff

**Compare Against Phase 2**:
```bash
# Check one script
diff _p2_001.sh _p3_001.sh

# Expected differences:
# - Mode: plan → advanced
# - Phase: 2 → 3
# - Command: epic-plan → epic-scan
# - Output: 02-architecture-plan.md → 03-audit-report.md
# - Purpose: Architecture Planning → DNA & PR Audit
```

**Validation**: Only phase-specific changes, no structural changes.

### Step 5: Line Ending Conversion (Windows → Linux)

**Convert CRLF to LF**:
```bash
# If generated on Windows
dos2unix _p3_*.sh launch_phase3_*.sh

# OR use sed
sed -i 's/\r$//' _p3_*.sh launch_phase3_*.sh
```

### Step 6: Set Permissions

**Make Executable**:
```bash
chmod +x _p3_*.sh launch_phase3_*.sh
```

---

## Phase 3 Upload to VM

### Upload Scripts

```bash
# Upload individual epic scripts (80 files)
gcloud compute scp scripts/wave4/_p3_*.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Upload launcher scripts (2 files)
gcloud compute scp scripts/wave4/launch_phase3_all.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave4/launch_phase3_test.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a

# Verify upload
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ls -lh _p3_*.sh | wc -l && ls -lh launch_phase3_*.sh"
# Expected: 80 epic scripts + 2 launchers
```

### Set Permissions on VM

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && chmod +x _p3_*.sh launch_phase3_*.sh"
```

---

## Phase 3 Pilot Test

### Pilot Test #1: EPIC-CCN-001

**Purpose**: Validate Phase 3 script before full wave launch

**Launch**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ./launch_phase3_test.sh"
```

**Monitor** (check after 1 minute, then every 4 minutes):
```bash
# Check screen session
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# Check file created
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh universal-or-strategy/docs/brain/EPIC-CCN-001/03-audit-report.md"

# Check log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -50 universal-or-strategy/logs/phase3/EPIC-CCN-001.log"
```

**Success Criteria**:
1. ✅ Screen session completes (no socket found)
2. ✅ File exists: `docs/brain/EPIC-CCN-001/03-audit-report.md`
3. ✅ File size >1K (substantial content)
4. ✅ No errors in log
5. ✅ Bobcoin usage reported (Cost: X.XX)
6. ✅ Sequential thinking MCP used (check log for "sequentialthinking")

**If Pilot Fails**: Debug, fix, re-test before full wave launch.

**If Pilot Succeeds**: Proceed to full wave launch.

---

## Phase 3 Full Wave Launch

### Launch Command

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && nohup ./launch_phase3_all.sh > launch_phase3.log 2>&1 &"
```

**Launch Details**:
- **Epics**: 80 (EPIC-CCN-001 through EPIC-CCN-080)
- **Delay**: Constant 12s between launches
- **Launch Duration**: 16 minutes (05:XX:XX - 05:XX:XX)
- **Execution Duration**: ~10 minutes per epic
- **Peak Concurrency**: ~50 agents

### Monitoring Schedule (V2.0 Protocol)

**Initial Check**: 1 minute after first script launch
**Subsequent Checks**: Every 4 minutes

**Commands**:
```bash
# All-in-one check
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && echo '=== SESSIONS ===' && screen -ls | grep -c 'p3-' && echo '=== FILES ===' && ls docs/brain/EPIC-CCN-*/03-audit-report.md 2>/dev/null | wc -l"

# Detailed check
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ls -lh docs/brain/EPIC-CCN-{001..010}/03-audit-report.md 2>/dev/null"

# Bobcoin sample
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase3/EPIC-CCN-{001..005}.log 2>/dev/null | head -10"
```

---

## Phase 3 Success Criteria

### Per Epic
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/03-audit-report.md`
- ✅ File size >1K (substantial content)
- ✅ DNA compliance checks documented
- ✅ PR hygiene validation documented
- ✅ Bobcoin usage <10 per epic
- ✅ No P0 blockers

### Wave Completion
- ✅ 80/80 files created (100% success rate)
- ✅ Total bobcoin usage <800 (within budget)
- ✅ All APIs remain positive (>10 bobcoins)
- ✅ Sequential thinking MCP validated in logs
- ✅ No wave-wide failures

---

## Phase 3 Completion Actions

### Immediate (After Phase 3)

1. **Sync Files to Local**:
   ```bash
   gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-*/03-audit-report.md ./docs/brain/ --zone=us-central1-a
   ```

2. **Extract Bobcoin Usage**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase3/*.log > phase3_bobcoin_usage.txt"
   ```

3. **Create Completion Report**:
   - File count and success rate
   - Bobcoin usage analysis
   - Issues encountered and resolutions
   - Lessons learned
   - Next steps for Phase 4

4. **Update Epic Roadmap**:
   - Mark Phase 3 complete for all 80 epics
   - Update `epic_roadmap_wave4_fresh.json`

---

## Key Differences from Phase 2

| Aspect | Phase 2 | Phase 3 |
|--------|---------|---------|
| **Mode** | `plan` | `advanced` |
| **Command** | `epic-plan` | `epic-scan` |
| **Input** | `01-scope-boundary.md` | `02-architecture-plan.md` |
| **Output** | `02-architecture-plan.md` | `03-audit-report.md` |
| **Duration** | ~25 min/epic | ~10 min/epic |
| **Budget** | ~2.5 bobcoins/epic | ~5-10 bobcoins/epic |
| **MCP Tools** | jCodemunch (optional) | jCodemunch + Graphify (required) |
| **Jane Street** | ✅ MANDATORY | ⚠️ RECOMMENDED |

---

## Troubleshooting

### Issue: "advanced mode not found"
- **Cause**: Custom mode not configured
- **Fix**: Verify `.bob/custom_modes.yaml` has `advanced` mode defined

### Issue: "jCodemunch MCP error"
- **Cause**: Windows binary on Linux VM
- **Impact**: Non-blocking (jCodemunch not critical for Phase 3)
- **Action**: Accept and continue

### Issue: "File not found: 02-architecture-plan.md"
- **Cause**: Phase 2 files not synced to VM
- **Fix**: Run file sync command (see Pre-Phase 3 Checklist #1)

### Issue: "Sequential thinking MCP not found"
- **Cause**: `.bob/mcp.linux.json` not deployed
- **Fix**: Upload MCP config to VM (already done in Phase 2)

---

## Timeline Estimate

| Task | Duration |
|------|----------|
| **Pre-Phase 3 Checklist** | 30 minutes |
| **Script Generation** | 15 minutes |
| **Upload to VM** | 5 minutes |
| **Pilot Test** | 15 minutes |
| **Full Wave Launch** | 16 minutes |
| **Execution** | ~10 minutes/epic (parallel) |
| **Monitoring** | ~30 minutes (7 checks × 4 min) |
| **Completion Actions** | 30 minutes |
| **TOTAL** | ~2.5 hours |

---

## Next Phase Preview

**Phase 4** (Ticket Generation):
- Mode: `plan`
- Command: `epic-tickets`
- Input: `03-audit-report.md`
- Output: `04-tickets.md`
- Duration: ~10 minutes per epic
- Budget: ~5-10 bobcoins per epic

---

## References

- **Phase 2 Completion Report**: `WAVE4_PHASE2_COMPLETION_REPORT.md`
- **Building-Blocks SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **10-Phase Workflow**: `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`
- **Polling Protocol V2.0**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md`
- **GCP VM Wave Execution**: `.bob/skills/gcp-vm-wave-execution/skill.md`

---

**Document Version**: 1.0
**Last Updated**: 2026-06-15T07:00:00Z
**Maintainer**: Wave 4 Execution Lead
**Status**: Ready for Phase 3 preparation