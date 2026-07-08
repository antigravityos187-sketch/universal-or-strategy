#!/bin/bash
# Complete EPIC-108 using screen session with proper Bob environment

set -e

EPIC_DIR="/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-108"
LOG_DIR="/home/malhitticrypto/universal-or-strategy/logs"

echo "=== EPIC-108 Screen-Based Completion ==="
echo "Started: $(date)"
echo ""

# Create screen session and run tickets sequentially
screen -dmS epic_108_completion bash -c '
cd /home/malhitticrypto/universal-or-strategy

# Source Bob environment if available
if [ -f ~/.bashrc ]; then
    source ~/.bashrc
fi

echo "=== EPIC-108 Execution in Screen ===" > logs/epic_108_screen.log
echo "Started: $(date)" >> logs/epic_108_screen.log
echo "" >> logs/epic_108_screen.log

# Execute T2-T5 sequentially
for TICKET in 2 3 4 5; do
    echo "" >> logs/epic_108_screen.log
    echo "=== Processing TICKET-$TICKET ===" >> logs/epic_108_screen.log
    
    # Execute ticket
    echo "  Executing..." >> logs/epic_108_screen.log
    bash "_p5_108_t${TICKET}.sh" >> logs/epic_108_screen.log 2>&1
    
    # Wait a bit for execution to complete
    sleep 5
    
    # Validate ticket
    echo "  Validating..." >> logs/epic_108_screen.log
    bash "_p5v_108_t${TICKET}.sh" >> logs/epic_108_screen.log 2>&1
    
    # Check result
    if grep -q "PASS" "docs/brain/EPIC-CCN-108/ticket-${TICKET}-verification.md" 2>/dev/null; then
        echo "  ✅ TICKET-$TICKET passed validation" >> logs/epic_108_screen.log
    else
        echo "  ❌ TICKET-$TICKET failed validation" >> logs/epic_108_screen.log
        echo "  Check: docs/brain/EPIC-CCN-108/ticket-${TICKET}-verification.md" >> logs/epic_108_screen.log
        break
    fi
done

echo "" >> logs/epic_108_screen.log
echo "=== EPIC-108 Completion ===" >> logs/epic_108_screen.log
echo "Completed: $(date)" >> logs/epic_108_screen.log
'

echo "✅ Screen session 'epic_108_completion' started"
echo ""
echo "Monitor progress with:"
echo "  screen -r epic_108_completion"
echo "  tail -f logs/epic_108_screen.log"
echo ""
echo "Check status with:"
echo "  screen -ls"

# Made with Bob
