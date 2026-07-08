# Wave 1 Phase 0 - Tmux Execution Guide

**Date**: 2026-06-14
**Execution Strategy**: Option C (Tmux Split Panes)
**Epics**: EPIC-006 through EPIC-015 (10 epics)
**Duration**: ~2 minutes (parallel execution)
**Visibility**: Watch all 10 epics simultaneously

---

## Overview

This guide covers executing Phase 0 for 10 epics using tmux split panes. You'll see all 10 epics running in a 2x5 grid layout, allowing real-time monitoring of all executions simultaneously.

## Prerequisites

- ✅ VM running: `v12-test-golden-v2`
- ✅ Tmux installed on VM (check with `tmux -V`)
- ✅ Phase 0 scripts uploaded: `_p0_006.sh` through `_p0_015.sh`
- ✅ Launcher uploaded: `launch_phase0_tmux.sh`
- ✅ Monitor uploaded: `check_tmux_status.sh`

## Tmux Layout

```
┌─────────────────────────────────────────────────────────┐
│  EPIC-006  │  EPIC-007  │                               │
├────────────┼────────────┤                               │
│  EPIC-008  │  EPIC-009  │                               │
├────────────┼────────────┤      2x5 Grid Layout          │
│  EPIC-010  │  EPIC-011  │      10 Panes Total           │
├────────────┼────────────┤                               │
│  EPIC-012  │  EPIC-013  │                               │
├────────────┼────────────┤                               │
│  EPIC-014  │  EPIC-015  │                               │
└─────────────────────────────────────────────────────────┘
```

**Features**:
- Mouse scrolling enabled
- Each pane shows live output
- Navigate with arrow keys
- Zoom any pane to fullscreen
- Detach/reattach without stopping execution

---

## Step-by-Step Execution

### Step 1: Upload Scripts to VM

From your local machine:

```powershell
# Upload Phase 0 scripts (10 files)
gcloud compute scp scripts/wave1/_p0_006.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_007.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_008.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_009.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_010.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_011.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_012.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_013.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_014.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_015.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Upload launcher and monitor
gcloud compute scp scripts/wave1/launch_phase0_tmux.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/check_tmux_status.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

**Faster (bulk upload)**:
```powershell
gcloud compute scp scripts/wave1/_p0_*.sh scripts/wave1/launch_phase0_tmux.sh scripts/wave1/check_tmux_status.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

### Step 2: Make Scripts Executable

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && chmod +x _p0_*.sh launch_phase0_tmux.sh check_tmux_status.sh"
```

### Step 3: Launch Tmux Session

**Option A: Launch and Auto-Attach** (Recommended)
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && ./launch_phase0_tmux.sh"
```

This will:
1. Create tmux session `wave1-p0`
2. Split into 2x5 grid
3. Launch all 10 epics
4. Automatically attach so you can watch

**Option B: Launch in Background**
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && nohup ./launch_phase0_tmux.sh &"
```

Then attach later:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a
tmux attach -t wave1-p0
```

### Step 4: Watch Execution

Once attached, you'll see all 10 epics running simultaneously.

**Tmux Controls**:

| Action | Keys |
|--------|------|
| Navigate panes | `Ctrl+B` then Arrow Keys |
| Zoom pane (fullscreen) | `Ctrl+B` then `Z` (toggle) |
| Scroll mode | `Ctrl+B` then `[` (press `q` to exit) |
| Detach (keep running) | `Ctrl+B` then `D` |
| Mouse scroll | Just scroll (enabled by default) |

**Tips**:
- Use `Ctrl+B Z` to zoom a pane and read details
- Use mouse to scroll any pane
- Press `Ctrl+B D` to detach and check status externally

### Step 5: Monitor from Outside (Optional)

If you detached or want to check status without attaching:

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && ./check_tmux_status.sh"
```

This shows:
- Which epics are complete
- Which files were created
- Bobcoin usage
- Pane status

### Step 6: Validate Completion

**Check all epics finished**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && ./check_tmux_status.sh"
```

**Expected output**:
```
Completion Status:
  EPIC-006: COMPLETE
  EPIC-007: COMPLETE
  EPIC-008: COMPLETE
  EPIC-009: COMPLETE
  EPIC-010: COMPLETE
  EPIC-011: COMPLETE
  EPIC-012: COMPLETE
  EPIC-013: COMPLETE
  EPIC-014: COMPLETE
  EPIC-015: COMPLETE

Files Created:
  EPIC-006: hotspots.md (150 lines) + manifest.json
  EPIC-007: hotspots.md (120 lines) + manifest.json
  ...
```

**Verify file count**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{006..015}/00-hotspots.md 2>/dev/null | wc -l"
```

Expected: `10`

**Extract bobcoin usage**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -h 'Cost:.*Balance:.*Model:' /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-*.log"
```

### Step 7: Kill Session (After Completion)

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tmux kill-session -t wave1-p0"
```

Or from inside tmux:
```bash
Ctrl+B then :kill-session
```

---

## Troubleshooting

### Issue: "tmux: command not found"

**Solution**: Install tmux on VM
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a
sudo apt-get update
sudo apt-get install -y tmux
```

### Issue: "session not found: wave1-p0"

**Meaning**: Session already completed or never started

**Solution**: Check logs
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/logs/phase0/"
```

### Issue: Can't see output in pane

**Solution**: 
1. Press `Ctrl+B` then `Z` to zoom the pane
2. Use mouse to scroll up
3. Press `Ctrl+B` then `[` to enter scroll mode

### Issue: Pane shows "EPIC-XXX DONE" but files missing

**Meaning**: File persistence bug (missing `--yolo` flag)

**Solution**: Check script has `bob --yolo` (not just `bob`)

### Issue: Want to stop all epics

**Solution**: Kill session
```bash
tmux kill-session -t wave1-p0
```

---

## Success Criteria

- ✅ All 10 tmux panes show "EPIC-XXX DONE"
- ✅ 20 files created (10 hotspots.md + 10 manifest.json)
- ✅ All files >100 lines (hotspots.md)
- ✅ Bobcoin usage reported for all 10 epics
- ✅ Model names reported for all 10 epics
- ✅ No errors in logs

---

## Next Steps

After Phase 0 completes:

1. **Sync files to local**:
   ```bash
   gcloud compute scp --recurse v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{006..015} docs/brain/ --zone=us-central1-a
   ```

2. **Create Phase 1 scripts** for all 15 epics (001-015)

3. **Launch Phase 1** using same tmux pattern

4. **Continue through Phase 6** for all 15 epics

---

## Comparison: Tmux vs Screen vs Sequential

| Aspect | Tmux | Screen | Sequential |
|--------|------|--------|------------|
| **Duration** | 2 min | 2 min | 20 min |
| **Visibility** | All 10 simultaneously | One at a time (attach) | One at a time |
| **Complexity** | Medium | Low | Very Low |
| **Monitoring** | Real-time grid | Attach/detach | Watch one |
| **Recovery** | Detach/reattach | Detach/reattach | N/A |
| **Best For** | Visual monitoring | Background execution | Debugging |

**Recommendation**: Tmux for Wave 1 (visual feedback), Screen for Wave 2+ (proven reliability)

---

## Key Learnings

1. **Tmux Layout**: 2x5 grid is optimal for 10 panes on standard terminal
2. **Mouse Support**: Essential for easy scrolling and navigation
3. **Zoom Feature**: Critical for reading detailed output
4. **Detach Safety**: Can detach and reattach without stopping execution
5. **Status Script**: External monitoring without attaching is valuable

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T06:18:00Z
**Maintainer**: V12 Orchestration Team