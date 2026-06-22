# Wave 7 Task 7 - Master Launch Script (COMPLETE)

**Status**: ✅ COMPLETE  
**Date**: 2026-06-21  
**Task**: Create master launch script for 161 epic autonomous execution

---

## Deliverables Created

### 1. Master Launch Script
**File**: `scripts/launch_wave7.sh` (545 lines)

**Features**:
- ✅ Sequential epic launch (161 epics)
- ✅ Two-phase polling (1-min → 4-min after epic 10)
- ✅ Lamport event monitoring
- ✅ Progress reporting (X/161 complete)
- ✅ Recovery loop for failed epics
- ✅ Cost tracking (per API)
- ✅ Error detection (UTF-8, xUnit, build)
- ✅ Graceful shutdown (Ctrl+C)

### 2. Status Check Script
**File**: `scripts/check_wave7_status.sh` (310 lines)

**Features**:
- ✅ Progress dashboard with progress bar
- ✅ VM status (sessions, disk usage)
- ✅ Phase distribution
- ✅ Error analysis from Lamport log
- ✅ Performance metrics (avg durations)
- ✅ Recent activity (last 10 events)
- ✅ Recommendations

### 3. Complete Documentation
**File**: `scripts/WAVE7_LAUNCH_README.md` (502 lines)

**Sections**:
- Overview & Scripts
- Prerequisites
- Execution Workflow
- Monitoring & Observability
- Cost Tracking (~$990 estimated)
- Recovery Protocol
- Success Criteria
- Troubleshooting

### 4. Quick Reference Card
**File**: `scripts/WAVE7_QUICK_REFERENCE.md` (139 lines)

**Quick Commands**:
- Launch, monitoring, debugging
- Recovery, post-wave validation
- Progress tracking
- Emergency procedures

---

## Key Features

### Two-Phase Polling Strategy

**Phase 1: Launch Verification (First 10 Epics)**
- Polling: 1-minute intervals
- Purpose: Catch immediate failures
- Duration: ~20 minutes
- Success: All 10 epics progress past Phase 0

**Phase 2: Full Wave (Remaining 151 Epics)**
- Polling: 4-minute intervals (cost-optimized)
- Purpose: Monitor 161 epics to completion
- Duration: ~8-12 hours
- Success: 161/161 complete

**Cost Optimization**: 88% reduction vs 30-second polling

### Lamport Event Integration

**Events Logged**:
- `wave_start`, `epic_start`, `phase_start`
- `phase_complete`, `phase_fail`
- `epic_complete`, `wave_complete`

**Location**: `.lamport/wave7/event_log.jsonl`

### Recovery Loop

**Automatic**:
1. Detect failed epics via manifest
2. Create failure-analysis.md template
3. Log warning and pause wave
4. Wait for manual intervention

**Manual**:
1. Analyze failure (UTF-8, xUnit, build, scope)
2. Fix issue
3. Re-run epic
4. Verify completion

---

## Usage

### Quick Start

```bash
# Launch Wave 7
bash scripts/launch_wave7.sh

# Monitor progress
bash scripts/check_wave7_status.sh

# Watch continuously
watch -n 240 bash scripts/check_wave7_status.sh
```

### Pre-Flight Checks

```bash
# VM connectivity
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="echo 'test'"

# Verify files
ls -la epic_roadmap_wave7.json
ls -la building-blocks/wave7/phase*_template_wave7.sh
ls -la .lamport/wave7/
```

### Post-Wave Validation

```bash
# Sync and validate
powershell -File .\deploy-sync.ps1
powershell -File .\scripts\pre_push_validation.ps1
python scripts/complexity_audit.py

# Test NinjaTrader (F5)
```

---

## Expected Timeline

- **Launch Verification**: ~20 minutes (first 10 epics)
- **Full Wave Execution**: ~8-12 hours (remaining 151 epics)
- **Post-Wave Validation**: ~35 minutes
- **Total**: ~9-13 hours (end-to-end)

---

## Cost Estimate

| Phase | Duration | Cost |
|-------|----------|------|
| 0 | 30 min | ~$50 |
| 1 | 45 min | ~$75 |
| 1.5 | 30 min | ~$40 |
| 2 | 75 min | ~$125 |
| 3 | 30 min | ~$60 |
| 4 | 30 min | ~$40 |
| 5 | 180 min | ~$400 |
| 5.V | 90 min | ~$120 |
| 6 | 30 min | ~$80 |
| **Total** | | **~$990** |

---

## Success Criteria

### Wave Completion
- ✅ 161/161 epics complete (100%)
- ✅ All phases pass
- ✅ Build passes (VM + local)
- ✅ F5 in NinjaTrader succeeds
- ✅ All methods CYC ≤8
- ✅ Zero UTF-8 violations
- ✅ Zero xUnit violations

### Quality Gates
- ✅ Pre-push validation (13/13)
- ✅ CodeScene score ≤8
- ✅ Codacy grade maintained
- ✅ Zero compilation errors
- ✅ Zero test failures
- ✅ Zero scope creep

---

## Next Steps

1. ✅ **Task 7 Complete** - Scripts created
2. ⏭️ **User Review** - Review documentation
3. ⏭️ **Pre-Flight** - Verify prerequisites
4. ⏭️ **Launch** - `bash scripts/launch_wave7.sh`
5. ⏭️ **Monitor** - `bash scripts/check_wave7_status.sh`
6. ⏭️ **Validate** - Post-wave checks
7. ⏭️ **PR** - Submit Wave 7

---

## References

- **Scripts**: `scripts/launch_wave7.sh`, `scripts/check_wave7_status.sh`
- **Docs**: `scripts/WAVE7_LAUNCH_README.md`, `scripts/WAVE7_QUICK_REFERENCE.md`
- **Plan**: `docs/workflow/WAVE7_EXECUTION_PLAN.md`
- **Protocol**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`
- **Roadmap**: `epic_roadmap_wave7.json`

---

**Status**: ✅ READY FOR EXECUTION  
**Version**: 1.0  
**Last Updated**: 2026-06-21