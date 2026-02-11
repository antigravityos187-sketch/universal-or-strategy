# Concurrency Guard Protocol (CGP)
**Status**: MANDATORY
**Scope**: All AI Agents (Antigravity, Rovo, Claude Code, Sub-agents)

## 1. The "Single Pilot" Rule
Only ONE agent may perform write operations (file edits, deletions, or moves) on a specific file at any given time.

## 2. Terminal Awareness (Active Guard)
Before an agent performs a `write_to_file`, `replace_file_content`, or `run_command` that modifies code:
1. **Check Running Tasks**: The agent MUST check if any long-running terminal commands are active (e.g., Rovo Dev running in a background process).
2. **Determine Collision Risk**: If the running process is an AI agent (like Rovo), the current agent MUST assume it lacks exclusive write access to the workspace.
3. **Yield Responsibility**: The current agent MUST yield all code-editing tasks to the agent running in the terminal until that process completes.

## 3. File-Level Locking
If an agent is performing a multi-step refactor (e.g., splitting a file into partials):
1. **Announcement**: The agent SHOULD mention in its `task_boundary` that it is taking "Exclusive Write Access" to [FileName].
2. **Conflict Detection**: If another agent encounters this task summary in the session history, it must NOT touch that file.

## 4. The "Rovo-Priority" Override
Since Rovo Dev often acts as the **PROJECT DIRECTOR** in long-running missions:
- **Antigravity Rule**: When Rovo is "on the bridge" (running in the terminal), Antigravity acts as a **Support Engineer** only.
- **Action**: Antigravity may read files, run diagnostics, and track APIs, but must **NEVER** edit code unless Rovo is stopped or the User gives a specific bypass command.

## 5. Collision Recovery
If a collision occurs:
1. **Immediate Halt**: Both agents (or the detecting agent) must stop all writes.
2. **Audit & Rollback**: Revert to the last known stable backup (per `development_safety.md`).
3. **User Notification**: Alert the User with "COLLISION DETECTED" and wait for manual intervention.

> [!IMPORTANT]
> This protocol is the "Mutual Exclusion" lock for the project. Ignoring it is a violation of the Symbiosis Pillar.
