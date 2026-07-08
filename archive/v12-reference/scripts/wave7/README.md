# Wave 7 Execution Scripts

**Building-Blocks Method**: All scripts copied from Wave 4 pattern and adapted for Wave 7.

## Overview

Wave 7 executes 161 complexity reduction epics (CYC > 8 → CYC ≤ 8) using autonomous refactoring workflows. All execution happens **ON the VM** (not via gcloud ssh).

## Directory Structure

```
scripts/wave7/
├── README.md                      # This file
├── generate_phase0_scripts.py     # Generate Phase 0 scripts for 161 epics
├── launch_phase0_pilot.sh         # Pilot test (3 epics)
├── launch_phase0_all.sh           # Launch all 161 Phase 0 epics
├── check_wave7_status.sh          # Monitor progress across all phases
├── _p0_001.sh through _p0_161.sh  # Generated Phase 0 scripts (161 files)
└── (similar for phases 1, 1.5, 2, 3, 4, 5, 5v, 6)
```

## Quick Start

### Step 1: Generate Phase 0 Scripts

```bash
# On VM: Generate all 161 Phase 0 scripts
cd /home/malhitticrypto/universal-or-strategy
python3 scripts/wave7/generate_phase0_scripts.py
```

**Output**: 161 scripts (`_p0_001.sh` through `_p0_161.sh`) in `scripts/wave7/`

### Step 2: Run Pilot Test

```bash
# Test 3 epics (low/medium/high complexity)
./scripts/wave7/launch_phase0_pilot.sh

# Wait 15 minutes, then verify
screen -ls | grep p0-  # Should show 0 active sessions
ls docs/brain/EPIC-W7-{001,050,100}/00-hotspots.md  # All 3 should exist
```

### Step 3: Launch Full Wave

```bash
# Launch all 161 epics
./scripts/wave7/launch_phase0_all.sh

# Monitor progress
./scripts/wave7/check_wave7_status.sh
```

## Cost-Optimized Polling Protocol

Wave 7 uses **two-phase polling** to reduce API costs by 88%:

### Phase 1: Launch Verification (First 10 Epics)
- **Interval**: 1 minute
- **Purpose**: Verify launch infrastructure works
- **Duration**: ~15 minutes (10 epics × 1.5 min each)

```bash
# Check first 10 epics every 1 minute
watch -n 60 'screen -ls | grep p0- | wc -l'
```

### Phase 2: Cost-Optimized Execution (Remaining 151 Epics)
- **Interval**: 4 minutes
- **Purpose**: Stay within 5-minute cache window (88% cost reduction)
- **Duration**: ~6 hours (151 epics × 2.5 min each)

```bash
# Check remaining epics every 4 minutes
watch -n 240 './scripts/wave7/check_wave7_status.sh 0'
```

## Monitoring Commands

### Active Sessions
```bash
# Count active Phase 0 sessions
screen -ls | grep 'p0-' | wc -l

# List all active sessions
screen -ls | grep 'p0-'

# Attach to specific session
screen -r p0-001
```

### Progress Tracking
```bash
# Overall status (all phases)
./scripts/wave7/check_wave7_status.sh

# Phase 0 specific status
./scripts/wave7/check_wave7_status.sh 0

# Count completed epics
ls docs/brain/EPIC-W7-*/00-hotspots.md | wc -l
```

### Log Analysis
```bash
# View live log
tail -f logs/phase0/EPIC-W7-001.log

# Check for errors
grep -l 'ERROR\|FAILED' logs/phase0/*.log

# Check bobcoin usage
grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase0/EPIC-W7-*.log | tail -20
```

### Lamport Event Tracking
```bash
# Monitor events in real-time
tail -f .lamport/wave7/event_log.jsonl

# Count Phase 0 events
grep '"phase":"0"' .lamport/wave7/event_log.jsonl | wc -l

# View latest events
tail -20 .lamport/wave7/event_log.jsonl | jq -r '"\(.timestamp) | \(.epic_id) | Phase \(.phase) | \(.event_type)"'
```

## Phase Workflow

Each phase follows the same pattern:

1. **Generate Scripts**: `python3 scripts/wave7/generate_phaseX_scripts.py`
2. **Run Pilot**: `./scripts/wave7/launch_phaseX_pilot.sh` (3 epics)
3. **Verify Pilot**: Check logs and output files
4. **Launch Full Wave**: `./scripts/wave7/launch_phaseX_all.sh` (161 epics)
5. **Monitor Progress**: `./scripts/wave7/check_wave7_status.sh X`
6. **Verify Completion**: All 161 epics complete before next phase

## Success Criteria

### Per Phase
- ✅ All 161 epics complete (no active screen sessions)
- ✅ All 161 output files exist
- ✅ No errors in logs
- ✅ Bobcoin usage reasonable (<50 per epic)
- ✅ Lamport events logged for all transitions

### Wave Completion
- ✅ All phases (0 through 6) complete for all 161 epics
- ✅ All methods reduced from CYC > 8 to CYC ≤ 8
- ✅ Build passes on VM
- ✅ UTF-8 encoding verified
- ✅ xUnit tests generated (not NUnit/MSTest)

## Recovery Protocol

If epics fail:

1. **Identify Failures**:
   ```bash
   grep -l 'ERROR\|FAILED' logs/phaseX/*.log
   ```

2. **Analyze Root Cause**:
   ```bash
   tail -100 logs/phaseX/EPIC-W7-XXX.log
   ```

3. **Fix Issues**:
   - UTF-8 encoding violations: Fix source files
   - xUnit test violations: Regenerate tests
   - Build failures: Fix compilation errors

4. **Re-run Failed Epics**:
   ```bash
   ./scripts/wave7/_pX_XXX.sh
   ```

5. **Verify Fix**:
   ```bash
   ./scripts/wave7/check_wave7_status.sh X
   ```

## API Key Rotation

Wave 7 uses 15 API keys in round-robin rotation:

- **Epic 1**: API key 1
- **Epic 2**: API key 2
- ...
- **Epic 15**: API key 15
- **Epic 16**: API key 1 (cycle repeats)

**Distribution**:
- API keys 1-11: 11 epics each
- API keys 12-15: 10 epics each

## File Locations

### Input Files
- `epic_roadmap_wave7.json` - Epic definitions (161 epics)
- `building-blocks/wave7/phase0_template_wave7.sh` - Phase 0 template
- `docs/API/*.json` - API keys (15 files)

### Output Files (Per Epic)
```
docs/brain/EPIC-W7-XXX/
├── 00-hotspots.md              # Phase 0 output
├── 00-scope.md                 # Phase 1 output
├── 01-scope-boundary.md        # Phase 1.5 output
├── 02-architecture-plan.md     # Phase 2 output
├── 03-audit-report.md          # Phase 3 output
├── 04-tickets.md               # Phase 4 output
├── ticket-1-completion.md      # Phase 5 output
├── ticket-1-verification.md    # Phase 5.V output
├── 05-completion-report.md     # Phase 6 output
└── manifest.json               # State tracking
```

### Log Files
```
logs/phase0/EPIC-W7-001.log through EPIC-W7-161.log
logs/phase1/EPIC-W7-001.log through EPIC-W7-161.log
... (similar for all phases)
```

### Lamport Events
```
.lamport/wave7/event_log.jsonl  # All phase transitions
```

## Building-Blocks Method Compliance

All Wave 7 scripts follow the Building-Blocks Method:

✅ **Copied from Wave 4**: All scripts based on proven Wave 4 patterns
✅ **Same Phase Pattern**: Phase 0 copied from Wave 4 Phase 0 (not Phase 1)
✅ **Minimal Changes**: Only epic count (80→161) and format (CCN→W7) changed
✅ **No From-Scratch**: Zero scripts generated without Wave 4 reference
✅ **Template-Based**: All use `building-blocks/wave7/phaseX_template_wave7.sh`

## Troubleshooting

### Issue: Screen sessions not launching
**Solution**: Check Bob CLI path
```bash
which bob  # Should show ~/.npm-global/bin/bob
~/.npm-global/bin/bob --version
```

### Issue: Files not persisting
**Solution**: Verify working directory
```bash
pwd  # Should be /home/malhitticrypto/universal-or-strategy
ls docs/brain/  # Should show EPIC-W7-* directories
```

### Issue: API key exhaustion
**Solution**: Check bobcoin balances
```bash
grep -E 'Balance:' logs/phase0/*.log | tail -15
```

### Issue: UTF-8 encoding violations
**Solution**: Fix source files before continuing
```bash
# Check for non-UTF-8 files
find src/ -name "*.cs" -exec file {} \; | grep -v UTF-8
```

## Next Steps

After Phase 0 completes:

1. **Verify Completion**: `./scripts/wave7/check_wave7_status.sh 0`
2. **Generate Phase 1 Scripts**: `python3 scripts/wave7/generate_phase1_scripts.py`
3. **Run Phase 1 Pilot**: `./scripts/wave7/launch_phase1_pilot.sh`
4. **Launch Phase 1**: `./scripts/wave7/launch_phase1_all.sh`
5. **Repeat for Phases 1.5, 2, 3, 4, 5, 5.V, 6**

## References

- **Wave 4 Scripts**: `scripts/wave4/` (source of Building-Blocks pattern)
- **Execution Plan**: `docs/workflow/WAVE7_EXECUTION_PLAN.md`
- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Cost Protocol**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`
- **Lamport Events**: `.lamport/wave7/README.md`

---

**Made with Bob - Building-Blocks Method (copied from Wave 4)**