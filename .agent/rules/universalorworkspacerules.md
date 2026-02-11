# Workspace Rules: Universal OR Strategy (Multi-Platform)

## CORE INSTRUCTIONS
1. **Source of Truth**: The "Universal Brain" (located at `./.agent/brain/`) is the primary source of truth for all project reasoning, tasks, and implementation plans. This brain MUST be synced across all IDEs (Antigravity, Cursor, Codex). Refer to [AGENT.md](../../AGENT.md) for architectural context.
2. **Terminal Commands**: ALWAYS run terminal commands instead of asking the user to perform manual file operations.
3. **Dual-Deployment**: ALWAYS deploy strategy files to both the Project Repository and the NinjaTrader bin folder using the automated deployment script.
4. **Non-Coder Protocol**: ALWAYS provide complete, compilable code ready for Mo to compile. NEVER provide partial snippets or instructions to "insert here"—ALWAYS provide the full block or full file to ensure copy-paste safety.
5. **Multi-Agent Protocol**: ANY agent (Antigravity, Cursor Tab, Codex Chat) MUST update the task list and state files after significant changes.
6. **Code Simplification**: ALL refactoring tasks MUST follow the [.agent/skills/code-simplifier/SKILL.md](../skills/code-simplifier/SKILL.md) guidelines to ensure clarity and maintainability.
7. **Direct Connectivity**: NEVER build or use Excel-based RTD bridges. ALWAYS use the direct `TosRtdClient` for TOS data integration.

## AUTOMATION
- Use `scripts/ninja_deploy.ps1` for all deployment tasks.
- Use `delegation_bridge` to route expensive I/O to Gemini Flash.

## ASSISTANT DELEGATION
- **Cost Optimization**: ALL agents MUST use the `delegation_bridge` MCP for routine tasks (file I/O, documentation, directory listing).
- **Core Models**: `claude_opus_4.5` and `claude_sonnet_4.5` are reserved for complex logic and user interaction.
- **Verification**: Verify usage and savings via `savings_dashboard.html`.
- **Web Artifacts**: Artifacts (dashboards, reports) MUST use "Vanilla JS" to enable instant local viewing. Refer to Lesson 8 in the [Trading Knowledge Vault](../skills/trading-knowledge-vault/SKILL.md).

## MISSION BRIEFS & HANDOFFS
1. **Rule of Universal Paths**: ANY mission brief generated for handoff MUST use relative repository paths (e.g., `./.agent/brain/task.md`) to ensure portability across IDEs and platforms. Absolute OS paths should only be used as a secondary fallback.
2. **Context Continuity**: Always check the `artifact_formatting_guidelines` in your system prompt to find the current conversation-id and brain directory path.
3. **No "Blind" Handoffs**: Never assume another agent can see your current reasoning context. Always bridge the gap with full paths.
4. **Mandatory Surgical Prompt**: When delegating work to a subagent team (Claude Code/Rovo), ALWAYS include this instruction: "Load the context from .agent/brain/master_mission_brief_ui.md and follow the Team Protocol to execute Question 1 of the Surgical Refactor."
