# Local Autonomous Execution Pattern

**Version**: 1.0
**Created**: 2026-06-16
**Purpose**: Document the proven pattern for executing Wave 4 phases locally using Bob CLI

## Overview

This document captures the **building-blocks method** for local autonomous execution, proven during EPIC-CCN-016 completion. Use this pattern when VM execution fails or for local development.

## Core Pattern: Sequential Phase Execution

**Key Principle**: Execute ONE phase at a time using Bob CLI, exactly mirroring VM script execution.

### Pattern Structure

```powershell
# Set API key for the phase
$env:BOBSHELL_API_KEY='[phase-specific-api-key]'

# Execute Bob CLI with phase instructions
bob --yolo --chat-mode [mode] @"
[Phase instructions from VM script]
"@
```

## Phase-by-Phase Execution Guide

### Phase 1: Scope Definition

**Mode**: `plan`
**API Key**: From `scripts/wave4/_p1_XXX.sh` line 4
**Input**: None (reads from jCodemunch hotspots)
**Output**: `01-scope.md`, `01-scope-boundary.md`

**Command Template**:
```powershell
$env:BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'

bob --yolo --chat-mode plan @"
Execute Phase 1 (Scope Definition) for EPIC-CCN-XXX.

**CRITICAL FILE I/O PROTOCOL - READ THIS FIRST**

You are running in SSH/non-interactive mode where Bob's file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ❌ NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist
3. ❌ NEVER use run_shell_command tool - it also has persistence bugs in SSH mode
4. ✅ ALWAYS use execute_command tool with ``cat > file << 'EOF'`` to create files
5. ✅ ALWAYS use execute_command tool with ``ls -lh`` and ``wc -l`` to verify files
6. ✅ ALWAYS set cwd parameter to c:/WSGTA/universal-or-strategy
7. ✅ ALWAYS follow the EXACT tool usage patterns shown below (copy/paste them)

**WHY THIS MATTERS**:
- execute_command bypasses Bob's tool layer and works reliably in SSH mode
- run_shell_command, write_to_file, and read_file all fail in SSH/screen sessions
- The working directory must be explicitly set with cwd parameter

**YOUR TASK**: Focus on the analysis, not the tools. The shell commands below are proven to work.

---

## Phase 1 Task: Scope Definition

**Target Method**:
- Method: [MethodName]
- File: [FilePath]
- Complexity: [CYC]
- LOC: [Lines]
- Tier: [1/2/3]

**Phase 1: Scope Definition**

Create ``docs/brain/EPIC-CCN-XXX/01-scope.md`` with:

1. **Method Analysis**:
   - Current complexity and LOC
   - Extraction strategy overview
   - Target complexity (≤8)

2. **Boundary Definition**:
   - What's IN scope (method body only)
   - What's OUT of scope (callers, callees, other files)
   - V12.23 No Scope Creep validation

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.
"@
```

**Verification**:
```powershell
Get-Item docs/brain/EPIC-CCN-XXX/01-scope.md | Select-Object Name, Length
Get-Item docs/brain/EPIC-CCN-XXX/01-scope-boundary.md | Select-Object Name, Length
```

### Phase 2: Architecture Planning

**Mode**: `plan`
**API Key**: From `scripts/wave4/_p2_XXX.sh` line 4
**Input**: `01-scope-boundary.md`
**Output**: `02-architecture-plan.md`

**Command Template**:
```powershell
$env:BOBSHELL_API_KEY='bob_prod_bob-admin_5A6hXsy7FL4vf9T2jqr11gdYTmAZcFgxVm1dGD9qGPmpD5fV6emRy6XYzZPsqw56mjCtoiEbJmLU8B2VL4ZtgXeS_ALp1DF9sj3R3cU3dzddRRAVu44Y52VHhkt1BNkSdC2Nq'

bob --yolo --chat-mode plan @"
Execute Phase 2 (Architecture Planning) for EPIC-CCN-XXX.

**CRITICAL FILE I/O PROTOCOL - READ THIS FIRST**

[Same file I/O protocol as Phase 1]

---

## Phase 2 Task: Architecture Planning

**Input**: Read ``docs/brain/EPIC-CCN-XXX/01-scope-boundary.md``

**Target Method**:
- Method: [MethodName]
- File: [FilePath]
- Complexity: [CYC]
- LOC: [Lines]
- Tier: [1/2/3]

**Phase 2: Architecture Planning**

Create ``docs/brain/EPIC-CCN-XXX/02-architecture-plan.md`` with:

1. **Extraction Strategy**:
   - Current method complexity
   - Target complexity: ≤8 (Jane Street strict standard)
   - Proposed helper methods: 2-3 methods with clear responsibilities

2. **Method Signatures**:
   - Original method signature (from jCodemunch)
   - Proposed helper method signatures
   - Parameter types and return types
   - Access modifiers (private/internal)

3. **Call Graph**:
   - Which helper calls which
   - Data flow between methods
   - Shared state (if any)

4. **Lock-Free Validation**:
   - ✅ No lock() statements
   - ✅ Uses FSM/Actor Enqueue pattern
   - ✅ Atomic primitives only

5. **Jane Street Compliance**:
   - Query Jane Street KB for extraction patterns
   - Validate against HFT microsecond-latency requirements
   - Ensure cognitive simplicity (CYC ≤8)

**Sequential Thinking** (MANDATORY):
Use sequential thinking MCP to break down architectural decisions:
- Step 1: Analyze method complexity
- Step 2: Identify extraction boundaries
- Step 3: Design helper method signatures
- Step 4: Validate lock-free compliance
- Step 5: Verify Jane Street alignment

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.
"@
```

**Verification**:
```powershell
Get-Item docs/brain/EPIC-CCN-XXX/02-architecture-plan.md | Select-Object Name, Length
```

### Phase 3: DNA & PR Audit

**Mode**: `advanced`
**API Key**: From `scripts/wave4/_p3_XXX.sh` line 4
**Input**: `02-architecture-plan.md`
**Output**: `03-audit-report.md`

**Command Template**:
```powershell
$env:BOBSHELL_API_KEY='[phase-3-api-key]'

bob --yolo --chat-mode advanced @"
Execute Phase 3 (DNA & PR Audit) for EPIC-CCN-XXX.

[Same file I/O protocol]

## Phase 3 Task: DNA & PR Audit

**Input**: Read ``docs/brain/EPIC-CCN-XXX/02-architecture-plan.md``

Run V12 DNA compliance checks and PR hygiene validation.

Create ``docs/brain/EPIC-CCN-XXX/03-audit-report.md`` with:

1. **V12 DNA Compliance**:
   - Lock-free validation
   - ASCII-only check
   - Cyclomatic complexity targets
   - FSM/Actor pattern usage

2. **Jane Street Validation**:
   - Query KB for relevant rules
   - Check P0/P1/P2 violations
   - Validate cognitive simplicity

3. **PR Hygiene**:
   - Diff size estimate
   - File count
   - Scope boundary check

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.
"@
```

### Phase 4: Ticket Generation

**Mode**: `plan`
**API Key**: From `scripts/wave4/_p4_XXX.sh` line 4
**Input**: `02-architecture-plan.md`
**Output**: `04-tickets.md`

**Command Template**:
```powershell
$env:BOBSHELL_API_KEY='[phase-4-api-key]'

bob --yolo --chat-mode plan @"
Execute Phase 4 (Ticket Generation) for EPIC-CCN-XXX.

[Same file I/O protocol]

## Phase 4 Task: Ticket Generation

**Input**: Read ``docs/brain/EPIC-CCN-XXX/02-architecture-plan.md``

Generate surgical extraction tickets.

Create ``docs/brain/EPIC-CCN-XXX/04-tickets.md`` with:

1. **Ticket Breakdown**:
   - TICKET-1: Extract helper method 1
   - TICKET-2: Extract helper method 2
   - TICKET-3: Extract helper method 3 (if needed)

2. **Per Ticket**:
   - Method signature
   - Extraction logic
   - Test requirements
   - Success criteria

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.
"@
```

### Phase 5: Ticket Execution

**Mode**: `v12-engineer` (Bob CLI for src/ work)
**API Key**: From `scripts/wave4/_p5_XXX.sh` line 4
**Input**: `04-tickets.md`
**Output**: `ticket-1-completion.md`, `ticket-2-completion.md`, etc.

**Command Template**:
```powershell
$env:BOBSHELL_API_KEY='[phase-5-api-key]'

bob --yolo --chat-mode code @"
Execute Phase 5 (Ticket Execution) for EPIC-CCN-XXX.

[Same file I/O protocol]

## Phase 5 Task: Ticket Execution

**Input**: Read ``docs/brain/EPIC-CCN-XXX/04-tickets.md``

Execute all tickets sequentially:

1. **TICKET-1**: Extract first helper method
2. **TICKET-2**: Extract second helper method
3. **TICKET-3**: Extract third helper method (if exists)

**Per Ticket**:
- Apply extraction
- Verify build passes
- Run tests
- Create completion file

**CRITICAL**: Only use attempt_completion AFTER all ticket completion files verified on disk.
"@
```

### Phase 6: Final Review

**Mode**: `advanced`
**API Key**: From `scripts/wave4/_p6_XXX.sh` line 4
**Input**: All ticket completion files
**Output**: `06-verification-report.md`

**Command Template**:
```powershell
$env:BOBSHELL_API_KEY='[phase-6-api-key]'

bob --yolo --chat-mode advanced @"
Execute Phase 6 (Final Review) for EPIC-CCN-XXX.

[Same file I/O protocol]

## Phase 6 Task: Final Review

**Input**: Read all ticket completion files

Verify epic completion:

1. **Complexity Verification**:
   - Check final CYC ≤8
   - Verify all helpers ≤8

2. **Build Verification**:
   - dotnet build passes
   - No compilation errors

3. **Behavioral Verification**:
   - Tests pass
   - No behavioral changes

Create ``docs/brain/EPIC-CCN-XXX/06-verification-report.md`` with final status.

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.
"@
```

## Key Lessons from EPIC-CCN-016

### What Worked

1. **Sequential Execution**: One phase at a time, fresh Bob CLI session per phase
2. **File I/O Protocol**: Always use `execute_command` with PowerShell heredoc (`@' '@`)
3. **Verification**: Check file exists after every phase
4. **API Key Rotation**: Use phase-specific API keys from VM scripts
5. **Mode Selection**: Respect mode requirements (plan/advanced/code)

### What Failed

1. ❌ **Unix Commands**: `grep`, `cat`, `ls -lh` don't work on Windows
2. ❌ **Bob's File Tools**: `write_to_file`, `read_file` fail in SSH mode
3. ❌ **Python Scripts**: Multi-line Python in PowerShell requires escaping
4. ❌ **Background Execution**: Always use foreground for visibility

### PowerShell Adaptations

**File Creation**:
```powershell
# Use PowerShell heredoc
@'
[content]
'@ | Out-File -FilePath [path] -Encoding UTF8
```

**File Verification**:
```powershell
# Use Get-Item instead of ls
Get-Item [path] | Select-Object Name, Length
```

**File Reading**:
```powershell
# Use Get-Content instead of cat
Get-Content [path] -Raw
```

**Method Extraction**:
```powershell
# Use PowerShell regex instead of grep
$content = Get-Content [file] -Raw
if ($content -match '(?s)pattern') { $matches[0] }
```

## Building Blocks Checklist

Before starting local execution:

- [ ] Read VM script for target phase (`scripts/wave4/_pX_XXX.sh`)
- [ ] Extract API key from line 4
- [ ] Extract phase instructions from heredoc (lines 8-110)
- [ ] Adapt Unix commands to PowerShell equivalents
- [ ] Set `$env:BOBSHELL_API_KEY`
- [ ] Execute Bob CLI with `--yolo --chat-mode [mode]`
- [ ] Verify output files exist
- [ ] Check file sizes are reasonable (>1KB)
- [ ] Proceed to next phase

## Success Metrics

**Per Phase**:
- ✅ Output file(s) created
- ✅ File size >1KB (not empty)
- ✅ Bob CLI exit code 0
- ✅ No error messages in output

**Per Epic**:
- ✅ All 6 phases complete
- ✅ All output files committed to git
- ✅ Build passes (`dotnet build`)
- ✅ Complexity target met (CYC ≤8)

## Integration with Autonomous Refactor Command

This local execution pattern can be integrated into the `/autonomous-refactor` command for:

1. **Fallback Execution**: When VM is unavailable
2. **Development Testing**: Test phase scripts locally before VM deployment
3. **Recovery**: Complete failed epics locally
4. **Debugging**: Step through phases with full visibility

## Next Steps

1. Document Phase 3-6 execution (after completing EPIC-CCN-016)
2. Create PowerShell wrapper scripts for each phase
3. Add to `building-blocks/autonomous-refactoring/` directory
4. Update `GETTING_STARTED.md` with local execution option

---

**Status**: ✅ Phases 1-2 proven (EPIC-CCN-016)
**Next**: Complete Phases 3-6 and document patterns
**Maintainer**: Wave 4 Completion Lead