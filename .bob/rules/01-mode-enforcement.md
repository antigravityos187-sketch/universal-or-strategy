# Mode Enforcement Protocol (V12.18)

## Code Mode Ban

**CRITICAL**: Code mode is BANNED. All code modification tasks MUST use:
- **Advanced mode** (`advanced`) for general code work
- **Bob CLI** (`v12-engineer`) for src/ architectural work

## Pre-Task Validation

Before accepting any code modification task, verify:
1. Current mode is NOT `code`
2. If in code mode, immediately switch to `advanced`
3. Document switch in session notes

## Violation Handling

If code mode is detected during task execution:
- STOP immediately
- Switch to advanced mode via `switch_mode` tool
- Restart task from checkpoint
- Log violation: "Code mode violation detected at [timestamp] - switched to advanced mode"

## Rationale

Advanced mode provides superior capabilities:
- ✅ MCP tool access (jcodemunch-mcp, graphify)
- ✅ Browser tools for research and documentation
- ✅ Enhanced context management
- ✅ Full feature parity with deprecated code mode

## Enforcement Date

**Effective**: 2026-05-25 (V12.18)
**Mandatory Compliance**: All agents, all sessions