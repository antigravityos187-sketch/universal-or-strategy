# Wave 7 Template Verification

**Date**: 2026-06-19  
**Status**: Verified  
**Source**: `building-blocks/autonomous-refactoring/` (Wave 5 templates)

## Template Inventory

All 9 phase templates are available and verified:

| Phase | Template File | Status | Notes |
|-------|---------------|--------|-------|
| 0 | `phase0_template_v12_52.sh` | ✅ Verified | Hotspot Analysis |
| 1 | `phase1_template_v12_52.sh` | ✅ Verified | Scope Definition |
| 1.5 | `phase1_5_template_v12_52_FIXED.sh` | ✅ Verified | Scope Boundary (FIXED version with temp file pattern) |
| 2 | `phase2_template_v12_52.sh` | ✅ Verified | Architecture Planning |
| 3 | `phase3_template_v12_52.sh` | ✅ Verified | DNA Audit |
| 4 | `phase4_template_v12_52.sh` | ✅ Verified | Ticket Generation |
| 5 | `phase5_template_v12_52.sh` | ✅ Verified | Ticket Execution |
| 5.V | `phase5_v_template_v12_52.sh` | ✅ Verified | Verification |
| 6 | `phase6_template_v12_52.sh` | ✅ Verified | Final Review |

## Critical Verification Points

### ✅ Bob CLI Pattern
All templates MUST use the two-step temp file pattern:
```bash
cat > /tmp/phaseX_msg_$EPIC_ID.txt << 'EOFMSG'
[message content]
EOFMSG

~/.npm-global/bin/bob --yolo --chat-mode MODE "$(cat /tmp/phaseX_msg_$EPIC_ID.txt)"
```

**Status**: Phase 1.5 FIXED template verified to use this pattern.

### ✅ Mode Selection
Each phase uses the correct Bob mode:
- Phase 0: `v12-phase0-hotspot`
- Phase 1: `v12-phase1-scope`
- Phase 1.5: `v12-phase1-5-boundary`
- Phase 2: `v12-phase2-architecture`
- Phase 3: `v12-phase3-audit`
- Phase 4: `v12-phase4-tickets`
- Phase 5: `v12-engineer`
- Phase 5.V: `v12-phase5-v-verify`
- Phase 6: `v12-phase6-review`

### ✅ Lamport Event Logging
All templates include Lamport event logging for:
- `phase_start`
- `phase_complete`
- `phase_failed`

### ✅ UTF-8 Encoding Checks
Phase 5+ templates include UTF-8 encoding verification.

### ✅ xUnit Test Framework
Phase 5 template specifies xUnit test generation (NEVER NUnit/MSTest).

## Building-Blocks Method Compliance

### Golden Rule
**ALWAYS copy from previous wave's SAME phase**

### Wave 7 Script Generation Process

1. **Copy Template**:
   ```bash
   cp building-blocks/autonomous-refactoring/phase0_template_v12_52.sh \
      scripts/wave7/phase0_epic_001.sh
   ```

2. **Update ONLY Epic-Specific Data**:
   - Epic ID: `EPIC-CCN-XXX`
   - Method name: From roadmap
   - File path: From roadmap
   - Complexity score: From roadmap

3. **DO NOT Modify**:
   - Bob CLI invocation pattern
   - Mode selection
   - Lamport event logging
   - Output file paths
   - Success criteria

4. **Verify**:
   - Temp file pattern present
   - Correct mode used
   - Epic data updated
   - No syntax errors

## Script Generation Tool

Use `generate_wave7_scripts.py` to generate all 161 × 9 = 1,449 phase scripts:

```bash
python scripts/wave7/generate_wave7_scripts.py
```

This will:
1. Read `epic_roadmap_wave7.json`
2. For each epic (1-161):
   - For each phase (0, 1, 1.5, 2, 3, 4, 5, 5.V, 6):
     - Copy template from `building-blocks/autonomous-refactoring/`
     - Update epic-specific data
     - Write to `scripts/wave7/phaseX_epic_YYY.sh`
3. Verify all scripts generated
4. Report any errors

## Verification Checklist

Before executing any phase:

- [ ] All templates copied from Wave 5 building-blocks
- [ ] Epic-specific data updated (ID, method, file, CYC)
- [ ] Bob CLI temp file pattern present
- [ ] Correct mode specified
- [ ] Lamport event logging included
- [ ] UTF-8 encoding check (Phase 5+)
- [ ] xUnit test specification (Phase 5)
- [ ] No syntax errors
- [ ] Line endings correct (LF for VM, CRLF for local)

## Template Locations

### Source (Building Blocks)
```
building-blocks/autonomous-refactoring/
├── phase0_template_v12_52.sh
├── phase1_template_v12_52.sh
├── phase1_5_template_v12_52_FIXED.sh  ← Use FIXED version
├── phase2_template_v12_52.sh
├── phase3_template_v12_52.sh
├── phase4_template_v12_52.sh
├── phase5_template_v12_52.sh
├── phase5_v_template_v12_52.sh
└── phase6_template_v12_52.sh
```

### Destination (Wave 7 Scripts)
```
scripts/wave7/
├── phase0_epic_001.sh
├── phase0_epic_002.sh
├── ...
├── phase0_epic_161.sh
├── phase1_epic_001.sh
├── ...
└── phase6_epic_161.sh
```

## Next Steps

1. **Generate Scripts**: Run `generate_wave7_scripts.py`
2. **Verify Sample**: Manually verify 3 pilot epic scripts
3. **Test Pilot**: Execute Phase 0 for 3 pilot epics
4. **Full Execution**: Generate and execute all remaining scripts

## References

- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Templates**: `building-blocks/autonomous-refactoring/`
- **Roadmap**: `epic_roadmap_wave7.json`
- **Instructions**: `docs/workflow/AUTONOMOUS_REFACTOR_MODE_INSTRUCTIONS.md`