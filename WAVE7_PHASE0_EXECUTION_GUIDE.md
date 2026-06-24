# Wave 7 Phase 0 Execution Guide

**Date**: 2026-06-23  
**Status**: Ready to Execute  
**Current Progress**: 102/161 (63.4%)  
**Target**: 161/161 (100%)

## Quick Start

### Execute Recovery Script
```bash
# From /home/malhitticrypto/universal-or-strategy
./resume_wave7_phase0.sh
```

**Estimated Time**: ~4 hours (59 epics × 4 minutes)  
**Cost**: ~$X bobcoins (depends on API usage)

## What the Script Does

1. **Executes 59 incomplete epics** through Phase 0 (Hotspot Analysis)
2. **Uses absolute paths** to work around Bob IDE shell PATH issue
3. **4-minute polling intervals** for cost optimization (88% savings)
4. **Tracks progress** and logs failures
5. **Verifies completion** after each epic
6. **Final verification** count at end

## Monitoring Progress

### Real-Time Monitoring
```bash
# In a separate terminal, watch progress
watch -n 60 'find docs/brain/EPIC-CCN-* -name "00-hotspots.md" 2>/dev/null | wc -l'
```

### Check Failures
```bash
# View failed epics (if any)
cat phase0_failures.txt
```

### Check Logs
```bash
# View recent Phase 0 logs
find logs/ -name "*phase0*.log" -mmin -60 | head -10
```

## Incomplete Epic Ranges

### Range 1: EPIC-CCN-081 through 106
- **Count**: 26 epics
- **Complexity**: Mixed (low to high)
- **Scripts**: `_p0_081.sh` through `_p0_106.sh`

### Range 2: EPIC-CCN-126 through 161
- **Count**: 33 epics (excluding 128, 129, 155 which are complete)
- **Complexity**: Mixed (low to high)
- **Scripts**: `_p0_126.sh`, `_p0_127.sh`, `_p0_130.sh`, etc.

## Success Criteria

### Per Epic
- ✅ Script executes without errors
- ✅ `docs/brain/EPIC-CCN-XXX/00-hotspots.md` file created
- ✅ File contains hotspot analysis
- ✅ No Bob CLI errors in output

### Overall
- ✅ 161/161 epics with `00-hotspots.md` files
- ✅ All methods analyzed and categorized
- ✅ No errors in logs
- ✅ Ready for Phase 1 (Scope Definition)

## Troubleshooting

### If Script Fails

**Check for missing scripts**:
```bash
# Verify all Phase 0 scripts exist
for i in 81 82 83 84 85 86 87 88 89 90 91 92 93 94 95 96 97 98 99 100 101 102 103 104 105 106 126 127 130 131 132 133 134 135 136 137 138 139 140 141 142 143 144 145 146 147 148 149 150 151 152 153 154 156 157 158 159 160 161; do
  script="_p0_$(printf '%03d' $i).sh"
  if [ ! -f "$script" ]; then
    echo "Missing: $script"
  fi
done
```

**Check Bob CLI availability**:
```bash
# Verify Bob CLI is accessible
~/.npm-global/bin/bob --version
```

**Check API keys**:
```bash
# Verify environment variables are set
echo $ANTHROPIC_API_KEY | cut -c1-10
echo $GOOGLE_API_KEY | cut -c1-10
```

### If Epics Fail

1. **Check failure log**: `cat phase0_failures.txt`
2. **Review epic logs**: Check `logs/` directory for error details
3. **Apply Recovery Loop Protocol**:
   - Identify root cause
   - Fix issue
   - Re-run failed epic manually
   - Update failure log

### Manual Epic Execution

If you need to run a single epic manually:
```bash
# Execute specific epic
/usr/bin/bash _p0_081.sh

# Verify completion
ls -la docs/brain/EPIC-CCN-081/00-hotspots.md
```

## Recovery Loop Protocol

If failures occur, follow this protocol:

### Step 1: Identify Failures
```bash
cat phase0_failures.txt
```

### Step 2: Analyze Root Cause
```bash
# Check logs for the failed epic
grep -r "EPIC-CCN-081" logs/ | tail -20
```

### Step 3: Fix and Re-run
```bash
# Fix the issue (e.g., missing file, API error)
# Then re-run the specific epic
/usr/bin/bash _p0_081.sh
```

### Step 4: Verify Fix
```bash
# Confirm the epic now has output file
ls -la docs/brain/EPIC-CCN-081/00-hotspots.md
```

### Step 5: Update Status
```bash
# Remove from failures list if fixed
grep -v "EPIC-CCN-081" phase0_failures.txt > temp && mv temp phase0_failures.txt
```

## After Completion

### Verify Final Count
```bash
# Should show 161
/usr/bin/find docs/brain/EPIC-CCN-* -name '00-hotspots.md' 2>/dev/null | /usr/bin/wc -l
```

### Proceed to Phase 1
Once 161/161 complete:
1. Review Phase 0 outputs for quality
2. Generate Phase 1 scripts using Building-Blocks Method
3. Execute Phase 1 (Scope Definition) for all 161 epics

## Cost Optimization

### 4-Minute Polling
- **Interval**: 240 seconds between epics
- **Savings**: 88% vs 30-second polling
- **Total Time**: ~4 hours for 59 epics
- **Trade-off**: Slower execution, much lower cost

### Batch Execution
- All 59 epics run sequentially
- No parallel execution (to avoid API rate limits)
- Progress tracked in real-time

## Shell PATH Issue

### Current Workaround
All commands in the script use absolute paths:
- `/usr/bin/bash` instead of `bash`
- `/usr/bin/find` instead of `find`
- `/usr/bin/wc` instead of `wc`

### Permanent Fix Applied
`.bob/settings.json` updated with:
```json
"shell": {
  "env": {
    "PATH": "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
  }
}
```

**Note**: Fix takes effect after Bob IDE restart. Until then, use absolute paths.

## Files Created

- ✅ `resume_wave7_phase0.sh` - Master execution script
- ✅ `incomplete_epics.txt` - List of 59 incomplete epics
- ✅ `analyze_wave7_status.py` - Status analysis tool
- ✅ `phase0_failures.txt` - Created during execution (if failures occur)

## Next Phase

After Phase 0 completion (161/161):
1. **Phase 1**: Scope Definition
2. **Phase 1.5**: Scope Boundary Validation
3. **Phase 2**: Architecture Planning
4. **Phase 3**: DNA & PR Audit
5. **Phase 4**: Ticket Generation
6. **Phase 5**: Ticket Execution
7. **Phase 5.V**: Per-Ticket Verification
8. **Phase 6**: Final Review

## Support

If you encounter issues:
1. Check this guide's Troubleshooting section
2. Review `BOB_IDE_SHELL_PATH_DIAGNOSIS.md` for shell issues
3. Consult `docs/workflow/AUTONOMOUS_REFACTOR_MODE_INSTRUCTIONS.md`
4. Apply Recovery Loop Protocol for failures

---

**Ready to execute**: `./resume_wave7_phase0.sh`