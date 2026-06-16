#!/usr/bin/env python3
import json

manifest_path = "docs/brain/EPIC-CCN-111/manifest.json"

# Backup
import shutil
shutil.copy(manifest_path, manifest_path + ".backup")

# Load
with open(manifest_path, 'r') as f:
    data = json.load(f)

# Update
data['threshold'] = 8
data['status'] = 'ARCHITECTURE_PLANNING_COMPLETED'
data['compliance'] = 'COMBINED_VIOLATES_THRESHOLD'
data['recommendation'] = 'PROCEED_TO_PHASE_3'
data['rationale'] = 'Combined complexity (12) exceeds Jane Street threshold (8). Requires refactoring.'
data['phases'] = {
    '0': {'status': 'completed', 'outputs': ['00-hotspots.md']},
    '1': {'status': 'completed', 'outputs': ['00-scope.md']},
    '1.5': {'status': 'completed', 'outputs': ['01-scope-boundary.md']},
    '2': {
        'status': 'completed',
        'outputs': ['02-architecture-plan.md'],
        'notes': 'Threshold corrected from 15 to 8. Combined complexity (12) violates threshold. Epic proceeds to Phase 3.'
    }
}

# Save
with open(manifest_path, 'w') as f:
    json.dump(data, f, indent=2)

print("✅ EPIC-111 manifest updated:")
print("   - Threshold: 8 (Jane Street aligned)")
print("   - Status: ARCHITECTURE_PLANNING_COMPLETED")
print("   - Compliance: COMBINED_VIOLATES_THRESHOLD")
print("   - Recommendation: PROCEED_TO_PHASE_3")
print("   - Rationale: Combined complexity (12) > threshold (8)")

# Made with Bob
