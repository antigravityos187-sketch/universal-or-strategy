# Deploy Phase 0 Fix to VM

## Quick Start

```bash
# 1. Deploy fixed scripts to VM
gcloud compute scp _p0_*.sh v12-test-golden-v2:~/universal-or-strategy/ \
  --zone=us-central1-a \
  --project=project-14c86305-3cba-493f-a73

# 2. SSH into VM
gcloud compute ssh v12-test-golden-v2 \
  --zone=us-central1-a \
  --project=project-14c86305-3cba-493f-a73

# 3. Launch Phase 0 (all 9 epics)
cd ~/universal-or-strategy
bash scripts/wave2/launch_phase0_all_screen.sh

# 4. Monitor progress (in separate terminal)
watch -n 5 'screen -ls | grep phase0'

# 5. Check logs (after completion)
tail -20 logs/phase0/EPIC-CCN-*.log

# 6. Verify files created
for i in 107 108 109 110 111 112 113 114 115; do
  echo "=== EPIC-CCN-$i ==="
  ls -lh docs/brain/EPIC-CCN-$i/
done
```

## What Was Fixed

**Problem**: `run_shell_command` tool failed silently in SSH/screen mode
**Solution**: Replaced with `execute_command` + explicit `cwd` parameter

**Files Modified**: All 9 scripts (`_p0_107.sh` through `_p0_115.sh`)

## Success Criteria

✅ All 9 agents complete (DONE_EXIT=0)
✅ 18 files created:
   - 9 × `00-hotspots.md` (>100 lines each)
   - 9 × `manifest.json` (~20 lines each)
✅ Files verified with non-zero sizes
✅ Ready for Phase 1 (Scope Definition)

## Troubleshooting

### If files still missing:
```bash
# Check agent logs for errors
grep -i "error\|fail" logs/phase0/EPIC-CCN-*.log

# Verify execute_command was used (not run_shell_command)
grep "execute_command" _p0_107.sh

# Check if agents completed
grep "DONE_EXIT" logs/phase0/EPIC-CCN-*.log
```

### If agents didn't start:
```bash
# Check screen sessions
screen -ls

# Reattach to a session
screen -r phase0-epic-107

# Kill all and restart
pkill -f "bob --chat-mode v12-phase0-hotspot"
bash scripts/wave2/launch_phase0_all_screen.sh
```

## Next Steps After Success

1. **Verify Output Quality**:
   ```bash
   # Check one hotspot analysis
   cat docs/brain/EPIC-CCN-107/00-hotspots.md | head -50
   
   # Verify manifest structure
   cat docs/brain/EPIC-CCN-107/manifest.json
   ```

2. **Proceed to Phase 1** (Scope Definition):
   ```bash
   # Launch Phase 1 for all 9 epics
   bash scripts/wave2/launch_phase1_all_screen.sh
   ```

3. **Update Roadmap**:
   ```bash
   # Mark Phase 0 complete
   python scripts/update_roadmap.py --phase 0 --status completed
   ```

## Documentation

- **Fix Details**: `docs/workflow/WAVE2_EXECUTE_COMMAND_FIX.md`
- **Wave 2 Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md`
- **Custom Mode**: `.bob/custom_modes.yaml` (v12-phase0-hotspot)