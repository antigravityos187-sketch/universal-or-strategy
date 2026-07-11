#!/bin/bash
# Generate all Phase 1 scripts from pilot (building-blocks method)
# Source: _p1_pilot_epic_ccn_003_vm.sh

cd /home/malhitticrypto/universal-or-strategy/scripts/wave6

PILOT_SCRIPT="_p1_pilot_epic_ccn_003_vm.sh"
GENERATED=0

echo "Generating Phase 1 scripts from pilot..."
echo "Source: $PILOT_SCRIPT"
echo ""

# Generate for EPIC-CCN-001, 002, 004-023, 025-026, 028-080 (excluding 003, 024, 027)
for NUM in 001 002 $(seq -f "%03g" 4 23) 025 026 $(seq -f "%03g" 28 80); do
    EPIC_ID="EPIC-CCN-${NUM}"
    AGENT_ID="wave6-p1-${NUM}"
    OUTPUT_FILE="_p1_epic_ccn_${NUM}.sh"
    
    # Copy pilot and replace placeholders
    sed "s/EPIC-CCN-003/${EPIC_ID}/g; s/wave6-p1-003/${AGENT_ID}/g" "$PILOT_SCRIPT" > "$OUTPUT_FILE"
    chmod +x "$OUTPUT_FILE"
    
    GENERATED=$((GENERATED + 1))
    if [ $((GENERATED % 10)) -eq 0 ]; then
        echo "  Generated ${GENERATED} scripts..."
    fi
done

echo ""
echo "✅ Generated ${GENERATED} Phase 1 scripts"
echo "📁 Location: /home/malhitticrypto/universal-or-strategy/scripts/wave6/"
echo ""
echo "Verification:"
ls -1 _p1_epic_ccn_*.sh | wc -l
echo "scripts created (should be 78 total including pilot)"

# Made with Bob
