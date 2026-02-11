# SOP: V12 Project Orchestration

This protocol defines how we structure work to prevent "agent drift" and maintain high-fidelity trading logic across long development sessions.

## 1. HIERARCHY OF TRUTH
1. **Milestones**: Major versions or feature completions (e.g., "V12.12 Baseline - Stable").
2. **Checkpoints**: System-generated snapshots (numbered) that represent a specific "save point" in the conversation history.
3. **Tasks (task.md)**: The living heart of the project. Every session MUST update `brain/task.md` to mark progress.
4. **TODOs**: Inline code comments for immediate fixes.

## 2. ROLE DEFINITIONS
### Project Director (Master Agent)
- **Scope**: The "Brain". Holds the high-level roadmap and manages the `task.md`.
- **Primary Tool**: `acli rovodev run` with the Master Bridge.
- **Responsibility**: Strategic decisions, architectural audits, and cross-platform sync.

### Sub-Agents (Specialists)
- **Scope**: Single-file or single-mission focus (e.g., "UI Icon Fixer").
- **Primary Tool**: Rovo Subagent profiles or specific conversation threads.
- **Responsibility**: High-volume, repetitive, or isolated technical tasks.

### Agent Teams (Parallel Force)
- **Scope**: Multi-file refactors or complex comparisons.
- **Responsibility**: Executing independent branches of work simultaneously when high bandwidth is needed.

## 3. CHECKPOINT PROTOCOL
- **EOS (End of Session)**: Master Agent must summarize the "DNA" of the session and record it in `session_dna.md`.
- **SOS (Start of Session)**: New Agent must read `session_dna.md` and the `next_session_prompt.md` BEFORE taking any action.

## 4. MILESTONE ARCHIVING
- Before any high-risk refactor (surgical surgery), a full backup of relevant `.cs` files must be moved to `ARCHIVE_SNAPSHOTS/{DATE}_{DESC}/`.
