# Phase 0 Template - Building-Blocks Method

**Source**: EPIC-W7-001 (Successfully completed)
**Purpose**: Template for future Phase 0 executions

## Script Pattern (_p0_XXX.sh)

```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy

# API Key (rotate from working keys)
export BOBSHELL_API_KEY='bob_prod_bob-admin_...'

# Create directories (Python does this now, but keep for compatibility)
mkdir -p docs/brain/EPIC-W7-XXX
mkdir -p logs/phase0

# Create Bob message using temp file + command substitution pattern
cat > /tmp/phase0_msg_XXX.txt << 'EOFMSG'
# Phase 0: Hotspot Analysis - EPIC-W7-XXX

Analyze this method for Wave 7 complexity reduction:

**Method**: [METHOD_NAME]
**File**: `[FILE_PATH]`
**Line**: [LINE_NUMBER]
**Cyclomatic Complexity**: [COMPLEXITY]

## Your Task

1. Use jCodemunch MCP tools to analyze the method
2. Assess complexity hotspots and refactoring opportunities
3. Create `docs/brain/EPIC-W7-XXX/00-hotspots.md` with:
   - Method signature and location
   - Complexity metrics (CYC, nesting, parameters)
   - Hotspot analysis (what makes it complex)
   - Blast radius assessment
   - Churn analysis (git history)
   - Risk assessment
   - Refactoring recommendations

4. Create `docs/brain/EPIC-W7-XXX/manifest.json` with:
   - Epic metadata
   - Phase 0 status
   - Timestamp

## Success Criteria
- Both files created
- Analysis complete
- Ready for Phase 1 (Scope Definition)
EOFMSG

# Invoke Bob with command substitution (NEVER inline strings)
~/.npm-global/bin/bob --yolo --chat-mode ask "$(cat /tmp/phase0_msg_XXX.txt)" \
  > logs/phase0/EPIC-W7-XXX.log 2>&1

echo "Phase 0 complete for EPIC-W7-XXX"
```

## Output Files

### 00-hotspots.md Structure

```markdown
# Phase 0: Hotspot Analysis - EPIC-W7-XXX

**Epic ID**: EPIC-W7-XXX
**Method**: [METHOD_NAME]
**File**: [FILE_PATH]
**Line**: [LINE_NUMBER]
**Complexity**: [CYC_VALUE]

## Method Signature
[Full signature with parameters]

## Complexity Metrics
- **Cyclomatic Complexity**: [VALUE]
- **Max Nesting Depth**: [VALUE]
- **Parameter Count**: [VALUE]
- **Lines of Code**: [VALUE]

## Hotspot Analysis
[What makes this method complex - branches, loops, nested logic]

## Blast Radius
- **Direct Callers**: [COUNT] files
- **Transitive Impact**: [DESCRIPTION]
- **Risk Level**: [LOW/MEDIUM/HIGH]

## Churn Analysis
- **Commits (90 days)**: [COUNT]
- **Authors**: [COUNT]
- **Churn Rate**: [VALUE] commits/week
- **Stability**: [STABLE/ACTIVE/VOLATILE]

## Risk Assessment
- **Refactoring Risk**: [ASSESSMENT]
- **Regression Risk**: [ASSESSMENT]
- **Priority**: [P0/P1/P2/P3/P4]

## Refactoring Recommendations
[Specific strategies for reducing complexity]

---
**Generated**: [TIMESTAMP]
**Wave**: 7
**Phase**: 0 (Hotspot Analysis)
```

### manifest.json Structure

```json
{
  "epic_id": "EPIC-W7-XXX",
  "method_name": "[METHOD_NAME]",
  "file": "[FILE_PATH]",
  "line": [LINE_NUMBER],
  "complexity": [CYC_VALUE],
  "phases": {
    "phase_0": {
      "status": "completed",
      "output": "00-hotspots.md",
      "timestamp": "[ISO_TIMESTAMP]"
    }
  },
  "wave": 7,
  "created_at": "[ISO_TIMESTAMP]",
  "updated_at": "[ISO_TIMESTAMP]"
}
```

## Key Patterns

### 1. Bob CLI Invocation
**ALWAYS** use temp file + command substitution:
```bash
cat > /tmp/msg.txt << 'EOFMSG'
[message]
EOFMSG

bob --yolo --chat-mode ask "$(cat /tmp/msg.txt)"
```

**NEVER** use inline strings (causes freeze):
```bash
# ❌ WRONG
bob --yolo --chat-mode ask "inline message"
```

### 2. API Key Rotation
Use 17 working Bob Shell API keys in round-robin:
- Extract from successful epics
- Rotate to avoid budget exhaustion
- Each key has 160 bobcoin limit

### 3. Directory Creation
Python creates directories before script execution:
```python
os.makedirs(f"docs/brain/{epic_id}", exist_ok=True)
```

### 4. Environment Variables
Explicitly set PATH in subprocess:
```python
env = os.environ.copy()
env["PATH"] = "/usr/bin:/bin:/usr/local/bin:" + env.get("PATH", "")
subprocess.Popen(['/usr/bin/bash', script], env=env)
```

## Building-Blocks Method

**For Phase 1 scripts**: Copy from EPIC-W7-001's Phase 1 script
**For Phase 2 scripts**: Copy from EPIC-W7-001's Phase 2 script
**etc.**

**NEVER** generate scripts from scratch. Always copy from previous wave's SAME phase.

## Success Metrics

- ✅ 161/161 epics completed Phase 0
- ✅ Average execution time: ~2 minutes per epic
- ✅ Parallel execution: 151 concurrent processes
- ✅ Cost optimization: 4-minute polling intervals
- ✅ Recovery rate: 96.9% → 100% (4 recoveries needed)

## Lessons Learned

1. **PATH inheritance**: Always set explicitly in subprocess
2. **API key rotation**: Essential for large-scale execution
3. **Phantom epics**: Some methods already refactored (check src-vm-backup/)
4. **Bob CLI pattern**: Temp file + command substitution prevents freezes
5. **Python > Shell**: Use Python for orchestration, shell for Bob invocation only