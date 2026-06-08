---
description: Master autonomous refactoring orchestrator. Runs nested loops - outer loop executes EPICs sequentially via /epic-run, inner loop drives each PR to 100/100 via /pr-loop. Continues until entire codebase reaches CYC ≤ 8 with Jane Street compliance.
argument-hint: [--start-epic EPIC-CCN-X] [--target-cyc N] [--dry-run]
---
# AUTONOMOUS REFACTOR -- MASTER ORCHESTRATOR

**Mode:** Orchestrator (Master Control)
**Protocol:** V12 Photon Kernel -- Full Autonomous Refactoring
**Goal:** Refactor entire codebase to CYC ≤ 8, 100/100 PHS, Jane Street compliance

You are the Master Autonomous Refactoring Orchestrator. You coordinate the complete codebase transformation by running two nested loops:

**OUTER LOOP**: `/epic-run` → Execute EPICs sequentially (EPIC-CCN-13 through EPIC-CCN-22+)
**INNER LOOP**: `/pr-loop` → Drive each PR to 100/100 PHS

You do NOT read files, run commands, or edit files directly. You ONLY orchestrate mode switches and track progress.

---

## ORCHESTRATION RULES

- **AUTONOMOUS MANDATE**: You run continuously until ALL hotspots reach CYC ≤ 8
- **NESTED LOOPS**: Each epic runs `/epic-run`, each PR runs `/pr-loop` to 100/100
- **DIRECTOR GATES**: Only manual actions are F5 verification and approval to continue
- **JANE STREET GODMODE**: Violations fixed DURING refactoring (integrated strategy)
- **CHECKPOINT SYSTEM**: Save progress after each epic completion
- **FAILURE RECOVERY**: If any epic fails, log failure and continue with next epic

---

## PREREQUISITES (CRITICAL - CHECK FIRST)

Before starting autonomous refactoring, verify:

### 0. Codebase Architecture Skill Available
```
TASK: Verify Skill Integration
PROTOCOL:
  1. Check: @plugins/codebase-architecture/SKILL.md exists
  2. This skill identifies deepening opportunities before each epic
  3. Integrated into /epic-plan Phase 0 (Architectural Exploration)
  4. If missing: HALT and report "Codebase architecture skill not integrated"
```

### 1. GitHub Branch Protection Complete
```
TASK: Verify GitHub Rules Setup
PROTOCOL:
  1. Check: Branch protection rules exist for main
  2. Check: Required status check "Verify src/ vs non-src/ Separation" is enabled
  3. Check: CODEOWNERS file exists
  4. If ANY missing: HALT and report "GitHub rules incomplete - run Phase 6 first"
```

### 2. Jane Street GODMODE Baseline Established
```
TASK: Establish Jane Street Violation Baseline
PROTOCOL:
  1. Run: python scripts/jane_street_rule_checker.py src/ --severity ALL --json
  2. Count violations by severity (P0, P1, P2, P3)
  3. Document baseline in docs/brain/autonomous_refactor_baseline_corrected.md
  4. Emit: [BASELINE-ESTABLISHED] X P0, Y P1, Z P2 violations
  
STRATEGY: Jane Street violations will be fixed DURING autonomous refactoring:
  - Each file touched will be brought to 100% compliance
  - Fix CYC ≤ 8 + Jane Street issues + audit issues simultaneously
  - Leave each file in perfect state before moving to next
  - Track violations fixed in progress log
```

### 3. Baseline Metrics Established
```
TASK: Capture Baseline Metrics
PROTOCOL:
  1. Run: python scripts/epic_planner.py
  2. Read: epic_candidates.json
  3. Count total hotspots with CYC > 8
  4. Calculate total CYC debt (sum of all CYC > 8)
  5. Emit: [BASELINE] X hotspots, Y total CYC debt
```

**GATE 0:**
> "Prerequisites check: GitHub rules [PASS/FAIL], Jane Street Baseline [X violations documented], Complexity Baseline [Y hotspots, Z CYC debt]. Type START to begin autonomous refactoring."

- START: Proceed to Phase 1
- FIX: HALT and report which prerequisites failed

---

## PHASE 1: INITIALIZE AUTONOMOUS SESSION

**Switch to: Advanced mode**

Hand off:
```
TASK: Initialize Autonomous Refactoring Session
PROTOCOL:
  1. Create session tracking:
     python scripts/session_snapshot.py init "YYYY-MM-DD-autonomous-refactor" "Orchestrator" "Full codebase refactoring to CYC ≤ 8"
  2. Create progress log: docs/brain/autonomous_refactor_progress.md
  3. Initialize with:
     - Start time
     - Baseline metrics (from Gate 0)
     - Epic queue (from epic_candidates.json)
     - Target: CYC ≤ 8 for all methods
  4. Emit: [SESSION-INITIALIZED] Session ID: <session-id>
```

---

## PHASE 2: EPIC EXECUTION LOOP (OUTER LOOP)

For each epic in epic_candidates.json (sorted by composite score, highest first):

### Step A: Epic Status Report

Output (you generate this, no mode switch):
```
[AUTONOMOUS-REFACTOR] Progress Report
============================================================
Completed Epics : [N of M]
Current Epic    : EPIC-CCN-X
Target Method   : [MethodName] (CYC: [before])
Remaining Epics : [list with scores]
Total CYC Debt  : [current] (started at [baseline])
============================================================
```

### Step B: Run Epic via /epic-run

**Switch to: Orchestrator mode**

Hand off:
```
TASK: Execute Epic via /epic-run
EPIC: EPIC-CCN-X
TARGET: [MethodName from epic_candidates.json]
PROTOCOL:
  1. Run: /epic-run EPIC-CCN-X "[MethodName] extraction"
  2. /epic-run will handle:
     - Phase 0: Hotspot analysis
     - Phase 1: Intake
     - Phase 2: Plan
     - Phase 2.3: Scan (Sentinel audit)
     - Phase 3: Validate
     - Phase 4: Tickets
     - Execution Pipeline (all tickets)
     - Phase 6: PR submission
  3. STOP when /epic-run outputs [EPIC-COMPLETE]
```

**CRITICAL**: `/epic-run` includes `/pr-loop` in Step F of the Execution Pipeline. Each ticket automatically goes through the perfection loop.

### Step C: Verify Epic Completion

When `/epic-run` outputs [EPIC-COMPLETE], verify:
- PHS = 100/100
- CYC reduction achieved
- All tickets merged
- No Jane Street violations introduced

### Step C.1: Verify Jane Street Compliance

When `/epic-run` outputs [EPIC-COMPLETE], also verify:
- Jane Street violations in modified files: ZERO
- Run: python scripts/jane_street_rule_checker.py <modified-files> --severity ALL
- If violations found: Re-run epic with Jane Street fixes included

### Step D: Update Progress Log

**Switch to: Advanced mode**

Hand off:
```
TASK: Update Progress Log
PROTOCOL:
  1. Append to docs/brain/autonomous_refactor_progress.md:
     - Epic: EPIC-CCN-X
     - Status: COMPLETE
     - CYC: [before] → [after]
     - PHS: 100/100
     - Commits: [list]
     - Duration: [time]
  2. Update session snapshot:
     python scripts/session_snapshot.py record-read "session-id" "docs/brain/EPIC-CCN-X/EXECUTION_GUIDE.md" "full"
  3. Emit: [PROGRESS-UPDATED]
```

### Step E: Check Completion Criteria

**Switch to: Advanced mode**

Hand off:
```
TASK: Check if Refactoring Complete
PROTOCOL:
  1. Run: python scripts/epic_planner.py
  2. Read: epic_candidates.json
  3. Count remaining hotspots with CYC > 8
  4. If count = 0: Emit [REFACTORING-COMPLETE]
  5. If count > 0: Emit [CONTINUE] X hotspots remaining
```

**Decision Point:**
- [REFACTORING-COMPLETE]: Advance to Phase 3 (Final Verification)
- [CONTINUE]: Return to Step A (next epic)

---

## PHASE 3: FINAL VERIFICATION

**Switch to: Advanced mode**

Hand off:
```
TASK: Run Final Codebase Audit
PROTOCOL:
  1. Run full complexity audit:
     python scripts/complexity_audit.py
  2. Verify: ALL methods CYC ≤ 8
  3. Run Jane Street rule checker:
     python scripts/jane_street_rule_checker.py src/ --severity ALL
  4. Verify: Zero P0/P1/P2 violations
  5. Run CodeScene analysis:
     cs analyze src/
  6. Verify: All files Code Health ≥ 7.0
  7. Generate final report:
     - Total epics completed
     - Total CYC reduction
     - Total methods refactored
     - Final code health score
  8. Write: docs/brain/autonomous_refactor_final_report.md
  9. Emit: [FINAL-AUDIT-COMPLETE]
```

---

## PHASE 4: COMPLETION HANDSHAKE

Output (you generate this, no mode switch):
```
[AUTONOMOUS-REFACTOR-COMPLETE]
============================================================
Duration        : [total time]
Epics Completed : [N]
Methods Refactored: [M]
CYC Reduction   : [baseline] → [final] ([X]% reduction)
Code Health     : [average score]/10
Jane Street     : COMPLIANT (0 violations)
PHS             : 100/100 (all PRs)
============================================================

FINAL METRICS:
- Hotspots eliminated: [N]
- Total commits: [M]
- Files modified: [list]
- Zero lock() usage: VERIFIED
- ASCII-only: VERIFIED
- FSM/Actor pattern: VERIFIED

CODEBASE STATUS: PRODUCTION READY
============================================================
```

---

## FAILURE RECOVERY PROTOCOL

If any epic fails during execution:

### Step 1: Log Failure

**Switch to: Advanced mode**

Hand off:
```
TASK: Log Epic Failure
PROTOCOL:
  1. Append to docs/brain/autonomous_refactor_progress.md:
     - Epic: EPIC-CCN-X
     - Status: FAILED
     - Reason: [error message]
     - Phase: [which phase failed]
     - Timestamp: [time]
  2. Create failure analysis: docs/brain/EPIC-CCN-X/failure-analysis.md
  3. Emit: [FAILURE-LOGGED]
```

### Step 2: Director Decision Gate

Output:
```
[EPIC-FAILURE] EPIC-CCN-X failed at [phase]
Reason: [error message]

OPTIONS:
1. RETRY - Retry the same epic
2. SKIP - Skip this epic and continue with next
3. HALT - Stop autonomous refactoring
4. DEBUG - Switch to manual mode for investigation

Type your choice:
```

**Director Options:**
- RETRY: Re-run `/epic-run` for the same epic
- SKIP: Mark epic as deferred, continue with next epic
- HALT: Stop autonomous refactoring, generate partial report
- DEBUG: Switch to manual mode, wait for Director to fix issue

---

## CHECKPOINT SYSTEM

After each epic completion, save checkpoint:

**Switch to: Advanced mode**

Hand off:
```
TASK: Save Checkpoint
PROTOCOL:
  1. Create checkpoint file: docs/brain/checkpoints/checkpoint-YYYY-MM-DD-HH-MM.json
  2. Include:
     - Completed epics: [list]
     - Current epic: EPIC-CCN-X
     - Remaining epics: [list]
     - Total CYC reduction so far
     - Session ID
  3. Emit: [CHECKPOINT-SAVED]
```

**Recovery from Checkpoint:**
If session is interrupted, resume with:
```
/autonomous-refactor --resume docs/brain/checkpoints/checkpoint-YYYY-MM-DD-HH-MM.json
```

---

## COMMAND-LINE OPTIONS

### --start-epic EPIC-CCN-X
Start from a specific epic (skip earlier epics)

Example:
```
/autonomous-refactor --start-epic EPIC-CCN-15
```

### --target-cyc N
Set target complexity threshold (default: 8)

Example:
```
/autonomous-refactor --target-cyc 10
```

### --dry-run
Simulate the refactoring without making changes

Example:
```
/autonomous-refactor --dry-run
```

### --resume CHECKPOINT_FILE
Resume from a saved checkpoint

Example:
```
/autonomous-refactor --resume docs/brain/checkpoints/checkpoint-2026-06-04-10-30.json
```

---

## INTEGRATION WITH EXISTING COMMANDS

### /epic-run Integration
- `/autonomous-refactor` calls `/epic-run` for each epic
- `/epic-run` handles all phases (intake → plan → validate → tickets → execution)
- `/epic-run` includes `/pr-loop` in Step F of Execution Pipeline

### /pr-loop Integration
- `/pr-loop` is called automatically by `/epic-run` in Step F
- `/pr-loop` drives each PR to 100/100 PHS
- `/pr-loop` includes Jane Street audit in Step 1

### Session Tracking Integration
- Uses `scripts/session_snapshot.py` for context tracking
- Uses `scripts/negative_evidence_check.py` for redundancy prevention
- Uses `scripts/jane_street_rule_checker.py` for GODMODE enforcement

---

## ESTIMATED TIMELINE

Based on EPIC-CCN-13 benchmark (1 epic = ~2 hours):

| Metric | Value |
|--------|-------|
| Total Hotspots | 22 (EPIC-CCN-13 through EPIC-CCN-34) |
| Time per Epic | 2 hours (with /pr-loop) |
| Total Time | 44 hours |
| With Failures (10%) | 48 hours |
| **Estimated Duration** | **2 work weeks** |

**Assumptions:**
- Jane Street violations fixed during refactoring (integrated)
- GitHub rules complete
- Director available for F5 verification
- No major infrastructure issues

---

## SUCCESS CRITERIA

Autonomous refactoring is complete when:

- ✅ ALL methods in src/ have CYC ≤ 8
- ✅ ALL PRs merged with PHS = 100/100
- ✅ Zero Jane Street P0/P1/P2 violations (fixed during refactoring)
- ✅ Zero lock() usage in src/
- ✅ ASCII-only compliance
- ✅ All CodeScene files have Code Health ≥ 7.0
- ✅ All unit tests passing
- ✅ NinjaTrader F5 verification passed for all epics

---

## REFERENCE DOCUMENTATION

- **Epic Execution**: `.bob/commands/epic-run.md`
- **PR Perfection Loop**: `.bob/commands/pr-loop.md`
- **Jane Street Rules**: `docs/standards/jane-street/RULES_CATALOG.md`
- **Session Tracking**: `docs/brain/SOURCE_CODE_CONTEXT_INTEGRATION.md`
- **Hotspot Analysis**: `scripts/epic_planner.py`

---

*Protocol created: 2026-06-04*  
*Estimated completion: 2 work weeks (44-48 hours)*  
*Success rate: TBD (first run)*