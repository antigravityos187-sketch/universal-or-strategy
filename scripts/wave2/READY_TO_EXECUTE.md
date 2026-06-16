# Wave 2 Phase 0 - Ready to Execute

## ✅ All Scripts Fixed and Validated

### What Was Fixed
1. **Archived bad scripts** with `write_to_file`/`read_file` tool instructions
2. **Generated correct scripts** using shell commands (`cat >`, `ls`, `wc -l`)
3. **Custom mode verified** - `.bob/custom_modes.yaml` is correct (no changes needed)
4. **Message template** uses shell commands only

### Files Generated
- `_p0_107.sh` through `_p0_115.sh` (9 epic scripts)
- `launch_phase0_all.sh` (parallel launcher)

### Execution Steps

#### Step 1: Upload to VM
```bash
gcloud compute scp _p0_*.sh launch_phase0_all.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

#### Step 2: Deploy Custom Modes (if not already done)
```bash
gcloud compute scp .bob/custom_modes.yaml v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/.bob/ --zone=us-central1-a
```

#### Step 3: TEST Single Epic First (EPIC-CCN-107)
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd /home/malhitticrypto/universal-or-strategy && bash _p0_107.sh'
```

**Expected Output**:
- Agent uses custom mode `v12-phase0-hotspot`
- Creates files using shell commands (not Bob tools)
- Generates `docs/brain/EPIC-CCN-107/00-hotspots.md`
- Generates `docs/brain/EPIC-CCN-107/manifest.json`
- Logs to `logs/phase0/EPIC-CCN-107.log`

#### Step 4: Verify Test Success
```bash
# Check files exist
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-107/'

# Check log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='tail -20 /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-CCN-107.log'
```

#### Step 5: Launch All 9 Epics (if test succeeds)
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd /home/malhitticrypto/universal-or-strategy && bash launch_phase0_all.sh'
```

### Monitoring

**Check running agents**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls'
```

**View specific log**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='tail -f /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-CCN-107.log'
```

**Verify all files created**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md'
```

### Success Criteria

For each epic:
- ✅ `00-hotspots.md` exists and contains jCodemunch analysis
- ✅ `manifest.json` exists with phase 0 status = "completed"
- ✅ Log shows no errors
- ✅ Bobcoin balance decreased by ~10-20 coins

### API Key Allocation (Immutable)

| Epic | API Key | Bobcoins |
|------|---------|----------|
| 107 | b (2).json | 160 |
| 108 | b.json | 160 |
| 109 | bob (1).json | 160 |
| 110 | bob (2).json | 160 |
| 111 | bob (3).json | 160 |
| 112 | bob (4).json | 160 |
| 113 | bob (5).json | 160 |
| 114 | bob (6).json | 160 |
| 115 | bob.json | 160 |

**Total**: 1,440 bobcoins allocated

### Obsidian Kanban Board

**Path**: `C:\Users\Mohammed Khalid\Documents\V12-Agent-Vault`

**Manual Update Required**: After Phase 0 completes, manually update the Kanban board:
- Move completed epics from "Phase 0" to "Phase 1" column
- Update epic cards with completion status

**No automation exists** - this is a manual process.

### What's Different This Time

1. **Shell Commands Only**: Agents use `cat >`, `ls`, `wc -l` instead of Bob tools
2. **Custom Mode**: `v12-phase0-hotspot` with explicit tool list
3. **No Tool Bugs**: Workaround for `write_to_file`/`read_file` path resolution issues
4. **Verified Template**: `phase0_message_template_shell.txt` tested and working

### Reference Documentation

- **Tool Bug Analysis**: `scripts/wave2/TOOL_ISSUE_ANALYSIS.md`
- **Shell Workaround**: `scripts/wave2/SOLUTION_SHELL_COMMANDS.md`
- **Cleanup Log**: `scripts/wave2/CLEANUP_BAD_SCRIPTS.md`
- **Skill Documentation**: `.bob/skills/gcp-vm-wave-execution/skill.md`

---

**Ready to execute!** Start with Step 1 (upload) and Step 3 (test single epic).