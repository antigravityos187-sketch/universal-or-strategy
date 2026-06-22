#!/bin/bash
# Wave 7 Phase 0 Recovery Script
# Identifies failed epics, regenerates scripts (fixed version), and re-launches them
# Handles Lamport clock to avoid duplicate events

set -e
cd "$(dirname "$0")/../.."

echo "=========================================="
echo "Wave 7 Phase 0 Recovery"
echo "=========================================="
echo "Time: $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
echo ""

# Step 1: Identify failed epics
echo "[1/5] Identifying failed epics..."
grep -l 'syntax error\|unexpected end of file\|FAILED' logs/phase0/*.log 2>/dev/null | \
    sed 's/.*EPIC-W7-//' | \
    sed 's/.log//' | \
    sort -n > scripts/wave7/failed_epics_phase0.txt

FAILED_COUNT=$(wc -l < scripts/wave7/failed_epics_phase0.txt)
echo "Found $FAILED_COUNT failed epics"

if [ "$FAILED_COUNT" -eq 0 ]; then
    echo "No failed epics found. Exiting."
    exit 0
fi

echo ""
echo "Failed epics:"
head -20 scripts/wave7/failed_epics_phase0.txt
if [ "$FAILED_COUNT" -gt 20 ]; then
    echo "... and $((FAILED_COUNT - 20)) more"
fi

# Step 2: Backup old scripts
echo ""
echo "[2/5] Backing up old scripts..."
mkdir -p scripts/wave7/backup_$(date +%Y%m%d_%H%M%S)
while read epic_num; do
    if [ -f "scripts/wave7/_p0_${epic_num}.sh" ]; then
        cp "scripts/wave7/_p0_${epic_num}.sh" "scripts/wave7/backup_$(date +%Y%m%d_%H%M%S)/"
    fi
done < scripts/wave7/failed_epics_phase0.txt
echo "Backed up $FAILED_COUNT scripts"

# Step 3: Regenerate scripts with fixed generator
echo ""
echo "[3/5] Regenerating scripts (fixed version - no heredocs)..."
python3 scripts/wave7/generate_phase0_scripts_fixed.py --failed-only

# Step 4: Clean up old Lamport events for failed epics
echo ""
echo "[4/5] Cleaning Lamport clock for failed epics..."
if [ -f ".lamport/wave7/event_log.jsonl" ]; then
    # Backup original
    cp .lamport/wave7/event_log.jsonl .lamport/wave7/event_log.jsonl.backup_$(date +%Y%m%d_%H%M%S)
    
    # Remove events for failed epics (they will be re-logged on retry)
    while read epic_num; do
        epic_id="EPIC-W7-${epic_num}"
        # Keep all events EXCEPT phase 0 events for this epic
        grep -v "\"epic_id\":\"$epic_id\".*\"phase\":\"0\"" .lamport/wave7/event_log.jsonl > .lamport/wave7/event_log.jsonl.tmp || true
        mv .lamport/wave7/event_log.jsonl.tmp .lamport/wave7/event_log.jsonl
    done < scripts/wave7/failed_epics_phase0.txt
    
    echo "Cleaned Lamport events for $FAILED_COUNT epics"
else
    echo "No Lamport log found (skipping)"
fi

# Step 5: Re-launch failed epics
echo ""
echo "[5/5] Re-launching failed epics..."
echo "Launching $FAILED_COUNT epics with 12-second stagger..."
echo ""

LAUNCHED=0
while read epic_num; do
    EPIC_ID="EPIC-W7-${epic_num}"
    SCRIPT="scripts/wave7/_p0_${epic_num}.sh"
    
    if [ ! -f "$SCRIPT" ]; then
        echo "[ERROR] Script not found: $SCRIPT"
        continue
    fi
    
    # Launch in screen session
    screen -dmS "p0-${epic_num}" bash -c "cd /home/malhitticrypto/universal-or-strategy && $SCRIPT"
    
    LAUNCHED=$((LAUNCHED + 1))
    echo "[$(date -u '+%H:%M:%S')] Launched $EPIC_ID ($LAUNCHED/$FAILED_COUNT)"
    
    # Stagger launches
    if [ "$LAUNCHED" -lt "$FAILED_COUNT" ]; then
        sleep 12
    fi
done < scripts/wave7/failed_epics_phase0.txt

echo ""
echo "=========================================="
echo "Recovery Launch Complete"
echo "=========================================="
echo "Launched: $LAUNCHED/$FAILED_COUNT epics"
echo "Time: $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
echo ""
echo "Monitor progress:"
echo "  screen -ls | grep 'p0-'"
echo "  ./scripts/wave7/check_wave7_status.sh 0"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase0/EPIC-W7-002.log"
echo ""

# Made with Bob
