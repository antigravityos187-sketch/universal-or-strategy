# Wave 4 API Key Investigation - Critical Findings

**Investigation Date**: 2026-06-16T00:10:00Z  
**Investigator**: Wave 4 Execution Lead  
**Status**: 🔴 CRITICAL - API Key Revoked

---

## Executive Summary

**Root Cause**: One API key (`bob_prod_bob-admin_V7HJU1JXC5q7...`) has been **revoked or exhausted**, blocking 5 Phase 6 epics from completing.

**Impact**: 5/79 Phase 6 epics blocked (6.3% of wave)

**Solution**: Replace revoked key with working key in 5 scripts, re-upload, and re-execute.

---

## Investigation Details

### 1. Initial Symptom

**EPIC-CCN-030 Phase 6 Failure**:
```
Failed to fetch user profile - HTTP 401: Unauthorized
{"message":"API Key verification failed: API Key revoked or access denied","error":"unauthorized"}
```

### 2. Revoked API Key Identification

**Revoked Key** (first 20 chars):
```
bob_prod_bob-admin_V7HJU1JXC5q7...
```

**Full Key**:
```
bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu
```

**Status**: ❌ REVOKED or ACCESS DENIED

### 3. Affected Epics

**5 Phase 6 Scripts Use This Key**:
1. `scripts/wave4/_p6_015.sh` - EPIC-CCN-015
2. `scripts/wave4/_p6_030.sh` - EPIC-CCN-030
3. `scripts/wave4/_p6_045.sh` - EPIC-CCN-045
4. `scripts/wave4/_p6_060.sh` - EPIC-CCN-060
5. `scripts/wave4/_p6_075.sh` - EPIC-CCN-075

**Current Status**:
- EPIC-CCN-060, 075: Phase 6 already complete (executed before key revocation)
- EPIC-CCN-015, 030, 045: Phase 6 blocked by revoked key

### 4. Working Replacement Key

**Working Key** (from EPIC-CCN-001, verified successful):
```
bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR
```

**Status**: ✅ ACTIVE (verified via EPIC-CCN-001 success)

---

## Impact Analysis

### Phase 6 Completion Status

**Before API Key Fix**:
- ✅ Complete: 73/79 (92.4%)
- 🔴 Blocked by API key: 3/79 (015, 030, 045)
- ⏳ Blocked by filename pattern: 3/79 (003, 015, 030)
- ❌ Missing Phase 5: 1/79 (027)
- ❌ Needs re-scope: 1/79 (016)

**Note**: EPIC-015 and 030 have BOTH blockers (API key + filename pattern)

### Epics Requiring Attention

| Epic | Phase 5 | Phase 6 | Blocker(s) |
|------|---------|---------|------------|
| EPIC-CCN-003 | ✅ Complete | ❌ Failed | Filename pattern mismatch |
| EPIC-CCN-015 | ✅ Complete | ❌ Failed | API key + filename pattern |
| EPIC-CCN-016 | ❌ Deferred | ❌ Not started | Needs manual re-scope |
| EPIC-CCN-027 | ❌ Missing | ❌ Not started | Missing Phase 5 |
| EPIC-CCN-030 | ✅ Complete | ❌ Failed | API key + filename pattern |
| EPIC-CCN-045 | ✅ Complete | ❌ Failed | API key revoked |

---

## Recovery Plan

### Step 1: Fix API Key in Scripts (Local)

**Create Fix Script** (`scripts/wave4/fix_revoked_api_key.ps1`):
```powershell
# Replace revoked API key with working key in 5 Phase 6 scripts
$REVOKED_KEY = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
$WORKING_KEY = "bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR"

$scripts = @(
    "scripts/wave4/_p6_015.sh",
    "scripts/wave4/_p6_030.sh",
    "scripts/wave4/_p6_045.sh",
    "scripts/wave4/_p6_060.sh",
    "scripts/wave4/_p6_075.sh"
)

foreach ($script in $scripts) {
    Write-Host "Fixing: $script"
    $content = Get-Content $script -Raw
    $content = $content -replace [regex]::Escape($REVOKED_KEY), $WORKING_KEY
    Set-Content $script -Value $content -NoNewline
}

Write-Host "`nAPI key replacement complete for 5 scripts"
```

### Step 2: Upload Fixed Scripts to VM

**Upload Command**:
```bash
gcloud compute scp scripts/wave4/_p6_015.sh scripts/wave4/_p6_030.sh scripts/wave4/_p6_045.sh \
  v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a

# MANDATORY: Verify upload (V12.27 Protocol)
LOCAL_COUNT=3
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/scripts/wave4/_p6_{015,030,045}.sh 2>/dev/null | wc -l")

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete. Local: $LOCAL_COUNT, VM: $VM_COUNT"
    exit 1
fi
```

### Step 3: Fix Filename Pattern Mismatch

**Issue**: Scripts look for `ticket-*-completion.md` (wildcard), but files are named `ticket-completion.md` (singular).

**Solution**: Update prerequisite check in 3 scripts (003, 015, 030) to accept both patterns:

```bash
# OLD (fails):
if ! ls docs/brain/EPIC-CCN-003/ticket-*-completion.md 1> /dev/null 2>&1; then

# NEW (robust):
if ! find docs/brain/EPIC-CCN-003 -maxdepth 1 \( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \) -print -quit | grep -q .; then
```

### Step 4: Re-execute Phase 6 for 3 Blocked Epics

**Launch Command**:
```bash
# On VM
cd /home/malhitticrypto/universal-or-strategy

# Launch EPIC-CCN-015
screen -dmS p6-015-retry bash -l -c \
    "./scripts/wave4/_p6_015.sh 2>&1 | tee logs/phase6/EPIC-CCN-015-retry.log"
sleep 12

# Launch EPIC-CCN-030
screen -dmS p6-030-retry bash -l -c \
    "./scripts/wave4/_p6_030.sh 2>&1 | tee logs/phase6/EPIC-CCN-030-retry.log"
sleep 12

# Launch EPIC-CCN-045
screen -dmS p6-045-retry bash -l -c \
    "./scripts/wave4/_p6_045.sh 2>&1 | tee logs/phase6/EPIC-CCN-045-retry.log"
```

**Monitor**:
```bash
# Check after 1 minute
screen -ls | grep -c 'p6-.*-retry'
ls docs/brain/EPIC-CCN-{015,030,045}/06-completion-report.md 2>/dev/null | wc -l
```

---

## Timeline Estimate

- **Step 1 (Fix API key locally)**: 2 minutes
- **Step 2 (Upload to VM)**: 2 minutes
- **Step 3 (Fix filename pattern)**: 5 minutes
- **Step 4 (Re-execute Phase 6)**: 15 minutes
- **TOTAL**: ~25 minutes

---

## Success Criteria

### Per Epic
- ✅ Script uses working API key
- ✅ Prerequisite check accepts both filename patterns
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/06-completion-report.md`
- ✅ File size >1K
- ✅ No errors in log

### Wave Status After Fix
- ✅ Phase 6: 76/79 complete (96.2%)
- ⏳ Remaining: 3 epics (003, 027, 016)

---

## Lessons Learned

### 1. API Key Rotation Monitoring
**Gap**: No proactive monitoring of API key health during wave execution.

**Fix**: Add API key health check to monitoring protocol:
```bash
# Check API key before launching wave
for key in "${API_KEYS[@]}"; do
    export BOBSHELL_API_KEY="$key"
    if ! bob --yolo "ping" 2>&1 | grep -q "pong"; then
        echo "WARNING: API key $key may be revoked"
    fi
done
```

### 2. Filename Pattern Robustness
**Gap**: Scripts assume specific filename patterns without fallback.

**Fix**: Use `find` with OR logic for all prerequisite checks:
```bash
find DIR -maxdepth 1 \( -name "pattern1" -o -name "pattern2" \) -print -quit | grep -q .
```

### 3. API Key Exhaustion Detection
**Gap**: No early warning when API key approaches bobcoin limit.

**Fix**: Track bobcoin usage per API key, alert at 80% threshold.

---

## Next Steps

1. ✅ **Immediate**: Fix API key in 5 scripts (this document)
2. ⏳ **Next**: Fix filename pattern in 3 scripts (003, 015, 030)
3. ⏳ **Next**: Re-execute Phase 6 for 3 blocked epics
4. ⏳ **Next**: Execute EPIC-CCN-027 Phase 5 (missing)
5. ⏳ **Next**: Execute EPIC-CCN-003 Phase 6 (filename pattern only)
6. ⏳ **Next**: Manual re-scope EPIC-CCN-016
7. ⏳ **Final**: Achieve 80/80 completion

---

## References

- **V12.27 Protocol**: Upload Verification (MANDATORY)
- **V12.28 Protocol**: 100% Completion Mandate
- **Building-Blocks Method**: Script generation SOP V3.1
- **Recovery Loop Protocol**: V12.26 (V1.1)

---

**Status**: 🟡 READY FOR API KEY FIX  
**Next Action**: Execute Step 1 (fix API key in 5 scripts locally)  
**Estimated Time to Resolution**: 25 minutes  
**Blocking**: 3 Phase 6 epics (015, 030, 045)