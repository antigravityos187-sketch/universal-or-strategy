#!/bin/bash
set -e

cd /home/malhitticrypto/universal-or-strategy

# Set API key
export BOB_API_KEY_FILE="$HOME/.bob/api-keys/bob (6).json"

# Create epic directory
mkdir -p docs/brain/EPIC-CCN-114

# Execute Phase 0 with MANDATORY file write verification
bob --mode advanced \
    --message "Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-114.

**CRITICAL**: You MUST write files to disk and verify they exist.

## Target Method
- Method: FlattenSinglePosition
- File: src/V12_002.cs
- Complexity: 27

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='FlattenSinglePosition')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='FlattenSinglePosition')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='FlattenSinglePosition')

### Step 2: Write 00-hotspots.md
Use write_to_file tool to create docs/brain/EPIC-CCN-114/00-hotspots.md with:
- Method signature and location
- Complexity metrics (cyclomatic, nesting, parameters)
- Blast radius (files affected, importers)
- Call hierarchy (callers and callees)
- Risk assessment (LOW/MEDIUM/HIGH)

### Step 3: Write manifest.json
Use write_to_file tool to create docs/brain/EPIC-CCN-114/manifest.json:
```json
{
  "epic_id": "EPIC-CCN-114",
  "method": "FlattenSinglePosition",
  "file": "src/V12_002.cs",
  "complexity": 27,
  "phases": {
    "0": {
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }
  }
}
```

### Step 4: VERIFY files exist
Use read_file tool to verify BOTH files were created:
1. read_file docs/brain/EPIC-CCN-114/00-hotspots.md
2. read_file docs/brain/EPIC-CCN-114/manifest.json

If either file is missing, CREATE IT AGAIN.

### Step 5: Confirm completion
Only use attempt_completion when BOTH files exist and you've verified them with read_file.

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis
- manifest.json exists and shows phase 0 completed
- Both files verified with read_file tool
- No file creation errors"

echo "DONE_EXIT=$?"
