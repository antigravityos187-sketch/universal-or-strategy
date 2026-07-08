# Wave 7 Phase 1 - Quick Start

## Prerequisites Check
```bash
# Verify Phase 0 complete (expect: 18)
find docs/brain/EPIC-W7-* -name '00-hotspots.md' | wc -l

# Verify Phase 1 not started (expect: 0)
find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l

# Verify Bob CLI
~/.npm-global/bin/bob --version

# Verify API key
grep BOBSHELL_API_KEY ~/.bashrc
```

## Execute Phase 1 Batch

```bash
# Make executable
chmod +x scripts/wave7/launch_phase1_batch.sh

# Launch (background)
nohup bash scripts/wave7/launch_phase1_batch.sh > logs/wave7_phase1_launch.log 2>&1 &

# Monitor
tail -f logs/wave7_phase1_launch.log
```

## Monitor Progress

```bash
# Active sessions
screen -ls | grep wave7_phase1 | wc -l

# Completed epics
find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l

# Progress percentage
TOTAL=18
COMPLETED=$(find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l)
echo "Progress: $COMPLETED/$TOTAL ($(($COMPLETED * 100 / $TOTAL))%)"
```

## Troubleshooting

```bash
# Check for errors
grep -r "❌" logs/wave7/phase1/

# View specific epic log
tail -f logs/wave7/phase1/EPIC-W7-001.log

# Attach to session
screen -r wave7_phase1_EPIC-W7-001

# Re-run failed epic
bash scripts/wave7/phase1_scripts/EPIC-W7-XXX_phase1.sh
```

## Success Criteria

- ✅ 18/18 epics complete
- ✅ All `00-scope.md` files exist
- ✅ No active screen sessions
- ✅ No errors in logs

## Next Phase

After 18/18 completion:
```bash
bash scripts/wave7/launch_phase1_5_batch.sh
```

## Full Documentation

See: `WAVE7_PHASE1_EXECUTION_GUIDE.md`