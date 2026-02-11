---
description: How to coordinate task execution between Antigravity (IDE) and Claude Code (CLI) agents
---

# AI Fleet Coordination Protocol

This workflow defines the "Symmetric Multi-Agent" approach for NinjaTrader strategy development. It allows for simultaneous strategic planning and high-speed execution by using multiple AI sessions synchronized through a shared filesystem.

## Roles

| Agent | Primary Function | Best Used For |
|-------|------------------|---------------|
| **Antigravity (IDE)** | Strategic Lead | Planning, documentation, UI context, multi-file oversight, cross-session memory retrieval. |
| **Claude Code (CLI)** | Tactical Engineer | Bulk code generation, deep grep searches, high-speed file editing, and mass refactoring. |

## Standard Operating Procedure (SOP)

### 1. Handoff Initiation (In IDE)
- The IDE agent completes the **PLANNING** phase.
- **Task**: Create/Update `task.md` and `implementation_plan.md` in `<appDataDir>/brain/<id>/`.
- **Handoff**: Provide the user with a "CLI Handoff Prompt" containing the absolute path to the brain directory.

### 2. Fleet Execution (In CLI)
- User pasture the Handoff Prompt into the CLI Claude session.
- CLI agent must immediately read the brain artifacts (`task.md` and `implementation_plan.md`).
- CLI agent performs the heavy lifting and high-speed code edits.

### 3. State Synchronization (The Shared Blackboard)
- **Mandatory**: Agents MUST update `task.md` after every major milestone.
- **Auditing**: The IDE agent monitors `task.md` and the file system to provide real-time feedback or strategic adjustments.
- **Resolution**: Use the Shared Brain to avoid "hallucinating" outdated file states between conversations.

### 4. Verification & Convergence
- Once the Tactical Engineer (CLI) completes the execution, the Strategic Lead (IDE) performs a final review and compilation check.
- Both agents should finalize the `walkthrough.md` to document the joint achievement.

> [!TIP]
> This "AI Fleet" model is essentially the **SIMA (Single-Instance Multi-Account) Architecture** applied to intelligence. It prevents the "One-Thought-At-A-Time" bottleneck of single LLM sessions.
