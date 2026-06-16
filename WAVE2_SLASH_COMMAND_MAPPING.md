# Wave 2 Slash Command Mapping

**Date**: 2026-06-13
**Issue**: Phase scripts using `--chat-mode` instead of slash commands
**Impact**: JSON file write restrictions causing failures

---

## Current State vs Correct State

| Phase | Current Command | Correct Command | Slash Command Exists? |
|-------|----------------|-----------------|----------------------|
| **Phase 0** | `--chat-mode v12-phase0-hotspot` | ✅ CORRECT (custom mode) | N/A (uses custom mode) |
| **Phase 1** | `--chat-mode plan` | `/epic-intake` | ✅ YES (`.bob/commands/epic-intake.md`) |
| **Phase 1.5** | `--chat-mode plan` | `/epic-scope-boundary` | ✅ YES (`.bob/commands/epic-scope-boundary.md`) |
| **Phase 2** | `--chat-mode plan` | `/epic-plan` | ✅ YES (`.bob/commands/epic-plan.md`) |
| **Phase 3** | TBD | `/epic-scan` | ✅ YES (`.bob/commands/epic-scan.md`) |
| **Phase 4** | TBD | `/epic-tickets` | ✅ YES (`.bob/commands/epic-tickets.md`) |
| **Phase 5** | TBD | `/epic-validate` | ✅ YES (`.bob/commands/epic-validate.md`) |
| **Phase 5.V** | TBD | `/epic-verify-ticket` | ✅ YES (`.bob/commands/epic-verify-ticket.md`) |
| **Phase 6** | TBD | `/epic-review-final` | ✅ YES (`.bob/commands/epic-review-final.md`) |

---

## Why Slash Commands?

### Problem with `--chat-mode`
- **Restriction**: Plan mode only allows markdown files (`.md`)
- **Failure**: Cannot write JSON files like `manifest.json`
- **Result**: 8/9 Phase 2 epics crashed trying to update manifest

### Solution with Slash Commands
- **Flexibility**: Slash commands handle their own mode switching
- **JSON Support**: Can write both markdown and JSON files
- **Built-in Logic**: All phase logic already implemented in command files
- **Manifest Updates**: Commands handle manifest updates internally

---

## Phase 0: Custom Mode (Already Correct)

**Current**: `--chat-mode v12-phase0-hotspot`
**Status**: ✅ CORRECT

**Why**: Phase 0 uses a custom mode defined in `.bob/custom_modes.yaml`:
```yaml
v12-phase0-hotspot:
  name: "V12 Phase 0 Hotspot Analyzer"
  rules_path: ".bob/rules-v12-phase0-hotspot/"
  file_restrictions: "\.md$"
```

**Note**: Custom mode also has markdown-only restriction, but Phase 0 only creates markdown files, so no issue.

---

## Phase 1: Scope Definition

**Current**: `--chat-mode plan`
**Correct**: `/epic-intake EPIC-CCN-X`

**Command File**: `.bob/commands/epic-intake.md`

**What It Does**:
- Reads `00-hotspots.md`
- Creates `00-scope.md`
- Updates `manifest.json` (Phase 1 status)

**Script Change**:
```bash
# OLD
bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_107.txt)"

# NEW
bob --yolo /epic-intake EPIC-CCN-107
```

---

## Phase 1.5: Scope Boundary Validation

**Current**: `--chat-mode plan`
**Correct**: `/epic-scope-boundary EPIC-CCN-X --phase 1.5`

**Command File**: `.bob/commands/epic-scope-boundary.md`

**What It Does**:
- Reads `00-scope.md`
- Creates `01-scope-boundary.md`
- Updates `manifest.json` (Phase 1.5 status)
- Validates single-method boundary (V12.23 Protocol)

**Script Change**:
```bash
# OLD
bob --yolo --chat-mode plan "$(cat /tmp/phase1_5_msg_107.txt)"

# NEW
bob --yolo /epic-scope-boundary EPIC-CCN-107 --phase 1.5
```

---

## Phase 2: Architecture Planning

**Current**: `--chat-mode plan`
**Correct**: `/epic-plan EPIC-CCN-X`

**Command File**: `.bob/commands/epic-plan.md`

**What It Does**:
- Reads `01-scope-boundary.md`
- Creates `02-analysis.md`
- Creates `02-approach.md` or `02-architecture.md`
- Updates `manifest.json` (Phase 2 status)
- Runs jCodemunch analysis (blast radius, dependencies)

**Script Change**:
```bash
# OLD
bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_107.txt)"

# NEW
bob --yolo /epic-plan EPIC-CCN-107
```

---

## Phase 3: DNA & PR Audit

**Current**: TBD (not yet deployed)
**Correct**: `/epic-scan EPIC-CCN-X`

**Command File**: `.bob/commands/epic-scan.md`

**What It Does**:
- Reads `02-architecture-plan.md`
- Runs V12 DNA compliance checks
- Runs PR hygiene validation
- Creates `03-audit-report.md`
- Updates `manifest.json` (Phase 3 status)

**Script Template**:
```bash
bob --yolo /epic-scan EPIC-CCN-107
```

---

## Phase 4: Ticket Generation

**Current**: TBD (not yet deployed)
**Correct**: `/epic-tickets EPIC-CCN-X`

**Command File**: `.bob/commands/epic-tickets.md`

**What It Does**:
- Reads `02-architecture-plan.md`
- Generates surgical extraction tickets
- Creates `04-tickets.md`
- Updates `manifest.json` (Phase 4 status)

**Script Template**:
```bash
bob --yolo /epic-tickets EPIC-CCN-107
```

---

## Phase 5: Ticket Execution

**Current**: TBD (not yet deployed)
**Correct**: `/epic-validate EPIC-CCN-X --ticket N`

**Command File**: `.bob/commands/epic-validate.md`

**What It Does**:
- Reads `04-tickets.md`
- Executes specific ticket (surgical extraction)
- Creates `ticket-N-completion.md`
- Updates `manifest.json` (Phase 5.N status)

**Script Template**:
```bash
bob --yolo /epic-validate EPIC-CCN-107 --ticket 1
```

---

## Phase 5.V: Ticket Verification

**Current**: TBD (not yet deployed)
**Correct**: `/epic-verify-ticket EPIC-CCN-X --ticket N`

**Command File**: `.bob/commands/epic-verify-ticket.md`

**What It Does**:
- Reads `ticket-N-completion.md`
- Verifies complexity targets met
- Runs quality gates
- Creates `ticket-N-verification.md`
- Updates `manifest.json` (Phase 5.N.V status)

**Script Template**:
```bash
bob --yolo /epic-verify-ticket EPIC-CCN-107 --ticket 1
```

---

## Phase 6: Final Review

**Current**: TBD (not yet deployed)
**Correct**: `/epic-review-final EPIC-CCN-X`

**Command File**: `.bob/commands/epic-review-final.md`

**What It Does**:
- Reads all verification reports
- Generates completion report
- Updates roadmap with final status
- Creates `05-completion-report.md`
- Updates `manifest.json` (Phase 6 status)

**Script Template**:
```bash
bob --yolo /epic-review-final EPIC-CCN-107
```

---

## Implementation Plan

### Step 1: Update Script Generators

**Files to Modify**:
1. `scripts/wave2/generate_phase1_scripts.py` - Change to `/epic-intake`
2. `scripts/wave2/generate_phase1_5_scripts.py` - Change to `/epic-scope-boundary`
3. `scripts/wave2/generate_phase2_scripts.py` - Change to `/epic-plan`
4. Create `scripts/wave2/generate_phase3_scripts.py` - Use `/epic-scan`
5. Create `scripts/wave2/generate_phase4_scripts.py` - Use `/epic-tickets`
6. Create `scripts/wave2/generate_phase5_scripts.py` - Use `/epic-validate`
7. Create `scripts/wave2/generate_phase5_v_scripts.py` - Use `/epic-verify-ticket`
8. Create `scripts/wave2/generate_phase6_scripts.py` - Use `/epic-review-final`

### Step 2: Regenerate All Scripts

```bash
python scripts/wave2/generate_phase1_scripts.py
python scripts/wave2/generate_phase1_5_scripts.py
python scripts/wave2/generate_phase2_scripts.py
# Future phases when needed
```

### Step 3: Deploy to VM

```bash
gcloud compute scp _p*.sh launch_phase*_all_screen.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
```

### Step 4: Rerun Failed Phases

```bash
# Phase 1 (if needed)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && chmod +x _p1_*.sh launch_phase1_all_screen.sh && bash launch_phase1_all_screen.sh"

# Phase 1.5 (if needed)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && chmod +x _p1_5_*.sh launch_phase1_5_all_screen.sh && bash launch_phase1_5_all_screen.sh"

# Phase 2 (definitely needed)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && chmod +x _p2_*.sh launch_phase2_all_screen.sh && bash launch_phase2_all_screen.sh"
```

---

## Benefits of Slash Commands

1. **No Mode Restrictions**: Commands handle mode switching internally
2. **JSON Support**: Can write manifest.json without errors
3. **Built-in Logic**: All phase logic already implemented
4. **Consistent Interface**: Same pattern across all phases
5. **Easier Debugging**: Command files are self-documenting
6. **Future-Proof**: New phases just need new command files

---

## Verification Checklist

After regenerating scripts:

- [ ] Phase 0: Uses `--chat-mode v12-phase0-hotspot` (custom mode)
- [ ] Phase 1: Uses `/epic-intake EPIC-CCN-X`
- [ ] Phase 1.5: Uses `/epic-scope-boundary EPIC-CCN-X --phase 1.5`
- [ ] Phase 2: Uses `/epic-plan EPIC-CCN-X`
- [ ] Phase 3: Uses `/epic-scan EPIC-CCN-X`
- [ ] Phase 4: Uses `/epic-tickets EPIC-CCN-X`
- [ ] Phase 5: Uses `/epic-validate EPIC-CCN-X --ticket N`
- [ ] Phase 5.V: Uses `/epic-verify-ticket EPIC-CCN-X --ticket N`
- [ ] Phase 6: Uses `/epic-review-final EPIC-CCN-X`

---

## Reference

- **Slash Commands**: `.bob/commands/epic-*.md`
- **Custom Modes**: `.bob/custom_modes.yaml`
- **Mode Rules**: `.bob/rules-*/`
- **Script Generators**: `scripts/wave2/generate_phase*_scripts.py`

---

**Status**: Phase 0 ✅ | Phase 1 ❌ | Phase 1.5 ❌ | Phase 2 ❌ | Phase 3-6 ⏳ Pending
**Action Required**: Regenerate Phase 1, 1.5, 2 scripts with slash commands