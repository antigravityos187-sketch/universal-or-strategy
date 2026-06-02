# Skill Analysis: Code Structure Cleanup Pattern

**Date**: 2026-06-02  
**Source**: [micky-podcast-agentic-engineering/code-structure-cleanup](https://github.com/pawel-cell/micky-podcast-agentic-engineering/blob/main/skills/code-structure-cleanup/SKILL.md)  
**Analyst**: Advanced Mode Agent  
**Context**: EPIC-POSINFO workflow improvement

---

## Executive Summary

The external skill demonstrates a **post-implementation cleanup pattern** that is MISSING from our current epic workflow. It provides a structured approach to eliminating duplication and improving code structure AFTER a feature works, which aligns perfectly with EPIC-POSINFO's goal of refactoring working code.

**Key Finding**: Our workflow lacks an explicit "Scope Boundary Explanation" phase that would prevent scope creep and clarify what constitutes "cleanup" vs "redesign."

---

## External Skill Structure Analysis

### 1. Purpose & Positioning

**What it does**:
- Post-implementation cleanup pass for working features
- Eliminates duplicated mechanics, repeated API calls, messy structure
- Extracts reusable service-layer modules without changing behavior

**When to use**:
- Feature works locally but code feels duplicated/messy
- Agent created similar helpers in multiple files
- Need smaller, cleaner PR before review

**Critical constraint**: "Do not use this as permission to redesign the whole app"

### 2. Key Components

#### A. Clear Scope Definition
```md
Goal:
- Find duplicated runtime mechanics, repeated API calls, parsing, validation, business logic
- Move repeated mechanics into reusable service-layer functions/modules
- Keep domain policy in the calling route/action/component
- Do not change user-facing behavior
- Keep the diff small
```

**Analysis**: This is a **scope boundary contract** - it explicitly defines what's IN (mechanics) vs OUT (domain policy, behavior changes).

#### B. Structured Process (6 Steps)
1. Inspect files touched by feature
2. Identify repeated logic and **name the duplication clearly**
3. Propose smallest service-layer extraction
4. Implement it
5. Run relevant tests/typechecks
6. Summarize exactly what got simpler

**Analysis**: Step 2 ("name the duplication clearly") is a **forensic gate** - forces explicit articulation before action.

#### C. Anti-Pattern Guardrails
1. Refactoring the whole app (scope creep)
2. Renaming everything (noise in diff)
3. Mixing cleanup with new feature (concern separation)
4. Only formatting code (superficial cleanup)
5. Moving domain policy into services (wrong abstraction)

**Analysis**: These are **negative examples** - they teach by showing what NOT to do.

#### D. Verification Checklist
- [ ] User-facing behavior stayed the same
- [ ] Repeated mechanics were actually reduced
- [ ] Calling files became simpler
- [ ] Relevant tests/typechecks ran
- [ ] Diff stayed focused on feature area

**Analysis**: This is a **success criteria gate** - measurable outcomes, not subjective quality.

---

## Comparison to V12 Epic Workflow

### Current V12 Workflow (Phase 6 Recursive Protocol)

**Stage 0**: Forensic Intake (Orchestrator)  
**Stage 1**: Vision/Spec (Architect)  
**Stage 2**: Arch Planning (Architect)  
**Stage 3**: DNA & PR Audit (Adjudicator)  
**Stage 4**: Recursive Execution (Engineer)  
**Stage 5**: Verification/Review (Forensics)  
**Stage 6**: Sign-off (Director)

### Gap Analysis

| Element | External Skill | V12 Workflow | Status |
|---------|---------------|--------------|--------|
| **Scope Boundary Contract** | ✅ Explicit (mechanics vs policy) | ⚠️ Implicit (in 00-scope.md) | **GAP** |
| **Duplication Naming Gate** | ✅ Step 2 (name it clearly) | ❌ Missing | **GAP** |
| **Anti-Pattern Guardrails** | ✅ 5 explicit pitfalls | ⚠️ Scattered in DNA rules | **GAP** |
| **Behavior Preservation** | ✅ First checklist item | ✅ F5 verification (Stage 6) | ✅ ALIGNED |
| **Diff Size Constraint** | ✅ "Keep diff small" | ✅ <10k chars (PR hygiene) | ✅ ALIGNED |
| **Test Execution** | ✅ Step 5 + checklist | ✅ Pre-push validation | ✅ ALIGNED |
| **Success Criteria** | ✅ 5-item checklist | ✅ Success Criteria section | ✅ ALIGNED |

---

## Identified Gaps in V12 Workflow

### Gap 1: No Explicit Scope Boundary Explanation Phase

**Problem**: Our 00-scope.md defines IN/OUT scope, but doesn't explain WHY boundaries exist or what happens if violated.

**External Skill Solution**: 
- "Service layer" definition (mechanics vs policy)
- "Do not use this as permission to redesign the whole app"
- Anti-pattern #1: "Refactoring the whole app. Keep scope tied to feature."

**Impact on EPIC-POSINFO**: 
- Risk of scope creep (e.g., "while we're here, let's refactor PositionInfo struct")
- Unclear boundary between "cleanup" and "redesign"

### Gap 2: No Duplication Naming Gate

**Problem**: We jump from "identify duplication" (forensics) to "propose solution" (planning) without forcing explicit articulation.

**External Skill Solution**: Step 2 - "Identify repeated logic and **name the duplication clearly**"

**Impact on EPIC-POSINFO**:
- We identified "structural duplication" but didn't name the pattern (e.g., "Switch-Based Field Accessor Pattern")
- Harder to evaluate if proposed solution actually addresses the named problem

### Gap 3: Anti-Patterns Not Consolidated

**Problem**: V12 DNA rules are scattered across AGENTS.md, BOB.md, protocol docs. No single "pitfalls" reference for refactoring tasks.

**External Skill Solution**: 5 consolidated anti-patterns with clear rationale

**Impact on EPIC-POSINFO**:
- Risk of violating "No Scope Creep Protocol" (V12.23) without realizing it
- No quick reference for "what NOT to do" during refactoring

---

## Recommendations for EPIC-POSINFO

### Recommendation 1: Add Phase 1.5 - Scope Boundary Explanation

**Insert between Stage 1 (Vision/Spec) and Stage 2 (Arch Planning)**

**Purpose**: Force explicit articulation of:
1. What pattern is being eliminated (name it)
2. Why the boundary exists (mechanics vs policy)
3. What would constitute scope creep (anti-patterns)
4. How to verify boundary wasn't violated (checklist)

**Deliverable**: `docs/brain/EPIC-POSINFO/01.5-scope-boundaries.md`

**Content Template**:
```md
# Scope Boundary Explanation

## Pattern Being Eliminated
[Name the duplication clearly - e.g., "Switch-Based Field Accessor Pattern"]

## Boundary Definition
**IN SCOPE (Mechanics)**:
- [What we're allowed to change]

**OUT OF SCOPE (Policy/Behavior)**:
- [What we must NOT touch]

## Anti-Patterns for This Epic
1. [Specific pitfall #1 for this refactoring]
2. [Specific pitfall #2 for this refactoring]
...

## Verification Checklist
- [ ] [Specific check #1]
- [ ] [Specific check #2]
...
```

### Recommendation 2: Create V12 Refactoring Anti-Patterns Reference

**File**: `docs/protocol/REFACTORING_ANTIPATTERNS.md`

**Content**: Consolidate scattered rules into single reference:
1. Scope creep (V12.23 No Scope Creep Protocol)
2. Whitespace mutation (AGENTS.md Surgical Changes)
3. Mixing concerns (separate PRs per concern)
4. Premature abstraction (YAGNI principle)
5. Breaking zero-allocation (Jane Street alignment)

**Usage**: Link from every epic 00-scope.md

### Recommendation 3: Enhance 00-scope.md Template

**Add sections**:
- **Pattern Name**: [Explicit name for the duplication being eliminated]
- **Scope Rationale**: [Why these boundaries exist]
- **Anti-Patterns**: [What NOT to do for this specific epic]

**Example for EPIC-POSINFO**:
```md
## Pattern Name
"Switch-Based Field Accessor Pattern" - 6 methods with identical switch structure differing only in field names and return types

## Scope Rationale
- **Why 6 methods only**: These are the only methods exhibiting the pattern
- **Why no PositionInfo changes**: Struct fields are immutable by design (zero-allocation constraint)
- **Why no caller changes**: Private methods = isolated refactoring

## Anti-Patterns for This Epic
1. ❌ Introducing arrays/dictionaries (violates zero-allocation)
2. ❌ Using reflection (violates zero-allocation + performance)
3. ❌ Modifying PositionInfo struct (out of scope)
4. ❌ Changing method signatures (breaks 100+ call sites)
5. ❌ "While we're here" fixes to other methods (scope creep)
```

---

## Should We Adopt This Skill Pattern?

### YES - With V12 Adaptations

**Rationale**:
1. **Fills workflow gap**: We lack explicit scope boundary explanation
2. **Prevents scope creep**: Anti-pattern guardrails align with V12.23 protocol
3. **Improves clarity**: Naming the pattern forces precise thinking
4. **Lightweight**: Adds ~30 min to planning phase, saves hours in rework

**Adaptations Needed**:
1. Rename "service layer" to "extracted helper" (more general)
2. Add Jane Street constraints (zero-allocation, CYC ≤ 10)
3. Integrate with Phase 6 Recursive Protocol (insert as Stage 1.5)
4. Add V12-specific anti-patterns (locks, Unicode, whitespace mutation)

### Proposed V12 Skill: `refactoring-scope-boundary`

**File**: `plugins/refactoring-scope-boundary/SKILL.md`

**Purpose**: Force explicit scope boundary articulation before refactoring working code

**When to use**: 
- Any epic targeting existing code (not new features)
- Code works but has duplication/complexity issues
- Risk of scope creep exists

**Process**:
1. Name the pattern being eliminated
2. Define mechanics vs policy boundary
3. List anti-patterns for this specific refactoring
4. Create verification checklist
5. Get Director approval before proceeding to arch planning

---

## Impact on EPIC-POSINFO Workflow

### Current State (Without Skill)
```
Stage 0: Forensics → Stage 1: Vision/Spec → Stage 2: Arch Planning
         ↓                    ↓                      ↓
    "6 methods"      "Eliminate duplication"   "Evaluate options"
```

**Risk**: Jump from problem to solution without clarifying boundaries

### Proposed State (With Skill)
```
Stage 0: Forensics → Stage 1: Vision/Spec → Stage 1.5: Scope Boundaries → Stage 2: Arch Planning
         ↓                    ↓                         ↓                        ↓
    "6 methods"      "Eliminate duplication"   "Switch-Based Field      "Option 1: Inline helper"
                                                Accessor Pattern"         "Option 2: ..."
                                                + boundaries
                                                + anti-patterns
```

**Benefit**: Explicit contract prevents scope creep and clarifies success criteria

---

## Recommendation: Proceed or Update Workflow First?

### RECOMMENDATION: Update Workflow First (30-Minute Investment)

**Rationale**:
1. **Low cost**: Creating Phase 1.5 doc takes ~30 min
2. **High value**: Prevents scope creep that could waste hours
3. **Reusable**: Benefits all future refactoring epics (EPIC-8 through EPIC-14)
4. **Aligns with V12.23**: Reinforces "No Scope Creep Protocol"

**Action Plan**:
1. ✅ Create `docs/brain/EPIC-POSINFO/01.5-scope-boundaries.md` (this session)
2. ✅ Update `docs/brain/EPIC-POSINFO/00-scope.md` with pattern name + anti-patterns
3. ⏭️ Proceed to `/epic-plan` with enhanced scope clarity
4. 📋 Backlog: Create `plugins/refactoring-scope-boundary/SKILL.md` for future epics

---

## Key Takeaways

### From External Skill
1. **Name the pattern explicitly** - "Switch-Based Field Accessor Pattern" is clearer than "structural duplication"
2. **Define mechanics vs policy** - Clarifies what's refactorable vs what's domain logic
3. **Anti-patterns as guardrails** - Negative examples prevent common mistakes
4. **Verification checklist** - Measurable outcomes, not subjective quality

### For V12 Workflow
1. **Add Phase 1.5** - Scope Boundary Explanation between Vision and Planning
2. **Consolidate anti-patterns** - Create `REFACTORING_ANTIPATTERNS.md` reference
3. **Enhance scope template** - Add Pattern Name, Scope Rationale, Anti-Patterns sections
4. **Create reusable skill** - `refactoring-scope-boundary` for future epics

### For EPIC-POSINFO Specifically
1. **Pattern name**: "Switch-Based Field Accessor Pattern"
2. **Key boundary**: Mechanics (accessor logic) vs Policy (PositionInfo struct design)
3. **Top anti-pattern**: Introducing heap allocation (arrays, reflection, dictionaries)
4. **Success metric**: CodeScene 10/10 + CYC ≤ 10 + zero allocation preserved

---

## Next Steps

**Immediate (This Session)**:
1. Create `01.5-scope-boundaries.md` with pattern name and anti-patterns
2. Update `00-scope.md` with enhanced sections
3. Present to Director for approval

**After Director Approval**:
1. Proceed to `/epic-plan` with clarified scope
2. Evaluate refactoring options against explicit boundaries
3. Generate implementation plan with anti-pattern checks

**Future (Backlog)**:
1. Create `plugins/refactoring-scope-boundary/SKILL.md`
2. Create `docs/protocol/REFACTORING_ANTIPATTERNS.md`
3. Update Phase 6 Recursive Protocol to include Stage 1.5

---

## Conclusion

The external skill reveals a **critical gap** in our workflow: we lack an explicit phase for articulating scope boundaries and anti-patterns before planning refactoring work. Adding Phase 1.5 (Scope Boundary Explanation) is a **low-cost, high-value** improvement that:

- Prevents scope creep (V12.23 alignment)
- Forces precise thinking (Karpathy Protocol alignment)
- Creates reusable guardrails (Jane Street alignment)
- Takes 30 minutes now, saves hours in rework

**Recommendation**: Create Phase 1.5 documents NOW, then proceed to `/epic-plan`.