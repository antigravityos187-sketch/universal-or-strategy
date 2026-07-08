#!/usr/bin/env python3
"""
Update epic_manifest.py with all 10 custom phase modes (V12.53)
"""

import re

# Read the file
with open('scripts/epic_manifest.py', 'r', encoding='utf-8') as f:
    content = f.read()

# Define the new CUSTOM_MODES set
new_custom_modes = '''# Custom modes (defined in .bob/custom_modes.yaml):
# V12.53: ALL 10 phases now have custom modes with MCP tool enforcement
CUSTOM_MODES = {
    "v12-phase0-hotspot",      # Phase 0: Hotspot Analysis
    "v12-phase1-scope",        # Phase 1: Scope Definition
    "v12-phase1-5-boundary",   # Phase 1.5: Scope Boundary Validation
    "v12-phase2-architecture", # Phase 2: Architecture Planning
    "v12-phase3-audit",        # Phase 3: DNA & PR Audit
    "v12-phase4-tickets",      # Phase 4: Ticket Generation
    "v12-phase4-5-review",     # Phase 4.5: Ticket Review (Jane Street)
    "v12-engineer",            # Phase 5: Ticket Execution
    "v12-phase5-v-verify",     # Phase 5.V: Verification
    "v12-phase6-review",       # Phase 6: Final Review
    "v12-epic-planner",        # Interactive epic planning (not wave execution)
    "v12-phase7-lead",         # Concurrency engineering (not wave execution)
    "autonomous-refactor"      # Wave orchestration (not phase execution)
}'''

# Replace the old CUSTOM_MODES definition
pattern = r'# Custom modes \(defined in \.bob/custom_modes\.yaml\):\nCUSTOM_MODES = \{[^}]+\}'
content = re.sub(pattern, new_custom_modes, content, flags=re.DOTALL)

# Write back
with open('scripts/epic_manifest.py', 'w', encoding='utf-8') as f:
    f.write(content)

print("[OK] Updated epic_manifest.py with all 10 custom phase modes")
print("[OK] CUSTOM_MODES now includes:")
print("   - v12-phase0-hotspot (Phase 0)")
print("   - v12-phase1-scope (Phase 1)")
print("   - v12-phase1-5-boundary (Phase 1.5)")
print("   - v12-phase2-architecture (Phase 2)")
print("   - v12-phase3-audit (Phase 3)")
print("   - v12-phase4-tickets (Phase 4)")
print("   - v12-phase4-5-review (Phase 4.5)")
print("   - v12-engineer (Phase 5)")
print("   - v12-phase5-v-verify (Phase 5.V)")
print("   - v12-phase6-review (Phase 6)")
print("   - v12-epic-planner (interactive)")
print("   - v12-phase7-lead (concurrency)")
print("   - autonomous-refactor (orchestrator)")

# Made with Bob
