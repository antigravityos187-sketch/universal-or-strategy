# Wave 1 Phase 1 Deployment Guide

**Date**: 2026-06-14
**Status**: Ready for Deployment
**Epics**: EPIC-001 to EPIC-005 (5 epics)
**VM**: 1 × n2-standard-4
**API**: Single shared key (user will track manually)

---

## Scripts Created ✅

All Phase 0 scripts have been created using the Building Blocks method (copied from Wave 2 template):

1. `scripts/wave1/_p0_001.sh` - EPIC-001 (PropagateMaster methods)
2. `scripts/wave1/_p0_002.sh` - EPIC-002 (HandleFlatPosition methods)
3. `scripts/wave1/_p0_003.sh` - EPIC-003 (SyncLimitTarget methods)
4. `scripts/wave1/_p0_004.sh` - EPIC-004 (ProcessSingleFleetRMAAccount methods)
5. `scripts/wave1/_p0_005.sh` - EPIC-005 (EmergencyFlattenSingleFleetAccount methods)
6. `scripts/wave1/launch_phase0_all.sh` - Launcher script

**API Key**: All 5 scripts use the same API key from Wave 2 template
- Key: `bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
- **User will track usage manually**

---

## Deployment Steps

### Step 1: Deploy VM

```bash
gcloud compute instances create v12-wave1-test \
  --zone=us-central1-a \
  --machine-type=n2-standard-4 \
  --source-instance-template=v12-bob-shell-golden-v2 \
  --boot-disk-size=50GB \
  --boot-disk-type=pd-standard
```

**Verify**:
```bash
gcloud compute instances list --filter="name=v12-wave1-test"
```

Expected: VM status = RUNNING

---

### Step 2: Upload Scripts to VM

```bash
# Upload Phase 0 scripts
gcloud compute scp scripts/wave1/_p0_*.sh v12-wave1-test:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Upload launcher
gcloud compute scp scripts/wave1/launch_phase0_all.sh v12-wave1-test:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Make executable
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p0_*.sh"
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/launch_phase0_all.sh"
```

---

### Step 3: Launch Phase 0

```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="/home/malhitticrypto/universal-or-strategy/launch_phase0_all.sh"
```

---

### Step 4: Monitor Execution

**Check screen sessions** (should see 5 running):
```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="screen -ls"
```

**Check file creation** (expect 5 files):
```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-*/00-hotspots.md 2>/dev/null | wc -l"
```

**Extract bobcoin usage**:
```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="grep -E 'Cost:.*Balance:' /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-*.log"
```

**View specific log**:
```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="tail -100 /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-001.log"
```

---

### Step 5: Validate Success

**Success Criteria**:
- ✅ All 5 screen sessions complete (screen -ls shows "No Sockets found")
- ✅ 5 files exist: `docs/brain/EPIC-*/00-hotspots.md`
- ✅ 5 files exist: `docs/brain/EPIC-*/manifest.json`
- ✅ Bobcoin usage reported in logs
- ✅ API key remains positive (>10 bobcoins)

**If successful**: Proceed to Phase 1 (Scope + Boundary)
**If failed**: Debug logs, fix issues, relaunch

---

## Epic Details

### EPIC-001
- **File**: `src/V12_002.Orders.Callbacks.Propagation.cs`
- **Methods**: 
  - PropagateMaster_IdentifyMove (CYC 18)
  - PropagateMaster_HandleRejection (CYC 11)
  - PropagateMaster_SyncTargets (CYC 10)
- **Target**: All ≤8

### EPIC-002
- **File**: `src/V12_002.Orders.Callbacks.Execution.cs`
- **Methods**:
  - HandleFlatPosition_CleanupActivePositions (CYC 17)
  - HandleFlatPosition_UpdateState (CYC 10)
- **Target**: All ≤8

### EPIC-003
- **File**: `src/V12_002.Orders.Management.StopSync.cs`
- **Methods**:
  - SyncLimitTarget (CYC 17)
  - SyncStopTarget (CYC 9)
- **Target**: All ≤8

### EPIC-004
- **File**: `src/V12_002.SIMA.Execution.cs`
- **Methods**:
  - ProcessSingleFleetRMAAccount (CYC 16)
  - ExecuteFleetOrder (CYC 10)
  - ValidateFleetState (CYC 10)
- **Target**: All ≤8

### EPIC-005
- **File**: `src/V12_002.SIMA.Flatten.cs`
- **Methods**:
  - EmergencyFlattenSingleFleetAccount (CYC 16)
  - FlattenFleetPosition (CYC 9)
- **Target**: All ≤8

---

## Budget Tracking

**API Key**: Single shared key (Wave 2 template key)
**Initial Balance**: ~132 bobcoins (estimated from Wave 2 usage)
**Expected Usage**: 5 epics × ~10 bobcoins/epic = ~50 bobcoins
**Expected Remaining**: ~82 bobcoins

**User Action**: Track actual usage manually from logs

---

## Timeline

**Phase 0 Execution**: ~2 hours (5 epics × 20 min each, parallel)
**Validation**: 15 minutes
**Total**: ~2.25 hours

---

## Next Steps After Phase 0

1. **Validate Success**: Check all 5 epics completed
2. **Extract Bobcoin Usage**: Update tracking document
3. **Create Phase 1 Scripts**: Copy Phase 0 pattern for Scope + Boundary
4. **Launch Phase 1**: Repeat deployment process
5. **Continue Through Phase 6**: Complete all 10 phases

---

## Emergency Procedures

**Stop All Agents**:
```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="killall screen"
```

**Relaunch Single Epic** (example: EPIC-001):
```bash
gcloud compute ssh v12-wave1-test --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && screen -dmS p0-001 bash -l -c './_p0_001.sh 2>&1 | tee logs/phase0/EPIC-001.log'"
```

**Check API Balance**: Login to IBM Bob Shell dashboard

---

## Files Created

- `scripts/wave1/_p0_001.sh` (139 lines)
- `scripts/wave1/_p0_002.sh` (139 lines)
- `scripts/wave1/_p0_003.sh` (139 lines)
- `scripts/wave1/_p0_004.sh` (139 lines)
- `scripts/wave1/_p0_005.sh` (139 lines)
- `scripts/wave1/launch_phase0_all.sh` (38 lines)
- `WAVE1_PHASE1_DEPLOYMENT_GUIDE.md` (this file)

---

**Status**: ✅ Ready for Deployment
**Next Action**: Deploy VM and execute Step 1-5 above
**Session Cost**: $122.67
**Key Achievement**: Complete Phase 0 scripts using Building Blocks method