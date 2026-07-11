# Wave 7 Phase 2 Execution Plan

**Date**: 2026-06-24
**Phase**: Architecture Planning (Phase 2)
**Status**: Ready for Pilot Launch

## Overview

Wave 7 Phase 2 will execute architecture planning for 161 epics using the Building-Blocks Method.

## Building-Blocks Method Applied

### Source Template
- **Template**: `building-blocks/wave7/phase2_template_wave7.sh`
- **Launch Pattern**: `building-blocks/wave7/launch_wave7_phase1_with_delays.sh`

### Changes Made
1. Phase number: `1` → `2`
2. Output file: `00-scope.md` → `02-architecture-plan.md`
3. Log directory: `phase1` → `phase2`
4. Script prefix: `_p1_` → `_p2_`

### Compliance Verification
✅ Bob CLI pattern: temp file + command substitution
✅ Lamport event tracking
✅ V12.52 triple verification gate
✅ 12-second delays between launches
✅ Jane Street KB query integration

## Generated Artifacts

### Phase 2 Scripts
- **Count**: 161 scripts
- **Pattern**: `_p2_001.sh` through `_p2_161.sh`
- **Location**: Repository root

### Launch Scripts
1. **Pilot**: `launch_wave7_phase2_pilot.sh`
   - Tests 3 epics: 001 (low), 050 (medium), 100 (high complexity)
   - Validates pattern before full wave

2. **Master**: `launch_wave7_phase2_master.sh`
   - Launches all 161 epics
   - 12-second delays between launches
   - Estimated time: 0h 32m

## Execution Protocol

### Step 1: Pilot Launch (NOW)
```bash
./launch_wave7_phase2_pilot.sh
```

**Monitor**:
```bash
watch -n 60 'find docs/brain/EPIC-W7-{001,050,100}/02-architecture-plan.md 2>/dev/null | wc -l'
```

**Success Criteria**:
- All 3 pilots complete Phase 2
- Architecture plans created
- No Lamport event failures

### Step 2: Full Wave Launch (After Pilot Success)
```bash
./launch_wave7_phase2_master.sh
```

**Monitor**:
```bash
watch -n 240 'find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l'
```

## Cost Optimization

### Polling Strategy
- **Interval**: 4 minutes (240 seconds)
- **Reduction**: 88% vs 30-second polling
- **Rationale**: Architecture planning takes ~5-10 minutes per epic

### API Key Rotation
- **Available Keys**: 16 keys in `docs/API/`
- **Rotation**: Automatic via Bob CLI
- **Pattern**: Keys 1-16, then repeat

## Monitoring

### Progress Tracking
```bash
# Current completion count
find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l

# Lamport event log
tail -f .lamport/wave7/event_log.jsonl | grep '"phase":"2"'

# Individual epic logs
tail -f logs/wave7/phase2/EPIC-W7-*.log
```

### Success Metrics
- **Target**: 161/161 epics complete
- **Output**: `docs/brain/EPIC-W7-XXX/02-architecture-plan.md`
- **Lamport Events**: `phase_started` → `phase_completed`

## Recovery Protocol

If pilot fails:
1. Review logs: `logs/wave7/phase2/EPIC-W7-{001,050,100}.log`
2. Check Lamport events for failure reason
3. Fix issue in template
4. Regenerate scripts: `python3 scripts/generate_phase2_scripts.py`
5. Relaunch pilot

If full wave has failures:
1. Identify failed epics via Lamport log
2. Review individual logs
3. Fix and relaunch failed epics only
4. Continue until 161/161 complete

## Next Phase

After Phase 2 completion (161/161):
- **Phase 3**: DNA & PR Audit
- **Command**: Generate Phase 3 scripts using Building-Blocks Method
- **Template**: `building-blocks/wave7/phase3_template_wave7.sh`

## References

- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Architecture**: `building-blocks/autonomous-refactoring/ARCHITECTURE.md`
- **Cost Protocol**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`
- **Jane Street KB**: Query via `python scripts/query_kb.py "<term>"`