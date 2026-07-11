# Wave 7 Phase 0 - Real-Time Monitoring Commands

## 🎯 Quick Status Check

### Check Current Progress
```bash
# Count completed epics (should increase from 103 to 161)
find docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l

# Show which epics are complete
ls -1d docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | sed 's|docs/brain/||;s|/00-hotspots.md||' | sort
```

### Watch Live Log (RECOMMENDED)
```bash
# Follow the recovery log in real-time (shows each epic as it completes)
tail -f wave7_phase0_recovery.log

# Press Ctrl+C to stop watching
```

### Watch Progress Counter
```bash
# Auto-refresh every 30 seconds showing completion count
watch -n 30 'echo "Completed: $(find docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)/161"'

# Press Ctrl+C to stop
```

## 📊 Detailed Monitoring

### Check Last 5 Completed Epics
```bash
ls -lt docs/brain/EPIC-CCN-*/00-hotspots.md | head -5
```

### Check Last 20 Lines of Log
```bash
tail -20 wave7_phase0_recovery.log
```

### Search for Errors in Log
```bash
grep -i "error\|fail\|exception" wave7_phase0_recovery.log
```

### Check Which Epic is Currently Running
```bash
# Look for "Executing EPIC-CCN-XXX" in the log
tail -50 wave7_phase0_recovery.log | grep "Executing EPIC-CCN"
```

### Count Remaining Epics
```bash
# Show incomplete epics
for i in $(seq -f '%03g' 1 161); do
  if [ ! -f "docs/brain/EPIC-CCN-$i/00-hotspots.md" ]; then
    echo "EPIC-CCN-$i"
  fi
done | wc -l
```

## 🔍 Advanced Monitoring

### Watch Multiple Metrics at Once
```bash
# Create a monitoring dashboard
watch -n 30 '
echo "=== Wave 7 Phase 0 Progress ==="
echo "Completed: $(find docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)/161"
echo "Remaining: $(for i in $(seq -f "%03g" 1 161); do [ ! -f "docs/brain/EPIC-CCN-$i/00-hotspots.md" ] && echo 1; done | wc -l)"
echo ""
echo "=== Last 5 Completed ==="
ls -lt docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | head -5 | awk "{print \$9}" | sed "s|docs/brain/||;s|/00-hotspots.md||"
echo ""
echo "=== Current Epic ==="
tail -50 wave7_phase0_recovery.log | grep "Executing EPIC-CCN" | tail -1
'
```

### Check Bob CLI Process
```bash
# See if Bob CLI is running
ps aux | grep "bob --yolo" | grep -v grep

# Count how many Bob processes are running
ps aux | grep "bob --yolo" | grep -v grep | wc -l
```

### Monitor API Key Usage
```bash
# Check which API keys are being used (from log)
grep "API Key" wave7_phase0_recovery.log | tail -10
```

## 🚨 Troubleshooting

### If Progress Seems Stuck
```bash
# Check if the resume script is still running
ps aux | grep "resume_wave7_phase0.sh" | grep -v grep

# Check last activity in log
tail -1 wave7_phase0_recovery.log
```

### If You See Errors
```bash
# Extract all error messages
grep -i "error\|fail\|exception" wave7_phase0_recovery.log > errors.txt
cat errors.txt
```

### Check Disk Space
```bash
# Make sure we have enough space for 161 epics
df -h .
```

## 📈 Estimated Time Remaining

```bash
# Calculate estimated completion time
python3 << 'EOF'
import os
from datetime import datetime, timedelta

# Count completed
completed = len([f for f in os.listdir('docs/brain') if f.startswith('EPIC-CCN-') and os.path.exists(f'docs/brain/{f}/00-hotspots.md')])
remaining = 161 - completed

# Estimate (5 minutes per epic)
minutes_remaining = remaining * 5
eta = datetime.now() + timedelta(minutes=minutes_remaining)

print(f"Completed: {completed}/161")
print(f"Remaining: {remaining}")
print(f"Estimated time: {minutes_remaining} minutes ({minutes_remaining/60:.1f} hours)")
print(f"ETA: {eta.strftime('%Y-%m-%d %H:%M:%S')}")
EOF
```

## 🎬 Recommended Monitoring Setup

**Option 1: Single Terminal (Simple)**
```bash
# Just watch the log
tail -f wave7_phase0_recovery.log
```

**Option 2: Split Terminal (Advanced)**
```bash
# Terminal 1: Watch log
tail -f wave7_phase0_recovery.log

# Terminal 2: Watch progress counter
watch -n 30 'echo "Completed: $(find docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)/161"'
```

**Option 3: Dashboard (Most Info)**
```bash
# Single command showing everything
watch -n 30 '
echo "=== Wave 7 Phase 0 Dashboard ==="
echo "Completed: $(find docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)/161"
echo ""
echo "=== Last Completed ==="
ls -lt docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | head -3 | awk "{print \$9}" | sed "s|docs/brain/||;s|/00-hotspots.md||"
echo ""
echo "=== Current Epic ==="
tail -50 wave7_phase0_recovery.log | grep "Executing EPIC-CCN" | tail -1
echo ""
echo "=== Recent Log ==="
tail -5 wave7_phase0_recovery.log
'
```

## ✅ Success Indicators

You'll know it's working when you see:
- ✅ "Executing EPIC-CCN-XXX" messages in log
- ✅ "Phase 0 complete" messages
- ✅ New `00-hotspots.md` files appearing in `docs/brain/EPIC-CCN-*/`
- ✅ Completion count increasing from 103 to 161

## 🛑 When to Intervene

Stop and investigate if:
- ❌ No new epics complete after 10 minutes
- ❌ Same error repeating in log
- ❌ Bob CLI process not running
- ❌ Disk space running low