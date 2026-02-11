# Universal Brain Integration - Implementation Plan

## Goal Description
Fully integrate the "Universal Brain" concept into the project's rules, skills, and handoff protocols. This ensures that mission-critical reasoning context (tasks, plans, logs) lives inside the repository at `.agent/brain/`, making the project effortlessly portable between Antigravity, Cursor, CLI, and other agents.

## User Review Required
> [!IMPORTANT]
> **Path Precedence**: Agents will now be instructed to look at `.agent/brain/` as the PRIMARY source of truth for planning and tasks, falling back to platform-specific "brains" only for temporary session data.

## Proposed Changes

---

### [Component] .agent/rules/

#### [MODIFY] [universalorworkspacerules.md](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/.agent/rules/universalorworkspacerules.md)
- Update Rule #20 and #21 to mandate the use of repository-based paths (`.agent/brain/`) for all reasoning artifacts.
- Enforce a "Single Source of Truth" rule for tasks and plans.

---

### [Component] .agent/skills/

#### [MODIFY] [antigravity-bridge/SKILL.md](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/.agent/skills/antigravity-bridge/SKILL.md)
- Redefine the "Brain Gap" to explain that while Antigravity creates local artifacts, the *Project Brain* lives in the repo.
- Update the MISSION BRIEF template to use relative repo paths (e.g., `./.agent/brain/task.md`) for maximum portability.

#### [MODIFY] [antigravity-core/SKILL.md](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/.agent/skills/antigravity-core/SKILL.md) (Checking for references in next step)

---

### [Component] .agent/brain/ (Universal)

#### [MODIFY] [task.md](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/.agent/brain/task.md)
- Add a new Task (Task 4) for "Global Universalization Integration".

## Verification Plan

### Automated Tests
- I will run a `grep_search` across the `.agent` folder to ensure no remnants of the old path instructions remain.

### Manual Verification
1. **Handoff Verification**: I will generate a sample handoff brief for the user to review, ensuring it uses the new `./.agent/brain/` paths.
2. **Path Resolution**: Verify that the generated paths are clickable and valid within the current workspace.
