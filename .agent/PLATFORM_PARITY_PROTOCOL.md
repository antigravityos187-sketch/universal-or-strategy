# Platform Parity Protocol (P3)

This protocol ensures that every AI platform (IDE, CLI, Cloud) has identical access to the "Universal Brain", skills, workflows, and MCP tools.

## 1. Source of Truth (SoT)
The project-level `.agent/` directory is the master Source of Truth.
- **MCP Settings**: `.agent/brain/full_mcp_settings.json`
- **Workflows**: `.agent/workflows/`
- **Skills**: `.agent/skills/`
- **Memory/Context**: `.agent/brain/`

## 2. Synchronization Rules
Whenever a new MCP server is added or a workflow is changed, the following platforms MUST be updated:
- **Antigravity IDE**: `mcp_settings.json` in the user's specific IDE config.
- **Rovo Dev CLI**: `C:\Users\Mohammed Khalid\.rovodev\mcp.json`
- **Claude Desktop**: `C:\Users\Mohammed Khalid\AppData\Roaming\Claude\claude_desktop_config.json`

## 3. Tool Access Parity
All platforms must have access to:
1. **Delegation Bridge**: For cost-optimized execution.
2. **Supermemory**: For long-term architectural persistence.
3. **Workspace Access**: Full read/write access to the repository root.

## 4. Environment Consistency
- Every platform must load the root `.env` file.
- Sensitive keys (API tokens) remain in `.env` and are never hardcoded.

## 5. State Persistence & Checkpoints
To prevent "Memory Drift" between platforms, we implement mandatory checkpointing:
1. **Session Checkpoints**: Rovo Dev auto-persists state in `C:\Users\Mohammed Khalid\.rovodev\sessions`.
2. **Architectural DNA Snapshots**: Major shifts MUST be snapshotted to **Supermemory** via the `remember` tool using `[DNA_CHECKPOINT_V12.X]`.

## 6. Code Checkpoints (The Safety Net)
Actual code changes are protected by a three-tier checkpoint system:
1. **Tier 1: Git Commits**:
   - Every major feature completion MUST be committed to Git.
   - Command: `git add . && git commit -m "[V12] Checkpoint: Feature Name"`
2. **Tier 2: NinjaTrader Backup**:
   - Before any destructive refactor, run the `ninja_backup` workflow (or manual copy).
   - Location: `Documents\NinjaTrader 8\bin\Custom\Strategies\Backup\`
3. **Tier 3: The DNA Anchor**:
   - Updating `.agent/brain/v12_dna.md` acts as the "Logical Checkpoint". Even if code is lost, the AI can rebuild it from the DNA instructions.

## 7. Context Management (Anti-Bloat)
To maintain peak AI performance and reduce token costs:
1. **Mandatory Archiving**: Any version older than the current - 1 (e.g., if we are on V12, V10 and below) MUST be moved to `LEGACY_ARCHIVE/`.
2. **AI Index Blocking**: `LEGACY_ARCHIVE/` must remain in `.claudeignore` and `.cursorignore` to prevent AI agents from wasting context on old logic.

## 8. Strategic Development Cycle
We optimize work based on market access to ensure maximum safety:
1. **The Weekend (Market Closed)**:
    - **Focus**: Heavy architectural refactors, setup, planning, and documentation.
    - **Goal**: Zero-risk structural changes that don't need immediate live data validation.
2. **Market Hours (RTH/GLOBEX)**:
    - **Focus**: Execution logic, live testing, order verification, and debugging.
    - **Goal**: Validation of logic against real-time data feeds and market volatility.
