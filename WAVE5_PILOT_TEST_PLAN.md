# Wave 5 Pilot Test Plan - EPIC-CCN-001

**Date**: 2026-06-16  
**Status**: Ready for Execution  
**Epic**: EPIC-CCN-001 (Pilot Test)  
**Protocols**: V12.34 (SURGICAL ONLY), V12.36 (VM-Local Git Sync)

---

## Executive Summary

This pilot test validates Wave 5 hardened protocols before launching full wave execution on 77 epics. EPIC-CCN-001 serves as the canary to verify:
- Phase 5 SURGICAL ONLY mandate
- Phase 5.V 5-check verification protocol
- Building-blocks script generation method
- VM-Local git sync protocol
- UTF-8 encoding compliance

---

## Pre-Flight Checklist ✅

### Completed
- [x] All 7 protocol files exist and complete
- [x] Phase 5 MCP server has SURGICAL ONLY mandate
- [x] Phase 5.V MCP server has 5-check protocol
- [x] Pre-flight validation tested (9 skip, 6 local, 158 normal)
- [x] Phase 5 script created (`scripts/wave5/_p5_001.sh`)
- [x] Phase 5.V script created (`scripts/wave5/_p5v_001.sh`)

### Pending (Execute Before Pilot)
- [ ] UTF-8 encoding pre-check (`.\scripts\check_encoding.ps1`)
- [ ] VM-Local git sync (5-step verification)
- [ ] VM build passes (`dotnet build`)
- [ ] Local build passes (`dotnet build`)

---

## Pilot Test Execution Steps

### Step 1: Pre-Execution Validation

**1.1 Encoding Check (MANDATORY)**
```powershell
# Local execution
.\scripts\check_encoding.ps1

# Expected: Exit code 0, all files UTF-8 without BOM
# If violations: .\scripts\check_encoding.ps1 -Fix
```

**1.2 VM-Local Git Sync (MANDATORY - V12.36)**
```bash
# Check local state
git log -1 --oneline
git status

# Push to origin
git push origin gitbutler/workspace --force --no-verify

# Check VM state (before sync)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --oneline"

# Sync VM to match local
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git fetch origin && git reset --hard origin/gitbutler/workspace && git clean -fd"

# Verify sync (BLOCKING GATE)
LOCAL_COMMIT=$(git log -1 --format="%H")
VM_COMMIT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --format='%H'")

echo "Local: $LOCAL_COMMIT"
echo "VM:    $VM_COMMIT"

if [ "$LOCAL_COMMIT" = "$VM_COMMIT" ]; then
  echo "✅ SYNC VERIFIED"
else
  echo "❌ SYNC FAILED - DO NOT PROCEED"
  exit 1
fi
```

**1.3 Build Verification**
```bash
# Local build
dotnet build

# VM build
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && dotnet build"

# Both must pass (exit code 0)
```

---

### Step 2: Upload Scripts to VM

**2.1 Upload Phase 5 Script**
```powershell
gcloud compute scp scripts/wave5/_p5_001.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave5/ --zone=us-central1-a
```

**2.2 Upload Phase 5.V Script**
```powershell
gcloud compute scp scripts/wave5/_p5v_001.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave5/ --zone=us-central1-a
```

**2.3 Fix Line Endings (MANDATORY)**
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy/scripts/wave5 && sed -i 's/\r$//' _p5_001.sh _p5v_001.sh"
```

**2.4 Set Permissions**
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy/scripts/wave5 && chmod +x _p5_001.sh _p5v_001.sh"
```

**2.5 Verify Upload (MANDATORY - V3.1)**
```bash
# Count local scripts
LOCAL_COUNT=$(ls scripts/wave5/_p5*.sh | wc -l)

# Count VM scripts
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/scripts/wave5/_p5*.sh | wc -l")

echo "Local: $LOCAL_COUNT scripts"
echo "VM:    $VM_COUNT scripts"

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
  echo "❌ UPLOAD INCOMPLETE"
  exit 1
fi
```

---

### Step 3: Execute Phase 5 (Ticket Execution)

**3.1 Launch Phase 5 in Foreground**
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && bash -l -c './scripts/wave5/_p5_001.sh' 2>&1 | tee logs/pilot_phase5.log"
```

**3.2 Monitor Execution**
- Watch for "SUCCESS: Phase 5 complete for EPIC-CCN-001"
- Check for ticket completion files created
- Extract bobcoin usage from output

**3.3 Verify Phase 5 Success**
```bash
# Check ticket completion files exist
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/docs/brain/EPIC-CCN-001/ticket-*-completion.md"

# Expected: At least one ticket-X-completion.md file
```

---

### Step 4: Execute Phase 5.V (Verification)

**4.1 Launch Phase 5.V in Foreground**
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && bash -l -c './scripts/wave5/_p5v_001.sh' 2>&1 | tee logs/pilot_phase5v.log"
```

**4.2 Monitor 5-Check Execution**
Watch for:
- ✅ Check 1: Compilation PASS
- ✅ Check 2: Complexity ≤8 PASS
- ✅ Check 3: Scope compliance PASS
- ✅ Check 4: xUnit tests PASS
- ✅ Check 5: UTF-8 encoding PASS

**4.3 Verify Phase 5.V Success**
```bash
# Check verification report exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cat ~/universal-or-strategy/docs/brain/EPIC-CCN-001/06-verification-report.md"

# Expected: Report with ALL 5 CHECKS PASS
```

---

### Step 5: Post-Execution Analysis

**5.1 Download Artifacts**
```powershell
# Download logs
gcloud compute scp v12-test-golden-v2:~/universal-or-strategy/logs/pilot_phase5.log ./logs/ --zone=us-central1-a
gcloud compute scp v12-test-golden-v2:~/universal-or-strategy/logs/pilot_phase5v.log ./logs/ --zone=us-central1-a

# Download brain files
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-001/ ./docs/brain/ --zone=us-central1-a
```

**5.2 Extract Bobcoin Usage**
```bash
# From Phase 5 log
grep -E "Cost:.*Balance:" logs/pilot_phase5.log

# From Phase 5.V log
grep -E "Cost:.*Balance:" logs/pilot_phase5v.log

# Calculate total cost
```

**5.3 Verify Scope Compliance**
```bash
# Check git diff (should show ONLY target method)
git diff HEAD~1 --stat

# Expected: Only target file modified, no adjacent changes
```

**5.4 Run Greptile Audit (Optional)**
```bash
# Check for P0/P1 issues
# Expected: 0 issues (vs Wave 4: 28 issues)
```

---

## Success Criteria

### Phase 5 Success
- ✅ Ticket completion files created
- ✅ Build passes
- ✅ No compilation errors
- ✅ Bobcoin usage tracked
- ✅ SURGICAL ONLY discipline maintained

### Phase 5.V Success
- ✅ Check 1: Compilation PASS
- ✅ Check 2: Complexity ≤8 PASS
- ✅ Check 3: Scope compliance PASS (no adjacent changes)
- ✅ Check 4: xUnit tests PASS
- ✅ Check 5: UTF-8 encoding PASS
- ✅ Verification report generated

### Overall Pilot Success
- ✅ 0 P0/P1 Greptile issues (vs Wave 4: 9 P0)
- ✅ Total cost ≤ $0.10 (2 phases × $0.05)
- ✅ Execution time ≤ 30 minutes
- ✅ No protocol violations
- ✅ No rollback needed

---

## Failure Scenarios & Recovery

### Scenario 1: Phase 5 Fails (Compilation Error)
**Recovery**:
1. Review Phase 5 log for error details
2. Check if pre-existing errors (STOP if yes)
3. If Bob error: Analyze root cause
4. Update Phase 5 script if needed
5. Re-execute Phase 5

### Scenario 2: Phase 5.V Check Fails
**Recovery**:
1. Identify which check failed (1-5)
2. Review failure details in log
3. Apply targeted fix:
   - Check 1: Fix compilation
   - Check 2: Re-extract if CYC >8
   - Check 3: Revert scope violations
   - Check 4: Fix or regenerate tests
   - Check 5: Run `check_encoding.ps1 -Fix`
4. Re-execute Phase 5.V

### Scenario 3: Scope Violation Detected
**Recovery**:
1. Run `git diff HEAD~1` to see violations
2. Document violated methods
3. Revert changes: `git reset --hard HEAD~1`
4. Update Phase 5 script with stronger SURGICAL ONLY prompt
5. Re-execute Phase 5 + 5.V

### Scenario 4: UTF-16 Encoding Corruption
**Recovery**:
1. Run `.\scripts\check_encoding.ps1`
2. Auto-fix: `.\scripts\check_encoding.ps1 -Fix`
3. Verify fix: `.\scripts\check_encoding.ps1`
4. Commit encoding fix
5. Re-execute Phase 5.V

---

## Post-Pilot Decision Matrix

### If Pilot PASSES (All Criteria Met)
**Action**: Proceed with Wave 5 full launch
- Generate scripts for remaining 77 epics
- Use same building-blocks method
- Launch with staggered delay (9s)
- Monitor with cost-optimized polling (4 min)
- Estimated timeline: ~28 hours
- Estimated cost: $4.00

### If Pilot FAILS (Any Criterion Unmet)
**Action**: Iterate on protocols
1. Document failure root cause
2. Update relevant protocol file
3. Update MCP server if needed
4. Re-test pilot until success
5. **DO NOT** launch full wave until pilot passes

---

## Next Steps After Pilot Success

1. **Generate Wave 5 Scripts** (77 epics)
   - Use building-blocks method
   - Copy `_p5_001.sh` → `_p5_002.sh` ... `_p5_080.sh`
   - Copy `_p5v_001.sh` → `_p5v_002.sh` ... `_p5v_080.sh`
   - Update epic IDs and API keys only

2. **Upload to VM**
   - Upload all 154 scripts (77 × 2 phases)
   - Fix line endings
   - Set permissions
   - Verify upload count

3. **Launch Wave 5**
   - Staggered launch (9s delay)
   - Cost-optimized polling (4 min)
   - Monitor bobcoin usage
   - Track completion rate

4. **Monitor & Report**
   - Check every 4 minutes
   - Extract bobcoin usage
   - Document any failures
   - Apply recovery loop if needed

---

## References

- **Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md` (V2.7)
- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Phase 5 Protocol**: `docs/protocol/PHASE5_EXECUTION_PROTOCOL.md` (V12.34)
- **Phase 5.V Protocol**: `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md` (V12.34)
- **Git Sync Protocol**: `docs/protocol/VM_LOCAL_GIT_SYNC_PROTOCOL.md` (V12.36)
- **Encoding Protocol**: `docs/protocol/FILE_ENCODING_PROTOCOL.md` (V12.33)
- **Recovery Protocol**: `docs/protocol/RECOVERY_LOOP_PROTOCOL.md` (V12.26)

---

## Execution Log

### Pre-Execution (2026-06-16)
- [x] Protocol files verified (all 7 exist)
- [x] MCP servers verified (hardened)
- [x] Pre-flight validation tested
- [x] Phase 5 script created
- [x] Phase 5.V script created
- [ ] Encoding check (pending)
- [ ] VM-Local git sync (pending)
- [ ] Build verification (pending)

### Pilot Execution (Pending)
- [ ] Scripts uploaded to VM
- [ ] Phase 5 executed
- [ ] Phase 5.V executed
- [ ] Artifacts downloaded
- [ ] Success criteria verified

### Post-Pilot (Pending)
- [ ] Decision: PASS or ITERATE
- [ ] If PASS: Generate Wave 5 scripts
- [ ] If FAIL: Update protocols and retry

---

**Status**: Ready for execution. Awaiting Director approval to proceed with Step 1 (Pre-Execution Validation).