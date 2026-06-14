#!/bin/bash
# Check status of Wave 1 Phase 0 tmux session without attaching

echo "=== Wave 1 Phase 0 Tmux Status ==="
echo ""

# Check if session exists
if ! tmux has-session -t wave1-p0 2>/dev/null; then
    echo "Session 'wave1-p0' not found"
    echo "Either not started yet or already completed"
    exit 0
fi

echo "Session: wave1-p0 (RUNNING)"
echo ""

# List all panes with their commands
echo "Pane Status:"
tmux list-panes -t wave1-p0 -F '  Pane #{pane_index}: #{pane_current_command} (#{pane_width}x#{pane_height})'
echo ""

# Count completed epics (look for "DONE" in logs)
echo "Completion Status:"
cd /home/malhitticrypto/universal-or-strategy
for epic in 006 007 008 009 010 011 012 013 014 015; do
    if [ -f "logs/phase0/EPIC-${epic}.log" ]; then
        if grep -q "EPIC-${epic} DONE" "logs/phase0/EPIC-${epic}.log" 2>/dev/null; then
            echo "  EPIC-${epic}: COMPLETE"
        else
            # Check if still running
            if grep -q "Phase 0: Hotspot Analysis" "logs/phase0/EPIC-${epic}.log" 2>/dev/null; then
                echo "  EPIC-${epic}: RUNNING"
            else
                echo "  EPIC-${epic}: STARTING"
            fi
        fi
    else
        echo "  EPIC-${epic}: NOT STARTED"
    fi
done
echo ""

# Check file creation
echo "Files Created:"
for epic in 006 007 008 009 010 011 012 013 014 015; do
    hotspot_file="docs/brain/EPIC-${epic}/00-hotspots.md"
    manifest_file="docs/brain/EPIC-${epic}/manifest.json"
    
    if [ -f "$hotspot_file" ] && [ -f "$manifest_file" ]; then
        hotspot_size=$(wc -l < "$hotspot_file")
        echo "  EPIC-${epic}: hotspots.md (${hotspot_size} lines) + manifest.json"
    elif [ -f "$hotspot_file" ]; then
        echo "  EPIC-${epic}: hotspots.md only (manifest missing)"
    elif [ -f "$manifest_file" ]; then
        echo "  EPIC-${epic}: manifest.json only (hotspots missing)"
    else
        echo "  EPIC-${epic}: No files yet"
    fi
done
echo ""

# Extract bobcoin usage if available
echo "Bobcoin Usage:"
if ls logs/phase0/EPIC-*.log 1> /dev/null 2>&1; then
    grep -h "Cost:.*Balance:" logs/phase0/EPIC-*.log 2>/dev/null | head -10 || echo "  No bobcoin reports yet"
else
    echo "  No logs yet"
fi
echo ""

echo "To attach and watch live:"
echo "  tmux attach -t wave1-p0"
echo ""
echo "To kill session:"
echo "  tmux kill-session -t wave1-p0"

# Made with Bob
