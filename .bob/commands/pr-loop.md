# pr-loop

description: Repeatable 100/100 Perfection Loop V2. Iteratively repairs and verifies code until the Project Health Score is 100/100. Includes mandatory Bot Forensics extraction before fixes.

## Usage

```
/pr-loop <PR_NUMBER>
```

**Example:**
```
/pr-loop 6
```

## Protocol

You are the V12 Perfection Orchestrator. You MUST NOT STOP until PHS is 100/100.

### ORCHESTRATION RULES

- **SCORE 100 MANDATE**: You are BANNED from merging or ending the loop if PHS < 100.
- **HYGIENE GATE**: You MUST pass Step 0 (Clean Branch & Diff Size) before every push.
- **FORENSICS FIRST**: You MUST extract bot findings (Step 1) before any fix attempts.
- **LOCAL FIRST**: You must achieve Local Score 13/13 before every push.
- **FORENSIC AUDIT**: Every failure must be categorized as [VALID], [HALLUCINATION], [INFRA-NOISE], or [ACCESS_BLOCKED].
- **F5 GATE**: The only manual action is the final NinjaTrader verification at Score 100.
- **OKF MANDATORY**: Query `python scripts/query_kb.py "<issue>"` for any Jane Street pattern question before fixing.

---

## THE PERFECTION CYCLE

### Step -1: PR Existence Check

**Switch to: agent mode**

Hand off:
```
TASK: Check if PR Already Exists
PR: $1
PROTOCOL:
  1. Check current branch: git branch --show-current
     - Note the branch name for use in subsequent steps.

  2. Check if PR exists: gh pr view $1 --json headRefName --jq '.headRefName'

  3. If PR exists (exit code 0):
     - Extract branch name from output.
     - If not already on that branch: git checkout <branch_name>
     - Emit: [PR-EXISTS] PR #$1 exists, branch: <branch_name>
     - Proceed to Step 1 (Pre-Flight Hygiene)

  4. If PR doesn't exist (exit code 1):
     - Emit: [PR-NEW] PR #$1 does not exist yet
     - Proceed to Step 0 (create branch and PR)
```

**Gate:**
- If PR exists: Proceed to Step 1
- If PR doesn't exist: Proceed to Step 0

---

### Step 0: Pre-Flight Hygiene

**Switch to: agent mode**

Hand off:
```
TASK: Verify PR Hygiene
PR: $1
PROTOCOL:
  IF PR is new (from Step -1):
    1. Create branch: git checkout -b <epic-slug>-pr
    2. Run `bash scripts/verify_pr_hygiene.ps1` (or the .sh equivalent on Linux).

  IF PR already exists (from Step -1):
    1. Verify on correct branch: git branch --show-current
    2. Pull latest: git pull origin <pr-branch-name> --rebase
    3. Run `powershell -File .\scripts\verify_pr_hygiene.ps1`.

  4. If FAIL: HALT and report the violation (e.g. "Diff > 10k" or "Branch is dirty").
  5. If PASS: Emit [HYGIENE-PASS] and advance to Step 1.
```

---

### Step 1: Bot Forensics + OKF Audit (MANDATORY)

**Switch to: agent mode**

Hand off:
```
TASK: Extract and Categorize Bot Findings with OKF Alignment Review
PR: $1
PROTOCOL:
  1. Run: powershell -File .\scripts\extract_pr_forensics.ps1 -PrNumber $1
  2. Read the generated forensics report: docs/brain/pr_$1_forensics.md
  3. OKF AUDIT (MANDATORY):
     - Read: docs/intel/jane-street/index.md
     - For each applicable issue, query: python scripts/query_kb.py "<issue keyword>"
     - For each VALID issue, check if it conflicts with OKF patterns
     - Categorize as:
       * [VALID-FIX]: Issue aligns with Jane Street OKF principles - must fix
       * [VALID-SUPPRESS]: Issue conflicts with Jane Street OKF - suppress via .codacy.yml
       * [HALLUCINATION]: Bot error - log and ignore
       * [INFRA-NOISE]: Infrastructure issue - ignore
  4. Present summary to Director:
     - Total VALID-FIX issues (P0/P1/P2 breakdown)
     - Total VALID-SUPPRESS issues (with OKF rationale)
     - Hallucinations detected
     - INFRA-NOISE filtered
  5. If P0 VALID-FIX issues exist: Flag as CRITICAL and proceed to Step 2.
  6. If only VALID-SUPPRESS issues: Update .codacy.yml, document in docs/intel/jane-street/
  7. If no VALID issues: Skip to Step 3 (verification only).
  8. Emit: [FORENSICS-READY] X VALID-FIX, Y VALID-SUPPRESS, Z hallucinations
```

**Outputs:**
- `docs/brain/pr_$1_forensics.md` - Full categorized findings
- `docs/brain/pr_$1_fix_queue.md` - Priority-ordered fix list (VALID-FIX only)
- `docs/brain/pr_$1_suppress_queue.md` - Suppression list (VALID-SUPPRESS with OKF rationale)
- `docs/brain/bot_hallucinations.md` - Updated hallucination log

**Gate:** Review forensics report. If P0 VALID-FIX issues exist, they MUST be fixed before proceeding. If VALID-SUPPRESS issues exist, they MUST be documented before proceeding.

---

### Step 1.5: Logic Bug Repair Plan (CONDITIONAL)

**Trigger:** Only when `pr_$1_fix_queue.md` contains items tagged `[LOGIC-BUG]`.

**Switch to: v12-phase2-architecture mode** via `start_subtask`

Hand off:
```
TASK: Logic Bug Repair Plan for PR #$1
INPUT: @docs/brain/pr_$1_fix_queue.md (LOGIC-BUG items only)
OKF: Run python scripts/query_kb.py for each bug's domain before planning.
     - Clock/time bugs        → query_kb.py "deterministic clock"
     - Correctness bugs       → query_kb.py "correctness by construction"
     - Concurrency bugs       → query_kb.py "lock-free patterns"
     - State machine bugs     → query_kb.py "FSM extraction"
PROTOCOL:
  For each [LOGIC-BUG] item:
    1. Read the relevant source lines (provided in fix_queue)
    2. Query OKF for applicable pattern
    3. Write repair spec:
       - Root cause (one sentence)
       - Exact old code (copy from source)
       - Exact new code (the fix)
       - Jane Street rationale (cite OKF document + rule)
       - Edge cases to verify
       - CYC delta (must stay ≤ 8)
OUTPUT: docs/brain/wave7-pr-repairs/PR-$1/plan.md
STOP when plan.md is written. Do not touch src/.
```

---

### Step 2: Local Repair (VALID-FIX) + Suppression (VALID-SUPPRESS)

**Switch to: v12-engineer mode** via `start_subtask`

Hand off:
```
TASK: Fix VALID-FIX Issues and Document VALID-SUPPRESS Issues
INPUT: @docs/brain/pr_$1_fix_queue.md
       @docs/brain/pr_$1_suppress_queue.md
       @docs/brain/wave7-pr-repairs/PR-$1/plan.md  (if exists — logic bugs only)
BRANCH: <branch for PR #$1> — checkout before editing
PROTOCOL:
  PART A: Logic Bug Fixes (if plan.md exists)
    1. Read plan.md completely.
    2. For each logic bug: apply the exact old→new from plan.md.
    3. Verify locally after each fix (compile, spot-check).

  PART B: Mechanical Fixes (remaining VALID-FIX items)
    1. Read fix queue for non-LOGIC-BUG items.
    2. For each item (P0 first, then P1, then P2):
       - Apply fix
       - Verify locally
       - Mark as [x] FIXED in fix queue

  PART C: Jane Street Suppressions (VALID-SUPPRESS)
    1. For each VALID-SUPPRESS issue:
       - Add file/pattern to .codacy.yml with OKF rationale
       - Document in docs/intel/jane-street/ as a new deviation note
       - Mark as [x] SUPPRESSED in suppress queue

  PART D: Validation
    1. Run: dotnet csharpier format src/
    2. Run: powershell -File .\scripts\pre_push_validation.ps1
       (13 checks — ASCII, Build, Tests, Lint, Formatting, Security,
        Markdown Links, PR Hygiene, Complexity ≤8, Dead Code,
        Codacy Preview, Semgrep, CodeRabbit AI)
    3. If ANY blocking check fails: identify, fix, re-run validation.
    4. If ALL checks pass: emit [LOCAL-READY] with fix summary.
    5. Write: docs/brain/wave7-pr-repairs/PR-$1/completion.md
```

**Gate:** ALL local blocking checks PASS (9/13). If any blocking check fails, repeat Step 2.

---

### Step 3: Global Push & Monitor

**Switch to: agent mode**

Hand off:
```
TASK: Push and Monitor PR #$1
PROTOCOL:
  1. powershell -File .\deploy-sync.ps1  (syncs NT8 hard links — MANDATORY before push)
  2. git add -A
  3. git commit -m "fix(wave7/pr$1): bot review repairs — OKF-aligned"
  4. git push --force-with-lease origin <branch>
  5. Wait for bots — sleep 300s first check, 180s subsequent checks.
  6. Run: powershell -File .\scripts\calculate_fleet_score.ps1 -PrNumber $1
  7. If Score < 100: emit [PHS-RETRY] Current: X/100.
  8. If Score = 100: emit [PHS-PERFECT] 100/100.
```

**Gate:**
- If [PHS-RETRY]: **RESTART at Step 1** (re-extract forensics for new findings).
- If [PHS-PERFECT]: **Advance to Step 4**.

---

### Step 4: Manual Override Gate

**Mode:** Orchestrator
**Trigger:** PHS < 100 after 3+ iterations

**Protocol:**
1. Present current PHS and remaining issues to Director.
2. Classify remaining issues:
   - VALID but low-priority (P2 style issues)
   - Hallucinations not yet logged
   - INFRA-NOISE
3. Ask Director: "PHS is X/100. Remaining issues: [list]. Approve merge? (YES/NO/DEFER)"

**Director Options:**
- **YES**: Proceed to Step 5 (F5 Gate)
- **NO**: Provide guidance, restart at Step 1
- **DEFER**: Create follow-up ticket, proceed to Step 5

---

### Step 5: Final F5 Verification

**Mode:** Orchestrator
**Action:** Director presses F5 in NinjaTrader

Output:
```
[F5-GATE] PR #$1 - PHS <SCORE>/100
All automated gates: PASSED/APPROVED
Remaining issues: [list if <100]

ACTION REQUIRED: Press F5 in NinjaTrader IDE.
When you see the BUILD_TAG banner, type: F5 done [BUILD_TAG]
```

**Gate:** Wait for Director confirmation.

---

## FINAL HANDSHAKE

Once 100/100 is achieved (or Director approves <100), output:

```
[PHS-PERFECT] PR #$1 - Ready for Merge
============================================================
PHS Score       : <SCORE>/100
VALID Issues    : <COUNT> (all fixed or approved)
Hallucinations  : <COUNT> (logged)
INFRA-NOISE     : <COUNT> (ignored)

Commits: [list of hashes]
============================================================
Branch ready for merge. Awaiting F5 verification.
```

---

## V3 Changes (V12.35 — OKF + Standard Git)

| Aspect | V2 (Old) | V3 (New) |
|--------|----------|----------|
| Mode name | "Advanced mode" (removed) | `agent` mode |
| Branch workflow | GitButler virtual branches | Standard `git checkout` |
| KB references | Firebase/Firestore | Local OKF (`docs/intel/jane-street/`) |
| KB query | External API | `python scripts/query_kb.py` |
| Logic bug handling | Engineer acts cold | Step 1.5 planner via `start_subtask` |
| Complexity threshold | CYC ≤ 15 | CYC ≤ 8 (Jane Street strict) |
| Validation checks | 14/14 (phantom Check #14) | 13/13 (actual pre_push_validation.ps1) |
| Suppression docs | `docs/standards/JANE_STREET_DEVIATIONS.md` | `docs/intel/jane-street/` OKF |

---

## Reference Documentation

- OKF Index: `docs/intel/jane-street/index.md`
- Full protocol: `docs/protocol/PR_LOOP_V2.md`
- Pre-push validation: `scripts/pre_push_validation.ps1`
- Forensics extraction: `scripts/extract_pr_forensics.ps1`
