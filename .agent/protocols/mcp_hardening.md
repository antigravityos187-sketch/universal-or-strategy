# MCP Hardening Protocol (Rovo Dev + Windows)

## 1. THE 1343-CHARACTER LIMIT
Windows `stdio` pipes, particularly when used with Atlassian's `acli`, can experience truncation if a single JSON-RPC line exceeds ~1343 characters.
- **Root Cause**: FastMCP/MCP-SDK often generates massive schemas for tool definitions.
- **Solution**: Use **Raw JSON-RPC** (manual `json.loads` and `sys.stdout.write`). Hardcode tool descriptions to be ultra-concise (< 50 characters each).

## 2. STRICT JSON-RPC 2.0
Rovo's Pydantic parser will crash if it receives responses to "Notifications" or if the response format varies.
- **Rule**: Only respond to messages that include an `"id"`.
- **Rule**: Notifications (like health pings or progress updates) must be ignored or logged to `stderr`.
- **Rule**: Use standard error codes (e.g., `-32601` for Method Not Found) instead of custom text strings.

## 3. UNIFIED BRIDGE ARCHITECTURE
Multiple MCP servers compete for system resources and increase the chance of pipe collision or "Ghost Server" counts.
- **Pattern**: Consolidate all domain tools (Brain, AI, Deploy) into a single `v12_master_bridge.py`. 
- **Benefit**: Reduces the startup handshake time and ensures a single stable `stdio` connection.

## 4. SEARCH BLINDNESS (DISCOVERY)
Rovo may fail to find methods in files > 5000 lines if using fuzzy globbing (`*Filename.cs`).
- **Fix**: Provide **Target Coordinates** (Line Numbers) in the mission brief to skip the discovery phase and save tokens.
