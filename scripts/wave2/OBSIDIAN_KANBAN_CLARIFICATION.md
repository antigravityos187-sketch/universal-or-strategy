# Obsidian Kanban Board - Clarification

## Question
"Will the agents update this while working?"

## Answer: YES - AUTOMATIC via Monitoring Script!

### How It Works (Fully Automated)

**Local Kanban Board**: `C:\Users\Mohammed Khalid\Documents\V12-Agent-Vault\WAVE_2_KANBAN.md`

**Monitoring Script**: `scripts/monitor_vm_progress.py`
- ✅ Polls VM every 5 minutes via SSH
- ✅ Reads manifest.json files from VM
- ✅ Updates local Kanban board automatically
- ✅ No manual sync required!

**Workflow**:
1. Start monitoring: `python scripts/monitor_vm_progress.py`
2. Agents work on VM (update manifest.json files)
3. Monitoring script polls VM every 5 minutes
4. Local Kanban board updates automatically
5. Obsidian detects file change and refreshes view

### What Agents Update on VM

Agents update these files **on the VM**:
- ✅ `docs/brain/EPIC-CCN-{ID}/00-hotspots.md`
- ✅ `docs/brain/EPIC-CCN-{ID}/manifest.json` ← **Monitoring script reads this**
- ✅ `logs/phase0/EPIC-CCN-{ID}.log`

### Start Monitoring (One Command)

```bash
cd c:/WSGTA/universal-or-strategy
python scripts/monitor_vm_progress.py
```

**What it does**:
- Polls VM every 5 minutes
- Reads all manifest.json files
- Updates Kanban board with current phase for each epic
- Shows progress in terminal
- Runs until you press Ctrl+C

**Quick check** (single update, no continuous monitoring):
```bash
python scripts/monitor_vm_progress.py --once
```

### Configuration

The monitoring script is already configured for Wave 2:
- **VM**: `v12-test-golden-v2` (update line 15 if different)
- **Zone**: `us-central1-a` (update line 17 if different)
- **Kanban Path**: `C:\Users\Mohammed Khalid\Documents\V12-Agent-Vault\WAVE_2_KANBAN.md` (line 18)
- **Poll Interval**: 5 minutes (line 19)

**Update VM name** if using different VM:
```python
# Line 15 in scripts/monitor_vm_progress.py
VM_NAME = "v12-test-golden-v2"  # Change if needed
```

### Obsidian Integration

**Auto-refresh**: Obsidian detects file changes automatically. When monitoring script updates the Kanban board, Obsidian refreshes the view within seconds.

**Kanban Plugin**: If you have Obsidian Kanban plugin installed, the board renders as native Kanban. Otherwise, displays as readable markdown.

**Pin the dashboard**: Right-click the Kanban file in Obsidian → "Pin" to keep it always visible.

## Summary

**Agents update**: `manifest.json` files on VM (phase status, completion)
**Monitoring script**: Polls VM every 5 minutes, updates local Kanban board
**Obsidian**: Auto-refreshes when Kanban file changes
**You do**: Start monitoring script once, then watch Obsidian update automatically

**No manual sync required!** The monitoring script handles everything.