#!/usr/bin/env python3
"""
Generate Wave 7 Epic Roadmap from Fresh Complexity Audit

Parses complexity_audit_fresh_2026-06-14.txt and creates epic_roadmap_wave7.json
with all 180 methods that have CYC > 8.

Wave 7 = Complexity reduction (CYC > 8 → CYC ≤ 8)
Wave 8 = Jane Street violations (separate wave)

Usage:
    python scripts/generate_wave7_roadmap.py
"""

import json
import re
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Any

def parse_complexity_audit(audit_file: Path) -> List[Dict[str, Any]]:
    """Parse complexity audit file and extract methods with CYC > 8."""
    methods = []
    current_file = None
    
    # Try different encodings (file may have BOM or be UTF-16)
    for encoding in ['utf-8-sig', 'utf-16', 'utf-16-le', 'utf-8']:
        try:
            with open(audit_file, 'r', encoding=encoding) as f:
                lines = f.readlines()
            break
        except UnicodeDecodeError:
            continue
    else:
        raise ValueError(f"Could not decode {audit_file} with any known encoding")
    
    # Parse table format
    # === FILE: filename.cs ===
    # | Method | LOC | Est. CYC | ... | Action |
    # | MethodName | X | Y | ... | REFACTOR |
    
    for line in lines:
        line = line.strip()
        
        # Check for file header
        if line.startswith('=== FILE:') and line.endswith('==='):
            current_file = line.replace('=== FILE:', '').replace('===', '').strip()
            continue
        
        # Skip header lines and separators
        if not line or line.startswith('|---') or line.startswith('| Method'):
            continue
        
        # Parse table row
        if line.startswith('|') and current_file:
            parts = [p.strip() for p in line.split('|')]
            if len(parts) >= 6:  # | Method | LOC | Est. CYC | M5 | Action |
                method_name = parts[1]
                try:
                    cyc = int(parts[3])
                    action = parts[5] if len(parts) > 5 else ''
                    
                    # Only include methods marked for REFACTOR (CYC > 8)
                    if action == 'REFACTOR' and cyc > 8:
                        methods.append({
                            'file': current_file,
                            'method': method_name,
                            'line': 0,  # Line number not in this format
                            'cyc': cyc
                        })
                except (ValueError, IndexError):
                    continue
    
    return methods

def generate_roadmap(methods: List[Dict[str, Any]]) -> Dict[str, Any]:
    """Generate Wave 7 roadmap structure."""
    
    # Sort by complexity (descending) for prioritization
    methods_sorted = sorted(methods, key=lambda x: x['cyc'], reverse=True)
    
    roadmap = {
        "wave_id": "wave7",
        "description": "Wave 7 Fresh Start - All 180 complexity epics (CYC > 8 → CYC ≤ 8)",
        "created": datetime.utcnow().isoformat() + "Z",
        "total_epics": len(methods_sorted),
        "target_complexity": 8,
        "source_file": "complexity_audit_fresh_2026-06-14.txt",
        "execution_model": {
            "vm": "All epics (automated wave execution)",
            "local": "Fallback for .dll dependencies (if discovered)"
        },
        "critical_requirements": {
            "utf8_encoding": "ALL source files MUST be UTF-8 (no BOM)",
            "test_framework": "ALWAYS generate xUnit tests - NEVER NUnit/MSTest"
        },
        "phases": [
            "0 (Hotspot)",
            "1 (Scope)",
            "1.5 (Boundary)",
            "2 (Architecture)",
            "3 (Audit)",
            "4 (Tickets)",
            "5 (Execute)",
            "5.V (Verify)",
            "6 (Review)"
        ],
        "epics": {}
    }
    
    # Generate epic entries
    for idx, method in enumerate(methods_sorted, start=1):
        epic_id = f"EPIC-CCN-{idx:03d}"
        
        roadmap["epics"][epic_id] = {
            "epic_id": epic_id,
            "method": method['method'],
            "file": method['file'],
            "line": method['line'],
            "cyc_before": method['cyc'],
            "cyc_target": 8,
            "priority": "high" if method['cyc'] >= 16 else "medium" if method['cyc'] >= 11 else "low",
            "phases": {
                "0": "pending",
                "1": "pending",
                "1.5": "pending",
                "2": "pending",
                "3": "pending",
                "4": "pending",
                "5": "pending",
                "5.V": "pending",
                "6": "pending"
            },
            "status": "pending",
            "brain_dir": f"docs/brain/{epic_id}",
            "lamport_clock_start": None,
            "lamport_clock_end": None
        }
    
    return roadmap

def main():
    """Main execution."""
    # Paths
    repo_root = Path(__file__).parent.parent
    audit_file = repo_root / "complexity_audit_fresh_2026-06-14.txt"
    output_file = repo_root / "epic_roadmap_wave7.json"
    
    print(f"Parsing complexity audit: {audit_file}")
    
    if not audit_file.exists():
        print(f"ERROR: Audit file not found: {audit_file}")
        return 1
    
    # Parse audit
    methods = parse_complexity_audit(audit_file)
    print(f"Found {len(methods)} methods with CYC > 8")
    
    if len(methods) != 180:
        print(f"WARNING: Expected 180 methods, found {len(methods)}")
        print("Proceeding with found methods...")
    
    # Generate roadmap
    roadmap = generate_roadmap(methods)
    
    # Write roadmap
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(roadmap, f, indent=2, ensure_ascii=False)
    
    print(f"\nRoadmap generated: {output_file}")
    print(f"Total epics: {roadmap['total_epics']}")
    
    # Print summary by priority
    priorities = {"high": 0, "medium": 0, "low": 0}
    for epic in roadmap["epics"].values():
        priorities[epic["priority"]] += 1
    
    print(f"\nPriority breakdown:")
    print(f"  High (CYC >=16): {priorities['high']}")
    print(f"  Medium (CYC 11-15): {priorities['medium']}")
    print(f"  Low (CYC 9-10): {priorities['low']}")
    
    # Print top 10 most complex
    print(f"\nTop 10 most complex methods:")
    sorted_epics = sorted(roadmap["epics"].items(), 
                         key=lambda x: x[1]["cyc_before"], 
                         reverse=True)
    for epic_id, epic in sorted_epics[:10]:
        print(f"  {epic_id}: {epic['method']} (CYC {epic['cyc_before']}) - {epic['file']}")
    
    return 0

if __name__ == "__main__":
    exit(main())

# Made with Bob
