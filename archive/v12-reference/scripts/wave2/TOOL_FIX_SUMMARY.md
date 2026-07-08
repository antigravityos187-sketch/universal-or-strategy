# Wave 2 Phase 0 - Tool Fix Summary

**Date**: 2026-06-13  
**Issue**: `write_to_file` tool unavailable in `v12-phase0-hotspot` mode  
**Fix**: Updated `.bob/custom_modes.yaml` with explicit tool configuration  
**Status**: ✅ Ready for deployment

---

## The Fix

**File**: `.bob/custom_modes.yaml` (lines 145-151)

**Before** (broken):
```yaml
groups:
  - read
  - edit  # Simple group reference - not working on VM
  - command
  - mcp
```

**After** (fixed):
```yaml
groups:
  - read
  - - edit
    - fileRegex: \.(md|json|yaml|yml|txt)$
      description: Documentation and config files (Phase 0 outputs)
  - command
  - mcp
```

**Why**: Explicit nested structure with file regex matches the working pattern from `v12-epic-planner` mode.

---

## What Stayed the Same

✅ **API Key Allocation**: Identical to Wave 2 v4  
✅ **Budget**: 150 bobcoins per epic (1,350 total)  
✅ **Epic List**: Same 9 epics (107-115)  
✅ **Launch Scripts**: `launch_phase0_all.sh` unchanged  
✅ **Mode**: Still using `v12-phase0-hotspot`

---

## Deployment

```powershell
# Run from repo root
powershell -File .\scripts\wave2\deploy_and_test_tool_fix.ps1
```

This will:
1. Push fixed `.bob/custom_modes.yaml` to VM
2. Generate Phase 0 scripts (same as before)
3. Push scripts to VM
4. Display testing instructions

---

## Testing

**Option A: Single Epic Test** (recommended first)
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a
cd /home/malhitticrypto/universal-or-strategy
bash _p0_107.sh
ls -la docs/brain/EPIC-CCN-107/  # Should show 00-hotspots.md and manifest.json
```

**Option B: Full Launch** (after single test passes)
```bash
bash launch_phase0_all.sh
screen -r p0-107  # Monitor any epic
tail -f logs/phase0/EPIC-CCN-107.log  # Check logs
```

---

## Success Criteria

✅ Files created: `docs/brain/EPIC-CCN-{ID}/00-hotspots.md`  
✅ Files created: `docs/brain/EPIC-CCN-{ID}/manifest.json`  
✅ No "tool not available" errors in logs  
✅ All 9 epics complete Phase 0 successfully

---

## References

- **Deployment Script**: `scripts/wave2/deploy_and_test_tool_fix.ps1`
- **Phase 0 Generator**: `scripts/wave2/launch_phase0_v3_custom_mode.py`
- **Launch Script**: `scripts/wave2/launch_phase0_all.sh`
- **Custom Modes**: `.bob/custom_modes.yaml`