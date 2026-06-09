---
description: Phase 3 - Stress-test the refactoring approach before ticket breakdown.
argument-hint: <epic-slug>
---
# PHASE 3: EPIC VALIDATE
**Epic Slug:** $1
**Protocol:** V12 Photon Kernel -- Manifest-Based Independent Subtask

> You are an Architect who stress-tests the refactoring approach before implementation starts.
> You validate that the approach is safe, minimal, and grounded in the actual codebase.
> You do NOT touch src/ files in this phase.
> You update the approach docs IN-PLACE (no forked copies) when issues are resolved.

---

## STEP 0 -- LOAD MANIFEST

```python
import sys
sys.path.append('scripts')
from epic_manifest import load_manifest, validate_dependencies

# Load manifest
try:
    manifest = load_manifest("$1")
except FileNotFoundError:
    print("[ERROR] Manifest not found. Run /epic-intake first.")
    exit(1)

# Verify Phase 2 and 2.3 complete
if not validate_dependencies("$1", "3"):
    print("[ERROR] Phase 2 and 2.3 must be completed first")
    print("Dependencies not satisfied for Phase 3")
    exit(1)

print("[✓] Manifest loaded. Phase 2 and 2.3 complete.")
print(f"[✓] Inputs:")
for artifact in manifest['phases']['2']['output_artifacts']:
    print(f"    - {artifact}")
if '2.3' in manifest['phases']:
    for artifact in manifest['phases']['2.3']['output_artifacts']:
        print(f"    - {artifact}")
```

---

## ROLE & PHILOSOPHY
Validate that the refactoring is safe, simple, and grounded in the actual codebase before it
is broken into tickets. Focus on five questions:
1. Are invariants explicit and testable?
2. Is the migration strategy safe for the actual blast radius?
3. Do mitigations match the hotspots from the Analysis?
4. Does the test/verification strategy provide a real safety net?
5. Is this the MINIMUM change that solves the problem?

---

## STEP 1 -- GATHER CONTEXT

Read and internalize all previous phase outputs:

```python
# Collect all input artifacts
all_inputs = []
for phase_id in ['1', '1.5', '2']:
    if phase_id in manifest['phases']:
        all_inputs.extend(manifest['phases'][phase_id]['output_artifacts'])
if '2.3' in manifest['phases']:
    all_inputs.extend(manifest['phases']['2.3']['output_artifacts'])

print(f"[→] Reading all previous outputs:")
for artifact in all_inputs:
    print(f"    - {artifact}")
```

Use `read_file` to load:
- Scope document (shared understanding)
- Boundary analysis (scope validation)
- Analysis document (dependency map, risk hotspots)
- Approach document (decisions, target state, invariants)
- Sentinel report (if Phase 2.3 was run)

Use `get_file_outline` on each target file to confirm live code matches the analysis.

---

## STEP 2 -- IDENTIFY CRITICAL DECISIONS

Extract the 3-5 decisions that most affect safety, complexity, or sequencing. Focus on:
- Decomposition and placement of responsibilities
- Interface preservation vs intentional contract changes
- Extraction order (which methods first?)
- Whether new partial files are needed (file > 1200 LOC threshold)
- V12 DNA constraint compliance in the approach

---

## STEP 3 -- STRESS-TEST EACH DECISION

For each critical decision, ask:
- What breaks if this decision is wrong?
- Could the same outcome be achieved more simply?
- What happens in partial extraction states (mid-ticket)?
- Is the V12 DNA verification strategy strong enough to catch regressions here?

V12-specific stress-test checklist:
- [ ] Does each proposed sub-method meet the 15-LOC extraction floor?
- [ ] Does the residual God-method drop below 20 CYC after all extractions?
- [ ] Does the approach preserve all FSM state transitions untouched?
- [ ] Does the approach guarantee zero new lock() statements?
- [ ] Is deploy-sync.ps1 explicitly called after EVERY src/ edit in the ticket plan?
- [ ] Are all proposed sub-method names ASCII-only PascalCase verb-noun?
- [ ] Is there any risk of signal name or order ID mutation during extraction?

---

## STEP 4 -- ISSUE CLASSIFICATION

Categorize any issues found:

**CRITICAL -- Address before ticketing:**
- Likely regression of a stated invariant
- Extraction that leaves the codebase uncompilable mid-ticket
- CYC reduction approach that cannot reach the < 20 target
- V12 DNA violation baked into the approach (lock, Unicode, etc.)

**SIGNIFICANT -- Address before proceeding:**
- Overly complex extraction path when a simpler one exists
- Approach that fights existing V12 partial class patterns
- Missing method signature or call site change in approach
- Risk mitigation too vague to guide ticket execution

**MODERATE -- Clarify and decide:**
- Naming inconsistencies with existing V12 method naming conventions
- Boundary ambiguity between tickets (which extraction goes in which ticket)
- Verification step that needs tightening

---

## STEP 5 -- INTERVIEW FOR RESOLUTION

Present findings to the Director. For each gap or concern:
- Explain the issue and why it matters to safe refactoring
- Ask focused questions to confirm intent or choose between options
- Resolve CRITICAL issues before moving to SIGNIFICANT ones

---

## STEP 6 -- UPDATE SOURCE DOCUMENTS IN-PLACE

As issues are resolved through clarification:
- Update Phase 2 approach document with agreed decisions and mitigations
- Update Phase 2 analysis document if validation reveals missing hotspots
- DO NOT fork into separate documents -- keep one source of truth per doc

```python
# Get paths to Phase 2 outputs for in-place updates
phase2_outputs = manifest['phases']['2']['output_artifacts']
analysis_doc = [a for a in phase2_outputs if 'analysis' in a][0]
approach_doc = [a for a in phase2_outputs if 'approach' in a or 'architecture' in a][0]

print(f"[→] Update these documents in-place as needed:")
print(f"    - {analysis_doc}")
print(f"    - {approach_doc}")
```

---

## STEP 7 -- CONFIRM READINESS

Once all CRITICAL and SIGNIFICANT issues are resolved:
- Review the updated documents with the Director
- Confirm the plan is safe and concrete enough for ticket breakdown
- Provide a one-paragraph readiness summary

---

## STEP 8 -- UPDATE MANIFEST

```python
from epic_manifest import update_manifest

# Phase 3 doesn't create new artifacts - it validates and updates Phase 2 docs
# So we reference the Phase 2 outputs as our "outputs" (they were updated in-place)
phase2_outputs = manifest['phases']['2']['output_artifacts']

# Update manifest
update_manifest(
    "$1",
    "3",
    "completed",
    outputs=phase2_outputs,  # Reference Phase 2 docs (updated in-place)
    notes="Approach validated and refined. All CRITICAL and SIGNIFICANT issues resolved."
)

print(f"[✓] Phase 3 complete. Phase 2 documents validated and updated in-place.")
```

---

## !! VALIDATION GATE !!
**STOP HERE.** Only proceed to /epic-tickets when the Director confirms:
"[EPIC-VALIDATE-PASS] Plan validated. Ready for ticket breakdown."

Output: "[VALIDATE-GATE] Architecture validation complete. Awaiting Director sign-off."
