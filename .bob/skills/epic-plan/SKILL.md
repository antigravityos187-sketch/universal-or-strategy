---
name: epic-plan
description: >-
  Phase 2 - Architecture planning for a Wave 7 epic. Subagent reads
  01-scope-boundary.md, uses jCodemunch MCP for dependency analysis,
  queries Jane Street KB, writes 02-architecture-plan.md.
  No scripts, no plugins/ refs, no python manifest calls.
metadata:
  user-invocable: true
  disable-model-invocation: true
  argument-hint: <epic-id>
---

# PHASE 2: ARCHITECTURE PLANNING
**Epic ID:** EPIC-W7-NNN (passed by orchestrator)
**Mode:** `v12-phase2-architecture`
**Protocol:** V12.28 Bob IDE V2 — Independent Subagent

> You are a Technical Architect. You plan before executing.
> You do NOT touch src/ files in this phase.
> You produce 02-architecture-plan.md then return to orchestrator.

---

## MANDATORY: JANE STREET KB QUERY

Before any analysis, query the KB:
```
python scripts/query_kb.py "complexity reduction extraction"
python scripts/query_kb.py "FSM extraction patterns"
```

Apply all P0/P1 rules from the KB to this plan.

---

## STEP 1 — READ INPUTS

Read these files using `read_file`:
1. `docs/brain/EPIC-W7-NNN/00-scope.md` — target method, CYC, source file
2. `docs/brain/EPIC-W7-NNN/01-scope-boundary.md` — validated scope, risks

Confirm you understand:
- Target method and file
- Agreed scope and boundaries
- Validated complexity metrics
- Pre-existing issues (OUT OF SCOPE)

---

## STEP 2 — DEPENDENCY ANALYSIS (jCodemunch MCP)

### 2a — Blast Radius
```
get_blast_radius { "repo": ".", "symbol": "<target-method>", "depth": 2 }
```
Capture: direct callers, indirect dependents, shared state.

### 2b — Dependency Graph
```
get_dependency_graph { "repo": ".", "file": "<target-file>", "direction": "both" }
```

### 2c — Call Hierarchy
```
get_call_hierarchy { "repo": ".", "symbol_id": "<target-method>", "direction": "both" }
```

### 2d — Coupling Metrics
```
get_coupling_metrics { "repo": ".", "module_path": "<target-file>" }
```
Record Ca/Ce/Instability. Target: stable modules (I < 0.5) for core logic.

### 2e — Dependency Cycles
```
get_dependency_cycles { "repo": "." }
```
Document any existing cycles. Verify refactoring won't introduce new ones.

---

## STEP 3 — IDENTIFY KEY TECHNICAL DECISIONS

Analyze 3-5 decisions that shape the refactoring:

**Structure:** How to decompose the God-method?
- By concern, by flow, by guard clause?

**Extraction placement:** Same file (partial class) or new partial file?
- New file threshold: target file > 1200 LOC

**Naming:** PascalCase verb-noun (Handle..., Process..., Validate..., Route...)
- Each extracted method >= 15 LOC

**Transition:** Incremental extraction or full rewrite?

---

## STEP 4 — WRITE ARCHITECTURE PLAN

Write `docs/brain/EPIC-W7-NNN/02-architecture-plan.md`:

```markdown
# EPIC-W7-NNN — Architecture Plan

## Jane Street KB Rules Applied
- [List P0/P1 rules from KB query that apply]

## Target Method
- **Method**: [MethodName]
- **File**: [src/V12_002.*.cs]
- **Current CYC**: [N]
- **Target CYC**: ≤ 8

## Dependency Map
| Caller | File | How It Uses Target |
|--------|------|-------------------|
| ...    | ...  | ...               |

## Coupling Metrics
- Ca (afferent): [N] — files that depend on this
- Ce (efferent): [N] — files this depends on
- Instability: [I = Ce/(Ca+Ce)]

## Key Technical Decisions
### Decision 1: [Name]
- Chosen: [approach]
- Rationale: [why]
- V12 DNA alignment: [how]

## Extraction Plan
| Extracted Method | Responsibility | Est. LOC | Target CYC |
|-----------------|----------------|----------|------------|
| [MethodName]    | [concern]      | [N]      | ≤ 8        |

## Target State
- Residual God-method CYC after extraction: [N] (must be ≤ 8)
- New partial file needed: [YES/NO]
- Files modified: [list]

## Invariants (MUST NOT change)
- External behavior: [list]
- FSM state transitions: [any that must be preserved]
- Signal names and order IDs: unchanged

## V12 DNA Verification Plan
- CYC ≤ 8: Verified via jCodemunch get_symbol_complexity
- No lock() added: grep -r "lock(" src/
- ASCII-only: grep -Prn "[^\x00-\x7F]" <files>
- xUnit tests: [Fact] / Assert.Equal() only — NEVER NUnit/MSTest
- UTF-8 encoding: All files UTF-8 (no BOM)

## Risk Assessment
| Risk | Likelihood | Mitigation |
|------|-----------|-----------|
| ...  | ...        | ...        |
```

---

## STEP 5 — UPDATE MANIFEST

Update `docs/brain/EPIC-W7-NNN/manifest.json`:
- Set `phase_2.status = "completed"`
- Set `phase_2.output = "02-architecture-plan.md"`
- Set `phase_2.timestamp = "<ISO8601>"`

---

## RETURN TO ORCHESTRATOR

```json
{
  "status": "success",
  "epic_id": "EPIC-W7-NNN",
  "phase": "2",
  "output_artifact": "docs/brain/EPIC-W7-NNN/02-architecture-plan.md",
  "extractions_planned": N,
  "target_cyc": 8,
  "new_file_needed": false
}
```
