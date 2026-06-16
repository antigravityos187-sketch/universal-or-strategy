# Building Block: Phase 4 Script Generation

**Purpose**: Generate Phase 4 (Ticket Generation) scripts for any wave by copying the previous wave's Phase 4 pattern.

**Version**: 1.0
**Created**: 2026-06-13
**Last Used**: Wave 3 Phase 4

---

## The Golden Rule

**ALWAYS copy the SAME phase from the PREVIOUS wave, NOT adjacent phases from the current wave.**

For Phase 4:
- ✅ Copy `scripts/wave{N-1}/generate_phase4_scripts.py` → `scripts/wave{N}/generate_wave{N}_phase4_scripts.py`
- ❌ Do NOT copy Phase 3 or Phase 5 from current wave
- ❌ Do NOT create from scratch

---

## Step-by-Step Process

### Step 1: Copy Previous Wave's Phase 4 Generator

```bash
# Example: Wave 3 copies from Wave 2
cp scripts/wave2/generate_phase4_scripts.py scripts/wave3/generate_wave3_phase4_scripts.py
```

### Step 2: Update Epic Numbers ONLY

**Find and Replace** (3 locations):

1. **API_ALLOCATION dictionary** (lines 8-18):
   ```python
   # OLD (Wave 2)
   API_ALLOCATION = {
       "107": "b (2).json",
       "108": "b.json",
       ...
       "115": "bob.json",
   }
   
   # NEW (Wave 3)
   API_ALLOCATION = {
       "116": "b (2).json",
       "117": "b.json",
       ...
       "125": "sean.carter.jr@atomicmail.io.json",  # Add 10th epic if needed
   }
   ```

2. **active_epics list** (line 109):
   ```python
   # OLD
   active_epics = [e for e in ["107", "108", "109", "111", "112", "113", "114", "115"] if e not in SKIP_EPICS]
   
   # NEW
   active_epics = [e for e in ["116", "117", "118", "119", "120", "121", "122", "123", "124", "125"] if e not in SKIP_EPICS]
   ```

3. **epic_num loop** (line 167):
   ```python
   # OLD
   for epic_num in ["107", "108", "109", "110", "111", "112", "113", "114", "115"]:
   
   # NEW
   for epic_num in ["116", "117", "118", "119", "120", "121", "122", "123", "124", "125"]:
   ```

### Step 3: Update API Keys (If Adding 10th Epic)

If your wave has 10 epics (previous had 9), add the 10th API key:

```python
# In API_KEYS dictionary (line 31)
"sean.carter.jr@atomicmail.io.json": "bob_prod_bob-admin_44TtZXuuACpNu133KVpJ7nSGsRr8hhdVUJj3h3jYe5MUk44L1xm6bUAbv5WDab98VadJx53pvp1Kdxmch4E4Qh1H_7J5ULr6U54NC12M2tpGVD6FWjmjk5rgZWcDie42W6mRh",
```

### Step 4: Update SKIP_EPICS (If Applicable)

```python
# Wave 2 had one skip
SKIP_EPICS = ["110"]

# Wave 3 has no skips
SKIP_EPICS = []
```

### Step 5: Update Header Comments

```python
# OLD
"""
Generate Phase 4 (Ticket Generation) scripts for Wave 2
Copies Phase 3 pattern, only changes phase-specific details
"""

# NEW
"""
Generate Phase 4 (Ticket Generation) scripts for Wave 3
Copies Wave 2 Phase 4 pattern, only changes epic numbers
"""
```

### Step 6: Generate Scripts

```bash
python scripts/wave3/generate_wave3_phase4_scripts.py
```

**Expected Output**:
```
============================================================
Phase 4 Script Generator (Ticket Generation)
============================================================

[OK] Validated 10 unique API keys

Generating Phase 4 scripts...
[OK] Created _p4_116.sh (API: b (2).json)
[OK] Created _p4_117.sh (API: b.json)
...
[OK] Created _p4_125.sh (API: sean.carter.jr@atomicmail.io.json)

Generating launcher...
[OK] Created launch_phase4_all_screen.sh

============================================================
Phase 4 Generation Complete!
  Active epics: 10
  Skipped epics: 0
============================================================
```

### Step 7: Move Scripts to Wave Directory

```bash
# Move generated scripts
Move-Item -Path _p4_*.sh -Destination scripts/wave3/ -Force
Move-Item -Path launch_phase4_all_screen.sh -Destination scripts/wave3/ -Force
```

### Step 8: Verify One Script

```bash
# Check first script has correct epic number
head -60 scripts/wave3/_p4_116.sh
```

**Verify**:
- Line 5: `mkdir -p docs/brain/EPIC-CCN-116`
- Line 9: `You are executing Phase 4 (Ticket Generation) for EPIC-CCN-116.`
- Line 12: `- Read docs/brain/EPIC-CCN-116/02-architecture-plan.md`
- Line 50: `bob --yolo --chat-mode plan "$(cat /tmp/phase4_msg_116.txt)"`

---

## What NOT to Change

**DO NOT modify**:
- Bob Shell command structure (`bob --yolo --chat-mode plan`)
- Message file approach (`/tmp/phase4_msg_X.txt`)
- Environment variable name (`BOBSHELL_API_KEY`)
- File verification logic
- Bobcoin reporting format
- Screen session naming pattern
- Log file paths

**ONLY change**:
- Epic numbers (3 locations)
- API allocation (if adding/removing epics)
- SKIP_EPICS list (if applicable)
- Header comments (wave number)

---

## Phase 4 Specifications (Reference)

### Mode & Command

**Mode**: `plan` (strategic planning, no code changes)

**Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase4_msg_X.txt)"`

### Input Artifacts

1. `docs/brain/EPIC-CCN-X/02-architecture-plan.md` (from Phase 2)
2. `docs/brain/EPIC-CCN-X/03-audit-report.md` (from Phase 3)

### Output Artifacts

1. `docs/brain/EPIC-CCN-X/04-tickets.md` with:
   - Ticket breakdown (one ticket per extraction target)
   - Method signatures
   - Extraction steps (numbered, surgical)
   - Test requirements
   - Verification criteria
   - Estimated complexity reduction

2. `docs/brain/EPIC-CCN-X/manifest.json` (updated):
   - Phase "4" status → "completed"
   - "04-tickets.md" added to outputs

### Success Criteria

- ✅ `04-tickets.md` file created (5-15K typical size)
- ✅ Manifest updated with phase 4 completion
- ✅ Bobcoin usage reported (Cost + Balance)
- ✅ All tickets independently executable
- ✅ Target complexity ≤8 per extracted method
- ✅ No scope creep (single-method boundary verified)

---

## Deployment Pattern

### Upload to VM

```bash
gcloud compute scp scripts/wave3/_p4_*.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave3/launch_phase4_all_screen.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a
```

### Launch

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && bash launch_phase4_all_screen.sh"
```

### Monitor

```bash
# Check screen sessions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# Check file creation
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/04-tickets.md 2>/dev/null | wc -l"

# Extract bobcoin usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase4/EPIC-CCN-*.log"
```

---

## Budget Estimates

**Per Epic**: 5-10 bobcoins (ticket generation is lightweight)

**Total Phase 4**: 50-100 bobcoins (for 10 epics)

**Typical File Size**: 5K-15K per `04-tickets.md`

---

## Common Mistakes (Avoid These)

### ❌ Mistake 1: Copying Wrong Phase

**Wrong**: Copying Phase 3 or Phase 5 from current wave
**Right**: Copying Phase 4 from previous wave

**Why**: Each phase has different execution mode, command structure, and output format.

### ❌ Mistake 2: Incomplete Epic Number Updates

**Wrong**: Updating only 1 or 2 of the 3 locations
**Right**: Update all 3 locations (API_ALLOCATION, active_epics, epic_num loop)

**Why**: Partial updates cause KeyError or wrong epic numbers in scripts.

### ❌ Mistake 3: Modifying Command Structure

**Wrong**: Changing `bob --yolo --chat-mode plan` to `bob /epic-tickets`
**Right**: Keep exact command structure from previous wave

**Why**: Command structure is phase-specific and tested. Changes cause execution failures.

### ❌ Mistake 4: Forgetting 10th API Key

**Wrong**: Adding 10th epic without adding 10th API key
**Right**: Add API key to both API_ALLOCATION and API_KEYS dictionaries

**Why**: Script validates unique API keys - missing key causes validation failure.

---

## Validation Checklist

Before deployment:

- [ ] Generator script copied from previous wave's Phase 4
- [ ] Epic numbers updated in all 3 locations
- [ ] API_ALLOCATION has correct number of epics
- [ ] API_KEYS has all required keys (including 10th if applicable)
- [ ] SKIP_EPICS updated (empty if no skips)
- [ ] Header comments updated with wave number
- [ ] Scripts generated successfully (10 individual + 1 launcher)
- [ ] Scripts moved to wave directory
- [ ] One script verified (correct epic numbers in all references)
- [ ] Command structure unchanged (`bob --yolo --chat-mode plan`)

---

## Wave History

### Wave 2 Phase 4 (Baseline)

- **Epics**: CCN-107 through CCN-115 (9 epics, 1 skip)
- **SKIP_EPICS**: ["110"] (closed as compliant)
- **API Keys**: 9 unique keys
- **Status**: ✅ Complete

### Wave 3 Phase 4 (Current)

- **Epics**: CCN-116 through CCN-125 (10 epics, no skips)
- **SKIP_EPICS**: [] (all active)
- **API Keys**: 10 unique keys (added sean.carter.jr@atomicmail.io.json)
- **Status**: ✅ Scripts generated, ready for deployment

### Wave 4 Phase 4 (Future)

- **Epics**: CCN-126 through CCN-135 (projected)
- **Pattern**: Copy Wave 3 Phase 4, update epic numbers only
- **API Keys**: Reuse same 10 keys (reset balances if needed)

---

## Related Building Blocks

- **PHASE0_SCRIPT_GENERATION.md** - Hotspot analysis pattern
- **PHASE1_SCRIPT_GENERATION.md** - Scope definition pattern
- **PHASE2_SCRIPT_GENERATION.md** - Architecture planning pattern
- **PHASE3_SCRIPT_GENERATION.md** - DNA & PR audit pattern
- **WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md** - Master SOP for all phases

---

## Version History

- **V1.0** (2026-06-13): Initial building block created from Wave 3 Phase 4 success
  - Documented copy-from-previous-wave pattern
  - Added 3-location epic number update checklist
  - Included 10th API key addition procedure
  - Validated with Wave 3 Phase 4 generation

---

**Last Updated**: 2026-06-13T19:09:00-07:00
**Maintainer**: V12 Orchestration Team
**Status**: VALIDATED (Wave 3 Phase 4)