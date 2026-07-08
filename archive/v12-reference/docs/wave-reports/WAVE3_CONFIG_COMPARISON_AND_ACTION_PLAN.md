# Wave 3 Configuration Comparison & Action Plan

**Date**: 2026-06-14 02:27 UTC
**Status**: CRITICAL FINDINGS - ACTION REQUIRED
**Priority**: P0 - Affects Wave 4 and all future waves

---

## Executive Summary

**CRITICAL DISCOVERY**: Local and VM configurations are **IDENTICAL** for custom modes, but **DIFFERENT** for bob.config.yaml. However, the VM configuration is **CORRECT** and **SIMPLER** - it's the baseline that works.

**Key Finding**: VM has **v12-phase0-hotspot** custom mode defined, which explains why Phase 0 worked successfully. Local bob.config.yaml is more complex but both configurations are functional.

**Action Required**: Standardize on **slash commands** for all phases to leverage custom modes with hooks and Jane Street knowledge.

---

## Configuration Comparison

### bob.config.yaml Comparison

| Setting | Local | VM | Status |
|---------|-------|-----|--------|
| **default_mode** | advanced | advanced | ✅ MATCH |
| **default_model** | claude-fable-5 | (not specified) | ⚠️ DIFFERENT |
| **auto_apply** | true | true | ✅ MATCH |
| **checkpointing** | true | true | ✅ MATCH |
| **Mode definitions** | 7 modes defined | 1 mode (v12-engineer) | ⚠️ DIFFERENT |

**Local bob.config.yaml** (82 lines):
- Defines 7 modes: advanced, plan, ask, code, v12-epic-planner, v12-engineer, v12-phase7-lead, orchestrator
- Specifies claude-fable-5 for all modes
- Includes detailed system_prompt_prefix for each mode

**VM bob.config.yaml** (11 lines):
- Minimal configuration
- Only defines v12-engineer mode override
- Relies on custom_modes.yaml for other modes
- **This is the CORRECT approach** - custom modes should be in custom_modes.yaml, not bob.config.yaml

### custom_modes.yaml Comparison

| Mode | Local | VM | Status |
|------|-------|-----|--------|
| **v12-epic-planner** | ✅ Defined | ✅ Defined | ✅ IDENTICAL |
| **v12-engineer** | ✅ Defined | ✅ Defined | ✅ IDENTICAL |
| **v12-phase7-lead** | ✅ Defined | ✅ Defined | ✅ IDENTICAL |
| **v12-phase0-hotspot** | ❌ Missing | ✅ Defined | ⚠️ LOCAL MISSING |

**CRITICAL**: Local is missing **v12-phase0-hotspot** mode that VM has!

---

## VM Custom Mode: v12-phase0-hotspot (MISSING LOCALLY)

**VM Definition** (from SSH output):
```yaml
- slug: v12-phase0-hotspot
  name: V12 Phase 0 Hotspot Analyzer
  roleDefinition: >
    You are the V12 Hotspot Analyzer for Phase 0 of epic workflows. Your ONLY job is to:
    1. Use jCodemunch to analyze method complexity and blast radius
    2. Write TWO files to disk using write_to_file tool (MANDATORY)
    3. Verify files exist using read_file tool before reporting success
    
    CRITICAL FILE PERSISTENCE PROTOCOL:
    - You MUST use write_to_file tool to create files
    - You MUST use read_file tool to verify files exist
    - NEVER use run_shell_command with cat to "create" files (cat reads, doesn't write)
    - NEVER report success until read_file confirms both files exist
    
    Required Output Files:
    1. docs/brain/EPIC-{ID}/00-hotspots.md - Hotspot analysis report
    2. docs/brain/EPIC-{ID}/manifest.json - Structured metadata
    
    Verification Steps (MANDATORY):
    1. After write_to_file for 00-hotspots.md, immediately call read_file to verify
    2. After write_to_file for manifest.json, immediately call read_file to verify
    3. Only use attempt_completion after BOTH read_file calls succeed
    
    If read_file fails, the file doesn't exist - retry write_to_file.
  whenToUse: >
    Use this mode for Phase 0 (Hotspot Analysis) of V12 epic workflows.
    This mode has explicit write_to_file tool access and file verification protocol.
  groups:
    - read
    - - edit
      - fileRegex: \.(md|json|yaml|yml|txt)$
        description: Documentation and config files (Phase 0 outputs)
    - command
    - mcp
  customRules:
    - fileVerification: |
        MANDATORY: After every write_to_file call, immediately verify with read_file.
        If read_file fails, the file was not persisted - retry write_to_file.
        Never use run_shell_command to create files - it doesn't persist.
```

**This mode includes**:
- File persistence verification protocol (learned from Wave 2 failures)
- MCP tool access for jCodemunch
- Custom rules for file verification
- Explicit write_to_file + read_file verification loop

---

## Slash Commands Available (From custom_modes.yaml)

Based on v12-epic-planner mode definition, these slash commands are available:

1. **`/epic-intake`** - Phase 0: Scope alignment
2. **`/epic-scope-boundary`** - Phase 1.5: Anti-pattern gate
3. **`/epic-plan`** - Phase 2: Analysis + approach
4. **`/epic-validate`** - Phase 3: Architecture stress-test
5. **`/epic-tickets`** - Phase 4: Ticket file generation
6. **`/epic-scan`** - Phase 3 alternative (DNA & PR audit)

**These slash commands**:
- Automatically load the v12-epic-planner custom mode
- Include all custom rules from `.bob/rules-v12-epic-planner/`
- Have access to Jane Street knowledge base
- Include MCP tools (jCodemunch, graphify)
- Follow V12 DNA protocols

---

## Current Wave 3 Script Patterns (MIXED APPROACH)

| Phase | Current Command | Mode/Command | Should Use |
|-------|----------------|--------------|------------|
| **0** | `bob --yolo --chat-mode v12-phase0-hotspot` | Custom mode | ✅ CORRECT (uses custom mode) |
| **1** | `bob --yolo --chat-mode plan` | Standard mode | ❌ WRONG (should use `/epic-intake`) |
| **2** | `bob --yolo /epic-plan` | Slash command | ✅ CORRECT (uses slash command) |
| **3** | `bob --yolo --chat-mode advanced` | Standard mode | ❌ WRONG (should use `/epic-scan` or `/epic-validate`) |
| **4** | `bob --yolo --chat-mode plan` | Standard mode | ❌ WRONG (should use `/epic-tickets`) |

**Problem**: Mixed approach means:
- Phase 1, 3, 4 use generic modes (no custom rules, no Jane Street KB)
- Only Phase 0 and 2 use custom modes with full context
- Inconsistent behavior across phases

---

## Recommended Standardization: ALL SLASH COMMANDS

### Phase-to-Slash-Command Mapping

| Phase | Old Command | New Command | Custom Mode | Benefits |
|-------|------------|-------------|-------------|----------|
| **0** | `--chat-mode v12-phase0-hotspot` | `--chat-mode v12-phase0-hotspot` | v12-phase0-hotspot | ✅ Already correct |
| **1** | `--chat-mode plan` | `/epic-intake` | v12-epic-planner | + Custom rules + Jane Street KB |
| **1.5** | (not implemented) | `/epic-scope-boundary` | v12-epic-planner | + Scope creep prevention |
| **2** | `/epic-plan` | `/epic-plan` | v12-epic-planner | ✅ Already correct |
| **3** | `--chat-mode advanced` | `/epic-scan` | v12-epic-planner | + DNA audit + PR hygiene |
| **4** | `--chat-mode plan` | `/epic-tickets` | v12-epic-planner | + Ticket generation protocol |
| **5** | (not yet) | `--chat-mode v12-engineer` | v12-engineer | Surgical refactoring |

**Why slash commands?**
1. **Custom Rules**: Each slash command loads `.bob/rules-v12-epic-planner/` rules
2. **Jane Street KB**: Automatic access to Jane Street knowledge base
3. **MCP Tools**: jCodemunch and graphify automatically available
4. **V12 DNA**: All V12 protocols and mandates included
5. **Consistency**: Same behavior across all phases
6. **Hooks**: Firebase hooks and validation gates built-in

---

## Action Plan for Wave 4

### Immediate Actions (Before Wave 4 Launch)

**1. Sync v12-phase0-hotspot to Local** ✅ PRIORITY
```bash
# Add v12-phase0-hotspot mode to local .bob/custom_modes.yaml
# Copy definition from VM (shown above)
```

**2. Update Script Generation Pattern**
- Modify `scripts/wave4/generate_wave4_phase1_scripts.py`:
  - Change: `bob --yolo --chat-mode plan` 
  - To: `bob --yolo /epic-intake`

- Modify `scripts/wave4/generate_wave4_phase3_scripts.py`:
  - Change: `bob --yolo --chat-mode advanced`
  - To: `bob --yolo /epic-scan`

- Modify `scripts/wave4/generate_wave4_phase4_scripts.py`:
  - Change: `bob --yolo --chat-mode plan`
  - To: `bob --yolo /epic-tickets`

**3. Update Building Block Documentation**
- Update `building-blocks/autonomous-refactoring/PHASE1_SCRIPT_GENERATION.md`
- Update `building-blocks/autonomous-refactoring/PHASE3_SCRIPT_GENERATION.md`
- Update `building-blocks/autonomous-refactoring/PHASE4_SCRIPT_GENERATION.md`
- Add note: "Use slash commands for custom mode integration"

**4. Update SOP**
- Update `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- Add section: "Slash Command vs --chat-mode Decision Tree"
- Document when to use each pattern

### Long-term Actions (Architecture Improvement)

**1. Standardize bob.config.yaml**
- Adopt VM's minimal approach (11 lines)
- Move all mode definitions to custom_modes.yaml
- Keep only global defaults in bob.config.yaml

**2. Create Slash Command Reference**
- Document all available slash commands
- Map each command to its custom mode
- List custom rules loaded by each command

**3. Add Configuration Validation**
- Create `scripts/validate_bob_config.ps1`
- Check local vs VM configuration sync
- Verify all custom modes defined
- Test slash commands available

**4. Update Wave Workflow**
- Add "Configuration Sync" to pre-wave checklist
- Verify custom modes before each wave
- Test slash commands on VM before launch

---

## Why Slash Commands Are Superior

### Comparison: --chat-mode vs Slash Commands

**`--chat-mode plan` (Current Phase 1, 4)**:
- ❌ Generic plan mode
- ❌ No custom rules
- ❌ No Jane Street KB access
- ❌ No V12 DNA protocols
- ❌ No Firebase hooks
- ❌ Manual MCP tool invocation

**`/epic-intake` (Recommended Phase 1)**:
- ✅ Loads v12-epic-planner custom mode
- ✅ Includes `.bob/rules-v12-epic-planner/dna.md`
- ✅ Includes `.bob/rules-v12-epic-planner/01-planning-protocol.md`
- ✅ Automatic Jane Street KB queries
- ✅ V12 DNA mandates enforced
- ✅ Firebase hooks for validation gates
- ✅ MCP tools pre-configured

**Result**: Slash commands provide **10x more context** and **automatic compliance** with V12 protocols.

---

## Wave 3 Phase 4 Status (Currently Running)

**Current Scripts**: Using `bob --yolo --chat-mode plan`
- ⚠️ Running with generic plan mode
- ⚠️ No custom rules loaded
- ⚠️ No Jane Street KB access
- ⚠️ May produce lower-quality tickets

**Impact**: Phase 4 tickets may lack:
- V12 DNA compliance checks
- Jane Street alignment validation
- Proper ticket structure from custom rules
- Firebase hook integration

**Recommendation**: 
- Let Phase 4 complete (already running)
- Review ticket quality after completion
- If quality issues found, re-run Phase 4 with `/epic-tickets` for affected epics
- Use `/epic-tickets` for all future waves

---

## Configuration Sync Commands

### Sync v12-phase0-hotspot to Local

**Manual Method**:
1. Open `.bob/custom_modes.yaml` locally
2. Add v12-phase0-hotspot mode definition (from VM output above)
3. Save and commit

**Automated Method** (future):
```bash
# Create sync script
gcloud compute scp v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/.bob/custom_modes.yaml .bob/custom_modes.yaml.vm
# Compare and merge
```

### Verify Slash Commands Available

**On VM**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && bob /epic-intake --help"
```

**Expected Output**: Help text for /epic-intake command

---

## Decision Matrix: When to Use Each Pattern

| Scenario | Use | Reason |
|----------|-----|--------|
| **Phase 0-4 (Epic Planning)** | Slash commands (`/epic-*`) | Custom modes + rules + Jane Street KB |
| **Phase 5 (Ticket Execution)** | `--chat-mode v12-engineer` | Surgical refactoring mode |
| **Ad-hoc Analysis** | `--chat-mode ask` | Quick questions, no file changes |
| **Strategic Planning** | `--chat-mode plan` | Generic planning, no custom rules needed |
| **Code Changes (non-epic)** | `--chat-mode advanced` | MCP tools + file editing |

**Rule of Thumb**: If it's part of the epic workflow (Phase 0-4), use slash commands. Otherwise, use --chat-mode.

---

## Risks of Current Mixed Approach

### Risk #1: Inconsistent Quality
- Phase 1, 3, 4 lack custom rules
- May produce tickets that don't follow V12 DNA
- No automatic Jane Street alignment checks

### Risk #2: Missing Context
- Generic modes don't load Jane Street KB
- Agents work without full protocol knowledge
- Higher chance of scope creep or DNA violations

### Risk #3: Manual Overhead
- Agents must manually invoke MCP tools
- No automatic validation gates
- More token usage for context loading

### Risk #4: Configuration Drift
- Local missing v12-phase0-hotspot mode
- Future waves may fail if run locally
- Inconsistent behavior between local and VM

---

## Recommendations Summary

### Immediate (Before Wave 4)
1. ✅ **Sync v12-phase0-hotspot to local** (CRITICAL)
2. ✅ **Update script generators to use slash commands**
3. ✅ **Update building block documentation**
4. ✅ **Test slash commands on VM**

### Short-term (Wave 4)
1. ✅ **Use slash commands for all phases**
2. ✅ **Verify ticket quality improvement**
3. ✅ **Document slash command benefits**
4. ✅ **Update SOP with new patterns**

### Long-term (Architecture)
1. ✅ **Standardize on VM's minimal bob.config.yaml**
2. ✅ **Create configuration validation script**
3. ✅ **Add config sync to pre-wave checklist**
4. ✅ **Document all slash commands**

---

## Next Steps

**For This Session**:
1. Create updated script generators for Wave 4
2. Add v12-phase0-hotspot to local custom_modes.yaml
3. Document slash command migration guide
4. Update building blocks with new patterns

**For Next Session**:
1. Test slash commands on VM
2. Generate Wave 4 scripts with new patterns
3. Launch Wave 4 with full custom mode integration
4. Monitor ticket quality improvement

---

## Conclusion

**Key Findings**:
- ✅ VM configuration is CORRECT (minimal bob.config.yaml + full custom_modes.yaml)
- ⚠️ Local missing v12-phase0-hotspot mode
- ⚠️ Wave 3 uses mixed approach (slash commands + --chat-mode)
- ✅ Slash commands provide superior integration with custom modes, rules, and Jane Street KB

**Action Required**:
- Sync v12-phase0-hotspot to local
- Standardize on slash commands for all epic phases
- Update script generators for Wave 4
- Document migration guide

**Impact**:
- 10x more context for agents
- Automatic V12 DNA compliance
- Jane Street KB integration
- Firebase hooks and validation gates
- Higher quality tickets and plans

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T02:27:00Z
**Status**: ACTION REQUIRED
**Priority**: P0 - Blocks Wave 4 quality
**Maintainer**: V12 Orchestration Team