# Watsonx MCP Setup Guide - V12 Worker Agents

**Version**: 1.0  
**Date**: 2026-06-10  
**Status**: Production Ready

## 🎯 What This Enables

**Before** (Manual Orchestration):
- ❌ 5 open VS Code editors (1 orchestrator + 4 workers)
- ❌ Manual epic assignment
- ❌ No real-time coordination
- ❌ Orchestrator polls for status

**After** (MCP-Based Orchestration):
- ✅ **Single Bob session** spawns 4 parallel worker agents via MCP
- ✅ Automatic epic assignment
- ✅ Real-time worker status
- ✅ Event-driven coordination
- ✅ Watsonx Orchestrate integration

## 📦 What We Created

### 1. Worker Agent MCP Server
**File**: `scripts/worker_agent_mcp.py` (424 lines)

**5 MCP Tools per Worker**:
- `claim_epic` - Atomically claim epic with git locking
- `execute_epic` - Run all 6 phases (intake, scope, plan, scan, tickets, validate)
- `release_epic` - Release lock after completion/failure
- `get_worker_status` - Query worker state (idle/busy/health)
- `get_next_pending_epic` - Get next available epic from roadmap

### 2. MCP Configuration
**File**: `.mcp.json`

Configured 4 workers:
- `worker-1` → `C:/WSGTA/universal-or-epic-cluster-1`
- `worker-2` → `C:/WSGTA/universal-or-epic-cluster-2`
- `worker-3` → `C:/WSGTA/universal-or-epic-cluster-3`
- `worker-4` → `C:/WSGTA/universal-or-epic-cluster-4`

### 3. Test Client
**File**: `scripts/test_worker_mcp_client.py` (135 lines)

Tests all 5 MCP tools for each worker.

### 4. Orchestrator Agent
**File**: `v12_orchestrator_agent.yaml` (213 lines)

Watsonx Orchestrate agent configuration with:
- 20 tools (5 per worker × 4 workers)
- Comprehensive instructions
- Error recovery protocols
- Performance metrics

## 🚀 Setup Instructions

### Step 1: Install Python Dependencies

```bash
# Install MCP SDK
pip install mcp>=0.9.0

# Or use requirements file
pip install -r scripts/requirements_worker_mcp.txt
```

### Step 2: Verify MCP Configuration

Check that `.mcp.json` contains all 4 workers:

```bash
# View MCP config
cat .mcp.json

# Should show:
# - jcodemunch-mcp
# - greptile
# - worker-1
# - worker-2
# - worker-3
# - worker-4
```

### Step 3: Test Worker MCP Tools

```bash
# Test single worker
python scripts/test_worker_mcp_client.py worker-1

# Test all workers
python scripts/test_worker_mcp_client.py
```

**Expected Output**:
```
Testing Worker: worker-1
Worktree: C:/WSGTA/universal-or-epic-cluster-1

Test 1: Get Worker Status
{
  "worker_id": "worker-1",
  "worktree_path": "C:/WSGTA/universal-or-epic-cluster-1",
  "assigned_epic": null,
  "health": "healthy"
}

Test 2: Get Next Pending Epic
{
  "success": true,
  "epic_id": "EPIC-CCN-21",
  "method": "ExecuteFFMALimitEntry",
  "file": "src/V12_002.cs",
  "cyclomatic": 9
}

Test 3: Claim Epic (EPIC-CCN-21)
{
  "success": true,
  "epic_id": "EPIC-CCN-21",
  "worker_id": "worker-1",
  "lock_timestamp": "2026-06-10T19:00:00Z"
}

Test 5: Release Epic (EPIC-CCN-21)
{
  "success": true,
  "epic_id": "EPIC-CCN-21",
  "worker_id": "worker-1"
}

✅ All tests passed for worker-1
```

### Step 4: Import Worker Tools to Watsonx Orchestrate

**Option A: Using WxO ToolBox(v2) Extension** (Recommended)

1. Open VS Code Command Palette (`Ctrl+Shift+P`)
2. Search: "WxO ToolBox: Import MCP Tools"
3. Select `.mcp.json`
4. Choose workers to import:
   - ✅ worker-1
   - ✅ worker-2
   - ✅ worker-3
   - ✅ worker-4
5. Click "Import to Watsonx Orchestrate"

**Option B: Using Orchestrate CLI**

```bash
# Import worker-1 tools
orchestrate toolkits add \
  --name "v12_worker_1" \
  --mcp-server "worker-1" \
  --config-file ".mcp.json"

# Import worker-2 tools
orchestrate toolkits add \
  --name "v12_worker_2" \
  --mcp-server "worker-2" \
  --config-file ".mcp.json"

# Import worker-3 tools
orchestrate toolkits add \
  --name "v12_worker_3" \
  --mcp-server "worker-3" \
  --config-file ".mcp.json"

# Import worker-4 tools
orchestrate toolkits add \
  --name "v12_worker_4" \
  --mcp-server "worker-4" \
  --config-file ".mcp.json"

# Verify import
orchestrate toolkits list
```

### Step 5: Import Orchestrator Agent

**Option A: Using WxO Builder Extension** (Recommended)

1. Open VS Code Command Palette (`Ctrl+Shift+P`)
2. Search: "WxO Builder: Import Agent"
3. Select `v12_orchestrator_agent.yaml`
4. Review agent configuration
5. Click "Deploy to Watsonx Orchestrate"

**Option B: Using Orchestrate CLI**

```bash
# Import orchestrator agent
orchestrate agents import v12_orchestrator_agent.yaml

# Verify import
orchestrate agents list

# Should show:
# - v12_epic_orchestrator (gpt-4o)
```

### Step 6: Test Orchestrator in Watsonx Orchestrate

1. Open Watsonx Orchestrate UI
2. Navigate to "Manage Agents"
3. Find "v12_epic_orchestrator"
4. Click "Test Agent"
5. Enter prompt: **"Execute next 4 epics in parallel"**

**Expected Response**:
```
## Epic Orchestration Report

**Workers**: 4 active
**Epics Assigned**: 4
**Status**: In Progress

### Worker Status
- Worker 1: EPIC-CCN-21 (ExecuteFFMALimitEntry) - Claimed ✅
- Worker 2: EPIC-CCN-22 (ProcessSessionReset) - Claimed ✅
- Worker 3: EPIC-CCN-23 (OnBarUpdate) - Claimed ✅
- Worker 4: EPIC-CCN-24 (DrawORBox) - Claimed ✅

### Next Actions
- Executing epics in parallel (estimated 30 minutes)
- Will release locks on completion
- Will assign next 4 epics automatically
```

## 🎮 Usage Examples

### Example 1: Execute 4 Epics in Parallel

**Prompt**: "Execute next 4 epics in parallel"

**What Happens**:
1. Orchestrator checks worker status (all idle)
2. Gets next 4 pending epics from roadmap
3. Claims epics atomically (git-based locking)
4. Executes all 4 epics in parallel
5. Releases locks on completion
6. Reports results

**Time**: ~30 minutes (vs 120 minutes sequential)

### Example 2: Monitor Worker Progress

**Prompt**: "Show worker status"

**Response**:
```
### Worker Status
- Worker 1: EPIC-CCN-21 - Phase 4/6 (epic-scan)
- Worker 2: EPIC-CCN-22 - Phase 5/6 (epic-tickets)
- Worker 3: EPIC-CCN-23 - Phase 3/6 (epic-plan)
- Worker 4: EPIC-CCN-24 - Phase 6/6 (epic-validate)
```

### Example 3: Recover from Failure

**Prompt**: "Worker 2 failed on EPIC-CCN-22, recover"

**What Happens**:
1. Orchestrator releases EPIC-CCN-22 lock
2. Gets next pending epic
3. Assigns to Worker 2
4. Continues execution

### Example 4: Execute Specific Epic

**Prompt**: "Assign EPIC-CCN-25 to Worker 1"

**What Happens**:
1. Orchestrator claims EPIC-CCN-25 for Worker 1
2. Executes epic phases
3. Releases lock on completion

## 🔧 Troubleshooting

### Issue: "MCP server not found"

**Cause**: Bob not restarted after `.mcp.json` update

**Solution**:
1. Close Bob IDE
2. Reopen Bob IDE
3. Check Bob Settings > MCP > Project MCPs
4. Verify worker-1, worker-2, worker-3, worker-4 appear

### Issue: "Epic already assigned"

**Cause**: Another worker claimed the epic first (race condition)

**Solution**:
- Orchestrator automatically gets next pending epic
- No manual intervention needed

### Issue: "Git push failed"

**Cause**: Merge conflict in `epic_roadmap.json`

**Solution**:
```bash
# Pull latest
git pull origin gitbutler/workspace --rebase

# Resolve conflicts
# (Orchestrator will retry automatically)
```

### Issue: "Worker not responding"

**Cause**: Worker process crashed or worktree inaccessible

**Solution**:
1. Check worktree exists: `ls C:/WSGTA/universal-or-epic-cluster-X`
2. Check Python process: `ps aux | grep worker_agent_mcp`
3. Restart Bob IDE
4. Release orphaned locks: `python scripts/validate_epic.py --release EPIC-CCN-X`

## 📊 Performance Metrics

### Theoretical Performance

| Metric | Sequential | Parallel (4 Workers) | Speedup |
|--------|-----------|---------------------|---------|
| **Time per Epic** | 30 min | 30 min | 1x |
| **Epics per Hour** | 2 | 8 | 4x |
| **173 Epics Total** | 86.5 hours | 21.6 hours | 4x |

### Real-World Performance

**Factors Affecting Speedup**:
- Git synchronization overhead (~5%)
- Epic complexity variance (±20%)
- Worker coordination overhead (~10%)

**Expected Real-World Speedup**: **3.5x** (vs 4x theoretical)

## 🎯 Success Criteria

- ✅ All 4 workers executing in parallel
- ✅ No duplicate epic assignments
- ✅ All locks released after completion
- ✅ Average epic time <30 minutes
- ✅ Zero git conflicts
- ✅ 100% success rate (no failed epics)
- ✅ Single Bob session (no 5 open editors)

## 🔄 Migration from Old Workflow

### Old Workflow (Manual)
```bash
# Terminal 1: Orchestrator
cd C:/WSGTA/universal-or-strategy
# Monitor workers manually

# Terminal 2: Worker 1
cd C:/WSGTA/universal-or-epic-cluster-1
epic-validate EPIC-CCN-21

# Terminal 3: Worker 2
cd C:/WSGTA/universal-or-epic-cluster-2
epic-validate EPIC-CCN-22

# Terminal 4: Worker 3
cd C:/WSGTA/universal-or-epic-cluster-3
epic-validate EPIC-CCN-23

# Terminal 5: Worker 4
cd C:/WSGTA/universal-or-epic-cluster-4
epic-validate EPIC-CCN-24
```

### New Workflow (MCP-Based)
```bash
# Single Bob session
# Prompt: "Execute next 4 epics in parallel"
# Done! Orchestrator handles everything.
```

## 📚 References

- **Worker Agent MCP**: `scripts/worker_agent_mcp.py`
- **Test Client**: `scripts/test_worker_mcp_client.py`
- **Orchestrator Agent**: `v12_orchestrator_agent.yaml`
- **MCP Configuration**: `.mcp.json`
- **Epic Locking Protocol**: `docs/workflow/EPIC_LOCKING_PROTOCOL.md`
- **Watsonx Integration**: `docs/workflow/WATSONX_ORCHESTRATE_INTEGRATION.md`

## 🎉 Next Steps

1. ✅ **Test Worker MCP Tools** - Run `python scripts/test_worker_mcp_client.py`
2. ✅ **Import to Watsonx** - Use WxO ToolBox(v2) or CLI
3. ✅ **Deploy Orchestrator** - Use WxO Builder
4. ✅ **Execute Parallel Epics** - Test with 4 workers
5. ✅ **Monitor Progress** - Use Watsonx dashboard

---

**Status**: Ready for production deployment  
**Last Updated**: 2026-06-10  
**Maintainer**: V12 Architecture Team