#!/bin/bash
# Regenerate scripts for the 18 failed epics with new API key distribution
# This script creates a temporary epic list and runs the generator

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/../.."

echo "[*] Regenerating scripts for 18 failed epics..."
echo "[*] Failed epics: 008, 018, 038, 053, 068, 069, 083, 090, 098, 099, 108, 113, 121, 128, 141, 143, 153, 158"

# Create temporary epic list file
cat > scripts/wave7/failed_epics_phase0.txt << 'EOF'
008
018
038
053
068
069
083
090
098
099
108
113
121
128
141
143
153
158
EOF

echo "[*] Running generator with --failed-only flag..."
python3 scripts/wave7/generate_phase0_scripts_fixed.py --failed-only

echo ""
echo "[OK] Regenerated 18 Phase 0 scripts with new API distribution"
echo ""
echo "[*] Next steps:"
echo "    1. Deploy to VM: git add . && git commit -m 'fix(wave7): Regenerate 18 failed epics with 20-key distribution' && git push"
echo "    2. Pull on VM: git pull origin main"
echo "    3. Re-launch: ./scripts/wave7/launch_missing_epics.sh"
echo "    4. Monitor: watch -n 120 './scripts/wave7/verify_phase0_completion.sh'"

# Made with Bob
