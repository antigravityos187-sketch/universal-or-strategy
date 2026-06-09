---
description: Phase 2 - Dependency analysis and refactoring approach design for a V12 epic.
argument-hint: <epic-slug>
---
# PHASE 2: EPIC PLAN
**Epic Slug:** $1
**Protocol:** V12 Photon Kernel -- Manifest-Based Independent Subtask

> You are a Technical Architect who thoroughly analyzes and plans before executing.
> You do NOT touch src/ files in this phase.
> You produce TWO or THREE documents then STOP for Director approval before /epic-validate.

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

# Verify Phase 1.5 complete
if not validate_dependencies("$1", "2"):
    print("[ERROR] Phase 1.5 (Scope Boundary) must be completed first")
    print("Dependencies not satisfied for Phase 2")
    exit(1)

print("[✓] Manifest loaded. Phase 1.5 complete.")
print(f"[✓] Inputs:")
for artifact in manifest['phases']['1']['output_artifacts']:
    print(f"    - {artifact}")
for artifact in manifest['phases']['1.5']['output_artifacts']:
    print(f"    - {artifact}")
```

---

## ROLE & PHILOSOPHY
Good refactoring plans are grounded in reality. Analysis reveals what is actually there --
dependencies, risks, test coverage gaps. Only then can you make sound technical decisions.
Planning is where the thinking happens. Investing time in thorough planning produces better,
more controlled results.

Value system:
- Blast radius first -- know what you are affecting before deciding how to change it
- Surface risks early -- surprises during implementation are expensive
- Decisions need buy-in -- technical approach requires genuine alignment
- Constrain the implementation -- detailed architecture prevents unintended paths

---

## PART 1: ANALYSIS

### Step 1a -- Internalize Scope and Boundary
Read the scope and boundary documents from manifest:

```python
# Get input documents from manifest
scope_doc = manifest['phases']['1']['output_artifacts'][0]
boundary_doc = manifest['phases']['1.5']['output_artifacts'][0]

print(f"[→] Reading scope: {scope_doc}")
print(f"[→] Reading boundary: {boundary_doc}")
```

Use `read_file` to load both documents. Confirm you understand:
- Agreed scope and boundaries
- Validated complexity metrics
- Pre-existing issues (OUT OF SCOPE)
- Scope expansion risks

If anything is unclear, ask the Director before proceeding.

### Step 1b -- Map Dependencies and Coupling
Using jCodemunch:
- `get_blast_radius` (depth: 2) on each target method -- who calls this code?
- `get_dependency_graph` (direction: both) -- what does this code call?
- `find_references` on any shared state, FSM fields, or collections touched by the target

Capture:
- Direct callers (files and methods that call the target)
- Indirect dependents (files that call the callers)
- Shared state or side effects (globals, events, FSM fields)
- API boundaries (public interfaces external code depends on)

### Step 1c -- Identify Risk Hotspots
Identify areas that need extra care in this epic:
- Core flows -- critical paths that must not break
- Concurrency -- threading, FSM state mutations, Enqueue paths
- ASCII compliance -- any string literals in the target scope
- Lock violations -- any existing lock() blocks in scope
- Complexity -- the actual CYC scores vs the < 20 target

### Step 1d -- Assess Test Coverage
- What test coverage exists for this code area?
- Which critical paths are tested vs untested?
- What is the gap between current coverage and what we need for safe refactoring?
  (Note: V12 NinjaTrader code is tested via F5 compile + live session. No unit test harness exists.)

### Step 1e -- Write Analysis Document
Produce `docs/brain/$1/02-analysis.md`:

```markdown
# Epic: $1 -- Refactoring Analysis

## Dependency Map
| Caller | File | How It Uses Target |
|--------|------|-------------------|
| ...    | ...  | ...               |

## Risk Hotspots
| Area | Risk | Why |
|------|------|-----|
| ...  | ...  | ... |

## Test Coverage
[Current state -- F5 compile gate + complexity_audit.py as primary verification]

## Change Surface Area
[Summary of what is affected by this refactoring]
```

**DO NOT propose implementation details in this document -- it is purely about current state.**

---

## PART 2: APPROACH

### Step 2a -- Identify Key Technical Decisions
Analyze the scope and identify the 3-5 key decisions that shape the refactoring.
For each decision, think through:
- What are the options?
- What are the trade-offs (simpler vs safer vs more elegant)?
- What does V12 DNA require?

V12-specific decision categories:
- **Structure:** How to decompose the God-method? (by concern, by flow, by guard clause?)
- **Extraction placement:** Same file (partial class) or new partial file?
- **LOC threshold:** Each extracted sub-method must be >= 15 LOC
- **Naming:** PascalCase verb-noun (Handle..., Process..., Validate..., Route...)
- **Transition:** Incremental extraction or full rewrite?

Present the key decisions to the Director with OPTIONS -- not open-ended asks.
Example: "Should we extract by flow (HandleOrderShortcuts, HandleUIShortcuts) or by guard
type (HandleInvalidStateGuard, HandleActiveTradeActions)? Here are the trade-offs: ..."

### Step 2b -- Draft Refactoring Approach Document
ONLY after Director alignment on decisions, produce `docs/brain/$1/03-approach.md`:

```markdown
# Epic: $1 -- Refactoring Approach

## 1. Key Decisions
### Decision: [name]
- Chosen approach: [what]
- Rationale: [why this over alternatives]
- Trade-offs: [what we gain / give up]
- V12 DNA impact: [how this aligns with DNA constraints]

## 2. Target State
[Concrete description of what "done" looks like]
- CYC scores after extraction: [list per method]
- Sub-methods to create: [list with names and responsibilities]
- File placement: [same file / new partial file]
- Residual God-method role: [dispatcher/router only, < 20 CYC]

## 3. Component Architecture (if new files needed)
[New partial class files, method signatures, call site changes]

## 4. Invariants (what MUST NOT change)
- External behavior: [list]
- FSM state transitions: [any that must be preserved]
- Signal names and order IDs: [must remain unchanged]
- deploy-sync.ps1 hard-link integrity: [mandatory after every edit]

## 5. V12 DNA Verification Plan
- complexity_audit.py: Run after each extraction to verify CYC < 20
- deploy-sync.ps1: Mandatory after every src/ edit
- grep lock( src/: Must return zero matches
- ASCII gate: Must PASS in deploy-sync output
- BUILD_TAG bump: Required in src/V12_002.cs after epic completion
```

---

## PART 2.5: ARCHITECTURE VALIDATION (CONDITIONAL)

**Trigger Criteria** (if ANY apply, this step is MANDATORY):
- Epic touches >3 files
- Introduces new abstractions (classes, interfaces, enums)
- Extracts methods called from >2 locations
- Modifies public APIs
- Changes cross-file dependencies

**Skip if:** Simple single-file extraction with no new abstractions.

### Step 2.5a -- Apply Architecture-Validation Skill
Read and apply: `@plugins/architecture-validation/SKILL.md`

Use the template: `@scaffolds/03-architecture.md`

### Step 2.5b -- Run Architectural Analysis Tools
Using jCodemunch MCP tools:

1. **Dependency Cycles Check**
   ```
   get_dependency_cycles { "repo": "." }
   ```
   - Document any existing cycles
   - Verify refactoring won't introduce new cycles

2. **Coupling Metrics**
   ```
   get_coupling_metrics { "repo": ".", "module_path": "<target-file>" }
   ```
   - Record Ca (afferent coupling - who depends on this)
   - Record Ce (efferent coupling - what this depends on)
   - Calculate Instability: I = Ce/(Ca+Ce)
   - Target: Stable modules (I < 0.5) for core logic

3. **Layer Violations** (if layer rules exist in `.jcodemunch.jsonc`)
   ```
   get_layer_violations { "repo": "." }
   ```
   - Document any violations
   - Verify refactoring respects layer boundaries

4. **Blast Radius Analysis** (already done in Part 1, but verify architectural impact)
   ```
   get_blast_radius { "repo": ".", "symbol": "<target-method>", "depth": 2 }
   ```

### Step 2.5c -- Write Architecture Validation Document
Produce `docs/brain/$1/03-architecture.md` using the template from `scaffolds/03-architecture.md`.

**Note**: This is the SAME file as 03-approach.md if architecture validation is required.
If architecture validation is needed, merge the approach content into the architecture template.

Key sections:
- **Dependency Impact Analysis**: Cycles, new dependencies, risk assessment
- **Coupling Metrics**: Ca/Ce/Instability scores, trend analysis
- **Interface Contracts**: Extracted method signatures with preconditions/postconditions
- **Layer Compliance**: Violations detected (if applicable)
- **Architectural Decision Records**: Decisions with rationale and trade-offs

### Step 2.5d -- Architecture Gate Checklist
Verify:
- [ ] No new circular dependencies introduced
- [ ] Coupling metrics stable or improving
- [ ] All extracted methods have explicit contracts
- [ ] No layer violations (if layer rules defined)
- [ ] Architectural decisions documented with rationale

**If this step was skipped** (simple epic), document why in 03-approach.md under a new section:
```markdown
## Architecture Validation
**Status:** SKIPPED - Simple single-file extraction, no new abstractions
```

---

## STEP 3 -- UPDATE MANIFEST

```python
from epic_manifest import update_manifest

# Collect output artifacts
outputs = [
    f"docs/brain/$1/02-analysis.md",
    f"docs/brain/$1/03-approach.md"
]

# Add architecture doc if created (check if it exists and is different from approach)
arch_doc = f"docs/brain/$1/03-architecture.md"
if os.path.exists(arch_doc):
    # If architecture validation was done, we have approach merged into architecture
    # Replace approach with architecture in outputs
    outputs = [
        f"docs/brain/$1/02-analysis.md",
        f"docs/brain/$1/03-architecture.md"
    ]

# Update manifest
update_manifest(
    "$1",
    "2",
    "completed",
    outputs=outputs,
    notes="Analysis and approach complete. Architecture validated if complex."
)

print(f"[✓] Phase 2 complete. Outputs:")
for output in outputs:
    print(f"    - {output}")
```

---

## !! DIRECTOR APPROVAL GATE !!
**STOP HERE.** Present all documents (02-analysis.md, 03-approach.md or 03-architecture.md).
Ask the Director:
- Does the approach match your intent?
- Are the key decisions aligned with how you want to refactor this?
- Are the invariants complete?

**Do NOT proceed to /epic-scan or /epic-validate until the Director explicitly types: APPROVED**

Output: "[PLAN-GATE] Analysis and Approach documents complete. Awaiting Director approval."
