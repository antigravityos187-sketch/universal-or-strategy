# scripts/ - Python Tooling Rules

**Last Updated**: 2026-06-08T22:46:00Z
**Scope**: Python automation scripts and CLI tools

---

## Script Categories

### Epic Planning & Analysis
- `epic_planner.py` - Multi-signal hotspot analysis (complexity + churn + health)
- `complexity_audit.py` - Cyclomatic complexity analysis (CYC ≤ 8 enforcement)
- `dead_code_scan.py` - Unreachable code detection

### Index & Knowledge Management
- `verify_index_freshness.py` - jCodemunch index staleness detection
- `query_kb.py` - Jane Street knowledge base queries (Firebase)
- `capture_lesson.py` - Manual lesson capture to Firebase

### Build & Deployment
- `build_readiness.ps1` - Pre-push validation (13 checks)
- `pre_push_validation.ps1` - Full quality gate (ASCII, build, tests, lint, etc.)
- `deploy-sync.ps1` - Hard link synchronization (83 files)

### Code Quality
- `lint.ps1` - Roslyn analyzer execution
- `test_stress.ps1` - Stress testing
- `ascii_audit.py` - Unicode detection

### Git & PR Management
- `verify_pr_hygiene.ps1` - PR diff size validation (<10k chars)
- `verify_links.ps1` - Markdown link validation

---

## Python Standards

### Shebang
```python
#!/usr/bin/env python3
```

### Imports
```python
# Standard library
import json
import sys
from pathlib import Path
from datetime import datetime, timezone

# Third-party
import firebase_admin
from firebase_admin import firestore

# Local
from utils import log_debug
```

### Docstrings
```python
def verify_index_freshness(max_age_days: int = 7) -> dict:
    """
    Verify jCodemunch index is fresh.
    
    Args:
        max_age_days: Maximum acceptable age in days
        
    Returns:
        {
            "fresh": bool,
            "index_age_days": float,
            "stale_files": list[str],
            "action_required": str
        }
    """
```

### Type Hints
- **Required**: All function signatures
- **Format**: PEP 484 style
- **Union Types**: Use `|` syntax (Python 3.10+)

### Error Handling
```python
try:
    result = risky_operation()
except SpecificError as e:
    print(f"❌ Error: {e}", file=sys.stderr)
    sys.exit(1)
```

---

## Firebase Integration

### Credentials
**File**: `firebase-credentials.json` (gitignored)
**Restore**: Documented in `docs/brain/FIREBASE_CREDENTIALS_REFERENCE.md`

### Collections
- `jane_street_knowledge_base` - 22 repos indexed
- `learnings` - Lessons from epic failures
- `agent_sessions` - Session metadata

### Query Pattern
```python
import firebase_admin
from firebase_admin import credentials, firestore

cred = credentials.Certificate("firebase-credentials.json")
firebase_admin.initialize_app(cred)
db = firestore.client()

# Query
docs = db.collection("learnings").where("category", "==", "workflow").stream()
for doc in docs:
    print(doc.to_dict())
```

---

## CLI Tool Conventions

### Argument Parsing
```python
import argparse

parser = argparse.ArgumentParser(description="Tool description")
parser.add_argument("--flag", action="store_true", help="Flag description")
parser.add_argument("--value", type=int, default=7, help="Value description")
args = parser.parse_args()
```

### Output Formats
- **Human-readable**: Default (colored, formatted)
- **JSON**: `--json` flag for automation
- **Exit codes**: 0 = success, 1 = failure

### Example
```python
if args.json:
    print(json.dumps(result, indent=2))
else:
    if result["fresh"]:
        print(f"✅ INDEX FRESH")
        sys.exit(0)
    else:
        print(f"❌ INDEX STALE")
        sys.exit(1)
```

---

## PowerShell Standards

### Shebang
```powershell
#!/usr/bin/env pwsh
```

### Error Handling
```powershell
$ErrorActionPreference = "Stop"

try {
    # Risky operation
} catch {
    Write-Error "❌ Error: $_"
    exit 1
}
```

### Output
```powershell
Write-Host "✅ Success" -ForegroundColor Green
Write-Host "❌ Failure" -ForegroundColor Red
Write-Host "⚠️  Warning" -ForegroundColor Yellow
```

---

## Script-Specific Rules

### epic_planner.py
**Purpose**: Generate `epic_candidates.json` with multi-signal ranking

**Signals**:
1. Hotspot score (complexity × log(1 + churn))
2. Code health (CodeScene 0-10 rating)
3. Composite score (weighted: 40% hotspot + 30% health + 20% severity + 10% churn)

**Output**: `epic_candidates.json` (168 hotspots, CYC > 8)

**Usage**: Called by `/epic-run` Phase 0

### verify_index_freshness.py
**Purpose**: Prevent EPIC-CCN-1 failure mode (stale index)

**Logic**:
1. Compare graphify-out/graph.json timestamp to git HEAD
2. If delta > 7 days: return `fresh=false`
3. List files modified since last index

**Output**: JSON with `fresh`, `index_age_days`, `stale_files`, `action_required`

**Usage**: Called by `/epic-run` Phase -1

### capture_lesson.py
**Purpose**: Manual lesson capture to Firebase

**Parameters**:
- `--title` - Lesson title
- `--category` - workflow/refactoring/architecture/testing/debugging
- `--description` - Lesson description
- `--root-cause` - Root cause analysis
- `--prevention` - Prevention strategy
- `--confidence` - 0.0-1.0 confidence score

**Usage**: Manual or via `.bob/hooks/after_epic_failure.py`

### query_kb.py
**Purpose**: Query Jane Street knowledge base

**Parameters**:
- `<term>` - Search term (e.g., "complexity reduction", "FSM extraction")

**Output**: Relevant Jane Street patterns and principles

**Usage**: Called during `/epic-plan` Phase 2

---

## Testing

### Unit Tests
**Location**: `tests/` (if applicable)
**Framework**: pytest
**Coverage**: Aim for 80%+

### Integration Tests
**Method**: Run script with test data
**Verification**: Check exit code and output

---

## Common Pitfalls

### ❌ Missing Shebang
**Problem**: Script not executable on Unix
**Solution**: Add `#!/usr/bin/env python3` or `#!/usr/bin/env pwsh`

### ❌ Hardcoded Paths
**Problem**: Script breaks on different machines
**Solution**: Use `Path(__file__).parent` or environment variables

### ❌ No Error Handling
**Problem**: Silent failures
**Solution**: Wrap risky operations in try/except, exit with code 1

---

## Index

**Parent**: [`../AGENTS.md`](../AGENTS.md) (root)
**Children**: None (leaf node)
**Related**:
- [`../src/AGENTS.md`](../src/AGENTS.md) - Source code rules
- [`../docs/AGENTS.md`](../docs/AGENTS.md) - Documentation rules