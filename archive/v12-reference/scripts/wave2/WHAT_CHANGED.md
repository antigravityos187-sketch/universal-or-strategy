# What Changed Between v3 and v4

## Structure: IDENTICAL ✅
Both scripts have the same:
- EPICS list (lines 6-16)
- API_ALLOCATION (lines 18-22)
- load_api_key function (lines 24-25)
- create_script function structure
- main() function structure
- Bash script generation logic

## Only 3 Things Changed:

### 1. Message Content (Lines 28-48)
**v3 (OLD - BROKEN)**:
```python
# Hardcoded message in create_script()
cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
Phase 0 Hotspot Analysis for EPIC-CCN-{epic_id}

Target: {method} (CYC {cyc})

MANDATORY STEPS:
1. jCodemunch: get_hotspots, get_blast_radius, get_call_hierarchy
2. write_to_file docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md    ❌ BROKEN
3. read_file docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md        ❌ BROKEN
4. write_to_file docs/brain/EPIC-CCN-{epic_id}/manifest.json     ❌ BROKEN
5. read_file docs/brain/EPIC-CCN-{epic_id}/manifest.json         ❌ BROKEN
6. attempt_completion ONLY after BOTH read_file calls succeed

CRITICAL: Use write_to_file tool, NOT run_shell_command with cat.
EOFMSG
```

**v4 (NEW - FIXED)**:
```python
# Load from external template file
SHELL_TEMPLATE = Path("scripts/wave2/phase0_message_template_shell.txt").read_text(encoding='utf-8')

# Customize template for this epic
message = SHELL_TEMPLATE.replace("EPIC-CCN-X", f"EPIC-CCN-{epic_id}")
message = message.replace("MethodName", method)
message = message.replace("CYC XX", f"CYC {cyc}")

cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
{message}  # Uses shell commands from template ✅
EOFMSG
```

### 2. Added Logging (Line 41, 47)
**v3**: No log directory creation, no log capture
**v4**: 
```bash
mkdir -p logs/phase0  # Line 41
bob --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_id}.txt)" 2>&1 | tee logs/phase0/EPIC-CCN-{epic_id}.log  # Line 47
```

### 3. Added Launcher Script (Lines 66-95)
**v3**: No launcher script
**v4**: Creates `launch_phase0_all.sh` to run all 9 epics in parallel using `screen`

## What Stayed the Same ✅
- EPICS data (107-115)
- API key allocation (immutable)
- Bash script structure
- Custom mode usage (`v12-phase0-hotspot`)
- Directory creation (`mkdir -p docs/brain/EPIC-CCN-{epic_id}`)
- API key export (`export BOBSHELL_API_KEY`)
- Bob invocation (`bob --chat-mode v12-phase0-hotspot`)

## Why This is Safe
1. **Same Python structure** - Only changed message content source
2. **Same bash script logic** - Only changed what message gets embedded
3. **Same API allocation** - No changes to key distribution
4. **Same custom mode** - Still uses `v12-phase0-hotspot`
5. **Added features only** - Logging and launcher are additions, not replacements

## The Critical Fix
**v3 told agents**: "Use write_to_file tool" ❌
**v4 tells agents**: "Use shell commands (cat >, ls, wc -l)" ✅

This is the ONLY functional difference that matters for fixing the tool bug.