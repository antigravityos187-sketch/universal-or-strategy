# Wave 3 Merge Analysis

**Date**: 2026-06-14
**Question**: Do we need to merge before Wave 3 launch for Firebase fixes to take effect?
**Answer**: **NO - Merge NOT required**

## Current Git Status

**Branch**: `gitbutler/workspace`
**Status**: Up to date with `origin/gitbutler/workspace`
**Last Commit**: `e9140153` - "docs: Firebase key exposure incident documentation and fix"

## Firebase Files Analysis

### Files Already Committed (e9140153)

✅ **Committed to gitbutler/workspace**:
1. `docs/security/FIREBASE_KEY_EXPOSURE_INCIDENT.md`
2. `docs/security/FIREBASE_KEY_UPDATE_INSTRUCTIONS.md`
3. `docs/security/FIREBASE_FIX_COMPLETE.md`
4. `.gitignore` (fixed - properly excludes firebase credentials)
5. `firebase-key.json.template` (documentation only)

### Files NOT in Git (Gitignored - Correct)

❌ **Gitignored** (as intended):
1. `firebase-credentials.json` (active key - MUST stay gitignored)
2. `firebase-credentials.json.revoked` (backup - untracked)

### Files Pending Commit (Documentation)

⏳ **Untracked** (Wave 3 planning docs):
1. `FIREBASE_INTEGRATION_CORRECTION.md`
2. `WAVE3_API_ROTATION_STRATEGY.md`
3. `WAVE3_FINAL_EXECUTION_PLAN.md`
4. `docs/security/VM_FIREBASE_SETUP_REQUIRED.md`

## Critical Question: Does VM Need Merge?

### Answer: NO

**Reason**: Firebase credentials are **NOT in git** (gitignored)

**VM Setup Process**:
1. Install `firebase-admin` package (via pip)
2. Copy `firebase-credentials.json` from local to VM (via gcloud scp)
3. Test connectivity

**None of these steps require git merge**:
- Package installation: `pip3 install firebase-admin` (no git dependency)
- Credentials: Copied directly via `gcloud scp` (bypasses git)
- Scripts: Already on VM from previous waves

## What Files Does VM Need?

### Already on VM (from previous waves)

✅ **Scripts**:
- `scripts/query_kb.py` (Line 9: `CREDENTIALS_PATH = 'firebase-credentials.json'`)
- `scripts/phase_4_5_ticket_review_mcp.py` (Line 19: `CREDENTIALS_PATH = 'firebase-credentials.json'`)

✅ **Custom Modes**:
- `.bob/custom_modes.yaml` (defines `v12-epic-planner`, `v12-engineer`)
- `.bob/rules-v12-epic-planner/01-planning-protocol.md`
- `.bob/rules-v12-engineer/dna.md`

### Missing on VM (need to add)

❌ **Firebase Package**: `firebase-admin` (install via pip)
❌ **Firebase Credentials**: `firebase-credentials.json` (copy via gcloud scp)

### NOT Needed on VM

✅ **Documentation** (local only):
- `FIREBASE_INTEGRATION_CORRECTION.md`
- `WAVE3_API_ROTATION_STRATEGY.md`
- `WAVE3_FINAL_EXECUTION_PLAN.md`
- `docs/security/*.md`

## Merge Decision Matrix

| Scenario | Merge Required? | Reason |
|----------|----------------|--------|
| **Firebase Installation** | ❌ NO | Package installed via pip, not git |
| **Firebase Credentials** | ❌ NO | Copied via gcloud scp, not git |
| **Query Scripts** | ❌ NO | Already on VM from previous waves |
| **Custom Modes** | ❌ NO | Already on VM from previous waves |
| **Wave 3 Docs** | ❌ NO | Documentation only, not needed on VM |
| **Future Waves** | ⚠️ MAYBE | If scripts change, sync via git pull |

## VM File Sync Strategy

### Current Approach (Correct)

**For Wave 3**:
1. ✅ Use existing scripts on VM (no git pull needed)
2. ✅ Install Firebase package (pip)
3. ✅ Copy credentials (gcloud scp)
4. ✅ Generate phase scripts locally
5. ✅ Upload phase scripts to VM (gcloud scp)

**No merge to main required**.

### When Would Merge Be Required?

**Scenario 1**: Script changes in `scripts/query_kb.py`
- If we modify the query script
- VM would need `git pull` to get updates
- But for Wave 3: script unchanged, no pull needed

**Scenario 2**: Custom mode changes in `.bob/custom_modes.yaml`
- If we modify mode definitions
- VM would need `git pull` to get updates
- But for Wave 3: modes unchanged, no pull needed

**Scenario 3**: Phase script changes (e.g., `scripts/phase_4_5_ticket_review_mcp.py`)
- If we modify phase scripts
- VM would need `git pull` to get updates
- But for Wave 3: scripts unchanged, no pull needed

## Recommendation

### For Wave 3 Launch

**DO NOT MERGE** - Proceed with current setup:

```bash
# 1. Install Firebase on VM (no git needed)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="pip3 install firebase-admin"

# 2. Copy credentials to VM (no git needed)
gcloud compute scp firebase-credentials.json \
  v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ \
  --zone=us-central1-a

# 3. Test connectivity (no git needed)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && python3 scripts/query_kb.py test"

# 4. Launch Wave 3 (scripts uploaded via gcloud scp)
# No git merge required
```

### For Future Waves

**Consider merging IF**:
- Scripts change (query_kb.py, phase scripts)
- Custom modes change (mode definitions, rules)
- Need to sync VM with latest codebase

**For Wave 3**: No changes to scripts/modes, so **merge NOT required**.

## Commit Strategy for Wave 3 Docs

### Option A: Commit Now (Recommended)

```bash
git add FIREBASE_INTEGRATION_CORRECTION.md
git add WAVE3_API_ROTATION_STRATEGY.md
git add WAVE3_FINAL_EXECUTION_PLAN.md
git add docs/security/VM_FIREBASE_SETUP_REQUIRED.md
git commit -m "docs: Wave 3 planning and Firebase integration analysis"
git push origin gitbutler/workspace
```

**Benefit**: Documentation preserved, no impact on VM

### Option B: Commit After Wave 3

```bash
# Wait until Wave 3 complete
# Then commit all Wave 3 docs together
```

**Benefit**: Single commit with complete Wave 3 story

### Recommendation

**Option A** - Commit now:
- Documentation is complete
- No impact on VM (docs not needed there)
- Preserves planning decisions
- Can reference in future waves

## Summary

**Question**: Do we need to merge before Wave 3 for Firebase fixes to take effect?

**Answer**: **NO**

**Reason**:
1. Firebase credentials are gitignored (not in git)
2. Firebase package installed via pip (not git)
3. Scripts already on VM from previous waves
4. Custom modes already on VM from previous waves
5. Wave 3 docs are local only (not needed on VM)

**Action**: Proceed with Firebase installation on VM, no merge required.

**Optional**: Commit Wave 3 planning docs to preserve decisions (no impact on VM).

---

**Status**: Ready to proceed with Wave 3 launch without merge.

**Next Step**: Install Firebase on VM, then launch Wave 3 Phase 0.