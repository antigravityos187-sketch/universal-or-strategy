# pr-loop

description: PR Review & Repair Loop V5. Iteratively triages bot findings, repairs confirmed issues, and verifies until all bots are green. Uses poll_all_bots.py for OKF-filtered 8-bot signal extraction. Used both manually (single PR) and as the inner loop of Phase 7 wave-orch-phase7-lane workers.

## Usage

```
/pr-loop <PR_NUMBER>
```

**Example:**
```
/pr-loop 20
```

## Machine-callable contract (Phase 7 workers)

Input (from start_subtask message):
```
PR_NUMBER: <number>
BRANCH: <wave7/prN-...>
CLUSTER: <cluster name>
MAX_ROUNDS: 3  (default)
```

Output (emit at end):
```
LANE_COMPLETE L<N> PR#<N> status=(MERGED_READY|NEEDS_DIRECTOR) findings=<N>_fixed
```
or on failure:
```
LANE_HARD_FAILURE L<N> PR#<N> reason=<one-line>
```

---

## ORCHESTRATION RULES

- **TRIAGE FIRST**: Extract and classify ALL bot findings before touching any code.
- **LOGIC BUGS**: ALWAYS route through v12-phase2-architecture planner first. Never free-hand a logic fix.
- **MECHANICAL/DNA**: Apply directly — no planner needed. One commit per category.
- **GATE BEFORE PUSH**: `python3 scripts/wave7_prepush_gate.py --base origin/main` must PASS.
- **BUILD BEFORE PUSH**: `dotnet build Linting.csproj` must be 0 errors.
- **BRANCH HYGIENE**: src/ edits → PR branch. Docs/artifacts → main. Never mixed.
- **MAX 3 ROUNDS**: If bots still find VALID issues after 3 repair rounds → LANE_HARD_FAILURE.

---

## THE REPAIR CYCLE

### Step 0: Branch Setup

```
BRANCH HYGIENE RULES (read before touching anything):
  - git checkout {BRANCH} for ALL src/ edits and git push
  - NEVER commit docs/, scripts/, .bob/, .graphify/ to {BRANCH}
  - Artifact files go to docs/brain/wave7-pr-repairs/PR-{N}/ on main
  - One concern per commit — never mix src/ and docs/ changes

SETUP:
  1. git checkout {BRANCH}
  2. git fetch origin {BRANCH} && git status
     - If diverged: git pull --rebase origin {BRANCH}
  3. Record HEAD SHA: git rev-parse HEAD > /tmp/pr{N}_baseline_sha.txt
```

---

### Step 1: Bot Forensics (MANDATORY — before any code change)

```
EXTRACT BOT FINDINGS (V5 — use poll_all_bots.py, NOT raw gh pr view):
  python3 scripts/poll_all_bots.py {PR_NUMBER} \
    --repo antigravityos187-sketch/universal-or-strategy \
    > /tmp/pr{PR_NUMBER}_bot_poll.json

  The script outputs structured JSON with:
    - findings[]         : list of {bot, severity, body, okf_override}
    - satisfaction_score : N/5 (counts: CodeRabbit, Gemini, Cubic, Sourcery, CodeAnt)
    - all_green          : true if satisfaction_score == 5
    - okf_overrides[]    : findings auto-classified INFORMATIONAL by OKF policy

  OKF auto-overrides (never fix, log as INFORMATIONAL):
    - #nullable enable suggestions   -> OKF: not applicable in V12 lock-free model
    - lock() suggestions             -> HALT + LANE_HARD_FAILURE (escalate to Director)
    - NUnit/MSTest suggestions       -> xUnit only; but if existing tests pass, INFORMATIONAL
    - PR size warnings               -> INFRA-NOISE
    - deploy-sync.ps1 not in PR      -> INFRA-NOISE

  If all_green == true  -> skip to Step 4 (push already done, poll again to confirm)
  If all_green == false -> parse findings[] for VALID items, proceed to classification

CLASSIFY each finding as exactly one of:
  VALID-LOGIC-BUG   Behavior change, wrong output, data corruption,
                    timezone mismatch, wrong dict key, state regression.
                    EVIDENCE: multiple bots OR code trace confirms it.

  VALID-MECHANICAL  Naming violation, redundant code, dead guard,
                    O(N) where O(1) exists, unreachable branch.

  VALID-DNA         ASCII violation, DateTime.Now (should be UtcNow),
                    underscore local variable, lock() usage,
                    NUnit/MSTest (must be xUnit).

  HALLUCINATION     Contradicts actual code behavior.
                    VERIFY: read_file the affected line before classifying.
                    If code matches bot's claim -> NOT a hallucination.

  INFRA-NOISE       deploy-sync.ps1 not in PR, PR size warning,
                    process notes, "consider adding tests" suggestions.

WRITE: docs/brain/wave7-pr-repairs/PR-{N}/triage.md
  Format:
    ## Triage -- PR #{N} -- {BRANCH}
    | Finding | Bot(s) | Class | Notes |
    |---------|--------|-------|-------|
    | ...     | ...    | ...   | ...   |

    ### Fix Queue (ordered P0 first)
    - [ ] VALID-LOGIC-BUG: <description>
    - [ ] VALID-MECHANICAL: <description>
    - [ ] VALID-DNA: <description>

EMIT: TRIAGE_DONE PR#{N} logic=X mech=Y dna=Z hall=A noise=B okf_overrides=C
```

**Gate:** If zero VALID findings -> skip to Step 5 (poll). PR is already clean.

---

### Step 1.5: Logic Bug Planner (if VALID-LOGIC-BUG findings exist)

```
For EACH VALID-LOGIC-BUG finding:

  start_subtask(
    mode="v12-phase2-architecture",
    title="PR#{N} Logic-Bug Plan: {finding_summary}",
    message="
      TASK: Plan a minimal targeted fix for this confirmed logic bug.
      Branch: {BRANCH}  PR: #{N}
      Bug: {finding_description}
      Affected file/line: {file}:{line}

      PROTOCOL:
        1. Read the affected source (read_file).
        2. Confirm the bug is real (do not plan if hallucination).
        3. Produce the minimal fix: exact old_text and new_text.
           Max scope: the single method containing the bug + its immediate callers.
           No scope creep. No refactoring beyond the fix.
        4. State: CYC delta (must be 0 or negative).
        5. Output format:
             BUG_CONFIRMED: yes/no
             FILE: src/...
             OLD: <exact lines to replace>
             NEW: <replacement lines>
             CYC_DELTA: 0
      OKF: docs/intel/jane-street/how-to-build-an-exchange.md
    "
  )

  Validate plan: BUG_CONFIRMED=yes, CYC_DELTA<=0, scope is single method.
  If BUG_CONFIRMED=no: reclassify as HALLUCINATION, skip fix.
```

---

### Step 2: Repairs (on {BRANCH})

```
LOGIC BUG FIXES (one start_subtask per bug, sequential):
  start_subtask(
    mode="v12-engineer",
    title="PR#{N} Fix: {finding_summary}",
    message="
      Apply this exact fix. Branch: {BRANCH}. PR: #{N}.
      BRANCH HYGIENE: ONLY modify src/ files. No docs/, .bob/, scripts/.
      Fix:
        FILE: {file}
        OLD: {old_text_from_plan}
        NEW: {new_text_from_plan}
      After applying:
        1. dotnet build Linting.csproj -- must be 0 errors
        2. python3 -c 'data=open(\"{file}\",\"rb\").read(); bad=[i for i in range(len(data)) if data[i]>127]; print(\"ASCII OK\" if not bad else \"NON-ASCII: \"+str(bad[:3]))'
        3. git add {file}
        4. git commit -m 'fix(wave7/pr{N}): {description}'
      Report: REPAIR_DONE {file} build=PASS ascii=PASS
    "
  )

MECHANICAL / DNA FIXES (apply directly, no start_subtask needed):
  Rules (apply in order, one commit per category):
    - DateTime.Now -> DateTime.UtcNow  (only in newly added diff lines)
    - Unicode/em-dash -> ASCII (--)    (only in src/ files)
    - _localVar -> localVar            (only vars introduced in this PR's diff)
    - Redundant Contains -> remove     (verify inner helper handles the miss case)
    - lock() -> HALT, escalate to Director  (NEVER auto-fix lock)

  After each category:
    dotnet build Linting.csproj -- 0 errors required
    git add {changed_files}
    git commit -m "fix(wave7/pr{N}): DNA compliance -- {what}"
```

---

### Step 3: Local Gate

```
Run gate (MUST PASS before push):
  python3 scripts/wave7_prepush_gate.py --base origin/main

Expected output: "GATE PASSED. Ready to push."

If GATE FAILED:
  - Read each violation carefully
  - WARN (diff size raw>120k) -> acceptable if stripped<150k
  - FAIL (any check) -> fix the violation, rebuild, re-run gate
  - Max 3 gate iterations before escalating

Final build check:
  dotnet build Linting.csproj
  Must show: "Build succeeded. 0 Warning(s). 0 Error(s)."
```

---

### Step 4: Push

```
git push origin {BRANCH}

Verify exit code 0 and GitHub URL in output.
If pre-push hook blocks: read hook output, fix the issue, retry.
```

---

### Step 5: Bot Poll (wait for re-review)

```
POLLING PROTOCOL V5 (4-minute intervals -- cost-optimized):
  Use poll_all_bots.py for EVERY poll -- NOT raw gh pr checks.

  Round 1: sleep 240s (4 min), then:
    python3 scripts/poll_all_bots.py {PR_NUMBER} \
      --repo antigravityos187-sketch/universal-or-strategy \
      > /tmp/pr{PR_NUMBER}_poll_r1.json
  Round 2: sleep 240s, then: (same command -> _r2.json)
  Round 3: sleep 240s, then: (same command -> _r3.json)
  Round 4: sleep 240s, then: (same command -> _r4.json)
  Round 5: sleep 240s, then: (same command -> _r5.json)  (20 min total)

On each poll result (read .all_green and .satisfaction_score):
  all_green == true  OR satisfaction_score == 5  -> proceed to Step 6
  PENDING (bots not yet reviewed)                -> wait next interval
  NEW VALID FINDINGS (not in previous triage)    -> loop back to Step 1 (increment round counter)
  SAME FINDINGS REPEATING after fix              -> classify as HALLUCINATION, document, proceed

SATISFACTION THRESHOLD:
  5/5 bots green = LANE_COMPLETE status=MERGED_READY
  4/5 green (1 INFRA-NOISE or deferred) = LANE_COMPLETE status=MERGED_READY (log exception)
  <4/5 green with VALID findings = continue repair loop

ROUND COUNTER:
  Round 1 (first pass): normal
  Round 2 (one loop): log extra_round=1
  Round 3 (two loops): log extra_round=2, last chance
  Round 4+: emit LANE_HARD_FAILURE (max 3 repair rounds exceeded)
```

---

### Step 6: Write Artifacts (on main -- NOT on PR branch)

```
git checkout main
git pull origin main  (ensure up to date)

Write docs/brain/wave7-pr-repairs/PR-{N}/repair-log.md:
  ## Repair Log -- PR #{N} -- {BRANCH}
  **Completed**: {ISO timestamp}
  **Rounds**: {N}
  **Gate result**: PASS ({raw}raw/{stripped}stripped)

  ### Findings
  | Finding | Class | Fix Applied | Commit SHA |
  ...

  ### Bot Re-review Result
  All bots: GREEN / Remaining: {list if any}

Write docs/brain/wave7-pr-repairs/PR-{N}/completion.md:
  ## Completion Report -- PR #{N}
  **Status**: MERGED_READY / NEEDS_DIRECTOR
  **Remaining issues**: {list or none}
  **Director action required**: {description or none}

git add docs/brain/wave7-pr-repairs/PR-{N}/
git commit -m "docs(wave7/pr{N}): Phase 7 repair log + completion report"
git push origin main
```

---

### Step 7: Report

```
Emit one of:
  LANE_COMPLETE L{N} PR#{PR} status=MERGED_READY findings={X}_fixed
  LANE_COMPLETE L{N} PR#{PR} status=NEEDS_DIRECTOR findings={X}_fixed reason={why}
  LANE_HARD_FAILURE L{N} PR#{PR} reason={one-line description}
```

---

## Classification Quick Reference

| Signal | Class | Auto-fix? |
|--------|-------|-----------|
| Wrong dict key, wrong prefix length | VALID-LOGIC-BUG | Plan first |
| State regression after extraction | VALID-LOGIC-BUG | Plan first |
| Timezone mismatch (Now vs UtcNow) | VALID-DNA | Direct |
| Unicode in source (em-dash etc) | VALID-DNA | Direct |
| Underscore local variable | VALID-DNA | Direct |
| O(N) Contains where O(1) exists | VALID-MECHANICAL | Direct |
| lock() found | VALID-DNA | HALT -- Director |
| NUnit/MSTest usage | VALID-DNA | Direct |
| "deploy-sync.ps1 not in PR" | INFRA-NOISE | Skip |
| "PR too large" | INFRA-NOISE | Skip |
| "consider adding tests" | INFRA-NOISE | Skip |
| Claims bug that doesn't exist in code | HALLUCINATION | Skip + log |

---

## V5 Changes from V4

| Aspect | V4 | V5 |
|--------|----|----|
| Bot signal extraction | `gh pr view --json reviews,comments` | `scripts/poll_all_bots.py` (8-bot triage, OKF-filtered) |
| Poll step | `gh pr checks {N}` | `scripts/poll_all_bots.py` with JSON output |
| OKF auto-overrides | Manual classification | Built into poll_all_bots.py (nullable, NUnit, PR size) |
| Satisfaction threshold | Not defined | 5/5 bots = MERGED_READY; 4/5 acceptable with logged exception |
| Branch model | Standard git checkout | Standard git checkout (unchanged) |
| Gate tool | wave7_prepush_gate.py (5 checks) | wave7_prepush_gate.py (6 checks -- added Check 0 CS-only) |
| Machine contract | LANE_COMPLETE / LANE_HARD_FAILURE | Unchanged |
| Step 1.5 logic planner | Retained, formalized | Unchanged |
| Artifact location | main-only | main-only (unchanged) |
| Poll interval | 4 min uniform | 4 min uniform (unchanged) |
| Max rounds | 3 (then LANE_HARD_FAILURE) | 3 (unchanged) |
