# Test Phase 0 FastMCP Server

## Step 1: Check MCP Tools List

In Bob IDE, check if you see `phase-0-hotspot` in your available MCP tools.

## Step 2: Test Call

Try calling the Phase 0 MCP server with this payload:

```json
{
  "epic_id": "EPIC-CCN-26",
  "method": "ExecuteFFMALimitEntry",
  "file": "src/V12_002.cs",
  "cyc": 42,
  "jcodemunch_data": {
    "hotspots": [
      {
        "method": "ExecuteFFMALimitEntry",
        "file": "src/V12_002.cs",
        "complexity": 42,
        "churn": 15
      }
    ],
    "blast_radius": {
      "confirmed": 3,
      "potential": 7
    }
  }
}
```

## Expected Result

Should return immediately (< 1 second) with:

```json
{
  "status": "success",
  "message": "Phase 0 context prepared for EPIC-CCN-26",
  "context": {
    "phase": "Phase 0: Hotspot Analysis",
    "epic_id": "EPIC-CCN-26",
    "method": "ExecuteFFMALimitEntry",
    "file": "src/V12_002.cs",
    "complexity": 42,
    "jcodemunch_context": {...},
    "instructions": "...",
    "output_files": [...]
  }
}
```

## Possible Outcomes

1. ✅ **SUCCESS**: Returns context in <1 second
   - Next: Rewrite remaining 8 phase MCP servers using FastMCP
   
2. ❌ **TIMEOUT**: Still times out after 2 minutes
   - Next: Check Bob IDE logs for error messages
   
3. ❌ **NOT FOUND**: phase-0-hotspot not in MCP tools list
   - Next: Check `.bob/mcp.json` configuration
   
4. ❌ **OTHER ERROR**: Different error message
   - Next: Share error message for debugging

## What to Report

Please tell me:
1. Do you see `phase-0-hotspot` in your MCP tools list?
2. What happens when you call it?
3. How long does it take?
4. What result do you get?