---
description: Manual TDD-mode Epic Execution. Provides a structured path for manual ticket execution while maintaining autonomous /pr-loop hardening after every commit.
argument-hint: <epic-slug> <ticket-number> <pr-number>
---
# EPIC TDD -- MANUAL EXECUTION
**Epic Slug:** $1
**Ticket:** $2
**Target PR:** $3
**Mode:** Engineer (Manual-parity)
**Protocol:** V12 High-Precision TDD Gate

You are the V12 TDD Engineer. You are executing ticket $2 of epic $1 manually.
Your goal is to implement the changes and drive them to 100/100 PHS on PR #$3.

---

## VERIFICATION GATES (MANDATORY)

All 4 gates from epic-run MUST be executed before implementation:

### GATE 1: INTAKE (Director Approval)
**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Review ticket scope in docs/brain/$1/ticket-$2.md
OUTPUT: Summarize ticket scope, CYC targets, and extraction plan
STOP at [INTAKE-GATE] and do not proceed.
```

When v12-epic-planner outputs [INTAKE-GATE], present the summary to Director.

**GATE 1 CHECKPOINT:**
> "Ticket scope: [summary]. Does this match your intent? Reply YES to proceed or give corrections."

- YES: advance to GATE 2
- Corrections: relay to v12-epic-planner, re-run intake

---

### GATE 2: PLAN REVIEW (Independent Agent)
**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Review extraction plan in docs/brain/$1/ticket-$2.md
VERIFY:
  - Sub-method names are descriptive
  - CYC targets are realistic
  - No logic duplication
  - Caller impact is minimal
OUTPUT: Write plan review to docs/brain/$1/ticket-$2-review.md
STOP at [PLAN-GATE] and do not proceed.
```

When v12-epic-planner outputs [PLAN-GATE], present key findings.

**GATE 2 CHECKPOINT:**
> "Plan review complete. Key findings: [top 3]. Type APPROVED to proceed or provide feedback."

- APPROVED: advance to GATE 2.5
- Feedback: relay to v12-epic-planner, re-run plan review

---

### GATE 2.5: BOB FINDINGS REVIEW (NEW - V12.22)
**Switch to: v12-engineer mode**

Hand off this exact task:
```
EPIC: $1
TICKET: $2
TASK: Review Bob Findings for ticket scope
PROTOCOL:
  1. Open Bob Findings panel in VSCode (View → Problems → Filter to "Bob Findings")
  2. Identify files in ticket scope from docs/brain/$1/ticket-$2.md
  3. Filter Bob Findings to only those files
  4. For each finding:
     - P0 (CRITICAL): MUST fix before implementation - blocks GATE 2.5
     - P1 (HIGH): Add to ticket implementation notes as mandatory fix
     - P2 (MEDIUM): Document as technical debt in ticket-$2-debt.md
  5. Export findings to: docs/brain/$1/ticket-$2-bob-findings.md
  6. Update ticket-$2.md with Bob findings summary
  7. Emit: [BOB-FINDINGS-REVIEWED] X critical, Y high, Z medium
```

**GATE 2.5 CHECKPOINT:**
> "Bob Findings reviewed. Critical (P0): [list]. High (P1): [list]. Reply GO to proceed or FIX to address critical issues now."

- GO (if zero P0 issues): advance to GATE 2.3
- FIX (if P0 issues exist): Address critical findings using "Fix with Bob" button, then re-run review

**Rationale:** Bob CLI's static analysis catches architectural issues (god functions, coupling, complexity) that may not be visible in the extraction plan. Blocking on P0 prevents implementing flawed designs.

---

### GATE 2.3: SENTINEL AUDIT (Manual Review)
**MANUAL TASK (Greptile MCP not working)**

Director must manually verify:
1. Read ticket file: `docs/brain/$1/ticket-$2.md`
2. Use Semgrep or manual audit to check:
   - No new `lock()` statements
   - ASCII-only compliance
   - FSM/Actor pattern followed
   - No hardcoded secrets
3. Document findings in: `docs/brain/$1/ticket-$2-sentinel.md`

**GATE 2.3 CHECKPOINT:**
> "Sentinel Audit complete. Verdict: [PASSED/REVISION REQUIRED]. Gaps found: [list]. Reply GO to proceed to GATE 3 or REVISE to update the plan."

- GO: advance to GATE 3
- REVISE: update ticket, re-run GATE 2

---

### GATE 3: DNA VALIDATION (Compliance Check)
**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Validate ticket against V12 DNA principles
INPUT: @docs/brain/$1/ticket-$2.md @docs/standards/JANE_STREET_DEVIATIONS.md
VERIFY:
  - Zero locks (FSM/Actor pattern)
  - ASCII-only (no Unicode)
  - Complexity ≤15 target
  - No illegal states possible
OUTPUT: Update ticket-$2.md with DNA compliance notes
STOP at [VALIDATE-GATE] and do not proceed.
```

When v12-epic-planner outputs [VALIDATE-GATE], present compliance summary.

**GATE 3 CHECKPOINT:**
> "DNA validation complete. [N issues resolved]. Type GO to begin implementation or HOLD to review."

- GO: advance to Step 1 (Implementation)
- HOLD: wait for Director review, then re-validate

---

## EXECUTION WORKFLOW

### Step 1: Implementation
Execute the changes described in `docs/brain/$1/ticket-$2.md`.
Ensure:
- ✅ Zero locks.
- ✅ ASCII only.
- ✅ FSM/Actor pattern followed.

### Step 2: Local Verification
Run the FULL validation suite:
1. `powershell -File .\deploy-sync.ps1`
2. `powershell -File .\scripts\pre_push_validation.ps1`

Expected output: ALL blocking checks PASS (8/8 required, 5/5 warnings informational).

If ANY blocking check fails: **HALT and fix.**

### Step 3: Pre-Commit Panel Review (NEW - V12.22)
**MANDATORY before Step 4**

Check all three VSCode panels (see AGENTS.md Section 3.5 for full protocol):

1. **Bob Findings Panel** (View → Problems → Filter: "Bob Findings"):
   - Filter to files you modified
   - **BLOCKING**: Zero P0 (Critical) findings
   - **REQUIRED**: Document P1 (High) as technical debt in ticket-$2-debt.md
   - **OPTIONAL**: P2 (Medium) can be deferred

2. **Problems Panel** (View → Problems):
   - Filter to files you modified
   - **BLOCKING**: Zero errors (warnings OK)
   - Fix all compilation errors

3. **Comments Panel** (View → Comments):
   - Only relevant if working on existing PR
   - Review any new bot comments
   - Address P0/P1 issues

**If ANY blocking issue found**: Fix it before proceeding to Step 4.

### Step 4: F5 Verification
1. Press F5 in NinjaTrader.
2. Verify the BUILD_TAG banner.
3. Confirm the Logic Audit passes.

### Step 5: Commit & Push
1. `git add .`
2. `git commit -m "[$1] ticket-$2: manual TDD implementation"`
3. `git push`

### Step 6: Autonomous Perfection Gate
**Switch to: Orchestrator mode (/pr-loop)**

Immediately after pushing, hand off to the perfection loop:
```
EPIC: $1
TASK: Run /pr-loop $3
GOAL: Drive the ticket to 100/100 PHS.
STOP when /pr-loop outputs [PHS-PERFECT].
```

---

## COMPLETION
Once /pr-loop is 100/100, report status to Director:
"Ticket $2 complete. 100/100 PHS achieved. Proceed to next ticket?"
