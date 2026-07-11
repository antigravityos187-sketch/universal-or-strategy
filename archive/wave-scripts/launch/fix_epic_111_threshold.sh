#!/bin/bash
# Fix EPIC-111 manifest - update threshold from 15 to 8 and change status

cd ~/universal-or-strategy

MANIFEST="docs/brain/EPIC-CCN-111/manifest.json"

# Backup original
cp "$MANIFEST" "${MANIFEST}.backup"

# Update threshold and status
cat > "$MANIFEST" << 'EOF'
{
  "epic_id": "EPIC-CCN-111",
  "target_method": "HydrateExpectedPositionsFromBroker",
  "file": "src/V12_002.SIMA.Lifecycle.cs",
  "phase": "2-plan",
  "status": "ARCHITECTURE_PLANNING_COMPLETED",
  "claimed_ccn": 17,
  "actual_ccn": {
    "parent": 5,
    "delegate": 7,
    "combined": 12
  },
  "threshold": 8,
  "compliance": "COMBINED_VIOLATES_THRESHOLD",
  "recommendation": "PROCEED_TO_PHASE_3",
  "rationale": "Combined complexity (12) exceeds Jane Street threshold (8). Parent (5) and delegate (7) individually compliant, but combined logic requires refactoring.",
  "documents": {
    "00-hotspots": "Phase 0 hotspot analysis",
    "00-scope": "Phase 1 scope definition (claimed CCN 17, actual 12)",
    "01-scope-boundary": "Phase 1.5 scope boundary validation",
    "02-architecture-plan": "Phase 2 architecture plan (identified scope error, corrected threshold)",
    "02-analysis": "Phase 2 analysis (verified CCN 5, 7 via complexity_audit.py)",
    "03-approach": "Phase 2 approach (REVISED - proceed with correct threshold 8)"
  },
  "phases": {
    "0": {
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    },
    "1": {
      "status": "completed",
      "outputs": ["00-scope.md"]
    },
    "1.5": {
      "status": "completed",
      "outputs": ["01-scope-boundary.md"]
    },
    "2": {
      "status": "completed",
      "outputs": ["02-architecture-plan.md"],
      "notes": "Threshold corrected from 15 to 8. Combined complexity (12) violates threshold. Epic proceeds to Phase 3."
    }
  },
  "director_decision": "PROCEED - Combined complexity (12) > threshold (8)",
  "created": "2026-06-13T07:39:00Z",
  "updated": "2026-06-13T08:06:00Z"
}
EOF

echo "✅ EPIC-111 manifest updated:"
echo "   - Threshold: 8 (Jane Street aligned)"
echo "   - Status: SCOPE_VALIDATION_FAILURE → ARCHITECTURE_PLANNING_COMPLETED"
echo "   - Compliance: BOTH_METHODS_COMPLIANT → COMBINED_VIOLATES_THRESHOLD"
echo "   - Recommendation: ABORT_EPIC → PROCEED_TO_PHASE_3"
echo "   - Rationale: Combined complexity (12) > threshold (8) = REQUIRES refactoring"

# Made with Bob
