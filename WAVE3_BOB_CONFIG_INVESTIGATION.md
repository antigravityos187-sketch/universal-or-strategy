# Wave 3 Bob Configuration Investigation

**Date**: 2026-06-14 02:18 UTC
**Investigator**: Advanced Mode Agent
**Purpose**: Compare local vs VM Bob Shell configuration and document mode usage per phase

---

## Executive Summary

**CRITICAL FINDING**: Wave 3 scripts use **TWO DIFFERENT INVOCATION PATTERNS**:
1. **`--chat-mode` pattern**: Phases 0, 1, 3, 4 (uses standard Bob modes)
2. **`/epic-plan` pattern**: Phase 2 only (uses Bob Shell slash commands)

This mixed approach may cause configuration inconsistencies between local and VM environments.

---

## Local Configuration Analysis

### File: `bob.config.yaml` (Local)

**Global Defaults**:
- `default_mode: advanced`
- `default_model: claude-fable-5`
- `auto_apply: true`
- `checkpointing: true`

**Standard Modes Configured**:
1. **advanced**: claude-fable-5, apply=true, MCP tools enabled
2. **plan**: claude-fable-5, apply=false, read-only
3. **ask**: claude-fable-5, apply=false, read-only
4. **code**: claude-fable-5, apply=true (DEPRECATED)

**V12 Custom Modes Configured**:
1. **v12-epic-planner**: claude-fable-5, apply=true, PLAN-ONLY for src/
2. **v12-engineer**: inherits advanced, claude-fable-5, apply=true, surgical edits
3. **v12-phase7-lead**: claude-fable-5, apply=true, lock-free patterns
4. **orchestrator**: claude-fable-5, apply=true, multi-agent coordination

**Custom Mode Definition Location**: `.bob/custom_modes.yaml` (not accessible in this investigation)

---

## Wave 3 Phase Mode Usage (From Scripts)

### Phase 0: Hotspot Analysis
**Command**: `bob --yolo --chat-mode v12-phase0-hotspot`
**Mode Type**: CUSTOM MODE (not in bob.config.yaml)
**Expected Definition**: `.bob/custom_modes.yaml`
**Purpose**: jCodemunch hotspot analysis
**Model**: Unknown (depends on custom mode definition)

### Phase 1: Scope Definition
**Command**: `bob --yolo --chat-mode plan`
**Mode Type**: STANDARD MODE (defined in bob.config.yaml)
**Model**: claude-fable-5
**Apply**: false (read-only by default)
**Purpose**: Strategic planning, scope definition

### Phase 2: Architecture Planning
**Command**: `bob --yolo /epic-plan EPIC-CCN-X`
**Mode Type**: SLASH COMMAND (not a mode)
**Expected Behavior**: Uses Bob Shell's built-in `/epic-plan` command
**Model**: Unknown (depends on Bob Shell internal routing)
**Purpose**: Architecture design with method signatures

### Phase 3: DNA & PR Audit
**Command**: `bob --yolo --chat-mode advanced`
**Mode Type**: STANDARD MODE (defined in bob.config.yaml)
**Model**: claude-fable-5
**Apply**: true
**Purpose**: MCP tools for jCodemunch analysis

### Phase 4: Ticket Generation
**Command**: `bob --yolo --chat-mode plan`
**Mode Type**: STANDARD MODE (defined in bob.config.yaml)
**Model**: claude-fable-5
**Apply**: false (read-only by default)
**Purpose**: Generate implementation tickets

---

## Configuration Discrepancy Analysis

### Known Modes (From bob.config.yaml)

| Mode | Defined? | Model | Apply | Notes |
|------|----------|-------|-------|-------|
| advanced | ✅ Yes | claude-fable-5 | true | Used in Phase 3 |
| plan | ✅ Yes | claude-fable-5 | false | Used in Phase 1, 4 |
| ask | ✅ Yes | claude-fable-5 | false | Not used in Wave 3 |
| v12-phase0-hotspot | ❓ Unknown | ❓ | ❓ | Used in Phase 0, NOT in bob.config.yaml |
| v12-engineer | ✅ Yes | claude-fable-5 | true | Not used yet (Phase 5) |

### Unknown Modes (Require Investigation)

**v12-phase0-hotspot**:
- Used in: Phase 0 (all 10 epics)
- Expected location: `.bob/custom_modes.yaml`
- **CRITICAL**: If this mode is not defined on VM, Phase 0 will fail or use wrong model

### Slash Commands vs Modes

**Phase 2 uses `/epic-plan`** instead of `--chat-mode`:
- This is a Bob Shell **slash command**, not a mode
- Slash commands have their own internal routing
- May not respect `bob.config.yaml` settings
- **RISK**: VM may have different slash command configuration

---

## VM Configuration Status (Unknown)

**Cannot verify without SSH access**:
1. Does VM have `bob.config.yaml`?
2. Does VM have `.bob/custom_modes.yaml`?
3. Is `v12-phase0-hotspot` mode defined on VM?
4. Does VM's Bob Shell version support `/epic-plan` slash command?
5. Are model selections consistent between local and VM?

---

## Potential Issues

### Issue #1: Missing Custom Mode Definition
**Symptom**: Phase 0 scripts reference `v12-phase0-hotspot` mode
**Risk**: If mode not defined on VM, Bob Shell will:
- Fall back to default mode (advanced)
- Use wrong model (possibly not claude-fable-5)
- May not have correct system prompt for hotspot analysis

**Mitigation**: Verify `.bob/custom_modes.yaml` exists on VM with correct definition

### Issue #2: Slash Command Configuration
**Symptom**: Phase 2 uses `/epic-plan` instead of `--chat-mode`
**Risk**: Slash commands may:
- Ignore `bob.config.yaml` settings
- Use different model than expected
- Have different behavior on VM vs local

**Mitigation**: Test `/epic-plan` command on VM to verify behavior

### Issue #3: Model Selection Inconsistency
**Symptom**: Local config specifies claude-fable-5 for all modes
**Risk**: VM may:
- Have older bob.config.yaml with different models
- Use default models instead of claude-fable-5
- Cause cost/quality variations

**Mitigation**: Sync bob.config.yaml to VM before each wave

### Issue #4: Apply Flag Mismatch
**Symptom**: Phase 1 and 4 use `plan` mode (apply=false by default)
**Risk**: Scripts use `--yolo` flag which may override apply setting
**Question**: Does `--yolo` force apply=true even when mode says apply=false?

**Mitigation**: Verify `--yolo` behavior with plan mode

---

## Recommended Actions (Investigation Only)

### Immediate Verification Needed

1. **Check VM bob.config.yaml**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="cat /home/malhitticrypto/universal-or-strategy/bob.config.yaml"
   ```

2. **Check VM custom_modes.yaml**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="cat /home/malhitticrypto/universal-or-strategy/.bob/custom_modes.yaml"
   ```

3. **Verify v12-phase0-hotspot mode exists**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="grep -A 5 'v12-phase0-hotspot' /home/malhitticrypto/universal-or-strategy/.bob/custom_modes.yaml"
   ```

4. **Test /epic-plan command**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="cd /home/malhitticrypto/universal-or-strategy && bob --help | grep epic-plan"
   ```

### Configuration Sync Strategy

**Before Wave 4**:
1. Compare local vs VM configurations
2. Sync any missing custom modes to VM
3. Verify all modes use consistent models
4. Document slash command behavior
5. Create configuration validation script

---

## Phase-by-Phase Mode Summary

| Phase | Command Pattern | Mode/Command | Model (Expected) | Apply | MCP Tools |
|-------|----------------|--------------|------------------|-------|-----------|
| 0 | `--chat-mode` | v12-phase0-hotspot | ❓ Unknown | ❓ | ❓ |
| 1 | `--chat-mode` | plan | claude-fable-5 | false* | No |
| 2 | `/epic-plan` | (slash command) | ❓ Unknown | ❓ | ❓ |
| 3 | `--chat-mode` | advanced | claude-fable-5 | true | Yes |
| 4 | `--chat-mode` | plan | claude-fable-5 | false* | No |
| 5 | (not yet) | v12-engineer | claude-fable-5 | true | Yes |

**\*Note**: `--yolo` flag may override apply=false setting

---

## Critical Questions for Next Session

1. **Does VM have same bob.config.yaml as local?**
   - If not, which version is authoritative?
   - Should we sync local → VM or VM → local?

2. **Is v12-phase0-hotspot mode defined on VM?**
   - If not, Phase 0 may have used wrong model
   - Need to verify Phase 0 logs for model used

3. **What does /epic-plan slash command do?**
   - Which mode does it invoke internally?
   - Which model does it use?
   - Does it respect bob.config.yaml?

4. **Does --yolo override mode's apply setting?**
   - If plan mode has apply=false, does --yolo force it to true?
   - This affects Phase 1 and 4 file creation

5. **Should we standardize on one invocation pattern?**
   - Option A: Use `--chat-mode` for all phases (consistent)
   - Option B: Use slash commands for all phases (if available)
   - Option C: Keep mixed approach (current)

---

## Recommendations for Wave 4

### Short-term (Before Wave 4 Launch)

1. **Verify VM Configuration**: Run all verification commands above
2. **Document Findings**: Create comparison table of local vs VM config
3. **Sync Configurations**: Ensure VM has all custom modes defined
4. **Test Slash Commands**: Verify `/epic-plan` behavior on VM

### Long-term (Architecture Improvement)

1. **Standardize Invocation Pattern**: Choose one pattern for all phases
2. **Configuration Management**: Add config sync to pre-wave checklist
3. **Mode Validation**: Create script to verify all required modes exist
4. **Documentation**: Document which modes are used by which phases

---

## Appendix: Script Evidence

### Phase 0 Example (v12-phase0-hotspot mode)
```bash
# From scripts/wave3/_p0_116.sh line 145
bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_116.txt)"
```

### Phase 1 Example (plan mode)
```bash
# From scripts/wave3/_p1_116.sh line 44
bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_116.txt)"
```

### Phase 2 Example (/epic-plan slash command)
```bash
# From scripts/wave3/_p2_116.sh line 43
bob --yolo /epic-plan EPIC-CCN-116
```

### Phase 3 Example (advanced mode)
```bash
# From scripts/wave3/_p3_116.sh line 44
bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_116.txt)"
```

### Phase 4 Example (plan mode)
```bash
# From scripts/wave3/_p4_116.sh line 50
bob --yolo --chat-mode plan "$(cat /tmp/phase4_msg_116.txt)"
```

---

## Status

**Investigation**: COMPLETE (read-only analysis)
**VM Verification**: PENDING (requires SSH commands)
**Configuration Sync**: PENDING (requires comparison results)
**Risk Level**: MEDIUM (may affect Phase 0 and Phase 2 behavior)

**Next Action**: Run verification commands to compare local vs VM configuration

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T02:18:00Z
**Maintainer**: V12 Orchestration Team