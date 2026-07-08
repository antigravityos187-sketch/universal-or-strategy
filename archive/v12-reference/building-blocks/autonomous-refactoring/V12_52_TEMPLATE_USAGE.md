# V12.52 Phase Script Templates - Usage Guide

**Version**: V12.52
**Date**: 2026-06-17
**Status**: Production Ready

## Overview

V12.52 templates include Lamport Causal Verification gates that ensure deterministic, reproducible workflow execution. Each template follows a 4-step pattern:

1. **V12.52 Verification Gate** - Triple verification (dependencies, Lamport, filesystem)
2. **Start Phase Execution** - Records `phase_start` event with Lamport clock
3. **Execute Phase Work** - Runs Bob CLI or other phase-specific logic
4. **Complete Phase Execution** - Records `phase_complete` event with state hash

## Template Structure

### Phase 0: Hotspot Analysis

**File**: `phase0_template_v12_52.sh`

**Purpose**: Analyze method complexity using jCodemunch hotspots

**Inputs**: None (Phase 0 has no dependencies)

**Outputs**: `docs/brain/{EPIC_ID}/00-hotspots.md`

**Bob Mode**: `plan`

**Usage**:
```bash
# Generate script for EPIC-CCN-001
sed 's/{EPIC_ID}/EPIC-CCN-001/g; s/{AGENT_ID}/wave6-phase0-001/g' \
    phase0_template_v12_52.sh > scripts/wave6/_p0_EPIC-CCN-001.sh

# Upload to VM
gcloud compute scp scripts/wave6/_p0_EPIC-CCN-001.sh \
    v12-test-golden-v2:~/universal-or-strategy/scripts/wave6/ \
    --zone=us-central1-a

# Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="chmod +x ~/universal-or-strategy/scripts/wave6/_p0_EPIC-CCN-001.sh"

# Execute
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd ~/universal-or-strategy && bash -l -c './scripts/wave6/_p0_EPIC-CCN-001.sh'"
```

## V12.52 Verification Gates

### Gate 1: Dependencies (Manifest)

Checks that all prerequisite phases are completed:
- Phase 0: No dependencies
- Phase 1: Requires Phase 0
- Phase 1.5: Requires Phase 1
- Phase 2: Requires Phase 1.5
- etc.

**Failure**: `BLOCKED: Dependencies not satisfied (manifest)`

### Gate 2: Causal Verification (Lamport)

Checks workflow determinism:
- Dependencies satisfied (happens-before)
- State hash matches expected
- No concurrent conflicts

**Failure**: `BLOCKED: Causal verification failed: {reason}`

### Gate 3: Filesystem State (Dual Verification)

Checks manifest-filesystem consistency:
- Expected input artifacts exist
- No stale output artifacts present
- File sizes are non-zero

**Failure**: `BLOCKED: State mismatch: {reason}`

## Error Handling

### Verification Gate Failure

If any gate fails, the script exits immediately with exit code 1:

```bash
❌ BLOCKED: Dependencies not satisfied (manifest)
❌ V12.52 verification failed - aborting
```

**Recovery**: Fix the blocking issue (e.g., complete prerequisite phase) and re-run

### Phase Execution Failure

If Bob CLI or phase work fails, the script records a failure event:

```python
fail_phase_execution(epic_id, phase, agent_id, error_message)
```

This updates the manifest status to `failed` and records a `phase_fail` event in the Lamport log.

**Recovery**: Investigate error, fix issue, reset phase status to `pending`, and re-run

## Lamport Event Log

All phase transitions are recorded in `.lamport/event_log.jsonl`:

```json
{"clock": 1, "event_type": "phase_start", "epic_id": "EPIC-CCN-001", "phase": "0", "agent_id": "wave6-phase0-001", "status": "running", "state_hash": "abc123...", "timestamp": "2026-06-17T18:00:00Z"}
{"clock": 2, "event_type": "phase_complete", "epic_id": "EPIC-CCN-001", "phase": "0", "agent_id": "wave6-phase0-001", "status": "completed", "state_hash": "def456...", "data": {"outputs": ["docs/brain/EPIC-CCN-001/00-hotspots.md"]}, "timestamp": "2026-06-17T18:01:00Z"}
```

**Query Event Log**:
```python
from epic_manifest import get_event_log

# Get all events for an epic
events = get_event_log("EPIC-CCN-001")

# Get events for a specific phase
events = get_event_log("EPIC-CCN-001", "0")
```

## Building-Blocks Method

**MANDATORY**: Always copy from previous wave's working scripts for the SAME phase.

### Step 1: Copy Previous Wave Script

```bash
# Copy Phase 0 script from Wave 5 (if it exists)
cp scripts/wave5/_p0_EPIC-CCN-001.sh scripts/wave6/_p0_EPIC-CCN-001_base.sh
```

### Step 2: Update Epic-Specific Parameters

Use find-and-replace for:
- `{EPIC_ID}` → Actual epic ID (e.g., `EPIC-CCN-001`)
- `{AGENT_ID}` → Agent identifier (e.g., `wave6-phase0-001`)
- Wave number in paths (e.g., `wave5` → `wave6`)

**NEVER** change:
- V12.52 verification gate logic
- Lamport event recording calls
- Error handling structure
- Bob CLI command structure

### Step 3: Verify Script

```bash
# Check for syntax errors
bash -n scripts/wave6/_p0_EPIC-CCN-001.sh

# Verify epic ID and agent ID are correct
grep -E "EPIC_ID=|AGENT_ID=" scripts/wave6/_p0_EPIC-CCN-001.sh
```

## Pilot Testing Protocol

**MANDATORY**: Test ONE script before deploying all 79.

### Step 1: Generate Pilot Script

```bash
sed 's/{EPIC_ID}/EPIC-CCN-001/g; s/{AGENT_ID}/wave6-phase0-001/g' \
    phase0_template_v12_52.sh > scripts/wave6/_p0_EPIC-CCN-001.sh
```

### Step 2: Upload to VM

```bash
gcloud compute scp scripts/wave6/_p0_EPIC-CCN-001.sh \
    v12-test-golden-v2:~/universal-or-strategy/scripts/wave6/ \
    --zone=us-central1-a

gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="chmod +x ~/universal-or-strategy/scripts/wave6/_p0_EPIC-CCN-001.sh"
```

### Step 3: Verify Upload

```bash
LOCAL_COUNT=$(ls scripts/wave6/_p0_*.sh | wc -l)
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="ls ~/universal-or-strategy/scripts/wave6/_p0_*.sh | wc -l")

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete"
    exit 1
fi
```

### Step 4: Execute Pilot

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd ~/universal-or-strategy && bash -l -c './scripts/wave6/_p0_EPIC-CCN-001.sh' | tee logs/wave6/phase0/EPIC-CCN-001.log"
```

### Step 5: Verify Pilot Success

Check for:
- ✅ V12.52 verification passed
- ✅ Phase started (Lamport clock incremented)
- ✅ Bob CLI executed successfully
- ✅ Output file created (`00-hotspots.md`)
- ✅ Phase completed (Lamport event recorded)
- ✅ No errors in log

If pilot fails, fix issue and re-test before deploying remaining 78 scripts.

## Wave 6 Execution Checklist

### Pre-Wave

- [ ] V12.52 implementation tested locally (8/8 tests passing)
- [ ] Phase 0 template created with V12.52 gates
- [ ] VM environment verified (Python, Node.js, Bob CLI, clean git)
- [ ] VM-Local Git Sync complete (7-step protocol)
- [ ] Encoding pre-check passed (UTF-8 compliance)

### Pilot Test

- [ ] Generate pilot script (EPIC-CCN-001)
- [ ] Upload to VM with verification
- [ ] Execute pilot in foreground
- [ ] Verify 5 checks (compilation, complexity, scope, tests, encoding)
- [ ] Verify Lamport events recorded
- [ ] Verify 0 P0/P1 Greptile issues

### Full Wave

- [ ] Generate all 79 scripts (building-blocks method)
- [ ] Upload to VM with verification (count check)
- [ ] Launch with staggered delays (9s base, 40s max)
- [ ] Monitor every 4 minutes (cost-optimized polling)
- [ ] Track bobcoin usage per API
- [ ] Verify 100% completion (Recovery Loop Protocol if <100%)

### Post-Wave

- [ ] Sync VM changes to local
- [ ] Run pre-push validation
- [ ] Create PRs (cluster strategy)
- [ ] Run Greptile audit (expect 0 P0/P1)
- [ ] Update roadmap
- [ ] Document lessons learned

## Cost Tracking

**Per Epic**:
- Phase 0: ~$0.05 (Bob CLI plan mode)

**Wave 6 Total**:
- 79 epics × $0.05 = $3.95

**Monitoring**:
- Cost-optimized polling: 4-minute intervals (91% cost reduction vs 30s)

## References

- **Protocol**: `docs/protocol/V12_52_LAMPORT_CAUSAL_VERIFICATION.md`
- **Implementation**: `docs/protocol/V12_52_IMPLEMENTATION_SUMMARY.md`
- **Test Suite**: `scripts/test_v12_52.py`
- **Lamport Clock**: `scripts/lamport_clock.py`
- **Manifest Integration**: `scripts/epic_manifest.py`
- **Building-Blocks SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`