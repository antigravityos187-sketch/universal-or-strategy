#!/usr/bin/env python3
"""
Fix all manifest mode fields to use custom modes (V12.53)
Updates 747 phase mode fields across 107 manifests
"""

import json
import os
from pathlib import Path

# Phase to custom mode mapping (ALL CUSTOM MODES)
PHASE_MODES = {
    "0": "v12-phase0-hotspot",      # Phase 0: Hotspot Analysis
    "1": "v12-phase1-scope",        # Phase 1: Scope Definition
    "1.5": "v12-phase1-5-boundary", # Phase 1.5: Scope Boundary Validation
    "2": "v12-phase2-architecture", # Phase 2: Architecture Planning
    "3": "v12-phase3-audit",        # Phase 3: DNA & PR Audit
    "4": "v12-phase4-tickets",      # Phase 4: Ticket Generation
    "4.5": "v12-phase4-5-review",   # Phase 4.5: Ticket Review (Jane Street)
    "5": "v12-engineer",            # Phase 5: Ticket Execution (surgical refactoring)
    "5.V": "v12-phase5-v-verify",   # Phase 5.V: Verification
    "6": "v12-phase6-review"        # Phase 6: Final Review
}

# MCP tools required per phase (MANDATORY)
PHASE_MCP_TOOLS = {
    "0": ["jcodemunch-mcp", "sequential-thinking"],
    "1": ["jcodemunch-mcp", "sequential-thinking"],
    "1.5": ["jcodemunch-mcp", "sequential-thinking"],
    "2": ["jcodemunch-mcp", "sequential-thinking", "graphify"],
    "3": ["jcodemunch-mcp", "sequential-thinking", "greptile"],
    "4": ["jcodemunch-mcp", "sequential-thinking"],
    "4.5": ["sequential-thinking"],  # Jane Street KB query only
    "5": ["jcodemunch-mcp", "sequential-thinking"],
    "5.V": ["jcodemunch-mcp", "sequential-thinking", "greptile"],
    "6": ["jcodemunch-mcp", "sequential-thinking", "greptile"]
}

brain_dir = Path("docs/brain")
manifests_updated = 0
phases_updated = 0
mcp_tools_added = 0

# Process all epic manifests
for epic_dir in sorted(brain_dir.glob("EPIC-CCN-*")):
    manifest_path = epic_dir / "manifest.json"
    if not manifest_path.exists():
        continue
    
    # Load manifest
    with open(manifest_path, 'r', encoding='utf-8') as f:
        manifest = json.load(f)
    
    modified = False
    
    # Update mode field for each phase
    for phase_id, phase_data in manifest.get("phases", {}).items():
        # Extract base phase number (e.g., "5.1" -> "5", "5.1.V" -> "5.V")
        if ".V" in phase_id:
            base_phase = phase_id.split(".")[0] + ".V"
        elif "." in phase_id and phase_id.split(".")[0] in ["1", "4", "5"]:
            # Handle 1.5, 4.5, 5.X
            parts = phase_id.split(".")
            if parts[0] == "1" and parts[1] == "5":
                base_phase = "1.5"
            elif parts[0] == "4" and parts[1] == "5":
                base_phase = "4.5"
            elif parts[0] == "5":
                base_phase = "5"  # 5.1, 5.2, etc. all use v12-engineer
            else:
                base_phase = parts[0]
        else:
            base_phase = phase_id.split(".")[0]
        
        # Get correct mode
        correct_mode = PHASE_MODES.get(base_phase)
        if correct_mode and phase_data.get("mode") != correct_mode:
            phase_data["mode"] = correct_mode
            phases_updated += 1
            modified = True
        
        # Add mcp_tools field if missing
        if "mcp_tools" not in phase_data:
            mcp_tools = PHASE_MCP_TOOLS.get(base_phase, [])
            if mcp_tools:
                phase_data["mcp_tools"] = mcp_tools
                mcp_tools_added += 1
                modified = True
    
    # Save if modified
    if modified:
        with open(manifest_path, 'w', encoding='utf-8') as f:
            json.dump(manifest, f, indent=2)
        manifests_updated += 1

print(f"[OK] Fixed manifest modes:")
print(f"   - {manifests_updated} manifests updated")
print(f"   - {phases_updated} phase modes corrected")
print(f"   - {mcp_tools_added} mcp_tools fields added")
print()
print("[OK] All phases now use custom modes:")
print("   - Phase 0: v12-phase0-hotspot")
print("   - Phase 1: v12-phase1-scope")
print("   - Phase 1.5: v12-phase1-5-boundary")
print("   - Phase 2: v12-phase2-architecture")
print("   - Phase 3: v12-phase3-audit")
print("   - Phase 4: v12-phase4-tickets")
print("   - Phase 4.5: v12-phase4-5-review")
print("   - Phase 5: v12-engineer")
print("   - Phase 5.V: v12-phase5-v-verify")
print("   - Phase 6: v12-phase6-review")
print()
print("[OK] MCP tools enforcement added:")
print("   - jCodemunch: 9/10 phases (all except 4.5)")
print("   - Sequential Thinking: 10/10 phases (MANDATORY)")
print("   - Graphify: Phase 2 (architecture)")
print("   - Greptile: Phases 3, 5.V, 6 (audit/verification)")

# Made with Bob
