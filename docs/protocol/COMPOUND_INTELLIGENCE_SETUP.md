# Compound Intelligence 15-Minute Setup Guide

**Date**: 2026-06-04  
**Objective**: Activate full observability stack before EPIC-CCN-14  
**Duration**: 15 minutes

---

## Step 1: Refresh LangSmith API Key (5 minutes)

### 1.1 Get New API Key

**Link**: https://smith.langchain.com/settings

**Steps**:
1. Click the link above (opens LangSmith Settings)
2. Navigate to **API Keys** section
3. Click **Create API Key**
4. Name: `V12-Compound-Intelligence`
5. Copy the key (starts with `ls_...`)

### 1.2 Update .env File

Open `.env` in the repo root and add/update:

```bash
# LangSmith Configuration
LANGSMITH_TRACING=true
LANGSMITH_API_KEY=ls_YOUR_KEY_HERE
LANGSMITH_PROJECT=Sovereign-Multi-Agent
```

**Important**: Replace `ls_YOUR_KEY_HERE` with your actual key from Step 1.1

### 1.3 Test Connection

```powershell
python scripts/langsmith_bridge.py --test
```

**Expected Output**:
```
[*] Running LangSmith Connectivity Test...
[*] Tracing Handoff: Antigravity -> Claude (Mission: MISN-001)
[+] Trace emitted successfully.
```

**If you see "403 Forbidden"**: API key is invalid, repeat Step 1.1

---

## Step 2: Locate Obsidian Vault (2 minutes)

### 2.1 Find Vault Location

**Common Locations**:
- `C:\Users\Mohammed Khalid\Documents\Obsidian\V12-Vault`
- `C:\WSGTA\v12-obsidian-vault`
- `C:\Users\Mohammed Khalid\Obsidian\Universal-OR-Strategy`

**Search Command**:
```powershell
Get-ChildItem -Path "C:\Users\Mohammed Khalid" -Filter ".obsidian" -Directory -Recurse -ErrorAction SilentlyContinue | Select-Object FullName
```

### 2.2 Document Location

Once found, add to `AGENTS.md` under Section 4 (Communication & Context):

```markdown
- **Obsidian Vault**: Located at `<PATH_YOU_FOUND>`
  - 11 plugins installed
  - Human-readable knowledge base
  - Synced with Firebase/Firestore
```

---

## Step 3: Start Phoenix (10 minutes)

### 3.1 Install Phoenix

```powershell
pip install arize-phoenix
```

### 3.2 Create Startup Script

Create `scripts/start_phoenix.py`:

```python
#!/usr/bin/env python3
"""
Start Phoenix real-time trace visualization server.
Access at: http://localhost:6006
"""

import phoenix as px
import sys

def main():
    print("[*] Starting Phoenix trace visualization...")
    print("[*] Dashboard will be available at: http://localhost:6006")
    
    try:
        # Launch Phoenix server
        session = px.launch_app()
        print(f"[+] Phoenix started successfully!")
        print(f"[+] Session URL: {session.url}")
        print("[*] Press Ctrl+C to stop")
        
        # Keep server running
        session.wait()
        
    except KeyboardInterrupt:
        print("\n[*] Shutting down Phoenix...")
        sys.exit(0)
    except Exception as e:
        print(f"[-] Error starting Phoenix: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
```

### 3.3 Start Phoenix

```powershell
python scripts/start_phoenix.py
```

**Expected Output**:
```
[*] Starting Phoenix trace visualization...
[*] Dashboard will be available at: http://localhost:6006
[+] Phoenix started successfully!
[+] Session URL: http://localhost:6006
[*] Press Ctrl+C to stop
```

### 3.4 Verify Dashboard

Open browser: http://localhost:6006

**Expected**: Phoenix dashboard with empty trace list (will populate during epic execution)

### 3.5 Keep Phoenix Running

**Option A**: Leave terminal open (recommended for EPIC-CCN-14)  
**Option B**: Run in background (advanced):
```powershell
Start-Process python -ArgumentList "scripts/start_phoenix.py" -WindowStyle Hidden
```

---

## Step 4: Add Mise Tasks (Optional, 3 minutes)

Add to `.mise.toml`:

```toml
[tasks.phoenix]
description = "Start Phoenix real-time trace visualization"
run = "python scripts/start_phoenix.py"

[tasks.kb-query]
description = "Query Jane Street Knowledge Base"
run = """
if [ -z "$1" ]; then
  echo "Usage: mise run kb-query <search_term>"
  exit 1
fi
python scripts/query_kb.py "$1"
"""

[tasks.langsmith-test]
description = "Test LangSmith connectivity"
run = "python scripts/langsmith_bridge.py --test"

[tasks.compound-status]
description = "Check compound intelligence status"
run = """
echo "=== Compound Intelligence Status ==="
echo ""
echo "1. Firebase/Firestore:"
python scripts/query_kb.py "test" | head -5
echo ""
echo "2. LangSmith:"
python scripts/langsmith_bridge.py --test 2>&1 | grep -E "Trace emitted|403"
echo ""
echo "3. Phoenix:"
curl -s http://localhost:6006 > /dev/null && echo "   ✅ Running at http://localhost:6006" || echo "   ❌ Not running"
"""
```

**Test**:
```powershell
mise run langsmith-test
mise run kb-query "testing"
mise run compound-status
```

---

## Verification Checklist

After completing all steps, verify:

- [ ] LangSmith API key works (`mise run langsmith-test` → no 403 error)
- [ ] Obsidian vault location documented in `AGENTS.md`
- [ ] Phoenix running at http://localhost:6006
- [ ] All 3 systems show ✅ in `mise run compound-status`

---

## Integration with Workflows

### /pr-loop Integration

LangSmith will automatically trace:
- Step 1: Bot forensics extraction
- Step 2: Local repair (Bob CLI)
- Step 3: Global push & monitor

**No code changes needed** - `langsmith_bridge.py` uses `@traceable` decorator

### /epic-run Integration

Phoenix will visualize:
- Phase 1: Forensic intake
- Phase 2: Architecture planning
- Phase 4: Recursive execution
- Phase 5: Verification

**Traces appear in real-time** at http://localhost:6006

---

## Troubleshooting

### LangSmith 403 Forbidden

**Cause**: API key expired or invalid  
**Fix**: Generate new key at https://smith.langchain.com/settings

### Phoenix Won't Start

**Cause**: Port 6006 already in use  
**Fix**: 
```powershell
# Find process using port 6006
netstat -ano | findstr :6006
# Kill process (replace PID)
taskkill /PID <PID> /F
```

### Obsidian Vault Not Found

**Cause**: Vault in non-standard location  
**Fix**: Search entire C: drive:
```powershell
Get-ChildItem -Path "C:\" -Filter ".obsidian" -Directory -Recurse -ErrorAction SilentlyContinue
```

---

## Next Steps

After setup complete:

1. **Verify all systems**: `mise run compound-status`
2. **Return to orchestration**: Continue with EPIC-CCN-14 execution
3. **Monitor Phoenix**: Keep http://localhost:6006 open in browser tab
4. **Check LangSmith**: Visit https://smith.langchain.com/projects after epic completes

---

*Setup guide created: 2026-06-04*  
*Estimated time: 15 minutes*  
*Systems activated: LangSmith, Phoenix, Obsidian documentation*