#!/usr/bin/env python3
"""
Validate Wave 6 Epic Structure
Checks actual method counts per epic to verify 2-tier system integrity
"""

import json
import os
from pathlib import Path

def validate_wave6_structure():
    """Validate Wave 6 epic structure and method counts."""
    
    print("=" * 80)
    print("WAVE 6 EPIC STRUCTURE VALIDATION")
    print("=" * 80)
    print()
    
    # Wave 6 scope: EPIC-CCN-001 through 080 (excluding 024, 027)
    wave6_epics = []
    for i in range(1, 81):
        epic_id = f"EPIC-CCN-{i:03d}"
        if epic_id not in ["EPIC-CCN-024", "EPIC-CCN-027"]:
            wave6_epics.append(epic_id)
    
    print(f"Wave 6 Scope: {len(wave6_epics)} epics (001-080, excluding 024, 027)")
    print()
    
    # Check each epic's Phase 0 hotspot file for method count
    single_method_epics = []
    multi_method_epics = []
    missing_phase0 = []
    
    for epic_id in wave6_epics:
        brain_dir = Path(f"docs/brain/{epic_id}")
        hotspot_file = brain_dir / "00-hotspots.md"
        
        if not hotspot_file.exists():
            missing_phase0.append(epic_id)
            continue
        
        # Read hotspot file to determine method count
        content = hotspot_file.read_text(encoding='utf-8')
        
        # Look for method count indicators
        # Single method: "Target Method:" appears once
        # Multi method: Multiple "Method:" entries or "Methods:" plural
        
        method_count = content.count("**Method**:")
        if method_count == 0:
            method_count = content.count("**Target Method**:")
        
        if method_count == 1:
            single_method_epics.append(epic_id)
        elif method_count > 1:
            multi_method_epics.append((epic_id, method_count))
        else:
            # Check for "Methods:" plural indicator
            if "**Methods**:" in content or "multiple methods" in content.lower():
                # Count actual methods mentioned
                lines = content.split('\n')
                method_lines = [l for l in lines if l.strip().startswith('- ') and 'CYC' in l]
                multi_method_epics.append((epic_id, len(method_lines)))
            else:
                single_method_epics.append(epic_id)
    
    # Print results
    print("=" * 80)
    print("RESULTS")
    print("=" * 80)
    print()
    
    print(f"[OK] Single-Method Epics: {len(single_method_epics)}")
    print(f"[OK] Multi-Method Epics: {len(multi_method_epics)}")
    print(f"[!!] Missing Phase 0: {len(missing_phase0)}")
    print()
    
    total_methods = len(single_method_epics) + sum(count for _, count in multi_method_epics)
    print(f"[>>] Total Methods Targeted: {total_methods}")
    print()
    
    if multi_method_epics:
        print("=" * 80)
        print("MULTI-METHOD EPICS BREAKDOWN")
        print("=" * 80)
        print()
        for epic_id, count in sorted(multi_method_epics):
            print(f"  {epic_id}: {count} methods")
        print()
    
    if missing_phase0:
        print("=" * 80)
        print("MISSING PHASE 0")
        print("=" * 80)
        print()
        for epic_id in missing_phase0:
            print(f"  [!!] {epic_id}")
        print()
    
    # Validation summary
    print("=" * 80)
    print("VALIDATION SUMMARY")
    print("=" * 80)
    print()
    
    expected_epics = 78  # 80 - 2 (024, 027)
    actual_epics = len(single_method_epics) + len(multi_method_epics)
    
    if actual_epics == expected_epics:
        print(f"[OK] Epic count matches: {actual_epics}/{expected_epics}")
    else:
        print(f"[!!] Epic count mismatch: {actual_epics}/{expected_epics}")
        print(f"   Missing: {expected_epics - actual_epics} epics")
    
    print()
    print(f"[>>] 2-Tier System:")
    print(f"   Tier 1 (Single Method): {len(single_method_epics)} epics")
    print(f"   Tier 2 (Multi Method): {len(multi_method_epics)} epics")
    print(f"   Total Methods: {total_methods}")
    print()
    
    # Check if this matches original complexity audit
    print("=" * 80)
    print("RECOMMENDATION")
    print("=" * 80)
    print()
    
    if missing_phase0:
        print("[!!] Some epics missing Phase 0 - need to complete before proceeding")
    elif len(multi_method_epics) == 0:
        print("[!!] NO multi-method epics found - this suggests data loss or mixing with other waves")
        print("   Original design: ~80 epics covering 100+ methods via 2-tier system")
        print("   Current state: All epics appear to be single-method")
        print()
        print("   RECOMMENDATION: Review original complexity audit to verify method counts")
    else:
        print("[OK] 2-tier system intact - proceed with remaining phases")
    
    print()

if __name__ == "__main__":
    validate_wave6_structure()

# Made with Bob
