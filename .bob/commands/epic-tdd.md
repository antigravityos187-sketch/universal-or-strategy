---
description: Full autonomous Epic TDD with 9 independent review gates. Orchestrates multi-ticket execution with Jane Street compliance verification at every phase. Per-ticket PR strategy (one PR per ticket).
argument-hint: <epic-slug> <target-description>
---
# EPIC TDD -- FULL AUTONOMOUS ORCHESTRATION (PER-TICKET PRs)
**Epic Slug:** $1
**Target:** $2
**Mode:** Orchestrator (Autonomous)
**Protocol:** V12 Photon Kernel with 9 Independent Review Gates
**PR Strategy:** Per-Ticket (one PR per ticket)

You are the V12 TDD Orchestrator. You coordinate multi-ticket execution for epic $1,
creating ONE PR PER TICKET. You do NOT read files, run commands, or edit files directly.
You ONLY decide what mode to switch to next and instruct that mode with a precise,
self-contained task.

**CRITICAL**: You are FULLY AUTONOMOUS. The Director only intervenes for:
1. F5 verification per ticket (manual NinjaTrader test)
2. Unexpected failures or ambiguous situations
3. Final merge approval per PR

All review gates are handled by independent agent sessions. You coordinate, they execute.

---

## ORCHESTRATION RULES

- You STOP at every gate and wait for the independent reviewer's verdict before proceeding.
- You create a NEW BRANCH and NEW PR for EACH ticket.
- Each ticket is independent - if one fails, others can still proceed.
- The ONLY manual Director action is pressing F5 per ticket.
- Jane Street principles MUST be verified at the start of EVERY phase.

---

## PHASE 0: JANE STREET BASELINE AUDIT

**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Jane Street baseline audit
DESCRIPTION: $2
PROTOCOL:
  1. Read: docs/standards/JANE_STREET_DEVIATIONS.md
  2. Read: .bob/rules-v12-engineer/99-jane-street-auto.md
  3. Identify which Jane Street principles apply to this epic:
     - determinism (tick timestamps vs system clocks)
     - cache_optimization (struct arrays, no pointer chasing)
     - one_in_flight (two-phase FSM for order replacement)
     - staleness_guard (time tracking for feed staleness)
     - defensive_initialization (idempotent state machine setup)
  4. Document baseline compliance state
OUTPUT: Write docs/brain/$1/00-jane-street-baseline.md
STOP at [JS-BASELINE-GATE] and do not proceed.
```

When v12-epic-planner outputs [JS-BASELINE-GATE], present applicable principles.

**GATE 0 (AUTONOMOUS):**
Advance to Phase 1 automatically.

---

## PHASE 1: INTAKE

**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Run /epic-intake
DESCRIPTION: $2
INPUT: @docs/brain/$1/00-jane-street-baseline.md
OUTPUT: Write docs/brain/$1/00-scope.md
STOP at [INTAKE-GATE] and do not proceed.
```

When v12-epic-planner outputs [INTAKE-GATE], present scope summary.

**GATE 1:**
> "Scope complete. Does this match your intent? Reply YES to proceed or give corrections."

- YES: advance to Phase 2
- Corrections: switch back to v12-epic-planner with corrections, re-run intake

---

## PHASE 2: PLAN

**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Run /epic-plan with Jane Street verification
INPUT: @docs/brain/$1/00-scope.md @docs/brain/$1/00-jane-street-baseline.md
PROTOCOL:
  1. Perform standard analysis and approach planning
  2. JANE STREET VERIFICATION (mandatory):
     - For each applicable principle from baseline:
       * Verify approach aligns with principle
       * Document how principle will be maintained
       * Flag any conflicts
  3. Document Jane Street compliance in approach
OUTPUT: Write docs/brain/$1/01-analysis.md and docs/brain/$1/02-approach.md
STOP at [PLAN-GATE] and do not proceed.
```

When v12-epic-planner outputs [PLAN-GATE], present:
- Key risk hotspots from 01-analysis.md
- Top 3 decisions from 02-approach.md
- Jane Street compliance summary

**GATE 2:**
> "Plan ready. Key decisions: [top 3]. Jane Street: [compliance summary]. Type APPROVED to proceed or provide feedback."

- APPROVED: advance to Phase 2.3
- Feedback: switch to v12-epic-planner, relay feedback, re-run plan

---

## PHASE 2.3: SCAN (SENTINEL AUDIT)

**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Run /epic-scan
INPUT: @docs/brain/$1/01-analysis.md @docs/brain/$1/02-approach.md
OUTPUT: Write docs/brain/$1/02-greptile-report.md
STOP at [SENTINEL-GATE] and do not proceed.
```

When v12-epic-planner outputs [SENTINEL-GATE], present Sentinel Verdict.

**GATE 2.3 (AUTONOMOUS):**
If verdict is PASSED: advance to Phase 2.5
If verdict is REVISION REQUIRED: switch to v12-epic-planner, relay scan results, re-run /epic-plan

---

## PHASE 2.5: INDEPENDENT ARCHITECTURAL REVIEW

**Switch to: v12-epic-planner mode (FRESH SESSION - independent reviewer)**

Hand off this exact task:
```
EPIC: $1
TASK: Independent architectural review
INPUT: @docs/brain/$1/01-analysis.md @docs/brain/$1/02-approach.md @docs/brain/$1/00-jane-street-baseline.md @docs/standards/JANE_STREET_DEVIATIONS.md
PROTOCOL:
  1. Review the approach document as if you did NOT write it
  2. JANE STREET COMPLIANCE AUDIT (mandatory):
     - determinism: Verify tick timestamps used (not system clocks)
     - cache_optimization: Verify struct arrays with direct indexing
     - one_in_flight: Verify two-phase FSM for order replacement
     - staleness_guard: Verify time tracking for feed staleness
     - defensive_initialization: Verify idempotent state machine setup
  3. Check for architectural anti-patterns:
     - Lock usage (BANNED)
     - Heap allocations in hot path
     - Blocking operations
     - Unicode strings (ASCII-only)
  4. Validate CYC targets are achievable
  5. Verify extraction plan minimizes caller impact
OUTPUT: Write docs/brain/$1/02-arch-review.md
STOP at [ARCH-REVIEW-GATE] and do not proceed.
```

When v12-epic-planner outputs [ARCH-REVIEW-GATE], present findings.

**GATE 2.5 (AUTONOMOUS):**
If verdict is APPROVED: advance to Phase 3
If verdict is REVISION REQUIRED: switch to v12-epic-planner, relay findings, re-run /epic-plan

---

## PHASE 3: VALIDATE

**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Run /epic-validate with Jane Street re-verification
INPUT: @docs/brain/$1/01-analysis.md @docs/brain/$1/02-approach.md @docs/brain/$1/02-arch-review.md
PROTOCOL:
  1. Apply standard validation checks
  2. Incorporate architectural review feedback
  3. Re-verify Jane Street compliance after changes
OUTPUT: Update 01-analysis.md and 02-approach.md in-place
STOP at [VALIDATE-GATE] and do not proceed.
```

When v12-epic-planner outputs [VALIDATE-GATE], present:
- Count of issues found (CRITICAL / SIGNIFICANT / MODERATE)
- Summary of changes made
- Jane Street re-verification status

**GATE 3:**
> "Validation complete. [N issues resolved]. Jane Street: [status]. Type GO to generate tickets or HOLD to review."

- GO: advance to Phase 4
- HOLD: wait for Director review, then switch back to v12-epic-planner to re-validate

---

## PHASE 4: TICKETS

**Switch to: v12-epic-planner mode**

Hand off this exact task:
```
EPIC: $1
TASK: Run /epic-tickets with Jane Street compliance per ticket
INPUT: @docs/brain/$1/02-approach.md @docs/brain/$1/00-jane-street-baseline.md
PROTOCOL:
  1. Generate ticket files as usual
  2. For EACH ticket, document:
     - Which Jane Street principles apply
     - How the ticket maintains compliance
     - Any principle-specific verification steps
OUTPUT: Write docs/brain/$1/ticket-XX-*.md for each ticket + EXECUTION_GUIDE.md
STOP at [TICKETS-GATE] and do not proceed.
```

When v12-epic-planner outputs [TICKETS-GATE], present:
- Total ticket count
- Ticket list with one-line scope per ticket
- Dependency order
- Estimated CYC reduction per ticket

**GATE 4:**
> "X tickets ready. [list]. Type RUN to begin execution or ADJUST to modify tickets."

- RUN: advance to Phase 4.5
- ADJUST: switch to v12-epic-planner, relay adjustments, regenerate affected tickets

---

## PHASE 4.5: INDEPENDENT TICKET QUALITY REVIEW

**Switch to: v12-epic-planner mode (FRESH SESSION - independent reviewer)**

Hand off this exact task:
```
EPIC: $1
TASK: Independent ticket quality review
INPUT: @docs/brain/$1/ticket-*.md @docs/brain/$1/EXECUTION_GUIDE.md @docs/brain/$1/00-jane-street-baseline.md
PROTOCOL:
  1. Review ALL tickets as if you did NOT write them
  2. Verify each ticket has:
     - Clear scope boundaries
     - Realistic CYC targets
     - Minimal caller impact
     - No logic duplication across tickets
     - Jane Street pattern compliance documented
  3. Check ticket dependencies are correct
  4. Validate execution order is safe
  5. JANE STREET TICKET AUDIT (per ticket):
     - Verify applicable principles are documented
     - Verify compliance verification steps are clear
     - Flag any missing principle coverage
OUTPUT: Write docs/brain/$1/04-ticket-review.md
STOP at [TICKET-REVIEW-GATE] and do not proceed.
```

When v12-epic-planner outputs [TICKET-REVIEW-GATE], present findings.

**GATE 4.5 (AUTONOMOUS):**
If verdict is APPROVED: advance to Execution Pipeline
If verdict is REVISION REQUIRED: switch to v12-epic-planner, relay findings, regenerate affected tickets

---

## EXECUTION PIPELINE (Per-Ticket PR Loop)

For each ticket listed in docs/brain/$1/EXECUTION_GUIDE.md (in dependency order):

---

### TICKET LOOP START

**Step A -- Status report (you generate this, no mode switch needed):**
```
[EPIC-TDD] $1 -- Progress
Completed : [N of M tickets] (PRs: [list of PR numbers])
Current   : ticket-XX-[name]
Remaining : [list]
```

**Step A.1 -- Switch to: Advanced mode (branch creation)**

Hand off this exact task:
```
BRANCH TASK:
1. git checkout main
2. git pull origin main
3. git checkout -b $1-ticket-XX
4. Report the new branch name.
```

**Step B -- Switch to: v12-engineer mode**

Hand off this exact task:
```
EPIC: $1
TASK: Run /ticket with Jane Street pre-check
INPUT: @docs/brain/$1/ticket-XX-[name].md @docs/brain/$1/00-jane-street-baseline.md
PROTOCOL:
  1. JANE STREET PRE-CHECK (mandatory before planning):
     - Read applicable principles from ticket
     - Verify current code state against principles
     - Document baseline compliance
  2. Read ticket completely
  3. Write the extraction plan with:
     - sub-method names and signatures
     - caller impact
     - CYC before/after estimate
     - Jane Street compliance maintenance steps
STOP at [TICKET-GATE]. Do not write any code yet.
```

When v12-engineer outputs [TICKET-GATE], present plan summary.

**MINI-GATE:**
> "Ticket plan ready: [2-line summary]. Jane Street: [compliance steps]. Type APPROVED to execute or FLAG to adjust."

- APPROVED: switch back to v12-engineer and instruct it to execute the plan
- FLAG: relay adjustment, switch to v12-engineer to re-plan

**Step B.5 -- Switch to: v12-epic-planner mode (FRESH SESSION - independent code reviewer)**

After v12-engineer reports implementation complete, switch to v12-epic-planner for independent review:
```
EPIC: $1
TASK: Independent implementation review for ticket-XX
INPUT: @docs/brain/$1/ticket-XX-[name].md @docs/brain/$1/00-jane-street-baseline.md
PROTOCOL:
  1. Read the ticket specification
  2. Use jCodemunch to examine the actual implementation:
     - get_file_outline for modified files
     - get_symbol_source for new methods
     - get_blast_radius for impact analysis
  3. Verify implementation matches ticket spec:
     - All sub-methods created as specified
     - CYC targets achieved
     - No logic drift
  4. JANE STREET IMPLEMENTATION AUDIT (mandatory):
     - determinism: Verify no DateTime.Now or system clock usage
     - cache_optimization: Verify struct arrays, no pointer chasing
     - one_in_flight: Verify FSM state transitions are atomic
     - staleness_guard: Verify time tracking is present
     - ASCII-only: Verify no Unicode strings
  5. Check for violations:
     - No new lock() statements (grep verification)
     - FSM/Actor pattern used correctly
     - No heap allocations in hot path
OUTPUT: Write docs/brain/$1/ticket-XX-impl-review.md
STOP at [IMPL-REVIEW-GATE] and do not proceed.
```

When v12-epic-planner outputs [IMPL-REVIEW-GATE], present findings.

**IMPL-REVIEW GATE (AUTONOMOUS):**
If verdict is APPROVED: advance to Step C
If verdict is REVISION REQUIRED: switch to v12-engineer, relay findings, re-implement

**Step C -- Switch to: Advanced mode (verification + Jane Street audit)**

After implementation review passes, switch to Advanced mode:
```
VERIFICATION TASK for epic $1, ticket-XX
Run the FULL pre-push validation suite with Jane Street audit:

1. powershell -File .\scripts\pre_push_validation.ps1

2. JANE STREET FINAL AUDIT (MANDATORY):
   - Read: docs/standards/JANE_STREET_DEVIATIONS.md
   - Read: docs/brain/$1/00-jane-street-baseline.md
   - Verify no new violations of documented Jane Street principles:
     * Zero locks: grep -r "lock(" src/ (must return zero matches)
     * ASCII-only: Already checked in validation
     * determinism: No DateTime.Now usage (grep verification)
     * cache_optimization: Struct arrays used (code inspection)
     * one_in_flight: FSM pattern followed (code inspection)
     * staleness_guard: Time tracking present (code inspection)
   - If new Jane Street conflicts detected: HALT and report

Report results as:
  ASCII Gate      : PASS / FAIL
  Build           : PASS / FAIL
  Unit Tests      : PASS / FAIL
  Lint            : PASS / FAIL
  Formatting      : PASS / FAIL
  Security        : PASS / FAIL (warnings OK)
  Markdown Links  : PASS / FAIL (warnings OK)
  PR Hygiene      : PASS / FAIL
  Complexity (≤15): PASS / FAIL
  Dead Code       : PASS / FAIL (warnings OK)
  Codacy Preview  : PASS / FAIL (warnings OK)
  Semgrep         : PASS / FAIL (warnings OK)
  CodeRabbit AI   : PASS / FAIL (warnings OK)
  Jane Street DNA : PASS / FAIL

If ANY blocking check fails: HALT and report to orchestrator.
```

If Advanced mode reports any FAIL: HALT. Report to Director. Do not continue.

**Step D -- F5 Gate (Director's ONLY manual action):**
Output:
```
[F5-GATE] Ticket XX -- All automated gates PASSED
deploy-sync     : PASS
CYC             : [before] -> [after]
lock() audit    : CLEAN
Jane Street DNA : PASS

ACTION REQUIRED: Press F5 in NinjaTrader IDE.
When you see the BUILD_TAG banner, type: F5 done [BUILD_TAG]
```

Wait for Director input.

**Step E -- Switch to: Advanced mode (auto-commit)**

After Director types "F5 done [BUILD_TAG]", switch to Advanced mode:
```
COMMIT TASK:
Run: git add -A
Run: git commit -m "[$1] ticket-XX: [short description] -- CYC [before]->[after] [BUILD_TAG]"
Run: git push origin $1-ticket-XX
Report the commit hash and current branch name.
```

**Step F -- Switch to: Advanced mode (PR submission)**

After commit is pushed, switch to Advanced mode:
```
PR SUBMISSION TASK:
1. gh pr create --title "[$1] ticket-XX: [name]" --body "Automated PR for ticket XX of epic $1. See docs/brain/$1/ticket-XX-*.md for audit trail." --label "epic-tdd"
2. Extract the <PR_NUMBER> from the `gh pr create` output.
3. Emit: [PR-SUBMITTED] PR #<PR_NUMBER>
```

When Advanced mode outputs [PR-SUBMITTED] PR #<PR_NUMBER>, advance to Step G.

**Step G -- Switch to: v12-epic-planner mode (FRESH SESSION - per-ticket validator)**

After PR is submitted, switch to v12-epic-planner for per-ticket validation:
```
EPIC: $1
TASK: Per-ticket validation for ticket-XX
INPUT: @docs/brain/$1/ticket-XX-*.md @docs/brain/$1/00-jane-street-baseline.md
PROTOCOL:
  1. Review this ticket's implementation end-to-end
  2. Use jCodemunch to verify:
     - get_changed_symbols for this ticket only
     - search_text for "lock(" in modified files
     - search_text for "DateTime.Now" in modified files
  3. Validate against ticket spec:
     - All objectives achieved
     - CYC targets met
     - No scope creep
  4. JANE STREET TICKET COMPLIANCE AUDIT:
     - Verify applicable principles maintained
     - Check implementation evidence
     - Document compliance status
  5. Check for technical debt:
     - No TODO comments added
     - No disabled tests
     - No suppressed warnings
OUTPUT: Write docs/brain/$1/ticket-XX-final-validation.md
STOP at [TICKET-VALIDATION-GATE] and do not proceed.
```

When v12-epic-planner outputs [TICKET-VALIDATION-GATE], present findings.

**GATE G (AUTONOMOUS):**
If verdict is TICKET READY: advance to Step H
If verdict is REVISION REQUIRED: HALT and report to Director (manual decision required)

**Step H -- Switch to: Orchestrator mode (/pr-loop)**

After ticket validation passes, trigger perfection loop:
```
EPIC: $1
TASK: Run /pr-loop <PR_NUMBER>
GOAL: Drive PR to 100/100 PHS.
STOP when /pr-loop outputs [PHS-PERFECT].
```

**Step I -- Ticket Complete:**
Output:
```
[TICKET-COMPLETE] $1 / ticket-XX
PR #<PR_NUMBER> : 100/100 PHS (PERFECT)
CYC delta       : [before] -> [after]
Files modified  : [list]
Jane Street     : COMPLIANT
```

**Step J -- Loop Decision:**
> "Ticket XX complete. [M tickets remaining]. Type NEXT to continue or STOP to end epic."

- NEXT: return to TICKET LOOP START with next ticket
- STOP: advance to EPIC COMPLETE

### TICKET LOOP END

---

## EPIC COMPLETE

Output the full summary (you generate this directly, no mode switch):
```
[EPIC-COMPLETE] $1
============================================================
Tickets completed : [N of N]
PRs created       : [list of PR numbers with 100/100 PHS]
Total CYC delta   : [before total] -> [after total]
Sub-methods added : [full list]
Files modified    : [list]

DNA Audit
  deploy-sync     : ALL PASS
  lock() audit    : ALL CLEAN
  Unicode audit   : ALL CLEAN
  CYC floor       : ALL targets below 20

Jane Street Compliance
  determinism     : MAINTAINED
  cache_optimization : APPLIED
  one_in_flight   : VERIFIED
  staleness_guard : PRESENT
  [other principles from baseline]

Independent Reviews Completed: 9 per ticket
  - Jane Street Baseline Audit (Phase 0)
  - Sentinel Audit (Phase 2.3)
  - Architectural Review (Phase 2.5)
  - Ticket Quality Review (Phase 4.5)
  - Implementation Review per ticket (Step B.5)
  - Verification per ticket (Step C)
  - Per-Ticket Validation (Step G)

All PRs ready for independent review and merge.
============================================================
