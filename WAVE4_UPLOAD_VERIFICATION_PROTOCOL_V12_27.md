# Wave 4 Upload Verification Protocol V12.27

**Date**: 2026-06-15  
**Status**: MANDATORY for all future waves  
**Version**: V12.27

---

## Executive Summary

After Wave 4 Phase 5 root cause analysis revealed 7 scripts never uploaded to VM (causing 7 epic failures), implemented MANDATORY upload verification protocol across all workflow documents.

**Impact**: Prevents silent upload failures that cost 1-2 hours recovery time per incident.

---

## Root Cause

**Wave 4 Phase 5**: 7 scripts existed locally but were never uploaded to VM.

**Evidence**:
- ✅ Local scripts exist: `scripts/wave4/_p5_{003,015,030,031,033,042,055}.sh`
- ❌ VM scripts missing: No files found on VM
- ❌ No logs: Epics never launched (scripts weren't there to run)

**Why This Happened**: Upload command likely had a glob pattern issue that silently skipped 7 files. No verification step caught the gap.

---

## Protocol V12.27: MANDATORY Upload Verification

### Step 1: Upload Scripts
```bash
gcloud compute scp scripts/wave{N}/_p{X}_*.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave{N}/ --zone=us-central1-a
```

### Step 2: VERIFY Upload (CRITICAL)
```bash
# Count local scripts
LOCAL_COUNT=$(ls scripts/wave{N}/_p{X}_*.sh | wc -l)

# Count VM scripts
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh | wc -l")

# Compare and fail if mismatch
if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete. Local: $LOCAL_COUNT, VM: $VM_COUNT"
    exit 1
fi

echo "✅ Upload verified: $LOCAL_COUNT scripts"
```

### Step 3: Set Permissions
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="chmod +x ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh"
```

### Step 4: Proceed with Pilot Test
Only after verification passes.

---

## Files Updated

### 1. Script Generation SOP (V3.1)
**File**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

**Changes**:
- Added Step 5: MANDATORY Upload Verification
- Script count comparison protocol
- Updated verification checklist with upload verification requirement
- Version bumped: 3.0 → 3.1

**Key Addition**:
```markdown
### Step 5: MANDATORY Upload Verification

**CRITICAL**: Always verify ALL scripts uploaded successfully before proceeding.

**Why This Matters**:
- Wave 4 Phase 5: 7 scripts never uploaded → 7 epics failed
- Silent failure: No error message, scripts just missing
- Cost: 1-2 hours recovery time + debugging effort

**DO NOT PROCEED** until counts match exactly.
```

### 2. GCP VM Wave Execution Skill (V2.4)
**File**: `.bob/skills/gcp-vm-wave-execution/skill.md`

**Changes**:
- Added upload verification to Phase 0 launch example
- Verification commands with error handling
- Updated post-use audit with root cause reference
- Version bumped: 2.3 → 2.4

**Key Addition**:
```bash
# 3. MANDATORY: Verify Upload (CRITICAL - prevents silent failures)
LOCAL_COUNT=$(ls scripts/wave2/_p0_*.sh | wc -l)
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls ~/universal-or-strategy/scripts/wave2/_p0_*.sh | wc -l")

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete. Local: $LOCAL_COUNT, VM: $VM_COUNT"
    exit 1
fi
echo "✅ Upload verified: $LOCAL_COUNT scripts"
```

### 3. Autonomous Refactor Custom Mode (V12.27)
**File**: `.bob/custom_modes.yaml`

**Changes**:
- Added Protocol 2: UPLOAD VERIFICATION (CRITICAL)
- Updated Common Pitfalls (#2: Skipping upload verification)
- Added uploadVerification custom rule
- Updated pilotTesting to require upload verification first

**Key Addition**:
```yaml
2. UPLOAD VERIFICATION (V12.27 - CRITICAL): After uploading scripts to VM, ALWAYS verify
   script count matches. Compare local count vs VM count. If mismatch, STOP and investigate.
   Wave 4 Phase 5: 7 scripts never uploaded → 7 epics failed silently.
```

---

## Enforcement

### Pre-Launch Checklist

Before launching any wave phase:
- [ ] Scripts generated using building-blocks method
- [ ] Scripts uploaded to VM
- [ ] **UPLOAD VERIFIED** (counts match)
- [ ] Permissions set
- [ ] Pilot test run
- [ ] Pilot test validated
- [ ] Full wave launched

### Violation Consequences

**If upload verification skipped**:
- Silent failures (scripts missing on VM)
- Epics fail with no error message
- 1-2 hours recovery time per incident
- Debugging effort to identify root cause
- Wave completion delayed

---

## Recovery Procedure

**If upload verification fails**:

1. **Identify missing scripts**:
   ```bash
   # List local scripts
   ls scripts/wave{N}/_p{X}_*.sh
   
   # List VM scripts
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="ls ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh"
   
   # Compare outputs to find missing files
   ```

2. **Upload missing scripts**:
   ```bash
   # Upload specific missing scripts
   for epic in 003 015 030; do
       gcloud compute scp scripts/wave{N}/_p{X}_${epic}.sh \
         v12-test-golden-v2:~/universal-or-strategy/scripts/wave{N}/ \
         --zone=us-central1-a
   done
   ```

3. **Re-verify**:
   ```bash
   # Verify counts match now
   LOCAL_COUNT=$(ls scripts/wave{N}/_p{X}_*.sh | wc -l)
   VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="ls ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh | wc -l")
   
   if [ "$LOCAL_COUNT" == "$VM_COUNT" ]; then
       echo "✅ Upload complete: $LOCAL_COUNT scripts"
   fi
   ```

4. **Proceed with launch**

---

## Success Metrics

### Per Wave
- ✅ Upload verification performed before every phase launch
- ✅ Zero silent upload failures
- ✅ 100% script upload success rate

### Per Phase
- ✅ Local count = VM count (verified)
- ✅ All scripts executable on VM
- ✅ Pilot test succeeds before full launch

---

## Related Documents

- **Root Cause Analysis**: `WAVE4_ROOT_CAUSE_ANALYSIS.md`
- **Recovery Plan**: `WAVE4_COMPLETE_RECOVERY_AND_HARDENING_PLAN.md`
- **Script Generation SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Wave Execution Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md`
- **Custom Mode**: `.bob/custom_modes.yaml` (autonomous-refactor)

---

## Version History

### V12.27 (2026-06-15)
- **Added**: MANDATORY upload verification protocol
- **Updated**: 3 workflow documents (SOP, skill, custom mode)
- **Reason**: Wave 4 Phase 5 failures (7 scripts never uploaded)

---

**Status**: 🟢 ACTIVE  
**Enforcement**: MANDATORY for all future waves  
**Maintainer**: Wave Execution Lead  
**Last Updated**: 2026-06-15T22:37:00Z