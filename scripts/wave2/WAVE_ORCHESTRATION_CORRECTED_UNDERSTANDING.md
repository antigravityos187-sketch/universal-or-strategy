# Wave Orchestration - Corrected Understanding

## Critical Clarification

I apologize for the confusion in my previous analysis. Here is the CORRECT understanding:

## Wave Execution Strategy

**Wave 2** is a **pilot wave** to test the orchestration workflow before scaling to all 100+ epics.

### Execution Pattern: Batch-by-Phase

```
Wave 2 (9 epics: EPIC-CCN-107 through 115)
├─ Phase 0: Run ALL 10 epics in parallel (10 agents on VM)
│  └─ Wait for all to complete
├─ Phase 1: Run ALL 10 epics in parallel (10 agents on VM)
│  └─ Wait for all to complete
├─ Phase 2: Run ALL 10 epics in parallel (10 agents on VM)
│  └─ Wait for all to complete
... continue through all 9 phases
```

**NOT** one epic through all phases, then next epic.
**YES** all epics through Phase 0, then all epics through Phase 1, etc.

### Resource Allocation

- **10 API keys** (1 per agent)
- **10 parallel agents** on VM
- **9 phases** (current workflow, will consolidate to 7-8 after Wave 2 data)
- **Batch execution**: All 10 epics complete Phase N before ANY epic starts Phase N+1

### Custom Modes Requirement

**CRITICAL**: Wave 2 MUST use custom modes (e.g., `v12-phase0-hotspot`), NOT built-in modes.

**Why**: 
- Custom modes are configured for the specific phase requirements
- Built-in modes lack the specialized instructions
- Custom modes include shell command workarounds for tool bugs

### Current Status

**Wave 2 v4** used built-in `plan` mode - this was WRONG.
**Wave 2 v3** attempted custom mode `v12-phase0-hotspot` - this was CORRECT approach but hit tool bugs.

## The Tool Configuration Issue

The `.bob/custom_modes.yaml` configuration is **CORRECT**.

The problem is **NOT** the configuration - it's Bob Shell's `read_file` and `write_to_file` tools failing in SSH/non-interactive mode.

**Solution**: Custom mode instructions MUST tell agents to use shell commands instead of Bob tools.

## What Needs to Be Fixed

### 1. Deploy Custom Modes to VM

The `.bob/custom_modes.yaml` file exists locally but needs to be deployed to the VM:

```powershell
# Deploy custom modes
gcloud compute scp .bob/custom_modes.yaml v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/.bob/ --zone=us-central1-a
```

### 2. Update Launch Scripts to Use Custom Modes

Change from:
```bash
bob --chat-mode plan
```

To:
```bash
bob --chat-mode v12-phase0-hotspot
```

### 3. Ensure Shell Command Instructions Are in Custom Mode

The custom mode's `custom_instructions` MUST include the shell command workaround documented in `scripts/wave2/phase0_message_template_shell.txt`.

## Phase Consolidation Plan

**Current**: 9 phases (Phase 0 through Phase 6, with Phase 1.5 and per-ticket phases)

**After Wave 2 Data**: Consolidate to 7-8 phases

**Reference**: `docs/workflow/PHASE_CONSOLIDATION_ANALYSIS.md`

**Proposed Consolidation**:
- Merge Phase 0 + Phase 1 → Phase 0: Intake & Scope
- Keep Phase 1.5 (Scope Boundary) - critical safety gate
- Automate Phase 3 (DNA Audit)
- Conditional per-ticket verification (Phase 5.X.V)

## Obsidian Dashboard

The PowerShell terminal error shows the Obsidian dashboard update script is failing. This is a separate issue from the Wave 2 execution.

**To restart Obsidian sync**:
```powershell
# Navigate to the script directory
cd scripts

# Run the Obsidian sync script
python obsidian_sync.py
```

## Next Steps

1. **Deploy `.bob/custom_modes.yaml` to VM**
2. **Create new launch script** that uses custom modes for all phases
3. **Test Phase 0 with custom mode** (single epic first)
4. **Scale to all 10 epics** once verified
5. **Continue through all 9 phases** (batch-by-phase execution)
6. **Collect performance data** for phase consolidation analysis

## Key Takeaways

- ✅ Wave orchestration = batch-by-phase (NOT epic-by-epic)
- ✅ 10 API keys = 1 per parallel agent
- ✅ Custom modes = REQUIRED (not optional)
- ✅ Shell commands = workaround for Bob tool bugs
- ✅ 9 phases now, consolidate to 7-8 after Wave 2
- ✅ `.bob/custom_modes.yaml` is correct, just needs deployment

## Apology

I apologize for the confusion in my previous responses. I was conflating:
- `/epic-orchestrate` (single epic, all phases, sub-agent architecture)
- Wave execution (multiple epics, batch-by-phase, parallel agents)

These are two different workflows. Wave 2 uses the batch-by-phase approach with parallel agents on a VM.