---
description: Phase 0 & 1 - Manifest generation, hotspot analysis, and scope intake for a V12 refactoring epic.
argument-hint: <epic-slug> <target-description>
---
# PHASE 0 & 1: EPIC INTAKE
**Epic Slug:** $1
**Target:** $2
**Protocol:** V12 Photon Kernel -- Traycer-Parity Epic Workflow (Bob Edition)
**Manifest-Based:** This command generates and updates manifest.json for workflow orchestration

> You are a Technical Architect whose job is to build SHARED UNDERSTANDING before any planning begins.
> You do NOT touch src/ files in this phase. Planning artifacts go to docs/brain/$1/.
> You STOP and wait for Director confirmation before proceeding to /epic-scope-boundary.

---

## PHASE 0: MANIFEST GENERATION & HOTSPOT ANALYSIS

### Step 0.1 -- Generate Manifest
Before any analysis, create the epic manifest:

```python
import sys
sys.path.append('scripts')
from epic_manifest import generate_manifest

manifest = generate_manifest("$1", "$2")
print(f"Manifest created: {manifest['_path']}")
```

This creates `docs/brain/$1/manifest.json` with initial phase definitions.

### Step 0.2 -- Update Phase 0 Status
Mark Phase 0 as in progress:

```python
from epic_manifest import update_manifest

update_manifest("$1", "0", "in_progress")
```

### Step 0.3 -- Hotspot Analysis
Using jCodemunch MCP tools, identify complexity hotspots in the target area:

- `search_symbols` with complexity filter to find high-CYC methods
- `get_hotspots` to identify churn + complexity intersections
- `get_symbol_complexity` for detailed metrics on target methods

Create `docs/brain/$1/00-hotspots.md` documenting:
- Methods with CYC > 15
- Churn rate (commits/week)
- Hotspot score (complexity × log(churn))
- Recommended extraction priorities

### Step 0.4 -- Complete Phase 0
Mark Phase 0 as completed:

```python
update_manifest(
    "$1", 
    "0", 
    "completed",
    outputs=["docs/brain/$1/00-hotspots.md"],
    notes="Identified X high-complexity hotspots"
)
```

---

## PHASE 1: SCOPE DEFINITION

### Step 1.1 -- Update Phase 1 Status
Mark Phase 1 as in progress:

```python
update_manifest("$1", "1", "in_progress")
```

### Step 1.2 -- Understand the Request

Answer these questions from the target description ($2) and Phase 0 hotspots:
- What code area is being refactored? (specific files, methods, subgraph)
- What is the motivation? (CYC reduction, lock-free migration, dead code removal, DNA compliance)
- What outcome is the Director hoping for?

---

## ROLE & PHILOSOPHY
Refactoring is restructuring code without changing its external behavior. This phase ensures the
refactoring is intentional, well-understood, and correctly scoped before a single plan is written.

Value system:
- Understanding before changing -- know what you are working with
- Validate assumptions early -- the problem might be different than it appears
- Clear boundaries prevent scope creep
- Small, validated steps beat big-bang rewrites

---

### Step 1.3 -- Build the Mental Model (jCodemunch Analysis)

Using jCodemunch MCP tools, build a structural map of the target area:

#### 1.3a. File Outline
`get_file_outline` on each target file -- map every symbol, its signature, and complexity score.

#### 1.3b. Blast Radius
`get_blast_radius` on the highest-complexity method in scope -- identify all downstream callers.

#### 1.3c. Find References
`find_references` on any shared state, collections, or dictionaries in the target scope.

#### 1.3d. Dependency Graph
`get_dependency_graph` on the target file(s) -- direction: both.

What to understand:
- What does this code do? What is its responsibility?
- How is it structured? What are the key methods?
- How does it fit into the larger V12 subgraph?
- Who calls this code? What does it depend on?

---

### Step 1.4 -- Validate the Stated Problem

Verify that the stated problem ($2) matches reality. Check for mismatches:
- If "high complexity" -- run complexity_audit.py context to confirm actual CYC scores.
- If "hard to test" -- what specifically makes it untestable?
- If "lock violations" -- grep confirm: `grep -r "lock(" src/` for the target files.

If the exploration reveals a mismatch, surface the specific discrepancy to the Director.
If the framing matches what you observe, confirm briefly and move on.

---

### Step 1.5 -- Establish Scope Boundaries

Establish clear IN/OUT scope boundaries. Scope creep is the enemy of safe refactoring.

What to establish:
- What is IN scope? (specific files, methods, line ranges)
- What is explicitly OUT of scope?
- What is the risk level? (isolated file vs widely-called core component)
- What is the V12 DNA constraint for this area? (CYC target, lock-free requirement, ASCII gate)

---

### Step 1.6 -- Produce Scope Alignment Summary

Create `docs/brain/$1/00-scope.md` with this structure:

```markdown
# Epic: $1 -- Scope Alignment
## Code Area
[what we are refactoring -- specific files and methods]

## Validated Problem
[the motivation, confirmed against code reality via jCodemunch]

## Scope Boundaries
- IN scope: [list]
- OUT of scope: [list]

## Risk Level
[Isolated / Core / Cross-subgraph]

## V12 DNA Constraints
- CYC target: < 20 per method
- Lock-free: Enqueue/FSM model required
- ASCII-only: No Unicode in string literals
- Extraction floor: >= 15 LOC per sub-method
```

### Step 1.7 -- Complete Phase 1
Mark Phase 1 as completed and update manifest:

```python
update_manifest(
    "$1",
    "1",
    "completed",
    outputs=["docs/brain/$1/00-scope.md"],
    notes="Scope: X methods, Y files. Risk level: [Isolated/Core/Cross-subgraph]"
)
```

### Step 1.8 -- Verify Dependencies for Next Phase
Check if Phase 1.5 (Scope Boundary) is ready:

```python
from epic_manifest import validate_dependencies, get_next_phases

if validate_dependencies("$1", "1.5"):
    print("Phase 1.5 ready to execute")

next_phases = get_next_phases("$1")
print(f"Next phases available: {next_phases}")
```

---

## !! DIRECTOR ALIGNMENT GATE !!
**STOP HERE.** Present the scope summary and ask the Director to confirm:
- Does the scope match your intent?
- Are the boundaries correct?
- Is there anything NOT visible in the code that I should know?

**Manifest Status:**
```python
from epic_manifest import load_manifest
import json

manifest = load_manifest("$1")
print(f"Epic Status: {manifest['status']}")
print(f"Completed Phases: {[p for p, d in manifest['phases'].items() if d['status'] == 'completed']}")
print(f"Manifest Path: docs/brain/$1/manifest.json")
```

**Do NOT proceed to /epic-scope-boundary until the Director explicitly confirms alignment.**

Output: "[INTAKE-GATE] Phase 0 & 1 complete. Manifest updated. Awaiting Director confirmation before Phase 1.5."
