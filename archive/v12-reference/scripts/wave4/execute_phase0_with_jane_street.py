#!/usr/bin/env python3
"""
Phase 0 Execution Wrapper with Jane Street Integration
Fetches jCodemunch data and invokes Phase 0 MCP server.
"""

import json
import subprocess
import sys
from pathlib import Path

def execute_phase_0(epic_id: str, method: str, file: str, cyc: int):
    """
    Execute Phase 0 with Jane Street integration.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-001)
        method: Method name
        file: File path
        cyc: Cyclomatic complexity
    """
    
    # TODO: Fetch jCodemunch data
    # For now, use placeholder
    jcodemunch_data = {
        "method": method,
        "file": file,
        "complexity": cyc,
        "start_line": 0,
        "end_line": 999999,
        "blast_radius": {
            "confirmed_files": [],
            "potential_files": []
        }
    }
    
    # Load Jane Street violations
    sys.path.insert(0, str(Path(__file__).parent.parent))
    from jane_street_utils import load_violations_for_file, format_violation_report
    
    violations = load_violations_for_file(file)
    violation_report = format_violation_report(violations, f"Jane Street Violations in {method}")
    
    # Create output directory
    output_dir = Path(f"docs/brain/{epic_id}")
    output_dir.mkdir(parents=True, exist_ok=True)
    
    # Create hotspots document
    hotspots_content = f"""# Phase 0: Hotspot Analysis - {epic_id}

## Target Method
- **Method**: `{method}`
- **File**: `{file}`
- **Complexity**: {cyc}
- **Jane Street Violations**: {len(violations)}

## jCodemunch Context
```json
{json.dumps(jcodemunch_data, indent=2)}
```

{violation_report}

## Risk Assessment
- **Complexity Risk**: {"HIGH" if cyc > 20 else "MEDIUM" if cyc > 15 else "LOW"}
- **Jane Street Risk**: {"HIGH" if len(violations) > 5 else "MEDIUM" if len(violations) > 0 else "LOW"}
- **Overall Risk**: {"HIGH" if (cyc > 20 or len(violations) > 5) else "MEDIUM"}

## Recommendations
1. Extract method to reduce complexity from {cyc} to ≤8
2. Fix {len(violations)} Jane Street P0 violations during refactoring
3. Ensure no new violations introduced

## Success Criteria
- ✅ Complexity reduced to ≤8 (Jane Street strict standard)
- ✅ All {len(violations)} Jane Street violations fixed
- ✅ Build passes
- ✅ Tests pass
"""
    
    hotspots_path = output_dir / "00-hotspots.md"
    hotspots_path.write_text(hotspots_content, encoding='utf-8')
    
    # Create manifest
    manifest = {
        "epic_id": epic_id,
        "method": method,
        "file": file,
        "complexity": cyc,
        "jane_street_violations": len(violations),
        "phases": {
            "phase_0": {
                "status": "completed",
                "output": "00-hotspots.md",
                "jane_street_violations_found": len(violations)
            }
        }
    }
    
    manifest_path = output_dir / "manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    
    print(f"[OK] Phase 0 complete for {epic_id}")
    print(f"   - Hotspots: {hotspots_path}")
    print(f"   - Manifest: {manifest_path}")
    print(f"   - Jane Street Violations: {len(violations)}")
    
    return {
        "status": "success",
        "epic_id": epic_id,
        "violations": len(violations),
        "files_created": [str(hotspots_path), str(manifest_path)]
    }

if __name__ == "__main__":
    if len(sys.argv) != 5:
        print("Usage: execute_phase0_with_jane_street.py <epic_id> <method> <file> <cyc>")
        sys.exit(1)
    
    epic_id = sys.argv[1]
    method = sys.argv[2]
    file = sys.argv[3]
    cyc = int(sys.argv[4])
    
    result = execute_phase_0(epic_id, method, file, cyc)
    print(json.dumps(result, indent=2))

# Made with Bob
