---
name: epic-validate
description: >-
  Phase 3 - DNA audit of the architecture plan. Subagent reads
  02-architecture-plan.md, verifies V12 DNA compliance using jCodemunch,
  writes 03-audit-report.md. No scripts, no python manifest calls.
metadata:
  user-invocable: true
  disable-model-invocation: true
  argument-hint: <epic-id>
---

# PHASE 3: DNA & PR AUDIT
**Epic ID:** EPIC-W7-NNN (passed by orchestrator)
**Mode:** `v12-phase3-audit`
**Protocol:** V12.28 Bob IDE V2 — Independent Subagent

> You are an Architect who stress-tests the refactoring plan for V12 DNA compliance.
> You do NOT touch src/ files.
> You read 02-architecture-plan.md, audit it, write 03-audit-report.md.

---

## STEP 1 — READ INPUTS

Read using `read_file`:
1. `docs/brain/EPIC-W7-NNN/00-scope.md`
2. `docs/brain/EPIC-W7-NNN/01-scope-boundary.md`
3. `docs/brain/EPIC-W7-NNN/02-architecture-plan.md`

Use `get_file_outline` on each target file to confirm live code matches the plan.

---

## STEP 2 — V12 DNA COMPLIANCE AUDIT

### 2a — CYC Reduction Viability
- `get_symbol_complexity` on target method — confirm current CYC
- Verify the extraction plan will reduce residual to ≤ 8
- Each extracted method achieves CYC ≤ 8

### 2b — Lock-Free Compliance
- `search_ast { "pattern": "call:lock" }` on target files
- Document any existing lock() — plan must not add new ones

### 2c — Scope Creep Check
- Verify architecture plan touches ONLY methods in scope
- Verify no unrelated changes mixed in

### 2d — Test Framework Compliance
- Verify plan specifies xUnit `[Fact]` / `Assert.Equal()` — NEVER NUnit/MSTest
- Verify UTF-8 encoding in all target files

### 2e — Dependency Impact
- `get_dependency_cycles` — verify plan doesn't introduce cycles
- `get_blast_radius` — confirm callers won't break

---

## STEP 3 — STRESS-TEST CHECKLIST

For each extracted method in the plan:
- [ ] Achieves CYC ≤ 8 individually?
- [ ] >= 15 LOC (not trivially small)?
- [ ] PascalCase verb-noun name (ASCII only)?
- [ ] Residual God-method drops to ≤ 8 after all extractions?
- [ ] FSM state transitions preserved untouched?
- [ ] No new lock() statements?
- [ ] Diff < 10,000 chars (check scope metrics)?

---

## STEP 4 — WRITE AUDIT REPORT

Write `docs/brain/EPIC-W7-NNN/03-audit-report.md`:

```markdown
# EPIC-W7-NNN — DNA & Architecture Audit

## Audit Verdict
**Status**: [APPROVED / NEEDS_REVISION]

## CYC Reduction Analysis
| Method | Current CYC | Post-Extraction CYC | Achieves ≤ 8? |
|--------|------------|---------------------|---------------|
| [target]    | [N] | [N] | ✓/✗ |
| [extracted1] | — | [N] | ✓/✗ |

## V12 DNA Compliance
- Lock-free: [PASS/FAIL] — [N] existing lock() found (pre-existing, out of scope)
- ASCII-only: [PASS/FAIL]
- xUnit tests: [PASS/FAIL — plan specifies xUnit]
- UTF-8 encoding: [PASS/FAIL]
- ONE CONCERN rule: [PASS/FAIL]

## Stress-Test Results
- Extraction viability: [PASS/FAIL]
- Residual CYC: [N] (target ≤ 8)
- Dependency impact: [PASS/FAIL]
- Scope boundaries: [PASS/FAIL]

## Issues Found
| Severity | Issue | Resolution |
|----------|-------|-----------|
| [CRITICAL/SIGNIFICANT/MODERATE] | ... | ... |

## Recommendation
[APPROVED — proceed to Phase 4 ticket generation]
OR
[NEEDS_REVISION — fix these issues before proceeding: ...]
```

---

## STEP 5 — UPDATE MANIFEST

Update `docs/brain/EPIC-W7-NNN/manifest.json`:
- Set `phase_3.status = "completed"`
- Set `phase_3.output = "03-audit-report.md"`
- Set `phase_3.timestamp = "<ISO8601>"`

---

## RETURN TO ORCHESTRATOR

```json
{
  "status": "success",
  "epic_id": "EPIC-W7-NNN",
  "phase": "3",
  "output_artifact": "docs/brain/EPIC-W7-NNN/03-audit-report.md",
  "verdict": "APPROVED",
  "dna_compliance": {
    "lock_free": true,
    "ascii_only": true,
    "xunit_tests": true,
    "utf8": true
  }
}
```
