# Wave 7 Phase 0 Completion Guide

## Goal
Ensure all 161 epics complete Phase 0 (Hotspot Analysis) successfully.

## Current Status (as of 2026-06-22 01:40 UTC)
- **Target**: 161/161 epics (100%)
- **Progress**: Check with `./scripts/wave7/verify_phase0_completion.sh`

## Verification Commands

### On VM (Primary)
```bash
ssh malhitticrypto@34.69.114.138
cd universal-or-strategy

# Quick check
./scripts/wave7/verify_phase0_completion.sh

# Detailed status
./scripts/wave7/check_wave7_status.sh 0
```

### From Local
```bash
# SSH into VM and check
ssh malhitticrypto@34.69.114.138 "cd universal-or-strategy && ./scripts/wave7/verify_phase0_completion.sh"
```

## Success Criteria

Each epic MUST have:
1. ✅ `docs/brain/EPIC-W7-XXX/00-hotspots.md` exists
2. ✅ `docs/brain/EPIC-W7-XXX/manifest.json` shows phase 0 completed
3. ✅ File sizes reasonable (>500 bytes for hotspots.md)
4. ✅ No errors in `logs/phase0/EPIC-W7-XXX.log`

## Common Issues & Solutions

### Issue 1: Bobcoin Budget Exhaustion
**Symptom**: Epic logs show "You've gone over your budget allowance"

**Solution**:
1. User provides fresh API key
2. Update `scripts/wave7/generate_phase0_scripts_fixed.py` with new key
3. Regenerate affected scripts: `python3 scripts/wave7/generate_phase0_scripts_fixed.py --failed-only`
4. Deploy and re-launch: `./scripts/wave7/recover_failed_phase0.sh`

### Issue 2: Heredoc Syntax Errors
**Symptom**: Logs show "syntax error: unexpected end of file"

**Solution**: Already fixed in `generate_phase0_scripts_fixed.py` (no heredocs)

### Issue 3: File I/O Failures
**Symptom**: Bob reports "File not found" or files not persisting

**Solution**: Scripts already use `execute_command` with explicit `cwd` parameter

### Issue 4: Screen Session Died
**Symptom**: No active screen sessions but epics incomplete

**Solution**:
```bash
# Check for dead sessions
screen -ls

# Re-launch failed epics
./scripts/wave7/recover_failed_phase0.sh
```

## Recovery Workflow

If any epics are incomplete:

### Step 1: Identify Failed Epics
```bash
./scripts/wave7/verify_phase0_completion.sh > failed_status.txt
```

### Step 2: Check Logs for Root Cause
```bash
# Check most recent failures
tail -100 logs/phase0/EPIC-W7-*.log | grep -i error
```

### Step 3: Apply Fix
- **Bobcoin exhaustion**: Swap API key (see Issue 1)
- **Syntax errors**: Already fixed (shouldn't occur)
- **File I/O**: Already fixed (shouldn't occur)
- **Other**: Document in `scripts/wave7/RECOVERY_STATUS.md`

### Step 4: Regenerate and Re-launch
```bash
# Regenerate failed scripts
python3 scripts/wave7/generate_phase0_scripts_fixed.py --failed-only

# Commit and deploy
git add scripts/wave7/_p0_*.sh /tmp/phase0_msg_*.txt
git commit -m "fix(wave7): Regenerate failed Phase 0 scripts"
git push origin main

# On VM: Pull and re-launch
git pull origin main
./scripts/wave7/recover_failed_phase0.sh
```

### Step 5: Monitor Until 161/161
```bash
# Check every 5 minutes
watch -n 300 './scripts/wave7/verify_phase0_completion.sh'
```

## API Key Management

### Current Keys (20 total)
1. bob.json
2. bob (1).json
3. bob (2).json
4. bob (4).json
5. bob (5).json
6. bob (6).json
7. b.json
8. b (3).json
9. jessica.json
10. mikethelife.json
11. iyanajackson.json
12. pepeescobar.json
13. rakaarababa.json
14. ranirabah (1).json
15. sammy96.json
16. sean.carter.jr@atomicmail.io.json
17. tory.json
18. alprofit.json
19. jimmydore.json
20. api_rotation.json

**Note**: `bob (3).json` was deleted due to exhaustion

### Key Distribution
With 20 keys and 161 epics:
- Each key handles: 161 ÷ 20 = 8 epics (with 1 epic getting a 9th)
- Round-robin: Epic 1→Key 1, Epic 2→Key 2, ..., Epic 21→Key 1, etc.

### Budget Tracking
- Each key: 160 bobcoins
- Per epic cost: ~15 bobcoins (Phase 0 only)
- Key capacity: 160 ÷ 15 = ~10 epics
- **Risk**: Keys may exhaust before all 8-9 assigned epics complete

### Swap Strategy
When a key exhausts:
1. User provides fresh key
2. Update `API_FILES` list in generator
3. Regenerate affected scripts
4. Deploy and re-launch

## Final Verification

Before proceeding to Phase 1:

```bash
# Must show 161/161
./scripts/wave7/verify_phase0_completion.sh

# Verify file sizes (should be >500 bytes each)
find docs/brain/EPIC-W7-*/00-hotspots.md -size -500c

# Check for empty manifests
find docs/brain/EPIC-W7-*/manifest.json -size -50c

# Verify no errors in logs
grep -i "error\|failed\|exception" logs/phase0/EPIC-W7-*.log | wc -l
```

All checks must pass before Phase 1 launch.

## Next Phase

Once 161/161 complete:
1. ✅ Verify completion: `./scripts/wave7/verify_phase0_completion.sh`
2. ✅ Commit results: `git add docs/brain/EPIC-W7-*/00-hotspots.md docs/brain/EPIC-W7-*/manifest.json`
3. ✅ Push to GitHub: `git commit -m "feat(wave7): Complete Phase 0 (161/161)" && git push`
4. ✅ Generate Phase 1 scripts: `python3 scripts/wave7/generate_phase1_scripts.py`
5. ✅ Launch Phase 1: `./scripts/wave7/launch_phase1_all.sh`

## Cost Tracking

Phase 0 expected costs:
- Per epic: ~15 bobcoins
- Total: 161 × 15 = 2,415 bobcoins
- With 20 keys @ 160 each: 3,200 bobcoins available
- **Buffer**: 785 bobcoins (32% overhead)

Monitor actual usage in logs.

## Support

If issues persist:
1. Document in `scripts/wave7/RECOVERY_STATUS.md`
2. Check `docs/protocol/SCREEN_SESSION_SCRIPT_PROTOCOL.md`
3. Review `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
4. Escalate to Director if unresolvable

---

**Last Updated**: 2026-06-22 01:40 UTC  
**Status**: Phase 0 in progress, monitoring for 161/161 completion