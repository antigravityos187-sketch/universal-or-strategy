# Wave 2 vs /epic-orchestrate Analysis

## Executive Summary

**Wave 2** and **`/epic-orchestrate`** are **DIFFERENT workflows** with different purposes:

- **Wave 2**: Parallel execution of **Phase 0 ONLY** (hotspot analysis) for 9 epics
- **`/epic-orchestrate`**: Sequential execution of **ALL 6 phases** for a single epic

## Key Findings

### 1. Wave 2 Configuration History

| Version | Mode Used | Command | Purpose |
|---------|-----------|---------|---------|
| **v4** (SUCCESSFUL) | `--chat-mode plan` | Built-in mode | Phase 0 only, 9 parallel agents |
| **v3** (ATTEMPTED) | `--chat-mode v12-phase0-hotspot` | Custom mode | Phase 0 only, 9 parallel agents |
| **v2** | `--chat-mode plan` | Built-in mode | Phase 0 only, 9 parallel agents |

**Evidence from `_wave2_v4_launch_generated.sh` (line 22)**:
```bash
bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow...'
```

**Evidence from `launch_phase0_v3_custom_mode.py` (line 50)**:
```bash
bob --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_id}.txt)"
```

### 2. `/epic-loop` vs `/epic-orchestrate`

#### `/epic-loop` (DEPRECATED)
- **Status**: Replaced by `/autonomous-refactor`
- **Purpose**: Autonomous multi-epic orchestration (EPIC-CCN-15 through EPIC-CCN-45)
- **Migration Path**: Use `/autonomous-refactor` instead
- **Reference**: `.bob/commands/epic-loop.md` (lines 10-11)

#### `/epic-orchestrate` (CURRENT)
- **Status**: Active command for V12 Multi-Agent Architecture
- **Purpose**: Orchestrate **single epic** through all 6 phases
- **Phases**: 0 (Hotspot) → 1 (Scope) → 2 (Architecture) → 3 (Audit) → 4 (Tickets) → 5 (Execution) → 6 (Review)
- **Reference**: `.bob/commands/epic-orchestrate.md`

### 3. Wave 2 vs Full Epic Workflow

**Wave 2 (Parallel Phase 0 Only)**:
- 9 epics (EPIC-107 through EPIC-115)
- Phase 0: Hotspot Analysis (PARALLEL)
- Output: 9 × 00-hotspots.md files

**`/epic-orchestrate` (Full 6-Phase Workflow)**:
- Single epic (e.g., EPIC-CCN-21)
- Phase 0: Hotspot Analysis (ask mode)
- Phase 1: Scope Definition (plan mode)
- Phase 2: Architecture Planning (plan mode)
- Phase 3: DNA & PR Audit (advanced mode)
- Phase 4: Ticket Generation (plan mode)
- Phase 5: Ticket Execution (v12-engineer mode)
- Phase 6: Final Review (advanced mode)
- Output: Complete epic with all artifacts + implementation

## Answer to Your Questions

### Q1: "Were any Wave 2 runs using custom modes?"

**YES** - Wave 2 v3 attempted custom mode `v12-phase0-hotspot`:
- Script: `scripts/wave2/launch_phase0_v3_custom_mode.py`
- Command: `bob --chat-mode v12-phase0-hotspot`
- Status: Attempted but encountered tool bugs

**NO** - Wave 2 v4 (successful) used built-in mode:
- Script: `scripts/wave2/_wave2_v4_launch_generated.sh`
- Command: `bob --chat-mode plan`
- Status: Successful execution

### Q2: "Were we supposed to be using /epic-loop?"

**NO** - Wave 2 does NOT use `/epic-loop`:
- `/epic-loop` is for **sequential multi-epic orchestration** (EPIC-CCN-15 through 45)
- Wave 2 is for **parallel Phase 0 analysis** (EPIC-CCN-107 through 115)
- `/epic-loop` is **DEPRECATED** - replaced by `/autonomous-refactor`

**Correct Commands**:
- **For Wave 2**: Direct `bob` invocation with `--chat-mode plan`
- **For Full Epic**: `/epic-orchestrate EPIC-CCN-XXX`

## Tool Configuration Issues

### Wave 2 v3 (Custom Mode) - Tool Bug

**Problem**: `read_file` and `write_to_file` tools failed in SSH/non-interactive mode
**Root Cause**: Bob Shell tool path resolution bug
**Workaround**: Use shell commands (`cat >`, `ls`, `wc -l`)

### Wave 2 v4 (Built-in Mode) - API Key Issue

**Problem**: API key revocation during execution
**Root Cause**: Duplicate API key allocation (not tool bug)
**Solution**: Validated unique API key allocation

## Recommendations

### For Wave 2 Execution
1. Use built-in mode: `--chat-mode plan` (proven in v4)
2. Use shell commands for file I/O (workaround for tool bug)
3. Validate unique API keys before launch
4. Use template: `scripts/wave2/phase0_message_template_shell.txt`

### For Full Epic Workflow
1. Use `/epic-orchestrate` (NOT `/epic-loop`)
2. Sub-agent architecture: Each phase spawns dedicated sub-agent
3. F5 gates: Human verification after each ticket

## Workflow Decision Tree

**Need to analyze multiple epics in parallel?**
→ YES: Use Wave 2 workflow
  - Launch: `python scripts/wave2/launch_wave_v4_safe_budget.py`
  - Mode: `--chat-mode plan`
  - Output: Phase 0 only

**Need to complete single epic end-to-end?**
→ YES: Use `/epic-orchestrate EPIC-CCN-XXX`
  - Phases: 0 through 6
  - Output: Complete implementation

**Need to run multiple epics sequentially?**
→ YES: Use `/autonomous-refactor` (replaces `/epic-loop`)

## References

- Wave 2 v4: `scripts/wave2/_wave2_v4_launch_generated.sh`
- Wave 2 v3: `scripts/wave2/launch_phase0_v3_custom_mode.py`
- Epic Orchestrate: `.bob/commands/epic-orchestrate.md`
- Epic Loop (Deprecated): `.bob/commands/epic-loop.md`
- Shell Workaround: `scripts/wave2/phase0_message_template_shell.txt`